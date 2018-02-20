using UnityEngine;
//using System.Collections;

using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Linq;
using UnityEditor;


public class Spline : MonoBehaviour
{
	bool debug = false;

	[StructLayout(LayoutKind.Sequential, Pack=1)]
	[Serializable]
	public struct dot2d
	{
		public float x; //{ get; set; }
		public float y; //{ get; set; }

		public dot2d (float x, float y)
		{
			this.x = x;
			this.y = y;
		}
	};

	[SerializeField]
	float resolution = 0.5f;

	//	public dot2d[] Lines;
	//	public List<dot2d> Points;
	[SerializeField]
//	dot2d[] Points = new dot2d[400];
	Vector2[] points = new Vector2[400];

	[SerializeField]
	int pointsNumber;

//	Vector2 lastPos;
	//Has to be at least 4 points
	public Transform[] controlPointsList;

	public bool isLooping = true;

	//	List<int> Polies; 

	public List <int> lines;
//	public List <int> Segment;
//	List <int> inters_left;
//	List <int> inters_right;

	//	List<int> Lines = new List<int>();
	[SerializeField]
	public List< List<int> > polies = new List <List<int>>();

	[SerializeField]
	public List <int> segment;

	[SerializeField]
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

//	public float angle;

	void OnDrawGizmos()
	{	
		Gizmos.color = Color.cyan;
		pointsNumber = 0;
		lines.Clear ();
//		polies = new List< List<int> > { new List<int> () };

		Color[] colors = { Color.red, Color.green, Color.blue  };

		for (int i = 0; i < controlPointsList.Length; i++)
		{
						if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1) && !isLooping)
						{
							continue;
						}

			DisplayCatmullRomSpline(i);
		}

//		AddPoint(points[ 0 ]);
		lines.Add (0);//pointsNumber++);
		AddIntersections();



		int colorSwitch = 0; 

		for (int linesIterator = 1; linesIterator < lines.Count; linesIterator++) {
			colorSwitch = linesIterator % 3;
//
//			if (col == 0) {
//				col++;
//				continue;
//			}

			Gizmos.color = colors[ colorSwitch ];
			Handles.Label (new Vector3 (points [lines [linesIterator]].x, points [lines [linesIterator]].y+(-(linesIterator % 2)/5f), 0), lines [linesIterator].ToString());

//			Handles.Label (new Vector3 (Points [line].x, Points [line].y+(-(col % 2)/3f), 0), line.ToString());
////			Handles.CubeHandleCap (1,new Vector3 (Points [Lines [c1]].x, Points [Lines [c1]].y, 0), Quaternion.identity, .1f, EventType.Repaint);
//			Handles.ArrowHandleCap (2,new Vector3 (Points [line].x, Points [line].y+(-(c1 % 2)/3f), 0), Quaternion.identity, 2, EventType.Repaint);
//			Gizmos.DrawLine ( new Vector2 (Points[ start ].x, Points[ start ].y),
//				new Vector2 (Points[ line ].x, Points[ line ].y));
//			start = line;
			Gizmos.DrawLine ( points[ lines [linesIterator-1] ], points[ lines [linesIterator] ]);
		}

		FillSegments();
		CombinePolies ();
	}



	int ClampListPos(int pos)
	{
		if (pos < 0)
		{
			pos = controlPointsList.Length - 1;
		}

		if (pos > controlPointsList.Length)
		{
			pos = 1;
		}
		else if (pos > controlPointsList.Length - 1)
		{
			pos = 0;
		}

		return pos;
	}


	//Display a spline between 2 points derived with the Catmull-Rom spline algorithm
	void DisplayCatmullRomSpline(int pos)
	{		

		Vector2 p0 = controlPointsList[ClampListPos(pos - 1)].position;
		Vector2 p1 = controlPointsList[pos].position;
		Vector2 p2 = controlPointsList[ClampListPos(pos + 1)].position;
		Vector2 p3 = controlPointsList[ClampListPos(pos + 2)].position;

		Vector2 lastPos = p1;

		int loops = Mathf.FloorToInt(1f / resolution);

		//		init segment
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
		
//		Line LineToCheck;
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
					
					if (!((currentLineIndex == (lines.Count - 1)) && (linesIterator == 1))) {
						GetIntersectionXY (out intersectionXY, lineToCheck.Start, lineToCheck.End, currentLine.Start, currentLine.End);
						
						Handles.SphereHandleCap (2, intersectionXY, Quaternion.identity, 0.7f, EventType.Repaint);
					
						var lineStartIndex = lines [ linesIterator - 1 ];
						var lineEndIndex   = lines [ linesIterator 	   ];

						AddPoint (intersectionXY); 
						lines.Insert (currentLineIndex, pointsNumber);
						lines.Insert (linesIterator, pointsNumber);
//										pointsNumber += 1;
						linesIterator += 1; // was +2
						currentLineIndex += 2;
						pointsNumber++;
					}
				}
				linesIterator++;
			}
		currentLineIndex++;
		}

