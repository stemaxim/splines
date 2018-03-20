using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
//using Random = System.Random;

public class Board : MonoBehaviour {

	static Board _instance;
	public static Board instance
	{
		get {return _instance;}
	}

	//	public int xSize, ySize;
	public int mapWidth;
	public int mapHeight;
	public static int tileSize = 20;
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

	private struct DirectionsData
	{
		public int direction;
		public int x,y;
	}

	private static DirectionsData[] reverseDirection = new DirectionsData[] {
		new DirectionsData { direction = 2, x = -1*tileSize, y = 0*tileSize },
		new DirectionsData { direction = 3, x = 0*tileSize, y = 1*tileSize },
		new DirectionsData { direction = 0, x = 1*tileSize, y = 0*tileSize },
		new DirectionsData { direction = 1, x = 0*tileSize, y = -1*tileSize }
	};
	//	Quaternion[] Rotations = new Quaternion[] { Vector3.down, Vector3.left, Vector3.up, Vector3.right };
	int direction = 0;

	public struct MinMaxPos {
		public int min;
		public int max;
	}

	private struct LinesData {
		public GameObject gobj;
		public int direction;
		public int x, y;
		public string hashKey;
		public string hashKeyRev;
		public LinesData (GameObject _gobj, int _direction, int _x, int _y) {
			gobj = _gobj;
			direction = _direction;
			x = _x;
			y = _y;
			hashKey = "" + _direction + "/" + _x + "/" + _y;
			hashKeyRev = "" + reverseDirection [_direction].direction + "/" + (_x + reverseDirection [_direction].x) + "/" + 
																				(_y + reverseDirection [_direction].y);

		}

	}

//	public struct linesData
//	{
//		public int direction {
//			get { return direction; }
//			set {
//				hashKey ="" + direction + "/" + x + "/" + y;
//				direction = value;
//			}
//		}
//		public int x
//		{
//			get { return x; }
//			set
//			{
//				hashKey = "" + direction + "/" + x + "/" + y;
//				x = value;
//			}
//		}
//		public int y
//		{
//			get { return y; }
//			set
//			{
//				hashKey = "" + direction + "/" + x + "/" + y;
//				y = value;
//			}
//		}
//		public string hashKey {  get; private set; }
//	}

	public static Dictionary <string,bool> lines = new Dictionary<string, bool> ();
	// = new minMaxPos[mapHeight](new minMaxPos{5555,-1});

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
		lines.Clear();
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
		Vector3 prevRollbackPos = Vector3.zero;

		var linesToStore = 20;

		var linesRollback = new List<LinesData> ();

		var currLineData = new LinesData ();
//		var currLineDataRev = new LinesData ();

		int ahead = 1;
		int loopCount = 1;

//		int offsetX, offsetY = 0;

		//fill in linesData array;
//1		linesData = Enumerable.Repeat ( new minMaxPos { min = 5555, max = 0 }, mapHeight).ToArray();

		//		lineObjHelper.transform.position = nextStepPos;
		//		lineObjHandler.transform.position = nextStepPos;

