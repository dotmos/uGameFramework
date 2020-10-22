using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using FlatBuffers;
using System;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;

namespace Service.PerformanceTest {

    public partial class PerformanceTestServiceImpl : PerformanceTestServiceBase {

        // sry for this hack, but I need to bypass Zenject in this case
        public static PerformanceTestServiceImpl instance;


        private static ConcurrentDictionary<Type, int> instanceCount = new ConcurrentDictionary<Type, int>();

        public class PerfTestData {
            public System.Diagnostics.Stopwatch watch;
            public int calls;
            public void init() {
                watch = new System.Diagnostics.Stopwatch();
                calls = 0;
            }
        }
        public ConcurrentDictionary<String, PerfTestData> perfTestStopwatches = new ConcurrentDictionary<String, PerfTestData>();

        protected override void AfterInitialize() {
            instanceCount.Clear();
            instance = this;
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
#if LEAK_DETECTION            
            rxOnStartup.Add(() => {
                var devui = Kernel.Instance.Resolve<Service.DevUIService.IDevUIService>();
                var view = devui.CreateView("leak");
                view.AddElement(new DevUIService.DevUIButton("GC.Collect()", () => { GC.Collect(); }));

                view.AddElement(new DevUIService.DevUIButton("Show instance-counts",()=> {
                    OutputInstanceViewToLog();
                }));

            });
#endif
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

        public override void AddInstance(object o) {
            Type type = o.GetType();
            instanceCount.TryGetValue(type, out int currentAmount);
            instanceCount[type] = currentAmount + 1;
        }

        public override void RemoveInstance(object o) {
            Type type = o.GetType();
            instanceCount.TryGetValue(type, out int currentAmount);
            instanceCount[type] = currentAmount - 1;
            if (instanceCount[type] < 0) {
                UnityEngine.Debug.LogWarning($"Something went wrong with leak-detection! You removed more Instances that you put in before! Type: [{type}] ");
            }
        }

        public override int GetInstanceCount(Type instanceType) {
            if (instanceCount.TryGetValue(instanceType,out int currentAmount)) {
                return currentAmount;
            }
            return -1;
        }

        public override ConcurrentDictionary<Type, int> GetInstanceView() {
            return instanceCount;
        }

        public override void OutputInstanceViewToLog(bool gccollect=true) {
            if (gccollect) {
                System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
            }

            StringBuilder stb = new StringBuilder("LEAK_INSTANCE_CHECK: ");
            stb.Append("GC-Total-Memory:").Append(GC.GetTotalMemory(false)).Append("\n\n");
            foreach (var kv in instanceCount.OrderByDescending(data => data.Value)) {
                stb.Append(kv.Key).Append(" : ").Append(kv.Value).Append('\n');
            }
            UnityEngine.Debug.LogWarning(stb.ToString());
        }

    }



}
