using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using Service.GameStateService;

using Zenject;
using UniRx;

namespace Service.TimeService {

    partial class TimeServiceImpl : TimeServiceBase, ITickable {

        /// <summary>
        /// Global tickables that run all the time. Game-logic should be added to the tick of the gamestate
        /// </summary>
        List<ITickable> globalTickables = new List<ITickable>();

        /// <summary>
        /// Holds all global timers that got executed the whole time independent of any gamestate
        /// </summary>
        private List<TimerElement> globalTimers = new List<TimerElement>();

        protected override void AfterInitialize() {
            Kernel.Instance.rxStartup.Add(() => {
                Observable.EveryUpdate().Subscribe(dt => {
                    for (int i = 0; i < globalTickables.Count; i++) {
                        globalTickables[i].Tick();
                    }
                }).AddTo(disposables);
            });

            globalTickables.Add(this);
        }

		
		/// <summary>
        /// Adds a timer in the global update-method and calls the callback n-times (or infinite till application end) 
        /// <param name="interval"></param>
        /// <param name="callback"></param>
        /// <param name="repeatTimes"></param>
        /// <param name="info"></param>
 /// </summary>
        

		public override TimerElement CreateGlobalTimer(float interval,Action callback,int repeatTimes,string info="") {
            var timer = new TimerElement() {
                info = info,
                timerCallback = callback,
                repeatTimes = repeatTimes,
                interval = interval,
                timeLeft = interval
            };

            globalTimers.Add(timer);
            return timer;
        }

		//docs come here
		public override void RemoveGlobalTimer(TimerElement timer) {
            globalTimers.Remove(timer);
        }
 

        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }


        private void ProcessTimers() {
            float dt = UnityEngine.Time.deltaTime;

            for (int i = globalTimers.Count-1; i >= 0; i--) {
                var timer = globalTimers[i];
                timer.timeLeft -= dt;
                if (timer.timeLeft <= 0) {
                    timer.timerCallback();
                    // if repeatTime is atm 1 and is about to change to 0, remove it.
                    // if repeatTimes is 0 => infinite looping
                    if (timer.repeatTimes == 1) {
                        globalTimers.RemoveAt(i);
                        continue;
                    } else if (timer.repeatTimes > 1) {
                        timer.repeatTimes--;
                    }
                    timer.timeLeft = timer.interval;

                }
            }
        }

        public void Tick() {
            // process the timers
            ProcessTimers();
        }
    }



}
