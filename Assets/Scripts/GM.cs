using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using System.Linq;
//using System;
using System;
using UnityEngine.UI;


[RequireComponent(typeof(LineRenderer))]
public class GM : MonoBehaviour {
	static GM _instance;
	public static GM Instance{
		get {return _instance;}
	}
	void Awake(){
		_instance = this;
		lineRenderer = GetComponent<LineRenderer> ();
	}

	LineRenderer lineRenderer;
	//public LineRenderer lineDebug;
//	public GameObject lineDebugPrefab;
	public GameObject emptyPrefab;
	public GameObject hullPrefab = null;

	[Range(0f,20f)]
	public float radiusField = 10f;
	[Range(0f,30f)]
	public int countControlPoints = 12;

//	public List<Transform> controlPointsList = new List<Transform>();

	[SerializeField]
	public float resolution = 0.25f;


//	[StructLayout(LayoutKind.Sequential, Pack=1)]
//	[Serializable]


	[SerializeField]
	Vector2[] points = new Vector2[400];

	[SerializeField]
	int pointsNumber;

	public float minX, maxX, minY, maxY;
//	public Transform[] controlPointsList;
	public List < Transform > controlPointsList = new List < Transform >();
	public bool isLooping = true;


	public List <int> lines;

	public struct poliesInfo
	{
		public string hash;
		public List<int> segments;
		public float size;
		public Vector2[] points;
		public bool meshObj;
	}

	[SerializeField]
	List < poliesInfo > polies = new List < poliesInfo >();

//	[SerializeField]
	public List< List<int> > segments = new List <List<int>>();

	struct Line {
		public Vector2 Start;
		public Vector2 End;
	}

	public struct segmentAngle {
		public float Angle;
		public List<int> SegmentNum;
		public List<int> Segment;
	}

	public  HashSet <string> uniqPolies = new HashSet <string>();


	public float drawDelay = 0.1f;

	[NonSerialized]
	public int touches;

	bool debug = true;

	[SerializeField]
	public Text touchesTxt;

	void Start () {
		
		GenerateControlPoints (countControlPoints);
		StartCoroutine (GenerateStart());
		buildPolies ();
	}


	void GenerateControlPoints(int count = 20){

//		controlPointsList.Clear ();
		pointsNumber = 0;
//		lines.Clear ();

		Vector2 v = Vector2.zero;
		for (int i = 0; i < count; i++) {
			
			v = UnityEngine.Random.insideUnitCircle * radiusField;

			GameObject go = Instantiate (emptyPrefab, v, Quaternion.identity) as GameObject;
			go.name = "dP" + i.ToString ();
		
			controlPointsList.Add (go.transform);
		}

		for (int i = 0; i < controlPointsList.Count(); i++){
			DisplayCatmullRomSpline(i);
		}
		lines.Add (0);

		Debug.LogError ("lines.Count: :" + lines.Count);
	}


	IEnumerator GenerateStart(){
		yield return null;

		lineRenderer.loop = false;
		lineRenderer.positionCount = 1;
		lineRenderer.SetPosition (0, points[lines[0]]);
		for (int i = 1; i < lines.Count(); i++) {			
			yield return new WaitForSeconds(drawDelay);

			lineRenderer.positionCount = i + 1;
			lineRenderer. SetPosition (i, points[lines[i]]);
		}
		yield return new WaitForSeconds(drawDelay);
		lineRenderer.loop = true;
		yield return new WaitForSeconds(drawDelay);
	}


	void buildPolies() {

		AddIntersections();
		FillSegments();
		CombinePolies();
		if (debug) 
			Debug.LogError ("finished");
	}

//	void OnDrawGizmos()
//	{	
//		Gizmos.color = Color.cyan;
//		pointsNumber = 0;
//		lines.Clear ();
//
//
//		Color[] colors = { Color.red, Color.green, Color.blue  };
//
//		for (int i = 0; i < controlPointsList.Length; i++)
//		{
//			if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1) && !isLooping)
//			{
//				continue;
//			}
//
//			DisplayCatmullRomSpline(i);
//		}
//		lines.Add (0);
//		AddIntersections();
//
//
//
//		int colorSwitch = 0; 
//
//		for (int linesIterator = 1; linesIterator < lines.Count; linesIterator++) {
//			colorSwitch = linesIterator % 3;
//
//			Gizmos.color = colors[ colorSwitch ];
////			Handles.Label (new Vector3 (points [lines [linesIterator]].x, points [lines [linesIterator]].y+(-(linesIterator % 2)/5f), 0), lines [linesIterator].ToString());
//
//			Gizmos.DrawLine ( points[ lines [linesIterator-1] ], points[ lines [linesIterator] ]);
//		}
//
//		FillSegments();
//		CombinePolies();
//	}



