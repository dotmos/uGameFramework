using Service.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using Zenject;

namespace Service.GameStateService
{
    public class GameState {

        public CompositeDisposable gamestateDisposable = new CompositeDisposable();

        public GSStatus currentState = GSStatus.noneStatus;

        /// <summary>
        /// PriorityList to be called when this gamestate is started
        /// </summary>
        public PriorityList OnEnter = new PriorityList();
        /*protected PriorityList OnPause = new PriorityList();
        protected PriorityList OnResume = new PriorityList();*/
        /// <summary>
        /// PriorityList to be called when this gamestate is left
        /// </summary>
        public PriorityList OnExit = new PriorityList();
        protected PriorityList OnTick = new PriorityList();

        public string Name { get; private set; }

        private GSContext currentContext = null;

        public GSContext Context { get { return currentContext; } }

        [Inject]
        IEventsService eventsService;

        public GameState(string name) {
            this.Name = name;
        }

        /// <summary>
        /// Called when gamestate is activated. Returns IObservable<bool>. Gamestate is marked as 'starting' until the OnEnter-Observable emits a true-element (and then is in running-state)
        /// Override this method with gamestate startup-logic (if needed)
        /// </summary>
        /// <returns></returns>
        public IObservable<bool> DoOnEnter(GSContext ctx=null) {
            this.currentContext = ctx;

            eventsService.Publish(new Events.GameStateStatusChange() {
                gameState = this,
                fromStatus = currentState,
                toStatus = GSStatus.starting
            });
            currentState = GSStatus.starting;

            // if not overriden, exit immediately
            return OnEnter.RxExecute().Finally(()=> {
                eventsService.Publish(new Events.GameStateStatusChange() {
                    gameState = this,
                    fromStatus = currentState,
                    toStatus = GSStatus.starting
                });
            });
        }

        /// <summary>
        /// Called every tick, if gamestate is in running-state. Add additional gamestate logic here.
        /// </summary>
        /// <param name="tick"></param>
        public virtual void DoOnTick(float tick) {
            //TODO
        }

        /// <summary>
        /// Called when the gamestate is deactivated. Returns IObservable<bool>. Gamestate is marked 'closing' until this OnExit-Observable emits true. 
        /// This gives you time to properly end/stop the GameState before the next gamestate can be activated
        /// </summary>
        /// <returns></returns>
        public virtual IObservable<bool> DoOnExit() {
            eventsService.Publish(new Events.GameStateStatusChange() {
                gameState = this,
                fromStatus = currentState,
                toStatus = GSStatus.closing
            });
            currentState = GSStatus.closing;

            // if not overriden, exit immediately
            return OnExit.RxExecute().Finally(() => {
                // clear all disposables connected to this gamestate
                gamestateDisposable.Clear();

                eventsService.Publish(new Events.GameStateStatusChange() {
                    gameState = this,
                    fromStatus = currentState,
                    toStatus = GSStatus.noneStatus
                });
            });
        }
    }
}
