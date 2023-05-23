using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.Rendering;
using UnityEngine;

using static Unity.Mathematics.math;

using Unity.Mathematics;
using UnityEngine.UIElements;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;
using Random = UnityEngine.Random;

[System.Serializable]
public struct SpaceTRS
{
    public float3 translation, rotation, scale;

    public float3x4 Matrix
    {
        get
        {
            float4x4 m = float4x4.TRS(
                translation, quaternion.EulerZXY(math.radians(rotation)), scale
            );
            return math.float3x4(m.c0.xyz, m.c1.xyz, m.c2.xyz, m.c3.xyz);
        }
    }
}

public class HashVisualization : MonoBehaviour
{
    static int
    hashesId = Shader.PropertyToID("_Hashes"),
    configId = Shader.PropertyToID("_Config");

    [SerializeField]
    SpaceTRS domain = new SpaceTRS
    {
        scale = 8f
    };

    [SerializeField]
    Mesh instanceMesh;

    [SerializeField]
    Material material;

    [SerializeField, Range(1, 512)]
    int resolution = 16;

    NativeArray<uint> hashes;

    ComputeBuffer hashesBuffer;

    MaterialPropertyBlock propertyBlock;

    public readonly struct SmallXXHash
    {
        readonly uint accumulator;



        public SmallXXHash(uint accumulator)
        {
            this.accumulator = accumulator;
        }

        public static implicit operator uint(SmallXXHash hash)
        {
            uint avalanche = hash.accumulator;
            avalanche ^= avalanche >> 15;
            avalanche *= primeB;
            avalanche ^= avalanche >> 13;
            avalanche *= primeC;
            avalanche ^= avalanche >> 16;
            return avalanche;
        }

        public static SmallXXHash Seed(int seed) => (uint)seed + primeE;


        public static implicit operator SmallXXHash(uint accumulator) =>
    new SmallXXHash(accumulator);

        public SmallXXHash Eat(int data) =>
            RotateLeft(accumulator + (uint)data * primeC, 17) * primeD;

        public SmallXXHash Eat(byte data) =>
            RotateLeft(accumulator + data * primeE, 11) * primeA;

        static uint RotateLeft(uint data, int steps) =>
            (data << steps) | (data >> 32 - steps);

        const uint primeA = 0b10011110001101110111100110110001;
        const uint primeB = 0b10000101111010111100101001110111;
        const uint primeC = 0b11000010101100101010111000111101;
        const uint primeD = 0b00100111110101001110101100101111;
        const uint primeE = 0b00010110010101100110011110110001;
    }



    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    struct HashJob : IJobFor
    {

        public int seed;

        public int resolution;

        public float invResolution;

        [WriteOnly]
        public NativeArray<uint> hashes;

        public SmallXXHash hash;

        public float3x4 domainTRS;
        public void Execute(int i)
        {
            float vf = floor(invResolution * i + 0.00001f);
            float uf = invResolution * (i - resolution * vf + 0.5f) - 0.5f;
            vf = invResolution * (vf + 0.5f) - 0.5f;

            float3 p = mul(domainTRS, float4(uf, 0f, vf, 1f));

            int u = (int)floor(p.x);
            int v = (int)floor(p.z);
            int w = (int)floor(p.z);


            hashes[i] = hash.Eat(u).Eat(v).Eat(w);
        }
    }

    [SerializeField]
    int seed;

    [SerializeField, Range(-2f, 2f)]
    float verticalOffset = 1f;
    void OnEnable()
    {
        int length = resolution * resolution;
        hashes = new NativeArray<uint>(length, Allocator.Persistent);
        hashesBuffer = new ComputeBuffer(length, 4);

        new HashJob
        {
            hashes = hashes,
            resolution = resolution,
            invResolution = 1f / resolution,
            hash = SmallXXHash.Seed(seed),
            domainTRS = domain.Matrix
        }.ScheduleParallel(hashes.Length, resolution, default).Complete();

        hashesBuffer.SetData(hashes);

        propertyBlock ??= new MaterialPropertyBlock();
        propertyBlock.SetBuffer(hashesId, hashesBuffer);
        propertyBlock.SetVector(configId, new Vector4(
            resolution, 1f / resolution, verticalOffset / resolution
        ));
        propertyBlock.SetVector(configId, new Vector4(resolution, 1f / resolution));
    }

    void OnDisable()
    {
        hashes.Dispose();
        hashesBuffer.Release();
        hashesBuffer = null;
    }

    void OnValidate()
    {
        if (hashesBuffer != null && enabled)
        {
            OnDisable();
            OnEnable();
        }
    }

    void Update()
    {
        Graphics.DrawMeshInstancedProcedural(
            instanceMesh, 0, material, new Bounds(Vector3.zero, Vector3.one),
            hashes.Length, propertyBlock
        );
    }
}

