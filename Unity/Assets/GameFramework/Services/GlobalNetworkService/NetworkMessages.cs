using UnityEngine.Networking;

public partial class NetworkMessageTypes {

    public const short SendBytesToServer = 40;
    public const short SendBytesToClient = 41;


    public const short RequestTickrateFromServer = 50;
    public const short ReceiveTickrateFromServer = 51;

}

public partial class NetworkMessages{
    public class SendBytesToServer : MessageBase{
        public ushort byteMessageID;
        public byte[] bytes;
    }

    public class SendBytesToClient : MessageBase{
        public ushort byteMessageID;
        public byte[] bytes;
    }


    public class RequestTickrateFromServer : MessageBase{
    }

    public class ReceiveTickrateFromServer : MessageBase{
        public uint currentTick;
        public int tickrate;
    }
}
