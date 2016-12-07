using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProjectorManagerScript : MonoBehaviour {

	public List<ProjectorScript> projectors;

	// Use this for initialization
	void Start () {
	
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
