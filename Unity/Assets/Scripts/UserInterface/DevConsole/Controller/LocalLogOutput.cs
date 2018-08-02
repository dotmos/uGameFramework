using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using UniRx;
using Service.LoggingService;

public class LocalLogOutput : GameComponent {

    [Inject]
    private Service.LoggingService.ILoggingService logging;

    public Transform logOutputContainer;
    public GameObject logOutputPrefab;
    /// <summary>
    /// After how many characters should the log be split into a new text box.
    /// </summary>
    public int maxCharacterAmount = 3000;

    private Text currentLogOutput;
    private int currentCharCount;

    string theLog;
    Queue logQueue = new Queue();

    protected override void AfterBind() {
        CreateNewLogOutput();
        logging.GetRxOutputData().ObserveAdd().Subscribe(evt => AddData(evt.Value)).AddTo(this);
    }

    private void AddData(LogData data) {
        currentLogOutput.text += data.ToString()+"\n";
    }

    //private void OnEnable() {
    //    Application.logMessageReceived += HandleLog;
    //}

    //void OnDisable() {
    //    Application.logMessageReceived -= HandleLog;
    //}

    /*  void HandleLog(string logString, string stackTrace, LogType type) {
          theLog = logString;

          //Add to current char count
          currentCharCount += theLog.Length;

          string newString = "[" + type + "] : " + theLog + "\n";
          logQueue.Enqueue(newString);

          if (type == LogType.Exception) {
              newString = "\n" + stackTrace;
              logQueue.Enqueue(newString);
          }

          foreach (string log in logQueue) {
              string[] subLogs = log.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

              foreach (string sublog in subLogs) {
                  currentLogOutput.text += sublog;

                  if (currentLogOutput.text.Length > maxCharacterAmount) {
                      CreateNewLogOutput();
                  }
              }
          }

          logQueue.Clear();
      }*/

    void CreateNewLogOutput() {
        GameObject logOutputGO = Instantiate(logOutputPrefab) as GameObject;
        logOutputGO.transform.SetParent(logOutputContainer, false);
        currentLogOutput = logOutputGO.GetComponent<Text>();
    }
}
