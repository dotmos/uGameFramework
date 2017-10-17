using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

namespace Service.GlobalNetwork{
    public class Events {

        /// <summary>
        /// A Client connected to server
        /// </summary>
        public class ClientConnectedToServerEvent{
            public NetworkConnection networkConnection;
        }

        /// <summary>
        /// A Client disconnected from the server
        /// </summary>
        public class ClientDisconnectedFromServerEvent{
            public NetworkConnection networkConnection;
        }

        /// <summary>
        /// Server started event.
        /// </summary>
        public class ServerStartedEvent{
        }

        /// <summary>
        /// Server stopped event.
        /// </summary>
        public class ServerStoppedEvent{
        }

        /// <summary>
        /// Connecting to server event.
        /// </summary>
        public class ConnectingToServerEvent{
            public string address;
            public int port;
        }

        /// <summary>
        /// Local client connected to server event.
        /// </summary>
        public class ConnectedToServerEvent{
            public NetworkConnection networkConnection;
        }

        /// <summary>
        /// Local client disconnected from server event.
        /// </summary>
        public class DisconnectedFromServerEvent{
        }
    }
}