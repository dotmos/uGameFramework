using ModestTree;
using System;
using System.Collections;
using System.Collections.Generic;

using UniRx;
using UnityEngine;
using UnityEngine.Assertions;
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


public class DirtyFlag : IDirtyFlagable
{
    private bool _isDirty = false;

    public bool IsDirty => _isDirty;
    public void SetDirtyFlag() {
        _isDirty = true;
    }

    public void ClearDirtyFlag() {
        _isDirty = false;
    }
}
public class UtilsObservable
{
    public static IObservable<bool> LoadScene(string sceneName, bool makeActive = false) {
        return Observable.Create<bool>((observer) => {
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
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
            var scene = SceneManager.GetSceneByName(sceneName);
            if (scene.isLoaded) {
                AsyncOperation async = SceneManager.UnloadSceneAsync(scene.buildIndex);
                async.completed += (val) => {
                    Debug.LogWarning("Unload Scene Progress:" + val.progress);
                    if (val.isDone) {
                        observer.OnNext(true);
                        observer.OnCompleted();
                    }

                };
            } else {
                observer.OnNext(true);
                observer.OnCompleted();
            }
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

public interface IObservableDictionary
{
    Type GetKeyType();
    Type GetValueType();
    IDictionary InnerIDict {
        get;
    }
}

public class ObservableList<T> : IList<T>,IObservableList, IDirtyFlagable {

    public List<T> __innerList;
    private bool isDirty = true;

    public ObservableList() { __innerList = new List<T>(); }
    public ObservableList(int capacity) { __innerList = new List<T>(capacity); }
    public ObservableList(ObservableList<T> list) {
        __innerList = new List<T>(list.Count);
        __innerList.AddRange(list.__innerList);
    }
    public ObservableList(List<T> list) {
        __innerList = new List<T>();
        __innerList.AddRange(list);
    }

    public ObservableList(T[] t) {
        __innerList = new List<T>(t);
    }

    public List<T> InnerList {
        get { return __innerList; }
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

    public int Count => ((IList<T>)__innerList).Count;

    public bool IsReadOnly => ((IList<T>)__innerList).IsReadOnly;

    public IList InnerIList => (IList)__innerList;
    public IList<T> InnerIListGeneric => (IList<T>)__innerList;

    public T this[int index] { get => ((IList<T>)__innerList)[index]; set  { ((IList<T>)__innerList)[index] = value; SetDirtyFlag(); } }

    public int IndexOf(T item) {
        return ((IList<T>)__innerList).IndexOf(item);
    }

    public void Insert(int index, T item) {
        ((IList<T>)__innerList).Insert(index, item);
        SetDirtyFlag();
    }

    public void RemoveAt(int index) {
        ((IList<T>)__innerList).RemoveAt(index);
        SetDirtyFlag();
    }

    public void Add(T item) {
        ((IList<T>)__innerList).Add(item);
        SetDirtyFlag();
    }

    public void AddRange(ObservableList<T> rangeList) {
        __innerList.AddRange(rangeList.__innerList);
        SetDirtyFlag();
    }

    public void AddRange(T[] rangeArray) {
        __innerList.AddRange(rangeArray);
        SetDirtyFlag();
    }

    public void AddRange(List<T> list) {
        __innerList.AddRange(list);
        SetDirtyFlag();
    }

    public void Clear() {
        ((IList<T>)__innerList).Clear();
        SetDirtyFlag();
    }

    public bool Contains(T item) {
        return ((IList<T>)__innerList).Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex) {
        SetDirtyFlag();
        ((IList<T>)__innerList).CopyTo(array, arrayIndex);
    }

    public bool Remove(T item) {
        SetDirtyFlag();
        return ((IList<T>)__innerList).Remove(item);
    }

    public IEnumerator<T> GetEnumerator() {
        return ((IList<T>)__innerList).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return ((IList<T>)__innerList).GetEnumerator();
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

public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDirtyFlagable, IObservableDictionary {
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

    public IDictionary InnerIDict => innerDictionary;

    public TValue this[TKey key] { get => ((IDictionary<TKey, TValue>)innerDictionary)[key]; set { ((IDictionary<TKey, TValue>)innerDictionary)[key] = value; SetDirtyFlag(); } }

    public void Add(TKey key, TValue value) {
        ((IDictionary<TKey, TValue>)innerDictionary).Add(key, value);
        SetDirtyFlag();
    }

    public bool ContainsKey(TKey key) {
        return ((IDictionary<TKey, TValue>)innerDictionary).ContainsKey(key);
    }

    public bool ContainsValue(TValue value) {
        return innerDictionary.ContainsValue(value);
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

    public Type GetKeyType() {
        return typeof(TKey);
    }

    public Type GetValueType() {
        return typeof(TValue);
    }
}

/*public class TestUtils {

    private static Type typeIList = typeof(IList);
    private static Type typeIDictionary = typeof(IDictionary);

    public static void AssertCompareObjs<T>(T self, T to,string prefix="") where T : class {
        if (self != null && to != null) {
            Type type = typeof(T);
            foreach (System.Reflection.PropertyInfo pi in type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)) {
                var prop = type.GetProperty(pi.Name);
                object selfValue = type.GetProperty(pi.Name).GetValue(self, null);
                object toValue = type.GetProperty(pi.Name).GetValue(to, null);
                if (prop.PropertyType.IsPrimitive) {
                    //Assert.AreNotEqual()
                    UnityEngine.Assertions.Assert.AreEqual(selfValue, toValue,$"{typeof(T)}.{prop.PropertyType.GetSimpleName()} same?");
                } else {
                    AssertCompareObjs(selfValue, toValue, $"{prefix}{typeof(T)}.{prop.PropertyType.GetSimpleName()}.");
                }
            }

            foreach (var fi in type.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)) {
                var prop = type.GetField(fi.Name);
                object selfValue = type.GetField(fi.Name).GetValue(self);
                object toValue = type.GetField(fi.Name).GetValue(to);
                var fieldType = prop.FieldType;
                if (fieldType.IsPrimitive) {
                    //Assert.AreNotEqual()
                    UnityEngine.Assertions.Assert.AreEqual(selfValue, toValue, $"{typeof(T)}.{prop.FieldType.GetSimpleName()} same?");
                } 
                else if (fieldType.IsAssignableFrom(typeIList)) {
                    IList selfList = (IList)selfValue;
                    IList ToList = (IList)toValue;
                    for (int i = 0; i < selfList.Count; i++) {
                        Type innerType = selfList[i].GetType();
                        if (innerType.IsPrimitive) { 
                        }
                        AssertCompareObjs(selfList[i], ToList[i], $"{typeof(T)}.{prop.FieldType.GetSimpleName()}");
                    }
                }
                else if (fieldType.IsAssignableFrom(typeIDictionary)) {
                    IDictionary selfList = (IDictionary)selfValue;
                    IDictionary ToList = (IDictionary)toValue;
                    for (int i = 0; i < selfList.Count; i++) {
                        AssertCompareObjs(selfList[i], ToList[i], $"{typeof(T)}.{prop.FieldType.GetSimpleName()}");
                    }
                }  
                else {
                    AssertCompareObjs(selfValue, toValue);
                }
            }
        }
    }
}*/

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

    /// <summary>
    /// Logs Error in UNITY_EDITOR or Warning in BUILD
    /// </summary>
    /// <param name="output"></param>
    /// <param name="prefixGameTime"></param>
    public static void LogEditorErrBuildWarn(String output) {
#if UNITY_EDITOR
        Debug.LogError(output);
#else
        Debug.LogWarning(output);
#endif
    }
}

public class PoolList<T> where T : new()
{
    private List<T> innerList;

    // insane pooling of gridconnections. (sry)
    private bool inConnectionMode = false;
    private int connectionModeIdx = 0;
    private List<T> pool = new List<T>();
    private int maxPoolSize = 10;

    public PoolList(List<T> externalList = null) {
        innerList = externalList ?? new List<T>();
    }

    /// <summary>
    /// Start adding and overwriting new elements in list
    /// </summary>
    public void BeginListAssignment() {
        inConnectionMode = true;
        connectionModeIdx = 0;
    }

    /// <summary>
    /// Clear all pool-elements
    /// </summary>
    public void ClearPool() {
        pool.Clear();
    }

    /// <summary>
    /// Clear all (list-data and pool)
    /// </summary>
    public void ClearAll() {
        pool.Clear();
        innerList.Clear();
    }

    public List<T> InnerList {
        get => innerList;
    }

    /// <summary>
    /// Set max size for inner pool(default 10)
    /// </summary>
    /// <param name="size"></param>
    public void SetMaxPoolSize(int size) {
        maxPoolSize = size;
    }

    /// <summary>
    /// Finished writing to list => elements that are extra go to the pool
    /// </summary>
    public void FinishListAssignment() {
        int amount = innerList.Count;
        // release the rest
        for (int i = amount - 1; i >= connectionModeIdx; i--) {
            var releaseConnection = innerList[i];
            if (pool.Count < maxPoolSize) pool.Add(releaseConnection);
            innerList.RemoveAt(i);
        }
        inConnectionMode = false;
    }

    /// <summary>
    /// Amount of elements in this pooled list
    /// </summary>
    public int Count {
        get => innerList.Count;
    }

    public bool Remove(T obj) {
        bool found = innerList.Remove(obj);
        if (found && pool.Count < maxPoolSize) pool.Add(obj);
        return found;
    }

    /// <summary>
    /// Get one element. CAUTION: This data needs to be overwritten completely!!!!
    /// </summary>
    /// <returns></returns>
    public T RetrieveElement() {
        if (!inConnectionMode) {
            Debug.LogError("You need to start connection mode");
            return default(T);
        }
        if (innerList.Count > connectionModeIdx) {
            return innerList[connectionModeIdx++];
        } else {
            connectionModeIdx++;
            if (pool.Count > 0) {
                var poolElem = pool[0];
                pool.RemoveAt(0);
                innerList.Add(poolElem);
                return poolElem;
            } else {
                var newElem = new T();
                innerList.Add(newElem);
                return newElem;
            }
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
            T result = (T)pool.Acquire();
            return result;
        } else {
            // no pool for this type? create one
             
            // try to receive a custom creator that was registered for this type
            customCreators.TryGetValue(typeof(T), out Func<object> createFunc);
            if (createFunc == null) {
                createFunc = () => { return new T(); };
            }

            SimplePool<object> newPool = new SimplePool<object>(createFunc, maxElementsOnStack);

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
            object newObj = newPool.Acquire();
            return (T)newObj;
        }
    }

    public void ClearAndRelease<T>(List<T> clearReleaseList) {
        if (pools.TryGetValue(typeof(T), out SimplePool<object> pool)) {
            for (int i = clearReleaseList.Count - 1; i >= 0; i--) {
                pool.Release(clearReleaseList[i]);
            }
            clearReleaseList.Clear();
        }
    }

    public void Release(params object[] releaseObjs) {
        for (int i = releaseObjs.Length - 1; i >= 0; i--) {
            object releaseObj = releaseObjs[i];
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
        foreach (SimplePool<object> pool in pools.Values) {
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
            List<T> listElem = new List<T>(initialCapacity);
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
                List<T> list = releasedList[i];
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


public abstract class SimplePoolDisposable {
    public enum KeepTrackMode : int {
        None = 0, RefCounted = 1, WeakReferenced = 2
    }

    public abstract void Dispose(bool includingAcquiredObjects);
};
public class SimplePool<T> : SimplePoolDisposable where T : class {

    public static List<SimplePoolDisposable> allPools = new List<SimplePoolDisposable>();

    public static void DisposeAll() {
        for (int i = allPools.Count - 1; i >= 0; i--) {
            allPools[i].Dispose(true);
        }
    }

    // keep the available objects in a stack
    readonly Stack<T> _stack = new Stack<T>();
    public Func<T> createFunc;
    public Action<T> onAcquire;
    public Action<T> onRelease;
    public Action<T> onDispose;

    /// <summary>
    /// Objects that are acquired by the user
    /// </summary>
    readonly List<WeakReference<T>> acquiredObjectsWeak;
    readonly SimplePool<WeakReference<T>> weakRefPool;

    readonly List<T> acquiredObjectsRef;

    readonly KeepTrackMode keepTrackMode;

    int discardObjectCount = -1;

    Func<T> _defaultCreate = null;

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
    public SimplePool(Func<T> createFunc=null,int maxObjectsOnStack=-1, KeepTrackMode keepTrackMode = KeepTrackMode.None,Action<T> onAcquire=null,Action<T> onRelease = null,Action<T> onDispose=null) {
        allPools.Add(this);

        if (createFunc == null) {
            if (_defaultCreate == null) {
                _defaultCreate = () => {
                    return (T)Activator.CreateInstance(typeof(T));
                };
            }
            this.createFunc = _defaultCreate;
        } else {
            this.createFunc = createFunc;
        }
        this.onAcquire = onAcquire;
        this.onRelease = onRelease;
        this.onDispose = onDispose;
        SetDiscardObjectCount(maxObjectsOnStack);
        this.keepTrackMode = keepTrackMode;
        if (keepTrackMode == KeepTrackMode.WeakReferenced) {
            acquiredObjectsWeak = new List<WeakReference<T>>();
            weakRefPool = new SimplePool<WeakReference<T>>(()=> { return new WeakReference<T>(null); });
        }
        else if (keepTrackMode == KeepTrackMode.RefCounted) {
            acquiredObjectsRef = new List<T>();
        }
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
        if (keepTrackMode > 0) {
            if (keepTrackMode == KeepTrackMode.WeakReferenced) {
                WeakReference<T> wr = weakRefPool.Acquire();
                wr.SetTarget(obj);
                acquiredObjectsWeak.Add(wr);
            } else if (keepTrackMode == KeepTrackMode.RefCounted) {
                acquiredObjectsRef.Add(obj);
            }
        }

        return obj;
    }

    /// <summary>
    /// Release the object to the pool again so that is can be reused
    /// CAUTIOUS: make sure no reference to this object remain. using released objects will lead to severe problems
    /// </summary>
    /// <param name="obj"></param>
    public void Release(T obj) {
        if (onRelease != null) {
            onRelease(obj);
        } else {
            if (obj is IList) {
                ((IList)obj).Clear();
            }
        }

        if (keepTrackMode == KeepTrackMode.RefCounted) {
            acquiredObjectsRef.Remove(obj);
        }
        else if (keepTrackMode == KeepTrackMode.WeakReferenced) {
            // remove the current and all weak pointers which got invalid
            for (int i = acquiredObjectsWeak.Count - 1; i >= 0; i--) {
                WeakReference<T> wr = acquiredObjectsWeak[i];
                if (wr.TryGetTarget(out T target) && target == obj) {
                    wr.SetTarget(null);
                    weakRefPool.Release(wr);
                    acquiredObjectsRef.RemoveAt(i);
                    break;
                }
            }
        }

        _Release(obj);
    }

    private void _Release(T obj) {
        if (discardObjectCount == -1 || _stack.Count < discardObjectCount) {
            _stack.Push(obj);
        } else if (onDispose != null) {
            // we exceeded the stack amount therefore let the GC do what it have to do
            onDispose(obj);
        }
    }

    
    /// <summary>
    /// Release all acquired objects. 
    /// CAUTIOUS: make sure no reference to this objects remain. using released objects will lead to severe problems
    /// </summary>
    public void ReleaseAcquired() {
        if (keepTrackMode == KeepTrackMode.None) {
            Debug.LogError("Using release acquired! But using this pool in keepingTrack-Mode!");
            return;
        }
#if UNITY_EDITOR
        int releasedObjectsCount = 0;
#endif        
        if (keepTrackMode == KeepTrackMode.RefCounted) {
            for (int i = acquiredObjectsRef.Count - 1; i >= 0; i--) {
                T obj = acquiredObjectsRef[i];
                if (onRelease != null) {
                    onRelease(obj);
                }
                _Release(obj);
#if UNITY_EDITOR
                releasedObjectsCount++;
#endif
            }
            acquiredObjectsRef.Clear();
        } else if (keepTrackMode == KeepTrackMode.WeakReferenced) {
            for (int i = acquiredObjectsWeak.Count - 1; i >= 0; i--) {
                WeakReference<T> wr = acquiredObjectsWeak[i];
                if (wr.TryGetTarget(out T target)) {
                    _Release(target);
                }
                wr.SetTarget(null);
                weakRefPool.Release(wr);
#if UNITY_EDITOR
                releasedObjectsCount++;
#endif
            }
            acquiredObjectsRef.Clear();
        }
#if UNITY_EDITOR
        //Debug.Log("ReleaseAcquired:" + releasedObjectsCount);
#endif

    }

    /// <summary>
    /// Call the onDispose action on all objects still in the stack
    /// 
    /// </summary>
    /// <param name="includingAcquiredObjects"></param>
    public override void Dispose(bool includingAcquiredObjects) {
        if (onDispose != null) {
            while (_stack.Count > 0) {
                T obj = _stack.Pop();
                onDispose(obj);
            }

            if (keepTrackMode == KeepTrackMode.WeakReferenced) {
                foreach (WeakReference<T> weakRef in acquiredObjectsWeak) {
                    if (weakRef.TryGetTarget(out T obj)){
                        onDispose(obj);
                    }
                }
                while (_stack.Count > 0) {
                    T obj = _stack.Pop();
                    onDispose(obj);
                }

            }
            else if (keepTrackMode == KeepTrackMode.WeakReferenced) {
                foreach (T obj in acquiredObjectsRef) {
                    onDispose(obj);
                }
                while (_stack.Count > 0) {
                    T obj = _stack.Pop();
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
            T result = func();
            observer.OnNext(result);
            observer.OnCompleted();
            return null;
        });
    }

    public static float NextFloat(this System.Random rand,float from,float to) {
        float r = rand.Next((int)(from * 1000), (int)(to * 1000)) / 1000.0f;
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

        foreach (KeyValuePair<T, S> elem in data) {
            dest[elem.Key] = elem.Value;
        }
    }

    public static void ShallowCopyFrom<T, S>(this ObservableDictionary<T,S> dest, ObservableDictionary<T, S> data) {
        int amount = data.Count;

        foreach (KeyValuePair<T, S> elem in data) {
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
        foreach (T elem in data) {
            dest.Add(elem);
        }
    }

}

