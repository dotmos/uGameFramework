using UnityEngine;
using System.Collections;
using UniRx;
using System;

namespace Extensions
{
    public static partial class AsynOperation
    {
        /*
        public static IObservable<float> ToObservable(this UnityEngine.AsyncOperation asyncOperation)
        {
            if (asyncOperation == null) throw new ArgumentNullException("asyncOperation");

            return Observable.FromCoroutine<float>((observer, cancellationToken) => RunAsyncOperation(asyncOperation, observer, cancellationToken));
        }

        static IEnumerator RunAsyncOperation(UnityEngine.AsyncOperation asyncOperation, IObserver<float> observer, CancellationToken cancellationToken)
        {
            while (!asyncOperation.isDone && !cancellationToken.IsCancellationRequested)
            {
                observer.OnNext(asyncOperation.progress);
                yield return null;
            }
            if (!cancellationToken.IsCancellationRequested)
            {
                observer.OnNext(asyncOperation.progress); // push 100%
                observer.OnCompleted();
            }
        }
        */
    }
}
