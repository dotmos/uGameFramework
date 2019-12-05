using System;

namespace UniRx.Operators
{
    internal class TimerObservable : OperatorObservableBase<long>
    {
        readonly DateTimeOffset? dueTimeA;
        readonly TimeSpan? dueTimeB;
        readonly TimeSpan? period;
        readonly IScheduler scheduler;

        public TimerObservable(DateTimeOffset dueTime, TimeSpan? period, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread)
        {
            this.dueTimeA = dueTime;
            this.period = period;
            this.scheduler = scheduler;
        }

        public TimerObservable(TimeSpan dueTime, TimeSpan? period, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread)
        {
            this.dueTimeB = dueTime;
            this.period = period;
            this.scheduler = scheduler;
        }

        protected override IDisposable SubscribeCore(IObserver<long> observer, IDisposable cancel)
        {
            Timer timerObserver = new Timer(observer, cancel);

            TimeSpan dueTime = (dueTimeA != null)
                ? dueTimeA.Value - scheduler.Now
                : dueTimeB.Value;

            // one-shot
            if (period == null)
            {
                return scheduler.Schedule(Scheduler.Normalize(dueTime), () =>
                {
                    timerObserver.OnNext();
                    timerObserver.OnCompleted();
                });
            }
            else
            {
                ISchedulerPeriodic periodicScheduler = scheduler as ISchedulerPeriodic;
                if (periodicScheduler != null)
                {
                    if (dueTime == period.Value)
                    {
                        // same(Observable.Interval), run periodic
                        return periodicScheduler.SchedulePeriodic(Scheduler.Normalize(dueTime), timerObserver.OnNext);
                    }
                    else
                    {
                        // Schedule Once + Scheudle Periodic
                        SerialDisposable disposable = new SerialDisposable();

                        disposable.Disposable = scheduler.Schedule(Scheduler.Normalize(dueTime), () =>
                        {
                            timerObserver.OnNext(); // run first

                            TimeSpan timeP = Scheduler.Normalize(period.Value);
                            disposable.Disposable = periodicScheduler.SchedulePeriodic(timeP, timerObserver.OnNext); // run periodic
                        });

                        return disposable;
                    }
                }
                else
                {
                    TimeSpan timeP = Scheduler.Normalize(period.Value);

                    return scheduler.Schedule(Scheduler.Normalize(dueTime), self =>
                    {
                        timerObserver.OnNext();
                        self(timeP);
                    });
                }
            }
        }

        class Timer : OperatorObserverBase<long, long>
        {
            long index = 0;

            public Timer(IObserver<long> observer, IDisposable cancel)
                : base(observer, cancel)
            {
            }

            public void OnNext()
            {
                try
                {
                    base.observer.OnNext(index++);
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public override void OnNext(long value)
            {
                // no use.
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                try { observer.OnCompleted(); }
                finally { Dispose(); }
            }
        }
    }
}