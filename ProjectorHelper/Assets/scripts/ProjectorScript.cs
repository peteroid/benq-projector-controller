using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Threading;

public class ProjectorScript : MonoBehaviour {

	public Button btnPowerOn;
	public Button btnPowerOff;
    public InputField inputStatus, inputDelay;
    public Button statusConnection;

    public const float PROJECT_INIT_TO_DO_DELAY_S = 10;
    public const float PROJECTOR_ON_TO_3D_DELAY_S = 60;
    public const string PREF_PROJECTOR_ON_TO_3D_DELAY_PREFIX = "_pref_on_3d_delay_";

    public string portName;
    private ProjectorPort pPort;
    private string modelName;
    private float delayOnTo3D = PROJECTOR_ON_TO_3D_DELAY_S;

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

    public bool isPortNameInvalid {
        get {
            return portName == "" || Array.IndexOf(ProjectorManagerScript.PROJECTOR_ERROR_STRINGS, portName) >= 0;
        }
    }

	// Use this for initialization
	void Start () {
        inputStatus.text = inputStatus.text == "" ? portName : inputStatus.text;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnApplicationQuit () {
        if (!isPortNameInvalid) {
            Debug.Log("Save float");
            PlayerPrefs.SetFloat(PREF_PROJECTOR_ON_TO_3D_DELAY_PREFIX + portName, delayOnTo3D);
            PlayerPrefs.Save();
        }
    }

	public void Init () {
        portName = inputStatus.text;
        if (isPortNameInvalid) return;
        pPort = new ProjectorPort(portName);
        pPort.Open ();
		//Debug.Log (pPort.IsPortInitialized);

        inputStatus.text = portName + (pPort.IsWorking ? "" : " not") + " connected";
        ColorBlock colorBlock = statusConnection.colors;
        Color nextColor = pPort.IsWorking ? Color.green : Color.red;
        colorBlock.normalColor = nextColor;
        colorBlock.highlightedColor = nextColor;
        statusConnection.colors = colorBlock;
    }

    public void OnDelayChangeHandler () {
        float delay;
        try {
            delay = float.Parse(inputDelay.text);
            delayOnTo3D = delay;
            //Debug.Log(delay);
        } catch (FormatException e) {
            Debug.Log(e.Message);
        }
    }

    public void SetPortName (string portName) {
        this.portName = portName;
        inputStatus.text = portName;

        if (!isPortNameInvalid) {
            delayOnTo3D = PlayerPrefs.GetFloat(PREF_PROJECTOR_ON_TO_3D_DELAY_PREFIX + portName, delayOnTo3D);
            inputDelay.text = delayOnTo3D.ToString();
        }
    }

	public void End () {
        if (pPort != null)
		    pPort.Close ();
	}

    private void TryInitAndDo (Action cb) {
        if (!isProjectorInit) Init();

        if (pPort != null) {
            cb();
        }
    }

    private IEnumerator TryInitAndDoAsync(Action cb, float actionDelaySec = PROJECT_INIT_TO_DO_DELAY_S) {
        bool didInit = false;
        if (!isProjectorInit) {
            Init();
            didInit = true;
        }

        if (pPort != null) {
            Debug.Log("Wait for " + actionDelaySec);
            yield return new WaitForSeconds(didInit ? actionDelaySec : 0.5f);
            cb();
        }
    }

    IEnumerator IE_PowerAnd3DOnHandler () {
        Debug.Log("Async init? " + isProjectorInit);
        bool didInit = false;
        if (!isProjectorInit) {
            Init();
            didInit = true;
        }

        if (pPort != null) {
            Debug.Log("Async power + 3d");
            yield return new WaitForSeconds(didInit ? PROJECT_INIT_TO_DO_DELAY_S : 0.5f);
            pPort.PowerOn();
            yield return new WaitForSeconds(delayOnTo3D);
            Debug.Log("Async 3d");
            pPort._3DEnable();
            yield return new WaitForSeconds(1f);
            UpdateHandler();
        }
    }

	public void PowerOnHandler () {
        pPort.PowerOn();
	}

	public void PowerOffHandler () {
        StartCoroutine(TryInitAndDoAsync(() => {
            Debug.Log("now off...");
            pPort.PowerOff();
        }));
    }

    public void ThreeDOnHandler() {
        StartCoroutine(TryInitAndDoAsync(() => {
            Debug.Log("now 3d on...");
            pPort._3DEnable();
        }));
    }

    public void ThreeDOffHandler () {
        StartCoroutine(TryInitAndDoAsync(() => {
            Debug.Log("now 3d off...");
            pPort._3DDisable();
        }));
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
