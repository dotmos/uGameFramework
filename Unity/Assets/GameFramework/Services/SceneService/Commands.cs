using Zenject;
using UniRx;
using System;
using Service.Events;
using UnityEngine;
using System.Collections.Generic;

namespace Service.Scene{
    public class Commands : CommandsBase {

        ISceneService _sceneService;

        [Inject]
        void Initialize(
            [Inject] ISceneService sceneService
        ){
            _sceneService = sceneService; 
 
            this.OnEvent<LoadSceneCommand>().Subscribe(e => LoadSceneCommandHandler(e)).AddTo(this);
            this.OnEvent<UnloadSceneCommand>().Subscribe(e => UnloadSceneCommandHandler(e)).AddTo(this);
            this.OnEvent<SetActiveSceneCommand>().Subscribe(e => SetActiveSceneCommandHandler(e)).AddTo(this);
            this.OnEvent<GetActiveSceneIDCommand>().Subscribe(e => GetActiveSceneIDCommandHandler(e)).AddTo(this);
            this.OnEvent<ActivateSceneCommand>().Subscribe(e => ActivateSceneCommandHandler(e)).AddTo(this);
            this.OnEvent<DeactivateSceneCommand>().Subscribe(e => DeactivateSceneCommandHandler(e)).AddTo(this);
            this.OnEvent<GetSceneRootObjectsCommand>().Subscribe(e => GetSceneRootObjectsCommandHandler(e)).AddTo(this);
        }

        /// <summary>
        /// Load scene command.
        /// </summary>
        public class LoadSceneCommand{
            public string sceneID;
            public bool additive = true;
            public bool asynchron = true;
            public bool makeActive = true;
        }
        protected void LoadSceneCommandHandler(LoadSceneCommand cmd){
            //Do not load scene if scene is already loaded and cmd.doNotLoadIfSceneAlreadyExists is set to true (default)
            //if(cmd.doNotLoadIfSceneAlreadyExists && _sceneService.IsSceneLoaded(cmd.sceneID)) return;

            if(cmd.asynchron) _sceneService.LoadAsync(cmd.sceneID, cmd.additive, cmd.makeActive);
            else _sceneService.Load(cmd.sceneID, cmd.additive, cmd.makeActive);
        }

        /// <summary>
        /// Unload scene command.
        /// </summary>
        public class UnloadSceneCommand{
            public string sceneID;
        }
        protected void UnloadSceneCommandHandler(UnloadSceneCommand cmd){
            _sceneService.Unload(cmd.sceneID);
        }

        /// <summary>
        /// Set active scene command.
        /// </summary>
        public class SetActiveSceneCommand{
            public string sceneID;
        }
        protected void SetActiveSceneCommandHandler(SetActiveSceneCommand cmd)
        {
            _sceneService.SetActiveScene(cmd.sceneID);
        }

        /// <summary>
        /// Get active scene identifier command.
        /// </summary>
        public class GetActiveSceneIDCommand{
            public string result;
        }
        protected void GetActiveSceneIDCommandHandler( GetActiveSceneIDCommand cmd){
            cmd.result = _sceneService.GetActiveSceneID();
        }

        /// <summary>
        /// Activate scene command.
        /// </summary>
        public class ActivateSceneCommand{
            public string sceneID;
        }
        protected void ActivateSceneCommandHandler( ActivateSceneCommand cmd){
            _sceneService.ActivateScene(cmd.sceneID);
        }

        /// <summary>
        /// Deactivate scene command.
        /// </summary>
        public class DeactivateSceneCommand{
            public string sceneID;
        }
        protected void DeactivateSceneCommandHandler( DeactivateSceneCommand cmd){
            _sceneService.DeactivateScene(cmd.sceneID);
        }

        /// <summary>
        /// Gets all root objects for the scene. result.Count will be 0 if scene is currently not loaded.
        /// </summary>
        public class GetSceneRootObjectsCommand {
            public string sceneID;
            public List<GameObject> result;
        }
        protected void GetSceneRootObjectsCommandHandler( GetSceneRootObjectsCommand cmd) {
            cmd.result = _sceneService.GetSceneRootObjects(cmd.sceneID);
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
