using System;
using System.Collections.Generic;
using UniRx;
using Zenject;

namespace Service.Events {
    public class EventsService : IEventsService, IDisposable //, ISubject<object>
    {
        bool isDisposed;
        //readonly Subject<object> eventsSubject = new Subject<object>();

        /// <summary>
        /// Stores all streams for all the different commands/events. Used to optimize eventservice performance (commands/events are now pre-sorted by stream)
        /// </summary>
        Dictionary<Type, Subject<object>> streams = new Dictionary<Type, Subject<object>>();

        Service.AsyncManager.IAsyncManager _asyncManager;

        [Inject]
        void Initialize(
            [InjectOptional] Service.AsyncManager.IAsyncManager asyncManager
        ) {
            _asyncManager = asyncManager;
        }

        public IObservable<TEvent> OnEvent<TEvent>() {
            return GetStream(typeof(TEvent)).Select(p => (TEvent)p);
            //return stream.Where(p => p is TEvent).Select(p => (TEvent)p);
        }

        public IObservable<TEvent> OnEvent<TEvent>(Subject<object> eventStream) {
            return eventStream.Where(p => p is TEvent).Select(p => (TEvent)p);
        }

        public void Publish<TEvent>(TEvent evt) {
            GetStream(evt.GetType()).OnNext(evt);
            //eventsSubject.OnNext(evt);
        }

        public void Publish<TEvent>(TEvent evt, Subject<object> eventStream) {
            eventStream.OnNext(evt);
        }

        /// <summary>
        /// Get the correct stream for the event/command
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <returns></returns>
        Subject<object> GetStream(Type type) {
            Subject<object> stream;
            if (streams.ContainsKey(type)) {
                stream = streams[type];
            }
            else {
                stream = new Subject<object>();
                streams.Add(type, stream);
            }
            return stream;
            

            //return eventsSubject;
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing) {
            if (isDisposed) return;

            //TODO: Dispose all streams

            //eventsSubject.Dispose();
            isDisposed = true;
        }

        public AsyncPublishContext PublishAsync<TEvent>(TEvent evt, Action<TEvent> onFinished = null, AsyncType type = AsyncType.Publish_Main_Result_Main) {
            // create the context-object which can be used to check for finished if e.g. from within a coroutine
            AsyncPublishContext result = new AsyncPublishContext();

            if (type == AsyncType.Publish_Main_Result_Main) {
                _asyncManager.AddToMainThread(() => {
                    // inside coroutine
                    Publish(evt);
                    result.finished = true;
                    result.result = evt;
                    if (onFinished != null) {
                        onFinished(evt);
                    }

                });  
                      
            }
            else 
            // atm, at this point there are only the worker-options, so I don't need to query specially for those
            //if (type == AsyncType.Publish_Worker_Result_Worker || type == AsyncType.Publish_Worker_Result_Main) 
            {
                _asyncManager.AddToWorkerThread(() => {
                    // inside worker-thread
                    Publish(evt);
                    result.finished = true;
                    result.result = evt;

                    if (type == AsyncType.Publish_Worker_Result_Main) {
                        // since we want the result send to the main-thread, we need to wrap it in an Action
                        // and add it to the main-thread-queue
                        _asyncManager.AddToMainThread(() => {
                            onFinished(evt);
                        });
                    } 
                    else if (type == AsyncType.Publish_Worker_Result_Worker) {
                        // immediately send the result to the callback which will be called from within(!) the worker-thread    
                        onFinished(evt);
                    }

                },null);
            }
            // return the context-object
            return result;
        }

        /*
        public void OnCompleted()
        {
            eventsSubject.OnCompleted();
        }

        public void OnError(Exception error)
        {
            eventsSubject.OnError(error);
        }

        public void OnNext(object value)
        {
            eventsSubject.OnNext(value);
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            return eventsSubject.Subscribe(observer);
        }
        */
    }
}