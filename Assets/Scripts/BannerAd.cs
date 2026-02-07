using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;

public class BannerAd : MonoBehaviour
{
    [SerializeField] BannerPosition _bannerPosition = BannerPosition.BOTTOM_CENTER;

    [SerializeField] string _androidAdUnitId = "Banner_Android";
    [SerializeField] string _iOSAdUnitId = "Banner_iOS";
    string _adUnitId = null; // This will remain null for unsupported platforms.

    void Start()
    {
        // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif

        // Set the banner position:
        Advertisement.Banner.SetPosition(_bannerPosition);

        // Load and show the banner automatically:
        LoadAndShowBanner();
    }

    // Load and show the banner:
    public void LoadAndShowBanner()
    {
        // Set up options to notify the SDK of load events:
        BannerLoadOptions options = new BannerLoadOptions
        {
            loadCallback = OnBannerLoaded,
            errorCallback = OnBannerError
        };

        // Load the Ad Unit with banner content:
        Advertisement.Banner.Load(_adUnitId, options);
    }

    // Called when the banner is loaded successfully:
    void OnBannerLoaded()
    {
        Debug.Log("Banner loaded");

        // Set up options to notify the SDK of show events:
        BannerOptions options = new BannerOptions
        {
            clickCallback = OnBannerClicked,
            hideCallback = OnBannerHidden,
            showCallback = OnBannerShown
        };

        // Show the banner:
        Advertisement.Banner.Show(_adUnitId, options);
    }

    // Called when there's an error loading the banner:
    void OnBannerError(string message)
    {
        Debug.Log($"Banner Error: {message}");
    }

    // Optional callbacks for banner events:
    void OnBannerClicked() { Debug.Log("Banner clicked"); }
    void OnBannerShown() { Debug.Log("Banner shown"); }
    void OnBannerHidden() { Debug.Log("Banner hidden"); }

    void OnDestroy()
    {
        // Hide the banner when the script is destroyed:
        Advertisement.Banner.Hide();
    }
}
