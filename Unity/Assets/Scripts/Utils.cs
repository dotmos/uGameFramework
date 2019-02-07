using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;


public interface IExecutionWrapper
{
    /// <summary>
    /// Wraps a function with T return value with additional logics. e.g. exception handling, logging, performance testing (based on the executionType if stated, otherwise executionType will be unknown)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="executionType"></param>
    /// <returns></returns>
    Func<T> Wrap<T>(Func<T> input,ExecutionDomain executionType=ExecutionDomain.unknown);
    /// <summary>
    /// Wraps the function and immediately call it
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="executionType"></param>
    /// <returns></returns>
    T WrapExe<T>(Func<T> input, ExecutionDomain executionType = ExecutionDomain.unknown);
    /// <summary>
    /// Wraps an action with additional logics. e.g. exception handling, logging, performance testing (based on the executionType if stated, otherwise executionType will be unknown)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="input"></param>
    /// <param name="executionType"></param>
    /// <returns></returns>
    Action Wrap(Action input, ExecutionDomain executionType = ExecutionDomain.unknown);
    /// <summary>
    /// Wraps an action and immediately calls it. This is an conveniece calls for Wrap(...)();
    /// </summary>
    /// <param name="input"></param>
    /// <param name="executionType"></param>
    void WrapExe(Action input, ExecutionDomain executionType = ExecutionDomain.unknown);
}

public class UtilsObservable
{
    public static IObservable<bool> LoadScene(string sceneName, bool makeActive = false) {
        return Observable.Create<bool>((observer) => {
            var async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            async.completed += (val) => {
                if(makeActive) SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
                observer.OnNext(true);
                observer.OnCompleted();
            };
            return null;
        });
    }

    public static IObservable<bool> UnloadScene(string sceneName) {
        return Observable.Create<bool>((observer) => {
            int idx = SceneManager.GetSceneByName(sceneName).buildIndex;
            var async = SceneManager.UnloadSceneAsync(idx);
            async.completed += (val) => {
                observer.OnNext(true);
                observer.OnCompleted();
            };
            return null;
        });
    }

}

public class DefaultExecutionWrapper : IExecutionWrapper
{
    public Func<T> Wrap<T>(Func<T> input, ExecutionDomain executionType = ExecutionDomain.unknown) {
        return (() => {
            // TODO START TIMER
            try {
                return input();
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("There was an catched exception:");
                UnityEngine.Debug.LogException(e);
                return default(T);
            }
            finally {
                
                // TODO STOP TIMER
            }
        });
    }



    public Action Wrap(Action input, ExecutionDomain executionType = ExecutionDomain.unknown) {
        return (() => {
            // TODO START TIMER
            try {
                input();
            }
            catch (Exception e) {
                UnityEngine.Debug.LogError("There was an catched exception:");
                UnityEngine.Debug.LogException(e);
            }
            finally {
                // TODO STOP TIMER
            }
        });
    }

    public T WrapExe<T>(Func<T> input, ExecutionDomain executionType = ExecutionDomain.unknown) {
        return Wrap(input, executionType)();
    }

    public void WrapExe(Action input, ExecutionDomain executionType = ExecutionDomain.unknown) {
        Wrap(input, executionType);
    }
}

public interface IReactiveExecutionWrapper
{
    IObservable<T> Wrap<T>(IObservable<T> input, ExecutionDomain executionType = ExecutionDomain.unknown);
    IObservable<T> Wrap<T>(Func<T> input, ExecutionDomain executionType = ExecutionDomain.unknown);
    IObservable<bool> Wrap(Action input, ExecutionDomain executionType = ExecutionDomain.unknown);
}

public class DefaultReactiveExecutionWrapper : IReactiveExecutionWrapper
{
    public IObservable<T> Wrap<T>(IObservable<T> input, ExecutionDomain executionType = ExecutionDomain.unknown) {

        return input
            // TODO: To the actual wrapping with errorhandling and starting/stoping timers
            //.Catch(err => {  }) 
            ;
    }

    public IObservable<T> Wrap<T>(Func<T> input, ExecutionDomain executionType = ExecutionDomain.unknown) {
        return Wrap(Observable.Create<T>((observer) => {
            T result = input();
            observer.OnNext(result);
            observer.OnCompleted();
            return null;
        }));
    }

    public IObservable<bool> Wrap(Action input, ExecutionDomain executionType = ExecutionDomain.unknown) {
        return Wrap(Observable.Create<bool>((observer) => {
            input();
            observer.OnNext(true);
            observer.OnCompleted();
            return null;
        }));
    }

}

public interface IObservableEnumeration {
    bool IsDirty { get; }
    void SetDirtyFlag();
    void ClearDirtyFlag();
}

public class ObservableList<T> : IList<T>, IObservableEnumeration {

    private List<T> innerList = new List<T>();
    private bool isDirty = true;

    public List<T> InnerList {
        get { return innerList; }
    }

    public bool IsDirty {
        get { return isDirty; }
    }

    public void SetDirtyFlag() {
        isDirty = true;
    }

    public void ClearDirtyFlag() {
        isDirty = false;
    }

    public int Count => ((IList<T>)innerList).Count;

