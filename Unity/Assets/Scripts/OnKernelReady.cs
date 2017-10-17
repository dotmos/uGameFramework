using UnityEngine;
using UniRx;


public class OnKernelReady : GameComponent {

     protected override void AfterBind()
    {
        base.AfterBind();

        Observable.NextFrame().DelayFrame(1).Subscribe(e => { _OnKernelReady(); }).AddTo(this); ;
    }

    void _OnKernelReady(){
        Debug.Log("Kernel ready!");

        //Kernel is ready, you can "start" your game now
    }
}
