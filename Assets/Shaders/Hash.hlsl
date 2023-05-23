#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
StructuredBuffer<uint> _Hashes;
#endif

float4 _Config;

float3 GetHashColor() {
#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
	uint hash = _Hashes[unity_InstanceID];
	return (1.0 / 255.0) * float3(
		hash & 255,
		(hash >> 8) & 255,
		(hash >> 16) & 255
		);
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
	float v = floor(_Config.y * unity_InstanceID + 0.00001);
	float u = unity_InstanceID - _Config.x * v;

	unity_ObjectToWorld = 0.0;
	unity_ObjectToWorld._m03_m13_m23_m33 = float4(
		_Config.y * ((u*.06) + 0.5) - 0.5,
		_Config.z * ((1.0 / 255.0) * (_Hashes[unity_InstanceID] >> 24) - 0.5),
		_Config.y * ((v*.06) + 0.5) - 0.5,
		1.0
		);
	unity_ObjectToWorld._m00_m11_m22 = _Config.y;
#endif
}

void ShaderGraphFunction_float(float3 In, out float3 Out, out float3 Color) {
	Out = In;
	Color = GetHashColor();
}

void ShaderGraphFunction_half(half3 In, out half3 Out, out half3 Color) {
	Out = In;
	Color = GetHashColor();
}