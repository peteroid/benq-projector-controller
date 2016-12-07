using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;

public class ProjectorPort {

	public const int PROJECTOR_BAUD_RATE = 115200;
	public const int PROJECTOR_READ_DELAY_MS = 100;

	private const char PROJECTOR_CR_CHAR = '\r';
	private const char PROJECTOR_NL_CHAR = '\n';
	private static char[] PROJECTOR_TRIM_CHARS = new char[]{PROJECTOR_CR_CHAR, PROJECTOR_NL_CHAR, ' '};
	private const string PROJECTOR_ATTR_PATTERN = @"=((?:(?!\?).)*)#";

	private string portName, modelName = "";
	private bool isInitialized = false;
	private Regex attrRegex = new Regex (PROJECTOR_ATTR_PATTERN);
	private SerialPort _port;

	// getter / setter
	public bool IsPortInitialized {
		get {
			return isInitialized;
		}
	}

	public ProjectorPort (string portName) {
		this.portName = portName;
		_port = new SerialPort (portName, PROJECTOR_BAUD_RATE);
	}

	private void WriteCommand (string command) {
		_port.DiscardInBuffer ();
		string _toWrite = "" + PROJECTOR_CR_CHAR + '*' + command + '#' + PROJECTOR_CR_CHAR;
		Debug.Log ("write: " + _toWrite);
		_port.Write (_toWrite);
	}

	private string ReadCommand () {
		return _port.BytesToRead > 0 ? _port.ReadExisting () : "";
	}

	// TODO
	private string GetAttr (string attr, int delayMs = PROJECTOR_READ_DELAY_MS) {
		Debug.Log ("get: " + attr);

		WriteCommand (attr + "=?");
		Thread.Sleep (delayMs);
		string result = ReadCommand ();
		Debug.Log ("read: " + result);

		if (result.Contains ("?#")) {
			result = result.Substring (result.IndexOf ('?') + 2);
		}

		Match match = attrRegex.Match (result);
		if (match.Success) {
			result = match.Groups [match.Groups.Count - 1].Value;
		} else {
			result = result.Trim (PROJECTOR_TRIM_CHARS);
		}
		return result;
	}

	public void Open () {
		try {
			_port.Open ();
			isInitialized = true;
		} catch (IOException e) {
			isInitialized = false;
			Debug.LogError (e.Message);
			Debug.LogError ("Err on port: " + portName);
		}
	}

	public void Close () {
		if (_port.IsOpen) {
			_port.Close ();
		}
	}

	private void SendCommand (string command) {
		WriteCommand (command);
		ReadCommand ();
	}

	public string GetPower () {
		return GetAttr ("pow");
	}

	public string GetSource () {
		return GetAttr ("sour");
	}

	public string GetModelName () {
		if (modelName == "")
			modelName = GetAttr ("modelname", 6000);
		return modelName;
	}

	public string Get3DStatus () {
		return GetAttr ("3d");
	}

	public void PowerOn () {
		SendCommand ("pow=on");
	}

	public void PowerOff () {
		SendCommand ("pow=off");
	}

	public void _3DEnable () {
		SendCommand ("3d=fs");
	}

	public void _3DDisable () {
		SendCommand ("3d=off");
	}
}
