using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Board : MonoBehaviour {

	static Board _instance;
	public static Board instance
	{
		get {return _instance;}
	}

	//	public int xSize, ySize;
	public int mapWidth;
	public int mapHeight;
	public int tileSize = 20;
	private int mapSize;


	[SerializeField]
	private Text _area;
	private int size = 0;
	public int area 
	{	 
		get {  return size;}
		set { 
			size = value;
			_area.text = (size * 100 / mapSize).ToString();
			//			Debug.Log ("size: " + size + " mapSizeRel:" + mapSize.ToString());
		}
	}

//	private GameObject[,] tiles;

	public int totalSteps = 800;
	public int randomMin;
	public int randomMax;
	private int startX, startY;

	Vector3[] Directions = new Vector3[] { Vector3.up, Vector3.right, Vector3.down, Vector3.left  };
	//	Quaternion[] Rotations = new Quaternion[] { Vector3.down, Vector3.left, Vector3.up, Vector3.right };
	int direction = 0;

	//	public Sprite[,] tiles = new Sprite[tile_width,tile_height];
	public struct minMaxPos {
		public int min;
		public int max;
	}

	public minMaxPos[] linesData;// = new minMaxPos[mapHeight](new minMaxPos{5555,-1});

	public float drawDelay = 0.02f;

	[SerializeField]
	GameObject bgTile, line, cover, dbgPrefab;

	GameObject newTile;

	public bool isDelayOn = true;

	/// <summary> variables end
	/// ////////////////// ////////////////// ////////////////// ////////////////// ////////////////// ///////////////
	/// </summary>

	void Awake(){
		_instance = this;
		//		mapSize = (double)(100 / (mapWidth * mapHeight));
		mapSize = mapWidth * mapHeight;
	}

	void Start () {

		startX = (int)transform.position.x;     
		startY = (int)transform.position.y;

		//		Vector2 offset = new Vector2 (tileSize, tileSize);//bgTile.GetComponent<SpriteRenderer> ().bounds.size;
		CreateBoard (tileSize, tileSize);//(offset.x, offset.y); 
//		yield return 
		StartCoroutine( DrawLine(totalSteps) );
//		StartCoroutine( DisableAdjacent() );
	}


	private void CreateBoard (float xOffset, float yOffset) {


//1		tiles = new GameObject[mapWidth, mapHeight];    

		float startX = transform.position.x;     
		float startY = transform.position.y;

		for (int x = 0; x < mapWidth; x++) {      
			for (int y = 0; y < mapHeight; y++) {
				newTile = Instantiate ( bgTile, transform );
				newTile.transform.position = new Vector3 ( startX + (xOffset * x), startY + (yOffset * y), 1);
//1				tiles [x, y] = newTile;
			}
		}
	}

	IEnumerator DrawLine (int movesNum) {
		yield return null;
		Vector3 nextStepPos;

		int steps = Random.Range (randomMin, randomMax);

		//		Quaternion rotationHelper = new Quaternion();

		//		Transform rotationHelper = new Quaternion(); //= new Transform;

		var rotationHelper = Quaternion.Euler(new Vector3( 0, 0, -180));

		nextStepPos = new Vector3 { x = startX + 2*tileSize,//-2*tileSize),
									y = startY + 2*tileSize,
									z = 0 };

		var startPos = nextStepPos;

//		int offsetX, offsetY = 0;

		//fill in linesData array;
//1		linesData = Enumerable.Repeat ( new minMaxPos { min = 5555, max = 0 }, mapHeight).ToArray();

		//		lineObjHelper.transform.position = nextStepPos;
		//		lineObjHandler.transform.position = nextStepPos;

		for ( ; movesNum > 0; movesNum-- ) {

			if (isBorder ( nextStepPos, direction )||(steps == 0)) { 
				direction++;
				direction %= 4;
				steps = Random.Range (randomMin, randomMax);


				GameObject lineCorner = Instantiate ( line, nextStepPos, rotationHelper, transform );
				lineCorner.transform.position = nextStepPos;
				lineCorner.transform.rotation = rotationHelper;

				rotationHelper *= Quaternion.Euler(new Vector3( 0, 0, -90 ));
//				Debug.Log (rotationHelper.eulerAngles.ToString());
				//				rotationHelper.eulerAngles *= Vector3( 0,0, -90);//Quaternion.Euler(new Vector3( 0,0, -90));
			} ;

			//			lineObjHelper.transform.position += (Vector3)( Directions[direction % 4] * tileSize );



			GameObject newLine = Instantiate ( line, nextStepPos, rotationHelper, transform );

			newLine.transform.position = nextStepPos;
			newLine.transform.rotation = rotationHelper;

			if ( movesNum == 1 && startPos != nextStepPos ) {
				Debug.LogWarning ("direction: "+direction+" movesNum: "+movesNum);
				switch (direction) {
				case 2:
					steps = (int)(nextStepPos.y - startPos.y)/tileSize;//((-startY - tileSize * 2 + (int)nextStepPos.y )/tileSize);
					movesNum = steps+1;
					break;
				case 3:
					steps = (int)(nextStepPos.x - startPos.x)/tileSize;//((-startX - tileSize * 2 + (int)nextStepPos.x )/tileSize);
					movesNum = steps+1;
					break;
				case 0:
//					steps = 5;//Random.Range (randomMin, randomMax);
					movesNum = 5;
					break;
				case 1:
//					steps = 5;//Random.Range (randomMin, randomMax);
					movesNum = 5;
					break;
				}
			}
//1			int tilex = (int) (nextStepPos.x / tileSize);
//1			int tiley = (int) (nextStepPos.y / tileSize);

//1		if ( direction == 0 ) {
//1			linesData [tiley].min = Mathf.Min ( linesData [tiley].min,  tilex);
//1			linesData [tiley].max = Mathf.Max ( linesData [tiley].max, tilex + 1);
//1		} else
//1			if ( direction == 2 ) {
//1				linesData [tiley].min = Mathf.Min ( linesData [tiley].min, tilex - 1);
//1				linesData [tiley].max = Mathf.Max ( linesData [tiley].max, tilex );
//1			} else
//1				if ( direction == 1 ) {
//1					linesData [tiley].min = Mathf.Min ( linesData [tiley].min, tilex - 1);
//1					linesData [tiley].max = Mathf.Max ( linesData [tiley].max, tilex );
//1				} else
//1					if ( direction == 3 ) {
//1						linesData [tiley].min = Mathf.Min ( linesData [tiley].min, tilex - 1);
//1						linesData [tiley].max = Mathf.Max ( linesData [tiley].max, tilex );
//1					};

			nextStepPos += ( Directions[direction] * tileSize );

			steps--;

			if (isDelayOn) yield return new WaitForSeconds(0.01f);

			//			Debug.Log (rotationHelper.eulerAngles);
		}
//		DisableAdjacent ();

//		this.gameObject.GetComponentInParent<>().
//		Destroy(GameObject.FindWithTag("Cover"));
//		newTile.GetComponent<TileTouch>().paintTile(TileTouch.State.disabled);
		cover.GetComponent<BoxCollider2D>().enabled = false;
		Destroy(cover);
		TileTouch.instance.paintTile( TileTouch.State.disabled );

		Instantiate ( dbgPrefab, nextStepPos, Quaternion.identity, transform );
	}


	bool isBorder (Vector2 pos, int dir) {

		if ( 	(( pos.x >= (startX + mapWidth * tileSize - tileSize * 3)) 	&& dir == 1 ) || 
				(( pos.x <= (startX + tileSize * 2 ))						&& dir == 3 ) || 
				(( pos.y >= (startY + mapHeight * tileSize - tileSize * 3))	&& dir == 0 ) ||
				(( pos.y <= (startY + tileSize * 2 )) 						&& dir == 2 ))
			return true;
		else
			return false;
	}


	void DisableAdjacent() {
			
			Destroy(cover);
			TileTouch.instance.paintTile(TileTouch.State.disabled);
//1	IEnumerator DisableAdjacent () {
//1		yield return StartCoroutine( DrawLine(totalSteps));
//1
//1   for (int y = 0; y < mapHeight; y++) {
//1   	if (linesData [y].min < 5555)
//1   		for (int x = 0; x <= linesData [y].min; x++) {
//1   			tiles [x, y].GetComponent<BoxCollider2D>().enabled = false;
//1   			tiles [x, y].GetComponent<TileTouch>().image.color = Color.red;
//1   	}
//1   }
	}

}

