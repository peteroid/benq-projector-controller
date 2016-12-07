using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

[System.Serializable]
public class CommandOnClickEvent : UnityEvent<string> {}

public class ProjectorManagerScript : MonoBehaviour {

    public GameObject commandsBlock;
    public Button commandTemplate;
    public List<string> projectorCommands;
    public List<ProjectorScript> projectors;

    public static string[] availablePortNames;

	// Use this for initialization
	void Start () {
        ProjectorManagerScript.availablePortNames = SerialPort.GetPortNames();
        string portNamesStr = "Found ports: ";
        
        foreach (string portName in ProjectorManagerScript.availablePortNames) {
            portNamesStr += portName + ", ";
        }

        Debug.Log(portNamesStr);

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
            projector.Invoke(method, 0);
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

	void OnApplicationQuit() {
		Debug.Log ("Quit");
		TerminateProjectors ();
	}
}
