using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onTap : MonoBehaviour {

	private int i = 1;

	[SerializeField]
	public int area;

	void OnMouseDown () {

		MeshRenderer mR = this.GetComponent<MeshRenderer> () as MeshRenderer;
		mR.material.color = Color.red;
		Debug.LogError ("Clicked on poly: "+this.name);
		GM.Instance.touches += i;
		i-=i;
		GM.Instance.touchesTxt.text = GM.Instance.touches.ToString () + " touches   size: ";
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
