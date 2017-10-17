using System;

namespace Service.CloudStorage{
    public class CloudResult{
        public string key;
        public string userID;
        public string result;
        public string errorMessage;
        public bool HasError{
            get{ if(string.IsNullOrEmpty(errorMessage)) return false; else return true;}
        }

        public Action<CloudResult> resultHandler;
    }

    public interface ICloudStorageService  {
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
        /// <param name="key">Key.</param>
        /// <param name="resultHandler">Result handler.</param>
        /// <param name="userID">User I.</param>
        void GetString(string key, Action<CloudResult> resultHandler, string userID = default(string));
    }
}