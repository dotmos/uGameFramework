using System;
using MVC;

namespace UniRx
{
    public static partial class DisposableExtensions
    {
        /// <summary>Add disposable(self) to CompositeDisposable(or other ICollection). Return value is self disposable.</summary>

        // ISystem
        public static T AddTo<T>(this T disposable, ECS.ISystem system)
            where T : IDisposable
        {
            if (disposable == null) throw new ArgumentNullException("disposable");
            if (system == null) throw new ArgumentNullException("system");

            system.AddDisposable(disposable);

            return disposable;
        }

    }
}