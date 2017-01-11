using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SystemD = System.Diagnostics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text.RegularExpressions;

public class ProjectorManagerScript : MonoBehaviour {

    public GameObject commandsBlock;
    public Button commandTemplate;
    public ProjectorScript projectorObject;
    public List<string> projectorCommands;
    public List<string> projectorPortNames;
    public string portNameDetectPattern;
    public InputField logText;

    public const string PREF_DEFAULT_PORT_NAMES_KEY = "_pref_default_port_names";
    public const string PROJECTOR_NOT_CONNECTED_STRING = "Not connected";
    public const string PROJECTOR_NOT_DETECTED_STRING = "Cannot detect this port. Please check connection and detect again.";
    public const string PROJECTOR_NOT_SAVED_STRING = "Cannot load this port. Please detect or input it.";
    public const string PROJECTOR_ON_ONLY_TAG = "OnOnly";
    public const string PROJECTOR_OFF_ONLY_TAG = "OffOnly";
    public static string[] PROJECTOR_ERROR_STRINGS = new string[] { PROJECTOR_NOT_DETECTED_STRING , PROJECTOR_NOT_SAVED_STRING };
    public static string[] availablePortNames;

    private List<ProjectorScript> projectors;
    private Regex portNameDetectRegex;
    private bool willBeShutdown = true;
    private Coroutine shutdownCoroutine;

