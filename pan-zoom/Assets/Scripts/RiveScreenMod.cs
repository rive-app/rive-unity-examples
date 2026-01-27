using UnityEngine;
using UnityEngine.Rendering;
using System.Linq;
using Rive;
using RenderQueue = Rive.RenderQueue;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.Assertions;


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

    public enum RiveScalingMode {

        /// <summary>
        /// This mode will keep the artboard at a constant pixel size, regardless of screen resolution. This means that the artboard may appear larger or smaller depending on the screen resolution.
        /// </summary>
        ConstantPixelSize = 0,

        /// <summary>
        /// This mode will scale the artboard to maintain the same relative size as the original artboard dimensions across different resolutions. This means that the artboard will always appear the same size relative to the screen.
        /// </summary>
        ReferenceArtboardSize = 1,

        /// <summary>
        /// Maintains consistent physical size (in inches) across different devices by accounting for screen DPI. On higher DPI displays, content will appear larger to maintain consistent physical dimensions.
        /// </summary>
        ConstantPhysicalSize = 2,
    }

    private PanZoom panZoom;

    public Asset asset;
    public CameraEvent cameraEvent = CameraEvent.BeforeImageEffects;
    public Fit fit = Fit.Contain;
    public Alignment alignment = Alignment.Center;
    public float scaleFactor = 1.0f;
    public RiveScalingMode scalingMode = RiveScalingMode.ReferenceArtboardSize;
    [Tooltip("Fallback DPI to use if the screen DPI is not available.")]
    public float fallbackDPI = 96f;
    [SerializeField] private float m_referenceDPI = 150f;
    private Rive.AudioEngine m_audioEngine;

    private RenderQueue m_renderQueue;
    private Rive.Renderer m_riveRenderer;
    private CommandBuffer m_commandBuffer;

    //Used for UI positioning (wide or 4:3 mode).
    [HideInInspector]
    public float aspectRatio = 0;

    private bool speedUp = false;
    private float speedUpFactor = 10f;
    private float speedUpTime = 1f;
    private float speedUpTimeAccumulated = 0f;

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

    private Vector2 lastPrimaryFingerPosRive;
    bool m_wasMouseDown = false;
    private Camera cam;

    private int m_lastCameraWidth;
    private int m_lastCameraHeight;
    private float m_lastScaleFactor = 1.0f;
    private float m_originalArtboardWidth;
    private float m_originalArtboardHeight;

    public void TempSpeedUp() {

        speedUp = true;
    }

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

            Rect rect;

            if ( FlipY() ) {

                rect = new Rect(0, unityPixelHeight, unityPixelWidth, -unityPixelHeight);

            } else {

                rect = new Rect(0, 0, unityPixelWidth, unityPixelHeight);
            }

            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
        }
    }

    private void Start() {

        GameObject obj = GameObject.Find("Main Camera");
        panZoom = obj.GetComponent<PanZoom>();

        InitializeRiveAsset();
        SetupCamera();
        ConfigureRiveRenderer();
    }

    private int GetAudioChannelCount() {
        switch ( AudioSettings.speakerMode ) {
            case AudioSpeakerMode.Mono:
                return 1;
            case AudioSpeakerMode.Stereo:
            case AudioSpeakerMode.Prologic:
                return 2;
            case AudioSpeakerMode.Quad:
                return 4;
            case AudioSpeakerMode.Surround:
                return 5;
            case AudioSpeakerMode.Mode5point1:
                return 6;
            case AudioSpeakerMode.Mode7point1:
                return 8;
            default:
                return 2;
        }
    }

    void OnAudioFilterRead(float[] data, int channels) {

        m_audioEngine?.Sum(data, channels);
    }

    private void InitializeRiveAsset() {

        if ( asset == null )
            return;

        m_file = Rive.File.Load(asset);

        if ( m_file != null ) {

            m_artboard = m_file.Artboard(0);
            m_stateMachine = m_artboard?.StateMachine();
        }

        // Store original artboard dimensions
        if ( m_artboard != null ) {

            m_originalArtboardWidth = m_artboard.Width;
            m_originalArtboardHeight = m_artboard.Height;
        }

        int channelCount = GetAudioChannelCount();
        m_audioEngine = Rive.AudioEngine.Make(channelCount, AudioSettings.outputSampleRate);
        m_artboard?.SetAudioEngine(m_audioEngine);

    }

    private void SetupCamera() {

        cam = gameObject.GetComponent<Camera>();

        Assert.IsNotNull(cam, "TestRive must be attached to a camera.");

        bool drawToScreen = Rive.RenderQueue.supportsDrawingToScreen();
        m_renderQueue = new Rive.RenderQueue(null, !drawToScreen);

        if ( !drawToScreen ) {

            m_helper = new CameraTextureHelper2(cam, m_renderQueue);

            unityPixelWidth = m_helper.camera.scaledPixelWidth;
            unityPixelHeight = m_helper.camera.scaledPixelHeight;

        } else {

            unityPixelWidth = cam.scaledPixelWidth;
            unityPixelHeight = cam.scaledPixelHeight;
        }
    }

    private void ConfigureRiveRenderer() {

        if ( m_commandBuffer != null ) {

            cam.RemoveCommandBuffer(cameraEvent, m_commandBuffer);
            m_commandBuffer.Clear();
        }

        m_riveRenderer = m_renderQueue.Renderer();
        m_commandBuffer = m_riveRenderer.ToCommandBuffer();
        cam.AddCommandBuffer(cameraEvent, m_commandBuffer);

        // Force the visuals to update
        m_stateMachine?.Advance(0f);

        DrawRive();
    }

    void DrawRive() {

        if ( m_artboard != null ) {

            float effectiveScale = GetEffectiveScaleFactor();

            if ( fit == Fit.Layout ) {

                m_artboard.Width = cam.pixelWidth / effectiveScale;
                m_artboard.Height = cam.pixelHeight / effectiveScale;

            } else {

                // Reset to original dimensions if not in Layout mode
                m_artboard.ResetArtboardSize();
            }

            m_stateMachine?.Advance(0f);

            m_riveRenderer.Align(fit, alignment, m_artboard, effectiveScale);
            m_riveRenderer.Draw(m_artboard);
        }
    }

    /// <summary>
    /// Calculates the effective scale factor based on the scaling mode and provided parameters.
    /// </summary>
    /// <param name="scalingMode">The scaling mode to use.</param>
    /// <param name="scaleFactor">The scale factor to apply.</param>
    /// <param name="originalArtboardSize">The original size of the artboard.</param>
    /// <param name="frameRect">The frame rect where the artboard will be displayed.</param>
    /// <param name="referenceDPI">The reference DPI to use for scaling.</param>
    /// <param name="fallbackDPI">The fallback DPI to use if the current screen DPI is not available.</param>
    /// <param name="screenDPI">The screen DPI to use for scaling. If not provided, Screen.dpi will be used.</param>
    public static float CalculateEffectiveScaleFactor(

        RiveScalingMode scalingMode,
        float scaleFactor,
        Vector2 originalArtboardSize,
        Rect frameRect,
        float referenceDPI,
        float fallbackDPI = 96f,
        float screenDPI = -1f
    ) {

        float originalWidth = originalArtboardSize.x;
        float originalHeight = originalArtboardSize.y;

        switch ( scalingMode ) {
            case RiveScalingMode.ConstantPixelSize:
                return scaleFactor;

            case RiveScalingMode.ReferenceArtboardSize: {
                if ( originalWidth <= 0 || originalHeight <= 0 ) {
                    return 1.0f;
                }

                float resolutionScale = frameRect.height / originalHeight;

                return scaleFactor * resolutionScale;
            }

            case RiveScalingMode.ConstantPhysicalSize: {

                float dpi = screenDPI > 0f ? screenDPI : Screen.dpi;

                if ( dpi <= 0f ) {
                    
                    dpi = fallbackDPI;
                }

                float devicePixelRatio = dpi / referenceDPI;

                return scaleFactor * devicePixelRatio;
            }

            default:
                return 1.0f;
        }
    }

    private float GetEffectiveScaleFactor() {

        return CalculateEffectiveScaleFactor(

            scalingMode,
            scaleFactor,
            new Vector2(m_originalArtboardWidth, m_originalArtboardHeight),
            new Rect(0, 0, cam.pixelWidth, cam.pixelHeight),
            m_referenceDPI,
            fallbackDPI
        );
    }


    private void FixedUpdate() {

        if ( cam != null ) {

            //Used for UI positioning (wide or 4:3 mode).
            aspectRatio = (float) cam.pixelHeight / cam.pixelWidth;

            Vector2 primaryFingerPosUnity;
            Vector2 secondaryFingerPosUnity;
            int activeTouchCount;
            Vector2 primaryFingerPosRive;
            Vector2 secondaryFingerPosRive = Vector2.zero;

            GetTouchPosUnity(out activeTouchCount, out primaryFingerPosUnity, out secondaryFingerPosUnity);
            primaryFingerPosRive = UnityToRivePos(primaryFingerPosUnity);

            if ( activeTouchCount > 1 ) {

                secondaryFingerPosRive = UnityToRivePos(secondaryFingerPosUnity);
            }

            panZoom.PanZoomLogic(activeTouchCount, primaryFingerPosUnity, secondaryFingerPosUnity, primaryFingerPosRive, secondaryFingerPosRive);


            if ( (m_artboard != null) && (lastPrimaryFingerPosRive != primaryFingerPosRive) ) {

                m_stateMachine?.PointerMove(primaryFingerPosRive);
            }

            if ( !isPanOrZoom ) {

                if ( activeTouchCount == 1 ) {

                    m_stateMachine?.PointerDown(primaryFingerPosRive);
                    m_wasMouseDown = true;
                }

                if ( (activeTouchCount == 0) && m_wasMouseDown ) {

                    m_wasMouseDown = false;
                    m_stateMachine?.PointerUp(lastPrimaryFingerPosRive);
                }
            }

            if ( activeTouchCount == 0 ) {

                isPanOrZoom = false;
                m_wasMouseDown = false;
            }

            m_helper?.update();

            CheckForDimensionChanges();

            lastPrimaryFingerPosRive = primaryFingerPosRive;
        }

        if ( m_stateMachine != null ) {

            foreach ( var reportedEvent in m_stateMachine.ReportedEvents() ) {

                OnRiveEvent?.Invoke(reportedEvent);
            }
        }

        if ( speedUp ) {

            m_stateMachine?.Advance(Time.deltaTime * speedUpFactor);

            speedUpTimeAccumulated += Time.deltaTime;

            if ( speedUpTimeAccumulated >= speedUpTime ) {

                speedUp = false;
            }

        } else {

            m_stateMachine?.Advance(Time.deltaTime);
        }
    }

    private void CheckForDimensionChanges() {

        if ( m_lastCameraWidth != cam.pixelWidth ||
            m_lastCameraHeight != cam.pixelHeight ||
            m_lastScaleFactor != scaleFactor ) {
            m_lastCameraWidth = cam.pixelWidth;
            m_lastCameraHeight = cam.pixelHeight;
            m_lastScaleFactor = scaleFactor;

            if ( fit == Fit.Layout ) {

                ConfigureRiveRenderer();
            }
        }
    }

    private void OnDisable() {

        Camera camera = gameObject.GetComponent<Camera>();

        if ( m_commandBuffer != null && camera != null ) {

            camera.RemoveCommandBuffer(cameraEvent, m_commandBuffer);
        }
    }

    private void OnDestroy() {

        m_file?.Dispose();
    }


    public Vector2 UnityToRivePos(Vector3 mousePosUnity) {

        Vector2 mouseRiveScreenPos = GetRiveScreenPos(mousePosUnity);
        float effectiveScale = GetEffectiveScaleFactor();

        Vector2 mouseRivePos = m_artboard.LocalCoordinate(mouseRiveScreenPos, new Rect(0, 0, cam.pixelWidth / effectiveScale, cam.pixelHeight / effectiveScale), fit, alignment);
        return mouseRivePos;
    }

    //Change from Unity coordinate system (origin in bottom left) to Rive coordinate system (origin in top left).
    //The range is (0,0) in the top left to (Screen.Width , Screen.Height), so the artboard size is not used.
    public Vector2 GetRiveScreenPos(Vector3 mousePosUnity) {

        Vector3 mousePosViewPortUnity = cam.ScreenToViewportPoint(mousePosUnity);

        float effectiveScale = GetEffectiveScaleFactor();

        //Vector2 mouseRiveScreenPos = new Vector2(mousePosViewPortUnity.x * unityPixelWidth, (1 - mousePosViewPortUnity.y) * unityPixelHeight);
        Vector2 mouseRiveScreenPos = new Vector2(mousePosViewPortUnity.x * unityPixelWidth / effectiveScale, (1 - mousePosViewPortUnity.y) * unityPixelHeight / effectiveScale);

        return mouseRiveScreenPos;
    }

    public void GetTouchPosUnity(out int activeTouchCount, out Vector2 touchPos1, out Vector2 touchPos2) {

        touchPos1 = Vector2.zero;
        touchPos2 = Vector2.zero;
        activeTouchCount = 0;

        //Track pad or mouse used
        if ( Mouse.current != null ) {

            if ( Mouse.current.leftButton.isPressed ) {

                activeTouchCount = 1;

            } else {

                activeTouchCount = 0;
            }

            touchPos1 = Mouse.current.position.ReadValue();
        }

        //Touch screen available
        if ( Touchscreen.current != null ) {

            // Iterate through all possible touches
            foreach ( TouchControl touchControl in Touchscreen.current.touches.Where(t => t.press.isPressed) ) {

                activeTouchCount++;

                if ( activeTouchCount == 1 ) {

                    touchPos1 = touchControl.position.ReadValue();
                }

                if ( activeTouchCount == 2 ) {

                    touchPos2 = touchControl.position.ReadValue();

                    break;
                }
            }
        }

        //Pen available
        if ( (Pen.current != null) && (activeTouchCount == 0) ) {

            if ( Pen.current.tip.isPressed ) {

                activeTouchCount = 1;

                // Update the Pen position
                touchPos1 = Pen.current.position.ReadValue();

            } else {

                activeTouchCount = 0;
            }
        }
    }
}

