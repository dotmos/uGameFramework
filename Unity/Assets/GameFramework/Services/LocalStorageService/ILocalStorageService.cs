namespace Service.LocalStorage{
    public interface ILocalStorageService  {
        /// <summary>
        /// Sets the string.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <param name="userID">User I.</param>
        void SetString(string key, string value, string userID = default(string));
       
        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="key">Key.</param>
        /// <param name="userID">User I.</param>
        string GetString(string key, string userID = default(string));
    }
}