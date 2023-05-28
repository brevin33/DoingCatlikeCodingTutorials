using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public static partial class Noise
{

    public struct LatticeSpan4
    {
        public int4 p0, p1;
        public float4 g0, g1;
        public float4 t;

        public interface ILattice
        {
            LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency);
        }

        public struct LatticeNormal : ILattice
        {

            public LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency)
            {
                coordinates *= frequency;
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
        }

        public struct LatticeTiling : ILattice
        {

            public LatticeSpan4 GetLatticeSpan4(float4 coordinates, int frequency)
            {
                coordinates *= frequency;
                float4 points = floor(coordinates);
                LatticeSpan4 span;
                span.p0 = (int4)points;
                span.g0 = coordinates - span.p0;
                span.g1 = span.g0 - 1f;

                span.p0 -= (int4)ceil(points / frequency) * frequency;
                span.p0 = select(span.p0, span.p0 + frequency, span.p0 < 0);
                span.p1 = span.p0 + 1;
                span.p1 = select(span.p1, 0, span.p1 == frequency);

                span.t = coordinates - points;
                span.t = span.t * span.t * span.t * (span.t * (span.t * 6f - 15f) + 10f);
                return span;
            }
        }
    }




    public interface INoise
    {
        float4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency);
    }

    [BurstCompile(FloatPrecision.Standard, FloatMode.Fast, CompileSynchronously = true)]
    public struct Job<N> : IJobFor where N : struct, INoise
    {

        [ReadOnly]
        public NativeArray<float3x4> positions;

        [WriteOnly]
        public NativeArray<float4> noise;

        public Settings settings;

        public float3x4 domainTRS;

        public void Execute(int i)
        {
            float4x3 position = domainTRS.TransformVectors(transpose(positions[i]));
            var hash = SmallXXHash4.Seed(settings.seed);
            int frequency = settings.frequency;
            float amplitude = 1f, amplitudeSum = 0f;
            float4 sum = 0f;

            for (int o = 0; o < settings.octaves; o++)
            {
                sum += amplitude * default(N).GetNoise4(position, hash + o, frequency);
                frequency *= settings.lacunarity;
                amplitude *= settings.persistence;
                amplitudeSum += amplitude;
            }
            noise[i] = sum / amplitudeSum;
        }

        public static JobHandle ScheduleParallel(
            NativeArray<float3x4> positions, NativeArray<float4> noise, //int seed,
            Settings settings, SpaceTRS domainTRS, int resolution, JobHandle dependency
        ) => new Job<N>
        {
            positions = positions,
            noise = noise,
            //hash = SmallXXHash.Seed(seed),
            settings = settings,
            domainTRS = domainTRS.Matrix,
        }.ScheduleParallel(positions.Length, resolution, dependency);
    }

    public delegate JobHandle ScheduleDelegate(
        NativeArray<float3x4> positions, NativeArray<float4> noise,
        Settings settings, SpaceTRS domainTRS, int resolution, JobHandle dependency
    );
}