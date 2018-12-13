using System.Collections;
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
            return ID == -1;
        }

        public override bool Equals(object obj) {
            if (obj is UID) {
                return ((UID)obj).ID == ID;
            } else {
                return base.Equals(obj);
            }
        }

        public static readonly UID NULL = new UID() { ID = -1 };
    }

}