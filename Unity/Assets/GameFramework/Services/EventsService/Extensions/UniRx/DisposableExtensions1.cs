using System;

namespace UniRx
{
    public static partial class DisposableExtensions
    {
        /// <summary>Add disposable(self) to CompositeDisposable(or other ICollection). Return value is self disposable.</summary>

        // IModel
        public static T AddTo<T>(this T disposable, Service.Events.CommandsBase commandBase)
            where T : IDisposable
        {
            if (disposable == null) throw new ArgumentNullException("disposable");
            if (commandBase == null) throw new ArgumentNullException("commandBase");

            commandBase.AddDisposable(disposable);

            return disposable;
        }
    }
}