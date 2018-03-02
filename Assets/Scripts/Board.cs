using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour {

	static Board _instance;
	public static Board instance
	{
		get {return _instance;}
	}

//	public int xSize, ySize;
	public int mapWidth = 31;
	public int mapHeight = 50;
	public int tileSize = 20;
	public int area = 0;

	private GameObject[,] tiles;

	public int totalSteps = 800;
	public int randomMin;
	public int randomMax;
	private int startX, startY;

	Vector3[] Directions = new Vector3[] { Vector3.down, Vector3.left, Vector3.up, Vector3.right };
//	Quaternion[] Rotations = new Quaternion[] { Vector3.down, Vector3.left, Vector3.up, Vector3.right };
	int direction = 0;

//	public Sprite[,] tiles = new Sprite[tile_width,tile_height];

	[SerializeField]
	GameObject bgTile, filledTile, inactiveTile, wall, line;


//	private int tilesNumX = 


	void Awake(){
		_instance = this;

	}

		void Start () {

		startX = (int)transform.position.x;     
		startY = (int)transform.position.y;

//		Vector2 offset = new Vector2 (tileSize, tileSize);//bgTile.GetComponent<SpriteRenderer> ().bounds.size;
		CreateBoard (tileSize, tileSize);//(offset.x, offset.y); 
		LineCrawl(totalSteps);
	}
		

	private void CreateBoard (float xOffset, float yOffset) {
	

		tiles = new GameObject[mapWidth, mapHeight];    

		float startX = transform.position.x;     
		float startY = transform.position.y;

		for (int x = 0; x < mapWidth; x++) {      
			for (int y = 0; y < mapHeight; y++) {
				GameObject newTile = Instantiate (bgTile, transform );
				newTile.transform.position = new Vector3 ( startX + (xOffset * x), startY + (yOffset * y), 20);
//				tiles [x, y] = newTile;
			}
		}
	}


	void Update () {

//		LineCrawl ();
		
	}

	void LineCrawl (int movesNum) {

		Vector3 nextStepPos;

		int steps = Random.Range (randomMin, randomMax);
			
//		Quaternion rotationHelper = new Quaternion();

//		Transform rotationHelper = new Quaternion(); //= new Transform;

		var rotationHelper = Quaternion.Euler(new Vector3( 0, 0, 0));

		nextStepPos = new Vector3 { x = startX + mapWidth*tileSize/2,
									y = startY + (mapHeight*tileSize/2),
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
			};

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

		if (pos.x >= startX + mapWidth*tileSize-tileSize+5 || pos.x <= startX + tileSize+5 || pos.y >= startY + mapHeight*tileSize-tileSize-5 || pos.y <= startY + tileSize+5 )
			return true;
		else
			return false;
	}

}
