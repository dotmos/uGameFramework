using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using FlatBuffers;
using System;
using System.Text;
using System.Linq;

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
            try
            {
                stb.Append("PerfTest:\n-----------------------------------\n");
                foreach (KeyValuePair<string, PerfTestData> pTest in perfTestStopwatches.OrderByDescending(kv => kv.Value.watch.ElapsedMilliseconds))
                {
                    if (pTest.Value.calls == 0)
                    {
                        continue;
                    }
                    float average = (float)pTest.Value.watch.Elapsed.TotalSeconds / pTest.Value.calls;
                    stb.Append(pTest.Value.watch.Elapsed.TotalSeconds + "s overall " + pTest.Key + ": overall:" + "ms calls:" + pTest.Value.calls + " average:" + average + "s\n");
                }
                stb.Append("----------------------------------\n");
                return stb.ToString();
            }
            catch (Exception e) {
                stb.Append("PerfTestOutputAsString: Exception: " + e.Message + "\n" + e.StackTrace+"\n");
                return stb.ToString();
            }
        }

        public override void PerfTestOutputToConsole() {
            UnityEngine.Debug.Log(PerfTestOutputAsString());
        }

        public override void Clear() {
            perfTestStopwatches.Clear();
        }

    }



}
