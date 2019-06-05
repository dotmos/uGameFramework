/*
using UnityEngine.Networking;
using UnityEngine;

namespace Service.GlobalNetwork{
    public interface IServer : IGlobalNetworkService {
        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <param name="port">Port.</param>
        /// <param name="maxConnections">Max connections.</param>
        /// <param name="pingTimeout">Ping timeout.</param>
        void StartServer(int port, int maxConnections, uint pingTimeout);

        /// <summary>
        /// Stops the server.
        /// </summary>
        void StopServer();

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
        /// Sends the message to client.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="msgType">Message type.</param>
        /// <param name="message">Message.</param>
        void SendMessageToClient(int connectionId, short msgType, MessageBase message);
        /// <summary>
        /// Sends the message to client.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="msgType">Message type.</param>
        /// <param name="message">Message.</param>
        void SendMessageToAllClients(short msgType, MessageBase message);

        /// <summary>
        /// Sends the bytes to all clients.
        /// </summary>
        /// <param name="byteMessageID">Byte message I.</param>
        /// <param name="bytes">Bytes.</param>
        void SendBytesToAllClients(ushort byteMessageID, byte[] bytes);
        /// <summary>
        /// Sends the bytes to a client.
        /// </summary>
        /// <param name="byteMessageID">Byte message I.</param>
        /// <param name="bytes">Bytes.</param>
        /// <param name="clientConnectionID">Client connection I.</param>
        void SendBytesToClient(ushort byteMessageID, byte[] bytes, int clientConnectionID);

        /// <summary>
        /// Spawns the prefab defined in spawnhandler on server. Will also spawn the object on all clients that are ready. Use AddSpawnHandler on both Client and Server before calling this
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="spawnHandlerID">Spawn handler I.</param>
        GameObject SpawnObject(string spawnHandlerID);
    }
}
*/