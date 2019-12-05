using Service.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

namespace Service.GameStateService
{
    public class GameState {

        private class TickEnvelope
        {
            /// <summary>
            /// Priority for this call in the gamestate's tick-list
            /// </summary>
            public int priority = Priorities.PRIORITY_DEFAULT;

            /// <summary>
            /// The execution domain of this call
            /// </summary>
            public ExecutionDomain executionDomain = ExecutionDomain.unknown;

            /// <summary>
            /// Amount of calls (sense?)
            /// </summary>
            public uint calls = 0;
            /// <summary>
            /// simple action call encapsulating the logic
            /// </summary>
            public Action action = null;
            /// <summary>
            /// bool function encapsulting the logic. returns false=>removed from execution list, return true=>stays in list
            /// </summary>
            public Func<bool> func = null;
        }

        protected ECS.IEntityManager entityManager;

        /// <summary>
        /// The gamestateDisposable that get emptied when
        /// </summary>
        public CompositeDisposable gamestateDisposable = new CompositeDisposable();

        /// <summary>
        /// The tick disposable
        /// </summary>
        public IDisposable tickDisposable = null;


        /// <summary>
        /// Whether or not the gamestate is allowed to have it's Tick() function called
        /// </summary>
        public bool AllowedToTick {
            get;
            set;
        }

        /// <summary>
        /// Get the systems execution wrapper for wrapping actions and function-blocks with special logic
        /// </summary>
        [Inject]
        public IExecutionWrapper executionWrapper;

        public ReactiveProperty<GSStatus> currentStateProperty = new ReactiveProperty<GSStatus>(GSStatus.noneStatus);
        public GSStatus CurrentState {
            get { return currentStateProperty.Value; }
            private set { currentStateProperty.Value = value; }
        }

        /// <summary>
        /// PriorityList to be called when this gamestate is started
        /// </summary>
        public ReactivePriorityExecutionList OnEnter = new ReactivePriorityExecutionList();
        /// <summary>
        /// PriorityList to be called when this gamestate is left
        /// </summary>
        public ReactivePriorityExecutionList OnExit = new ReactivePriorityExecutionList();

        /// <summary>
        /// Sorted to execute the gamestate-tick-loop
        /// </summary>
        private bool isTickListDirty = false;
        private List<TickEnvelope> OnTick = new List<TickEnvelope>();


        /// <summary>
        /// Gamestate Name
        /// </summary>
        public string GamestateName { get; private set; }

        private GSContext currentContext = null;

        public GSContext Context { get { return currentContext; } }

        // Event-Templates
        private Events.GameStateBeforeEnter evtBeforeEnter = new Events.GameStateBeforeEnter();
        private Events.GameStateAfterEnter evtAfterEnter = new Events.GameStateAfterEnter();
        private Events.GameStateBeforeTick evtBeforeTick = new Events.GameStateBeforeTick();
        private Events.GameStateAfterTick evtAfterTick = new Events.GameStateAfterTick();
        private Events.GameStateBeforeExit evtBeforeExit = new Events.GameStateBeforeExit();
        private Events.GameStateAfterExit evtAfterExit = new Events.GameStateAfterExit();

        [Inject]
        IEventsService _eventService;

        /// <summary>
        /// Adds an action to the gamestate's queue list and will be executed on the next tick sorted by its priority(default:Priorities.PRIORITY_DEFAULT=512)
        /// and will be removed from the executionlist afterwards
        /// </summary>
        /// <param name="act"></param>
        /// <param name="priority"></param>
        /// <param name="exeType"></param>
        public void AddTick(Action act, int priority=Priorities.PRIORITY_DEFAULT, ExecutionDomain exeType=ExecutionDomain.unknown) {
            TickEnvelope envelope = new TickEnvelope() {
                action = act,
                priority = priority,
                executionDomain = exeType
            };
            OnTick.Add(envelope);
            isTickListDirty = true;
        }

        /// <summary>
        /// Adds a function(returing bool) to the gamestate's queue list and will be executed on the next tick sorted by its priority(default:Priorities.PRIORITY_DEFAULT=512)
        /// as long as the function returns true the function stays in the execution-queue until it returns false
        /// </summary>
        /// <param name="func"></param>
        /// <param name="priority"></param>
        public void AddTick(Func<bool> func,int priority = Priorities.PRIORITY_DEFAULT, ExecutionDomain exeType = ExecutionDomain.unknown) {
            TickEnvelope envelope = new TickEnvelope() {
                func = func,
                priority = priority,
                executionDomain = exeType
            };
            OnTick.Add(envelope);
            isTickListDirty = true;
        }

