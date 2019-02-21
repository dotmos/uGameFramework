using System;

namespace ECS {

    /// <summary>
    /// Simple state based systemcomponents processor
    /// </summary>
    /// <typeparam name="TSystemComponents"></typeparam>
    /// <typeparam name="TStates"></typeparam>
    public abstract class SystemComponentStateProcessor<TSystemComponents, TStates> where TSystemComponents : ISystemComponents where TStates : System.Enum {

        //public delegate TStates GetState(TSystemComponents components);
        //GetState getState;

        //public delegate void SetState(TSystemComponents components, TStates state);
        //SetState setState;

        public SystemComponentStateProcessor() {
            Kernel.Instance.Inject(this);

            //this.getState = getState;
            //this.setState = setState;
        }


        protected abstract void OnEnter(TSystemComponents components, TStates oldState, TStates newState, float deltaTime);

        protected abstract void OnExit(TSystemComponents components, TStates oldState, TStates newState, float deltaTime);

        protected abstract TStates OnProcess(TSystemComponents components, TStates state, float deltaTime);

        protected abstract TStates GetState(TSystemComponents components);
        protected abstract void SetState(TSystemComponents components, TStates newState);
        protected abstract bool StateEqual(TStates stateA, TStates stateB);

        public void Process(TSystemComponents components, float deltaTime) {
            TStates state = GetState(components);
            TStates newState = OnProcess(components, state, deltaTime);
            
            if (!StateEqual(state, newState)) {
                OnExit(components, state, newState, deltaTime);
                SetState(components, newState);
                OnEnter(components, state, newState, deltaTime);
            }
            
        }
    }
}