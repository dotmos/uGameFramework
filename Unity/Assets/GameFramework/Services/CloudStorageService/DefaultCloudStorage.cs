using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Zenject;
using UniRx;
using System;
using System.Linq;

namespace Service.CloudStorage{
    public class DefaultCloudStorage : ICloudStorageService, IDisposable{

        Service.Events.IEventsService _eventService;
        DisposableManager _dManager;

        CompositeDisposable disposables = new CompositeDisposable();

        List<CloudResult> cloudCalls = new List<CloudResult>();

        [Inject]
        void Initialize(
            [Inject] Service.Events.IEventsService eventService,
            [Inject] DisposableManager dManager
        ){
            _eventService = eventService;

            _dManager = dManager;
            _dManager.Add(this);

            disposables.Add(_eventService.OnEvent<Service.Backend.Events.GotUserDataEvent>().Subscribe(e => OnGotUserData(e)));
        }

        /// <summary>
        /// Sets the string.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value.</param>
        /// <param name="userID">User I.</param>
        public void SetString(string key, string value, string userID = default(string)){
            //Store data via backend service
            System.Collections.Generic.Dictionary<string, string> data = new System.Collections.Generic.Dictionary<string, string>();
            data.Add(key, value);

            Service.Backend.Commands.UpdateUserDataCommand cmd = new Service.Backend.Commands.UpdateUserDataCommand();
            cmd.Data = data;
            cmd.UserID = userID;
            _eventService.Publish(cmd);
        }

        /// <summary>
        /// Gets the string. Calls resultHandler once we got the data or when there was an error fetching the data
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="resultHandler">Result handler.</param>
        /// <param name="userID">User I.</param>
        public void GetString(string key, Action<CloudResult> resultHandler, string userID = default(string)){
            cloudCalls.Add(new CloudResult(){
                key = key,
                userID = userID,
                resultHandler = resultHandler
            });

            //Fetch data through backendservice
            Service.Backend.Commands.GetUserDataCommand cmd = new Service.Backend.Commands.GetUserDataCommand();
            cmd.Fields = new List<string>(){key};
            cmd.UserID = userID;
            _eventService.Publish(cmd);
        }


        void OnGotUserData(Service.Backend.Events.GotUserDataEvent evt){
            //Check if the userdata matches a request
            CloudResult result = cloudCalls.Find(e => e.userID == evt.userID && e.key == evt.data.Keys.ElementAtOrDefault(0));

            if(result != null)
            {
                //Check if there was an error (i.e. No Data available)
                if( evt.HasError ){
                    //Return error
                    result.errorMessage = evt.errorMessage;
                    _eventService.Publish(new Events.GotStringEvent(){result = result});
                    if(result.resultHandler != null) result.resultHandler(result);

                    cloudCalls.Remove(result);
                    return;
                }

                //Get data
                result.result = evt.data[result.key];
                //Call resultHandler
                _eventService.Publish(new Events.GotStringEvent(){result = result});
                if(result.resultHandler != null) result.resultHandler(result);

                cloudCalls.Remove(result);
            }
        }


        bool disposed = false;
        public void Dispose(){
            if(disposed) return;
            disposed = true;

            disposables.Dispose();
            _dManager.Remove(this);
        }
    }
}