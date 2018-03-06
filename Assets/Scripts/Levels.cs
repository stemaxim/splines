using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Levels : MonoBehaviour {


	void Start() {
		
	}

	void Update() {
		
	}

	public void QuitBtnClick() {
		Application.Quit();
	}

	public void OnMouseDown () {
		Debug.LogWarning ("Button Pressed@");
		SceneManager.LoadScene (2);
	}
}
