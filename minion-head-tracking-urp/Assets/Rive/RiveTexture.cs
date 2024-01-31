using UnityEngine;
using UnityEngine.Rendering;

using Rive;

public class RiveTexture : MonoBehaviour
{
    public Asset asset;
    public Fit fit = Fit.contain;
    public int size = 512;

    private RenderTexture _renderTexture;
    private Rive.RenderQueue m_renderQueue;
    private Rive.Renderer m_riveRenderer;
    private CommandBuffer m_commandBuffer;

    private File m_file;
    private Artboard m_artboard;
    private StateMachine m_stateMachine;
    public StateMachine stateMachine => m_stateMachine;

    private void Awake()
    {
        _renderTexture = new RenderTexture(
            size,
             size,
             0,
             RenderTextureFormat.ARGB32
         );
        _renderTexture.enableRandomWrite = true;

        UnityEngine.Renderer renderer = GetComponent<UnityEngine.Renderer>();
        Material material = renderer.material;
        material.mainTexture = _renderTexture;

        m_renderQueue = new Rive.RenderQueue(_renderTexture);
        m_riveRenderer = m_renderQueue.Renderer();
        if (asset != null)
        {
            m_file = Rive.File.Load(asset);
            m_artboard = m_file.Artboard(0);
            m_stateMachine = m_artboard?.StateMachine();
        }

        if (m_artboard != null && _renderTexture != null)
        {
            m_riveRenderer.Align(fit, Alignment.Center, m_artboard);
            m_riveRenderer.Draw(m_artboard);

            m_commandBuffer = new CommandBuffer();
            // m_renderQueue.toCommandBuffer();
            m_commandBuffer.SetRenderTarget(_renderTexture);
            m_commandBuffer.ClearRenderTarget(true, true, UnityEngine.Color.clear, 0.0f);
            m_riveRenderer.AddToCommandBuffer(m_commandBuffer);
        }
    }

    private void Update()
    {
        m_riveRenderer.Submit();
        if (m_stateMachine != null)
        {
            m_stateMachine.Advance(Time.deltaTime);
        }
    }
}
