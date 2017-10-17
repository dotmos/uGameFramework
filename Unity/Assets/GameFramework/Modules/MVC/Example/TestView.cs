using UnityEngine;
using System.Collections;
using Zenject;
using UniRx;
using MVC;

public class TestView : View<TestController> {

    protected override void PreBind()
    {
        base.PreBind();
    }

    protected override void AfterBind()
    {
        base.AfterBind();

        //Listen to controller event
        OnController<TestController.ControllerEvents.SomeControllerEvent>().Subscribe(e => Debug.Log(this.ToString() +": Response from controller: "+e.someString)).AddTo(this);

        //Send event from view to controller
        PublishToController(new TestController.ViewEvents.SomeViewEvent(){someString = "it works!"});


        Debug.Log("-----------------------------------------------------------");

        Debug.Log(Model.Serialize());

        Model.Deserialize(" {\"SomeString\":\"someString\",\"SomeInt\":42,\"SomeStringProperty\":{\"Value\":\"someString\"},\"SomeIntProperty\":{\"Value\":42},\"someClass\":{\"aString\":\"lolol\",\"SomeStringProperty\":{\"Value\":\"lolol\"},\"anInt\":43},\"stringList\":[\"one\",\"two\",\"three\"],\"someClassList\":[{\"aString\":\"omg\",\"SomeStringProperty\":{\"Value\":\"omg\"},\"anInt\":42}],\"stringArray\":[\"a\",\"b\",\"c\"]} ");
        Debug.Log(Model.Serialize());

        Model.Deserialize(" {\"SomeString\":null,\"SomeInt\":0,\"SomeStringProperty\":{\"Value\":null},\"SomeIntProperty\":{\"Value\":0},\"someClass\":null,\"stringList\":null,\"someClassList\":null,\"stringArray\":null} ");

        Debug.Log(Model.Serialize());
    }

    TestController.ViewEvents.SomeViewEvent someViewEvent = new TestController.ViewEvents.SomeViewEvent();
    public override void Tick()
    {
        //Send current game time to controller
        if(Input.GetMouseButtonDown(0))
        {
            someViewEvent.someString = Time.time.ToString();
            PublishToController(someViewEvent);
        }
    }
}