	int ClampListPos(int pos)
	{
		if (pos < 0)
		{
			pos = controlPointsList.Count - 1;
		}

		if (pos > controlPointsList.Count)
		{
			pos = 1;
		}
		else if (pos > controlPointsList.Count - 1)
		{
			pos = 0;
		}

		return pos;
	}


//	int ClampListPos(int pos){
//		pos = (pos + controlPointsList.Count) % controlPointsList.Count;
//		return pos;
//	}


	void DisplayCatmullRomSpline(int pos)
	{		

		Vector2 p0 = controlPointsList[ClampListPos(pos - 1)].position;
		Vector2 p1 = controlPointsList[pos].position;
		Vector2 p2 = controlPointsList[ClampListPos(pos + 1)].position;
		Vector2 p3 = controlPointsList[ClampListPos(pos + 2)].position;

		Vector2 lastPos = p1;

		int loops = Mathf.FloorToInt(1f / resolution);

		for (int i = 1; i <= loops; i++)
		{
			float t = i * resolution;

			Vector2 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

			AddPoint(lastPos);
			lines.Add (pointsNumber++);
			lastPos = newPos;
		}
	}


	void AddIntersections () {

		Line lineToCheck, currentLine;
		Vector2 intersectionXY;

		int currentLineIndex = 2;
		while ( currentLineIndex < lines.Count ) {

			int linesIterator = 1;

			while ( linesIterator < currentLineIndex-2 ) {

				lineToCheck.Start = points[ lines[ linesIterator-1 ]]; //
				lineToCheck.End   = points[ lines[ linesIterator   ]]; //PointIndex

				currentLine.Start = points[ lines[ currentLineIndex-1 ]];
				currentLine.End   = points[ lines[ currentLineIndex  ]];

				if ( CheckIntersections ( currentLine, lineToCheck ) ) { 

					if (!((currentLineIndex == (lines.Count() - 1)) && (linesIterator == 1))) {
						GetIntersectionXY (out intersectionXY, lineToCheck.Start, lineToCheck.End, currentLine.Start, currentLine.End);

//						Handles.SphereHandleCap (2, intersectionXY, Quaternion.identity, 0.7f, EventType.Repaint);

						var lineStartIndex = lines [ linesIterator - 1 ];
						var lineEndIndex   = lines [ linesIterator 	   ];

						AddPoint (intersectionXY); 
						lines.Insert (currentLineIndex, pointsNumber);
						lines.Insert (linesIterator, pointsNumber);

						linesIterator += 1; 
						currentLineIndex += 2;
						pointsNumber++;
					}
				}
				linesIterator++;
			}
			currentLineIndex++;
		}
		if (debug)
			Debug.LogError ("Addintersections finished");
	}



