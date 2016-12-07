using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ProjectorScript : MonoBehaviour {

	public Button btnPowerOn;
	public Button btnPowerOff;
    public InputField inputStatus;

	public string portName;
	private ProjectorPort pPort;

	// Use this for initialization
	void Start () {
        inputStatus.text = portName;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Init () {
        pPort = new ProjectorPort(inputStatus.text);
        pPort.Open ();
		Debug.Log (pPort.IsPortInitialized);
	}

	public void End () {
        if (pPort != null)
		    pPort.Close ();
	}

	public void PowerOnHandler () {
		pPort.PowerOn ();
	}

	public void PowerOffHandler () {
		pPort.PowerOff ();
	}

    public void ThreeDOnHandler() {
        pPort._3DEnable();
    }

    public void ThreeDOffHandler () {
        pPort._3DDisable();
    }

	public void UpdateHandler () {
        inputStatus.text = String.Format("{0} : {1}", pPort.GetModelName(), pPort.GetPower());
    }
}
