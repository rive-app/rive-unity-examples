using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Rive;

//! An example implementation that showcases using the Rive Renderer to draw procedural shapes.

[ExecuteInEditMode]
public class RiveProcedural : MonoBehaviour
{
    public RenderTexture renderTexture;
    private Rive.RenderQueue m_renderQueue;
    private Rive.Renderer m_riveRenderer;
    private CommandBuffer m_commandBuffer;

    private Camera m_camera;

    Path m_path;
    Paint m_paint;
    private void Start()
    {
        m_renderQueue = new Rive.RenderQueue();
        m_riveRenderer = m_renderQueue.Renderer();
        m_path = new Path();
        m_paint = new Paint();
        m_paint.Color = new Rive.Color(0xFFFF0000);
        m_paint.Style = PaintingStyle.stroke;
        m_paint.Join = StrokeJoin.round;
        m_paint.Thickness = 20.0f;
        m_riveRenderer.Draw(m_path, m_paint);

        m_commandBuffer = m_riveRenderer.ToCommandBuffer();
        m_commandBuffer.SetRenderTarget(renderTexture);
        m_riveRenderer.AddToCommandBuffer(m_commandBuffer);
        m_camera = Camera.main;
        if (m_camera != null)
        {
            Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, m_commandBuffer);
        }
    }

    private void Update()
    {
        if (m_path == null)
        {
            return;
        }
        m_path.Reset();

        float expand = Time.fixedTime * 10;
        m_path.MoveTo(256, 256 - 100 - expand);
        m_path.LineTo(256 + 50 + expand, 256 + 50 + expand);
        m_path.LineTo(256 - 50 - expand, 256 + 50 + expand);
        m_path.Close();
        m_path.Flush();


        m_paint.Thickness = (Mathf.Sin(Time.fixedTime * Mathf.PI * 2) + 1.0f) * 20.0f + 1.0f;
        m_paint.Flush();
    }

    private void OnDisable()
    {
        if (m_camera != null && m_commandBuffer != null)
        {
            m_camera.RemoveCommandBuffer(CameraEvent.AfterEverything, m_commandBuffer);
        }
    }
}