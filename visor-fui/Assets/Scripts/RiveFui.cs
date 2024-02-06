using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Rive;

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

public class RiveFui : MonoBehaviour
{
    public Rive.Asset asset;
    public RenderTexture renderTexture;
    public Fit fit = Fit.contain;
    public Alignment alignment = Alignment.Center;

    private Rive.RenderQueue m_renderQueue;
    private Rive.Renderer m_riveRenderer;
    private CommandBuffer m_commandBuffer;

    private Rive.File m_file;
    private Artboard m_artboard;
    private StateMachine m_stateMachine;

    private Camera m_camera;

    private void Start()
    {
        renderTexture.enableRandomWrite = true;
        m_renderQueue = new Rive.RenderQueue(renderTexture);
        m_riveRenderer = m_renderQueue.Renderer();
        if (asset != null)
        {
            m_file = Rive.File.Load(asset);
            m_artboard = m_file.Artboard(0);
            m_stateMachine = m_artboard?.StateMachine();
        }

        if (m_artboard != null && renderTexture != null)
        {
            m_riveRenderer.Align(fit, alignment, m_artboard);
            m_riveRenderer.Draw(m_artboard);

            m_commandBuffer = m_riveRenderer.ToCommandBuffer();
            m_commandBuffer.SetRenderTarget(renderTexture);
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
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_stateMachine.PointerDown(Vector2.zero);
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_stateMachine?.PointerUp(Vector2.zero);
        }

        if (m_stateMachine != null)
        {
            m_stateMachine.Advance(Time.deltaTime);
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
