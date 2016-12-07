using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

public class ProjectorManagerScript : MonoBehaviour {

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
    }
	
	// Update is called once per frame
	void Update () {
	
	}

	public void ConnectProjectors () {
		foreach (ProjectorScript projector in projectors) {
			projector.Init ();
		}
	}

	public void TerminateProjectors () {
		foreach (ProjectorScript projector in projectors) {
			projector.End ();
		}
	}

	void OnApplicationQuit() {
		Debug.Log ("Quit");
		TerminateProjectors ();
	}
}
