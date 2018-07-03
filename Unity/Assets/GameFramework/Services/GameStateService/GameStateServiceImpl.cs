using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.GameStateService {
    public class GameStateServiceImpl : GameStateServiceBase {

        class API
        {
            GameStateServiceImpl instance;

            public API(GameStateServiceImpl instance) {
                this.instance = instance;
            }

            public List<string> GetGamestates() {
                return new List<string>(instance.gameStates.Keys);
            }

            public void StartGamestate(string name) {
                try {
                    var gs = instance.gameStates[name];
                    instance.StartGameState(gs).Subscribe(_=> { }).AddTo(gs.gamestateDisposable);
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

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

            Kernel.Instance.Container.Bind<Service.GameStateService.GameState>().FromMethod(() => {
                return CurrentGameState;
            });

            try {
                var cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
                Publish(cmdGetScript);
                cmdGetScript.result.Globals["GS"] = new API(this);
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("Error activating default scripting for Service.GameStateService with lua-name: GS");
                UnityEngine.Debug.LogException(e);
            }
        }




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

        public override IObservable<bool> StartGameState(GameState gamestate, GSContext ctx = null) {
            var oldGameState = CurrentGameState;
            CurrentGameState = null;

            List<IObservable<bool>> startupSequence = new List<IObservable<bool>>();
            if (oldGameState != null) {
                // if there is a gamestate already active, first make sure it's onExit-observable is finished
                startupSequence.Add(oldGameState.DoOnExit());
            }
            startupSequence.Add(gamestate.DoOnEnter());

            return Observable.Concat(startupSequence).Do(_ => {
                CurrentGameState = gamestate;
            });
        }
    }
}
