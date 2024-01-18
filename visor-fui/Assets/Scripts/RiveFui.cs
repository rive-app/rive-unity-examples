using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

namespace Rive
{
    public class RiveFui : MonoBehaviour
    {
        public Rive.Asset asset;
        public RenderTexture renderTexture;
        public Fit fit = Fit.contain;
        public Alignment alignment = Alignment.center;

        private RenderQueue m_renderQueue;
        private CommandBuffer m_commandBuffer;

        private Rive.File m_file;
        private Artboard m_artboard;
        private StateMachine m_stateMachine;

        private Camera m_camera;

        private void Start()
        {
            renderTexture.enableRandomWrite = true;
            m_renderQueue = new RenderQueue(renderTexture);
            if (asset != null)
            {
                m_file = Rive.File.load(asset);
                m_artboard = m_file.artboard(0);
                m_stateMachine = m_artboard?.stateMachine();
            }

            if (m_artboard != null && renderTexture != null)
            {
                m_renderQueue.align(fit, alignment, m_artboard);
                m_renderQueue.draw(m_artboard);

                m_commandBuffer = new CommandBuffer();
                m_renderQueue.toCommandBuffer();
                m_commandBuffer.SetRenderTarget(renderTexture);
                m_commandBuffer.ClearRenderTarget(true, true, UnityEngine.Color.clear, 0.0f);
                m_renderQueue.addToCommandBuffer(m_commandBuffer);
                m_camera = Camera.main;
                if (m_camera != null)
                {
                    Camera.main.AddCommandBuffer(CameraEvent.AfterEverything, m_commandBuffer);
                }
            }
        }

        private Vector2 m_lastMousePosition;
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                m_stateMachine.pointerDown(Vector2.zero);
            }
            if (Input.GetMouseButtonUp(0))
            {
                m_stateMachine?.pointerUp(Vector2.zero);
            }

            if (m_stateMachine != null)
            {
                m_stateMachine.advance(Time.deltaTime);
            }
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
