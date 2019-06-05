/*
using UnityEngine;
using System.Collections;
using UniRx;
using UnityEngine.Networking;

namespace Service.GlobalNetwork{

    public enum NetworkStateEnum{
        Disconnected,
        Connecting,
        Connected
    }

    public interface INetworkTickableObject{
        void NetworkTick(uint tick);

        /// <summary>
        /// The time in seconds between each network tick. Do not manually change this!
        /// </summary>
        /// <value>The network tick delta.</value>
        float NetworkTickDelta{get;set;}

        /// <summary>
        /// The current network tick. Do not manually change this!
        /// </summary>
        /// <value>The current network tick.</value>
        uint CurrentNetworkTick{get;set;}
    }

    public delegate void BytesReceivedDelegate(byte[] bytes);

    public interface IGlobalNetworkService {
        ReactiveProperty<NetworkStateEnum> NetworkStateProperty {get;}

        /// <summary>
        /// Gets the tickrate.
        /// </summary>
        /// <value>The tickrate.</value>
        int Tickrate{get;}

        /// <summary>
        /// Gets the current tick.
        /// </summary>
        /// <value>The current tick.</value>
        uint CurrentTick{get;}

        /// <summary>
        /// Sets the tickrate.
        /// </summary>
        /// <param name="tickrate">Tickrate.</param>
        void SetTickrate(int tickrate);

        /// <summary>
        /// Adds the tickable.
        /// </summary>
        /// <param name="tickable">Tickable.</param>
        void AddTickable(INetworkTickableObject tickable);
        /// <summary>
        /// Removes the tickable.
        /// </summary>
        /// <param name="tickable">Tickable.</param>
        void RemoveTickable(INetworkTickableObject tickable);

        /// <summary>
        /// Sets the bytes received handler.
        /// </summary>
        /// <param name="byteMessageID">Byte message I.</param>
        /// <param name="handler">Handler.</param>
        void SetBytesReceivedHandler(ushort byteMessageID, BytesReceivedDelegate handler);
    }





    /// <summary>
    /// Holds data for spawning different prefabs on client and server
    /// </summary>
    public class SpawnHandlerData{
        /// <summary>
        /// The server prefab. Both client and server need this. Also, the prefabs MUST have a NetworkIdentity attached!
        /// </summary>
        public GameObject serverPrefab;
        /// <summary>
        /// The client prefab. Only client needs this
        /// </summary>
        public GameObject clientPrefab;
        /// <summary>
        /// The client spawn delegate. Only client needs this
        /// </summary>
        public SpawnDelegate clientSpawnDelegate;
        /// <summary>
        /// The client un spawn delegate. Only client neeeds this
        /// </summary>
        public UnSpawnDelegate clientUnSpawnDelegate;
    }
}
*/