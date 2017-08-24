float4x4 CUSTOM_MATRIX_P;
float4x4 MATRIX_I_VP;

float4 _ObjectToCustomClipPos(float4 local)
{
	float4 proj = mul(CUSTOM_MATRIX_P, mul(UNITY_MATRIX_MV, local));
	float4 unityProj = UnityObjectToClipPos(local);
	return float4(proj.xy / proj.w * unityProj.w, unityProj.zw);
}

float4 ObjectToCustomClipPos(float4 local)
{
	#ifdef CUSTOM_PERSPECTIVE_ON
		return _ObjectToCustomClipPos(local);
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
