Shader "Custom/FlameTest2"
{
	Properties
	{
		_MainColor ("Main Color", Color) = (0,0,0,1)
		_SecondaryColor("Secondary Color", Color) = (0,0,0,1)
		_Slide ("Slide", Range(0,1)) = 1
		_AMax("Alpha Mul", Range(0,1)) = 1
		_Speed("Speed", Range(0,10)) = 1
		_Amp("Amplitude", Range(0,10)) = 1
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "IgnoreProjector"="True"}
		//ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha
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
				half2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			float4 _MainColor;
			float4 _SecondaryColor;
			float _Slide;
			float _AMax;
			float _Speed;
			float _Amp;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				//if (o.vertex.y > 0) {
				//	o.vertex.x += 2;
				//}
				o.uv = v.uv;
				
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			float4 frag (v2f i) : SV_Target
			{
				// sample the texture
				//fixed4 col = tex2D(_MainTex, i.uv);
				// lerp color
				float t = length(i.uv - float2(0.5, 0.5)) * 2;
				if (t > 1) { t = 1; }
				t = 1 - t;

				float a;
				if (t > _Slide) {
					a = 1;
				}
				else {
					a = t / _Slide;
					a *= a;
				}
				if (a > _AMax) { a = _AMax; }
				_MainColor.a = a;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, _MainColor);
				return _MainColor;
			}
			ENDCG
		}
	}
}
