using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


using static Unity.Mathematics.math;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

public class cool : MonoBehaviour
{

    [SerializeField, Range(1, 8)]
    int depth = 4;

    [SerializeField]
    Mesh mesh;

    [SerializeField]
    Material material;

    [SerializeField]
    Gradient gradientA, gradientB;

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct UpdateFractalLevelJob : IJobFor
    {

        public float spinAngleDelta;
        public float scale;

        [ReadOnly]
        public NativeArray<FractalPart> parents;

        public NativeArray<FractalPart> parts;

        [WriteOnly]
        public NativeArray<float4x4> matrices;

        public void Execute(int i)
        {
            FractalPart parent = parents[i / 5];
            FractalPart part = parts[i];
            part.spinAngle += spinAngleDelta;
            part.worldRotation = mul(parent.worldRotation,
                mul(part.rotation, quaternion.RotateY(part.spinAngle))
            );
            part.worldPosition =
                parent.worldPosition +
                mul(parent.worldRotation, 1.5f * scale * part.direction);
            parts[i] = part;

            matrices[i] = float4x4.TRS(
                part.worldPosition, part.worldRotation, float3(scale)
            );
        }
    }

    static readonly int matricesId = Shader.PropertyToID("_Matrices"),
        colorAId = Shader.PropertyToID("_ColorA"),
        colorBId = Shader.PropertyToID("_ColorB");

    struct FractalPart
    {
        public float3 direction, worldPosition;
        public quaternion rotation, worldRotation;
        public float spinAngle;
    }

    static float3[] directions = {
        up(), right(), left(), forward(), back()
    };

    static quaternion[] rotations = {
        quaternion.identity,
        quaternion.RotateZ(-0.5f * PI), quaternion.RotateZ(0.5f * PI),
        quaternion.RotateX(0.5f * PI), quaternion.RotateX(-0.5f * PI)
    };

    NativeArray<FractalPart>[] parts;

    NativeArray<float4x4>[] matrices;

    ComputeBuffer[] matricesBuffers;

    void OnEnable()
    {
        sequenceNumbers = new Vector4[depth];
        parts = new NativeArray<FractalPart>[depth];
        matrices = new NativeArray<float4x4>[depth];
        matricesBuffers = new ComputeBuffer[depth];
        int stride = 16 * 4;
        for (int i = 0, length = 1; i < parts.Length; i++, length *= 5)
        {
            parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            matrices[i] = new NativeArray<float4x4>(length, Allocator.Persistent);
            matricesBuffers[i] = new ComputeBuffer(length, stride);
            sequenceNumbers[i] = new Vector4(Random.value, Random.value);
        }
        parts[0][0] = CreatePart(0);
        for (int li = 1; li < parts.Length; li++)
        {
            NativeArray<FractalPart> levelParts = parts[li];
            for (int fpi = 0; fpi < levelParts.Length; fpi += 5)
            {
                for (int ci = 0; ci < 5; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(ci);
                }
            }
        }
    }

    void OnDisable()
    {
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].Release();
            parts[i].Dispose();
            matrices[i].Dispose();
        }
        parts = null;
        matrices = null;
        sequenceNumbers = null;
        matricesBuffers = null;
    }

    void OnValidate()
    {
        if (parts != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    static readonly int xId = Shader.PropertyToID("_X"),
        sequenceNumbersId = Shader.PropertyToID("_SequenceNumbers"),
        yId = Shader.PropertyToID("_Y"),
        zId = Shader.PropertyToID("_Z");

    Vector4[] sequenceNumbers;
    void Update()
    {
        float spinAngleDelta = 0.125f * PI * Time.deltaTime;

        FractalPart rootPart = parts[0][0];
        rootPart.spinAngle += spinAngleDelta;
        rootPart.worldRotation = mul(transform.rotation,
            mul(rootPart.rotation, quaternion.RotateY(rootPart.spinAngle))
        );
        rootPart.worldPosition = Vector3.zero;
        parts[0][0] = rootPart;
        float objectScale = transform.lossyScale.x;
        matrices[0][0] = float4x4.TRS(
            rootPart.worldPosition, rootPart.worldRotation, float3(objectScale)
        );

        float scale = objectScale;
        JobHandle jobHandle = default;
        for (int li = 1; li < parts.Length; li++)
        {
            scale *= 0.5f;
            var job = new UpdateFractalLevelJob
            {
                spinAngleDelta = spinAngleDelta,
                scale = scale,
                parents = parts[li - 1],
                parts = parts[li],
                matrices = matrices[li]
            };
            jobHandle = job.Schedule(parts[li].Length, jobHandle);
        }
        jobHandle.Complete();
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            matricesBuffers[i].SetData(matrices[i]);
        }
        var bounds = new Bounds(rootPart.worldPosition, 3f * objectScale * Vector3.one);
        float3 p = transform.position;
        float[] pos = new float[3];
        pos[0] = p.x; pos[1] = p.y; pos[2] = p.z;
        for (int i = 0; i < matricesBuffers.Length; i++)
        {
            ComputeBuffer buffer = matricesBuffers[i];
            buffer.SetData(matrices[i]);
            material.SetFloatArray("_Position", pos);
            float gradientInterpolator = i / (matricesBuffers.Length - 1f);
            material.SetColor(colorAId, gradientA.Evaluate(gradientInterpolator));
            material.SetColor(colorBId, gradientB.Evaluate(gradientInterpolator));
            material.SetBuffer(matricesId, buffer);
            material.SetFloat(xId, transform.position.x);
            material.SetFloat(yId, transform.position.y);
            material.SetFloat(zId, transform.position.z);
            material.SetVector(sequenceNumbersId, sequenceNumbers[i]);
            Graphics.DrawMeshInstancedProcedural(
                mesh, 0, material, bounds, buffer.count
            );
        }
    }

    FractalPart CreatePart(int childIndex) => new FractalPart
    {
        direction = directions[childIndex],
        rotation = rotations[childIndex]
    };
}