	void FillSegments() {


//		var _lines = lines;
//		if (debug)
//			Debug.LogError ("FillSegments start\n"+" last lines element: "+_lines.Last().ToString());
			
		List<int> segment = new List<int> ();
//		List<int> tmpListsConcat = new List<int> ();


		var tmpLines = lines;
		int intStartIndex = tmpLines[tmpLines.Count() - 2];
		if (debug)
			Debug.LogError ("intStartIndex: " + intStartIndex);

		int intersectionPos = -1;
		while ( (intersectionPos = tmpLines.FindIndex (1, e => ( e > intStartIndex ))) > -1 ) {

//			try {
			segment = tmpLines.GetRange( 0, Math.Min (intersectionPos+1, tmpLines.Count()-1)).ToList();
//			}
//			catch {
//				Debug.LogWarning("Error: GetRange( 0, Math.Min ("+intersectionPos+1+", "+(tmpLines.Count-1)+")");
//			}

			segments.Add( segment ); 
			if (debug)
				Debug.Log (" segments: "+ String.Join (",", segment.Select(v => v.ToString()).ToArray())+";");

//			try {
				tmpLines.RemoveRange (0, intersectionPos); 
//			}
//			catch {
//				Debug.LogWarning ("Error in RemoveRange: Trying to remove elements from 0 to "+intersectionPos.ToString()); 
//			}
		}
		tmpLines.Remove (0);

		segment = segments[0];

		List<int> tmpListsConcat = tmpLines.Concat (segment).ToList();
		segments[0] = tmpListsConcat;
	}	


/// <summary>
/// ///////
/// </summary>
	void CombinePolies () {

//		polies.Clear ();

		if (debug) {
			Debug.Log ("Combine start");
		}

		foreach ( List<int> currentSegment in segments ) {

			if (debug) Debug.LogWarning("---Combine polies start---");

			var poly = FindNextSegment (currentSegment, currentSegment.Last (), 1);
			Debug.Log ("CombinePolies: trying to output polies results");

			if (poly.Count > 0) {
				Debug.Log ("Constructed poly clockwise: " + String.Join (",", poly.Select (v => v.ToString ()).ToArray ()));
				AddPoly (1,poly);
			}

			var poly1 = FindNextSegment (currentSegment, currentSegment.Last (), -1);

			if (poly.Count > 0) {
				Debug.Log("Constructed poly CounterClockWise: "+String.Join( ",", poly1.Select( v => v.ToString() ).ToArray()));

				AddPoly (-1,poly1);
			}
		}
/*		var tmpSegment = segments[0];
		if (tmpSegment.Count == 0)
			return;

//		Debug.Log ("Segments[0]: " + String.Join (",", segments[0].Select (v => v.ToString ()).ToArray ()));

		var poly = FindNextSegment (tmpSegment, tmpSegment.Last (), -1);
		Debug.Log ("CombinePolies: trying to output polies results");

		if ( poly.Count > 0 ) {
			Debug.Log ("Constructed poly clockwise: " + String.Join (",", poly.Select (v => v.ToString ()).ToArray ()));
			AddPoly (-1, poly);
		}

//		Debug.Log ("Segments[0]: " + String.Join (",", segments[0].Select (v => v.ToString ()).ToArray ()));
		poly = FindNextSegment (tmpSegment, tmpSegment.Last (), 1);
		Debug.Log ("CombinePolies: trying to output polies results");

		if ( poly.Count > 0 ) {
			Debug.Log ("Constructed poly clockwise: " + String.Join (",", poly.Select (v => v.ToString ()).ToArray ()));
			AddPoly (1,poly);
		}
*/
		FilterPolies ();
	}


	void FilterPolies() {

//		if (debug) {
//			foreach (var tmpPoly in polies ) {
//				Debug.Log ("Polies without duplicates: id = "+tmpPoly.hash.ToString()+" segments: "+ String.Join (",", tmpPoly.segments.Select(v => v.ToString()).ToArray())+";");
////				createHullMesh (tmpPoly);
//			}
		if (debug) Debug.LogError("polies.Count: "+polies.Count);
		if (polies.Count > 0)
			createHullMesh (polies[0]);
//		}
	}



	void AddPoly ( int dir, List<int> poly ) {

		float area = 0;
		var  elements = new List<int>();


		if ( poly.Count == 0 ) return;

		if ( poly.GroupBy( k => Math.Abs(k) ).Any( g => ( g.Count()>1 ) ) ) return;

		var tmpPoly = poly.Select( v => v = Math.Abs(v) ).ToList();

		tmpPoly.Sort();

		string uniqId = String.Join (",", tmpPoly.Select( v => v.ToString() ).ToArray());

		if (uniqPolies.Add (uniqId)) {

			elements.Clear ();

			foreach ( int segment in poly ){  

				int index = 0;
				if (segment < 0) {
					index = segment * -1;
					segments [index].Reverse ();
				}
				elements.AddRange (segments [index]);
			}

			var dots = elements.GroupBy (k => k).Select( v =>  points [ v.First() ] ).ToArray();

			area = calculateArea (dots);

			if (area < 0) {
				dots.Reverse ();
			}
			Debug.LogErrorFormat ("Adding poly: {0} {1} ", uniqId, area);

			polies.Add ( new poliesInfo { hash = uniqId, segments = poly, size = area, points = dots, meshObj = false } );
		}
	}

	/// <summary>
	/// ///
	/// </summary>
	/// <returns>The area.</returns>
	/// <param name="points">Points.</param>
	float calculateArea (Vector2[] points) {

		float area = 0;

		int arraySize = points.Length;

		Array.Resize (ref points, arraySize+1);
		points [arraySize] = points [0];

		for ( var i = 0; i < arraySize; i++) {
			area += points [i].x * points [i + 1].y;
			area -= points [i].y * points [i + 1].x;
		}

		return area/2;
	}