    public bool IsReadOnly => ((IList<T>)innerList).IsReadOnly;

    public T this[int index] { get => ((IList<T>)innerList)[index]; set  { ((IList<T>)innerList)[index] = value; SetDirtyFlag(); } }

    public int IndexOf(T item) {
        return ((IList<T>)innerList).IndexOf(item);
    }

    public void Insert(int index, T item) {
        ((IList<T>)innerList).Insert(index, item);
        SetDirtyFlag();
    }

    public void RemoveAt(int index) {
        ((IList<T>)innerList).RemoveAt(index);
        SetDirtyFlag();
    }

    public void Add(T item) {
        ((IList<T>)innerList).Add(item);
        SetDirtyFlag();
    }

    public void Clear() {
        ((IList<T>)innerList).Clear();
        SetDirtyFlag();
    }

    public bool Contains(T item) {
        return ((IList<T>)innerList).Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        SetDirtyFlag();
        ((IList<T>)innerList).CopyTo(array, arrayIndex);
    }

    public bool Remove(T item) {
        SetDirtyFlag();
        return ((IList<T>)innerList).Remove(item);
    }

    public IEnumerator<T> GetEnumerator() {
        return ((IList<T>)innerList).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IList<T>)innerList).GetEnumerator();
    }
}

public class ObservableDictionary<TKey,TValue> : IDictionary<TKey, TValue>, IObservableEnumeration {
    private Dictionary<TKey, TValue> innerDictionary = new Dictionary<TKey, TValue>();
    private bool isDirty = true;

    public Dictionary<TKey,TValue> InnerDictionary {
        get { return innerDictionary; }
    }

    public bool IsDirty {
        get { return isDirty; }
    }

    public void SetDirtyFlag() {
        isDirty = true;
    }

    public void ClearDirtyFlag() {
        isDirty = false;
    }

    public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)innerDictionary).Keys;

    public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)innerDictionary).Values;

    public int Count => ((IDictionary<TKey, TValue>)innerDictionary).Count;

    public bool IsReadOnly => ((IDictionary<TKey, TValue>)innerDictionary).IsReadOnly;

    public TValue this[TKey key] { get => ((IDictionary<TKey, TValue>)innerDictionary)[key]; set { ((IDictionary<TKey, TValue>)innerDictionary)[key] = value; SetDirtyFlag(); } }

    public void Add(TKey key, TValue value) {
        ((IDictionary<TKey, TValue>)innerDictionary).Add(key, value);
        SetDirtyFlag();
    }

    public bool ContainsKey(TKey key) {
        return ((IDictionary<TKey, TValue>)innerDictionary).ContainsKey(key);
    }

    public bool Remove(TKey key) {
        SetDirtyFlag();
        return ((IDictionary<TKey, TValue>)innerDictionary).Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value) {
        return ((IDictionary<TKey, TValue>)innerDictionary).TryGetValue(key, out value);
    }

    public void Add(KeyValuePair<TKey, TValue> item) {
        SetDirtyFlag();
        ((IDictionary<TKey, TValue>)innerDictionary).Add(item);
    }

    public void Clear() {
        SetDirtyFlag();
        ((IDictionary<TKey, TValue>)innerDictionary).Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item) {
        return ((IDictionary<TKey, TValue>)innerDictionary).Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
        SetDirtyFlag();
        ((IDictionary<TKey, TValue>)innerDictionary).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<TKey, TValue> item) {
        SetDirtyFlag();
        return ((IDictionary<TKey, TValue>)innerDictionary).Remove(item);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
        return ((IDictionary<TKey, TValue>)innerDictionary).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IDictionary<TKey, TValue>)innerDictionary).GetEnumerator();
    }
}

public class DebugUtils {
    /// <summary>
    /// Call this method the debugger for some reason finds no breakpoint. you can use conditional
    /// breakpoints for different values you give as paramter
    /// </summary>
    /// <param name="val"></param>
    public static void DebugJump(string outputString, object userdata=null,bool output = false) {
        if (output) {
            Debug.Log(outputString);
        }
    }
}

public static partial class UtilsExtensions
{
    public static IObservable<bool> ToObservable(this Action act) {
        return Observable.Create<bool>((observer) => {
            act();
            observer.OnNext(true);
            observer.OnCompleted();
            return null;
        });
    }
    public static IObservable<T> ToObservable<T>(this Func<T> func) {
        return Observable.Create<T>((observer) => {
            var result = func();
            observer.OnNext(result);
            observer.OnCompleted();
            return null;
        });
    }

    public static Vector2 StringToVector2(string sVector) {
        sVector = sVector.Replace(" ", "");
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")")) {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        Vector2 result = new Vector2(
                                float.Parse(sArray[0].Trim()),
                                float.Parse(sArray[1].Trim())
                        );

        return result;
    }
    public static Vector3 StringToVector3(string sVector) {
        // Remove the parentheses
        sVector = sVector.Replace(" ", "");
        if (sVector.StartsWith("(") && sVector.EndsWith(")")) {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }
    public static Vector4 StringToVector4(string sVector) {
        sVector = sVector.Replace(" ", "");
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")")) {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector4 result = new Vector4(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));

        return result;
    }



}

