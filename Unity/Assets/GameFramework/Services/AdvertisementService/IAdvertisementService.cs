namespace Service.Advertisement{
    public interface IAdvertisementService{
        /// <summary>
        /// Shows an interstitial ad
        /// </summary>
        void Show();
        /// <summary>
        /// Shows a rewarded ad
        /// </summary>
        void ShowRewarded();
    }
}