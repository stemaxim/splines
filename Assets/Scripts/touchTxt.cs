using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
	public class touchTxt : MonoBehaviour {
	Text txt;
	// Use this for initialization
	void Start () {
		txt = GetComponent<Text> ();
	}

	// Update is called once per frame
	void Update () {
	}
}

