using Rive;
using Rive.Components;
using UnityEngine;

public class ImageAssetSwapController : MonoBehaviour
{
    [Tooltip("The RiveWidget component to load the asset into")]
    [SerializeField] private RiveWidget m_riveWidget;

    [Tooltip("The Rive asset to load")]
    [SerializeField] private Asset m_asset;

    [Tooltip("The image assets to swap in. Should be 300x500")]
    [SerializeField] private ImageOutOfBandAsset[] m_imageAssets;
    private File m_file;

    private ImageEmbeddedAssetReference m_imageEmbeddedAssetReference;

    private bool OobAssetLoaderDelegate(EmbeddedAssetReference assetReference)
    {
        if (assetReference is ImageEmbeddedAssetReference imageEmbeddedAssetReference && imageEmbeddedAssetReference.Name == "background_image")
        {
            ImageOutOfBandAsset imageAsset = GetNextImageAsset();

            imageEmbeddedAssetReference.SetImage(imageAsset);

            // Store the reference so we can change the image later if needed
            // We can do that by calling SetImage on this reference again with a new image outside of this callback
            m_imageEmbeddedAssetReference = imageEmbeddedAssetReference;

            return true;
        }
        return false;
    }

    private int m_imageIndex = 0;

    private ImageOutOfBandAsset GetNextImageAsset()
    {
        // Increase the index by 1 each time until it reaches the end of the array, then loop back to 0
        m_imageIndex++;
        if (m_imageIndex >= m_imageAssets.Length)
        {
            m_imageIndex = 0;
        }
        return m_imageAssets[m_imageIndex];
    }

    private void PreloadImageAssets()
    {
        foreach (ImageOutOfBandAsset imageAsset in m_imageAssets)
        {
            imageAsset.Load();
        }
    }

    private void UnloadImageAssets()
    {
        foreach (ImageOutOfBandAsset imageAsset in m_imageAssets)
        {
            imageAsset.Unload();
        }
    }

    void Start()
    {
        PreloadImageAssets();
        m_file = Rive.File.Load(m_asset, OobAssetLoaderDelegate);

        m_riveWidget.Load(m_file);

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            ImageOutOfBandAsset imageAsset = GetNextImageAsset();
            m_imageEmbeddedAssetReference.SetImage(imageAsset);
        }
    }

    private void OnDestroy()
    {
        UnloadImageAssets();
        m_file?.Dispose();
    }
}
