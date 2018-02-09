using UnityEngine;
//using System.Collections;

//Interpolation between points with a Catmull-Rom spline
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
//using System.Collections;
using System.Linq;
using UnityEditor;


public class Spline : MonoBehaviour
{

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
//	dot2d IntersectionPoint;
//	int PointIndex = 0;

//	[StructLayout(LayoutKind.Sequential, Pack=1)]
//	struct Line
//	{
//		public dot2d Start;
//		public dot2d End;
//		public Line(Vector2 start, Vector2 end)
//		{
//			this.Start.x = (float) start.x;
//			this.Start.y = (float) start.y;
//			this.End.x = (float) end.x;
//			this.End.y = (float) end.y;
//		}
//	};
	struct Line {
		public Vector2 Start;
		public Vector2 End;
	}

	void OnDrawGizmos()
	{	
		Gizmos.color = Color.cyan;
		pointsNumber = 0;
//		intersectedPoly = 0;
		lines.Clear ();
//		polies.Clear ();
		polies = new List<List<int>> { new List<int> () };
//		Segment.Clear();
		//		Points.Clear(Points,0,Points.Length);
//		Points = new dot2d[400];

		Color[] colors = { Color.red, Color.green, Color.blue  };

		//		var Segment = new List<int>;

		//		Lines.Add( 0 );

		for (int i = 0; i < controlPointsList.Length; i++)
		{
						if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1) && !isLooping)
						{
							continue;
						}

			DisplayCatmullRomSpline(i);
		}

		AddPoint(points[ 0 ]);
		lines.Add(pointsNumber++);
		AddIntersections();


		int colorSwitch = 0; 
//		bool skip;

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

//		Vector2 newPos;

		Vector2 lastPos = p1;



//
//		int inters_position1;
//		int inters_position2;

		int loops = Mathf.FloorToInt(1f / resolution);

		//		init segment
		for (int i = 1; i <= loops; i++)
		{
			float t = i * resolution;

			Vector2 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

			//Main intersection cycle

//			Points [ ind ] = new dot2d { x = lastPos.x, y = lastPos.y };

		
			AddPoint(lastPos);
			lines.Add(pointsNumber++);

			lastPos = newPos;
			}
