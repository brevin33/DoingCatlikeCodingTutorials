using Unity.Mathematics;

using static Unity.Mathematics.math;
using static Noise;
using static Shapes;
using Unity.Collections;
using Unity.Jobs;

public static partial class Noise
{


    public struct Lattice1D : INoise
    {

        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash)
        {
            int4 p0 = (int4)floor(positions.c0);
            int4 p1 = p0 + 1;
            float4 v = (uint4)hash.Eat(p1) & 255;
            float4 t = positions.c0 - p0;
            return lerp(hash.Eat(p0).Floats01A, hash.Eat(p1).Floats01A, t) * 2f - 1f;

        }
    }

    public struct Lattice2D : INoise
    {

        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash)
        {
            LatticeSpan4
                x = GetLatticeSpan4(positions.c0), z = GetLatticeSpan4(positions.c2);
            SmallXXHash4 h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1);

            return lerp(
                    lerp(h0.Eat(z.p0).Floats01A, h0.Eat(z.p1).Floats01A, z.t),
                    lerp(h1.Eat(z.p0).Floats01A, h1.Eat(z.p1).Floats01A, z.t),
                    x.t
                ) * 2f - 1f;
        }
    }

    public struct Lattice3D : INoise
    {

        public float4 GetNoise4(float4x3 positions, SmallXXHash4 hash)
        {
            LatticeSpan4
                x = GetLatticeSpan4(positions.c0),
                y = GetLatticeSpan4(positions.c1),
                z = GetLatticeSpan4(positions.c2);

            SmallXXHash4
                h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1),
                h00 = h0.Eat(y.p0), h01 = h0.Eat(y.p1),
                h10 = h1.Eat(y.p0), h11 = h1.Eat(y.p1);


            return lerp(
                lerp(
                    lerp(h00.Eat(z.p0).Floats01A, h00.Eat(z.p1).Floats01A, z.t),
                    lerp(h01.Eat(z.p0).Floats01A, h01.Eat(z.p1).Floats01A, z.t),
                    y.t
                ),
                lerp(
                    lerp(h10.Eat(z.p0).Floats01A, h10.Eat(z.p1).Floats01A, z.t),
                    lerp(h11.Eat(z.p0).Floats01A, h11.Eat(z.p1).Floats01A, z.t),
                    y.t
                ),
                x.t
            ) * 2f - 1f;
        }
    }
}