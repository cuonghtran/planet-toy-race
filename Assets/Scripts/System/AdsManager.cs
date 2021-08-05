using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsListener
{
    public static AdsManager SharedInstance;

    private readonly string appStoreID = "4122858";
    private readonly string playStoreID = "4122859";

    //private readonly string videoAd = "video";
    private readonly string rewardedVideoAd = "rewardedVideo";
    private readonly string bannerAd = "banner";

    public bool isTestMode;

    private void Awake()
    {
        SharedInstance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Advertisement.AddListener(this);
        InitializeAds();
    }

    void InitializeAds()
    {
        string gameID = appStoreID;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
            gameID = appStoreID;
        else if (Application.platform == RuntimePlatform.Android)
            gameID = playStoreID;

        Advertisement.Initialize(gameID, isTestMode);
    }

    //public void PlayVideoAd()
    //{
    //    if (!Advertisement.IsReady(videoAd))
    //        return;
    //    Advertisement.Show(videoAd);
    //}

    public void PlayRewardedVideoAd()
    {
        if (!Advertisement.IsReady(rewardedVideoAd))
            return;
        Advertisement.Show(rewardedVideoAd);
    }

    public void ShowBannerAd()
    {
        if (!Advertisement.IsReady(bannerAd))
            return;
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_CENTER);
        Advertisement.Banner.Show(bannerAd);
    }

    public void HideBannerAd()
    {
        Advertisement.Banner.Hide();
    }

    public void OnUnityAdsReady(string placementId)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUnityAdsDidError(string message)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        //throw new System.NotImplementedException();
    }
}
