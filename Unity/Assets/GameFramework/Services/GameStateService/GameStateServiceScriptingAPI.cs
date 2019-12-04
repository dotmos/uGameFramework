using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.GameStateService {



    partial class GameStateServiceImpl : GameStateServiceBase
    {
        protected override void InitAPI() {
            Scripting.Commands.GetMainScriptCommand cmdGetScript = new Service.Scripting.Commands.GetMainScriptCommand();
            Publish(cmdGetScript);
            API api = new API(this);
            Kernel.Instance.Inject(api);
            cmdGetScript.result.Globals["GameStateService"] = api;
        }


        class API
        {
            GameStateServiceImpl instance;

            public API( GameStateServiceImpl instance) {
                this.instance = instance;
            }

            /* add here scripting for this service */
            public List<string> GetGamestates() {
                return new List<string>(instance.gameStates.Keys);
            }

            public void StartGamestate(string name) {
                try {
                    GameState gs = instance.gameStates[name];
                    instance.StartGameState(gs).Subscribe(_ => { }).AddTo(gs.gamestateDisposable);
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
            }

            public void StopGamestate(string name) {
                try {
                    GameState gs = instance.gameStates[name];
                    instance.StopGameState(gs).Subscribe(_ => { }).AddTo(gs.gamestateDisposable);
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }
    }

}
