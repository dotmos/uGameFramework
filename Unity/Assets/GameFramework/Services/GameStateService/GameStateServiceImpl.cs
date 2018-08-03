using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using UnityEngine;

namespace Service.GameStateService {
    public partial class GameStateServiceImpl : GameStateServiceBase {

        private Dictionary<string, GameState> gameStates = new Dictionary<string, GameState>();
        public ReactiveProperty<GameState> CurrentGameStateProperty = new ReactiveProperty<GameState>((GameState)null);

        public GameState CurrentGameState {
            get { return CurrentGameStateProperty.Value; }
            private set { CurrentGameStateProperty.Value = value; }
        }

        public bool TransitionRunning {
            get { return TransitionRunningProperty.Value; }
        }

        public ReactiveProperty<bool> TransitionRunningProperty = new ReactiveProperty<bool>();

        protected override void AfterInitialize() {
            // dependency injection 
            Kernel.Instance.Container.Bind<Service.GameStateService.GameState>().FromMethod(() => {
                return CurrentGameState;
            });
        }
        /*
        [Inject]
        public void Test() {
            ReactivePriorityExecutionList pl = new ReactivePriorityExecutionList();

            pl.Add(() => { Debug.Log("4"); },Priorities.PRIORITY_LATE);
            pl.Add(() => { Debug.Log("5"); }, Priorities.PRIORITY_LATE);
            pl.Add(() => { Debug.Log("6"); }, Priorities.PRIORITY_LATE);
            pl.Add(() => { Debug.Log("7"); }, Priorities.PRIORITY_LATE);
            pl.Add(() => { Debug.Log("1"); });
            pl.Add(() => { Debug.Log("2"); });
            pl.Add(() => { Debug.Log("3"); });
            pl.RxExecute().Subscribe(evt => {
                Debug.Log("SUB");
            }, 
                e => { Debug.LogError("error:" + e); }, () => Debug.Log("COMPLETED"));
            int a = 0;
        }*/


        /*        [Inject]
                public void Test() {

                    var a = Observable.Return(1);

                    var b = a.Concat(Observable.Merge(Observable.Return(18), Observable.Return(95), Observable.Return(1895)));

                    b.Subscribe(val => {
                        UnityEngine.Debug.Log("OTU:" + val);
                    }); ;

                    // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
                    PriorityList pl = new PriorityList();

                    Func<int, IObservable<bool>> createIt = (nr) => {
                        return Observable.Create<bool>((observer) => { UnityEngine.Debug.Log(nr); observer.OnNext(true); return null; });
                    };

                    pl.QueueElement(createIt(1), 100);
                    pl.QueueElement(createIt(2), 100);
                    pl.QueueElement(createIt(3), 100);
                    pl.QueueElement(createIt(4), 100);
                    pl.QueueElement(createIt(5), 100);
                    pl.QueueElement(createIt(6), 1000);

                    pl.QueueElement(Observable.Interval(TimeSpan.FromSeconds(2)).Take(10)
                                    .Do(i => {
                                        UnityEngine.Debug.Log("WAIT:" + i);
                                    })
                                    .Last().Select(_ => true), 500);
                    pl.QueueElement(Observable.Interval(TimeSpan.FromSeconds(1)).Take(10)
                                    .Do(i => {
                                        UnityEngine.Debug.Log("WAITET:" + i);
                                    })
                                    .Last().Select(_ => true), 500);

                    pl.RxExecute().Subscribe(_ => {
                        UnityEngine.Debug.Log("DONE");
                    });

                } */


        public override GameState RegisterGameState(string name, GameState gamestate=null) {
            if (gameStates.ContainsKey("name")) {
                UnityEngine.Debug.LogError("GameState with name " + name + " already registered! Abort!");
                return null;
            }
            if (gamestate == null) {
                gamestate = new GameState(name);
                Kernel.Instance.Inject(gamestate);
            }
            gameStates[name] = gamestate;

            Kernel.Instance.Container.Bind<Service.GameStateService.GameState>().WithId(name).FromInstance(gamestate);

            return gamestate;
        }

        public override GameState GetCurrentGameState() {
            return CurrentGameState;            
        }



        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed
        }

        public override GameState GetGameState(string name) {
            if (gameStates.ContainsKey(name)) {
                return gameStates[name];
            }
            return null;
        }

        public override IObservable<bool> StartGameState(GameState gamestate, GSContext ctx = null) {
            var oldGameState = CurrentGameState;
            CurrentGameState = null;

            List<IObservable<bool>> startupSequence = new List<IObservable<bool>>();
            if (oldGameState != null) {
                // if there is a gamestate already active, first make sure it's onExit-observable is finished
                startupSequence.Add(oldGameState.DoOnExit());
            }
            startupSequence.Add(Observable.Return(true).Do(_=> {
                // make sure the new gamestate is set before OnEnter-calls are executed
                CurrentGameState = gamestate;
            }));
            startupSequence.Add(gamestate.DoOnEnter());

            return Observable.Concat(startupSequence).Last();
        }

        public override IObservable<bool> StopGameState(GameState gamestate) {
            return gamestate.DoOnExit().Finally( ()=> { CurrentGameState = null; });
        }
    }
}
