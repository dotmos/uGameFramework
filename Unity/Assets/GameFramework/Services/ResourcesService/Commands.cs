using Zenject;
using UniRx;
using System;
using Service.Events;
using System.Collections.Generic;

namespace Service.Resources{
    public class Commands : CommandsBase {

        IResourcesService _resourcesService;

        [Inject]
        void Initialize(
            [Inject] IResourcesService resourcesService
        ){
            _resourcesService = resourcesService;

            this.OnEvent<PreloadResourcesCommand>().Subscribe(e => PreloadResourcesCommandHandler(e)).AddTo(this);
            this.OnEvent<UnloadResourcesCommand>().Subscribe(e => UnloadResourcesCommandHandler(e)).AddTo(this);
        }
            
        /// <summary>
        /// Preload resources command.
        /// </summary>
        public class PreloadResourcesCommand{
            public List<ResourcesData> resources;
        }
        protected void PreloadResourcesCommandHandler(PreloadResourcesCommand cmd){
            _resourcesService.PreloadResources(cmd.resources);
        }

        /// <summary>
        /// Unload resources command.
        /// </summary>
        public class UnloadResourcesCommand{
        }
        protected void UnloadResourcesCommandHandler(UnloadResourcesCommand cmd)
        {
            _resourcesService.UnloadResources();
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
