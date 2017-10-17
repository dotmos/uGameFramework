using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Advertisements;

public class UnityAdsBuildProcessor : Editor
{
    #if UNITY_ADS
    [PostProcessScene]
    public static void OnPostprocessScene ()
    {
        AdvertisementSettings.enabled = true;
        AdvertisementSettings.initializeOnStartup = false;
    }
    #endif
}