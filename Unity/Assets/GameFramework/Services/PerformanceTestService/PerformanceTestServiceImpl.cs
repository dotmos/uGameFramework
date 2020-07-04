using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using FlatBuffers;
using System;
using System.Text;

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
                PerfTestData newData = new PerfTestData();
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

        public override string PerfTestOutputAsString() {
            StringBuilder stb = new StringBuilder();
            stb.Append("PerfTest:\n-----------------------------------");
            foreach (KeyValuePair<string, PerfTestData> pTest in perfTestStopwatches) {
                if (pTest.Value.calls == 0) {
                    continue;
                }
                float average = pTest.Value.watch.ElapsedMilliseconds / pTest.Value.calls;
                stb.Append(pTest.Key + ": overall:" + pTest.Value.watch.ElapsedMilliseconds + "ms calls:" + pTest.Value.calls + " average:" + average + "ms");
            }
            stb.Append("----------------------------------\n");
            return stb.ToString();
        }

        public override void PerfTestOutputToConsole() {
            UnityEngine.Debug.Log(PerfTestOutputAsString());
        }

        public override void Clear() {
            perfTestStopwatches.Clear();
        }

    }



}
