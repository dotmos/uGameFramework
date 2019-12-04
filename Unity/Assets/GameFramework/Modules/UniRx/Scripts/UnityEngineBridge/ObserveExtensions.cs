using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UniRx.Triggers;

#if !UniRxLibrary
using ObservableUnity = UniRx.Observable;
#endif

namespace UniRx
{
    public static partial class ObserveExtensions
    {
        /// <summary>
        /// Publish target property when value is changed. If source is destroyed/destructed, publish OnCompleted.
        /// </summary>
        /// <param name="fastDestroyCheck">If true and target is UnityObject, use destroyed check by additional component. It is faster check for lifecycle but needs initial cost.</param>
        public static IObservable<TProperty> ObserveEveryValueChanged<TSource, TProperty>(this TSource source, Func<TSource, TProperty> propertySelector, FrameCountType frameCountType = FrameCountType.Update, bool fastDestroyCheck = false)
            where TSource : class
        {
            return ObserveEveryValueChanged(source, propertySelector, frameCountType, UnityEqualityComparer.GetDefault<TProperty>(), fastDestroyCheck);
        }

        /// <summary>
        /// Publish target property when value is changed. If source is destroyed/destructed, publish OnCompleted.
        /// </summary>
        public static IObservable<TProperty> ObserveEveryValueChanged<TSource, TProperty>(this TSource source, Func<TSource, TProperty> propertySelector, FrameCountType frameCountType, IEqualityComparer<TProperty> comparer)
            where TSource : class
        {
            return ObserveEveryValueChanged(source, propertySelector, frameCountType, comparer, false);
        }

        /// <summary>
        /// Publish target property when value is changed. If source is destroyed/destructed, publish OnCompleted.
        /// </summary>
        /// <param name="fastDestroyCheck">If true and target is UnityObject, use destroyed check by additional component. It is faster check for lifecycle but needs initial cost.</param>
        public static IObservable<TProperty> ObserveEveryValueChanged<TSource, TProperty>(this TSource source, Func<TSource, TProperty> propertySelector, FrameCountType frameCountType, IEqualityComparer<TProperty> comparer, bool fastDestroyCheck)
            where TSource : class
        {
            if (source == null) return Observable.Empty<TProperty>();
            if (comparer == null) comparer = UnityEqualityComparer.GetDefault<TProperty>();

            UnityEngine.Object unityObject = source as UnityEngine.Object;
            bool isUnityObject = source is UnityEngine.Object;
            if (isUnityObject && unityObject == null) return Observable.Empty<TProperty>();

            // MicroCoroutine does not publish value immediately, so publish value on subscribe.
            if (isUnityObject)
            {
                return ObservableUnity.FromMicroCoroutine<TProperty>((observer, cancellationToken) =>
                {
                    if (unityObject != null)
                    {
                        TProperty firstValue = default(TProperty);
                        try
                        {
                            firstValue = propertySelector((TSource)(object)unityObject);
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            return EmptyEnumerator();
                        }

                        observer.OnNext(firstValue);
                        return PublishUnityObjectValueChanged(unityObject, firstValue, propertySelector, comparer, observer, cancellationToken, fastDestroyCheck);
                    }
                    else
                    {
                        observer.OnCompleted();
                        return EmptyEnumerator();
                    }
                }, frameCountType);
            }
            else
            {
                WeakReference reference = new WeakReference(source);
                source = null;

                return ObservableUnity.FromMicroCoroutine<TProperty>((observer, cancellationToken) =>
                {
                    object target = reference.Target;
                    if (target != null)
                    {
                        TProperty firstValue = default(TProperty);
                        try
                        {
                            firstValue = propertySelector((TSource)target);
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            return EmptyEnumerator();
                        }
                        finally
                        {
                            target = null;
                        }

                        observer.OnNext(firstValue);
                        return PublishPocoValueChanged(reference, firstValue, propertySelector, comparer, observer, cancellationToken);
                    }
                    else
                    {
                        observer.OnCompleted();
                        return EmptyEnumerator();
                    }
                }, frameCountType);
            }
        }

        static IEnumerator EmptyEnumerator()
        {
            yield break;
        }

        static IEnumerator PublishPocoValueChanged<TSource, TProperty>(WeakReference sourceReference, TProperty firstValue, Func<TSource, TProperty> propertySelector, IEqualityComparer<TProperty> comparer, IObserver<TProperty> observer, CancellationToken cancellationToken)
        {
            TProperty currentValue = default(TProperty);
            TProperty prevValue = firstValue;

            while (!cancellationToken.IsCancellationRequested)
            {
                object target = sourceReference.Target;
                if (target != null)
                {
                    try
                    {
                        currentValue = propertySelector((TSource)target);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        yield break;
                    }
                    finally
                    {
                        target = null; // remove reference(must need!)
                    }
                }
                else
                {
                    observer.OnCompleted();
                    yield break;
                }

                if (!comparer.Equals(currentValue, prevValue))
                {
                    observer.OnNext(currentValue);
                    prevValue = currentValue;
                }

                yield return null;
            }
        }

        static IEnumerator PublishUnityObjectValueChanged<TSource, TProperty>(UnityEngine.Object unityObject, TProperty firstValue, Func<TSource, TProperty> propertySelector, IEqualityComparer<TProperty> comparer, IObserver<TProperty> observer, CancellationToken cancellationToken, bool fastDestroyCheck)
        {
            TProperty currentValue = default(TProperty);
            TProperty prevValue = firstValue;

            TSource source = (TSource)(object)unityObject;

            if (fastDestroyCheck)
            {
                ObservableDestroyTrigger destroyTrigger = null;
                {
                    UnityEngine.GameObject gameObject = unityObject as UnityEngine.GameObject;
                    if (gameObject == null)
                    {
                        UnityEngine.Component comp = unityObject as UnityEngine.Component;
                        if (comp != null)
                        {
                            gameObject = comp.gameObject;
                        }
                    }

                    // can't use faster path
                    if (gameObject == null) goto STANDARD_LOOP;

                    destroyTrigger = GetOrAddDestroyTrigger(gameObject);
                }

                // fast compare path
                while (!cancellationToken.IsCancellationRequested)
                {
                    bool isDestroyed = destroyTrigger.IsActivated
                        ? !destroyTrigger.IsCalledOnDestroy
                        : (unityObject != null);

                    if (isDestroyed)
                    {
                        try
                        {
                            currentValue = propertySelector(source);
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                            yield break;
                        }
                    }
                    else
                    {
                        observer.OnCompleted();
                        yield break;
                    }

                    if (!comparer.Equals(currentValue, prevValue))
                    {
                        observer.OnNext(currentValue);
                        prevValue = currentValue;
                    }

                    yield return null;
                }

                yield break;
            }

            STANDARD_LOOP:
            while (!cancellationToken.IsCancellationRequested)
            {
                if (unityObject != null)
                {
                    try
                    {
                        currentValue = propertySelector(source);
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                        yield break;
                    }
                }
                else
                {
                    observer.OnCompleted();
                    yield break;
                }

                if (!comparer.Equals(currentValue, prevValue))
                {
                    observer.OnNext(currentValue);
                    prevValue = currentValue;
                }

                yield return null;
            }
        }

        static ObservableDestroyTrigger GetOrAddDestroyTrigger(UnityEngine.GameObject go)
        {
            ObservableDestroyTrigger dt = go.GetComponent<ObservableDestroyTrigger>();
            if (dt == null)
            {
                dt = go.AddComponent<ObservableDestroyTrigger>();
            }
            return dt;
        }
    }
}