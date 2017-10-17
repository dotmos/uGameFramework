using System;
using UniRx;

namespace MVC{
    public interface IController : IDisposable{
        void Bind();
        //Fired BEFORE disposing the controller
        void OnDispose();
        //Needed for UniRx extension so we can add disposables to the controller. Can also be used for other IDisposables of course
        void AddDisposable(IDisposable disposable);

        //Return the model
        IModel GetModel();

        /// <summary>
        /// Creates a model which is suitable for the controller. Does NOT assign the model to the controller
        /// </summary>
        /// <returns>The model.</returns>
        IModel CreateModel();
        /// <summary>
        /// Creates a model. Does NOT assign the model to the controller.
        /// </summary>
        /// <returns>The model.</returns>
		TModel CreateModel<TModel>() where TModel : IModel, new();
        /// <summary>
        /// Creates a model. Does NOT assign the model to the controller.
        /// </summary>
        /// <returns>The model.</returns>
        IModel CreateModel(Type modelType);
        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <param name="model">Model.</param>
        void SetModel(IModel model);
        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <typeparam name="TModel">The 1st type parameter.</typeparam>
		void SetModel<TModel>() where TModel : IModel, new();
        /// <summary>
        /// Sets the model.
        /// </summary>
        /// <param name="modelType">Model type.</param>
        void SetModel(Type modelType);

        /// <summary>
        /// Copies values from the supplied model and puts them in the controller model
        /// </summary>
        /// <param name="model">Model.</param>
        void CopyModelValues(IModel model);
        // Use this to listen to an event from the controller which is meant for the view
        IObservable<TEvent> OnController<TEvent>();
        // Use this to publish an event from a view to the controller
        void PublishToController<TEvent>(TEvent evt);

        ReactiveCommand OnDisposing { get; }
    }
}