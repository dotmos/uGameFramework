/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UniRx;
using Zenject;
using Service.Events;
using System;

namespace Service.GlobalNetwork{
    public class GlobalNetworkServiceBase : NetworkManager,IGlobalNetworkService, IDisposable {

        public class NetworkChannels{
            public readonly int ReliableSequenced = 1;
            public readonly int ReliableFragmented = 2;
        }
        public static NetworkChannels networkChannels = new NetworkChannels();

        //Event service
        protected IEventsService _eventService;
        //Disposable Manager
        DisposableManager _dManager;

        /// <summary>
        /// Gets the network state property.
        /// </summary>
        /// <value>The network state property.</value>
        public ReactiveProperty<NetworkStateEnum> NetworkStateProperty{
            get;
            private set;
        }
        protected NetworkStateEnum NetworkState{
            get{
                return NetworkStateProperty.Value;
            }
            set{
                NetworkStateProperty.Value = value;
            }
        }


        /// <summary>
        /// The tickrate property.
        /// </summary>
        protected ReactiveProperty<int> TickrateProperty = new ReactiveProperty<int>();
        /// <summary>
        /// Gets the tickrate.
        /// </summary>
        /// <value>The tickrate.</value>
        public int Tickrate{
            get {return TickrateProperty.Value;}
            protected set{
                TickrateProperty.Value = Mathf.Max(1, value);
            }
        }
        /// <summary>
        /// The time in seconds between each tick
        /// </summary>
        /// <value>The tick delta.</value>
        protected float TickDelta{
            get{ return 1.0f/(float)Tickrate; }
        }


        protected ReactiveProperty<uint> CurrentTickProperty = new ReactiveProperty<uint>();
        /// <summary>
        /// Gets the current tick.
        /// </summary>
        /// <value>The current tick.</value>
        public uint CurrentTick{
            get{return CurrentTickProperty.Value;}
            set{CurrentTickProperty.Value = value;}
        }
        List<INetworkTickableObject> networkTickables = new List<INetworkTickableObject>();
        IDisposable tickDisposable;
        float _lastNetworkTick;

        Dictionary<ushort, BytesReceivedDelegate> bytesReceivedDelegates = new Dictionary<ushort, BytesReceivedDelegate>();

        bool isInitialized = false;

        void Awake(){
            //Prevent "DontDestroyOnLoad only work for root GameObjects or components on root GameObjects." warning when creating the service
            dontDestroyOnLoad = false;
        }

        //Inject stuff and initialize service
        [Inject]
        void Initialize(
            [Inject] IEventsService eventService,
            [Inject] DisposableManager dManager
        )
        {
            if(isInitialized) return;

            _eventService = eventService;
            _dManager = dManager;

            _dManager.Add(this);

            NetworkStateProperty = new ReactiveProperty<NetworkStateEnum>(NetworkStateEnum.Disconnected);

            customConfig = true;

            autoCreatePlayer = false;
            scriptCRCCheck = false;

            connectionConfig.AddChannel(QosType.ReliableSequenced);
            connectionConfig.AddChannel(QosType.ReliableFragmented);
            connectionConfig.AddChannel(QosType.UnreliableSequenced);
            connectionConfig.AddChannel(QosType.Unreliable);
            connectionConfig.DisconnectTimeout = 2000;
            connectionConfig.PingTimeout = 1500;
            connectionConfig.NetworkDropThreshold = 90;

            isInitialized = true;

            Tickrate = 15;
        }

        /// <summary>
        /// Sets the bytes received handler. Setting handler to null will remove a handler.
        /// </summary>
        /// <param name="byteMessageID">Byte message I.</param>
        /// <param name="handler">Handler.</param>
        public void SetBytesReceivedHandler(ushort byteMessageID, BytesReceivedDelegate handler){
            //If handler is null, remove existing handler, if there is one
            if(handler == null){
                if(GetBytesReceivedHandler(byteMessageID) != null) bytesReceivedDelegates.Remove(byteMessageID);
            }
            else {
                //Replace handler if there already is a handler
                if(GetBytesReceivedHandler(byteMessageID) != null){
                    bytesReceivedDelegates[byteMessageID] = handler;
                }
                //Add new handler if there is no handler at the moment
                else {
                    bytesReceivedDelegates.Add(byteMessageID, handler);
                }
            }
        }

        /// <summary>
        /// Gets the bytes received handler.
        /// </summary>
        /// <returns>The bytes received handler.</returns>
        /// <param name="byteMessageID">Byte message I.</param>
        protected BytesReceivedDelegate GetBytesReceivedHandler(ushort byteMessageID){
            BytesReceivedDelegate _brd;
            bytesReceivedDelegates.TryGetValue(byteMessageID, out _brd);
            return _brd;
        }

        /// <summary>
        /// Sets the tickrate.
        /// </summary>
        /// <param name="tickrate">Tickrate.</param>
        public void SetTickrate(int tickrate){
            Tickrate = tickrate;
        }

