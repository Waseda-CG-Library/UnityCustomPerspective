float EnableCustomMatrix = 0; //false:0, true:1 lerp()ÇÃÇΩÇﬂÇ…floatå^
float4x4 CUSTOM_MATRIX_P;
float4x4 MATRIX_I_VP;

float4 ObjectToCustomClipPos(float4 local)
{
	float4 proj = mul(CUSTOM_MATRIX_P, mul(UNITY_MATRIX_MV, local));
	float4 unityProj = UnityObjectToClipPos(local);
	proj = float4(proj.xy / proj.w * unityProj.w, unityProj.zw);

	return lerp(unityProj, proj, EnableCustomMatrix);
}

float4 _ObjectToCustomClipPos(float4 local)
{
	float4 proj = mul(CUSTOM_MATRIX_P, mul(UNITY_MATRIX_MV, local));
	float4 unityProj = UnityObjectToClipPos(local);
	return float4(proj.xy / proj.w * unityProj.w, unityProj.zw);
}

float4 ObjectToCustomWorldPos(float4 local)
{
	float4 customProj = _ObjectToCustomClipPos(local);
	float4 customWorld = mul(MATRIX_I_VP, customProj);
	float4 unityWorld = mul(unity_ObjectToWorld, local);

	return lerp(unityWorld, customWorld, EnableCustomMatrix);
}

float4 ObjectToCustomObjectPos(float4 local)
{
	float4 customProj = _ObjectToCustomClipPos(local);
	float4 customLocal = mul(unity_WorldToObject, mul(MATRIX_I_VP, customProj));

	return lerp(local, customLocal, EnableCustomMatrix);
}
