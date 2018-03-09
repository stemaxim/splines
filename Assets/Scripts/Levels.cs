using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class Levels : MonoBehaviour {

	public void QuitBtnClick() {
		Application.Quit();
	}

	public void OnMouseDown () {
		SceneManager.LoadScene (2);
	}
}