        /// <summary>
        /// Starts the ticking.
        /// </summary>
        protected void StartTicking(){
            StopTicking();
            _lastNetworkTick = UnityEngine.Time.time;

            //Note: Observable.Interval can't be used here, as it is not 100% correct. Ticktimes are different each tick as the remainder of the tick is not being taken into account...
            //tickDisposable = Observable.Interval(TimeSpan.FromMilliseconds(1000.0/(double)Tickrate)).Subscribe(e => OnTick());
            //Custom interval solution, taking remainder into account
            tickDisposable = Observable.EveryUpdate().Where(v => UnityEngine.Time.time - _lastNetworkTick >= TickDelta).Subscribe(e => OnTick());

            OnStartTick();
        }

        /// <summary>
        /// Stops the ticking.
        /// </summary>
        protected void StopTicking(){
            if(tickDisposable != null) tickDisposable.Dispose();
            OnStopTick();
        }

        /// <summary>
        /// Resets the ticking.
        /// </summary>
        protected void ResetTicking(){
            StopTicking();
            CurrentTick = 0;
        }
            
        /// <summary>
        /// Called on every tick. Based on Tickrate
        /// </summary>
        protected virtual void OnTick(){
            float _tickDelta = TickDelta; //Time.time - _lastNetworkTick;
            for(int i=0; i<networkTickables.Count; ++i){
                INetworkTickableObject _tickable = networkTickables[i];
                _tickable.NetworkTickDelta = _tickDelta;
                _tickable.CurrentNetworkTick = CurrentTick;
                _tickable.NetworkTick(CurrentTick);
            }
            CurrentTick++;
            //Set last network tick. Take into account possible remaining milliseconds so a tick is always fired at the same time. This is needed so server/client don't desync
            _lastNetworkTick = UnityEngine.Time.time - ((UnityEngine.Time.time - _lastNetworkTick) - _tickDelta);
        }

        protected virtual void OnStartTick(){
        }

        protected virtual void OnStopTick(){
        }

        /// <summary>
        /// Adds the tickable.
        /// </summary>
        /// <param name="tickable">Tickable.</param>
        public void AddTickable(INetworkTickableObject tickable){
            if(!networkTickables.Contains(tickable)) networkTickables.Add(tickable);
        }
        /// <summary>
        /// Removes the tickable.
        /// </summary>
        /// <param name="tickable">Tickable.</param>
        public void RemoveTickable(INetworkTickableObject tickable){
            if(networkTickables.Contains(tickable)) networkTickables.Remove(tickable);
        }

        /// <summary>
        /// Register a spawn handler on the client
        /// </summary>
        protected virtual void RegisterSpawnHandlerOnClient(SpawnHandlerData spawnData)
        {
            if(spawnData.serverPrefab == null || spawnData.clientSpawnDelegate == null || spawnData.clientUnSpawnDelegate == null)
            {
                Debug.LogWarning("SpawnData is missing needed data (spawn/unspawn delegate or server prefab)");
                return;
            }

            //Create a custom handler for the server prefab, so we can spawn a custom client prefab
            ClientScene.RegisterSpawnHandler( spawnData.serverPrefab.GetComponent<NetworkIdentity>().assetId, spawnData.clientSpawnDelegate, spawnData.clientUnSpawnDelegate );
        }

        /// <summary>
        /// Unregister a spawnhandler on the client
        /// </summary>
        /// <param name="spawnData">Spawn data.</param>
        protected virtual void UnregisterSpawnHandlerOnClient(SpawnHandlerData spawnData)
        {
            if(spawnData.serverPrefab == null)
            {
                Debug.LogWarning("No serverPrefab set for spawnHandler!");
                return;
            }
            ClientScene.UnregisterSpawnHandler(spawnData.serverPrefab.GetComponent<NetworkIdentity>().assetId);
        }

        //Holds all spawnhandler data
        protected static Dictionary<string,SpawnHandlerData> spawnHandlerData = new Dictionary<string, SpawnHandlerData>();

        /// <summary>
        /// Adds new spawn handler, or overwrites it if the supplied id already exists.
        /// </summary>
        /// <param name="id">Identifier.</param>
        /// <param name="data">Data.</param>
        public virtual void AddSpawnHandler(string id, SpawnHandlerData data)
        {
            if(spawnHandlerData.ContainsKey(id))
            {
                spawnHandlerData[id] = data;
            }
            else
            {
                spawnHandlerData.Add(id, data);
            }

            #if !SERVER
            RegisterSpawnHandlerOnClient(data);
            #endif
        }

        /// <summary>
        /// Removes the spawn handler.
        /// </summary>
        /// <param name="id">Identifier.</param>
        public void RemoveSpawnHandler(string id)
        {
            #if !SERVER
            UnregisterSpawnHandlerOnClient(GetSpawnHandlerData(id));
            #endif

            if(spawnHandlerData.ContainsKey(id)) spawnHandlerData.Remove(id);
        }

        /// <summary>
        /// Gets the spawn handler data.
        /// </summary>
        /// <returns>The spawn handler data.</returns>
        /// <param name="id">Identifier.</param>
        public virtual SpawnHandlerData GetSpawnHandlerData(string id)
        {
            if(spawnHandlerData.ContainsKey(id)) return spawnHandlerData[id];
            else return null;
        }

