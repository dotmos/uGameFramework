using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ECS {
    public class Future<T> {
        private Semaphore semaphore = new Semaphore(0, 1);

        private Func<T> logic;
        private T result;
        private bool finished = false;

        public Future(Func<T> logic) {
            this.logic = logic;
        }

        public bool IsFinished() {
            return finished;
        }

        public void execute() {
            result = logic();
            finished = true;
            semaphore.Release();
        }

        public T WaitForResult() {
            if (!finished) {
                semaphore.WaitOne();
            }
            return result;
        }
    }
}
