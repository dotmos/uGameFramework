using System.Collections.Generic;
using System;
using FlatBuffers;
using System.Linq;
using System.Collections;
using System.Text;
using System.Threading;

namespace Service.Serializer {
    public class FlatBufferSerializer2 {

        public static ListPool<int> poolListInt = new ListPool<int>(10, 10);

        public enum Mode {
            serializing, deserializing
        }


        public static byte[] SerializeToBytes(IFBSerializable root,int initialBufferSize=5000000)  {
            byte[] buf = null;

            return buf;
        }

        public static T DeepCopy<T>(T original) where T : IFBSerializable,new() {
            byte[] buf = SerializeToBytes(original, 2048);
            T result = DeserializeFromBytes<T>(buf, default(T), original.GetType());
            return result;
        }

        public static void SerializeToFileDomain(FileSystem.FSDomain domain, String filename, IFBSerializable root)  {
            byte[] buf = SerializeToBytes(root);
            FileSystem.IFileSystemService fs = Kernel.Instance.Container.Resolve<Service.FileSystem.IFileSystemService>();
            fs.WriteBytesToFileAtDomain(domain, filename, buf);
        }

        public static T DeserializeFromBytes<T>(byte[] buf,T dataRoot=default(T), Type type = null) where T : IFBSerializable,new() {
            return dataRoot;
        }




    }


}
