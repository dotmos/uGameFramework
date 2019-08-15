using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using FlatBuffers;
using System;

namespace Service.PerformanceTest {

    partial class PerformanceTestServiceImpl : PerformanceTestServiceBase {

        public class PerfTestData {
            public System.Diagnostics.Stopwatch watch;
            public int calls;
            public void init() {
                watch = new System.Diagnostics.Stopwatch();
                calls = 0;
            }
        }
        public Dictionary<String, PerfTestData> perfTestStopwatches = new Dictionary<String, PerfTestData>();

        protected override void AfterInitialize() {
			// this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
        }

        public override void StartWatch(String t) {
            if (perfTestStopwatches.TryGetValue(t, out PerfTestData data)) {
                data.watch.Start();
                data.calls++;
            } else {
                var newData = new PerfTestData();
                newData.init();
                perfTestStopwatches[t] = newData;
                newData.calls++;
                newData.watch.Start();
            }
        }
        public override void StopWatch(String t) {
            if (perfTestStopwatches.TryGetValue(t, out PerfTestData data)) {
                data.watch.Stop();
            } else {
                UnityEngine.Debug.LogError("Tried to stop a watch of a type, that didn't start at all:" + t);
            }
        }

        public override void PrintPerfTests() {
            UnityEngine.Debug.Log("PerfTest:\n-----------------------------------");
            foreach (var pTest in perfTestStopwatches) {
                if (pTest.Value.calls == 0) {
                    continue;
                }
                float average = pTest.Value.watch.ElapsedMilliseconds / pTest.Value.calls;
                UnityEngine.Debug.Log(pTest.Key + ": overall:" + pTest.Value.watch.ElapsedMilliseconds + "ms calls:" + pTest.Value.calls + " average:" + average + "ms");
            }
            UnityEngine.Debug.Log("----------------------------------\n");
        }

        public override void Clear() {
            perfTestStopwatches.Clear();
        }

    }



}
