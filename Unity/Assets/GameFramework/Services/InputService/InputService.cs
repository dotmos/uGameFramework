using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using UniRx;
using Zenject;

namespace Service.Input{
    public class InputService : IInputService, IDisposable {

        Dictionary<object, List<Func<bool>>> buttonDownHandlers = new Dictionary<object, List<Func<bool>>>();
        Dictionary<object, List<Func<bool>>> buttonUpHandlers = new Dictionary<object, List<Func<bool>>>();
        Dictionary<object, List<Func<bool>>> buttonHoldHandlers = new Dictionary<object, List<Func<bool>>>();

        Dictionary<object, List<Func<float>>> axisHandlers = new Dictionary<object, List<Func<float>>>();

        DisposableManager _dManager;
        CompositeDisposable disposables = new CompositeDisposable();
        bool inputEnabled = true;

        Service.Events.IEventsService _eventService;

        [Inject]
        void Initialize(
            [Inject] DisposableManager dManager,
            [Inject] Service.Events.IEventsService eventService
        ){
            _dManager = dManager;
            _eventService = eventService;

            _dManager.Add(this);

            //Fire events on button up/down
            disposables.Add( Observable.EveryUpdate().Subscribe(e => {
                if (inputEnabled) {
                    //Check button down
                    foreach (KeyValuePair<object, List<Func<bool>>> kv in buttonDownHandlers) {
                        if (GetButtonDown(kv.Key)) _eventService.Publish(new Events.ButtonDownEvent() { button = kv.Key });
                    }
                    //Check button up
                    foreach (KeyValuePair<object, List<Func<bool>>> kv in buttonUpHandlers) {
                        if (GetButtonUp(kv.Key)) _eventService.Publish(new Events.ButtonUpEvent() { button = kv.Key });
                    }
                }
            }) );
        }

        /// <summary>
        /// Enable/Disable input
        /// </summary>
        /// <param name="enable"></param>
        public void EnableInput(bool enable) {
            inputEnabled = enable;
        }

        /// <summary>
        /// Registers a new button. If the input already exists, an additional handler will be added, resulting in the input being set by multiple handlers
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="downHandler">Down handler.</param>
        /// <param name="upHandler">Up handler.</param>
        /// <param name="holdHandler">Hold handler.</param>
        public void RegisterButton(object input, Func<bool> downHandler, Func<bool> upHandler, Func<bool> holdHandler){

            //Register down handler
            if(downHandler != null && buttonDownHandlers.ContainsKey(input)) {
                //If handler does not already exist
                if(!buttonDownHandlers[input].Contains(downHandler)) buttonDownHandlers[input].Add(downHandler);
            }
            else if(downHandler != null){
                buttonDownHandlers.Add(input, new List<Func<bool>>());
                buttonDownHandlers[input].Add(downHandler);
            }

            //Register up handler
            if(upHandler != null && buttonUpHandlers.ContainsKey(input)) {
                //If handler does not already exist
                if(!buttonUpHandlers[input].Contains(upHandler)) buttonUpHandlers[input].Add(upHandler);
            }
            else if(upHandler != null){
                buttonUpHandlers.Add(input, new List<Func<bool>>());
                buttonUpHandlers[input].Add(upHandler);
            }

            //Register hold handler
            if(holdHandler != null && buttonHoldHandlers.ContainsKey(input)) {
                //If handler does not already exist
                if(!buttonHoldHandlers[input].Contains(holdHandler)) buttonHoldHandlers[input].Add(holdHandler);
            }
            else if(holdHandler != null){
                buttonHoldHandlers.Add(input, new List<Func<bool>>());
                buttonHoldHandlers[input].Add(holdHandler);
            }
        }

        /// <summary>
        /// Removes the button.
        /// </summary>
        /// <param name="input">Input.</param>
        public void RemoveButton(object input){
            if(buttonDownHandlers.ContainsKey(input)){
                buttonDownHandlers[input].Clear();
                buttonDownHandlers.Remove(input);
            }
            if(buttonUpHandlers.ContainsKey(input)){
                buttonUpHandlers[input].Clear();
                buttonUpHandlers.Remove(input);
            }
            if(buttonHoldHandlers.ContainsKey(input)){
                buttonHoldHandlers[input].Clear();
                buttonHoldHandlers.Remove(input);
            }
        }

        /// <summary>
        /// Registers a new axis. If the axis already exists, an additional handler will be added, resulting in the input being set by multiple handlers
        /// </summary>
        /// <param name="input">Input.</param>
        /// <param name="axisHandler">Axis handler.</param>
        public void RegisterAxis(object input, Func<float> axisHandler){
            //Register acis
            if(axisHandler != null && axisHandlers.ContainsKey(input)) {
                //If handler does not already exist
                if(!axisHandlers[input].Contains(axisHandler)) axisHandlers[input].Add(axisHandler);
            }
            else if(axisHandler != null){
                axisHandlers.Add(input, new List<Func<float>>());
                axisHandlers[input].Add(axisHandler);
            }
        }

        /// <summary>
        /// Removes the axis.
        /// </summary>
        /// <param name="input">Input.</param>
        public void RemoveAxis(object input){
            if(axisHandlers.ContainsKey(input)){
                axisHandlers[input].Clear();
                axisHandlers.Remove(input);
            }
        }

        /// <summary>
        /// Return true if the input has just been pressed down in this frame
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="input">Input.</param>
        public bool GetButtonDown(object input){
            if (!inputEnabled) return false;

            bool result = false;

            if(input != null && buttonDownHandlers.ContainsKey(input))
            {
                List<Func<bool>> handlers = buttonDownHandlers[input];

                for(int i=0; i<handlers.Count; ++i)
                {
                    if(handlers[i]() == true){
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the input has just been released in this frame
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="input">Input.</param>
        public bool GetButtonUp(object input){
            if (!inputEnabled) return false;

            bool result = false;

            if(input != null && buttonUpHandlers.ContainsKey(input))
            {
                List<Func<bool>> handlers = buttonUpHandlers[input];

                for(int i=0; i<handlers.Count; ++i)
                {
                    if(handlers[i]() == true){
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns true if the input is being hold down
        /// </summary>
        /// <returns>true</returns>
        /// <c>false</c>
        /// <param name="input">Input.</param>
        public bool GetButton(object input){
            if (!inputEnabled) return false;

            bool result = false;

            if(input != null && buttonHoldHandlers.ContainsKey(input))
            {
                List<Func<bool>> handlers = buttonHoldHandlers[input];

                for(int i=0; i<handlers.Count; ++i)
                {
                    if(handlers[i]() == true){
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }
        /// <summary>
        /// Returns the current value of the supplied axis
        /// </summary>
        /// <returns>The axis.</returns>
        /// <param name="input">Input.</param>
        public float GetAxis(object input){
            if (!inputEnabled) return 0;

            float result = 0;

            if(input != null && axisHandlers.ContainsKey(input))
            {
                List<Func<float>> handlers = axisHandlers[input];

                float tempValue = 0;

                //Return biggest axis value
                for(int i=0; i<handlers.Count; ++i)
                {
                    tempValue = handlers[i]();
                    if(Mathf.Abs(tempValue) > result) result = tempValue;
                }
            }

            return result;
        }



        bool bDisposed = false;
        public void Dispose(){
            if(bDisposed) return;
            bDisposed = true;

            disposables.Dispose();
            _dManager.Remove(this);
        }
    }
}
