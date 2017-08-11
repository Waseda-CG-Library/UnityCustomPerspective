float EnableCustomMatrix = 0; //false:0, true:1 lerp()ÇÃÇΩÇﬂÇ…floatå^
float4x4 CUSTOM_MATRIX_P;

float4 ObjectToCustomClipPos(float4 local)
{
	float4 proj = mul(CUSTOM_MATRIX_P, mul(UNITY_MATRIX_MV, local));
	float4 unityProj = UnityObjectToClipPos(local);

	return lerp(unityProj, proj, EnableCustomMatrix);
}
