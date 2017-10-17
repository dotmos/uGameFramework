using Zenject;
using UniRx;
using System;
using Service.Events;
using System.Collections.Generic;

namespace Service.Menu{
    public class Commands : CommandsBase {

        IMenuService _menuService;

        [Inject]
        void Initialize(
            [Inject] IMenuService menuService
        ){
            _menuService = menuService;

            this.OnEvent<ShowMenuCommand>().Subscribe(e => ShowMenuCommandHandler(e)).AddTo(this);
            this.OnEvent<CloseMenuCommand>().Subscribe(e => CloseMenuCommandHandler(e)).AddTo(this);
            this.OnEvent<AddMenuCommand>().Subscribe(e => AddMenuCommandHandler(e)).AddTo(this);
            this.OnEvent<AddDoNotCloseMenuCommand>().Subscribe(e => AddDoNotCloseMenuCommandHandler(e)).AddTo(this);
            this.OnEvent<RemoveDoNotCloseMenuCommand>().Subscribe(e => RemoveDoNotCloseMenuCommandHandler(e)).AddTo(this);
        }

        /// <summary>
        /// Show menu command.
        /// </summary>
        public class ShowMenuCommand{
            public Type menuType;
            public bool closeOtherMenus = true;
            public List<Type> doNotClose = null;
        }
        protected void ShowMenuCommandHandler(ShowMenuCommand cmd)
        {
            if(cmd.menuType == null) return;

            _menuService.ShowMenu(cmd.menuType, cmd.closeOtherMenus, cmd.doNotClose);
        }

        /// <summary>
        /// Close menu command.
        /// </summary>
        public class CloseMenuCommand{
            public Type menuType = null;
        }
        protected void CloseMenuCommandHandler(CloseMenuCommand cmd)
        {
            if(cmd.menuType == null) _menuService.CloseMenu();
            else _menuService.CloseMenu(cmd.menuType);
        }

        /// <summary>
        /// Add menu command.
        /// </summary>
        public class AddMenuCommand{
            public IMenuScreen menuScreen = null;
        }
        protected void AddMenuCommandHandler(AddMenuCommand cmd)
        {
            if(cmd.menuScreen == null) return;
            _menuService.AddMenu(cmd.menuScreen);
        }

        /// <summary>
        /// Add do not close menu command.
        /// </summary>
        public class AddDoNotCloseMenuCommand{
            public Type menuType;
        }
        protected void AddDoNotCloseMenuCommandHandler(AddDoNotCloseMenuCommand cmd){
            _menuService.AddDontCloseMenu(cmd.menuType);
        }

        /// <summary>
        /// Remove do not close menu command.
        /// </summary>
        public class RemoveDoNotCloseMenuCommand{
            public Type menuType;
        }
        protected void RemoveDoNotCloseMenuCommandHandler(RemoveDoNotCloseMenuCommand cmd){
            _menuService.RemoveDontCloseMenu(cmd.menuType);
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
