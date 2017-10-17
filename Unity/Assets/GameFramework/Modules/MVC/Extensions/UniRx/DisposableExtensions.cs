using System;
using MVC;

namespace UniRx
{
    public static partial class DisposableExtensions
    {
        /// <summary>Add disposable(self) to CompositeDisposable(or other ICollection). Return value is self disposable.</summary>

        // IModel
        public static T AddTo<T>(this T disposable, IModel model)
            where T : IDisposable
        {
            if (disposable == null) throw new ArgumentNullException("disposable");
            if (model == null) throw new ArgumentNullException("model");

            model.AddDisposable(disposable);

            return disposable;
        }

        // IController
        public static T AddTo<T>(this T disposable, IController controller)
            where T : IDisposable
        {
            if (disposable == null) throw new ArgumentNullException("disposable");
            if (controller == null) throw new ArgumentNullException("controller");

            controller.AddDisposable(disposable);

            return disposable;
        }
    }
}