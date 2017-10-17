using UniRx;
using System;

namespace Service.Events{
    public enum AsyncType {
        /// <summary>
        /// Do the publish call on the worker-thread and call the callback immediately with the result inside this thread
        /// </summary>
        Publish_Worker_Result_Worker,
        /// <summary>
        /// Do the publish call on the worker-thread and add the callback with the result inside to the mainthread-queue
        /// </summary>
        Publish_Worker_Result_Main,
        /// <summary>
        /// Queue the publish call in the MAIN-thread queue and call the callback immediately within the MAIN-thread
        /// </summary>
        Publish_Main_Result_Main
    }

    public class AsyncPublishContext {
        public bool finished = false;
        public object result = null;
    }

    public interface IEventsService
    {
        IObservable<TEvent> OnEvent<TEvent>();
        IObservable<TEvent> OnEvent<TEvent>(Subject<object> eventStream);
        void Publish<TEvent>(TEvent evt);
        void Publish<TEvent>(TEvent evt, Subject<object> eventStream);
        AsyncPublishContext PublishAsync<TEvent>(TEvent evt,Action<TEvent> onFinished=null,AsyncType type = AsyncType.Publish_Main_Result_Main);
    }
}