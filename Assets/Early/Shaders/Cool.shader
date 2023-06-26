Shader "Fractal/Fractal Surface GPU" {

	Properties{
		_BaseColor("Base Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_Smoothness("Smoothness", Range(0,1)) = 0.5
	}

		SubShader{
			CGPROGRAM
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles
			#pragma surface ConfigureSurface Standard fullforwardshadows addshadow
			#pragma instancing_options assumeuniformscaling procedural:ConfigureProcedural
			#pragma editor_sync_compilation

			#pragma target 4.5


			struct Input {
				float3 worldPos;
			};

			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				StructuredBuffer<float3x4> _Matrices;
				#endif

				float _X;
				float _Y;
				float _Z;
				float2 _SequenceNumbers;

				float4 _ColorA, _ColorB;

				float4 GetFractalColor() {
					#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
						return lerp(
							_ColorA, _ColorB,
							frac(unity_InstanceID * _SequenceNumbers.x + _SequenceNumbers.y)
						);
					#else
						return _ColorA;
					#endif
				}

				float4x4 m_translate(float4x4 m, float3 v)
				{
					float x = v.x + m._m03, y = v.y + m._m13, z = v.z + m._m23;
					m[0][3] = x;
					m[1][3] = y;
					m[2][3] = z;
					return m;
				}

				void ConfigureProcedural() {
				#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
					float3x4 m = _Matrices[unity_InstanceID];
					unity_ObjectToWorld._m00_m01_m02_m03 = m._m00_m01_m02_m03;
					unity_ObjectToWorld._m10_m11_m12_m13 = m._m10_m11_m12_m13;
					unity_ObjectToWorld._m20_m21_m22_m23 = m._m20_m21_m22_m23;
					unity_ObjectToWorld._m30_m31_m32_m33 = float4(0.0, 0.0, 0.0, 1.0);
					unity_ObjectToWorld = m_translate(unity_ObjectToWorld,float3(_X, _Y, _Z));
				#endif
				}

				void ShaderGraphFunction_float(float3 In, out float3 Out, out float4 FractalColor) {
					Out = In;
					FractalColor = GetFractalColor();
				}

				void ShaderGraphFunction_half(half3 In, out half3 Out, out half4 FractalColor) {
					Out = In;
					FractalColor = GetFractalColor();
				}

				float _Smoothness;

			void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
				surface.Albedo = GetFractalColor().rgb;
				surface.Smoothness = _Smoothness;
			}
			ENDCG
	}

		FallBack "Diffuse"
}