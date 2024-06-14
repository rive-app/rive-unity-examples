
using System;
using System.Collections.Concurrent;
using System.Linq;
using Rive;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

internal class CameraTextureHelper
{
    private Camera m_camera;
    private RenderTexture m_renderTexture;
    private int m_pixelWidth = -1;
    private int m_pixelHeight = -1;
    private Rive.RenderQueue m_renderQueue;

    // Queue to keep things on the main thread only.
    private static ConcurrentQueue<Action> mainThreadActions = new ConcurrentQueue<Action>();

    public RenderTexture renderTexture
    {
        get { return m_renderTexture; }
    }

    public Camera camera
    {
        get { return m_camera; }
    }

    internal CameraTextureHelper(Camera camera, Rive.RenderQueue queue)
    {
        m_camera = camera;
        m_renderQueue = queue;
        UpdateTextureHelper();
    }

    ~CameraTextureHelper()
    {
        // Since the GC calls the destructor and doesn't run on the main thread,
        // we need to ensure the cleanup() call happens on the main thread.
        mainThreadActions.Enqueue(() => Cleanup());
    }

    void Cleanup()
    {
        if (m_renderTexture != null)
        {
            m_renderTexture.Release();
        }
    }

    private void Update()
    {
        // Process main thread actions
        while (mainThreadActions.TryDequeue(out var action))
        {
            action();
        }
    }

    public bool UpdateTextureHelper()
    {

        if (m_pixelWidth == m_camera.pixelWidth && m_pixelHeight == m_camera.pixelHeight)
        {
            return false;
        }
        
        Cleanup();

        m_pixelWidth = m_camera.pixelWidth;
        m_pixelHeight = m_camera.pixelHeight;
        var textureDescriptor = TextureHelper.Descriptor(m_pixelWidth, m_pixelHeight);
        m_renderTexture = new RenderTexture(textureDescriptor);
        m_renderTexture.Create();
        m_renderQueue.UpdateTexture(m_renderTexture);

        return true;
    }

}

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
// Draw a Rive artboard to the screen. Must be bound to a camera.
public class RiveScreen : MonoBehaviour
{
    public Rive.Asset asset;
    public CameraEvent cameraEvent = CameraEvent.AfterEverything;
    public Fit fit = Fit.contain;
    public Alignment alignment = Alignment.Center;
    public event RiveEventDelegate OnRiveEvent;
    public delegate void RiveEventDelegate(ReportedEvent reportedEvent);

    private Rive.RenderQueue m_renderQueue;
    private Rive.Renderer m_riveRenderer;
    private CommandBuffer m_commandBuffer;

    private Rive.File m_file;
    private Artboard m_artboard;
    private StateMachine m_stateMachine;
    private CameraTextureHelper m_helper;

    public StateMachine stateMachine => m_stateMachine;

    private static bool flipY()
    {
        switch (UnityEngine.SystemInfo.graphicsDeviceType)
        {
            case UnityEngine.Rendering.GraphicsDeviceType.Metal:
            case UnityEngine.Rendering.GraphicsDeviceType.Direct3D11:
                return true;
            default:
                return false;
        }
    }

    void OnGUI()
    {
        if (m_helper != null && Event.current.type.Equals(EventType.Repaint))
        {
            var texture = m_helper.renderTexture;

            var width = m_helper.camera.scaledPixelWidth;
            var height = m_helper.camera.scaledPixelHeight;

            GUI.DrawTexture(
                flipY() ? new Rect(0, height, width, -height) : new Rect(0, 0, width, height),
                texture,
                ScaleMode.StretchToFill,
                true
            );

            Navigation.DrawInstructions();
        }

    }

    private void Awake()
    {
        if (asset != null)
        {
            m_file = Rive.File.Load(asset);
            m_artboard = m_file.Artboard(0);
            m_stateMachine = m_artboard?.StateMachine();
        }

        Camera camera = gameObject.GetComponent<Camera>();
        Assert.IsNotNull(camera, "RiveScreen must be attached to a camera.");

        // Make a RenderQueue that doesn't have a backing texture and does not
        // clear the target (we'll be drawing on top of it).
        m_renderQueue = new Rive.RenderQueue(null, false);
        m_riveRenderer = m_renderQueue.Renderer();
        m_commandBuffer = m_riveRenderer.ToCommandBuffer();

        if (!Rive.RenderQueue.supportsDrawingToScreen())
        {
            m_helper = new CameraTextureHelper(camera, m_renderQueue);
            m_commandBuffer.SetRenderTarget(m_helper.renderTexture);
        }
        camera.AddCommandBuffer(cameraEvent, m_commandBuffer);

        DrawRive(m_renderQueue);
    }

    void DrawRive(Rive.RenderQueue queue)
    {
        if (m_artboard == null)
        {
            return;
        }
        m_riveRenderer.Align(fit, alignment ?? Alignment.Center, m_artboard);
        m_riveRenderer.Draw(m_artboard);
        
    }

    private Vector2 m_lastMousePosition;
    bool m_wasMouseDown = false;

    private void Update()
    {
        m_helper?.UpdateTextureHelper();
        if (m_artboard == null)
        {
            return;
        }

        Camera camera = gameObject.GetComponent<Camera>();
        if (camera != null)
        {
            Vector3 mousePos = camera.ScreenToViewportPoint(Input.mousePosition);
            Vector2 mouseRiveScreenPos = new Vector2(
                mousePos.x * camera.pixelWidth,
                (1 - mousePos.y) * camera.pixelHeight
            );
            if (m_lastMousePosition != mouseRiveScreenPos)
            {
                Vector2 local = m_artboard.LocalCoordinate(
                    mouseRiveScreenPos,
                    new Rect(0, 0, camera.pixelWidth, camera.pixelHeight),
                    fit,
                    alignment
                );
                m_stateMachine?.PointerMove(local);
                m_lastMousePosition = mouseRiveScreenPos;
            }
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 local = m_artboard.LocalCoordinate(
                    mouseRiveScreenPos,
                    new Rect(0, 0, camera.pixelWidth, camera.pixelHeight),
                    fit,
                    alignment
                );
                m_stateMachine?.PointerDown(local);
                m_wasMouseDown = true;
            }
            else if (m_wasMouseDown)
            {
                m_wasMouseDown = false;
                Vector2 local = m_artboard.LocalCoordinate(
                    mouseRiveScreenPos,
                    new Rect(0, 0, camera.pixelWidth, camera.pixelHeight),
                    fit,
                    alignment
                );
                m_stateMachine?.PointerUp(local);
            }
        }

        // Find reported Rive events before calling advance.
        foreach (var report in m_stateMachine?.ReportedEvents() ?? Enumerable.Empty<ReportedEvent>())
        {
            OnRiveEvent?.Invoke(report);
        }

        m_stateMachine?.Advance(Time.deltaTime);

    }

    private void OnDisable()
    {
        Camera camera = gameObject.GetComponent<Camera>();
        if (m_commandBuffer != null && camera != null)
        {
            camera.RemoveCommandBuffer(cameraEvent, m_commandBuffer);
        }

    }
}