using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;

using Zenject;
using UniRx;
using UnityEngine;
using System.Text;

namespace Service.LoggingService {

    partial class LoggingServiceImpl : LoggingServiceBase {

        private const int MAX_LOGS = 200;

        /// <summary>
        /// The logging data.
        /// </summary>
        private List<LogData> loggingData = new List<LogData>();

        private static List<String> preInitializeLogs = new List<string>();

        private StringBuilder stb = new StringBuilder();

        //[RuntimeInitializeOnLoadMethod]
        //private static void OnUnityStart() {
        //    Application.logMessageReceived += PreAdd;
        //}

        static void PreAdd(string logString, string stackTrace, LogType type) {
            string newString = "[" + type + "] : " + logString + "\n";

            if (type == LogType.Exception) {
                newString += "\n" + stackTrace;
            }

            preInitializeLogs.Add(newString);
        }
        /// <summary>
        /// This is the current logs to be shows using all filters wanted
        /// </summary>
        public ReactiveCollection<LogData> rxOutputData = new ReactiveCollection<LogData>();

        public ReactiveProperty<LoggingFilter> currentFilterProperty = new ReactiveProperty<LoggingFilter>(new LoggingFilter());

        public LoggingFilter CurrentFilter {
            get { return currentFilterProperty.Value; }
            set { currentFilterProperty.Value = value; }
        }


        /// <summary>
        /// Temporary List for filtering action
        /// </summary>
        private List<LogData> tempList = new List<LogData>();

        protected override void AfterInitialize() {
            // this is called right after the Base-Classes Initialize-Method. _eventManager and disposableManager are set
            Application.logMessageReceived -= PreAdd;

            Application.logMessageReceived += HandleNativeLog;

            // write the native-logs from startup till service-afterinit
            foreach (var preLog in preInitializeLogs) {
                AddLog(DebugType.native, preLog);
            }
            preInitializeLogs.Clear();
        }

        

		//docs come here
		public override void AddLog(DebugType debugType,string message,string domain="") {
            UnityEngine.Profiling.Profiler.BeginSample("AddLog");
            var newLog = new LogData() {
                domain = domain,
                message = message,
                type = debugType
            };
            // always add the new data on top
            loggingData.Insert(0, newLog);

            // check if this log applies to the current filter
            if (CurrentFilter.Check(newLog)) {
                // add it to the reactive outputlog
                rxOutputData.Add(newLog);
            }
        }

		//docs come here
		public override void Info(string message,string domain="") {
            AddLog(DebugType.info, message, domain);
        }

		//docs come here
		public override void Warn(string message,string domain="") {
            AddLog(DebugType.warning, message, domain);
        }

		//docs come here
		public override void Error(string message,string domain="") {
            AddLog(DebugType.error, message, domain);
        }

		//docs come here
		public override void Severe(string message,string domain="") {
            AddLog(DebugType.severe, message, domain);
        }


        public override ReactiveCollection<LogData> GetRxOutputData() {
            return rxOutputData;
        }

        void HandleNativeLog(string logString, string stackTrace, LogType type) {
            try {
                UnityEngine.Profiling.Profiler.BeginSample("HandleNativeLog");
                stb.Clear();
                stb.Append("[").Append(type).Append("] : ").Append(logString).Append("\n");

                if (type == LogType.Exception) {
                    stb.Append("\n").Append(stackTrace);
                }

                AddLog(DebugType.native, stb.ToString());
            }
            finally {
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        private void ApplyFilter() {
            rxOutputData.Clear();

            tempList.Clear();

            int lines = 0;
            int idx = 0;
            int amount = loggingData.Count;
            while (lines < MAX_LOGS && idx < amount) {
                var logData = loggingData[idx];
                
                if (CurrentFilter.Check(logData)) {
                    tempList.Insert(0,logData);
                    lines++;
                }

                idx++;
            }

            // now finally add it to the outputData that fill fire the visual output as well
            // TODO: make this more efficient? 
            for (int i = 0; i < tempList.Count; i++) {
                rxOutputData.Add(tempList[i]);
            }
        }

 

        protected override void OnDispose() {
            // do your IDispose-actions here. It is called right after disposables got disposed

        }

    }



}
