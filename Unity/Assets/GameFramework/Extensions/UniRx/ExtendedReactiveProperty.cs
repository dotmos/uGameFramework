namespace UniRx{
    public class ExtendedReactiveProperty<T> : ReactiveProperty<T>{

        public ExtendedReactiveProperty() : this(default(T))
        {
        }

        public ExtendedReactiveProperty(T initialValue)
        {
            LastValue = initialValue;
            SetValue(initialValue);
        }

        public ReactiveProperty<T> LastValueProperty = new ReactiveProperty<T>();
        public T LastValue {
            get{
                return LastValueProperty.Value;
            }
            private set
            {
                LastValueProperty.Value = value;
            }
        }

        protected override void SetValue(T value)
        {
            LastValue = this.Value;
            base.SetValue(value);
        }
    }
}