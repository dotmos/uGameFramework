using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using Zenject;

/// <summary>
/// Some priorities default priorities
/// </summary>
public partial class Priorities {
    public const int PRIORITY_VERY_EARLY = 4000;
    public const int PRIORITY_EARLY = 2000;
    public const int PRIORITY_DEFAULT = 1000;
    public const int PRIORITY_LATE = 500;
    public const int PRIORITY_VERY_LATE = 100;
}

public class ReactivePriorityExecutionList : IDisposable
{
    private bool injected = false;
    

    public ReactivePriorityExecutionList() {
    }

    public interface IContext {
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
    /// Only allow to use action as Queue-Element to prevent long lasting Observables (e.g. in tick-queue)
    /// </summary>
    private bool actionOnly = false;

    /// <summary>
    /// Keep a list with all PriorityListElements on execution 
    /// </summary>
    private bool showExecutingElements = false;

    /// <summary>
    /// The reactive collection of the sorted list, showing all currently running ListElements
    /// </summary>
    private ReactiveCollection<PriorityListElement> executingElements = null;

    /// <summary>
    /// Alter the Execution of the list-element (e.g. by adding before and after a select that starts/stops a clock or check for true/false values...using a default-wrapper(indentity) here
    /// </summary>
  //  private Func<IObservable<bool>, IObservable<bool>> executionWrapper = new Func<IObservable<bool>, IObservable<bool>>(rxIn => { return rxIn; });

    [Inject]
    IReactiveExecutionWrapper executionWrapper;

    /// <summary>
    /// The list of all ListElements cached before the list is executed
    /// </summary>
    private List<PriorityListElement> elemCache = new List<PriorityListElement>();
    
    /// <summary>
    /// Cache the sorted elemCache
    /// </summary>
    private List<PriorityListElement> sortedList = new List<PriorityListElement>();

    /// <summary>
    /// Caches the priority list's IObservable-Chain
    /// </summary>
    private IObservable<bool> rxCurrent = Observable.Return(true);

    /// <summary>
    /// Did the list change till last process?
    /// </summary>
    private bool isDirty = false;

    /// <summary>
    /// Create prioritylist with optional flag to only allow actions as queue-element and to show the QueueElements 
    /// </summary>
    /// <param name="actionOnly"></param>
    public ReactivePriorityExecutionList(bool showExecutionElements=false, bool actionOnly = false) {
        this.actionOnly = true;
    }

    public PriorityListElement Add(IObservable<bool> call, int priority=Priorities.PRIORITY_DEFAULT) {
        if (!injected) {
            Kernel.Instance.Inject(this);
            injected = true;
        }

        if (actionOnly) {
            Debug.LogError("You tried to add IObservable queue-element to action-only priority-list (prio:" + priority+") skipping....");
            return null;
        }
        // get a priority-list element from the pool
        PriorityListElement priorityListElem = PriorityListElement.Pool.Spawn();
        // set the data
        priorityListElem.call = call;
        priorityListElem.priority = priority;
        elemCache.Add(priorityListElem);
        isDirty = true;
        return priorityListElem;
    }

    public PriorityListElement Add(Action call, int priority = Priorities.PRIORITY_DEFAULT) {
        if (!injected) {
            Kernel.Instance.Inject(this);
            injected = true;
        }

        // get a priority-list element from the pool
        PriorityListElement priorityListElem = PriorityListElement.Pool.Spawn();
        // set the data
        priorityListElem.call = executionWrapper.Wrap(call);
        priorityListElem.priority = priority;
        elemCache.Add(priorityListElem);
        isDirty = true;
        return priorityListElem;
    }

    /// <summary>
    /// Create an IObservable thate executes the logic sorted by priorities. Same priority is called parallel( Observable.Merge) and lower priority is <br/>
    /// executed later (Observable.Concat of Priority-Blocks)
    /// </summary>
    /// <returns></returns>
    public IObservable<bool> RxExecute() {
        if (!injected && Kernel.Instance!=null) {
            Kernel.Instance.Inject(this);
            injected = true;
        }

        if (!isDirty || rxCurrent==null) {
            // no changes => nothing to do!
            return rxCurrent;
        }

        // process the list


        sortedList = elemCache.OrderByDescending(elem => elem.priority).ToList();
        // let's use the sorted list as next elemCache. (TODO: Not sure about the sort-algorithm, but that might have some inpact)
        elemCache = new List<PriorityListElement>(sortedList);

        if (showExecutingElements) {
            executingElements = new ReactiveCollection<PriorityListElement>(elemCache);
        }

        // the last observable of the rx-chain
        rxCurrent = null;

        // as long as there are elements in the priority list
        while (sortedList.Count>0) {
            // start with the highest priority of the list
            int currentPriority = sortedList[0].priority;

            List<IObservable<bool>> currentList = new List<IObservable<bool>>();
            while (sortedList.Count>0 && currentPriority==sortedList[0].priority) {
                // use the execution wrapper to add some logic before/after the call or just keep the default wrapper, that
                // just return the IObservable<bool> itself
                PriorityListElement currentElem = sortedList[0];
                IObservable<bool> execution = currentElem.call
                        .Take(1) // exactly one element is expected and accepted.
                        .Select(result => {
                            if (executingElements != null) {
                                executingElements.Remove(currentElem);
                            }
                            return result;
                        }); // remove this element from the execution list
                currentList.Add(execution);
                sortedList.RemoveAt(0);
            }
            
            if (currentList.Count > 0) {
                // TODO: recognize what executions did not finish properly (returned false)
                IObservable<bool> parallelExecution = Observable.Merge(currentList)
                                                  .Last(); // only propagate element the last element (that means: no matter if the single listElements returned true/false it will go on
                                                  
                if (rxCurrent == null) {
                    rxCurrent = parallelExecution;
                } else {
                    // start with the new execution once all parallel observables are executed
                    rxCurrent = rxCurrent.Concat(parallelExecution);
                }
            }
        }
        isDirty = false;
        return rxCurrent.Last().Finally(()=> {
            if (executingElements != null) {
                ClearExecutionElements();
            }
        });
    }

    public void RemoveQueueElement(PriorityListElement elem) {
        elemCache.Remove(elem);
    }

    
    private void ClearExecutionElements() {
        if (executingElements != null) {
            executingElements.Clear();
            executingElements.Dispose();
            executingElements = null;
        }
    }

    /// <summary>
    /// Clear the list
    /// </summary>
    public void Clear() {
        ClearExecutionElements();

        if (elemCache != null) {
            foreach (PriorityListElement elem in elemCache) {
                PriorityListElement.Pool.Despawn(elem);
            }
            elemCache.Clear();
            isDirty = true;
        }

    }

    public void Dispose() {
        Clear();
    }
}

public interface IExecutor {
    IObservable<bool> Call(Action act);
}

