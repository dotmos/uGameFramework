using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using Zenject;

public interface IExecutionWrapper
{
    IObservable<T> Wrap<T>(IObservable<T> input);
    IObservable<T> Wrap<T>(Func<T> input);
    IObservable<bool> Wrap(Action input);
}

public class DefaultExecutionWrapper : IExecutionWrapper
{
    public IObservable<T> Wrap<T>(IObservable<T> input) {
        return input;
    }

    public IObservable<T> Wrap<T>(Func<T> input) {
        return Wrap(Observable.Create<T>((observer) => {
            T result = input();
            observer.OnNext(result);
            observer.OnCompleted();
            return null;
        }));
    }

    public IObservable<bool> Wrap(Action input) {
        return Wrap(Observable.Create<bool>((observer) => {
            input();
            observer.OnNext(true);
            observer.OnCompleted();
            return null;
        }));
    }
}


/// <summary>
/// Some priorities default priorities
/// </summary>
public partial class Priorities {
    public const int PRIORITY_EARLY = 1024;
    public const int PRIORITY_DEFAULT = 512;
    public const int PRIORITY_LATE = 128;
}

public class PriorityList : IDisposable
{
         
    public PriorityList() {
        Kernel.Instance.Inject(this);
    }


    public class PriorityListElement
    {
        /// <summary>
        /// The logic returning an IObservable<bool> that emits true once it finished its work
        /// </summary>
        public IObservable<bool> call;
        /// <summary>
        /// The priority of this call (the higher the number the higher the priority
        /// </summary>
        public int priority = 0;

        public static readonly StaticMemoryPool<PriorityListElement> Pool = new StaticMemoryPool<PriorityListElement>();
    }

    /// <summary>
    /// The reactive collection of the sorted list, showing all currently running ListElements
    /// </summary>
    private ReactiveCollection<PriorityListElement> executingElements = null;

    /// <summary>
    /// Alter the Execution of the list-element (e.g. by adding before and after a select that starts/stops a clock or check for true/false values...using a default-wrapper(indentity) here
    /// </summary>
  //  private Func<IObservable<bool>, IObservable<bool>> executionWrapper = new Func<IObservable<bool>, IObservable<bool>>(rxIn => { return rxIn; });

    [Inject]
    IExecutionWrapper executionWrapper;

    /// <summary>
    /// The list of all ListElements cached before the list is executed
    /// </summary>
    private List<PriorityListElement> elemCache = new List<PriorityListElement>();
    
    public void QueueElement(IObservable<bool> call, int priority=Priorities.PRIORITY_DEFAULT) {
        // get a priority-list element from the pool
        var priorityListElem = PriorityListElement.Pool.Spawn();
        // set the data
        priorityListElem.call = call;
        priorityListElem.priority = priority;
        elemCache.Add(priorityListElem);
    }

    public void QueueElement(Action call, int priority = Priorities.PRIORITY_DEFAULT) {
        // get a priority-list element from the pool
        var priorityListElem = PriorityListElement.Pool.Spawn();
        // set the data
        priorityListElem.call = executionWrapper.Wrap(call);
        priorityListElem.priority = priority;
        elemCache.Add(priorityListElem);
    }

    public IObservable<bool> RxExecute() {
        var sortedList = elemCache.OrderByDescending(elem => elem.priority).ToList();
        executingElements = new ReactiveCollection<PriorityListElement>(sortedList);

        elemCache.Clear();

        // the last observable of the rx-chain
        IObservable<bool> currentObservable = null;

        // as long as there are elements in the priority list
        while (sortedList.Count>0) {
            // start with the highest priority of the list
            int currentPriority = sortedList[0].priority;

            List<IObservable<bool>> currentList = new List<IObservable<bool>>();
            while (sortedList.Count>0 && currentPriority==sortedList[0].priority) {
                // use the execution wrapper to add some logic before/after the call or just keep the default wrapper, that
                // just return the IObservable<bool> itself
                var currentElem = sortedList[0];
                var execution = currentElem.call
                        .Take(1) // exactly one element is expected and accepted.
                        .Select(result => {
                            executingElements.Remove(currentElem);
                            return result;
                        }); // remove this element from the execution list
                currentList.Add(execution);
                sortedList.RemoveAt(0);
            }
            
            if (currentList.Count > 0) {
                var parallelExecution = Observable.Merge(currentList)
                                                  .Last(); // only propagate element the last element (that means: no matter if the single listElements returned true/false it will go on
                                                  
                if (currentObservable == null) {
                    currentObservable = parallelExecution;
                } else {
                    // start with the new execution once all parallel observables are executed
                    currentObservable = currentObservable.Concat(parallelExecution);
                }
            }
        }

        return currentObservable;
    }

    public void Clear() {
        if (executingElements != null) {
            executingElements.Clear();
            executingElements.Dispose();
            executingElements = null;
        }
    }

    public void Dispose() {
        Clear();
    }
}

public interface IExecutor {
    IObservable<bool> Call(Action act);
}

