#define SPLINE_MAX_POINTS 8

#define DECLARE_SPLINE(splineName) \
int splineName##PointCount; \
uniform float splineName##Points[SPLINE_MAX_POINTS]; \
uniform float2 splineName##Tangents[SPLINE_MAX_POINTS]; \
uniform float splineName##KnotVector[SPLINE_MAX_POINTS];

#define CALCULATE_POINT(splineName, t) SplineCalculatePoint(t, splineName##Points, splineName##Tangents, splineName##KnotVector, splineName##PointCount)

int SplineGetPointCount(int currentSplineCount)
{
	return min(SPLINE_MAX_POINTS, currentSplineCount);
}

int SplineCalculateSpan( float x, float knotVector[SPLINE_MAX_POINTS], int pointCount ) 
{
	x = clamp( x, 0, 1 );

	int left = 0;
	int right = SplineGetPointCount(pointCount);
	int mid = ( left + right ) / 2;

	int refc = 20;

	while ( x < knotVector[mid] || x > knotVector[mid + 1] ) {
		if ( --refc < 0 ) {
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

float SplineCalculatePoint(float t, float points[SPLINE_MAX_POINTS], float2 tangents[SPLINE_MAX_POINTS], float knotVector[SPLINE_MAX_POINTS], int pointCount)
{
	int knotSpan = SplineCalculateSpan( t, knotVector, pointCount );
	float knotRange = ( knotVector[knotSpan + 1] - knotVector[knotSpan] );
	t = ( t - knotVector[knotSpan] ) / knotRange;

	float p0 = points[knotSpan];
	float p1 = points[knotSpan + 1];
	float t1 = tangents[knotSpan].y;
	float t2 = tangents[knotSpan + 1].x;

	float tSquared = t * t;
	float threeTSquared = 3 * tSquared;
	float tCubed = t * tSquared;

	float h2 = -2 * tCubed + threeTSquared;
	float h1 = -h2 + 1;

	float h3 = tCubed - 2 * tSquared + t;
	float h4 = tCubed - tSquared;

	return h1 * p0 + h2 * p1 + t1 * h3 + t2 * h4;
}
