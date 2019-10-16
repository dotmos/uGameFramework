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

public interface IDirtyFlagable {
    bool IsDirty { get; }
    void SetDirtyFlag();
    void ClearDirtyFlag();
}

public interface IObservableList {
    Type GetListType();
    IList InnerIList {
        get;
    }
}

public class ObservableList<T> : IObservableList, IList<T>, IDirtyFlagable {

    private List<T> innerList;
    private bool isDirty = true;

    public ObservableList() { innerList = new List<T>(); }
    public ObservableList(int capacity) { innerList = new List<T>(capacity); }
    public ObservableList(ObservableList<T> list) {
        innerList = new List<T>(list.Count);
        innerList.AddRange(list.innerList);
    }
    public ObservableList(List<T> list) {
        innerList = new List<T>();
        innerList.AddRange(list);
    }

    public ObservableList(T[] t) {
        innerList = new List<T>();
        innerList.AddRange(t);
    }

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

    public IList InnerIList => (IList)innerList;

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

    public void AddRange(ObservableList<T> rangeList) {
        innerList.AddRange(rangeList.innerList);
        SetDirtyFlag();
    }

    public void AddRange(List<T> list) {
        innerList.AddRange(list);
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

    public Type GetListType() {
        return typeof(T);
    }

}

public class Utils {
    /// <summary>
    /// Get a random number except one
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <param name="except"></param>
    /// <returns></returns>
    public static int RandomIntExcept(int min, int max, int except) {
        int result = UnityEngine.Random.Range(min, max - 1);
        if (result >= except) result += 1;
        return result;
    }
}

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDirtyFlagable {
    private Dictionary<TKey, TValue> innerDictionary = new Dictionary<TKey, TValue>();
    private bool isDirty = true;

    public ObservableDictionary(ObservableDictionary<TKey, TValue> dict){
        innerDictionary.ShallowCopyFrom(dict.innerDictionary);
    }

    public ObservableDictionary(Dictionary<TKey, TValue> dict) {
        innerDictionary.ShallowCopyFrom(dict);
    }

    public ObservableDictionary() {
    }


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

/// <summary>
/// One singleton that can pool any object that has a default constructor
/// </summary>
public class GenericPool {
    public Dictionary<Type, SimplePool<object>> pools = new Dictionary<Type, SimplePool<object>>();

    public static GenericPool Instance { get; } = new GenericPool(10);

    private int maxElementsOnStack;

    private GenericPool(int maxElementsOnPoolStack = 10) {
        this.maxElementsOnStack = maxElementsOnPoolStack;
    }

    public Dictionary<Type, Func<object>> customCreators = new Dictionary<Type, Func<object>>();

    public void RegisterCustomCreator<T>(Func<object> createFunc) where T : new() {
        customCreators[typeof(T)] = createFunc;
    }

    public T Acquire<T>() where T:new() {
        if (pools.TryGetValue(typeof(T),out SimplePool<object> pool)) {
            var result = (T)pool.Acquire();
            return result;
        } else {
            // no pool for this type? create one
             
            // try to receive a custom creator that was registered for this type
            customCreators.TryGetValue(typeof(T), out Func<object> createFunc);
            if (createFunc == null) {
                createFunc = () => { return new T(); };
            }

            var newPool = new SimplePool<object>(createFunc, maxElementsOnStack);

            // add some default release-methods to clear list-types on release
            if (typeof(T).IsAssignableFrom(typeof(IList))) {
                newPool.onRelease = (obj) => {
                    ((IList)obj).Clear();
                };
            }
            else if (typeof(T).IsAssignableFrom(typeof(IDictionary))) {
                newPool.onRelease = (obj) => {
                    ((IDictionary)obj).Clear();
                };
            }

            pools[typeof(T)] = newPool;
            var newObj = newPool.Acquire();
            return (T)newObj;
        }
    }

    public void Release(params object[] releaseObjs) {
        for (int i = releaseObjs.Length - 1; i >= 0; i--) {
            var releaseObj = releaseObjs[i];
            if (pools.TryGetValue(releaseObj.GetType(), out SimplePool<object> pool)) {
                pool.Release(releaseObj);
            } else {
                Debug.LogWarning("You tried to release an object that wasn't acquired via genericpool");
                // TODO: Do we want to release objects for a type that weren't created once with this? why not
            }
        }
    }

    /// <summary>
    /// Clear all pools with released objects
    /// </summary>
    public void ClearPool() {
        foreach (var pool in pools.Values) {
            pool.Dispose(false);
        }
        pools.Clear();
    }
}

public class ArrayPool<T> where T : struct {
    private List<T[]> pool = new List<T[]>();

    public ArrayPool() {
    }

    public T[] GetArray(int capacity) {
        lock (pool) {
            if (pool.Count == 0) {
                return new T[capacity];
            }
            int amount = pool.Count;
            int i = 0;
            T[] theArray = null;
            for (; i < amount; i++) {
                theArray = pool[i];
                int arrayLength = theArray.Length;
                if (arrayLength == capacity) {
                    pool.Remove(theArray);
                    return theArray;
                } else if (capacity < arrayLength) { // sorted list, no chance to find one with lower capactiy
                    break;
                }
            }
            return new T[capacity];
        }
    }

    public void Release(params T[][] releasedArray) {
        lock (pool) {
            for (int i = releasedArray.Length - 1; i >= 0; i--) {
                if (releasedArray[i] == null) {
                    int a = 0;
                }
                pool.Add(releasedArray[i]);
            }
            pool.Sort((x, y) => {
                /*
                if (x == null || y == null) {
                    int b = 0;
                }
                */
                if (x.Length < y.Length) return -1;
                else if (x.Length > y.Length) return 1;
                else return 0;
            });
        }
    }
}

public class ListPool<T>  {
    private List<List<T>> pool = new List<List<T>>();

    public ListPool(int initialAmount=0, int initialCapacity=0) {
        for (int i = 0; i < initialAmount; i++) {
            var listElem = new List<T>(initialCapacity);
            pool.Add(listElem);
        }
    }

    public List<T> GetList(int capacity) {
        lock (pool) {
            if (pool.Count == 0) {
                return new List<T>(capacity);
            }
            int amount = pool.Count;
            int i = 0;
            List<T> theList = null;
            for (; i < amount; i++) {
                theList = pool[i];
                if (theList.Capacity > capacity) {
                    pool.Remove(theList);
                    return theList;
                }
            }
            pool.Remove(theList);
            return theList;
        }
    }

    public void Release(params List<T>[] releasedList) {
        lock (pool) {
            for (int i = releasedList.Length - 1; i >= 0; i--) {
                var list = releasedList[i];
                list.Clear();
                pool.Add(list);
            }

            pool.Sort((x, y) => {
                if (x.Capacity < y.Capacity) return 1;
                else if (x.Capacity > y.Capacity) return -1;
                else return 0;
            });
        }
    }
}

public class SimplePool<T>  where T : class {
    // keep the available objects in a stack
    readonly Stack<T> _stack = new Stack<T>();
    public Func<T> createFunc;
    public Action<T> onAcquire;
    public Action<T> onRelease;
    public Action<T> onDispose;

    /// <summary>
    /// Objects that are acquired by the user
    /// </summary>
    readonly List<WeakReference<T>> acquiredObjects = new List<WeakReference<T>>();
    readonly bool keepTrackOfAcquired;

    int discardObjectCount = -1;

    /// <summary>
    /// Create a pool for any reference-type. you need to provide at least a createFunction that will create an object for the pool
    /// if no object can be acquired from the stack
    /// </summary>
    /// <param name="createFunc">function that creates a new instance</param>
    /// <param name="maxObjectsOnStack">amount of objects that can be kept on the stack waiting for acquire.(InitObjects is not affected by this value)</param>
    /// <param name="keepTrackOfAquiredObjects">keep a weakreference on all objects created by us an acquired by the use</param>
    /// <param name="onAcquire">function called when acquire is called and the object is passed to the user</param>
    /// <param name="onRelease">function called after release</param>
    /// <param name="onDispose">function called to dispose the object</param>
    public SimplePool(Func<T> createFunc,int maxObjectsOnStack=-1,bool keepTrackOfAquiredObjects=false,Action<T> onAcquire=null,Action<T> onRelease = null,Action<T> onDispose=null) {
        this.createFunc = createFunc;
        this.onAcquire = onAcquire;
        this.onRelease = onRelease;
        this.onDispose = onDispose;
        SetDiscardObjectCount(maxObjectsOnStack);
        this.keepTrackOfAcquired = keepTrackOfAquiredObjects;
    }

    /// <summary>
    /// Set the maximum amout of objects on the internal stack. Using InitObjects is not affect by this. But objects released will not be 
    /// added to the stack if the stack amount is higher that the discardObjectCount
    /// </summary>
    /// <param name="amount"></param>
    public void SetDiscardObjectCount(int amount) {
        discardObjectCount = amount;
    }

    /// <summary>
    /// Create this amount of objects and puts it on the internal stack
    /// </summary>
    /// <param name="amount"></param>
    public void InitObjects(int amount) {
        for (int i = amount - 1; i >= 0; i--) {
            T obj = createFunc();
            if (onAcquire != null) {
                onAcquire(obj);
            }
            _stack.Push(obj);
        }
    }

    /// <summary>
    /// Gets a pooled object or creates a new one if needed
    /// </summary>
    /// <returns></returns>
    public T Acquire() {
        T obj = _stack.Count > 0
                ? _stack.Pop()
                : createFunc();

        if (onAcquire != null) {
            onAcquire(obj);
        }
        if (keepTrackOfAcquired) {
            acquiredObjects.Add(new WeakReference<T>(obj));
        }
        return obj;
    }

    /// <summary>
    /// Release the object to the pool again so that is can be reused
    /// </summary>
    /// <param name="obj"></param>
    public void Release(T obj) {
        if (onRelease != null) {
            onRelease(obj);
        }

        if (keepTrackOfAcquired) {
            // remove the current and all weak pointers which got invalid
            acquiredObjects.RemoveAll(o => !o.TryGetTarget(out T target) || target == o);
        }

        if (discardObjectCount==-1 || _stack.Count < discardObjectCount) {
            _stack.Push(obj);
        } else if (onDispose != null) {
            // we exceeded the stack amount therefore let the GC do what it have to do
            onDispose(obj);
        }
    }

    /// <summary>
    /// Call the onDispose action on all objects still in the stack
    /// 
    /// </summary>
    /// <param name="includingAcquiredObjects"></param>
    public void Dispose(bool includingAcquiredObjects) {
        if (onDispose != null) {
            while (_stack.Count > 0) {
                var obj = _stack.Pop();
                onDispose(obj);
            }

            if (includingAcquiredObjects) {
                foreach (var weakRef in acquiredObjects) {
                    if (weakRef.TryGetTarget(out T obj)){
                        onDispose(obj);
                    }
                }
                while (_stack.Count > 0) {
                    var obj = _stack.Pop();
                    onDispose(obj);
                }

            }

        }
        _stack.Clear();
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

    public static float NextFloat(this System.Random rand,float from,float to) {
        var r = rand.Next((int)(from * 1000), (int)(to * 1000)) / 1000.0f;
        return r;
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

    public static void ShallowCopyFrom<T, S>(this Dictionary<T,S> dest, Dictionary<T, S> data) {
        int amount = data.Count;

        foreach (var elem in data) {
            dest[elem.Key] = elem.Value;
        }
    }

    public static void ShallowCopyFrom<T, S>(this ObservableDictionary<T,S> dest, ObservableDictionary<T, S> data) {
        int amount = data.Count;

        foreach (var elem in data) {
            dest[elem.Key] = elem.Value;
        }
    }

    public static void ShallowCopyFrom<T>(this List<T> dest, List<T> data) {
        int amount = data.Count;
        for (int i = 0; i < amount; i++) {
            dest.Add(data[i]);
        }
    }

    public static void ShallowCopyFrom<T>(this HashSet<T> dest, HashSet<T> data) {
        int amount = data.Count;
        foreach (var elem in data) {
            dest.Add(elem);
        }
    }

}

