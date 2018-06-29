using Zenject;
using UniRx;
using System;
using Service.Events;
using System.Collections.Generic;

namespace Service.Backend{
    public class Commands : CommandsBase {

        IBackendService _backendService;

        [Inject]
        void Initialize(
            [Inject] IBackendService backendService
        ){
            _backendService = backendService;

            this.OnEvent<LoginCommand>().Subscribe(e => LoginCommandHandler(e)).AddTo(this);
            this.OnEvent<LogoutCommand>().Subscribe(e => LogoutCommandHandler(e)).AddTo(this);
            this.OnEvent<RegisterNewUser>().Subscribe(e => RegisterNewUserHandler(e)).AddTo(this);
            this.OnEvent<GetUserDataCommand>().Subscribe(e => GetUserDataCommandHandler(e)).AddTo(this);
            this.OnEvent<UpdateUserDataCommand>().Subscribe(e => UpdateUserDataCommandHandler(e)).AddTo(this);
            this.OnEvent<GetGameDataCommand>().Subscribe(e => GetGameDataCommandHandler(e)).AddTo(this);
            this.OnEvent<GetDownloadUrlCommand>().Subscribe(e => GetDownloadUrlCommandHandler(e)).AddTo(this);
            this.OnEvent<RequestMatchCommand>().Subscribe(e => RequestMatchCommandHandler(e)).AddTo(this);
            this.OnEvent<RedeemMatchCommand>().Subscribe(e => RedeemMatchCommandHandler(e)).AddTo(this);
            this.OnEvent<PlayerLeftMatchCommand>().Subscribe(e => PlayerLeftMatchCommandHandler(e)).AddTo(this);
            this.OnEvent<GetItemCatalogCommand>().Subscribe(e => GetItemCatalogCommandHandler(e)).AddTo(this);
        }


        /// <summary>
        /// Login command.
        /// </summary>
        public class LoginCommand{
            public string Username{get;set;}
            public string Password{get;set;}
        }
        protected void LoginCommandHandler(LoginCommand cmd){
            _backendService.Login(cmd.Username, cmd.Password);
        }

        /// <summary>
        /// Logout command.
        /// </summary>
        public class LogoutCommand{
        }
        protected void LogoutCommandHandler(LogoutCommand cmd){
            _backendService.Logout();
        }

        /// <summary>
        /// Register new user.
        /// </summary>
        public class RegisterNewUser{
            public string Username{get;set;}
            public string Password{get;set;}
            public string Email{get;set;}
        }
        protected void RegisterNewUserHandler(RegisterNewUser cmd){
            _backendService.RegisterNewUser(cmd.Username, cmd.Password, cmd.Email);
        }

        /// <summary>
        /// Get user data command.
        /// </summary>
        public class GetUserDataCommand{
            public string UserID{get;set;}
            public List<string> Fields{get;set;}
        }
        protected void GetUserDataCommandHandler(GetUserDataCommand cmd){
            _backendService.GetUserData(cmd.UserID, cmd.Fields);
        }

        /// <summary>
        /// Update user data command.
        /// </summary>
        public class UpdateUserDataCommand{
            public string UserID{get;set;}
            public Dictionary<string, string> Data{get;set;}
        }
        protected void UpdateUserDataCommandHandler(UpdateUserDataCommand cmd){
            _backendService.UpdateUserData(cmd.UserID, cmd.Data);
        }

        /// <summary>
        /// Get game data command.
        /// </summary>
        public class GetGameDataCommand{
            public List<string> Fields{get;set;}
        }
        protected void GetGameDataCommandHandler(GetGameDataCommand cmd){
            _backendService.GetGameData(cmd.Fields);
        }

        /// <summary>
        /// Get download URL command.
        /// </summary>
        public class GetDownloadUrlCommand{
            public string AssetKey{get;set;}
        }
        protected void GetDownloadUrlCommandHandler(GetDownloadUrlCommand cmd){
            _backendService.GetDownloadUrl(cmd.AssetKey);
        }

        /// <summary>
        /// Request match command.
        /// </summary>
        public class RequestMatchCommand{
            public string GameMode{get;set;}
            public string BuildVersion{get;set;}
            public string Region{get;set;}
        }
        protected void RequestMatchCommandHandler(RequestMatchCommand cmd){
            _backendService.RequestMatch(cmd.GameMode, cmd.BuildVersion, cmd.Region);
        }

        /// <summary>
        /// Redeem match command.
        /// </summary>
        public class RedeemMatchCommand{
            public string Ticket{get;set;}
            public string LobbyID{get;set;}
        }
        protected void RedeemMatchCommandHandler(RedeemMatchCommand cmd){
            _backendService.RedeemMatch(cmd.LobbyID, cmd.Ticket);
        }

        /// <summary>
        /// Player left match command.
        /// </summary>
        public class PlayerLeftMatchCommand{
            public string lobbyID;
            public string playerID;
        }
        protected void PlayerLeftMatchCommandHandler( PlayerLeftMatchCommand cmd){
            _backendService.PlayerLeftMatch(cmd.lobbyID, cmd.playerID);
        }


        /// <summary>
        /// Get item catalog command.
        /// </summary>
        public class GetItemCatalogCommand{
            public string CatalogID{get;set;}
        }
        protected void GetItemCatalogCommandHandler(GetItemCatalogCommand cmd){
            _backendService.GetItemCatalog(cmd.CatalogID);
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
