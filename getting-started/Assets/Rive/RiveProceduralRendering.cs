using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

//! An example implementation that showcases using the Rive Renderer to draw procedural shapes.

namespace Rive
{
    [ExecuteInEditMode]
    public class RiveProcedural : MonoBehaviour
    {
        public RenderTexture renderTexture;
        private RenderQueue m_renderQueue;
        private CommandBuffer m_commandBuffer;

        private Camera m_camera;

        Path m_path;
        Paint m_paint;
        private void Start()
        {
            m_renderQueue = new RenderQueue(renderTexture);
            m_path = new Path();
            m_paint = new Paint();
            m_paint.color = new Color(0xFFFF0000);
            m_paint.style = PaintingStyle.stroke;
            m_paint.join = StrokeJoin.round;
            m_paint.thickness = 20.0f;
            m_renderQueue.draw(m_path, m_paint);

            m_commandBuffer = new CommandBuffer();
            m_commandBuffer.SetRenderTarget(renderTexture);
            m_renderQueue.addToCommandBuffer(m_commandBuffer);
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
            m_path.reset();

            float expand = Time.fixedTime * 10;
            m_path.moveTo(256, 256 - 100 - expand);
            m_path.lineTo(256 + 50 + expand, 256 + 50 + expand);
            m_path.lineTo(256 - 50 - expand, 256 + 50 + expand);
            m_path.close();
            m_path.flush();


            m_paint.thickness = (Mathf.Sin(Time.fixedTime * Mathf.PI * 2) + 1.0f) * 20.0f + 1.0f;
            m_paint.flush();
        }

        private void OnDisable()
        {
            if (m_camera != null && m_commandBuffer != null)
            {
                m_camera.RemoveCommandBuffer(CameraEvent.AfterEverything, m_commandBuffer);
            }
        }
    }
}