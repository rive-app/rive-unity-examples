using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Rive;
using Rive.Components;

public class FontAssetSwapController : MonoBehaviour
{
    [Tooltip("The RiveWidget component to load the asset into")]
    [SerializeField] private RiveWidget m_riveWidget;

    [Tooltip("The Rive asset to load")]
    [SerializeField] private Asset m_asset;

    private static readonly string[] fontUrls = new[]
    {
        "https://cdn.rive.app/runtime/flutter/IndieFlower-Regular.ttf",
        "https://cdn.rive.app/runtime/flutter/comic-neue.ttf",
        "https://cdn.rive.app/runtime/flutter/inter.ttf",
        "https://cdn.rive.app/runtime/flutter/inter-tight.ttf",
        "https://cdn.rive.app/runtime/flutter/josefin-sans.ttf",
        "https://cdn.rive.app/runtime/flutter/send-flowers.ttf",
    };

    // Cache: maps font-index → loaded OOB font asset
    private readonly Dictionary<int, FontOutOfBandAsset> _cache = new Dictionary<int, FontOutOfBandAsset>();
    private int _currentIndex = 0;
    private bool _isSwapInProgress = false;

    private File _file;
    private FontEmbeddedAssetReference _fontRef;

    // Called by Rive during the file loading process when it encounters a font
    private bool OobAssetLoader(EmbeddedAssetReference embedded)
    {
        if (embedded is FontEmbeddedAssetReference f && _cache.ContainsKey(0))
        {
            f.SetFont(_cache[0]);
            _fontRef = f;
            return true;
        }
        return false;
    }

    private IEnumerator Start()
    {
        yield return DownloadAndCache(0);
        _cache[0].Load();

        _file = Rive.File.Load(m_asset, OobAssetLoader);
        m_riveWidget.Load(_file);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F) && !_isSwapInProgress && _fontRef != null)
        {
            int nextIndex = _currentIndex + 1;
            if (nextIndex >= fontUrls.Length)
                nextIndex = 0;
            StartCoroutine(SwapToFont(nextIndex));
        }
    }

    // Ensures only one swap at a time, downloads on demand, then swaps
    private IEnumerator SwapToFont(int idx)
    {
        _isSwapInProgress = true;

        // Download & cache if needed
        if (!_cache.ContainsKey(idx))
        {
            yield return DownloadAndCache(idx);
            _cache[idx].Load();
        }

        // Swap the font in the already‐loaded Rive file
        _fontRef.SetFont(_cache[idx]);
        _currentIndex = idx;

        _isSwapInProgress = false;
    }

    // Fetches raw TTF bytes and wraps them as an out-of-band asset
    private IEnumerator DownloadAndCache(int idx)
    {
        string url = fontUrls[idx];
        using (var www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to download font ({url}): {www.error}");
                yield break;
            }

            byte[] bytes = www.downloadHandler.data;
            var asset = OutOfBandAsset.Create<FontOutOfBandAsset>(bytes);
            _cache[idx] = asset;
        }
    }

    private void OnDestroy()
    {
        foreach (var asset in _cache.Values)
            asset.Unload();

        _file?.Dispose();
    }
}