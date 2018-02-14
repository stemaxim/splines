﻿using UnityEngine;
//using System.Collections;

using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
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

	[SerializeField]
	public List< List<int> > segments = new List <List<int>>();

	struct Line {
		public Vector2 Start;
		public Vector2 End;
	}

	public struct segmentAngle {
		public float Angle;
		public List<int> Segment;
	}

	void OnDrawGizmos()
	{	
		Gizmos.color = Color.cyan;
		pointsNumber = 0;
		lines.Clear ();
		segments = new List< List<int> > { new List<int> () };
		polies = new List< List<int> > { new List<int> () };

		Color[] colors = { Color.red, Color.green, Color.blue  };

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
			lines.Add(pointsNumber++);
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

		var tmpLines = lines;
		var intStartIndex = pointsNumber - 2;//lines.Count - points.Length;
//		intStartIndex = points.Length - intStartIndex * 2 - 1;
		int intersectionPos;
		while ( (intersectionPos = tmpLines.FindIndex (1, e => e > intStartIndex)) > -1 ) {
//			var intersectionPos = tmpLines.FindIndex (1, e => e > intStartIndex);
//			Debug.Log (" Trying to find: "+intStartIndex+" intersectionPos: "+intersectionPos);

			segment = tmpLines.GetRange(0,intersectionPos+1);
//			Debug.Log (" Found segment: " + String.Join( ",", segment.Select( v => v.ToString() ).ToArray()) ) ;
			segments.Add ( segment ); 

			tmpLines.RemoveRange (0, intersectionPos); 
		}
//			segments.AddRange ( tmpLines.GetRange( 0, intersectionPos+1 ) );
		segments.Add (tmpLines);
//		Debug.Log("Last segment: "+String.Join( ",", tmpLines.Select( v => v.ToString() ).ToArray() ) );

		////
		Debug.Log ( "Segments length: "+segments.Count() );		
		foreach (var segment in segments) { 
			Debug.Log ( String.Join( "/", segment.Select( v => v.ToString() ).ToArray() ) );
		}
	}	

	void CompoundPolies () {

		for (int i = 0; i < segments.Count; i++ ) {
			int elementsCount 	= segments [i].Count;
			int elementToFind 	= segments [i] [elementsCount - 1];
			int previousElement = segments [i] [elementsCount - 2];


		}
	}

	void FindNextLeaf (out List<int> hull, List <int> arc, int stopElement, float direction) {

		segmentAngle storedSegment = new { Angle = 888, Segment = List<int>() };

		for (int segmIndex = 0; segmIndex < segments.Count; segmIndex ++ ) {
			if (segments[segmIndex].Contains( arc.Last )) { 

				if ( arc == segments [segmIndex] ) continue;

				Line target;

				if (segments [segmIndex] [0] = arc.Last ) { //elementToFind) {
					target = new Line (points [segments [segmIndex] [0]], points [segments [segmIndex] [1]]);
				}
				else
				{ 	
					var segmElementsNumber = segments [segmIndex].Count;
					target = new Line (points [segments [segmIndex] [segmElementsNumber]], points [segments [segmIndex] [segmElementsNumber - 1]]);		
				};

//				int elementsCount 	= arc.Count;
				int previousElement = arc [ arc.Count - 2 ];


				var baseline = new Line ( points [ previousElement ], points [ arc.Last ]);
				var angle = Math.Abs( FindDirection ( baseline, target ) ); //use reverse direction

				if (direction != Math.Sign(angle) ) continue;

				var angleAbs = Math.Abs (angle);

				if ( angleAbs < storedSegment.Angle ) { 
					storedSegment.Angle = angleAbs;
					storedSegment.Segment = segments [segmIndex];
				}
			}
		}
		hull.AddRange( storedSegment.Segment );

		if (arc.Last != stopElement) {
			FindNextLeaf ( hull, storedSegment.Segment, stopElement, direction );
		}
	}

	float FindDirection ( Line line1, Line line2 ) {

		var baseline = new Vector2 (line1.End.x - line1.Start.x, line1.End.y - line1.Start.y);
		var target = new Vector2 (line2.End.x - line2.Start.x, line2.End.y - line2.Start.y);
		float angle = Vector2.SignedAngle (baseline, target);
		Debug.Log ("angle: "+angle);
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