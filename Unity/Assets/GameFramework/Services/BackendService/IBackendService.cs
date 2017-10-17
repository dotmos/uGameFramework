using System.Collections.Generic;

namespace Service.Backend{
    public interface IBackendService{
        string UserID{get;}

        /// <summary>
        /// Login to backend
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        void Login(string username, string password);

        /// <summary>
        /// Logout current user from backend
        /// </summary>
        void Logout();

        /// <summary>
        /// Register a new user.
        /// </summary>
        /// <param name="username">Username.</param>
        /// <param name="password">Password.</param>
        /// <param name="email">Email.</param>
        void RegisterNewUser(string username, string password, string email);

        /// <summary>
        /// Get user specific data from backend database
        /// </summary>
        /// <param name="userID">User I.</param>
        /// <param name="fields">Fields.</param>
        void GetUserData(string userID, List<string> fields);

        /// <summary>
        /// Updates the user data.
        /// </summary>
        /// <param name="userID">User I.</param>
        /// <param name="data">Data.</param>
        void UpdateUserData(string userID, Dictionary<string, string> data);

        /// <summary>
        /// Get game data from backend database
        /// </summary>
        /// <param name="fields">Fields.</param>
        void GetGameData(List<string> fields);

        /// <summary>
        /// Get download url for an asset from backend
        /// </summary>
        /// <param name="assetKey">Asset key.</param>
        void GetDownloadUrl(string assetKey);

        /// <summary>
        /// Request data for a multiplayer match from the backend
        /// </summary>
        /// <param name="gameMode">Game mode.</param>
        /// <param name="buildVersion">Build version.</param>
        /// <param name="region">Region.</param>
        void RequestMatch(string gameMode, string buildVersion, string region);

        /// <summary>
        /// Redeems the match. Call this on the server when client was allowed to join.
        /// </summary>
        /// <param name="lobbyID">Lobby ID</param>
        /// <param name="ticket">Ticket.</param>
        void RedeemMatch(string lobbyID, string ticket);

        /// <summary>
        /// Player left the match. Call this on server when client left a game
        /// </summary>
        /// <param name="lobbyID">Lobby I.</param>
        /// <param name="playerID">Player I.</param>
        void PlayerLeftMatch(string lobbyID, string playerID);

        /// <summary>
        /// Retrive an item catalog from the backend
        /// </summary>
        /// <param name="catalogID">Catalog I.</param>
        void GetItemCatalog(string catalogID);
    }

    /// <summary>
    /// Catalog item.
    /// </summary>
    public class CatalogItem{
        public string ID;
        public string displayName;
        public string description;
        public string itemClass;
        public string iconPath;
        public string assetPath;
        public Dictionary<string, uint> prices;
    }
}