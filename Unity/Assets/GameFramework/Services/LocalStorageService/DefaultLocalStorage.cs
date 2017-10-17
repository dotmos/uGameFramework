using UnityEngine;
using System.Collections;

namespace Service.LocalStorage{
    public class DefaultLocalStorage : ILocalStorageService {

        /// <summary>
        /// Sets the string.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <param name="userID">User I.</param>
        public void SetString(string key, string value, string userID = default(string)){
            PlayerPrefs.SetString(userID+key, value);
        }

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <returns>The string.</returns>
        /// <param name="key">Key.</param>
        /// <param name="userID">User I.</param>
        public string GetString(string key, string userID = default(string))
        {
            return PlayerPrefs.GetString(userID+key);
        }
    }
}
