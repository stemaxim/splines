using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RemoveDelay : MonoBehaviour{//, IPointerClickHandler {

	void OnMouseDown () {
		Board.instance.isDelayOn = false;
		Destroy (gameObject,1);
//		gameObject.SetActive(false);
	}

//	public void OnPointerClick(PointerEventData pointerEventData)
//	{
//		Debug.Log ("Clicked");
//	}
}
