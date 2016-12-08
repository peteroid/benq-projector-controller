using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MenuManagerScript : MonoBehaviour {

    public List<GameObject> menus;
    public int defaultMenuIndex;

	// Use this for initialization
	void Start () {
        GoToMenu(defaultMenuIndex);
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private void SetMenusActive (bool isActive) {
        foreach (GameObject m in menus) {
            m.SetActive(isActive);
        }
    }

    public void GoToMenu (int index) {
        SetMenusActive(false);
        menus[index].SetActive(true);
    }
}
