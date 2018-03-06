using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		get{  return size;}
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

	Vector3[] Directions = new Vector3[] { Vector3.down, Vector3.left, Vector3.up, Vector3.right };
	//	Quaternion[] Rotations = new Quaternion[] { Vector3.down, Vector3.left, Vector3.up, Vector3.right };
	int direction = 0;

	//	public Sprite[,] tiles = new Sprite[tile_width,tile_height];

	[SerializeField]
	GameObject bgTile, line;


	//	private int tilesNumX = 


	void Awake(){
		_instance = this;
		//		mapSize = (double)(100 / (mapWidth * mapHeight));
		mapSize = mapWidth * mapHeight;
	}

	void Start () {

		Debug.Log (name + transform.localPosition.ToString () + transform.position.ToString ());

		startX = (int)transform.position.x;     
		startY = (int)transform.position.y;

		//		Vector2 offset = new Vector2 (tileSize, tileSize);//bgTile.GetComponent<SpriteRenderer> ().bounds.size;
		CreateBoard (tileSize, tileSize);//(offset.x, offset.y); 
		DrawLine(totalSteps);
	}

	void Update() {

	}

	private void CreateBoard (float xOffset, float yOffset) {


		//		tiles = new GameObject[mapWidth, mapHeight];    

		float startX = transform.position.x;     
		float startY = transform.position.y;

		for (int x = 0; x < mapWidth; x++) {      
			for (int y = 0; y < mapHeight; y++) {
				GameObject newTile = Instantiate ( bgTile, transform );
				newTile.transform.position = new Vector3 ( startX + (xOffset * x), startY + (yOffset * y), 20);
				//				tiles [x, y] = newTile;
			}
		}
	}

	void DrawLine (int movesNum) {

		Vector3 nextStepPos;

		int steps = Random.Range (randomMin, randomMax);

		//		Quaternion rotationHelper = new Quaternion();

		//		Transform rotationHelper = new Quaternion(); //= new Transform;

		var rotationHelper = Quaternion.Euler(new Vector3( 0, 0, 0));

		nextStepPos = new Vector3 { x = startX + (mapWidth/2*tileSize-2*tileSize),
									y = startY + (mapHeight/2*tileSize),
			z = 0 };

		//		lineObjHelper.transform.position = nextStepPos;
		//		lineObjHandler.transform.position = nextStepPos;

		for ( ; movesNum > 0; movesNum-- ) {




			if (isBorder (nextStepPos)||(steps == 0)) { 
				direction++;
				steps = Random.Range (randomMin, randomMax);

				GameObject lineCorner = Instantiate ( line, nextStepPos, rotationHelper, transform );
				lineCorner.transform.position = nextStepPos;
				lineCorner.transform.rotation = rotationHelper;

				rotationHelper *= Quaternion.Euler(new Vector3( 0,0, -90));
				//				rotationHelper.eulerAngles *= Vector3( 0,0, -90);//Quaternion.Euler(new Vector3( 0,0, -90));
			} ;

			//			lineObjHelper.transform.position += (Vector3)( Directions[direction % 4] * tileSize );

			GameObject newLine = Instantiate ( line, nextStepPos, rotationHelper, transform );

			newLine.transform.position = nextStepPos;
			newLine.transform.rotation = rotationHelper;

			nextStepPos += ( Directions[direction % 4] * tileSize );

			steps--;

			//			Debug.Log (rotationHelper.eulerAngles);

		}
	}


	bool isBorder (Vector2 pos) {

		if (pos.x >= startX + mapWidth*tileSize-tileSize || pos.x <= startX || pos.y >= startY + mapHeight*tileSize-tileSize || pos.y <= startY )
			return true;
		else
			return false;
	}

}

