using UnityEngine;
using System.Collections;

namespace Service.GlobalNetwork{
    public class Channel{
        /// <summary>
        /// Reliable. Granted message order. Use for important stuff
        /// </summary>
        public const int ReliableSequenced = 0;
        /// <summary>
        /// Reliable. Use for sending big amount of important data
        /// </summary>
        public const int ReliableFragmented = 1;
        /// <summary>
        /// Unreliable sequenced. Granted message order. Use for unimportant stuff that should only take the newest message and drop older messages
        /// </summary>
        public const int UnreliableSequenced = 2;
        /// <summary>
        /// Unreliable. Not ordered. Use for unimportant stuff.
        /// </summary>
        public const int Unreliable = 3;
    }
}
