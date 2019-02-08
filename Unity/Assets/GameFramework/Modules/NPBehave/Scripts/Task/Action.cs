using UnityEngine.Assertions;

namespace NPBehave
{
    public class NPAction : Task
    {
        public enum Result
        {
            SUCCESS,
            FAILED,
            BLOCKED,
            PROGRESS
        }

        public enum Request
        {
            START,
            UPDATE,
            CANCEL,
        }

        private System.Func<Root,bool> singleFrameFunc = null;
        private System.Func<Root,bool, Result> multiFrameFunc = null;
        private System.Func<Root,Request, Result> multiFrameFunc2 = null;
        private System.Action<Root> action = null;
        private bool bWasBlocked = false;

        public NPAction(System.Action<Root> action) : base("Action")
        {
            this.action = action;
        }

        public NPAction(System.Func<Root,bool, Result> multiframeFunc) : base("Action")
        {
            this.multiFrameFunc = multiframeFunc;
        }

        public NPAction(System.Func<Root,Request, Result> multiframeFunc2) : base("Action")
        {
            this.multiFrameFunc2 = multiframeFunc2;
        }


        public NPAction(System.Func<Root,bool> singleFrameFunc) : base("Action")
        {
            this.singleFrameFunc = singleFrameFunc;
        }

        protected override void DoStart()
        {
            if (this.action != null)
            {
                this.action.Invoke(RootNode);
                this.Stopped(true);
            }
            else if (this.multiFrameFunc != null)
            {
                Result result = this.multiFrameFunc.Invoke(RootNode,false);
                if ( result == Result.PROGRESS )
                {
                    this.RootNode.Clock.AddUpdateObserver( OnUpdateFunc );
                }
                else if ( result == Result.BLOCKED )
                {
                    this.bWasBlocked = true;
                    this.RootNode.Clock.AddUpdateObserver( OnUpdateFunc );
                }
                else
                {
                    this.Stopped(result == Result.SUCCESS);
                }
            }
            else if (this.multiFrameFunc2 != null)
            {
                Result result = this.multiFrameFunc2.Invoke(RootNode,Request.START);
                if (result == Result.PROGRESS)
                {
                    this.RootNode.Clock.AddUpdateObserver(OnUpdateFunc2);
                }
                else if ( result == Result.BLOCKED )
                {
                    this.bWasBlocked = true;
                    this.RootNode.Clock.AddUpdateObserver( OnUpdateFunc2 );
                }
                else
                {
                    this.Stopped(result == Result.SUCCESS);
                }
            }
            else if (this.singleFrameFunc != null)
            {
                this.Stopped(this.singleFrameFunc.Invoke(RootNode));
            }
        }

        private void OnUpdateFunc()
        {
            Result result = this.multiFrameFunc.Invoke(RootNode,false);
            if (result != Result.PROGRESS && result != Result.BLOCKED)
            {
                this.RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc);
                this.Stopped(result == Result.SUCCESS);
            }
        }

        private void OnUpdateFunc2()
        {
            Result result = this.multiFrameFunc2.Invoke( RootNode,bWasBlocked ? Request.START : Request.UPDATE);

            if ( result == Result.BLOCKED )
            {
                bWasBlocked = true;
            }
            else if ( result == Result.PROGRESS )
            {
                bWasBlocked = false;
            }
            else
            {
                this.RootNode.Clock.RemoveUpdateObserver( OnUpdateFunc2 );
                this.Stopped( result == Result.SUCCESS );
            }
        }

        protected override void DoStop()
        {
            if (this.multiFrameFunc != null)
            {
                Result result = this.multiFrameFunc.Invoke(RootNode,true);
                Assert.AreNotEqual(result, Result.PROGRESS, "The Task has to return Result.SUCCESS, Result.FAILED/BLOCKED after beeing cancelled!");
                this.RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc);
                this.Stopped(result == Result.SUCCESS);
            }
            else if (this.multiFrameFunc2 != null)
            {
                Result result = this.multiFrameFunc2.Invoke(RootNode,Request.CANCEL);
                Assert.AreNotEqual(result, Result.PROGRESS, "The Task has to return Result.SUCCESS or Result.FAILED/BLOCKED after beeing cancelled!");
                this.RootNode.Clock.RemoveUpdateObserver(OnUpdateFunc2);
                this.Stopped(result == Result.SUCCESS);
            }
            else
            {
                Assert.IsTrue(false, "DoStop called for a single frame action on " + this);
            }
        }
    }
}