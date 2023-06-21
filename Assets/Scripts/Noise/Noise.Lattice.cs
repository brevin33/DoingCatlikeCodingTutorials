using System.Runtime.CompilerServices;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public static partial class Noise
{

    public struct Lattice1D<L, G> : INoise
        where L : struct, ILattice where G : struct, IGradient
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sample4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            LatticeSpan4 x = default(L).GetLatticeSpan4(positions.c0, frequency);

            var g = default(G);
            return g.EvaluateAfterInterpolation(lerp(
                g.Evaluate(hash.Eat(x.p0), x.g0).v,
                g.Evaluate(hash.Eat(x.p1), x.g1).v, x.t
            ));
        }
    }

    public struct Lattice2D<L, G> : INoise
        where L : struct, ILattice where G : struct, IGradient
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sample4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            LatticeSpan4
                x = l.GetLatticeSpan4(positions.c0, frequency),
                z = l.GetLatticeSpan4(positions.c2, frequency);

            SmallXXHash4 h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1);

            var g = default(G);
            return g.EvaluateAfterInterpolation(lerp(
                lerp(
                    g.Evaluate(h0.Eat(z.p0), x.g0, z.g0).v,
                    g.Evaluate(h0.Eat(z.p1), x.g0, z.g1).v,
                    z.t
                ),
                lerp(
                    g.Evaluate(h1.Eat(z.p0), x.g1, z.g0).v,
                    g.Evaluate(h1.Eat(z.p1), x.g1, z.g1).v,
                    z.t
                ),
                x.t
            ));
        }
    }

    public struct Lattice3D<L, G> : INoise
        where L : struct, ILattice where G : struct, IGradient
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Sample4 GetNoise4(float4x3 positions, SmallXXHash4 hash, int frequency)
        {
            var l = default(L);
            LatticeSpan4
                x = l.GetLatticeSpan4(positions.c0, frequency),
                y = l.GetLatticeSpan4(positions.c1, frequency),
                z = l.GetLatticeSpan4(positions.c2, frequency);

            SmallXXHash4
                h0 = hash.Eat(x.p0), h1 = hash.Eat(x.p1),
                h00 = h0.Eat(y.p0), h01 = h0.Eat(y.p1),
                h10 = h1.Eat(y.p0), h11 = h1.Eat(y.p1);

            var g = default(G);
            return g.EvaluateAfterInterpolation(lerp(
                lerp(
                    lerp(
                        g.Evaluate(h00.Eat(z.p0), x.g0, y.g0, z.g0).v,
                        g.Evaluate(h00.Eat(z.p1), x.g0, y.g0, z.g1).v,
                        z.t
                    ),
                    lerp(
                        g.Evaluate(h01.Eat(z.p0), x.g0, y.g1, z.g0).v,
                        g.Evaluate(h01.Eat(z.p1), x.g0, y.g1, z.g1).v,
                        z.t
                    ),
                    y.t
                ),
                lerp(
                    lerp(
                        g.Evaluate(h10.Eat(z.p0), x.g1, y.g0, z.g0).v,
                        g.Evaluate(h10.Eat(z.p1), x.g1, y.g0, z.g1).v,
                        z.t
                    ),
                    lerp(
                        g.Evaluate(h11.Eat(z.p0), x.g1, y.g1, z.g0).v,
                        g.Evaluate(h11.Eat(z.p1), x.g1, y.g1, z.g1).v,
                        z.t
                    ),
                    y.t
                ),
                x.t
            ));
        }
    }
}