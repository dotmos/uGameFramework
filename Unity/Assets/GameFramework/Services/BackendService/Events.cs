using System.Collections.Generic;

namespace Service.Backend{
    public class Events{

        /// <summary>
        /// Base event for backend events
        /// </summary>
        public class BackendEventBase{
            public string errorMessage;
            public bool HasError {
                get{
                    if(string.IsNullOrEmpty(errorMessage)) return false;
                    else return true;
                }
            }
        }

        /// <summary>
        /// Logged in event.
        /// </summary>
        public class LoggedInEvent : BackendEventBase{
            public string userID;
            public string username;
            public string password;
        }

        /// <summary>
        /// Logged out event.
        /// </summary>
        public class LoggedOutEvent : BackendEventBase{
        }

        /// <summary>
        /// Registered new user event.
        /// </summary>
        public class RegisteredNewUserEvent : BackendEventBase{
            public string userID;
            public string username;
            public string email;
            public string password;
        }

        /// <summary>
        /// Got user data event.
        /// </summary>
        public class GotUserDataEvent : BackendEventBase{
            public string userID;
            public Dictionary<string, string> data;
        }

        /// <summary>
        /// User data updated event.
        /// </summary>
        public class UpdatedUserDataEvent : BackendEventBase{
            public string userID;
        }

        /// <summary>
        /// Got game data event.
        /// </summary>
        public class GotGameDataEvent : BackendEventBase{
            public List<string> fields;
            public Dictionary<string, string> data;
        }

        /// <summary>
        /// Got download URL event.
        /// </summary>
        public class GotDownloadUrlEvent : BackendEventBase{
            public string assetKey;
            public string url;
        }

        /// <summary>
        /// Requesting match event.
        /// </summary>
        public class RequestingMatchEvent : BackendEventBase{
            public string gameMode;
            public string buildVersion;
            public string region;
        }

        /// <summary>
        /// Requested match event.
        /// </summary>
        public class RequestedMatchEvent : BackendEventBase{
            public string gameMode;
            public string buildVersion;
            public string region;

            public string lobbyID;
            public string hostname;
            public int port;
            public string ticket;
        }

        /// <summary>
        /// Redeemed match event.
        /// </summary>
        public class RedeemedMatchEvent : BackendEventBase{
            public bool ticketIsValid = false;
            public string ticket;
            public string userID;
            public string lobbyID;
        }

        /// <summary>
        /// Got item catalog.
        /// </summary>
        public class GotItemCatalog : BackendEventBase{
            public string catalogID;
            public List<CatalogItem> items;
        }
    }
}