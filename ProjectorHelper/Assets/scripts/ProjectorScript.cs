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
        portName = inputStatus.text;
        pPort = new ProjectorPort(inputStatus.text);
        pPort.Open ();
		Debug.Log (pPort.IsPortInitialized);

        inputStatus.text = portName + (pPort.IsPortInitialized ? "" : " not") + " connected";
	}

    public void SetPortName (string portName) {
        this.portName = portName;
        inputStatus.text = portName;
    }

	public void End () {
        if (pPort != null)
		    pPort.Close ();
	}

    IEnumerator IE_PowerAnd3DOnHandler () {
        Debug.Log("Async power + 3d");
        pPort.PowerOn();
        yield return new WaitForSeconds(60f);
        Debug.Log("Async 3d");
        pPort._3DEnable();
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
