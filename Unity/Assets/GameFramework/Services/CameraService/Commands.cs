using Zenject;
using UniRx;
using System;
using Service.Events;
using System.Collections.Generic;
using UnityEngine;

namespace Service.Camera{
    public class Commands : CommandsBase {

        ICameraService _cameraService;

        [Inject]
        void Initialize(
            [Inject] ICameraService cameraService
        ){
            _cameraService = cameraService;

            this.OnEvent<SetCameraCommand>().Subscribe(e => SetCameraCommandHandler(e)).AddTo(this);
            this.OnEvent<SetPositionCommand>().Subscribe(e => SetPositionCommandHandler(e)).AddTo(this);
            this.OnEvent<SetDirectionCommand>().Subscribe(e => SetDirectionCommandHandler(e)).AddTo(this);
            this.OnEvent<SetTargetCommand>().Subscribe(e => SetTargetCommandHandler(e)).AddTo(this);
            this.OnEvent<SetOffsetCommand>().Subscribe(e => SetOffsetCommandHandler(e)).AddTo(this);
            this.OnEvent<SetDampingCommand>().Subscribe(e => SetDampingCommandHandler(e)).AddTo(this);
        }

        /// <summary>
        /// Set camera command.
        /// </summary>
        public class SetCameraCommand{
            public UnityEngine.Camera camera;
        }
        /// <summary>
        /// Sets the camera command handler.
        /// </summary>
        /// <param name="cmd">Cmd.</param>
        protected void SetCameraCommandHandler(SetCameraCommand cmd){
            _cameraService.SetCamera(cmd.camera);
        }

        /// <summary>
        /// Set position command.
        /// </summary>
        public class SetPositionCommand{
            public Vector3 position;
        }
        /// <summary>
        /// Sets the position command handler.
        /// </summary>
        /// <param name="cmd">Cmd.</param>
        protected void SetPositionCommandHandler(SetPositionCommand cmd){
            _cameraService.SetPosition(cmd.position);
        }

        /// <summary>
        /// Set direction command.
        /// </summary>
        public class SetDirectionCommand{
            public Vector3 direction;
        }
        /// <summary>
        /// Sets the direction command handler.
        /// </summary>
        /// <param name="cmd">Cmd.</param>
        protected void SetDirectionCommandHandler(SetDirectionCommand cmd){
            _cameraService.SetDirection(cmd.direction);
        }

        /// <summary>
        /// Set target command.
        /// </summary>
        public class SetTargetCommand{
            public Transform target;
        }
        /// <summary>
        /// Sets the target command handler.
        /// </summary>
        /// <param name="cmd">Cmd.</param>
        protected void SetTargetCommandHandler(SetTargetCommand cmd){
            _cameraService.SetTarget(cmd.target);
        }

        /// <summary>
        /// Set offset command.
        /// </summary>
        public class SetOffsetCommand{
            public Vector3 position = Vector3.zero;
            public Vector3 direction = Vector3.zero;
        }
        /// <summary>
        /// Sets the offset command handler.
        /// </summary>
        /// <param name="cmd">Cmd.</param>
        protected void SetOffsetCommandHandler(SetOffsetCommand cmd){
            _cameraService.PositionOffset = cmd.position;
            _cameraService.DirectionOffset = cmd.direction;
        }

        /// <summary>
        /// Set damping command.
        /// </summary>
        public class SetDampingCommand{
            public float position = 0;
            public float rotation = 0;
        }
        /// <summary>
        /// Sets the damping command handler.
        /// </summary>
        /// <param name="cmd">Cmd.</param>
        protected void SetDampingCommandHandler(SetDampingCommand cmd){
            _cameraService.PositionDamping = cmd.position;
            _cameraService.RotationDamping = cmd.rotation;
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
