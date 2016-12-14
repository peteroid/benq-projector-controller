using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class ProjectorScript : MonoBehaviour {

	public Button btnPowerOn;
	public Button btnPowerOff;
    public InputField inputStatus;
    public Button statusConnection;

	public string portName;
    private ProjectorPort pPort;
    private string modelName, powerState, threeDState;

    public bool isProjectorInit {
        get {
            return pPort != null && pPort.IsPortInitialized;
        }
    }

    public bool isProjectorWorking {
        get {
            return pPort != null && pPort.IsWorking;
        }
    }

	// Use this for initialization
	void Start () {
        inputStatus.text = inputStatus.text == "" ? portName : inputStatus.text;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Init () {
        portName = inputStatus.text;
        if (portName == "" || Array.IndexOf(ProjectorManagerScript.PROJECTOR_ERROR_STRINGS, portName) >= 0) return;
        pPort = new ProjectorPort(portName);
        pPort.Open ();
		//Debug.Log (pPort.IsPortInitialized);

        inputStatus.text = portName + (pPort.IsWorking ? "" : " not") + " connected";
        ColorBlock colorBlock = statusConnection.colors;
        colorBlock.normalColor = pPort.IsWorking ? Color.green : Color.red;
        statusConnection.colors = colorBlock;
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
        Debug.Log("Async init? " + isProjectorInit);
        if (!isProjectorInit) Init();
        Debug.Log("Async power + 3d");
        pPort.PowerOn();
        yield return new WaitForSeconds(60f);
        Debug.Log("Async 3d");
        pPort._3DEnable();
        yield return new WaitForSeconds(1f);
        UpdateStatus();
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

    public void UpdateStatus () {
        inputStatus.text = String.Format("{0} : {1} : {2}", modelName, pPort.GetPower(), pPort.Get3DStatus());
    }

    public void UpdateHandler () {
        StartCoroutine(pPort.GetModelNameAsync((modelName) => {
            this.modelName = modelName;
            UpdateStatus();
        }));
    }
}
