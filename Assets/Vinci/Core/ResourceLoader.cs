using UnityEngine;

public static class ResourceLoader
{
    public static T Load<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }

    public static T[] LoadAll<T>(string path) where T : Object
    {
        return Resources.LoadAll<T>(path);
    }

    public static T LoadFromAssetBundle<T>(string bundlePath, string assetName) where T : Object
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
        if (bundle != null)
        {
            T asset = bundle.LoadAsset<T>(assetName);
            bundle.Unload(false);
            return asset;
        }
        else
        {
            Debug.LogError("Failed to load AssetBundle from path: " + bundlePath);
            return null;
        }
    }
}