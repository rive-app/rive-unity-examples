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

        Renderer renderer = GetComponent<Renderer>();
        Material material = renderer.material;
        material.mainTexture = _renderTexture;

        m_renderQueue = new Rive.RenderQueue(_renderTexture);
        if (asset != null)
        {
            m_file = Rive.File.load(asset);
            m_artboard = m_file.artboard(0);
            m_stateMachine = m_artboard?.stateMachine();
        }

        if (m_artboard != null && _renderTexture != null)
        {
            m_renderQueue.align(fit, Alignment.center, m_artboard);
            m_renderQueue.draw(m_artboard);

            m_commandBuffer = new CommandBuffer();
            // m_renderQueue.toCommandBuffer();
            m_commandBuffer.SetRenderTarget(_renderTexture);
            m_commandBuffer.ClearRenderTarget(true, true, UnityEngine.Color.clear, 0.0f);
            m_renderQueue.addToCommandBuffer(m_commandBuffer);
        }
    }

    private void Update()
    {
        m_renderQueue.submit();
        if (m_stateMachine != null)
        {
            m_stateMachine.advance(Time.deltaTime);
        }
    }
}
