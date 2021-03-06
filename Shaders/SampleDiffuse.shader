﻿Shader "CustomPerspective/SampleDiffuse"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"

			#pragma multi_compile _ CUSTOM_PERSPECTIVE_ON
			#include "CustomPerspective.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				fixed4 diff : COLOR0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			uniform float4 _MainTex_ST;

			v2f vert (appdata_base v)
			{
				v2f o;

				o.vertex = ObjectToCustomClipPos(v.vertex);

				o.uv = v.texcoord;
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
				o.diff = nl * _LightColor0;
				o.diff.rgb += ShadeSH9(half4(worldNormal, 1));
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = float2(i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				fixed4 col = tex2D(_MainTex, uv);
				col.xyz *= i.diff;
				return col;
			}
			ENDCG
		}
	}

	Fallback "Hidden/CustomPerspective/Vertex"
}
