using System;
using System.Collections;
using System.Collections.Generic;
using FlatBuffers;
using Serial;


namespace ECS {
    /// <summary>
    /// Unique ID
    /// </summary>
    public struct UID : IFBSerializable, IEquatable<UID>, IEqualityComparer<UID> {

        public int ID;
        private int revision;
        public int Revision {
            get { return revision; }
            private set { revision = value; }
        }

        public UID(int ID, int revision) {
            this.ID = ID;
            this.revision = revision;
        }

        /*
        public void IncreaseRevision() {
            if(this.Revision + 1 >= int.MaxValue) {
                this.Revision = 0;
            }
            this.Revision++;
        }
        */

            /*
        public void SetID(int ID) {
            this.ID = ID;
        }
        */

        public bool IsNull() {
            return ID == 0;
        }

        public void SetNull() {
            ID = 0;
            revision = -1;
        }

        public override bool Equals(object obj) {
            //Generic equal. Creates garbage due to boxing
            if (obj is UID) {
                UID otherUID = (UID)obj;
                return Equals(otherUID);
            } else {
                return base.Equals(obj);
            }
        }

        //Garbage free IEquatable<UID> (non-boxing)
        public bool Equals(UID other) {
            return other.ID == ID && other.revision == revision;
        }

        // IEqualityComparer<UID>
        public bool Equals(UID x, UID y) {
            return x.Equals(y);
        }

        // IEqualityComparer<UID>
        public int GetHashCode(UID obj) {
            return obj.GetHashCode();
        }


        public static bool operator ==(UID c1, UID c2) {
            return c1.Equals(c2);// (c1.ID == c2.ID && c1.revision == c2.revision);
        }

        public static bool operator !=(UID c1, UID c2) {
            return !c1.Equals(c2); // (c1.ID != c2.ID || c1.revision != c2.revision);
        }

        public override int GetHashCode() {
            //Simple int hash
            //return ID;

            //Fast hashcode for ID & revision
            //See https://stackoverflow.com/questions/892618/create-a-hashcode-of-two-numbers
            int rotatedRevision = (revision << 16) | (revision >> (32 - 16));
            return ID ^ rotatedRevision;
        }

        /// <summary>
        /// Creates a null entity. NOTE: Do not use this to check if an entity is Null! Use UID.IsNull() instead!
        /// </summary>
        public static readonly UID CreateNull = new UID() { ID = 0, revision = -1 };



        //public Offset<Serial.FBUID> Serialize(FlatBufferBuilder builder) {
        //    return Serial.FBUID.CreateFBUID(builder, ID, revision);
        //}

        public void Deserialize(object incoming) {
            FBUID data = (Serial.FBUID)incoming;
            ID = data.Id;
            revision = data.Revision;
        }

        public int Serialize(FlatBufferBuilder builder) {
            return Serial.FBUID.CreateFBUID(builder,ID,revision).Value;
        }

        public void Deserialize(ByteBuffer buf) {
            throw new System.NotImplementedException();
        }

        public override string ToString() {
            return "UID:"+ID.ToString();
        }
        
    }

}