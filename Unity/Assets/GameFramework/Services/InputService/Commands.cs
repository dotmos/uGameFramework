using Zenject;
using UniRx;
using System;
using Service.Events;
using System.Collections.Generic;

namespace Service.Input{
    public class Commands : CommandsBase {

        IInputService _inputService;

        [Inject]
        void Initialize(
            [Inject] IInputService inputService
        ){
            _inputService = inputService;

            this.OnEvent<RegisterButtonCommand>().Subscribe(e => RegisterButtonCommandHandler(e)).AddTo(this);
            this.OnEvent<RemoveButtonCommand>().Subscribe(e => RemoveButtonCommandHandler(e)).AddTo(this);
            this.OnEvent<RegisterAxisCommand>().Subscribe(e => RegisterAxisCommandHandler(e)).AddTo(this);
            this.OnEvent<RemoveAxisCommand>().Subscribe(e => RemoveAxisCommandHandler(e)).AddTo(this);
            this.OnEvent<GetButtonDownCommand>().Subscribe(e => GetButtonDownCommandHandler(e)).AddTo(this);
            this.OnEvent<GetButtonUpCommand>().Subscribe(e => GetButtonUpCommandHandler(e)).AddTo(this);
            this.OnEvent<GetButtonCommand>().Subscribe(e => GetButtonCommandHandler(e)).AddTo(this);
            this.OnEvent<GetAxisCommand>().Subscribe(e => GetAxisCommandHandler(e)).AddTo(this);
            this.OnEvent<EnableInputCommand>().Subscribe(e => EnableInputCommandHandler(e)).AddTo(this);
        }
            
        /// <summary>
        /// Register button command.
        /// </summary>
        public class RegisterButtonCommand{
            public object input;
            public Func<bool> downHandler;
            public Func<bool> upHandler;
            public Func<bool> holdHandler;
        }
        protected void RegisterButtonCommandHandler( RegisterButtonCommand cmd){
            _inputService.RegisterButton(cmd.input, cmd.downHandler, cmd.upHandler, cmd.holdHandler);
        }

        /// <summary>
        /// Remove button command.
        /// </summary>
        public class RemoveButtonCommand{
            public object input;
        }
        protected void RemoveButtonCommandHandler( RemoveButtonCommand cmd){
            _inputService.RemoveButton(cmd.input);
        }


        /// <summary>
        /// Register axis command.
        /// </summary>
        public class RegisterAxisCommand{
            public object input;
            /// <summary>
            /// The axis handler. Should return a value between -1 and 1
            /// </summary>
            public Func<float> axisHandler;
        }
        protected void RegisterAxisCommandHandler( RegisterAxisCommand cmd){
            _inputService.RegisterAxis(cmd.input, cmd.axisHandler);
        }

        /// <summary>
        /// Remove axis command.
        /// </summary>
        public class RemoveAxisCommand{
            public object input;
        }
        protected void RemoveAxisCommandHandler( RemoveAxisCommand cmd){
            _inputService.RemoveAxis(cmd.input);
        }

        /// <summary>
        /// Get button down command.
        /// </summary>
        public class GetButtonDownCommand{
            public object input;
            public bool result;
        }
        protected void GetButtonDownCommandHandler( GetButtonDownCommand cmd){
            cmd.result = _inputService.GetButtonDown(cmd.input);
        }

        /// <summary>
        /// Get button up command.
        /// </summary>
        public class GetButtonUpCommand{
            public object input;
            public bool result;
        }
        protected void GetButtonUpCommandHandler( GetButtonUpCommand cmd){
            cmd.result = _inputService.GetButtonUp(cmd.input);
        }

        /// <summary>
        /// Get button command.
        /// </summary>
        public class GetButtonCommand{
            public object input;
            public bool result;
        }
        protected void GetButtonCommandHandler( GetButtonCommand cmd){
            cmd.result = _inputService.GetButton(cmd.input);
        }

        /// <summary>
        /// Get axis command.
        /// </summary>
        public class GetAxisCommand{
            public object input;
            public float result;
        }
        protected void GetAxisCommandHandler( GetAxisCommand cmd){
            cmd.result = _inputService.GetAxis(cmd.input);
        }

        public class EnableInputCommand {
            public bool enable = true;
        }
        protected void EnableInputCommandHandler(EnableInputCommand cmd) {
            _inputService.EnableInput(cmd.enable);
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
