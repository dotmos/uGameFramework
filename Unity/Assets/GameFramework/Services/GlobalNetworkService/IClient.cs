/*
using UnityEngine.Networking;

namespace Service.GlobalNetwork{
    public interface IClient : IGlobalNetworkService {
        /// <summary>
        /// Connect to a server
        /// </summary>
        /// <param name="inIP">In I.</param>
        /// <param name="inPort">In port.</param>
        /// <param name="pingTimeout">Ping timeout.</param>
        void Connect(string ip, int port, uint pingTimeout);
        /// <summary>
        /// Disconnect from the current server
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Simulates the lag.
        /// </summary>
        /// <param name="ms">Ms.</param>
        /// <param name="packetLossPercentage">Packet loss percentage.</param>
        void SimulateLag(int ms, float packetLossPercentage);

        /// <summary>
        /// Adds new spawn handler, or overwrites it if the supplied id already exists.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="data">Data.</param>
        void AddSpawnHandler(string id, SpawnHandlerData data);
        /// <summary>
        /// Removes the spawn handler.
        /// </summary>
        /// <param name="id">Identifier.</param>
        void RemoveSpawnHandler(string id);
        /// <summary>
        /// Gets the spawn handler data.
        /// </summary>
        /// <returns>The spawn handler data.</returns>
        /// <param name="id">Identifier.</param>
        SpawnHandlerData GetSpawnHandlerData(string id);
        /// <summary>
        /// Spawns a new player controlled object (NOT player authorative!) on server and client using a formerly registered spawn handler
        /// </summary>
        /// <param name="spawnHandlerID">Spawn handler I.</param>
        /// <param name="playerControllerID">Player controller I.</param>
        void AddPlayerControlledObject(string spawnHandlerID, short playerControllerID);

        /// <summary>
        /// Sets a message handler.
        /// </summary>
        /// <param name="msgType">Message type.</param>
        /// <param name="handler">Handler.</param>
        void SetMessageHandler(short msgType, NetworkMessageDelegate handler );
        /// <summary>
        /// Removes a message handler.
        /// </summary>
        /// <param name="msgType">Message type.</param>
        void RemoveMessageHandler(short msgType);
        /// <summary>
        /// Sends the message to server.
        /// </summary>
        /// <param name="msgType">Message type.</param>
        /// <param name="message">Message.</param>
        void SendMessageToServer(short msgType, MessageBase message);
        /// <summary>
        /// Sends the message to server using a network writer
        /// </summary>
        /// <param name="msgType">Message type.</param>
        /// <param name="message">Message.</param>
        /// <param name="writer">Writer.</param>
        /// <param name="channelID">Channel I.</param>
        void SendMessageToServer(NetworkWriter writer, int channelID);

        /// <summary>
        /// Sends the bytes to server.
        /// </summary>
        /// <param name="byteMessageID">Byte message I.</param>
        /// <param name="bytes">Bytes.</param>
        void SendBytesToServer(ushort byteMessageID, byte[] bytes);

        /// <summary>
        /// Gets the ping to server.
        /// </summary>
        /// <returns>The ping to server.</returns>
        int GetPingToServer();
    }
}
*/