        private List<TickEnvelope> removeList = new List<TickEnvelope>();

        /// <summary>
        /// Do the actual execution of all tick-actions sorted by their priority
        /// </summary>
        private void ExecuteTickList() {
            // CAUTION: this method is called every tick. so keep it as fast as possible


            if (isTickListDirty) {
                OnTick = OnTick.OrderBy(env => env.priority).ToList();
                isTickListDirty = false;
            }

            // using this kind of execution cause I think this is the fastest way
            for (int i = OnTick.Count - 1; i >= 0; i--) {
                TickEnvelope currentEnvelope = OnTick[i];
                if (currentEnvelope.action != null) {
                    // execute the action
                    currentEnvelope.action();
                    removeList.Add(currentEnvelope);
                }
                else if (currentEnvelope.func != null) {
                    bool keepOnExecuting = currentEnvelope.func();
                    if (!keepOnExecuting) {
                        removeList.Add(currentEnvelope);
                    } else {
                        currentEnvelope.calls++; // sensless stat!?!
                    }
                }
                else {
                    Debug.LogError("Tick-Envelope without logic... removing");
                    removeList.Add(currentEnvelope);
                }
            }

            for (int i = removeList.Count - 1; i >= 0; i = 0) {
                OnTick.Remove(removeList[i]);
            }
        }


        public GameState(string name) {
            Kernel.Instance.Inject(this);
            this.GamestateName = name;
            // set the gamestate for the corresponding events
            this.evtAfterEnter.gameState = this;
            this.evtAfterExit.gameState = this;
            this.evtAfterTick.gameState = this;
            this.evtBeforeEnter.gameState = this;
            this.evtBeforeExit.gameState = this;
            this.evtBeforeTick.gameState = this;
        }

        protected GSContext CreateDefaultContext() {
            return new GSContext();
        }

        /// <summary>
        /// Called when gamestate is activated. Returns IObservable<bool>. Gamestate is marked as 'starting' until the OnEnter-Observable emits a true-element (and then is in running-state)
        /// Override this method with gamestate startup-logic (if needed)
        /// </summary>
        /// <returns></returns>
        public IObservable<bool> DoOnEnter(GSContext ctx=null) {
            if (entityManager == null) {
                entityManager = Kernel.Instance.Container.Resolve<ECS.IEntityManager>();
            }

            this.currentContext = ctx==null ? CreateDefaultContext() : ctx;

            CurrentState = GSStatus.starting;

            // fire hook
            _eventService.Publish(evtBeforeEnter);

            // if not overriden, exit immediately
            return OnEnter.RxExecute().Finally(()=> {
                CurrentState = GSStatus.running;

                // fire hook
                _eventService.Publish(evtAfterEnter);

                AllowedToTick = true;

                // finally the gamestate is started
                tickDisposable = Observable.EveryUpdate()
                    .Subscribe(_ => {
                        // tell that the we start the tick. last chance to react
                        // TODO: performance? (i guess its ok, but 2 msgs per frame,...should be ok. yes?)
                        _eventService.Publish(evtBeforeTick);

                        ExecuteTickList();

                        // tell that the tick is finished
                        _eventService.Publish(evtAfterTick);
                    });

                gamestateDisposable.Add(tickDisposable); // add tick also to gamestate just to be sure the gamestate's tick gets disposed when the gamestate is left
            });
        }

        /// <summary>
        /// Called when the gamestate is deactivated. Returns IObservable<bool>. Gamestate is marked 'closing' until this OnExit-Observable emits true. 
        /// This gives you time to properly end/stop the GameState before the next gamestate can be activated
        /// </summary>
        /// <returns></returns>
        public virtual IObservable<bool> DoOnExit() {
            CurrentState = GSStatus.closing;

            _eventService.Publish(evtBeforeExit);

            // stop tick
            if (tickDisposable != null) {
                tickDisposable.Dispose();
                tickDisposable = null;
            }
            // clear the ticklist
            OnTick.Clear();

            AllowedToTick = false;

            // start the OnExit-Process
            return OnExit.RxExecute().Finally(() => {
                // clear all disposables connected to this gamestate
                gamestateDisposable.Clear();

                _eventService.Publish(evtAfterExit);

            });
        }

        public virtual void Tick(float deltaTime, float unscaledDeltaTime) {

        }
    }
}
