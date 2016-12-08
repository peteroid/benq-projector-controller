using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuManagerScript : MonoBehaviour {

    public List<GameObject> menus;
    public int defaultMenuIndex;

	// Use this for initialization
	void Start () {
        foreach (GameObject m in menus) {
            m.SetActive(true);
        }
        GoToMenu(defaultMenuIndex);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private void SetMenusActive (bool isActive, int except = -1) {
        for (int i = 0; i < menus.Count; i++) {
            GameObject m = menus[i];
            //m.SetActive(isActive);
            //m.GetComponent<MeshRenderer>().enabled = isActive;
            m.transform.localScale = isActive ? Vector3.one : Vector3.zero;
        }

        if (except >= 0) {
            menus[except].transform.localScale = Vector3.one;
        }
    }

    public void GoToMenu (int index) {
        SetMenusActive(false, index);
        Debug.Log("show: " + index);
    }
}
