using Zenject;
using UniRx;
using System;
using Service.Events;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace Service.GlobalNetwork{
    public class Commands : CommandsBase {

        IClient _client;
        IServer _server;

        [Inject]
        void Initialize(
            [InjectOptional] IServer server,
            [InjectOptional] IClient client
        ){
            _client = client;
            _server = server;

            //Start stop server
            this.OnEvent<StartServerCommand>().Subscribe(e => StartServerCommandHandler(e)).AddTo(this);
            this.OnEvent<StopServerCommand>().Subscribe(e => StopServerCommandHandler(e)).AddTo(this);
            //(Dis)Connect to server
            this.OnEvent<ConnectCommand>().Subscribe(e => ConnectCommandHandler(e)).AddTo(this);
            this.OnEvent<DisconnectCommand>().Subscribe(e => DisconnectCommandHandler(e)).AddTo(this);
            //Send receive messages
            this.OnEvent<SetMessageHandlerCommand>().Subscribe(e => SetMessageHandlerCommandHandler(e)).AddTo(this);
            this.OnEvent<RemoveMessageHandlerCommand>().Subscribe(e => RemoveMessageHandlerCommandHandler(e)).AddTo(this);
            this.OnEvent<SendMessageToAllClientsCommand>().Subscribe(e => SendMessageToAllClientsCommandHandler(e)).AddTo(this);
            this.OnEvent<SendMessageToClientCommand>().Subscribe(e => SendMessageToClientCommandHandler(e)).AddTo(this);
            this.OnEvent<SendMessageToServerCommand>().Subscribe(e => SendMessageToServerCommandHandler(e)).AddTo(this);
            //Spawn handlers
            this.OnEvent<AddSpawnHandlerCommand>().Subscribe(e => AddSpawnHandlerCommandHandler(e)).AddTo(this);
            this.OnEvent<RemoveSpawnHandlerCommand>().Subscribe(e => RemoveSpawnHandlerCommandHandler(e)).AddTo(this);
            this.OnEvent<GetSpawnHandlerDataCommand>().Subscribe(e => GetSpawnHandlerDataCommandHandler(e)).AddTo(this);
            this.OnEvent<AddPlayerControlledObjectCommand>().Subscribe(e => AddPlayerControlledObjectCommandHandler(e)).AddTo(this);
            //Spawn object
            this.OnEvent<SpawnObjectCommand>().Subscribe(e => SpawnObjectCommandHandler(e)).AddTo(this);
            //Ping and lag
            this.OnEvent<SimulateClientLagCommand>().Subscribe(e => SimulateClientLagCommandHandler(e)).AddTo(this);
            this.OnEvent<GetPingToServerCommand>().Subscribe(e => GetPingToServerCommandHandler(e)).AddTo(this);
            //Network tickable
            this.OnEvent<AddTickableCommand>().Subscribe(e => AddTickableCommandHandler(e)).AddTo(this);
            this.OnEvent<RemoveTickableCommand>().Subscribe(e => RemoveTickableCommandHandler(e)).AddTo(this);
            this.OnEvent<SetTickrateCommand>().Subscribe(e => SetTickrateCommandHandler(e)).AddTo(this);
            //Send/Receive bytes
            this.OnEvent<SetBytesReceivedHandlerCommand>().Subscribe(e => SetBytesReceivedHandlerCommandHandler(e)).AddTo(this);
            this.OnEvent<SendBytesToAllClientsCommand>().Subscribe(e => SendBytesToAllClientsCommandHandler(e)).AddTo(this);
            this.OnEvent<SendBytesToClientCommand>().Subscribe(e => SendBytesToClientCommandHandler(e)).AddTo(this);
            this.OnEvent<SendBytesToServerCommand>().Subscribe(e => SendBytesToServerCommandHandler(e)).AddTo(this);
        }

        /// <summary>
        /// Start server command.
        /// </summary>
        public class StartServerCommand{
            public int port = 7777;
            public int maxConnections = 8;
            public uint pingTimeout = 1000;
        }
        protected void StartServerCommandHandler(StartServerCommand cmd)
        {
            _server.StartServer(cmd.port, cmd.maxConnections, cmd.pingTimeout);
        }

        /// <summary>
        /// Stop server command.
        /// </summary>
        public class StopServerCommand{
        }
        protected void StopServerCommandHandler(StopServerCommand cmd)
        {
            _server.StopServer();
        }

        /// <summary>
        /// Connect command.
        /// </summary>
        public class ConnectCommand{
            public string ip = "localhost";
            public int port = 7777;
            public uint pingTimeout = 1000;
        }
        protected void ConnectCommandHandler(ConnectCommand cmd)
        {
            _client.Connect(cmd.ip, cmd.port, cmd.pingTimeout);
        }

        /// <summary>
        /// Disconnect command.
        /// </summary>
        public class DisconnectCommand{
        }
        protected void DisconnectCommandHandler(DisconnectCommand cmd)
        {
            _client.Disconnect();
        }

        /// <summary>
        /// Set message handler command.
        /// </summary>
        public class SetMessageHandlerCommand{
            public short msgType;
            public NetworkMessageDelegate handler;
        }
        protected void SetMessageHandlerCommandHandler(SetMessageHandlerCommand cmd){
            if(_client != null) _client.SetMessageHandler(cmd.msgType, cmd.handler);
            if(_server != null) _server.SetMessageHandler(cmd.msgType, cmd.handler);
        }

        /// <summary>
        /// Remove message handler command.
        /// </summary>
        public class RemoveMessageHandlerCommand{
            public short msgType;
        }
        protected void RemoveMessageHandlerCommandHandler(RemoveMessageHandlerCommand cmd){
            if(_client != null) _client.RemoveMessageHandler(cmd.msgType);
            if(_server != null) _server.RemoveMessageHandler(cmd.msgType);
        }

        /// <summary>
        /// Send message to client command.
        /// </summary>
        public class SendMessageToClientCommand{
            public int connectionId;
            public short msgType;
            public MessageBase message;
        }
        protected void SendMessageToClientCommandHandler(SendMessageToClientCommand cmd){
            _server.SendMessageToClient(cmd.connectionId, cmd.msgType, cmd.message);
        }

        /// <summary>
        /// Send message to all clients command.
        /// </summary>
        public class SendMessageToAllClientsCommand{
            public short msgType;
            public MessageBase message;
        }
        protected void SendMessageToAllClientsCommandHandler(SendMessageToAllClientsCommand cmd){
            _server.SendMessageToAllClients(cmd.msgType, cmd.message);
        }

        /// <summary>
        /// Send message to server command.
        /// </summary>
        public class SendMessageToServerCommand{
            public short msgType;
            public MessageBase message;
        }
        protected void SendMessageToServerCommandHandler(SendMessageToServerCommand cmd){
            _client.SendMessageToServer(cmd.msgType, cmd.message);
        }

        /// <summary>
        /// Add spawn handler command.
        /// </summary>
        public class AddSpawnHandlerCommand{
            public string id;
            public SpawnHandlerData data;
        }
        protected void AddSpawnHandlerCommandHandler(AddSpawnHandlerCommand cmd){
            if(_server != null) _server.AddSpawnHandler(cmd.id, cmd.data);
            if(_client != null) _client.AddSpawnHandler(cmd.id, cmd.data);
        }

        /// <summary>
        /// Get spawn handler data command.
        /// </summary>
        public class GetSpawnHandlerDataCommand{
            public string id;
            public SpawnHandlerData result;
        }
        protected void GetSpawnHandlerDataCommandHandler(GetSpawnHandlerDataCommand cmd){
            if(_server != null) cmd.result = _server.GetSpawnHandlerData(cmd.id);
            else if(_client != null) cmd.result = _client.GetSpawnHandlerData(cmd.id);
        }

        /// <summary>
        /// Remove spawn handler command.
        /// </summary>
        public class RemoveSpawnHandlerCommand{
            public string id;
        }
        protected void RemoveSpawnHandlerCommandHandler(RemoveSpawnHandlerCommand cmd){
            if(_server != null) _server.RemoveSpawnHandler(cmd.id);
            if(_client != null) _client.RemoveSpawnHandler(cmd.id);
        }

        /// <summary>
        /// Add player controlled object command.
        /// </summary>
        public class AddPlayerControlledObjectCommand{
            public string spawnHandlerID;
            public short playerControllerID;
        }
        protected void AddPlayerControlledObjectCommandHandler(AddPlayerControlledObjectCommand cmd){
            _client.AddPlayerControlledObject(cmd.spawnHandlerID, cmd.playerControllerID);
        }

        /// <summary>
        /// Spawn object command.
        /// </summary>
        public class SpawnObjectCommand{
            public string spawnHandlerID;
            public GameObject result;
        }
        protected void SpawnObjectCommandHandler(SpawnObjectCommand cmd){
            cmd.result = _server.SpawnObject(cmd.spawnHandlerID);
        }

        /// <summary>
        /// Simulate client lag command.
        /// </summary>
        public class SimulateClientLagCommand{
            public int ms;
            public float packetLossPercentage;
        }
        protected void SimulateClientLagCommandHandler( SimulateClientLagCommand cmd){
            _client.SimulateLag(cmd.ms, cmd.packetLossPercentage);
        }

        /// <summary>
        /// Get ping to server command.
        /// </summary>
        public class GetPingToServerCommand{
            public int result;
        }
        protected void GetPingToServerCommandHandler( GetPingToServerCommand cmd){
            cmd.result = _client.GetPingToServer();
        }

        /// <summary>
        /// Add tickable command.
        /// </summary>
        public class AddTickableCommand{
            public INetworkTickableObject tickable;
        }
        protected void AddTickableCommandHandler( AddTickableCommand cmd){
            if(_server != null) _server.AddTickable(cmd.tickable);
            else if(_client != null) _client.AddTickable(cmd.tickable);
        }

        /// <summary>
        /// Remove tickable command.
        /// </summary>
        public class RemoveTickableCommand{
            public INetworkTickableObject tickable;
        }
        protected void RemoveTickableCommandHandler(RemoveTickableCommand cmd){
            if(_server != null) _server.RemoveTickable(cmd.tickable);
            else if (_client != null) _client.RemoveTickable(cmd.tickable);
        }

        /// <summary>
        /// Set tickrate command.
        /// </summary>
        public class SetTickrateCommand{
            public int tickrate;
        }
        protected void SetTickrateCommandHandler(SetTickrateCommand cmd){
            if(_server != null) _server.SetTickrate(cmd.tickrate);
            else if(_client != null) _client.SetTickrate(cmd.tickrate);
        }


        /// <summary>
        /// Set bytes received handler command.
        /// </summary>
        public class SetBytesReceivedHandlerCommand{
            public ushort byteMessageID;
            public BytesReceivedDelegate handler;
        }
        protected void SetBytesReceivedHandlerCommandHandler(SetBytesReceivedHandlerCommand cmd){
            if(_server != null) _server.SetBytesReceivedHandler(cmd.byteMessageID, cmd.handler);
            else if(_client != null) _client.SetBytesReceivedHandler(cmd.byteMessageID, cmd.handler);
        }

        /// <summary>
        /// Send bytes to server command.
        /// </summary>
        public class SendBytesToServerCommand{
            public ushort byteMessageID;
            public byte[] bytes;
        }
        protected void SendBytesToServerCommandHandler(SendBytesToServerCommand cmd){
            if(_client != null) _client.SendBytesToServer(cmd.byteMessageID, cmd.bytes);
        }

        /// <summary>
        /// Send bytes to all clients command.
        /// </summary>
        public class SendBytesToAllClientsCommand{
            public ushort byteMessageID;
            public byte[] bytes;
        }
        protected void SendBytesToAllClientsCommandHandler(SendBytesToAllClientsCommand cmd){
            if(_server != null) _server.SendBytesToAllClients(cmd.byteMessageID, cmd.bytes);
        }

        /// <summary>
        /// Send bytes to client command.
        /// </summary>
        public class SendBytesToClientCommand : SendBytesToAllClientsCommand{
            public int clientConnectionID;
        }
        protected void SendBytesToClientCommandHandler(SendBytesToClientCommand cmd){
            if(_server != null) _server.SendBytesToClient(cmd.byteMessageID, cmd.bytes, cmd.clientConnectionID);
        }
    }

    public class CommandsInstaller : Installer<CommandsInstaller>{
        public override void InstallBindings()
        {
            Commands cmds = Container.Instantiate<Commands>();
            Container.BindAllInterfaces<Commands>().FromInstance(cmds);
        }
    }
}