	/// <summary>
	/// //
	/// </summary>
	/// <param name="poly">Poly.</param>
	void createHullMesh ( poliesInfo poly ) {

		GameObject newHull;
		MeshFilter mFilter;
		MeshRenderer mRenderer;
		MeshCollider mCollider;
		Mesh mesh = null;

		var dotsNumber = poly.points.Length;

		Triangulator tr = new Triangulator( poly.points );
		int[] triangles = tr.Triangulate();


		var vertices = new Vector3[ dotsNumber ];

		vertices = poly.points.Select (v => ( new Vector3 (v.x, v.y, 0)) ).ToArray();
//		var normals = Enumerable.Repeat( Vector3.forward, dotsNumber).ToArray (); 

		var colors = Enumerable.Repeat (Color.red, dotsNumber).ToArray ();

		if ( poly.meshObj ) {
			newHull = GameObject.Find("hull["+poly.hash+"]");
		} else {
//			Debug.LogError ("Can't get name of stored poly mesh");
			newHull = (GameObject)Instantiate (hullPrefab, gameObject.transform) as GameObject;
			newHull.name = ("hull["+poly.hash+"]");
		}

//		Debug.LogError ("Creating hull: hull["+poly.hash+"]");


		GameObject go = Instantiate (emptyPrefab, poly.points[0], Quaternion.identity) as GameObject;
		go.name = newHull.name;

//		Debug.LogError ("Created hull: "+newHull.name);

		mRenderer = newHull.GetComponent (typeof(MeshRenderer)) as MeshRenderer;
		mFilter = newHull.GetComponent (typeof(MeshFilter)) as MeshFilter;
		mCollider = newHull.GetComponent (typeof(MeshCollider)) as MeshCollider;

		mesh = mFilter.mesh;

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.colors = colors;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();

		mCollider.sharedMesh = null;
		mCollider.sharedMesh = mesh;
//		newHull.AddComponent<MeshCollider> ().sharedMesh = mesh;

		poly.meshObj = true;
	}


	/// <summary>
	/// /
	/// </summary>
	/// <returns>The next segment.</returns>
	/// <param name="StartSegment">Start segment.</param>
	/// <param name="stopElement">Stop element.</param>
	/// <param name="direction">Direction.</param>
	List<int> FindNextSegment (List <int> StartSegment, int stopElement, float direction) {

		segmentAngle storedSegment;
		storedSegment  = new segmentAngle { Angle = 888f, SegmentNum = new List<int>(), Segment = new List<int>()};
		var tmpSegment = new List<int>();
		var tmpResult  = new List<int>();
		int NodeElement = -1;


		var segmentNode = StartSegment.Last();


		foreach (var currentSegment in segments ) { 
			if (currentSegment.Contains( segmentNode )) { 
				var angle = GetAngle ( StartSegment, currentSegment );

				if ( (direction != Math.Sign (angle)) || (angle == 0) ){//|| Math.Abs(angle) == 180  ) { 
					if (debug) Debug.Log ("Skip: direction = " + direction.ToString () + "  angle = " + Math.Sign (angle));//+String.Join( ",", sideSegment.Select( v => v.ToString() ).ToArray() ));
					continue;
				} 
				else {
					if (debug) Debug.Log ("Angle matches");
				}

				var tmpSegmentNum = segments.IndexOf(currentSegment);

				var angleAbs = Math.Abs (angle);

				//				if (angleAbs == 180) {
				//					angleAbs--;
				//				}

				if ( angleAbs < storedSegment.Angle ) { 
					storedSegment.Angle = angleAbs;
					if (segmentNode == currentSegment.Last()) {
						currentSegment.Reverse ();
						tmpSegmentNum *= -1;
					}
					storedSegment.Segment = currentSegment;
					NodeElement = currentSegment.Last ();
					tmpSegment = new List<int> {tmpSegmentNum};
				}
			}
		}
		if (debug) Debug.LogWarning ("Segments cycle finished. Checking  .Last() and stopElement: "+NodeElement.ToString()+" =? "+stopElement.ToString());
		if ( NodeElement != stopElement ) {	
			if (NodeElement == -1) {
//				tmpSegment = new List<int>(); return tmpSegment;
				return new List<int>();
			}

			if (debug) Debug.Log ("From segment: "+String.Join( ",", StartSegment.Select( v => v.ToString() ).ToArray() ));
			if (debug) Debug.Log ("To segment: "+String.Join( ",", storedSegment.Segment.Select( v => v.ToString() ).ToArray() ));

			tmpResult = FindNextSegment (storedSegment.Segment, stopElement, direction);
			if (tmpResult.Count > 0) {
				tmpSegment.AddRange (tmpResult);
			} else {
				return new List<int>();
			}
		}
		return tmpSegment;
	}


