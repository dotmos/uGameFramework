using System.Collections.Generic;

namespace Service.Advertisement{
    public class Events{

        public enum AdvertisementResultEnum{
            NotReady,
            Finished,
            Skipped,
            Failed
        }
        public class AdvertisementEventBase{
            public AdvertisementResultEnum result;
        }

        /// <summary>
        /// Shown event.
        /// </summary>
        public class ShownEvent : AdvertisementEventBase{
        }

        /// <summary>
        /// Rewarded shown event.
        /// </summary>
        public class RewardedShownEvent : AdvertisementEventBase{
        }
    }
}