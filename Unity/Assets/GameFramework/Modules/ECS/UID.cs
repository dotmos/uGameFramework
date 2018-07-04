using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ECS {
    /// <summary>
    /// Unique ID
    /// </summary>
    public struct UID {

        public int ID;

        public UID(int ID) {
            this.ID = ID;
        }
    }
}