	float GetAngle ( List<int> baseLineSegment, List<int> targetLineSegment ) {

	
		int[] elementIndex;

		if (targetLineSegment.First () == baseLineSegment.Last ()) { //determines a side which the target segment is connected from 
			elementIndex = new int[] { 0, 1 };
		} else {
			elementIndex	= new int[] { targetLineSegment.Count - 1, targetLineSegment.Count - 2 }; 
		}

		var targetLine 		= new Line { Start = points [ targetLineSegment[ elementIndex[0] ]], End = points [ targetLineSegment[ elementIndex[1] ]] };

		int previousElement = baseLineSegment [ baseLineSegment.Count - 2 ];
		var baseLine 		= new Line { End = points [ previousElement ], Start = points [ baseLineSegment.Last() ] };

		var baseLineVect 	= new Vector2 (baseLine.End.x - baseLine.Start.x, baseLine.End.y - baseLine.Start.y);
		var targetLineVect 	= new Vector2 (targetLine.End.x - targetLine.Start.x, targetLine.End.y - targetLine.Start.y);

		float angle 		= Vector2.SignedAngle (baseLineVect, targetLineVect);

		if (debug) Debug.Log ("From "+previousElement+"-"+baseLineSegment.Last()+"  To  "+targetLineSegment[ elementIndex[0] ]+"-"+targetLineSegment[ elementIndex[1] ]+"  Angle: "+angle.ToString ());

		return angle;
	}



	bool CheckIntersections ( Line l1, Line l2 )
	{
		double v1=( l2.End.x - l2.Start.x ) * ( l1.Start.y - l2.Start.y ) - ( l2.End.y - l2.Start.y ) * ( l1.Start.x - l2.Start.x );
		double v2=( l2.End.x - l2.Start.x ) * ( l1.End.y - l2.Start.y ) - ( l2.End.y - l2.Start.y ) * ( l1.End.x - l2.Start.x );
		double v3=( l1.End.x - l1.Start.x ) * ( l2.Start.y - l1.Start.y ) - ( l1.End.y - l1.Start.y ) * ( l2.Start.x - l1.Start.x );
		double v4=( l1.End.x - l1.Start.x ) * ( l2.End.y - l1.Start.y ) - ( l1.End.y - l1.Start.y )* ( l2.End.x - l1.Start.x );
		return ((v1*v2<=0) && (v3*v4<=0));
	}


	int AddPoint( Vector2 point) {

		GameObject go = Instantiate (emptyPrefab, point, Quaternion.identity) as GameObject;
		go.name = "PointPref["+pointsNumber+"]";

		minX = Mathf.Min (minX, point.x);
		maxX = Mathf.Max (maxX, point.x);
		minY = Mathf.Min (minY, point.y);
		maxY = Mathf.Max (maxY, point.y);
		points [pointsNumber] = point;
		return pointsNumber;
	}


	public static bool GetIntersectionXY(
		out Vector2 intersection,	
		Vector2 p1,
		Vector2 p2,
		Vector2 p3,
		Vector3 p4
	)
	{
		intersection = Vector2.zero;

		var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

		if (d == 0.0f)
		{
			return false;
		}

		var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
		var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

		if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f)
		{
			return false;
		}

		intersection.x = p1.x + u * (p2.x - p1.x);
		intersection.y = p1.y + u * (p2.y - p1.y);

		return true;
	}


	Vector2 GetCatmullRomPosition(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	{
		Vector2 pp2 = p2 + p2;
		Vector2 a = p1 + p1;
		Vector2 b = p2 - p0;
		Vector2 c = p0 + p0 - a - a - p1 + pp2 + pp2 - p3;
		Vector2 d = -p0 + a + p1 - p2 - p2 - p2 + p3;


		float tt  = t * t;
		float ttt = tt * t;

		Vector2 pos = 0.5f * (a + (b * t) + (c * tt) + (d * ttt));


		return pos;
	}

	void Update () {
		
	}

}
