using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace UniRx {


        /// <summary>
        /// This observable takes two IObservable<bool> that do check something and compare everytime
        /// one observable emits a new value using the cached value of the other to compare with.
        /// 
        /// The CachedBoolObservalbe itself emits the bool comparison between both values (AND or OR)
        /// 
        /// e.g. IObservable
        /// </summary>
    class CachedBoolObservable : IObservable<bool>, IDisposable {

        CompositeDisposable disposables = new CompositeDisposable();

        public enum CompareOperation {
            AND, OR
        }


        IObservable<bool> first = null;
        IObservable<bool> second = null;
        bool currentValueFirst = false;
        bool currentValueLast = false;

        bool completedFirst = false;
        bool completedLast = false;

        IObserver<bool> observer;
        CompareOperation op;

        public CachedBoolObservable(IObservable<bool> first, IObservable<bool> second, CompareOperation op) {
            this.first = first;
            this.second = second;
            this.op = op;
        }

        private void Check() {

                if (this.op == CompareOperation.AND && currentValueFirst && currentValueLast) {
                    observer.OnNext(true);
                } else if (this.op == CompareOperation.OR && (currentValueFirst || currentValueLast)) {
                    observer.OnNext(true);
                } 
                else observer.OnNext(false);
        }

        private void FirstCompleted() {
            completedFirst = true;
            if (completedFirst && completedLast) {
                observer.OnCompleted();
            }
        }

        private void SecondCompleted() {
            completedLast = true;
            if (completedFirst && completedLast) {
                observer.OnCompleted();
            }
        }

        public IDisposable Subscribe(IObserver<bool> observer) {
            this.observer = observer;

            bool initial = true;

            disposables.Add(first.Subscribe(val => {
                currentValueFirst = val;
                if (!initial) {
                    Check();
                } else {
                    initial = false;
                    // postpone the first call to the next frame to wait for the initial call of the other element
                    Observable.NextFrame().Take(1).Subscribe(_ => Check());
                }
            }, err => { observer.OnError(err); }, FirstCompleted));
            disposables.Add( second.Subscribe(val => {
                currentValueLast = val;
                if (!initial) {
                    Check();
                } else {
                    initial = false;
                    // postpone the first call to the next frame to wait for the initial call of the other element
                    Observable.NextFrame().Take(1).Subscribe(_ => Check());
                }
            }, err => { observer.OnError(err); }, SecondCompleted));
            return this;
        }

        public static CachedBoolObservable myzip(IObservable<bool> first, IObservable<bool> second, CompareOperation op) {
            return new CachedBoolObservable(first, second, op);
        }

        public void Dispose() {
            disposables.Dispose();
            disposables = null;

        }

    }


    
    

}