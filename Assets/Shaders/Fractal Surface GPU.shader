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

			#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
				StructuredBuffer<float4x4> _Matrices;
			#endif

				//float _Step;

				void ConfigureProcedural() {
					#if defined(UNITY_PROCEDURAL_INSTANCING_ENABLED)
						unity_ObjectToWorld = _Matrices[unity_InstanceID];
					#endif
				}

				void ShaderGraphFunction_float(float3 In, out float3 Out) {
					Out = In;
				}

				void ShaderGraphFunction_half(half3 In, out half3 Out) {
					Out = In;
				}

			struct Input {
				float3 worldPos;
			};

			float4 _BaseColor;
			float _Smoothness;

			void ConfigureSurface(Input input, inout SurfaceOutputStandard surface) {
				surface.Albedo = _BaseColor.rgb;
				surface.Smoothness = _Smoothness;
			}
			ENDCG
	}

		FallBack "Diffuse"
}