        /// <summary>
        /// Container for sending data to server when calling ClientScene.AddPlayer(...)
        /// </summary>
        public class AddPlayerExtraMessage : MessageBase{
            /// <summary>
            /// The spawn handler to use when instantiating the prefabs on server and client
            /// </summary>
            public string spawnHandlerID;
        }

        /// <summary>
        /// Sets a message handler.
        /// </summary>
        /// <param name="msgType">Message type.</param>
        /// <param name="handler">Handler.</param>
        public virtual void SetMessageHandler(short msgType, NetworkMessageDelegate handler ){
            #if SERVER
            NetworkServer.
            #else
            client.
            #endif
            RegisterHandler(msgType, handler);
        }

        /// <summary>
        /// Removes a message handler.
        /// </summary>
        /// <param name="msgType">Message type.</param>
        public virtual void RemoveMessageHandler(short msgType){
            #if SERVER
            NetworkServer.
            #else
            client.
            #endif
            UnregisterHandler(msgType);
        }

        // ---------------------------------- NETWORK STATE & EVENTS ----------------------------------------------
        Events.ConnectingToServerEvent connectingToServerEvent = new Events.ConnectingToServerEvent();
        Events.ConnectedToServerEvent connectedToServerEvent = new Events.ConnectedToServerEvent();
        Events.DisconnectedFromServerEvent disconnectedFromServerEvent = new Events.DisconnectedFromServerEvent();

        /// <summary>
        /// Called when client starts connecting to a server
        /// </summary>
        /// <param name="client">Client.</param>
        public override void OnStartClient(NetworkClient client)
        {
            base.OnStartClient(client);

            NetworkState = NetworkStateEnum.Connecting;

            connectingToServerEvent.address = networkAddress;
            connectingToServerEvent.port = networkPort;
            _eventService.Publish(connectingToServerEvent);
        }

        /// <summary>
        /// Called when client has connected to a server
        /// </summary>
        /// <param name="conn">Conn.</param>
        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            NetworkState = NetworkStateEnum.Connected;

            connectedToServerEvent.networkConnection = conn;
            _eventService.Publish(connectedToServerEvent);

            //Debug.Log("Connected to "+conn.address);
        }

        /// <summary>
        /// Called when client is stopped
        /// </summary>
        public override void OnStopClient ()
        {
            base.OnStopClient ();

            NetworkState = NetworkStateEnum.Disconnected;

            _eventService.Publish(disconnectedFromServerEvent);
        }

        /// <summary>
        /// Called when client has disconnected from a server
        /// </summary>
        /// <param name="conn">Conn.</param>
        public override void OnClientDisconnect (NetworkConnection conn)
        {
            base.OnClientDisconnect (conn);

            NetworkState = NetworkStateEnum.Disconnected;

            _eventService.Publish(disconnectedFromServerEvent);
        }

        /// <summary>
        /// Called when there was an error connecting to server
        /// </summary>
        /// <param name="conn">Conn.</param>
        /// <param name="errorCode">Error code.</param>
        public override void OnClientError (NetworkConnection conn, int errorCode)
        {
            base.OnClientError (conn, errorCode);

            NetworkState = NetworkStateEnum.Disconnected;

            _eventService.Publish(disconnectedFromServerEvent);
        }

         // SERVER -----------------------------------
        Events.ClientConnectedToServerEvent clientConnectedToServerEvent = new Events.ClientConnectedToServerEvent();
        Events.ClientDisconnectedFromServerEvent clientDisconnectedFromServerEvent = new Events.ClientDisconnectedFromServerEvent();
        Events.ServerStartedEvent serverStartedEvent = new Events.ServerStartedEvent();
        Events.ServerStoppedEvent serverStoppedEvent = new Events.ServerStoppedEvent();

        /// <summary>
        /// Called when a new client connected to the server
        /// </summary>
        /// <param name="conn">Conn.</param>
        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);

            //Publish event
            clientConnectedToServerEvent.networkConnection = conn;
            _eventService.Publish(clientConnectedToServerEvent);
        }

        /// <summary>
        /// Called when a client disconnected from the server
        /// </summary>
        /// <param name="conn">Conn.</param>
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            base.OnServerDisconnect(conn);

            //Publish event
            clientDisconnectedFromServerEvent.networkConnection = conn;
            _eventService.Publish(clientDisconnectedFromServerEvent);
        }

        /// <summary>
        /// Called when the server starts
        /// </summary>
        public override void OnStartServer()
        {
            base.OnStartServer();

            Observable.NextFrame().Subscribe(e => _eventService.Publish(serverStartedEvent));
        }

        /// <summary>
        /// Called when the server stops
        /// </summary>
        public override void OnStopServer()
        {
            base.OnStopServer();

            _eventService.Publish(serverStoppedEvent);
        }

        public virtual void Dispose(){
            if(_dManager != null) _dManager.Remove(this);
            StopTicking();
        }
    }
}
*/