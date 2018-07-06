
using System;
using System.Collections.Generic;
using UniRx;

namespace Service.GameStateService
{
    public partial class GameStateServiceImpl : GameStateServiceBase
    {
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
                    instance.StartGameState(gs).Subscribe(_ => { }).AddTo(gs.gamestateDisposable);
                }
                catch (Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }
    }
}