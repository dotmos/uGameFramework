using UnityEngine;
using System.Collections;
using Zenject;

namespace Service.Advertisement{
    public class UnityAdsService : IAdvertisementService{

        const string videoZoneID = "video";
        const string rewardedVideoZoneID = "rewardedVideo";

#if UNITY_ADS
        private Service.Events.IEventsService _eventService;
#endif

        public UnityAdsService(string androidGameID, string iosGameID, bool testMode)
        {
#if UNITY_ADS
            // If the platform is supported,
            if (UnityEngine.Advertisements.Advertisement.isSupported && !UnityEngine.Advertisements.Advertisement.isInitialized) { 
                // initialize Unity Ads.
#if UNITY_ANDROID
                UnityEngine.Advertisements.Advertisement.Initialize(androidGameID, testMode);
#elif UNITY_IOS
                UnityEngine.Advertisements.Advertisement.Initialize(iosGameID, testMode);
#endif
            }
#endif
        }

        [Inject]
        void Initialize(
            [Inject] Service.Events.IEventsService eventService
        )
        {
#if UNITY_ADS
            _eventService = eventService;
#endif
        }


        /// <summary>
        /// Shows an interstitial ad
        /// </summary>
        public void Show(){
#if UNITY_ADS
            if (!UnityEngine.Advertisements.Advertisement.IsReady(videoZoneID))
            {
                Debug.Log("Ads not ready for "+videoZoneID);
                _eventService.Publish(new Events.ShownEvent(){result = Events.AdvertisementResultEnum.NotReady});
                return;
            }

            UnityEngine.Advertisements.ShowOptions options = new UnityEngine.Advertisements.ShowOptions();
            options.resultCallback = HandleAdShown;
            UnityEngine.Advertisements.Advertisement.Show(videoZoneID, options);
#endif
        }
#if UNITY_ADS
        private void HandleAdShown (UnityEngine.Advertisements.ShowResult result)
        {
            switch (result)
            {
                case UnityEngine.Advertisements.ShowResult.Finished:
                    Debug.Log ("Video completed.");
                    _eventService.Publish(new Events.ShownEvent(){result = Events.AdvertisementResultEnum.Finished});
                    break;
                case UnityEngine.Advertisements.ShowResult.Skipped:
                    Debug.LogWarning ("Video was skipped.");
                    _eventService.Publish(new Events.ShownEvent(){result = Events.AdvertisementResultEnum.Skipped});
                    break;
                case UnityEngine.Advertisements.ShowResult.Failed:
                    Debug.LogError ("Video failed to show.");
                    _eventService.Publish(new Events.ShownEvent(){result = Events.AdvertisementResultEnum.Failed});
                    break;
            }
        }
#endif


        /// <summary>
        /// Shows a rewarded ad
        /// </summary>
        public void ShowRewarded(){
#if UNITY_ADS
            if (!UnityEngine.Advertisements.Advertisement.IsReady(rewardedVideoZoneID))
            {
                Debug.Log("Ads not ready for "+rewardedVideoZoneID);
                _eventService.Publish(new Events.RewardedShownEvent(){result = Events.AdvertisementResultEnum.NotReady});
                return;
            }

            UnityEngine.Advertisements.ShowOptions options = new UnityEngine.Advertisements.ShowOptions();
            options.resultCallback = HandleRewardedShown;
            UnityEngine.Advertisements.Advertisement.Show(rewardedVideoZoneID, options);
#endif
        }
#if UNITY_ADS
        private void HandleRewardedShown (UnityEngine.Advertisements.ShowResult result)
        {
            switch (result)
            {
                case UnityEngine.Advertisements.ShowResult.Finished:
                    Debug.Log ("Reward Video completed.");
                    _eventService.Publish(new Events.RewardedShownEvent(){result = Events.AdvertisementResultEnum.Finished});
                    break;
                case UnityEngine.Advertisements.ShowResult.Skipped:
                    Debug.LogWarning ("Reward Video was skipped.");
                    _eventService.Publish(new Events.RewardedShownEvent(){result = Events.AdvertisementResultEnum.Skipped});
                    break;
                case UnityEngine.Advertisements.ShowResult.Failed:
                    Debug.LogError ("Reward Video failed to show.");
                    _eventService.Publish(new Events.RewardedShownEvent(){result = Events.AdvertisementResultEnum.Failed});
                    break;
            }
        }
#endif

    }
}
