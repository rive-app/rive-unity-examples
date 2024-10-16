using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using Rive;
using RenderQueue = Rive.RenderQueue;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

internal class CameraTextureHelper2 {

    private Camera m_camera;
    private RenderTexture m_renderTexture;
    private int m_pixelWidth;
    private int m_pixelHeight;
    private RenderQueue m_renderQueue;

    public RenderTexture renderTexture {

        get {
            return m_renderTexture;
        }
    }

    public Camera camera {

        get {
            return m_camera;
        }
    }

    internal CameraTextureHelper2(Camera camera, RenderQueue queue) {

        m_camera = camera;
        m_renderQueue = queue;

        update();
    }

    ~CameraTextureHelper2() {

        cleanup();
    }

    void cleanup() {

        if ( m_renderTexture != null ) {

            m_renderTexture.Release();
        }
    }

    public bool update() {

        if ( m_pixelWidth == m_camera.pixelWidth && m_pixelHeight == m_camera.pixelHeight ) {

            return false;
        }

        cleanup();

        m_pixelWidth = m_camera.pixelWidth;
        m_pixelHeight = m_camera.pixelHeight;

        m_renderTexture = new RenderTexture(TextureHelper.Descriptor(m_camera.pixelWidth, m_camera.pixelHeight));
        m_renderTexture.Create();
        m_renderQueue.UpdateTexture(m_renderTexture);

        return true;
    }
}

[RequireComponent(typeof(Camera))]

// Draw a Rive artboard to the screen. Must be bound to a camera.
public class RiveScreenMod: MonoBehaviour {

    [HideInInspector]
    public string stateMachineName;
    public Asset asset;
    public CameraEvent cameraEvent = CameraEvent.BeforeImageEffects;
    public Fit fit = Fit.contain;
    public Alignment alignment = Alignment.Center;

    private RenderQueue m_renderQueue;
    private Rive.Renderer m_riveRenderer;
    private CommandBuffer m_commandBuffer;

    public event RiveEventDelegate OnRiveEvent;
    public delegate void RiveEventDelegate(ReportedEvent reportedEvent);

    private File m_file;
    public Artboard m_artboard;
    public StateMachine m_stateMachine;
    private CameraTextureHelper2 m_helper;
    public Material material;

    [HideInInspector]
    public bool isPanOrZoom = false;

    //How wide/high is the camera in pixels (accounting for dynamic resolution scaling).
    //Normally it is the same as Screen.Width and Screen.Height
    [HideInInspector]
    public float unityPixelWidth;
    [HideInInspector]
    public float unityPixelHeight;

    private Vector2 lastMousePosRive;
    bool m_wasMouseDown = false;
    private Camera cam;

    public bool FlipY() {

        switch ( SystemInfo.graphicsDeviceType ) {

            case GraphicsDeviceType.Metal:
            case GraphicsDeviceType.Direct3D11:
                return true;
            default:
                return false;
        }
    }

    public bool IsDeviceTypeOpenGL() {

        switch ( SystemInfo.graphicsDeviceType ) {

            case GraphicsDeviceType.OpenGLCore:
            case GraphicsDeviceType.OpenGLES3:
                return true;
            default:
                return false;
        }
    }

    public bool IsDeviceTypeMetal() {

        switch ( SystemInfo.graphicsDeviceType ) {

            case GraphicsDeviceType.Metal:
                return true;
            default:
                return false;
        }
    }

    void OnGUI() {

        if ( m_helper != null && Event.current.type.Equals(EventType.Repaint) ) {

            var texture = m_helper.renderTexture;
            unityPixelWidth = m_helper.camera.scaledPixelWidth;
            unityPixelHeight = m_helper.camera.scaledPixelHeight;

            Rect rect;

            if ( FlipY() ) {

                rect = new Rect(0, unityPixelHeight, unityPixelWidth, -unityPixelHeight);

            } else {

                rect = new Rect(0, 0, unityPixelWidth, unityPixelHeight);
            }

            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
        }
    }

    private AudioEngine m_audioEngine;

    void OnAudioFilterRead(float[] data, int channels) {

        if ( m_audioEngine == null ) {

            return;
        }

        m_audioEngine.Sum(data, channels);
    }

