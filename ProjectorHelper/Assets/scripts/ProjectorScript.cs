using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ProjectorScript : MonoBehaviour {

	public Button btnPowerOn;
	public Button btnPowerOff;
	public Text txStatus;

	public string portName;

	private ProjectorPort pPort;

	// Use this for initialization
	void Start () {
		pPort = new ProjectorPort (portName);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Init () {
		pPort.Open ();
		Debug.Log (pPort.IsPortInitialized);
//		txStatus.text = pPort.GetModelName ();
	}

	public void End () {
		pPort.Close ();
	}

	public void PowerOnHandler () {
		pPort.PowerOn ();
	}

	public void PowerOffHandler () {
		pPort.PowerOff ();
	}

	public void UpdateHandler () {
		txStatus.text = String.Format("{0} : {1}", pPort.GetModelName (), pPort.GetPower ());
	}
}
