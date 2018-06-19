using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MoonSharp.Interpreter;
using UniRx;

public class ScriptingConsoleComponent : GameComponent {

    public Text consoleText;
    public InputField consoleInput;

    private int historyID = -1;
    private List<string> history = new List<string>();
    private string currentViewText = "";
    private Service.Scripting.Commands.ExecuteStringOnMainScriptCommand executeLuaCmd = new Service.Scripting.Commands.ExecuteStringOnMainScriptCommand();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    protected override void AfterBind() {
        consoleInput.onEndEdit.AddListener(ProcessInput);
        Observable.EveryUpdate().Subscribe(_ => {
            if (Input.GetKeyDown(KeyCode.F8)) {
                gameObject.SetActive(!gameObject.activeSelf);
            }
            else if(consoleInput.isFocused && Input.GetKeyDown(KeyCode.UpArrow)){
                if (historyID+1 < history.Count) {
                    historyID++;
                }
                consoleInput.text = history[historyID];
            } 
            else if (consoleInput.isFocused && Input.GetKeyDown(KeyCode.DownArrow)) {
                if (historyID > 0) {
                    historyID--;
                } else if (historyID < 0) {
                    historyID = 0;
                }
                consoleInput.text = history[historyID];
            }
        }).AddTo(this);

        var cmdGetMainScript = new Service.Scripting.Commands.GetMainScriptCommand();
        Publish(cmdGetMainScript);
        cmdGetMainScript.result.Options.DebugPrint = (inputString) => {
            addToText(inputString);
        };
 

    }

    private void addToText(string txt) {
        currentViewText += txt + "\n";
        consoleText.text = currentViewText;
    }

    protected override void OnDispose() {
        base.OnDispose();
        consoleInput.onEndEdit.RemoveAllListeners();
    }

    public void ProcessInput(string input) {
        history.Insert(0, input);
        if (input.ToLower()=="clear") {
            currentViewText = "";
            consoleText.text = currentViewText;
        }
        else if (input.ToLower() == "close") {
            gameObject.SetActive(false);
        }
        else {
            currentViewText += input + "\n";
            executeLuaCmd.luaCode = input;
            Publish(executeLuaCmd);
            if (executeLuaCmd.result != "void") {
                currentViewText += executeLuaCmd.result+"\n";
            }
            consoleText.text = currentViewText;
            consoleInput.ActivateInputField();
        }
        consoleInput.text = "";
    }
}
