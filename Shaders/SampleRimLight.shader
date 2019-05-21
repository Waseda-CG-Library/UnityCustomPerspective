Shader "CustomPerspective/SampleRimLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (.5, .5, .5, 1)
		Power("Power", Float) = 3.0
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
            #include "UnityCG.cginc"

			#pragma multi_compile _ CUSTOM_PERSPECTIVE_ON
			#include "CustomPerspective.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float3 normal : NORMAL0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				float3 normal : NORMAL0;
				float3 viewDir: POSITION1;
			};

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;

			float Power;
			float4 ViewDirectionCorrectWorld;

            v2f vert (appdata v)
            {
                v2f o;

				o.vertex = ObjectToCustomClipPos(v.vertex);	
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normal = UnityObjectToWorldNormal(v.normal);

				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.viewDir = _WorldSpaceCameraPos - (worldPos + ViewDirectionCorrectWorld).xyz;

				return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float3 normal = normalize(i.normal);
				float3 viewDir = normalize(i.viewDir);

				float rimLight = 1 - abs(dot(normal, viewDir));
				rimLight = pow(rimLight, Power);

				float2 uv = float2(i.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw);
				fixed4 col = rimLight + _Color * tex2D(_MainTex, uv);

                return col;
            }
            ENDCG
        }
    }

	Fallback "Hidden/CustomPerspective/Vertex"
}