    // Use this for initialization
    void Start () {
        ProjectorManagerScript.availablePortNames = SerialPort.GetPortNames();

        portNameDetectRegex = new Regex(portNameDetectPattern == "" ? ".*" : portNameDetectPattern);

        string portNamesStr = "Found ports: ";
        foreach (string portName in ProjectorManagerScript.availablePortNames) {
            portNamesStr += portName + ", ";
        }
        Debug.Log(portNamesStr);
        
        Debug.Log("Create Projector ports");
        projectors = new List<ProjectorScript>();
        foreach (string portName in projectorPortNames) {
            ProjectorScript p = Instantiate<ProjectorScript>(projectorObject);
            p.SetPortName(portName);
            p.transform.SetParent(projectorObject.transform.parent);
            p.transform.localScale = Vector3.one;
            projectors.Add(p);
        }
        projectorObject.gameObject.SetActive(false);

        ////reset names
        //SavePortNames();

        string defaultPortNamesStr = PlayerPrefs.GetString(PREF_DEFAULT_PORT_NAMES_KEY);
        Debug.Log("default ports: " + defaultPortNamesStr);
        string[] defaultPortNames = defaultPortNamesStr.Split(',');
        for (int i = 0; i < defaultPortNames.Length; i++) {
            if (defaultPortNames[i] == "") {
                projectors[i].inputStatus.text = PROJECTOR_NOT_SAVED_STRING;
            } else {
                projectors[i].SetPortName(defaultPortNames[i]);
            }
        }

        Debug.Log("Create command blocks");
        foreach (string cmd in projectorCommands) {
            string[] cmdElements = cmd.Split(';');
            string btnMethod = cmdElements[1];
            string btnName = cmdElements[0];
            bool mustBeWorking = cmdElements.Length >= 3 ? (cmdElements[2] == "1") : true;
            //Debug.Log(btnName + " : " + btnMethod);

            Button b = Instantiate<Button>(commandTemplate);
            b.transform.SetParent(commandsBlock.transform);
            b.transform.localScale = Vector3.one;
            b.gameObject.SetActive(true);
            Text t = b.GetComponentInChildren<Text>();
            t.text = btnName;

            b.onClick.AddListener(delegate {
                InvokeProjectors(btnMethod, mustBeWorking);
            });
        }

        ToggleOnOnlyObjects(true);
        ToggleOffOnlyObjects(true);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnEnable () {
        Application.logMessageReceived += LogHanlder;
    }

    void OnDisable() {
        Application.logMessageReceived -= LogHanlder;
    }

    private void LogHanlder (string message, string stackTrace, LogType type) {
        logText.text += (message + "\n");
        logText.caretPosition = logText.text.Length - 1;
    }

    private void LoopProjectors (Action<ProjectorScript, int> cb) {
        for (int i = 0; i < projectors.Count; i++) {
            cb(projectors[i], i);
        }
    }

    private void InvokeProjectors (string method, bool mustBeWorking = true) {
        foreach (ProjectorScript projector in projectors) {
            if (mustBeWorking && !projector.isProjectorWorking) continue;
            if (method.Contains("IE_"))
                projector.StartCoroutine(method);
            else
                projector.Invoke(method, 0);
        }
    }

    private void LoopGameObjectsWithTag (string tag, Action<GameObject> cb) {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag)) cb(obj);
    }

    private void ToggleOnOnlyObjects (bool isOn) {
        LoopGameObjectsWithTag(PROJECTOR_ON_ONLY_TAG, (gameObj) => {
            Button btn = gameObj.GetComponent<Button>();
            if (btn != null) {
                btn.interactable = isOn;
            }
        });
    }

    private void ToggleOffOnlyObjects(bool isOn) {
        LoopGameObjectsWithTag(PROJECTOR_OFF_ONLY_TAG, (gameObj) => {
            Button btn = gameObj.GetComponent<Button>();
            if (btn != null) {
                btn.interactable = isOn;
            }
        });
    }

    public void DetectProjectors () {
        int supportedProjectorCount = 0;
        for (int i = 0; i < ProjectorManagerScript.availablePortNames.Length; i++) {
            string portName = ProjectorManagerScript.availablePortNames[i];
            if (!portNameDetectRegex.Match(portName).Success) continue;
            ProjectorPort port = new ProjectorPort(portName);
            port.Open();
            Debug.Log(portName + " is " + (port.IsPortSupported ? "good" : "bad"));
            if (port.IsPortSupported) {
                projectors[supportedProjectorCount++].SetPortName(portName);
            }
            port.Close();
        }

        for (int i = supportedProjectorCount; i < projectors.Count; i++) {
            projectors[i].inputStatus.text = PROJECTOR_NOT_DETECTED_STRING;
        }

        SavePortNames();
    }

    public void Turn3DProjectors () {
        InvokeProjectors("ThreeDOnHandler", false);
    }

    public void Turn2DProjectors() {
        InvokeProjectors("ThreeDOffHandler", false);
    }

    private IEnumerator _TurnOnProjectors () {
        //ToggleOffOnlyObjects(false);
        InvokeProjectors("IE_PowerAnd3DOnHandler", false);
        yield return new WaitForSeconds(1);
        //ToggleOnOnlyObjects(true);
    }

    public void TurnOnProjectors () {
        StartCoroutine("_TurnOnProjectors");        
    }

    public void UpdateProjectors () {
        InvokeProjectors("UpdateHandler");
    }

	public void ConnectProjectors () {
        InvokeProjectors("Init", false);
	}

	public void TerminateProjectors () {
        //ToggleOnOnlyObjects(false);
        InvokeProjectors("PowerOffHandler", false);
        //ToggleOffOnlyObjects(true);
    }

    private void SavePortNames () {
        string[] names = new string[projectors.Count];
        LoopProjectors((projector, index) => {
            names[index] = projector.portName;
        });
        // save the port names
        string namesStr = string.Join(",", names);
        Debug.Log("Saving: " + namesStr);
        PlayerPrefs.SetString(PREF_DEFAULT_PORT_NAMES_KEY, namesStr);
    }

    IEnumerator ShutdownSystemAfter (float seconds = 30) {
        if (seconds < 5) {
            seconds = 5;
        }
        yield return new WaitForSeconds(seconds - 5);
        // turn of the projectors first
        TerminateProjectors();

        yield return new WaitForSeconds(5);
        // issue a command to shutdown the computer
        SystemD.Process.Start("shutdown", "/s /t 10 /c \"Projectors are off. The system will be down in 10 seconds.\" ");
    }

    public void ShutdownSystem (Text buttonText) {
        if (!willBeShutdown) {
            StopCoroutine(shutdownCoroutine);
            buttonText.text = "Shutdown";
        } else {
            shutdownCoroutine = StartCoroutine(ShutdownSystemAfter());
            buttonText.text = "Cancel\nShutdown";
        }
        willBeShutdown = !willBeShutdown;
    }

    void OnApplicationQuit() {
		Debug.Log ("Quit");
		TerminateProjectors ();
	}

}
