using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace Service.GlobalNetwork{
    public class ClientBase : GlobalNetworkServiceBase, IClient {
        /// <summary>
        /// Connect to a server
        /// </summary>
        /// <param name="inIP">In I.</param>
        /// <param name="inPort">In port.</param>
        /// <param name="pingTimeout">Ping timeout.</param>
        public virtual void Connect(string inIP, int inPort = -1, uint pingTimeout = 1000)
        {
            networkAddress = inIP;
            if(inPort != -1)
                networkPort = inPort;

            connectionConfig.PingTimeout = pingTimeout;

            Debug.Log("Connecting to:"+inIP+":"+networkPort.ToString());
            StartClient(matchInfo, connectionConfig);
        }

        /// <summary>
        /// Disconnect from server
        /// </summary>
        public virtual void Disconnect()
        {
            StopClient();
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            //Setup message handler to receive bytes from server
            SetMessageHandler(NetworkMessageTypes.SendBytesToClient, OnReceivedBytesFromServer);

            //Setup message handler to receive server tickrate
            SetMessageHandler(NetworkMessageTypes.ReceiveTickrateFromServer, OnReceiveTickrateFromServer);
            //Request tickrate from server
            SendMessageToServer(NetworkMessageTypes.RequestTickrateFromServer, new NetworkMessages.RequestTickrateFromServer());
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);

            ResetTicking();
        }

        /// <summary>
        /// Sends the bytes to server.
        /// </summary>
        /// <param name="byteMessageID">Byte message I.</param>
        /// <param name="bytes">Bytes.</param>
        public virtual void SendBytesToServer(ushort byteMessageID, byte[] bytes)
        {
            SendMessageToServer(NetworkMessageTypes.SendBytesToServer, new NetworkMessages.SendBytesToServer(){byteMessageID = byteMessageID, bytes = bytes});
        }

        void OnReceivedBytesFromServer(NetworkMessage netMsg){
            NetworkMessages.SendBytesToClient msg = netMsg.ReadMessage<NetworkMessages.SendBytesToClient>();
            if(msg != null){
                //Forward bytes to receive handler
                BytesReceivedDelegate _brd = GetBytesReceivedHandler(msg.byteMessageID);
                if(_brd != null){
                    _brd(msg.bytes);
                }
            }
        }

        /// <summary>
        /// Raises the receive tickrate from server event.
        /// </summary>
        /// <param name="netMsg">Net message.</param>
        void OnReceiveTickrateFromServer(NetworkMessage netMsg){
            NetworkMessages.ReceiveTickrateFromServer msg = netMsg.ReadMessage<NetworkMessages.ReceiveTickrateFromServer>();
            if(msg != null){
                StopTicking();
                CurrentTick = msg.currentTick;
                Tickrate = msg.tickrate;
                StartTicking();
            }
        }

        /// <summary>
        /// Simulates the lag.
        /// </summary>
        /// <param name="ms">Ms.</param>
        /// <param name="packetLossPercentage">Packet loss percentage.</param>
        public virtual void SimulateLag(int ms, float packetLossPercentage){
            if(ms > 0 || packetLossPercentage > 0){
                this.useSimulator = true;
                this.packetLossPercentage = packetLossPercentage;
                this.simulatedLatency = ms;
            } else {
                this.useSimulator = false;
            }
        }

//        protected override void RegisterSpawnHandlersOnClient()
//        {
//            base.RegisterSpawnHandlersOnClient();
//
//            foreach(KeyValuePair<string, SpawnHandlerData> kv in spawnHandlerData)
//            {
//                //Cache spawn data
//                SpawnHandlerData spawnData = kv.Value;
//                //Sanity check if spawnData has all needed data
//                if(spawnData.serverPrefab == null || spawnData.clientSpawnDelegate == null || spawnData.clientUnSpawnDelegate == null)
//                {
//                    Debug.LogError("SpawnData is missing needed data (spawn/unspawn delegate or server prefab)");
//                    continue;
//                }
//
//                //Create a custom handler for the server prefab, so we can spawn a custom client prefab
//                ClientScene.RegisterSpawnHandler( spawnData.serverPrefab.GetComponent<NetworkIdentity>().assetId, spawnData.clientSpawnDelegate, spawnData.clientUnSpawnDelegate );
//                Debug.LogWarning("YES! "+spawnData.serverPrefab.GetComponent<NetworkIdentity>().assetId);
//            }
//        }

        /// <summary>
        /// Spawns a new player controlled object (NOT player authorative!) on server and client using a formerly registered spawn handler
        /// </summary>
        /// <param name="spawnHandlerID">Spawn handler I.</param>
        /// <param name="playerControllerID">Player controller I.</param>
        public void AddPlayerControlledObject(string spawnHandlerID, short playerControllerID)
        {
            ClientScene.AddPlayer(client.connection, playerControllerID, new AddPlayerExtraMessage(){spawnHandlerID = spawnHandlerID});
        }

        /// <summary>
        /// Sends the message to server.
        /// </summary>
        /// <param name="msgType">Message type.</param>
        /// <param name="message">Message.</param>
        public virtual void SendMessageToServer(short msgType, MessageBase message){
            client.Send(msgType, message);
        }

        /// <summary>
        /// Sends the message to server using a network writer
        /// </summary>
        /// <param name="msgType">Message type.</param>
        /// <param name="message">Message.</param>
        /// <param name="writer">Writer.</param>
        /// <param name="channelID">Channel ID</param>
        public virtual void SendMessageToServer(NetworkWriter writer, int channelID){
            client.SendWriter(writer, channelID);
        }

        /// <summary>
        /// Gets the ping to server.
        /// </summary>
        /// <returns>The ping to server.</returns>
        public virtual int GetPingToServer(){
            return client.GetRTT();
        }
    }
}