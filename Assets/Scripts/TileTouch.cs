using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

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

	private int debugNum;


	void Start () {
//		tileState = State.floor;
		tileSize = Board.instance.tileSize;
	}

	void Awake () {
		_instance = this;
	}

//	public void OnPointerDown (PointerEventData eventData) 
//	{
//		Debug.Log (this.gameObject.name + " Was Clicked.");
//	}

	void OnMouseDown () {

//		if (tileState != State.wall) {
//			image.color = Color.red;
//			tileState = State.painted;
//		}

//		hitBlocks = Physics2D.LinecastAll (transform.position, (Vector2)transform.position + Vector2.left * tileSize);
//		Debug.DrawLine ( transform.position, (Vector2)transform.position + Vector2.left*20, Color.green ); 
//		Debug.Log("Hit object: " + System.String.Join (",", hitBlocks.Select(v => v.collider.gameObject.name).ToArray())+";");// hitBlocks.collider.gameObject.name);
		paintTile (State.painted);
	}

	
	public void paintTile (State currState) {
		
		if (currState == State.disabled) {

			image.color = Color.grey;	
			tileState = State.disabled;
			GetComponent<BoxCollider2D>().enabled = false;
		
		} else if (currState == State.painted) {

			image.color = Color.red;
			tileState = State.painted;
			GetComponent<BoxCollider2D>().enabled = false;
			Board.instance.area++;
		
		} else
			return;


		foreach (Vector2 directionVect in new Vector2[4] {Vector2.left, Vector2.right, Vector2.up, Vector2.down}) {

			hitBlocks = Physics2D.LinecastAll (transform.position, (Vector2)transform.position + directionVect * tileSize);

			Debug.DrawLine ( transform.position, (Vector2)transform.position + Vector2.left*20, Color.green ); 
			Debug.Log("Hit object: " + System.String.Join (",", hitBlocks.Select(v => v.collider.gameObject.name).ToArray())+" "+currState+" by Vector: "+directionVect);// hitBlocks.collider.gameObject.name);

//			var dPrefab = Instantiate(debugPrefab, transform.position, Quaternion.identity, transform);
//			dPrefab.name = "State: " + currState + " "+Board.instance.area++;

			if (hitBlocks.Length > 1) {
				if (currState != State.disabled) {
					hitBlocks [1].collider.gameObject.GetComponent<TileTouch> ().paintTile (State.disabled);
//					return;
				}
			} else if (hitBlocks.Length > 0 && hitBlocks [0].collider.gameObject.GetComponent<TileTouch> ().tileState != State.wall )
				hitBlocks [0].collider.gameObject.GetComponent<TileTouch> ().paintTile (currState);
		}
	}

//					var firstHit = hitBlocks [0].collider.gameObject.GetComponent<TileTouch> ().tileState;
//					var secondHit = hitBlocks [1];//.collider.gameObject.GetComponent<TileTouch> ().tileState;
					
//					var behindWall = secondHit.collider.gameObject.GetComponent<TileTouch> (instance);

//					behindWall.image.color = Color.gray;
//					behindWall.tileState = State.disabled;
					
//					secondHit.collider.gameObject.GetComponent<TileTouch> (paintTile(State.disabled));
//					secondHit.collider.enabled = false;

	

//	void Update() {
//		if (Input.GetMouseButtonDown(0)) {
//			Debug.Log("Pressed left click, casting ray.");
//			CastRay();
//		}
//	}
//
//	void CastRay() {
//		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//		RaycastHit hit;
//		if (Physics.Raycast(ray, out hit, 100)) {
//			Debug.DrawLine(ray.origin, hit.point);
//			Debug.Log("Hit object: " + hit.collider.gameObject.name);
//		}
//	}
}