    private void Start() {

        if ( asset != null ) {

            m_file = File.Load(asset);

            if ( m_file != null ) {

                m_artboard = m_file.Artboard(0); 
                m_stateMachine = m_artboard?.StateMachine(); 

                int channelCount = 1;

                switch ( AudioSettings.speakerMode ) {

                    case AudioSpeakerMode.Mono:
                        channelCount = 1;
                        break;
                    case AudioSpeakerMode.Stereo:
                        channelCount = 2;
                        break;
                    case AudioSpeakerMode.Quad:
                        channelCount = 4;
                        break;
                    case AudioSpeakerMode.Surround:
                        channelCount = 5;
                        break;
                    case AudioSpeakerMode.Mode5point1:
                        channelCount = 6;
                        break;
                    case AudioSpeakerMode.Mode7point1:
                        channelCount = 8;
                        break;
                    case AudioSpeakerMode.Prologic:
                        channelCount = 2;
                        break;
                }

                m_audioEngine = AudioEngine.Make(channelCount, AudioSettings.outputSampleRate);
                m_artboard.SetAudioEngine(m_audioEngine);
            }
        }

        cam = gameObject.GetComponent<Camera>();
        bool drawToScreen = IsDeviceTypeMetal() ? true : !RenderQueue.supportsDrawingToScreen();
        m_renderQueue = new RenderQueue(null, drawToScreen);
        m_riveRenderer = m_renderQueue.Renderer();
        m_commandBuffer = m_riveRenderer.ToCommandBuffer();
        cam.AddCommandBuffer(cameraEvent, m_commandBuffer);

        if ( drawToScreen ) {

            m_helper = new CameraTextureHelper2(cam, m_renderQueue);
        }

        DrawRive();
    }

    void DrawRive() {

        if ( m_artboard != null ) {

            m_riveRenderer.Align(fit, alignment, m_artboard);
            m_riveRenderer.Draw(m_artboard);
        }
    }

    private void Update() {

        m_helper?.update();

        if ( cam != null ) {


            int activeTouchCount;
            Vector2 mousePosUnity = GetMousePosUnity(out activeTouchCount);
            Vector2 mousePosRive = UnityToRivePos(mousePosUnity);

            bool test = OnDemandRendering.willCurrentFrameRender;

            if ( (m_artboard != null) && (lastMousePosRive != mousePosRive) ) {

                m_stateMachine?.PointerMove(mousePosRive);
            }

            if ( !isPanOrZoom ) {

                if ( activeTouchCount == 1 ) {

                    m_stateMachine?.PointerDown(mousePosRive);
                    m_wasMouseDown = true;
                }

                if ( (activeTouchCount == 0) && m_wasMouseDown ) {

                    m_stateMachine?.PointerUp(lastMousePosRive);
                    m_wasMouseDown = false;
                }
            }

            if ( activeTouchCount == 0 ) {

                isPanOrZoom = false;
                m_wasMouseDown = false;
            }

            lastMousePosRive = mousePosRive;
        }

        // Find reported Rive events before calling advance.
        foreach ( var report in m_stateMachine?.ReportedEvents() ?? Enumerable.Empty<ReportedEvent>() ) {

            OnRiveEvent?.Invoke(report);
        }

        m_stateMachine?.Advance(Time.deltaTime);
    }

    private void OnDisable() {

        Camera camera = gameObject.GetComponent<Camera>();

        if ( m_commandBuffer != null && camera != null ) {

            camera.RemoveCommandBuffer(cameraEvent, m_commandBuffer);
        }
    }

    public Vector2 UnityToRivePos(Vector3 mousePosUnity) {

        Vector2 mouseRiveScreenPos = GetRiveScreenPos(mousePosUnity);

        //mouseArtboardPos is in Rive screen space, not affected by zoom.
        Vector2 mouseRivePos = m_artboard.LocalCoordinate(mouseRiveScreenPos, new Rect(0, 0, unityPixelWidth, unityPixelHeight), fit, alignment);

        return mouseRivePos;
    }

    //Change from Unity coordinate system (origin in bottom left) to Rive coordinate system (origin in top left).
    //The range is (0,0) in the top left to (Screen.Width , Screen.Height), so the artboard size is not used.
    public Vector2 GetRiveScreenPos(Vector3 mousePosUnity) {

        Vector3 mousePosViewPortUnity = cam.ScreenToViewportPoint(mousePosUnity);
        Vector2 mouseRiveScreenPos = new Vector2(mousePosViewPortUnity.x * unityPixelWidth, (1 - mousePosViewPortUnity.y) * unityPixelHeight);

        return mouseRiveScreenPos;
    }

    public Vector2 GetMousePosUnity(out int activeTouchCount) {

        Vector2 mousePosUnity = Vector2.zero;

        //Touch screen available
        if ( Touchscreen.current != null ) {

            activeTouchCount = 0;

            // Iterate through all possible touches
            foreach ( TouchControl touchControl in Touchscreen.current.touches ) {

                // Check if the touch is currently in progress
                if ( touchControl.press.isPressed ) {

                    activeTouchCount++;
                }
            }

            // Access the primary touch (first touch)
            TouchControl primaryTouch = Touchscreen.current.primaryTouch;

            // Check if the touch is currently in progress
            if ( primaryTouch.press.isPressed ) {

                // Update the touch position
                mousePosUnity = primaryTouch.position.ReadValue();
            }
        }

        //No touch screen available, but might have trackpad or mouse.
        else {

            if ( Mouse.current.leftButton.isPressed ) {

                activeTouchCount = 1;

            } else {

                activeTouchCount = 0;
            }

            mousePosUnity = Mouse.current.position.ReadValue();
        }

        return mousePosUnity;
    }
}
