Shader "Hidden/CustomPerspective/ViewPos"
{
    Properties
    {
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

            #define CUSTOM_PERSPECTIVE_ON
            #pragma multi_compile _ CUSTOM_PERSPECTIVE_ON
            #include "CustomPerspective.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float3 viewPos : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = ObjectToCustomClipPos(v.vertex);
                o.viewPos = UnityObjectToViewPos(v.vertex);
                o.viewPos.z = -o.viewPos.z;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.viewPos, 1);
            }
            ENDCG
        }
    }
}
