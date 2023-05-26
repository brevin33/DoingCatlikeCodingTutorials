#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
StructuredBuffer<float> _Noise;
StructuredBuffer<float3> _Positions, _Normals;
#endif

float4 _Config;

float3 GetNoiseColor() {
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	float noise = _Noise[unity_InstanceID];
	return noise < 0.0 ? float3(-noise, 0.0, 0.0) : noise;
#else
	return 1.0;
#endif
}

float4x4 m_scale(float4x4 m, float3 v)
{
	float x = v.x, y = v.y, z = v.z;

	m[0][0] *= x; m[1][0] *= y; m[2][0] *= z;
	m[0][1] *= x; m[1][1] *= y; m[2][1] *= z;
	m[0][2] *= x; m[1][2] *= y; m[2][2] *= z;
	m[0][3] *= x; m[1][3] *= y; m[2][3] *= z;

	return m;
}

void ConfigureProcedural() {
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	unity_ObjectToWorld = 0.0;
	unity_ObjectToWorld._m03_m13_m23_m33 = float4(
		_Positions[unity_InstanceID],
		1.0
		);
	unity_ObjectToWorld._m03_m13_m23 +=
		_Config.z * _Noise[unity_InstanceID] * _Normals[unity_InstanceID];
	unity_ObjectToWorld._m00_m11_m22 = _Config.y;
#endif
}

void ShaderGraphFunction_float(float3 In, out float3 Out, out float3 Color) {
	Out = In;
	Color = GetNoiseColor();
}

void ShaderGraphFunction_half(half3 In, out half3 Out, out half3 Color) {
	Out = In;
	Color = GetNoiseColor();
}