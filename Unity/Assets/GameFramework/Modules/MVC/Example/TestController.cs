using System.Collections;
using MVC;
using Zenject;
using UniRx;

public class TestController : Controller<TestModel>{

    public class ControllerEvents{
        public class SomeControllerEvent{
            public string someString;
        }
    }
    public class ViewEvents{
        public class SomeViewEvent{
            public string someString;
        }
    }

    ControllerEvents.SomeControllerEvent someControllerEvent = new ControllerEvents.SomeControllerEvent();
    protected override void AfterBind()
    {
        base.AfterBind();

        //Listen to an event from the view(s)
        OnView<ViewEvents.SomeViewEvent>().Subscribe(e => OnSomeViewEvent(e)).AddTo(this);
    }

    void OnSomeViewEvent(ViewEvents.SomeViewEvent evt)
    {
        UnityEngine.Debug.Log("View fired some event:"+evt.someString);

        //Fire back to view(s)
        someControllerEvent.someString = evt.someString;
        PublishToViews(someControllerEvent);
    }

    public override void OnDispose()
    {
    }
}