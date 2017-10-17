using Zenject;
using UniRx;
using System;

namespace Service.Events{
    public class CommandsBase : IDisposable {
        CompositeDisposable disposables = new CompositeDisposable();

        IEventsService _eventService;

        [Inject]
        void Initialize([Inject] IEventsService eventService)
        {
            _eventService = eventService;
        }

        protected void Publish(object evt)
        {
            _eventService.Publish(evt);
        }
        protected void Publish(object evt, Subject<object> eventStream) {
            _eventService.Publish(evt, eventStream);
        }


        protected IObservable<TEvent> OnEvent<TEvent>()
        {
            return _eventService.OnEvent<TEvent>();
        }
        protected IObservable<TEvent> OnEvent<TEvent>(Subject<object> eventStream) {
            return _eventService.OnEvent<TEvent>(eventStream);
        }

        public void AddDisposable(IDisposable disposable)
        {
            disposables.Add(disposable);
        }

        public void Dispose()
        {
            disposables.Dispose();
        }
    }
}
