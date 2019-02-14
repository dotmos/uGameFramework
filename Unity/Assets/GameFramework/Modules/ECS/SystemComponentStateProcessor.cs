using System;

namespace ECS {

    /// <summary>
    /// Simple state based systemcomponents processor
    /// </summary>
    /// <typeparam name="TSystemComponents"></typeparam>
    /// <typeparam name="TStates"></typeparam>
    public abstract class SystemComponentStateProcessor<TSystemComponents, TStates> where TSystemComponents : ISystemComponents {

        public delegate TStates GetState(TSystemComponents components);
        GetState getState;

        public delegate void SetState(TSystemComponents components, TStates state);
        SetState setState;

        public SystemComponentStateProcessor(GetState getState, SetState setState) {
            Kernel.Instance.Inject(this);

            this.getState = getState;
            this.setState = setState;
        }


        protected abstract void OnEnter(TSystemComponents components, TStates oldState, TStates newState, float deltaTime);

        protected abstract void OnExit(TSystemComponents components, TStates oldState, TStates newState, float deltaTime);

        protected abstract TStates OnProcess(TSystemComponents components, TStates state, float deltaTime);

        public void Process(TSystemComponents components, float deltaTime) {
            TStates state = getState(components);
            TStates newState = OnProcess(components, state, deltaTime);
            if (!state.Equals(newState)) {
                OnExit(components, state, newState, deltaTime);
                setState(components, newState);
                OnEnter(components, state, newState, deltaTime);
            }
        }
    }
}