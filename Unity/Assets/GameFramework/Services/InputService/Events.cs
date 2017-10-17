using UnityEngine;
using System.Collections;

namespace Service.Input{
    public class Events {
        /// <summary>
        /// Button down event.
        /// </summary>
        public class ButtonDownEvent{
            public object button;
        }

        /// <summary>
        /// Button up event.
        /// </summary>
        public class ButtonUpEvent{
            public object button;
        }
    }
}
