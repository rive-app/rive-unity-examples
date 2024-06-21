using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;
using Rive;
using UnityEngine.UIElements;

public class ShipFui : MonoBehaviour
{
    public Rive.Asset asset;
    private RenderTexture m_renderTexture;
    public Fit fit = Fit.contain;
    public Alignment alignment = Alignment.Center;

    private Rive.RenderQueue m_renderQueue;
    private Rive.Renderer m_riveRenderer;
    private CommandBuffer m_commandBuffer;

    private Rive.File m_file;
    private Artboard m_artboard;
    private StateMachine m_stateMachine;

    private Camera m_camera;

    private static bool FlipY()
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

    private void Start()
    {
        m_renderTexture = new RenderTexture(TextureHelper.Descriptor(5780, 356))
        {
            enableRandomWrite = (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Direct3D11)
        };

        m_renderTexture.Create();

        // Apply RenderTexture to the children; the FUI and the reflection.
        UnityEngine.Renderer[] renderers = GetComponentsInChildren<UnityEngine.Renderer>();

        foreach (UnityEngine.Renderer renderer in renderers)
        {
            Material mat = renderer.material;
            mat.SetTexture("_MainTex", m_renderTexture);

            if (!FlipY())
            {
                // Flip the render texture vertically for OpenGL
                mat.mainTextureScale = new Vector2(1, -1);
                mat.mainTextureOffset = new Vector2(0, 1);
            }

        }

        m_renderQueue = new Rive.RenderQueue(m_renderTexture);
        m_riveRenderer = m_renderQueue.Renderer();
        if (asset != null)
        {
            m_file = Rive.File.Load(asset);
            m_artboard = m_file.Artboard(0);
            m_stateMachine = m_artboard?.StateMachine();
        }

        if (m_artboard != null && m_renderTexture != null)
        {
            m_riveRenderer.Align(fit, alignment, m_artboard);
            m_riveRenderer.Draw(m_artboard);

            m_commandBuffer = new CommandBuffer();
            m_riveRenderer.ToCommandBuffer();
            m_commandBuffer.SetRenderTarget(m_renderTexture);
            m_commandBuffer.ClearRenderTarget(true, true, UnityEngine.Color.clear, 0.0f);
            m_riveRenderer.AddToCommandBuffer(m_commandBuffer);
            m_camera = Camera.main;
            if (m_camera != null)
            {
                Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, m_commandBuffer);
            }
        }
    }

    private Vector2 m_lastMousePosition;
    bool m_wasMouseDown = false;

    private void Update()
    {
        // Camera camera = gameObject.GetComponent<Camera>();
        // if (camera != null)
        // {
        //     Vector3 mousePos = camera.ScreenToViewportPoint(Input.mousePosition);
        //     Vector2 mouseRiveScreenPos = new Vector2(mousePos.x * camera.pixelWidth, (1 - mousePos.y) * camera.pixelHeight);
        //     if (m_artboard != null && m_lastMousePosition != mouseRiveScreenPos)
        //     {
        //         Vector2 local = m_artboard.LocalCoordinate(mouseRiveScreenPos, new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), fit, alignment);
        //         m_stateMachine?.PointerMove(local);
        //         m_lastMousePosition = mouseRiveScreenPos;
        //     }
        //     if (Input.GetMouseButtonDown(0))
        //     {
        //         Vector2 local = m_artboard.LocalCoordinate(mouseRiveScreenPos, new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), fit, alignment);
        //         m_stateMachine?.PointerDown(local);
        //         m_wasMouseDown = true;
        //     }
        //     else if (m_wasMouseDown)
        //     {
        //         m_wasMouseDown = false; Vector2 local = m_artboard.LocalCoordinate(mouseRiveScreenPos, new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), fit, alignment);
        //         m_stateMachine?.PointerUp(local);
        //     }
        // }
        if (m_stateMachine != null)
        {
            m_stateMachine.Advance(Time.deltaTime);
            // Graphics.ExecuteCommandBuffer(m_commandBuffer);
        }

        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private void OnDisable()
    {
        if (m_camera != null && m_commandBuffer != null)
        {
            m_camera.RemoveCommandBuffer(CameraEvent.AfterEverything, m_commandBuffer);
        }
    }
    void OnDestroy()
    {
        // Release the RenderTexture when it's no longer needed
        if (m_renderTexture != null)
        {
            m_renderTexture.Release();
        }
    }

}
