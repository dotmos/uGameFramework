using Zenject;
using UniRx;
using System;
using Service.Events;

namespace Service.CloudStorage{
    public class Commands : CommandsBase {

        ICloudStorageService _cloudStorageService;

        [Inject]
        void Initialize(
            [Inject] ICloudStorageService cloudStorageService
        ){
            _cloudStorageService = cloudStorageService;
 
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
            _cloudStorageService.SetString(cmd.key, cmd.value, cmd.userID);
        }

        /// <summary>
        /// Get string command.
        /// </summary>
        public class GetStringCommand{
            public string key;
            public string userID;
            public Action<CloudResult> resultHandler;
        }
        protected void GetStringCommandHandler(GetStringCommand cmd){
            _cloudStorageService.GetString(cmd.key, cmd.resultHandler, cmd.userID);
        }
    }

    public class CommandsInstaller : Installer<CommandsInstaller>{
        public override void InstallBindings()
        {
            Commands cmds = Container.Instantiate<Commands>();
//            Container.BindAllInterfaces<Commands>().FromInstance(cmds);
            Container.BindInterfacesTo<Commands>().FromInstance(cmds);

        }
    }
}