//			Gizmos.DrawLine(lastPos, newPos);
		}
	//		Points [ ind ] = new dot2d { x = lastPos.x, y = lastPos.y };



	void AddIntersections () {
		
//		Line LineToCheck;
		Line lineToCheck, currentLine;
		Vector2 intersectionXY;

//		List <int> segment;
		List <int> inters_left;
		List <int> inters_right;

		int insertStartPos;
		int insertEndPos;
		int headInsertSegmentPos = -1;
		int tailInsertSegmentPos = -1;

		int headInsertIndex = -1;
		int tailInsertIndex = -1;

		int intersectedPoly = -1;
		int previousIntersectedPoly = -1;
		int intersNum = 0;
		bool selfCross = false;
		bool previousIntersectionFound = false;
		int currentPolyIndex = 0;


		//			for (int c = 1; c < ind-3 ; c++) {

//		for (int CurrentLineIndex = 4; CurrentLineIndex < PointsNumber; CurrentLineIndex++) {
		int currentLineIndex = 2;
		segment = new List<int> {0,1};

		while ( currentLineIndex < lines.Count ) {

			int linesIterator = 1;

			segment.Add ( lines[ currentLineIndex-1 ] );

			while ( linesIterator < currentLineIndex-2 ) {

				lineToCheck.Start = points[ lines[ linesIterator-1 ]]; //
				lineToCheck.End   = points[ lines[ linesIterator   ]]; //PointIndex
				//				Debug.Log ("Lines ["+(c-1)+"] "+Points[ Lines [c-1] ].x+"/"+Points[ Lines[c-1] ].y);
				//				Handles.Label (new Vector3(newPos.x, newPos.y, 0), PointsNumber.ToString());
				currentLine.Start = points[ lines[ currentLineIndex-1 ]];
				currentLine.End   = points[ lines[ currentLineIndex  ]];


				//(new Line { Start.x = lastPos.x, Start.y = lastPos.y, End.x = newPos.x, End.y = newPos.y })
				if ( CheckIntersections ( currentLine, lineToCheck ) ) 
				{ 
					
					if (!((currentLineIndex == (lines.Count - 1)) && (linesIterator == 1))) {
						GetIntersectionXY (out intersectionXY, lineToCheck.Start, lineToCheck.End, currentLine.Start, currentLine.End);
						
						Handles.SphereHandleCap (2, intersectionXY, Quaternion.identity, 0.7f, EventType.Repaint);
					
						var lineStartIndex = lines [ linesIterator - 1 ];
						var lineEndIndex   = lines [ linesIterator 	   ];

						AddPoint (intersectionXY); //161
						lines.Insert (currentLineIndex, pointsNumber);
						lines.Insert (linesIterator, pointsNumber);
						segment.Add(pointsNumber);


						//						newPos = IntersectionXY;
//										pointsNumber += 1;
						linesIterator += 1; // was +2
						currentLineIndex += 2;


				
						// segment self-intersections detection 
//						Debug.Log ("lines[ linesIterator-1 ]/pointsNumber: "+lines[ linesIterator-1 ].ToString()+"/"+pointsNumber);
						if (segment.Contains( lineEndIndex )) { //Segment.Find(v => v == LinesIterator-2
							////						inters_position1 = Segment.IndexOf(c);
//							Debug.Log(lines[ linesIterator-1 ]);
								inters_left  = segment.TakeWhile ( v => v != lineEndIndex ).ToList();//Lines[ LinesIterator ] //PointsNumber ).ToList(); //(inters_position1); 
								inters_right = segment.SkipWhile ( v => v != lineEndIndex ).ToList();//.Skip (1);
		//						CurrentPolyIndex = 
								polies.Add (inters_right);
		//							Polies [ IntersectedPolyIndex+1 ] = inters_right;
		//						var ReversedRightList = inters_right;
		//							ReversedRightList.Reverse ();
								inters_right.Reverse ();
		//						Polies [IntersectedPolyIndex+2] = inters_left.Union(inters_right).ToList();//inters_right.Reverse() ).ToList();
								polies.Add ( inters_left.Union(inters_right).ToList() );
		////						IntersectedPolyIndex+=2;
		////						Polies [CurrentPolyIndex] = Segment;
		//						Segment = inters_left;
//								segment.Clear();
								segment = new List<int> {pointsNumber};//.Add (PointsNumber);//new List<int> {PointsNumber};
								previousIntersectedPoly = polies.Count() - 2; //+= 2;
								selfCross = true;
						} 
						else {
	//						FindIntersectionIndexInList
								var poliesNumber = polies.Count;

								for (int polyIndex = 0; polyIndex < poliesNumber; polyIndex++) {
									if (polies[polyIndex].Contains( lineEndIndex )) {

										intersectedPoly = polyIndex;
										var tmpPoly = polies [intersectedPoly];
										var tmpSegment = segment;
										//if (PreviousIntersectionFound) {

										tailInsertIndex = (tmpPoly.IndexOf ( lineStartIndex ) < tmpPoly.IndexOf ( lineEndIndex )) ? lineEndIndex : lineStartIndex;
											//(tmpPoly.Where ((v, v1) => v == lines [linesIterator - 1] && v1 == lines [linesIterator]).Count == 2) ? lines [linesIterator] : lines [linesIterator - 1];


									if (intersectedPoly == previousIntersectedPoly) {

										//find insertion. position
										tailInsertSegmentPos = tmpPoly.IndexOf (tailInsertIndex);
										// if direct dot elements order 
										if (tailInsertSegmentPos > headInsertSegmentPos) {
											//													var tmpPoly = polies [intersectedPoly];

											tmpPoly.RemoveRange (headInsertSegmentPos, tailInsertSegmentPos - headInsertSegmentPos);
											tmpPoly.InsertRange (headInsertSegmentPos, segment);

											tmpSegment.Reverse ();
											polies.Add (tmpPoly.Skip (headInsertSegmentPos).TakeWhile (ind => ind != tailInsertIndex).Concat (tmpSegment).ToList ()); 
											polies [intersectedPoly] = tmpPoly;//polies.Add (tmpPoly);
										} else {
											//											reversed dot elements order		(direct Segment order)
											polies.Add (tmpSegment.Concat (tmpPoly.Skip (tailInsertSegmentPos).TakeWhile (ind => ind != headInsertIndex)).ToList ());

											tmpPoly.RemoveRange (0, headInsertSegmentPos);
											tmpSegment.Reverse ();
											polies [intersectedPoly] = tmpSegment.Concat (tmpPoly).ToList (); //polies.Add ( tmpSegment+tmpPoly );
											//											tmpPoly.InsertRange (headInsertSegmentPos, Segment);
										}
									} else {
										//										Add left and right parts
										if (tailInsertSegmentPos > headInsertSegmentPos) {
											inters_left = tmpPoly.TakeWhile (v => v != tailInsertIndex).ToList ();
											tmpSegment.Reverse ();
											polies.Add (inters_left.Concat (tmpSegment).ToList ());

											inters_right = tmpPoly.SkipWhile (v => v != tailInsertIndex).ToList ();
											polies.Add (segment.Concat (inters_right).ToList ());
										} else {
											inters_left = tmpPoly.TakeWhile (v => v != headInsertIndex).ToList ();
											tmpSegment.Reverse ();
											polies.Add (inters_left.Concat (tmpSegment).ToList ());

											inters_right = tmpPoly.SkipWhile (v => v != headInsertIndex).ToList ();
											polies.Add (segment.Concat (inters_right).ToList ());
										}
									}
	//										if	previousIntersectionFound = true;
										segment = new List<int> {pointsNumber};
										headInsertIndex = (tmpPoly.IndexOf ( lineStartIndex ) < tmpPoly.IndexOf ( lineEndIndex )) ? lineEndIndex : lineStartIndex;
//										headInsertSegmentPos = Math.Max (tmpPoly.IndexOf ( lines [linesIterator - 1] ), tmpPoly.IndexOf ( lines [linesIterator] ));
										headInsertSegmentPos = tmpPoly.IndexOf (headInsertIndex);

										previousIntersectedPoly = intersectedPoly;
									}  // condition if poly was found
								} // polies list search cycle end 
							} // condition if segment contains intersection
							
							segment = new List<int> {pointsNumber};
							pointsNumber += 1;
	//					linesIterator += 2;
	//					currentLineIndex += 2;

					}
				}
				linesIterator++;
			}
		currentLineIndex++;
		}
		foreach (var poly in polies) { 
			Debug.Log ( String.Join( ",", poly.Select( v => v.ToString() ).ToArray() ) );
			//combi
		}
	}
	


	bool CheckIntersections ( Line l1, Line l2 )
	{
		double v1=(l2.End.x-l2.Start.x)*(l1.Start.y-l2.Start.y)-(l2.End.y-l2.Start.y)*(l1.Start.x-l2.Start.x);
		double v2=(l2.End.x-l2.Start.x)*(l1.End.y-l2.Start.y)-(l2.End.y-l2.Start.y)*(l1.End.x-l2.Start.x);
		double v3=(l1.End.x-l1.Start.x)*(l2.Start.y-l1.Start.y)-(l1.End.y-l1.Start.y)*(l2.Start.x-l1.Start.x);
		double v4=(l1.End.x-l1.Start.x)*(l2.End.y-l1.Start.y)-(l1.End.y-l1.Start.y)*(l2.End.x-l1.Start.x);
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