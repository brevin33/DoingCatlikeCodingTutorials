using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public static partial class Noise
{

    struct LatticeSpan4
    {
        public int4 p0, p1;
        public float4 g0, g1;
        public float4 t;
    }

    static LatticeSpan4 GetLatticeSpan4(float4 coordinates)
    {
        float4 points = floor(coordinates);
        LatticeSpan4 span;
        span.p0 = (int4)points;
        span.p1 = span.p0 + 1;
        span.t = coordinates - points;
        span.t = span.t * span.t * span.t * (span.t * (span.t * 6f - 15f) + 10f);
        span.g0 = coordinates - span.p0;
        span.g1 = span.g0 - 1f;
        return span;
    }
    public interface INoise
    {
        float4 GetNoise4(float4x3 positions, SmallXXHash4 hash);
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Job<N> : IJobFor where N : struct, INoise
    {

        [ReadOnly]
        public NativeArray<float3x4> positions;

        [WriteOnly]
        public NativeArray<float4> noise;

        public SmallXXHash4 hash;

        public float3x4 domainTRS;

        public void Execute(int i)
        {
            noise[i] = default(N).GetNoise4(
                domainTRS.TransformVectors(transpose(positions[i])), hash
            );
        }

        public static JobHandle ScheduleParallel(
            NativeArray<float3x4> positions, NativeArray<float4> noise,
            int seed, SpaceTRS domainTRS, int resolution, JobHandle dependency
        ) => new Job<N>
        {
            positions = positions,
            noise = noise,
            hash = SmallXXHash.Seed(seed),
            domainTRS = domainTRS.Matrix,
        }.ScheduleParallel(positions.Length, resolution, dependency);
    }

    public delegate JobHandle ScheduleDelegate(
        NativeArray<float3x4> positions, NativeArray<float4> noise,
        int seed, SpaceTRS domainTRS, int resolution, JobHandle dependency
    );
}