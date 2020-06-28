
Shader "Custom/Blur"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_pow("Pow", Int) = 20
	}
		SubShader
	{

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			int _pow;

			float4 box(sampler2D tex, float2 uv, float4 size)
			{
				/*float4 c = tex2D(tex, uv + float2(-size.x, size.y)) + tex2D(tex, uv + float2(0, size.y)) + tex2D(tex, uv + float2(size.x, size.y)) +
							tex2D(tex, uv + float2(-size.x, 0)) + tex2D(tex, uv + float2(0, 0)) + tex2D(tex, uv + float2(size.x, 0)) +
							tex2D(tex, uv + float2(-size.x, -size.y)) + tex2D(tex, uv + float2(0, -size.y)) + tex2D(tex, uv + float2(size.x, -size.y));*/

				float4 c;
				for (int i = 0; i < _pow; i++) {
					for (int j = 0; j < _pow; j++) {
						c += tex2D(tex, uv + float2(size.x * i, size.y * j));
						//c += tex2D(tex, uv + float2(-size.x * i, -size.y * j));
					}
				}

				return c / _pow / _pow;
			}

			float4 frag(v2f i) : SV_Target
			{
				float4 col = box(_MainTex, i.uv, _MainTex_TexelSize);
				return col;
			}
			ENDCG
		}
	}
}