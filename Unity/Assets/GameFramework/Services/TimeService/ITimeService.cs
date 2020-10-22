///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 

using System.Collections.Generic;
using MoonSharp.Interpreter;
using Service.GameStateService;

using System;
using static Service.TimeService.Events;
using System.Runtime.Serialization;
using FlatBuffers;
using Service.Serializer;
using System.Linq;
using Service.PerformanceTest;

namespace Service.TimeService {
    public interface ITimeService : IFBSerializable2, IFBSerializable, IService {


        
        
        /// <summary>
        /// Adds a timer in the global update-method and calls the callback n-times (or infinite till application end) 
                /// <param name="interval"></param>
                /// <param name="callback"></param>
                /// <param name="repeatTimes"></param>
                /// <param name="info"></param>
         /// </summary>
        
        
                    TimerElement CreateGlobalTimer(float interval,Action callback,int repeatTimes,string info="");
        
                           
        
                    void RemoveGlobalTimer(TimerElement timer);
        
                           

    }

    
    
    [System.Serializable]
    public partial class TimerElement: DefaultSerializable2
    {
#if LEAK_DETECTION
        /// <summary>
        /// Did be put this instance into the leak-detection? 
        /// TODO: necessary?
        /// </summary>
        
        [System.NonSerialized]
        [Newtonsoft.Json.JsonIgnore]
        private bool leakDetectionCounted;
#endif
        

        public TimerElement() {
#if LEAK_DETECTION
            try {
                if (PerformanceTestServiceImpl.instance != null) {
                    PerformanceTestServiceImpl.instance.AddInstance(this);
                    leakDetectionCounted = true;
                }
            }
            catch (System.Exception e) {
                UnityEngine.Debug.Log($"Leak-Detection: Could not add instance {GetType()} to leak-detection: {e.Message}");
                UnityEngine.Debug.LogException(e);
            }
#endif

        }
        
        /// <summary>
        /// 
        /// </summary>
        
        public string info ;
        
        /// <summary>
        /// 
        /// </summary>
        
        public float timeLeft ;
        
        /// <summary>
        /// 
        /// </summary>
        
        public float interval ;
        
        /// <summary>
        /// 
        /// </summary>
        
        public int repeatTimes ;
        
        /// <summary>
        /// 
        /// </summary>
        
        public Action timerCallback ;
        
        


        

#if LEAK_DETECTION
        ~TimerElement () {
            if (leakDetectionCounted) {
                PerformanceTestServiceImpl.instance.RemoveInstance(this);
                leakDetectionCounted = false;
            }
        }


#endif
        /// <summary>
        /// Merges data into your object. (no deep copy)
        /// </summary>
        /// <param name="incoming"></param>
        /// <param name="onlyCopyPersistedData"></param>
        public void MergeDataFrom(TimerElement incoming, bool onlyCopyPersistedData = false) {
            if (incoming == null) {
                UnityEngine.Debug.LogError("Trying to merge from null! Type: TimerElement");
                return;
            }
            // base.MergeDataFrom(incoming, onlyCopyPersistedData);

            if (!onlyCopyPersistedData) this.info = incoming.info;
            if (!onlyCopyPersistedData) this.timeLeft = incoming.timeLeft;
            if (!onlyCopyPersistedData) this.interval = incoming.interval;
            if (!onlyCopyPersistedData) this.repeatTimes = incoming.repeatTimes;
            if (!onlyCopyPersistedData) this.timerCallback = incoming.timerCallback;
            
        }

        
    }

    public partial class TimerElement : IFBSerializeAsTypedObject, IMergeableData<TimerElement> {
    }
        
}
///////////////////////////////////////////////////////////////////////
//
// WARNING: THIS FILE IS AUTOGENERATED! DO NOT CHANGE IT
//
////////////////////////////////////////////////////////////////////// 
