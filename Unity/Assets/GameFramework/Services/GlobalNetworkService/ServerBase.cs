using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Service.GlobalNetwork{
    public class ServerBase : GlobalNetworkServiceBase, IServer {

        /// <summary>
        /// Starts the server.
        /// </summary>
        /// <param name="port">Port.</param>
        /// <param name="maxConnections">Max connections.</param>
        /// <param name="pingTimeout">Ping timeout.</param>
        public virtual void StartServer(int port, int maxConnections, uint pingTimeout)
        {
            networkPort = port;
            //logLevel = LogFilter.FilterLevel.Fatal;
            connectionConfig.PingTimeout = pingTimeout;
            StartServer(connectionConfig, maxConnections);

            //Receive bytes from clients
            SetMessageHandler(NetworkMessageTypes.SendBytesToServer, OnReceivedBytesFromClient);

            //Forward tickrate to clients if they request it
            SetMessageHandler(NetworkMessageTypes.RequestTickrateFromServer, OnRequestTickrateFromServer);

            //Start ticking
            StartTicking();
        }

        /// <summary>
        /// Stops the server.
        /// </summary>
        new public virtual void StopServer(){
            ResetTicking();
            base.StopServer();
        }

        /// <summary>
        /// Sends the bytes to all clients.
        /// </summary>
        /// <param name="byteMessageID">Byte message I.</param>
        /// <param name="bytes">Bytes.</param>
        public virtual void SendBytesToAllClients(ushort byteMessageID, byte[] bytes)
        {
            SendMessageToAllClients(NetworkMessageTypes.SendBytesToClient, new NetworkMessages.SendBytesToClient(){byteMessageID = byteMessageID, bytes = bytes});
        }

        /// <summary>
        /// Sends the bytes to a client.
        /// </summary>
        /// <param name="byteMessageID">Byte message I.</param>
        /// <param name="bytes">Bytes.</param>
        /// <param name="clientConnectionID">Client connection I.</param>
        public virtual void SendBytesToClient(ushort byteMessageID, byte[] bytes, int clientConnectionID){
            SendMessageToClient(clientConnectionID, NetworkMessageTypes.SendBytesToClient, new NetworkMessages.SendBytesToClient(){byteMessageID = byteMessageID, bytes = bytes});
        }

        void OnReceivedBytesFromClient(NetworkMessage netMsg){
            NetworkMessages.SendBytesToServer msg = netMsg.ReadMessage<NetworkMessages.SendBytesToServer>();
            if(msg != null){
                //Forward bytes to receive handler
                BytesReceivedDelegate _brd = GetBytesReceivedHandler(msg.byteMessageID);
                if(_brd != null){
                    _brd(msg.bytes);
                }
            }
        }

        /// <summary>
        /// Called on server, when a client calls AddPlayer() function
        /// </summary>
        /// <param name="conn">Conn.</param>
        /// <param name="playerControllerId">Player controller identifier.</param>
        /// <param name="extraMessageReader">Extra message reader.</param>
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId, NetworkReader extraMessageReader)
        {
            //base.OnServerAddPlayer(conn, playerControllerId, extraMessageReader);

            //Read extra message
            AddPlayerExtraMessage extraMessage = extraMessageReader.ReadMessage<AddPlayerExtraMessage>();

            //Get spawn data
            SpawnHandlerData spawnData = GetSpawnHandlerData(extraMessage.spawnHandlerID);

            //Spawn gameobject on server depending on spawn data
            if(spawnData != null)
            {
                //Instantiate object
                GameObject serverGO = Instantiate(spawnData.serverPrefab);
                //Attach instance to client
                NetworkServer.AddPlayerForConnection(conn, serverGO, playerControllerId);
            }
            else
            {
                Debug.LogError("No spawnData for "+extraMessage.spawnHandlerID);
            }
        }

        /// <summary>
        /// Raises the request tickrate from server event.
        /// </summary>
        /// <param name="netMsg">Net message.</param>
        void OnRequestTickrateFromServer(NetworkMessage netMsg){
            SendMessageToClient(netMsg.conn.connectionId, NetworkMessageTypes.ReceiveTickrateFromServer, new NetworkMessages.ReceiveTickrateFromServer(){tickrate = Tickrate, currentTick = CurrentTick});
        }


        /// <summary>
        /// Sends the message to client.
        /// </summary>
        /// <param name="connectionId">Connection identifier.</param>
        /// <param name="msgType">Message type.</param>
        /// <param name="message">Message.</param>
        public virtual void SendMessageToClient(int connectionId, short msgType, MessageBase message){
            NetworkServer.SendToClient(connectionId, msgType, message);
        }

        /// <summary>
        /// Sends the message to all clients.
        /// </summary>
        /// <param name="msgType">Message type.</param>
        /// <param name="message">Message.</param>
        public virtual void SendMessageToAllClients(short msgType, MessageBase message){
            NetworkServer.SendToAll(msgType, message);
        }


        /// <summary>
        /// Spawns the prefab defined in spawnhandler on server. Will also spawn the object on all clients that are
        /// ready. Use AddSpawnHandler on both Client and Server before calling this
        /// </summary>
        /// <returns>The object.</returns>
        /// <param name="spawnHandlerID">Spawn handler I.</param>
        public virtual GameObject SpawnObject(string spawnHandlerID){

            //Get spawnhandler
            Service.GlobalNetwork.Commands.GetSpawnHandlerDataCommand cmd = new Commands.GetSpawnHandlerDataCommand(){id = spawnHandlerID};
            _eventService.Publish(cmd);
            if(cmd.result == null){
                Debug.LogWarning("SpawnHandler is null!");
                return null;
            }

            //Spawn on server
            GameObject go = GameObject.Instantiate(cmd.result.serverPrefab);

            //Spawn on client(s)
            NetworkServer.Spawn(go, cmd.result.serverPrefab.GetComponent<NetworkIdentity>().assetId);

            return go;
        }
    }
}
