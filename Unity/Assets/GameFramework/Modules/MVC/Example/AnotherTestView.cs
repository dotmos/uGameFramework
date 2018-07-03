using UnityEngine;
using System.Collections;
using Zenject;
using UniRx;
using MVC;

public class AnotherTestView : View<TestController> {

    public TestView testView;
    

    protected override void AfterBind()
    {
        base.AfterBind();

        //Once testView has a controller, also use it as controller for this view and subscribe to an event of the controller
        testView.ControllerProperty.Where(c => c != null).First().Subscribe(e => {
            SetController(e);
            //Listen to event from other testview controller
            OnController<TestController.ControllerEvents.SomeControllerEvent>().Subscribe(evt => Debug.Log(this.ToString() +" controller event: "+evt.someString)).AddTo(this);
        });
    }
}
