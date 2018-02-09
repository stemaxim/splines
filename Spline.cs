using UnityEngine;
//using System.Collections;

//Interpolation between points with a Catmull-Rom spline
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.Collections;
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
	dot2d[] Points = new dot2d[400];

	[SerializeField]
	int PointsNumber;
	int CurrentPolyIndex;
	int IntersNum = 0;
	bool SelfCross = false;

	Vector2 lastPos;
	//Has to be at least 4 points
	public Transform[] controlPointsList;

	public bool isLooping = true;

	//	List<int> Polies; 

	public List<int> Lines;
	public List<int> Segment;
	//	List<int> Lines = new List<int>();
	public List<int>[] Polies;

	dot2d IntersectionPoint;
//	int PointIndex = 0;

	[StructLayout(LayoutKind.Sequential, Pack=1)]
	struct Line
	{
		public dot2d Start;
		public dot2d End;
		public Line(Vector2 start, Vector2 end)
		{
			this.Start.x = (float) start.x;
			this.Start.y = (float) start.y;
			this.End.x = (float) end.x;
			this.End.y = (float) end.y;
		}
	};

	void OnDrawGizmos()
	{	
		Gizmos.color = Color.cyan;
		PointsNumber = 0;
		CurrentPolyIndex = 0;
		Lines.Clear();
		Segment.Clear();
		//		Points.Clear(Points,0,Points.Length);
//		Points = new dot2d[400];

		Color[] Colors = { Color.red, Color.green, Color.blue  };

		//		var Segment = new List<int>;

		//		Lines.Add( 0 );

		for (int i = 0; i < controlPointsList.Length; i++)
		{
						//Cant draw between the endpoints
						//Neither do we need to draw from the second to the last endpoint
						//...if we are not making a looping line
						if ((i == 0 || i == controlPointsList.Length - 2 || i == controlPointsList.Length - 1) && !isLooping)
						{
							continue;
						}

			DisplayCatmullRomSpline(i);
		}

//		AddIntersectionPoint(new Vector2 { x = Points[ Lines[0] ].x, y = Points[ Lines[0] ].y });
//
//		Lines.Add(ind++);

		Points[ PointsNumber-1 ].x = Points[ 0 ].x;
		Points[ PointsNumber-1 ].y = Points[ 0 ].y;
		Lines.Add((PointsNumber));

		int col = 0; 
//		int start = Lines[0]; 
		bool skip;

		for (int c1 = 1; c1 < PointsNumber; c1++) {
//		foreach ( int line in Lines ) {
			col = c1 % 3;
//
//			if (col == 0) {
//				col++;
//				continue;
//			}

			Gizmos.color = Colors[ col ];
			Handles.Label (new Vector3 (Points [Lines [c1]].x, Points [Lines [c1]].y+(-(c1 % 2)/3f), 0), c1.ToString());

//			Handles.Label (new Vector3 (Points [line].x, Points [line].y+(-(col % 2)/3f), 0), line.ToString());
////			Handles.CubeHandleCap (1,new Vector3 (Points [Lines [c1]].x, Points [Lines [c1]].y, 0), Quaternion.identity, .1f, EventType.Repaint);
//			Handles.ArrowHandleCap (2,new Vector3 (Points [line].x, Points [line].y+(-(c1 % 2)/3f), 0), Quaternion.identity, 2, EventType.Repaint);
//			Gizmos.DrawLine ( new Vector2 (Points[ start ].x, Points[ start ].y),
//				new Vector2 (Points[ line ].x, Points[ line ].y));
//			start = line;
			Gizmos.DrawLine ( new Vector2 (Points[ Lines [c1-1] ].x, Points[ Lines [c1-1] ].y),
							  new Vector2 (Points[ Lines [c1] ].x, Points[ Lines [c1] ].y));
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

		lastPos = p1;

		Line LineToCheck;
		Vector2 IntersectionXY;

		List<int> inters_left;
		List<int> inters_right;

		int inters_position1;
		int inters_position2;


//		if (!firstPoint) {
//			AddIntersectionPoint(new Vector2 { x = lastPos.x, y = lastPos.y });
//			Lines.Add(ind++);
//			firstPoint = false;
//		}

		//		List<Vector2> prefabXY;

//		AddIntersectionPoint(new Vector2 { x = lastPos.x, y = lastPos.y });
//		Lines.Add(ind++);
		//		Points [ ind ] = new dot2d { x = lastPos.x, y = lastPos.y };
		//		//			AddPoint( new dot2d { x = lastPos.x, y = lastPos.y } );
		//		//			Lines.Insert( ind, ind );
		//		Lines.Add(ind);
		//		ind++;

		int loops = Mathf.FloorToInt(1f / resolution);

		//		init segment
		for (int i = 1; i <= loops; i++)
		{
			float t = i * resolution;

			Vector2 newPos = GetCatmullRomPosition(t, p0, p1, p2, p3);

			//Main intersection cycle

//			Points [ ind ] = new dot2d { x = lastPos.x, y = lastPos.y };

		
			AddIntersectionPoint(new Vector2 { x = lastPos.x, y = lastPos.y });
			Lines.Add(PointsNumber++);

			int LinesIterator = 1;
			//			for (int c = 1; c < ind-3 ; c++) {
			while ( LinesIterator < PointsNumber-1 ) {

				//				/*var LineToCheck = {Start.x = lastPos.x;
				//								   Start.y = lastPos.y;
				//									End.x;
				//									End.y;}
				//				*/
				//				PointIndex++;
				LineToCheck.Start = Points[ Lines[   LinesIterator-1  ] ]; //
				LineToCheck.End   = Points[ Lines[   LinesIterator    ] ]; //PointIndex
//				Debug.Log ("Lines ["+(c-1)+"] "+Points[ Lines [c-1] ].x+"/"+Points[ Lines[c-1] ].y);
//				Handles.Label (new Vector3(newPos.x, newPos.y, 0), PointsNumber.ToString());

				//(new Line { Start.x = lastPos.x, Start.y = lastPos.y, End.x = newPos.x, End.y = newPos.y })
				if ( CheckIntersections ( new Line ( lastPos, newPos ), LineToCheck )) 
				{ 
					GetIntersectionXY ( out IntersectionXY, new Vector2 { x = LineToCheck.Start.x, y = LineToCheck.Start.y } , new Vector2 { x = LineToCheck.End.x, y = LineToCheck.End.y } , lastPos, newPos );
					Handles.SphereHandleCap (2, IntersectionXY, Quaternion.identity, 0.7f, EventType.Repaint);

					//					var tmpPoint = Lines [PointIndex];
					//					Lines [PointIndex] = AddIntersectionPoint (IntersectionXY + new Vector2 (-.2f, 0));
					//					Lines [PointIndex + 1] = tmpPoint;
//					Points.Equals
//					try {
						AddIntersectionPoint( IntersectionXY );
	//					Lines.Add( ind );
						Lines.Insert( LinesIterator, PointsNumber++ ); //+ new Vector2(-.2f,0) ) );
//						newPos = IntersectionXY;
						LinesIterator++; 
//					}
//					catch {
//					Debug.Log(" Inters: "+IntersectionXY.x+"/"+IntersectionXY.y+"Current Line index:"+(PointsNumber-1)+" Intersected Line index:"+(LinesIterator-1));
//					}
					//					#######
					//					newPos = IntersectionXY;
					//					if ( Segment.Find(v => v == Lines[c] ) ) {
					////						inters_position1 = Segment.IndexOf(c);
					//						inters_left = Segment.TakeWhile(v => v == Lines[c]);//(inters_position1);
					//						inters_right = Segment.SkipWhile(v => v == Lines[c]).Skip(1);
					//						Polies[CurrentPolyIndex] = inters_left;
					//						Polies[++CurrentPolyIndex] = inters_right.Insert(0, Lines[c]);
					//						SelfCross = true;
					//					}

					//					if ( IntersNum < 2 ) {
					//						IntersNum = 1; ....					
					//					}
					//					break;
				}
				LinesIterator++; //ind++;
			}
//			Gizmos.DrawLine(lastPos, newPos);

			lastPos = newPos;
//			ind++;
		}
	//		Points [ ind ] = new dot2d { x = lastPos.x, y = lastPos.y };
	}


	bool CheckIntersections ( Line L1, Line L2 )
	{
		double v1=(L2.End.x-L2.Start.x)*(L1.Start.y-L2.Start.y)-(L2.End.y-L2.Start.y)*(L1.Start.x-L2.Start.x);
		double v2=(L2.End.x-L2.Start.x)*(L1.End.y-L2.Start.y)-(L2.End.y-L2.Start.y)*(L1.End.x-L2.Start.x);
		double v3=(L1.End.x-L1.Start.x)*(L2.Start.y-L1.Start.y)-(L1.End.y-L1.Start.y)*(L2.Start.x-L1.Start.x);
		double v4=(L1.End.x-L1.Start.x)*(L2.End.y-L1.Start.y)-(L1.End.y-L1.Start.y)*(L2.End.x-L1.Start.x);
		return ((v1*v2<=0) && (v3*v4<=0));
	}


	int AddIntersectionPoint( Vector2 Point) {

		Points [ PointsNumber ] = new dot2d { x=Point.x, y=Point.y };
		return PointsNumber;
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
		Vector2 a = 2f * p1;
		Vector2 b = p2 - p0;
		Vector2 c = 2f * p0 - 5f * p1 + 4f * p2 - p3;
		Vector2 d = -p0 + 3f * p1 - 3f * p2 + p3;

//		Vector2 pp2 = p2 + p2;
//		Vector2 a = p1 + p1;
//		Vector2 b = p2 - p0;
//		Vector2 c = p0 + p0 - a - a - p1 + pp2 + pp2 - p3;
//		Vector2 d = -p0 + a + p1 - p2 - p2 - p2 + p3;
		

		float tt  = t * t;
		float ttt = tt * t;

		//The cubic polynomial: a + b * t + c * t^2 + d * t^3
		Vector2 pos = 0.5f * (a + (b * t) + (c * tt) + (d * ttt));

		//		Debug.Log("pos x/y: "+pos.x+" "+pos.y+"   pos:"+pos);

		return pos;
	}
}