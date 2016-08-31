Shader "Unlit/HermiteSplineGPU"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			#define MAX_POINTS 4
			uniform float points[MAX_POINTS];
			uniform float2 tangents[MAX_POINTS];
			uniform float knotVector[MAX_POINTS];

			int CalculateSpan( float x ) 
			{
				x = clamp( x, 0, 1 );

				int left = 0;
				int right = MAX_POINTS;
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

			// int CalculateSpan(float t)
			// {
			// 	return floor(t * (MAX_POINTS - 1));
			// }

			float CalculatePoint(float t)
			{
				int knotSpan = CalculateSpan( t );
				float knotRange = ( knotVector[knotSpan + 1] - knotVector[knotSpan] );
				t = ( t - knotVector[knotSpan] ) / knotRange;

				float p0 = points[knotSpan];
				float p1 = points[knotSpan + 1];
				float t1 = tangents[knotSpan].x;
				float t2 = tangents[knotSpan + 1].y;

				float tSquared = t * t;
				float threeTSquared = 3 * tSquared;
				float tCubed = t * tSquared;

				float h2 = -2 * tCubed + threeTSquared;
				float h1 = -h2 + 1;

				float h3 = tCubed - 2 * tSquared + t;
				float h4 = tCubed - tSquared;

				return h1 * p0 + h2 * p1 + t1 * h3 + t2 * h4;
				//tangent = result / knotRange;
			}
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = CalculatePoint(frac(_Time.y));//((_Time.x) - floor(_Time.x));//CalculatePoint();
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
