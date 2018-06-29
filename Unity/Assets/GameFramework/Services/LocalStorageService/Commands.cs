using Zenject;
using UniRx;
using System;
using Service.Events;

namespace Service.LocalStorage{
    public class Commands : CommandsBase {

        ILocalStorageService _localStorageService;

        [Inject]
        void Initialize(
            [Inject] ILocalStorageService localStorageService
        ){
            _localStorageService = localStorageService;
 
            this.OnEvent<SetStringCommand>().Subscribe(e => SetStringCommandHandler(e)).AddTo(this);
            this.OnEvent<GetStringCommand>().Subscribe(e => GetStringCommandHandler(e)).AddTo(this);
        }

        /// <summary>
        /// Set string command.
        /// </summary>
        public class SetStringCommand{
            public string key;
            public string value;
            public string userID;
        }
        protected void SetStringCommandHandler(SetStringCommand cmd){
            _localStorageService.SetString(cmd.key, cmd.value, cmd.userID);
        }

        /// <summary>
        /// Get string command.
        /// </summary>
        public class GetStringCommand{
            public string key;
            public string userID;
            public string result;
        }
        protected void GetStringCommandHandler(GetStringCommand cmd){
            cmd.result = _localStorageService.GetString(cmd.key, cmd.userID);
        }
    }

    public class CommandsInstaller : Installer<CommandsInstaller>{
        public override void InstallBindings()
        {
            Commands cmds = Container.Instantiate<Commands>();
            // commented out due to zenject update (26.06.18)
            //            Container.BindAllInterfaces<Commands>().FromInstance(cmds);
        }
    }
}
