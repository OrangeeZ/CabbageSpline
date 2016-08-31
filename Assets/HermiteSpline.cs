using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class HermiteSpline {
	[SerializeField]
	private int degree = 2;

	[SerializeField]
	private Vector3[] points = null;

	[SerializeField]
	private Vector3[] tangents = null;

	[SerializeField]
	private Vector3[] normals = null;

	//[SerializeField]
	//private float[] weights = null;

	[SerializeField]
	private float[] knotVector = null;

	[SerializeField]
	private bool isClosed = false;

	public HermiteSpline( int degree, Vector3[] points, Vector3[] tangents, Vector3[] normals, float[] weights, bool isClosed ) {
		this.degree = degree;
		this.points = points;
		this.tangents = tangents;
		//this.weights = weights;
		this.normals = normals;
		this.isClosed = isClosed;

		//if ( isClosed && points.Length > degree ) {
		//	var newPoints = points.ToList();
		//	var newWeights = weights.ToList();

		//	for ( var i = 0; i < degree; ++i ) {
		//		newPoints.Insert( i, points[points.Length - degree + i] );
		//		newWeights.Insert( i, weights[points.Length - degree + i] );
		//	}

		//	this.points = newPoints.ToArray();
		//	this.weights = newWeights.ToArray();
		//}

		knotVector = new float[this.points.Length];

		for ( var i = 0; i < knotVector.Length; ++i ) {

			knotVector[i] = (float)i / ( knotVector.Length - 1 );
		}
	}

	private float Clamp( float t ) {

		var shift = isClosed ? 0 : 1;
		var leftBound = degree - shift;
		var rightBound = points.Length + shift;

		return Mathf.Lerp( knotVector[leftBound], knotVector[rightBound], t );
	}

	private int CalculateSpan( float x ) {

		x = Mathf.Clamp01( x );

		var left = 0;
		var right = knotVector.Length - 1;
		var mid = ( left + right ) / 2;

		var refc = 20;

		while ( x < knotVector[mid] || x > knotVector[mid + 1] ) {
			if ( --refc < 0 ) {
				Debug.Log( x );
				break;
			}

			if ( x < knotVector[mid] ) {
				right = mid;
			} else {
				left = mid;
			}

			mid = ( left + right ) / 2;
		}

		return mid;
	}

	public Vector3 Evaluate( float t ) {

		var knotSpan = CalculateSpan( t );
		t = ( t - knotVector[knotSpan] ) / ( knotVector[knotSpan + 1] - knotVector[knotSpan] );

		var p1 = points[knotSpan];
		var p2 = points[knotSpan + 1];
		var t1 = tangents[knotSpan];
		var t2 = tangents[knotSpan + 1];

		var tSquared = t * t;
		var tCubed = t * tSquared;

		var h2 = -2 * tCubed + 3f * tSquared;
		var h1 = -h2 + 1;

		var h3 = tCubed - 2 * tSquared + t;
		var h4 = tCubed - tSquared;

		return h1 * p1 +
			   h2 * p2 +
			   t1 * h3 +
			   t2 * h4;
	}

	public Vector3 EvaluateDerivative( float t ) {

		var knotSpan = CalculateSpan( t );

		var p0 = points[knotSpan];
		var p1 = p0 + tangents[knotSpan] / 3f;

		var p3 = points[knotSpan + 1];
		var p2 = p3 - tangents[knotSpan + 1] / 3f;

		t = ( t - knotVector[knotSpan] ) / ( knotVector[knotSpan + 1] - knotVector[knotSpan] );

		var tSix = 6f * t;
		var tSquared = 3f * t * t;

		var result = ( 3f - tSix + tSquared ) * ( p1 - p0 ) +
					 ( tSix - 2f * tSquared ) * ( p2 - p1 ) +
					 tSquared * ( p3 - p2 );

		return result / ( knotVector[knotSpan + 1] - knotVector[knotSpan] );
	}

	public Vector3 EvaluateNormal( float t ) {

		var knotSpan = CalculateSpan( t );
		t = ( t - knotVector[knotSpan] ) / ( knotVector[knotSpan + 1] - knotVector[knotSpan] );

		return Vector3.Lerp( normals[knotSpan], normals[knotSpan + 1], t );
	}

	public void Evaluate( float t, out Vector3 point, out Vector3 tangent ) {

		var knotSpan = CalculateSpan( t );
		var knotRange = ( knotVector[knotSpan + 1] - knotVector[knotSpan] );
		t = ( t - knotVector[knotSpan] ) / knotRange;

		var p0 = points[knotSpan];
		var p1 = points[knotSpan + 1];
		var t1 = tangents[knotSpan];
		var t2 = tangents[knotSpan + 1];

		var tSquared = t * t;
		var threeTSquared = 3f * tSquared;
		var tCubed = t * tSquared;

		var h2 = -2 * tCubed + threeTSquared;
		var h1 = -h2 + 1;

		var h3 = tCubed - 2f * tSquared + t;
		var h4 = tCubed - tSquared;

		point = h1 * p0 +
			    h2 * p1 +
			    t1 * h3 +
			    t2 * h4;

		p1 = p0 + t1 / 3f;

		var p3 = points[knotSpan + 1];
		var p2 = p3 - t2 / 3f;

		var tSix = 6f * t;

		var result = ( 3f - tSix + threeTSquared ) * ( p1 - p0 ) +
					 ( tSix - 2f * threeTSquared ) * ( p2 - p1 ) +
					 threeTSquared * ( p3 - p2 );

		tangent = result / knotRange;
	}
}