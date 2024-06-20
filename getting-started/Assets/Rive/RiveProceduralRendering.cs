using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Rive;

//! An example implementation that showcases using the Rive Renderer to draw procedural shapes.

//[ExecuteInEditMode]
public class RiveProcedural : MonoBehaviour
{
    private RenderTexture m_renderTexture;
    private Rive.RenderQueue m_renderQueue;
    private Rive.Renderer m_riveRenderer;
    private CommandBuffer m_commandBuffer;

    private Camera m_camera;

    Path m_path;
    Paint m_paint;
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
        m_renderTexture = new RenderTexture(TextureHelper.Descriptor(256, 256));
        m_renderTexture.Create();

        UnityEngine.Renderer cubeRenderer = GetComponent<UnityEngine.Renderer>();
        Material mat = cubeRenderer.material;
        mat.mainTexture = m_renderTexture;

        if (!FlipY())
        {
            // Flip the render texture vertically for OpenGL
            mat.mainTextureScale = new Vector2(1, -1);
            mat.mainTextureOffset = new Vector2(0, 1);
        }

        m_renderQueue = new Rive.RenderQueue(m_renderTexture);
        m_riveRenderer = m_renderQueue.Renderer();

        m_path = new Path();
        m_paint = new Paint();
        m_paint.Color = new Rive.Color(0xFFFFFFFF);
        m_paint.Style = PaintingStyle.stroke;
        m_paint.Join = StrokeJoin.round;
        m_paint.Thickness = 20.0f;

        m_riveRenderer = m_renderQueue.Renderer();
        m_riveRenderer.Draw(m_path, m_paint);

        m_commandBuffer = m_riveRenderer.ToCommandBuffer();
        m_commandBuffer.SetRenderTarget(m_renderTexture);
        m_riveRenderer.AddToCommandBuffer(m_commandBuffer, true);
        m_camera = Camera.main;
        if (m_camera != null)
        {
            Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, m_commandBuffer);
        }
    }

    const int triangleStart = 125;
    const int triangleSize = 50;
    const float growSize = 20.0f;

    private void Update()
    {
        if (m_path == null)
        {
            return;
        }
        m_path.Reset();

        float expand = (Mathf.Sin(Time.fixedTime * Mathf.PI * 2) + 1.0f) * 20.0f + 1.0f;
        m_path.MoveTo(triangleStart, triangleStart - triangleSize - expand);
        m_path.LineTo(triangleStart + triangleSize + expand, triangleStart + triangleSize + expand);
        m_path.LineTo(triangleStart - triangleSize - expand, triangleStart + triangleSize + expand);
        m_path.Close();

        m_paint.Thickness = (Mathf.Sin(Time.fixedTime * Mathf.PI * 2) + 1.0f) * 20.0f + 1.0f;

        m_riveRenderer = m_renderQueue.Renderer();
        m_riveRenderer.Draw(m_path, m_paint);

        m_commandBuffer.Clear();
        m_commandBuffer.SetRenderTarget(m_renderTexture);
        m_riveRenderer.AddToCommandBuffer(m_commandBuffer, true);
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
