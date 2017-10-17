using UnityEngine;
using System.Collections;
using Zenject;

public partial class Kernel : SceneContext {
    //Kernel events
    public class Events{
        public class KernelReadyEvent{
        }
        /// <summary>
        /// Fired when the user tabs into the game
        /// </summary>
        public class OnApplicationFocus {
        }
        /// <summary>
        /// Fired when user tabs out of the game
        /// </summary>
        public class OnApplicationLostFocus {
        }

        /// <summary>
        /// Fired the application is quitted
        /// </summary>
        public class OnApplicationQuit {
        }
    }
}
