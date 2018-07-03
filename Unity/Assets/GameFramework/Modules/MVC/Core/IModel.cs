using System;
using UniRx;

namespace MVC{
    public interface IModel : IDisposable{
        void Bind();
        //Fired BEFORE disposing the model
        void OnDispose();
        //Needed for UniRx extension so we can add disposables to the model. Can also be used for other IDisposables of course
        void AddDisposable(IDisposable disposable);
        /// <summary>
        /// Copies the values from the supplied to model to the model
        /// </summary>
        /// <param name="model">Model.</param>
        void CopyValuesFromOtherModel(IModel model);
        //Serializes the model
        string Serialize();
        //Deserializes the model
        void Deserialize(string data);

        ReactiveCommand OnDisposing { get; }
    }
}