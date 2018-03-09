using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using System;

public class TileTouch : MonoBehaviour {


	static TileTouch _instance;
	public static TileTouch instance
	{
		get {return _instance;}
	}

	public Image image;
	public enum State { painted, disabled, floor, wall};

	public State tileState;

	//[NonSerialized]
	public int tileSize;

	public GameObject debugPrefab;

	private RaycastHit2D[] hitBlocks;



	void Start () {
		tileSize = Board.instance.tileSize;
	}

	void Awake () {
		_instance = this;
	}


	void OnMouseDown () {


		paintTile (State.painted);
	}

	
	public void paintTile (State currState) {
		
		if (currState == State.disabled) {

			tileState = State.disabled;
			GetComponent<BoxCollider2D>().enabled = false;
		
		} else if (currState == State.painted) {

			image.color = new Color32 (Convert.ToByte (0x45), Convert.ToByte (0xB0), Convert.ToByte (0xFF), 255); // 2570FFFF
			tileState = State.painted;
			GetComponent<BoxCollider2D>().enabled = false;
			Board.instance.area++;
		
		} else
			return;


		foreach (Vector2 directionVect in new Vector2[4] {Vector2.left, Vector2.up, Vector2.down, Vector2.right}) {

			hitBlocks = Physics2D.LinecastAll (transform.position, (Vector2)transform.position + directionVect * tileSize);




			if (hitBlocks.Length > 1) {
				if (currState != State.disabled && hitBlocks.Last().collider.tag != "Wall") {
					hitBlocks.Last().collider.gameObject.GetComponent<TileTouch> ().paintTile (State.disabled);
					continue;
				}
			} else if (hitBlocks.Length > 0 && hitBlocks.First().collider.tag != "Wall" )// hitBlocks [0].collider.gameObject.GetComponent<TileTouch> ().tileState != State.wall )
				hitBlocks.First().collider.gameObject.GetComponent<TileTouch> ().paintTile (currState);
		}
	}
}
