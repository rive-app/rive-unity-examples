using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using Rive;

using LoadAction = UnityEngine.Rendering.RenderBufferLoadAction;
using StoreAction = UnityEngine.Rendering.RenderBufferStoreAction;

[ExecuteInEditMode]
public class RiveTexture : MonoBehaviour
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

    private void Update()
    {

        HitTesting();

        if (m_stateMachine != null)
        {
            m_stateMachine.Advance(Time.deltaTime);
        }
    }

    bool m_wasMouseDown = false;
    private Vector2 m_lastMousePosition;

    void HitTesting()
    {
        Camera camera = Camera.main;

        if (camera == null || renderTexture == null || m_artboard == null) return;

        if (!Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            return;

        UnityEngine.Renderer rend = hit.transform.GetComponent<UnityEngine.Renderer>();
        MeshCollider meshCollider = hit.collider as MeshCollider;

        if (rend == null || rend.sharedMaterial == null || rend.sharedMaterial.mainTexture == null || meshCollider == null)
            return;

        Vector2 pixelUV = hit.textureCoord;

        pixelUV.x *= renderTexture.width;
        pixelUV.y *= renderTexture.height;

        Vector3 mousePos = camera.ScreenToViewportPoint(Input.mousePosition);
        Vector2 mouseRiveScreenPos = new(mousePos.x * camera.pixelWidth, (1 - mousePos.y) * camera.pixelHeight);

        if (m_lastMousePosition != mouseRiveScreenPos || transform.hasChanged)
        {
            Vector2 local = m_artboard.LocalCoordinate(pixelUV, new Rect(0, 0, renderTexture.width, renderTexture.height), fit, alignment);
            m_stateMachine?.PointerMove(local);
            m_lastMousePosition = mouseRiveScreenPos;
        }
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 local = m_artboard.LocalCoordinate(pixelUV, new Rect(0, 0, renderTexture.width, renderTexture.height), fit, alignment);
            m_stateMachine?.PointerDown(local);
            m_wasMouseDown = true;
        }
        else if (m_wasMouseDown)
        {
            m_wasMouseDown = false; Vector2 local = m_artboard.LocalCoordinate(mouseRiveScreenPos, new Rect(0, 0, renderTexture.width, renderTexture.height), fit, alignment);
            m_stateMachine?.PointerUp(local);
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
