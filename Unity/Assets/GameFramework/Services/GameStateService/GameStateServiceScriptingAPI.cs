using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;

namespace Service.GameStateService {



    partial class GameStateServiceImpl : GameStateServiceBase
    {
        protected override void InitAPI() {
            ActivateDefaultScripting("GameStateService");
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
