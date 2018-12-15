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

        public bool IsNull() {
            return ID == 0;
        }

        public override bool Equals(object obj) {
            if (obj is UID) {
                return ((UID)obj).ID == ID;
            } else {
                return base.Equals(obj);
            }
        }

        public static bool operator ==(UID c1, UID c2) {
            return c1.ID == c2.ID;
        }

        public static bool operator !=(UID c1, UID c2) {
            return c1.ID != c2.ID;
        }
        /*
        public override int GetHashCode() {
            return ID;
        }*/

        public static readonly UID NULL = new UID() { ID = 0 };
    }

}