		for (; movesNum > 0; movesNum-- ) {

			if (isBorder ( nextStepPos, direction )||(steps == 0)) { 

				steps = Random.Range (randomMin, randomMax);

				////// lines turn collision prediction /////////

				var tmpDirection = ((direction + 2) % 4);
				Vector3 checkPos = nextStepPos + Directions [ (direction + 1) % 4] * tileSize * steps * ahead;

				currLineData = new LinesData ( null, tmpDirection, (int)checkPos.x, (int)checkPos.y); //linesCorner
//				currLineDataRev = new LinesData ( null, reverseDirection [tmpDirection].direction, (int)checkPos.x + reverseDirection [tmpDirection].x,
//											(int)checkPos.y + reverseDirection [tmpDirection].y);

				if (lines.ContainsKey (currLineData.hashKey) ||  lines.ContainsKey (currLineData.hashKeyRev))  
				{	steps -= 3;};
				////////////////////////////////////////////////////////


				currLineData = new LinesData ( null, direction, (int)nextStepPos.x, (int)nextStepPos.y);

				if (lines.ContainsKey (currLineData.hashKey) || lines.ContainsKey (currLineData.hashKeyRev)) {
					continue;
				}
				///// fill-in lines dictionary to detect lines overlays further

				lines.Add (currLineData.hashKey, true);
				lines.Add (currLineData.hashKeyRev, true);

				GameObject lineCorner = Instantiate ( line, nextStepPos, rotationHelper, transform );
				lineCorner.name = "cornerInstance";
				lineCorner.transform.position = nextStepPos;
				lineCorner.transform.rotation = rotationHelper;
//				lineCorner.GetComponent<Image> ().color = Color.red;
				currLineData.gobj = lineCorner;

//1				currLineDataRev = new LinesData ( lineCorner, reverseDirection [tmpDirection].direction, (int)checkPos.x + reverseDirection [tmpDirection].x,
//1										(int)checkPos.y + reverseDirection [tmpDirection].y);

//				Debug.LogErrorFormat ("Automatic hashKey: {0}", currLineData.hashKey);

				//////// fill-in linesRollback list used on collision detection /////////

				if (linesToStore == 0) { 
					linesRollback.RemoveAt (0);
				}
				else {
					linesToStore--;
				};
				linesRollback.Add (currLineData);

				////////////////////////


				direction ++;
				direction %= 4;

				// replace with  er = Vect3(0,0,-90*direction);
				rotationHelper = Quaternion.Euler(new Vector3( 0, 0, (direction+2) * -90 ));
//				Debug.Log (rotationHelper.eulerAngles.ToString());
				//rotationHelper.eulerAngles *= Vector3( 0,0, -90);//Quaternion.Euler(new Vector3( 0,0, -90));
			} ;
			////////////// End of corner check ///////

			 
			//			lineObjHelper.transform.position += (Vector3)( Directions[direction % 4] * tileSize );


			///////// don't draw through existing lines
//			var stepsPred = nextStepPos + Directions[direction] * steps * tileSize;

			currLineData = new LinesData ( null, direction, (int)nextStepPos.x, (int)nextStepPos.y);
//			currLineDataRev = new LinesData ( null, reverseDirection [direction].direction, (int)nextStepPos.x + reverseDirection [direction].x,
//										(int)nextStepPos.y + reverseDirection [direction].y);

//			var linesKey = "" + direction + "/"+nextStepPos.x+"/" + nextStepPos.y;
//			var linesReverseKey = "" + reverseDirection [direction].direction +"/"+ (nextStepPos.x + reverseDirection [direction].x)
//									+"/"+ (nextStepPos.y + reverseDirection [direction].y);
			if (lines.ContainsKey (currLineData.hashKey) || lines.ContainsKey (currLineData.hashKeyRev) ) 
			{
//				Debug.LogWarning ("Found match at "+currLineData.hashKey+" direction: "+direction);

				var dbg = Instantiate ( dbgPrefab, nextStepPos, Quaternion.identity, transform );
				dbg.name = "dbg["+direction+","+nextStepPos+",steps: "+steps+"]";
				steps = 0;

				
				if (linesRollback.Count > 0 ) {

					var lC = linesRollback.Count - 1;
					nextStepPos = linesRollback.ElementAt (lC - 2 * loopCount).gobj.transform.position;
					rotationHelper = linesRollback.ElementAt (lC - 2 * loopCount).gobj.transform.rotation;
					direction = linesRollback.ElementAt (lC - 2 * loopCount).direction;

					if (nextStepPos == prevRollbackPos) {
						loopCount++;	
					} else
						loopCount = 1;

					for (int li = lC; li >=  lC - 2 * loopCount && li > 0; li -- ) {
						var lineToRemove = linesRollback [li];
						lines.Remove (lineToRemove.hashKey);
						lines.Remove (lineToRemove.hashKeyRev);
						Destroy ( lineToRemove.gobj );
						linesRollback.RemoveAt(li);
					}
//!					linesRollback.Clear ();
					linesToStore = 20;
					prevRollbackPos = nextStepPos;

				} 
//				loopCount++;

//				if (loopCount > 3) {
//					ahead *= -1;
//					nextStepPos += ( Directions[direction] * tileSize * ahead );
//					loopCount = 0;
//					continue;
//				}
				continue;
			}
//			loopCount = 1;
			/////////END don't draw through existing lines



//			lines.Add ("" + reverseDirection [direction].direction +"/"+ (nextStepPos.x + reverseDirection [direction].x)+"/"
//								+ (nextStepPos.y + reverseDirection [direction].y), true);

			/////////

			GameObject newLine = Instantiate ( line, nextStepPos, rotationHelper, transform );
			newLine.name = "Line [direction:" + direction + "]";
			newLine.transform.position = nextStepPos;
			newLine.transform.rotation = rotationHelper;

			currLineData.gobj = newLine;

			//////// fill-in linesRollback list used on collision detection /////////
			if (linesToStore == 0) { 
				linesRollback.RemoveAt(0);
			}
			else {
				linesToStore--;
			};

			linesRollback.Add (currLineData);
			lines.Add (currLineData.hashKey, true);
			lines.Add (currLineData.hashKeyRev, true);

			////////////////////////////// Return to start

//!			if ( movesNum == 1 && startPos != nextStepPos ) {
//!				switch (direction) {
//!				case 2:
//!					steps = (int)(nextStepPos.y - startPos.y)/tileSize;//((-startY - tileSize * 2 + (int)nextStepPos.y )/tileSize);
//!					movesNum = steps+1;
//!					break;
//!				case 3:
//!					steps = (int)(nextStepPos.x - startPos.x)/tileSize;//((-startX - tileSize * 2 + (int)nextStepPos.x )/tileSize);
//!					movesNum = steps+1;
//!					break;
//!				case 0: //					steps = 5;//Random.Range (randomMin, randomMax);
//!					movesNum = 5;
//!					break;
//!				case 1:
//!					movesNum = 5;
//!					break;
//!				}
//!			}


			nextStepPos += ( Directions[direction] * tileSize * ahead );

			steps--;

			if (isDelayOn) yield return new WaitForSeconds(0.01f);

			//			Debug.Log (rotationHelper.eulerAngles);
		}

//		Debug.Log (" linesData: "+ System.String.Join ("\n", lines.Select(kv => kv.Key).ToArray())+";");
//		DisableAdjacent ();

//		this.gameObject.GetComponentInParent<>().
//		Destroy(GameObject.FindWithTag("Cover"));
//		newTile.GetComponent<TileTouch>().paintTile(TileTouch.State.disabled);
		cover.GetComponent<BoxCollider2D>().enabled = false;
		Destroy(cover);
		TileTouch.instance.paintTile( TileTouch.State.disabled );
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


	void DisableAdjacentArea() {
			
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