//		foreach (var poly in polies) { 
//			Debug.Log ( String.Join( ",", poly.Select( v => v.ToString() ).ToArray() ) );
//		}
//		foreach (var segment in segments) { 
//			Debug.Log ( String.Join( ",", segment.Select( v => v.ToString() ).ToArray() ) );
//		}
	}



	void FillSegments() {

		segments.Clear ();
		var tmpLines = lines;
		var intStartIndex = pointsNumber-3;// (int) lines.Last () - 2;//pointsNumber - 2;//lines.Count - points.Length;
//		Debug.Log ( "Lines: "+ String.Join( ",", lines.Select( v => v.ToString() ).ToArray() ) );	
//				Debug.Log (intStartIndex);
		////		intStartIndex = points.Length - intStartIndex * 2 - 1;
		int intersectionPos = -1;
		while ( (intersectionPos = tmpLines.FindIndex (1, e => e > intStartIndex)) > -1 ) {
		//			var intersectionPos = tmpLines.FindIndex (1, e => e > intStartIndex);
		//			Debug.Log (" Trying to find: "+intStartIndex+" intersectionPos: "+intersectionPos);

			try {
				segment = tmpLines.GetRange( 0, Math.Min (intersectionPos+1, tmpLines.Count)).ToList();
			}
			catch {
				Debug.LogError("Error: GetRange( 0, Math.Min ("+intersectionPos+1+", "+(tmpLines.Count-1)+")");
			}

			//				Debug.Log (" Found segment: " + String.Join( ",", segment.Select( v => v.ToString()).ToArray()));

			segments.Add( segment ); 

			try {
			tmpLines.RemoveRange (0, intersectionPos); 
			}
			catch {
				Debug.LogError ("Error in RemoveRange: Trying to remove elements from 0 to "+intersectionPos.ToString()); 
			}
		}
		tmpLines.Remove (0);
		segments[0] = tmpLines.Concat(segments.First()).ToList();
		//			segments.AddRange ( tmpLines.GetRange( 0, intersectionPos+1 ) );
		//		segments.Add (tmpLines);

		//		Debug.Log ( "Segments length: "+segments.Count() );		
		//		Debug.Log("Last segment: " + String.Join( ",", tmpLines.Select( v => v.ToString() ).ToArray() ) );
//		foreach (var segment in segments) { 
//			Debug.Log ( "Segments: "+String.Join( "/", segment.Select( v => v.ToString() ).ToArray() ) );
//		}
	}	

	void CombinePolies () {

		//		Debug.Log("Segments.Count: "+segments.Count);
// the following test works
		debug = true;
//	Debug.Log("Constructed poly clockwise: "+String.Join( ",", FindNextSegment(segments [2],segments [2].Last(),1).Select( v => v.ToString() ).ToArray()));

//		Handles.DrawPolyLine ();
		//		for (int segmIndex = 0; segmIndex < segments.Count; segmIndex++ ) {
		//
		//			int elementsCount 	= segments [segmIndex].Count;
		//		//			Debug.Log (String.Join (",", segments [segmIndex].Select (v => v.ToString ()).ToArray ()));
		//		//			Debug.Log(elementsCount);
		//			int elementToFind = segments[segmIndex].Last();//[elementsCount-1];
		//			//			int previousElement = segments [segmIndex] [elementsCount - 2];
		//
		//			List<List<int>> segmentsWOcurrent = segments;
		//			var currentSegment = segments [segmIndex];
		//	//			Debug.Log ("*****************"+segmIndex);
		//		//			Debug.Log ( String.Join (",", enumerateThrough.Select (v => v.ToString ()).ToArray ()));
		//			segmentsWOcurrent.Remove(currentSegment);
				//			{				Debug.LogWarning ("ElementToFind: "+elementToFind+"   Removed from segments: "+String.Join( ",", currentSegment.Select( v => v.ToString() ).ToArray() ));
				//			};
				//			try {
				//				enumerateThrough.RemoveAt(segmIndex);// ( segments [segmIndex].ToList() );
				//			}
				//			catch {
				//				Debug.LogError ("Error: Can't remove segments. ");
				//			}

					//			enumerateThrough.Remove ( segments [segmIndex+1] ); //Range(0, segments.Count - 1).

			//			Debug.Log(enumerateThrough.Count);

//				 use this snipet afterwards
		foreach ( List<int> currentSegment in segments ) {

			var poly = FindNextSegment (currentSegment, currentSegment.Last (), 1);

			Debug.Log ("CombinePolies: trying to output polies results");
			if (poly != null) {
				Debug.Log ("Constructed poly clockwise: " + String.Join (",", poly.Select (v => v.ToString ()).ToArray ()));
			}

			if (debug) Debug.LogError("Start");
			var poly1 = FindNextSegment (currentSegment, currentSegment.Last (), -1);
			if (poly1 != null) {
				Debug.Log("Constructed poly CounterClockWise: "+String.Join( ",", poly1.Select( v => v.ToString() ).ToArray()));
				Debug.Log ("///////////////////////////////");
			}
		}

		//				if ( sideSegment.Contains(elementToFind) == false ) { 
		//					Debug.Log ("Skipped: "+String.Join( ",", sideSegment.Select( v => v.ToString() ).ToArray() ));
		//					continue;
		//				}
		//							Debug.Log ( String.Join( "/", sideSegment.Select( v => v.ToString()).ToArray()));
		//
		//				float angle = FindDirection ( currentSegment, sideSegment );
		//			Debug.Log("Constructed poly clockwise: "+String.Join( ",", FindNextSegment(currentSegment).Select( v => v.ToString() ).ToArray()));
		//				Debug.Log ("Angle between "+String.Join( ",", currentSegment.Select( v => v.ToString() ).ToArray() )+" and "
		//					+String.Join( ",", sideSegment.Select( v => v.ToString() ).ToArray() )+" is "+angle.ToString() );
//							}
//						}
	}


	List<int> FindNextSegment (List <int> StartSegment, int stopElement, float direction) {

		segmentAngle storedSegment;
		storedSegment = new segmentAngle { Angle = 888f, SegmentNum = new List<int>(), Segment = new List<int>()};
		List<int> tmpSegment = new List<int>();
		int NodeElement = -1;

//					var adjacentSegments = segments; //.Except(arc);

		var segmentNode = StartSegment.Last();
	
			//		Debug.LogWarning("segmentNode = arc.Last(); issues");

//					try {
//						adjacentSegments.Remove(StartSegment);
//					}
//					catch {
//						Debug.LogWarning ("adjacentSegments.Remove(arc) issues");
//					}
			//		for (int segmIndex = 0; segmIndex < adjacentSegments.Count; segmIndex ++ ) {
		foreach (var currentSegment in segments ) {
			//			bool cont;
			//
			//			try { cont = currentSegment.Contains( segmentNode ); }
			//			catch {
			//				Debug.LogWarning ("currentSegment.Contains( segmentNode ) issues");
			//			} 
			if (currentSegment.Contains( segmentNode )) { 

				var tmpSegmentNum = new List<int> {segments.IndexOf(currentSegment)};

				var angle = GetAngle ( StartSegment, currentSegment ); //use reverse direction

				if (direction != Math.Sign (angle) || angle == 0 ) { 
					if (debug) Debug.Log ("Skip: direction = " + direction.ToString () + "  angle = " + Math.Sign (angle));//+String.Join( ",", sideSegment.Select( v => v.ToString() ).ToArray() ));
					continue;
				} 
				else {
					if (debug) Debug.Log ("Angle matches");
				}

				var angleAbs = Math.Abs (angle);

				if (angleAbs == 180) {
					angleAbs--;
				}

				// check for minimal angle that matches desired direction 
				if ( angleAbs < storedSegment.Angle ) { 
					storedSegment.Angle = angleAbs;
					storedSegment.SegmentNum = tmpSegmentNum;
					if (segmentNode == currentSegment.Last()) {
						currentSegment.Reverse ();
					}
					storedSegment.Segment = currentSegment;
					NodeElement = currentSegment.Last ();
					tmpSegment = tmpSegmentNum;
//					break;
				}
			}
		}
		if (debug) Debug.LogWarning ("Segments cycle finished. Checking  .Last() and stopElement: "+NodeElement.ToString()+" =? "+stopElement.ToString());
		if ( NodeElement != stopElement ) { // for unpredictable cases NodeElement can be separated in additional condition with storedSegment.Segment.Clear and return
			if (NodeElement == -1) {
				tmpSegment = new List<int>(); return tmpSegment;
			}
			if (debug) Debug.Log ("From segment: "+String.Join( ",", StartSegment.Select( v => v.ToString() ).ToArray() ));
			if (debug) Debug.Log ("To segment: "+String.Join( ",", storedSegment.Segment.Select( v => v.ToString() ).ToArray() ));
				//			Debug.LogWarning ("Next recursive cycle");
									//			var nextSegment = FindNextSegment (storedSegment.Segment, stopElement, direction);
									//			if (nextSegment != new List<int>()) {
				//			storedSegment.Segment.AddRange (FindNextSegment (storedSegment.Segment, stopElement, direction));
//			if (debug) {
//				Debug.Log ("Reverted storedSegment.Segment: "+String.Join( ",", storedSegment.Segment.Select( v => v.ToString() ).ToArray() ));
//			}
			tmpSegment.AddRange (FindNextSegment (storedSegment.Segment, stopElement, direction));

			//			}
		}
		return tmpSegment;//storedSegment.FoundSegment;
	}



	float GetAngle ( List<int> baseLineSegment, List<int> targetLineSegment ) {//Line line1, Line line2 ) {

		int[] elementIndex;
//		try {
			if ( targetLineSegment.First() == baseLineSegment.Last()) //determines a side which the target segment is connected from 
			elementIndex = new int[] { 0, 1 };
		else
			elementIndex = new int[] { targetLineSegment.Count -1, targetLineSegment.Count -2 }; 
//		}
//		catch {
//			Debug.LogError ("Error in FindDirection");
//		}

		var targetLine = new Line { Start = points [ targetLineSegment[ elementIndex[0] ]], End = points [ targetLineSegment[ elementIndex[1] ]] };

		int previousElement = baseLineSegment [ baseLineSegment.Count - 2 ];
		var baseLine = new Line { End = points [ previousElement ], Start = points [ baseLineSegment.Last() ] };

		var baseLineVect = new Vector2 (baseLine.End.x - baseLine.Start.x, baseLine.End.y - baseLine.Start.y);
		var targetLineVect = new Vector2 (targetLine.End.x - targetLine.Start.x, targetLine.End.y - targetLine.Start.y);

		float angle = Vector2.SignedAngle (baseLineVect, targetLineVect);
		if (debug) Debug.Log ("From "+previousElement+"-"+baseLineSegment.Last()+"  To  "+targetLineSegment[ elementIndex[0] ]+"-"+targetLineSegment[ elementIndex[1] ]+"  Angle: "+angle.ToString ());
//		Debug.Log ("Angle: "+angle.ToString ());
		
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

		points [pointsNumber] = point;//new dot2d { x=Point.x, y=Point.y };
		return pointsNumber;
	}


	//	public bool GetIntersectionXY (out Vector2 intersection, Vector2 linePoint1, Vector2 lineVec1, Vector2 linePoint2, Vector2 lineVec2){
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
//		Vector2 a = 2f * p1;
//		Vector2 b = p2 - p0;
//		Vector2 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
//		Vector2 d = -p0 + 3f * p1 - 3f * p2 + p3;
//
		Vector2 pp2 = p2 + p2;
		Vector2 a = p1 + p1;
		Vector2 b = p2 - p0;
		Vector2 c = p0 + p0 - a - a - p1 + pp2 + pp2 - p3;
		Vector2 d = -p0 + a + p1 - p2 - p2 - p2 + p3;
		

		float tt  = t * t;
		float ttt = tt * t;

		//The cubic polynomial: a + b * t + c * t^2 + d * t^3
		Vector2 pos = 0.5f * (a + (b * t) + (c * tt) + (d * ttt));

		//		Debug.Log("pos x/y: "+pos.x+" "+pos.y+"   pos:"+pos);

		return pos;
	}
}