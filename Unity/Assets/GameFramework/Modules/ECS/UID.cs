﻿using System.Collections;
using System.Collections.Generic;

namespace ECS {
    /// <summary>
    /// Unique ID
    /// </summary>
    public struct UID {

        public int ID;

        public UID(int ID) {
            this.ID = ID;
        }

        public void SetID(int ID) {
            this.ID = ID;
        }
    }
}