using Zenject;
using UniRx;
using System;
using Service.Events;
using System.Collections.Generic;

namespace Service.Advertisement{
    public class Commands : CommandsBase {

        IAdvertisementService _advertisementService;

        [Inject]
        void Initialize(
            [Inject] IAdvertisementService advertisementService
        ){
            _advertisementService = advertisementService;

            this.OnEvent<ShowCommand>().Subscribe(e => ShowCommandHandler(e)).AddTo(this);       
            this.OnEvent<ShowRewardedCommand>().Subscribe(e => ShowRewardedCommandHandler(e)).AddTo(this);
        }

        /// <summary>
        /// Show interstitial command.
        /// </summary>
        public class ShowCommand{
        }
        /// <summary>
        /// Shows the interstitial command handler.
        /// </summary>
        /// <param name="cmd">Cmd.</param>
        protected void ShowCommandHandler(ShowCommand cmd){
            _advertisementService.Show();
        }


        /// <summary>
        /// Show rewarded command.
        /// </summary>
        public class ShowRewardedCommand{
        }
        /// <summary>
        /// Shows the rewarded command handler.
        /// </summary>
        /// <param name="cmd">Cmd.</param>
        protected void ShowRewardedCommandHandler(ShowRewardedCommand cmd){
            _advertisementService.ShowRewarded();
        }

    }

    public class CommandsInstaller : Installer<CommandsInstaller>{
        public override void InstallBindings()
        {
            Commands cmds = Container.Instantiate<Commands>();
            Container.BindAllInterfaces<Commands>().FromInstance(cmds);
        }
    }
}
