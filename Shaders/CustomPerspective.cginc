float4x4 CUSTOM_MATRIX_P;
float4x4 MATRIX_I_VP;

float4 _ObjectToCustomClipPos(float4 local)
{
	float4 view = float4(UnityObjectToViewPos(local), 1.0f);
	float4 proj = mul(CUSTOM_MATRIX_P, view);
	float4 unityProj = UnityObjectToClipPos(local);
	return float4(proj.xy / proj.w * unityProj.w, unityProj.zw);
}

float4 ObjectToCustomClipPos(float4 local)
{
	#ifdef CUSTOM_PERSPECTIVE_ON
		float4 view = float4(UnityObjectToViewPos(local), 1.0f);
		float4 proj = mul(CUSTOM_MATRIX_P, view);
		float4 unityProj = UnityObjectToClipPos(local);
		proj.z = unityProj.z / unityProj.w * proj.w;
		return proj;
	#else
		return UnityObjectToClipPos(local);
	#endif
}

float4 ObjectToCustomWorldPos(float4 local)
{
	#ifdef CUSTOM_PERSPECTIVE_ON
		float4 proj = _ObjectToCustomClipPos(local);
		return mul(MATRIX_I_VP, proj);
	#else
		return mul(unity_ObjectToWorld, local);
	#endif
}

float4 ObjectToCustomObjectPos(float4 local)
{
	#ifdef CUSTOM_PERSPECTIVE_ON
		float4 proj = _ObjectToCustomClipPos(local);
		return mul(unity_WorldToObject, mul(MATRIX_I_VP, proj));
	#else
		return local;
	#endif
}
