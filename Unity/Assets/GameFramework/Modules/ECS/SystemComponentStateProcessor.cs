namespace ECS {

    /// <summary>
    /// Simple state based systemcomponents processor
    /// </summary>
    /// <typeparam name="TSystemComponents"></typeparam>
    /// <typeparam name="TStates"></typeparam>
    public abstract class SystemComponentStateProcessor<TSystemComponents, TStates> where TSystemComponents : ISystemComponents {

        public SystemComponentStateProcessor() {
            Kernel.Instance.Inject(this);
        }

        protected abstract void OnEnter(TSystemComponents components, TStates oldState, TStates newState, float deltaTime);

        protected abstract void OnExit(TSystemComponents components, TStates oldState, TStates newState, float deltaTime);

        protected abstract TStates OnProcess(TSystemComponents components, TStates state, float deltaTime);

        public void Process(TSystemComponents components, TStates state, float deltaTime) {
            TStates newState = OnProcess(components, state, deltaTime);
            if (!state.Equals(newState)) {
                OnExit(components, state, newState, deltaTime);
                OnEnter(components, state, newState, deltaTime);
            }
        }
    }
}