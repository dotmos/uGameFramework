using UnityEngine;
using System.Collections;
using UniRx;

public class ReactiveStateMachineTest : MonoBehaviour {

    public enum Triggers{
        TriggerA,
        TriggerB,
        TriggerC
    }

    public enum States{
        StateA,
        StateB
    }

    public interface IState{
    }
    class StateA : IState{
        public string aString ="mööh";

        public void OnEnter()
        {
            Debug.Log("OnEnter for StateA! :)");
        }

        public void OnExit()
        {
            Debug.Log("OnExit for StateA! :)");
        }
    }
    class StateB : IState{
        public int anInt = 4243;


        public void OnEnter()
        {
            Debug.Log("OnEnter for StateB! :) We don't have OnExit defined for StateB, so it will not be invoked");
        }
    }



    void Start()
    {
        StateA stateA = new StateA();
        StateB stateB = new StateB();

        ReactiveStateMachine<Triggers, IState> rsm;
        rsm = new ReactiveStateMachine<Triggers, IState>();

        rsm.OnEnter(stateA).Subscribe(e => Debug.Log(((StateA)e).aString)).AddTo(this);
        rsm.OnEnter(stateB).Subscribe(e => Debug.Log(((StateB)e).anInt)).AddTo(this);

        rsm.AddTransition(stateA, Triggers.TriggerB, stateB);
        rsm.AddTransition(stateB, Triggers.TriggerA, stateA);

        rsm.Start(stateA);

        rsm.Trigger(Triggers.TriggerA);
        rsm.Trigger(Triggers.TriggerB);
        rsm.Trigger(Triggers.TriggerC); //should not do anything
        rsm.Trigger(Triggers.TriggerB); //should not do anything
        rsm.Trigger(Triggers.TriggerA);



        /*
        ReactiveStateMachine<Triggers, States> rsm2;
        rsm2 = new ReactiveStateMachine<Triggers, States>();

        rsm2.AddTransition(States.StateA, Triggers.TriggerB, States.StateB);
        rsm2.AddTransition(States.StateB, Triggers.TriggerA, States.StateA);

        rsm2.OnEnter(States.StateA).Subscribe(e => Debug.Log(e + " OnEnter"));
        rsm2.OnExit(States.StateA).Subscribe(e => Debug.Log(e + " OnExit"));

        rsm2.OnEnter(States.StateB).Subscribe(e => Debug.Log(e + " OnEnter"));
        rsm2.OnExit(States.StateB).Subscribe(e => Debug.Log(e + " OnExit"));

        rsm2.Start(States.StateB);

        rsm2.Trigger(Triggers.TriggerA);
        rsm2.Trigger(Triggers.TriggerB);
        rsm2.Trigger(Triggers.TriggerC); //should not do anything
        rsm2.Trigger(Triggers.TriggerB); //should not do anything
        rsm2.Trigger(Triggers.TriggerA);
*/


    }
}
