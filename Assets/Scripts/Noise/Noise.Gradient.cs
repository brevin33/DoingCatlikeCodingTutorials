using System;
using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;

public static partial class Noise
{

    [Serializable]
    public struct Settings
    {

        public int seed;

        [Min(1)]
        public int frequency;
        [Range(1, 6)]
        public int octaves;
        [Range(2, 4)]
        public int lacunarity;
        [Range(0f, 1f)]
        public float persistence;

        public static Settings Default => new Settings
        {
            frequency = 4,
            octaves = 1,
            lacunarity = 2,
            persistence = 0.5f
        };
    }

    public struct Value : IGradient
    {

        public float4 Evaluate(SmallXXHash4 hash, float4 x) => hash.Floats01A * 2f - 1f;

        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) =>
            hash.Floats01A * 2f - 1f;

        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) =>
            hash.Floats01A * 2f - 1f;

        public float4 EvaluateAfterInterpolation(float4 value) => value;

    }

    public struct Perlin : IGradient
    {

        public float4 Evaluate(SmallXXHash4 hash, float4 x) =>
            (1f + hash.Floats01A) * select(-x, x, ((uint4)hash & 1 << 8) == 0);

        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y)
        {
            float4 gx = hash.Floats01A * 2f - 1f;
            float4 gy = 0.5f - abs(gx);
            gx -= floor(gx + 0.5f);
            return (gx * x + gy * y) * (2f / 0.53528f);
        }

        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z)
        {
            float4 gx = hash.Floats01A * 2f - 1f, gy = hash.Floats01D * 2f - 1f;
            float4 gz = 1f - abs(gx) - abs(gy);
            float4 offset = max(-gz, 0f);
            gx += select(-offset, offset, gx < 0f);
            gy += select(-offset, offset, gy < 0f);
            return (gx * x + gy * y + gz * z) * (1f / 0.56290f);
        }

        public float4 EvaluateAfterInterpolation(float4 value) => value;

    }

    public struct Turbulence<G> : IGradient where G : struct, IGradient
    {

        public float4 Evaluate(SmallXXHash4 hash, float4 x) =>
            default(G).Evaluate(hash, x);

        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y) =>
            default(G).Evaluate(hash, x, y);

        public float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z) =>
            default(G).Evaluate(hash, x, y, z);

        public float4 EvaluateAfterInterpolation(float4 value) =>
            abs(default(G).EvaluateAfterInterpolation(value));
    }
    public interface IGradient
    {
        float4 Evaluate(SmallXXHash4 hash, float4 x);

        float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y);

        float4 Evaluate(SmallXXHash4 hash, float4 x, float4 y, float4 z);

        float4 EvaluateAfterInterpolation(float4 value);
    }
}