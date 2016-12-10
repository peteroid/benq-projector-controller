using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using SystemD = System.Diagnostics;
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

    public static string[] availablePortNames;

    private List<ProjectorScript> projectors;
    private Regex portNameDetectRegex;

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

        Debug.Log("Create command blocks");
        foreach (string cmd in projectorCommands) {
            string btnMethod = cmd.Substring(cmd.IndexOf(";") + 1);
            string btnName = cmd.Substring(0, cmd.IndexOf(";"));
            //Debug.Log(btnName + " : " + btnMethod);

            Button b = Instantiate<Button>(commandTemplate);
            b.transform.SetParent(commandsBlock.transform);
            b.transform.localScale = Vector3.one;
            b.gameObject.SetActive(true);
            Text t = b.GetComponentInChildren<Text>();
            t.text = btnName;

            b.onClick.AddListener(delegate {
                InvokeProjectors(btnMethod);
            });
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private void InvokeProjectors (string method) {
        foreach (ProjectorScript projector in projectors) {
            if (projector == null) continue;
            if (method.Contains("IE_"))
                projector.StartCoroutine(method);
            else
                projector.Invoke(method, 0);
        }
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
                projectors[supportedProjectorCount++].inputStatus.text = portName;
            }
            port.Close();
        }

        for (int i = supportedProjectorCount; i < projectors.Count; i++) {
            projectors[i].inputStatus.text = "";
        }
    }

    public void UpdateProjectors () {
        InvokeProjectors("UpdateHandler");
    }

	public void ConnectProjectors () {
        InvokeProjectors("Init");
	}

	public void TerminateProjectors () {
        InvokeProjectors("End");
	}

    public void ShutdownSystem () {
        SystemD.Process.Start("shutdown", "/s /t 0");
    }

	void OnApplicationQuit() {
		Debug.Log ("Quit");
		TerminateProjectors ();
	}
}
