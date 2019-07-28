//Original is builtin_shaders-2017.1.0f3 Normal-VertexLit.shader

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license
/* ----------
Copyright (c) 2016 Unity Technologies

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
---------- */

Shader "Hidden/CustomPerspective/Vertex" {
		SubShader{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		// Pass to render object as a shadow caster
		Pass{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile_instancing // allow instanced shadow pass for most of the shaders
			#include "UnityCG.cginc"

			#pragma multi_compile _ CUSTOM_PERSPECTIVE_ON
			#pragma multi_compile _ CUSTOM_PERSPECTIVE_DEPTH_PATH CUSTOM_PERSPECTIVE_SHADOW_PATH
			#include "CustomPerspective.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			float _CustomPerspective_ShadowMapScale;

			v2f vert(appdata_base v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

				#if defined(CUSTOM_PERSPECTIVE_ON) && defined(CUSTOM_PERSPECTIVE_DEPTH_PATH)
					o.pos = ObjectToCustomClipPos(v.vertex);
				#elif defined(CUSTOM_PERSPECTIVE_SHADOW_PATH)
					float2 xy = o.pos.xy * _CustomPerspective_ShadowMapScale;
					o.pos.xy = lerp(xy, o.pos.xy, _WorldSpaceLightPos0.w);
				#endif

				return o;
			}

			float4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}

			ENDCG
		}
	}

	Fallback "Vertex"
}
