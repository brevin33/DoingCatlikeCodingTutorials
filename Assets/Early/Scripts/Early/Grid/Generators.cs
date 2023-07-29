using Unity.Mathematics;
using UnityEngine;

using static Unity.Mathematics.math;
using static Unity.VisualScripting.LudiqRootObjectEditor;

namespace ProceduralMeshes.Generators
{

    public struct SquareGrid : IMeshGenerator
    {

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f));

        public int VertexCount => 4 * Resolution * Resolution;

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution;

        public int Resolution { get; set; }

        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            int vi = 4 * Resolution * z, ti = 2 * Resolution * z;

            for (int x = 0; x < Resolution; x++, vi += 4, ti += 2)
            {
                var xCoordinates = float2(x, x + 1f) / Resolution - 0.5f;
                var zCoordinates = float2(z, z + 1f) / Resolution - 0.5f;

                var vertex = new Vertex();
                vertex.normal.y = 1f;
                vertex.tangent.xw = float2(1f, -1f);

                vertex.position.x = xCoordinates.x;
                vertex.position.z = zCoordinates.x;
                streams.SetVertex(vi + 0, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.texCoord0 = float2(1f, 0f);
                streams.SetVertex(vi + 1, vertex);

                vertex.position.x = xCoordinates.x;
                vertex.position.z = zCoordinates.y;
                vertex.texCoord0 = float2(0f, 1f);
                streams.SetVertex(vi + 2, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.texCoord0 = 1f;
                streams.SetVertex(vi + 3, vertex);

                streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
                streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));
            }
        }
    }

    public struct SharedSquareGrid : IMeshGenerator
    {

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f));

        public int VertexCount => (Resolution + 1) * (Resolution + 1);

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution + 1;

        public int Resolution { get; set; }

        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            int vi = (Resolution + 1) * z, ti = 2 * Resolution * (z - 1);

            var vertex = new Vertex();
            vertex.normal.y = 1f;
            vertex.tangent.xw = float2(1f, -1f);

            vertex.position.x = -0.5f;
            vertex.position.z = (float)z / Resolution - 0.5f;
            vertex.texCoord0.y = (float)z / Resolution;
            streams.SetVertex(vi, vertex);

            vi += 1;

            for (int x = 1; x <= Resolution; x++, vi++, ti += 2)
            {
                vertex.position.x = (float)x / Resolution - 0.5f;
                vertex.texCoord0.x = (float)x / Resolution;
                streams.SetVertex(vi, vertex);

                if (z > 0)
                {
                    streams.SetTriangle(
                        ti + 0, vi + int3(-Resolution - 2, -1, -Resolution - 1)
                    );
                    streams.SetTriangle(
                        ti + 1, vi + int3(-Resolution - 1, -1, 0)
                    );
                }
            }
        }
    }

    public struct SharedSquare : IMeshGenerator
    {

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(1f, 0f, 1f));

        public int VertexCount => (Resolution + 1) * (Resolution + 1);

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution + 1;

        public int Resolution { get; set; }

        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            int vi = (Resolution + 1) * z, ti = 2 * Resolution * (z - 1);

            var vertex = new Vertex();
            vertex.normal.y = 1f;
            vertex.tangent.xw = float2(1f, -1f);

            vertex.position.x = -0.5f;
            vertex.position.z = (float)z / Resolution - 0.5f;
            vertex.texCoord0.y = (float)z / Resolution;
            streams.SetVertex(vi, vertex);

            vi += 1;

            for (int x = 1; x <= Resolution; x++, vi++, ti += 2)
            {
                vertex.position.x = (float)x / Resolution - 0.5f;
                vertex.texCoord0.x = (float)x / Resolution;
                streams.SetVertex(vi, vertex);

                if (z > 0)
                {
                    streams.SetTriangle(
                        ti + 0, vi + int3(-Resolution - 2, -1, -Resolution - 1)
                    );
                    streams.SetTriangle(
                        ti + 1, vi + int3(-Resolution - 1, -1, 0)
                    );
                }
            }
        }
    }

    public struct SharedTriangleGrid : IMeshGenerator
    {

        public Bounds Bounds => new Bounds(
            Vector3.zero, new Vector3(1f + 0.5f / Resolution, 0f, sqrt(3f) / 2f)
        );

        public int VertexCount => (Resolution + 1) * (Resolution + 1);

        public int IndexCount => 6 * Resolution * Resolution;

        public int JobLength => Resolution + 1;


        public int Resolution { get; set; }

        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            int vi = (Resolution + 1) * z, ti = 2 * Resolution * (z - 1);
            float xOffset = -0.25f;
            float uOffset = 0f;

            int iA = -Resolution - 2, iB = -Resolution - 1, iC = -1, iD = 0;
            var tA = int3(iA, iC, iD);
            var tB = int3(iA, iD, iB);

            if ((z & 1) == 1)
            {
                xOffset = 0.25f;
                uOffset = 0.5f / (Resolution + 0.5f);
                tA = int3(iA, iC, iB);
                tB = int3(iB, iC, iD);
            }
            xOffset = xOffset / Resolution - 0.5f;

            var vertex = new Vertex();
            vertex.normal.y = 1f;
            vertex.tangent.xw = float2(1f, -1f);

            vertex.position.x = xOffset;
            vertex.position.z = ((float)z / Resolution - 0.5f) * sqrt(3f) / 2f;
            vertex.texCoord0.x = uOffset;
            vertex.texCoord0.y = vertex.position.z / (1f + 0.5f / Resolution) + 0.5f;
            streams.SetVertex(vi, vertex);

            vi += 1;

            for (int x = 1; x <= Resolution; x++, vi++, ti += 2)
            {
                vertex.position.x = (float)x / Resolution + xOffset;
                vertex.texCoord0.x = x / (Resolution + 0.5f) + uOffset;
                streams.SetVertex(vi, vertex);

                if (z > 0)
                {
                    streams.SetTriangle(ti + 0, vi + tA);
                    streams.SetTriangle(ti + 1, vi + tB);
                }
            }
        }
    }

    public struct PointyHexagonGrid : IMeshGenerator
    {

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(
            (Resolution > 1 ? 0.5f + 0.25f / Resolution : 0.5f) * sqrt(3f),
            0f,
            0.75f + 0.25f / Resolution
        ));

        public int VertexCount => 7 * Resolution * Resolution;

        public int IndexCount => 18 * Resolution * Resolution;

        public int JobLength => Resolution;

        public int Resolution { get; set; }

        public void Execute<S>(int z, S streams) where S : struct, IMeshStreams
        {
            int vi = 7 * Resolution * z, ti = 6 * Resolution * z;
            float h = sqrt(3f) / 4f;

            float2 centerOffset = 0f;

            if (Resolution > 1)
            {
                centerOffset.x = (((z & 1) == 0 ? 0.5f : 1.5f) - Resolution) * h;
                centerOffset.y = -0.375f * (Resolution - 1);
            }


            for (int x = 0; x < Resolution; x++, vi += 7, ti += 6)
            {
                var center = (float2(2f * h * x, 0.75f * z) + centerOffset) / Resolution;
                var xCoordinates = center.x + float2(-h, h) / Resolution;
                var zCoordinates =
                    center.y + float4(-0.5f, -0.25f, 0.25f, 0.5f) / Resolution;
                var vertex = new Vertex();
                vertex.normal.y = 1f;
                vertex.tangent.xw = float2(1f, -1f);

                vertex.position.xz = center;
                vertex.texCoord0 = 0.5f;
                streams.SetVertex(vi + 0, vertex);

                vertex.position.z = zCoordinates.x;
                vertex.texCoord0.y = 0f;
                streams.SetVertex(vi + 1, vertex);

                vertex.position.x = xCoordinates.x;
                vertex.position.z = zCoordinates.y;
                vertex.texCoord0 = float2(0.5f - h, 0.25f);
                streams.SetVertex(vi + 2, vertex);

                vertex.position.z = zCoordinates.z;
                vertex.texCoord0.y = 0.75f;
                streams.SetVertex(vi + 3, vertex);

                vertex.position.x = center.x;
                vertex.position.z = zCoordinates.w;
                vertex.texCoord0 = float2(0.5f, 1f);
                streams.SetVertex(vi + 4, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.position.z = zCoordinates.z;
                vertex.texCoord0 = float2(0.5f + h, 0.75f);
                streams.SetVertex(vi + 5, vertex);

                vertex.position.z = zCoordinates.y;
                vertex.texCoord0.y = 0.25f;
                streams.SetVertex(vi + 6, vertex);

                streams.SetTriangle(ti + 0, vi + int3(0, 1, 2));
                streams.SetTriangle(ti + 1, vi + int3(0, 2, 3));
                streams.SetTriangle(ti + 2, vi + int3(0, 3, 4));
                streams.SetTriangle(ti + 3, vi + int3(0, 4, 5));
                streams.SetTriangle(ti + 4, vi + int3(0, 5, 6));
                streams.SetTriangle(ti + 5, vi + int3(0, 6, 1));
            }
        }
    }

    public struct FlatHexagonGrid : IMeshGenerator
    {

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(
            0.75f + 0.25f / Resolution,
            0f,
            (Resolution > 1 ? 0.5f + 0.25f / Resolution : 0.5f) * sqrt(3f)
        ));

        public int VertexCount => 7 * Resolution * Resolution;

        public int IndexCount => 18 * Resolution * Resolution;

        public int JobLength => Resolution;

        public int Resolution { get; set; }

        public void Execute<S>(int x, S streams) where S : struct, IMeshStreams
        {
            int vi = 7 * Resolution * x, ti = 6 * Resolution * x;

            float h = sqrt(3f) / 4f;

            float2 centerOffset = 0f;

            if (Resolution > 1)
            {
                centerOffset.x = -0.375f * (Resolution - 1);
                centerOffset.y = (((x & 1) == 0 ? 0.5f : 1.5f) - Resolution) * h;
            }

            for (int z = 0; z < Resolution; z++, vi += 7, ti += 6)
            {
                var center = (float2(0.75f * x, 2f * h * z) + centerOffset) / Resolution;
                var xCoordinates =
                    center.x + float4(-0.5f, -0.25f, 0.25f, 0.5f) / Resolution;
                var zCoordinates = center.y + float2(h, -h) / Resolution;
                var vertex = new Vertex();
                vertex.normal.y = 1f;
                vertex.tangent.xw = float2(1f, -1f);

                vertex.position.xz = center;
                vertex.texCoord0 = 0.5f;
                streams.SetVertex(vi + 0, vertex);

                vertex.position.x = xCoordinates.x;
                vertex.texCoord0.x = 0f;
                streams.SetVertex(vi + 1, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.position.z = zCoordinates.x;
                vertex.texCoord0 = float2(0.25f, 0.5f + h);
                streams.SetVertex(vi + 2, vertex);

                vertex.position.x = xCoordinates.z;
                vertex.texCoord0.x = 0.75f;
                streams.SetVertex(vi + 3, vertex);

                vertex.position.x = xCoordinates.w;
                vertex.position.z = center.y;
                vertex.texCoord0 = float2(1f, 0.5f);
                streams.SetVertex(vi + 4, vertex);

                vertex.position.x = xCoordinates.z;
                vertex.position.z = zCoordinates.y;
                vertex.texCoord0 = float2(0.75f, 0.5f - h);
                streams.SetVertex(vi + 5, vertex);

                vertex.position.x = xCoordinates.y;
                vertex.texCoord0.x = 0.25f;
                streams.SetVertex(vi + 6, vertex);

                streams.SetTriangle(ti + 0, vi + int3(0, 1, 2));
                streams.SetTriangle(ti + 1, vi + int3(0, 2, 3));
                streams.SetTriangle(ti + 2, vi + int3(0, 3, 4));
                streams.SetTriangle(ti + 3, vi + int3(0, 4, 5));
                streams.SetTriangle(ti + 4, vi + int3(0, 5, 6));
                streams.SetTriangle(ti + 5, vi + int3(0, 6, 1));
            }
        }
    }

    public struct UVSphere : IMeshGenerator
    {

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

        public int VertexCount => (ResolutionU + 1) * (ResolutionV + 1) - 2;

        public int IndexCount => 6 * ResolutionU * (ResolutionV - 1);

        public int JobLength => ResolutionU + 1;

        public int Resolution { get; set; }

        int ResolutionU => 4 * Resolution;

        int ResolutionV => 2 * Resolution;

        public void Execute<S>(int u, S streams) where S : struct, IMeshStreams
        {
            if (u == 0)
            {
                ExecuteSeam(streams);
            }
            else
            {
                ExecuteRegular(u, streams);
            }
        }

        public void ExecuteRegular<S>(int u, S streams) where S : struct, IMeshStreams
        {
            int vi = (ResolutionV + 1) * u - 2, ti = 2 * (ResolutionV - 1) * (u - 1);

            var vertex = new Vertex();
            vertex.position.y = vertex.normal.y = -1f;
            sincos(
                2f * PI * (u - 0.5f) / ResolutionU,
                out vertex.tangent.z, out vertex.tangent.x
            );
            vertex.tangent.w = -1f;
            vertex.texCoord0.x = (u - 0.5f) / ResolutionU;
            streams.SetVertex(vi, vertex);

            vertex.position.y = vertex.normal.y = 1f;
            vertex.texCoord0.y = 1f;
            streams.SetVertex(vi + ResolutionV, vertex);
            vi += 1;

            float2 circle;
            sincos(2f * PI * u / ResolutionU, out circle.x, out circle.y);
            vertex.tangent.xz = circle.yx;
            circle.y = -circle.y;
            vertex.texCoord0.x = (float)u / ResolutionU;

            int shiftLeft = (u == 1 ? 0 : -1) - ResolutionV;

            streams.SetTriangle(ti, vi + int3(-1, shiftLeft, 0));
            ti += 1;

            for (int v = 1; v < ResolutionV; v++, vi++)
            {
                sincos(
                    PI + PI * v / ResolutionV,
                    out float circleRadius, out vertex.position.y
                );
                vertex.position.xz = circle * -circleRadius;
                vertex.normal = vertex.position;
                vertex.texCoord0.y = (float)v / ResolutionV;
                streams.SetVertex(vi, vertex);

                if (v > 1)
                {
                    streams.SetTriangle(ti + 0, vi + int3(shiftLeft - 1, shiftLeft, -1));
                    streams.SetTriangle(ti + 1, vi + int3(-1, shiftLeft, 0));
                    ti += 2;
                }
            }

            streams.SetTriangle(ti, vi + int3(shiftLeft - 1, 0, -1));
        }

        public void ExecuteSeam<S>(S streams) where S : struct, IMeshStreams
        {
            var vertex = new Vertex();
            vertex.tangent.x = 1f;
            vertex.tangent.w = -1f;

            for (int v = 1; v < ResolutionV; v++)
            {
                sincos(
                    PI + PI * v / ResolutionV,
                    out vertex.position.z, out vertex.position.y
                );
                vertex.normal = vertex.position;
                vertex.texCoord0.y = (float)v / ResolutionV;
                streams.SetVertex(v - 1, vertex);
            }
        }
    }

    public struct CubeSphere : IMeshGenerator
    {

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

        public int VertexCount => 6 * 4 * Resolution * Resolution;

        public int IndexCount => 6 * 6 * Resolution * Resolution;

        public int JobLength => 6 * Resolution;

        public int Resolution { get; set; }

        struct Side
        {
            public int id;
            public float3 uvOrigin, uVector, vVector;
            public float3 normal;
            public float4 tangent;
        }

        static Side GetSide(int id) => id switch
        {
            0 => new Side
            {
                id = id,
                uvOrigin = -1f,
                uVector = 2f * right(),
                vVector = 2f * up(),
                normal = back(),
                tangent = float4(1f, 0f, 0f, -1f)
            },
            1 => new Side
            {
                id = id,
                uvOrigin = float3(1f, -1f, -1f),
                uVector = 2f * forward(),
                vVector = 2f * up(),
                normal = right(),
                tangent = float4(0f, 0f, 1f, -1f)
            },
            2 => new Side
            {
                id = id,
                uvOrigin = -1f,
                uVector = 2f * forward(),
                vVector = 2f * right(),
                normal = down(),
                tangent = float4(0f, 0f, 1f, -1f)
            },
            3 => new Side
            {
                id = id,
                uvOrigin = float3(-1f, -1f, 1f),
                uVector = 2f * up(),
                vVector = 2f * right(),
                normal = forward(),
                tangent = float4(0f, 1f, 0f, -1f)
            },
            4 => new Side
            {
                id = id,
                uvOrigin = -1f,
                uVector = 2f * up(),
                vVector = 2f * forward(),
                normal = left(),
                tangent = float4(0f, 1f, 0f, -1f)
            },
            _ => new Side
            {
                id = id,
                uvOrigin = float3(-1f, 1f, -1f),
                uVector = 2f * right(),
                vVector = 2f * forward(),
                normal = up(),
                tangent = float4(1f, 0f, 0f, -1f)
            }
        };

        static float3 CubeToSphere(float3 p) => p * sqrt(
            1f - ((p * p).yxx + (p * p).zzy) / 2f + (p * p).yxx * (p * p).zzy / 3f
        );

        public void Execute<S>(int i, S streams) where S : struct, IMeshStreams
        {
            int u = i / 6;
            Side side = GetSide(i - 6 * u);


            int vi = 4 * Resolution * (Resolution * side.id + u);
            int ti = 2 * Resolution * (Resolution * side.id + u);

            float3 uA = side.uvOrigin + side.uVector * u / Resolution;
            float3 uB = side.uvOrigin + side.uVector * (u + 1) / Resolution;

            float3 pA = CubeToSphere(uA), pB = CubeToSphere(uB);

            var vertex = new Vertex();
            vertex.tangent = float4(normalize(pB - pA), -1f);

            for (int v = 1; v <= Resolution; v++, vi += 4, ti += 2)
            {
                float3 pC = CubeToSphere(uA + side.vVector * v / Resolution);
                float3 pD = CubeToSphere(uB + side.vVector * v / Resolution);

                vertex.position = pA;
                vertex.normal = normalize(cross(pC - pA, vertex.tangent.xyz));

                vertex.texCoord0 = 0f;
                streams.SetVertex(vi + 0, vertex);

                vertex.position = pB;
                vertex.normal = normalize(cross(pD - pB, vertex.tangent.xyz));
                vertex.texCoord0 = float2(1f, 0f);
                streams.SetVertex(vi + 1, vertex);

                vertex.position = pC;
                vertex.tangent.xyz = normalize(pD - pC);
                vertex.normal = normalize(cross(pC - pA, vertex.tangent.xyz));
                vertex.texCoord0 = float2(0f, 1f);
                streams.SetVertex(vi + 2, vertex);

                vertex.position = pD;
                vertex.normal = normalize(cross(pD - pB, vertex.tangent.xyz));
                vertex.texCoord0 = 1f;
                streams.SetVertex(vi + 3, vertex);

                streams.SetTriangle(ti + 0, vi + int3(0, 2, 1));
                streams.SetTriangle(ti + 1, vi + int3(1, 2, 3));

                pA = pC;
                pB = pD;
            }
        }
    }

    public struct SharedCubeSphere : IMeshGenerator
    {

        public Bounds Bounds => new Bounds(Vector3.zero, new Vector3(2f, 2f, 2f));

        public int VertexCount => 6 * Resolution * Resolution + 2;

        public int IndexCount => 6 * 6 * Resolution * Resolution;

        public int JobLength => 6 * Resolution;

        public int Resolution { get; set; }

        struct Side
        {
            public int id;
            public float3 uvOrigin, uVector, vVector;
            public bool TouchesMinimumPole => (id & 1) == 0;
            public int seamStep;

        }

        static Side GetSide(int id) => id switch
        {
            0 => new Side
            {
                id = id,
                uvOrigin = -1f,
                uVector = 2f * right(),
                vVector = 2f * up(),
                seamStep = 4
            },
            1 => new Side
            {
                id = id,
                uvOrigin = float3(1f, -1f, -1f),
                uVector = 2f * forward(),
                vVector = 2f * up(),
                seamStep = 4
            },
            2 => new Side
            {
                id = id,
                uvOrigin = -1f,
                uVector = 2f * forward(),
                vVector = 2f * right(),
                seamStep = -2
            },
            3 => new Side
            {
                id = id,
                uvOrigin = float3(-1f, -1f, 1f),
                uVector = 2f * up(),
                vVector = 2f * right(),
                seamStep = -2
            },
            4 => new Side
            {
                id = id,
                uvOrigin = -1f,
                uVector = 2f * up(),
                vVector = 2f * forward(),
                seamStep = -2
            },
            _ => new Side
            {
                id = id,
                uvOrigin = float3(-1f, 1f, -1f),
                uVector = 2f * right(),
                vVector = 2f * forward(),
                seamStep = -2
            }
        };

        static float3 CubeToSphere(float3 p) => p * sqrt(
            1f - ((p * p).yxx + (p * p).zzy) / 2f + (p * p).yxx * (p * p).zzy / 3f
        );

        public void Execute<S>(int i, S streams) where S : struct, IMeshStreams
        {
            int u = i / 6;
            Side side = GetSide(i - 6 * u);


            int vi = Resolution * (Resolution * side.id + u) + 2;
            int ti = 2 * Resolution * (Resolution * side.id + u);

            float3 pStart = side.uvOrigin + side.uVector * u / Resolution;

            var vertex = new Vertex();
            if (i == 0)
            {
                vertex.position = -sqrt(1f / 3f);
                streams.SetVertex(0, vertex);
                vertex.position = sqrt(1f / 3f);
                streams.SetVertex(1, vertex);
            }

            vertex.position = CubeToSphere(pStart);
            streams.SetVertex(vi, vertex);
            bool firstColumn = u == 0;

            var triangle = int3(
                vi,
                firstColumn && side.TouchesMinimumPole ? 0 : vi - Resolution,
                vi + (firstColumn ?
                    side.TouchesMinimumPole ?
                        side.seamStep * Resolution * Resolution :
                        Resolution == 1 ? side.seamStep : -Resolution + 1 :
                    -Resolution + 1
                )
            );
            streams.SetTriangle(ti, triangle);
            vi += 1;
            ti += 1;

            u += 1;

            int zAdd = firstColumn && side.TouchesMinimumPole ? Resolution : 1;
            int zAddLast = firstColumn && side.TouchesMinimumPole ?
                Resolution :
                !firstColumn && !side.TouchesMinimumPole ?
                    Resolution * ((side.seamStep + 1) * Resolution - u) + u :
                    (side.seamStep + 1) * Resolution * Resolution - Resolution + 1;

            for (int v = 1; v < Resolution; v++, vi++, ti += 2)
            {
                vertex.position = CubeToSphere(pStart + side.vVector * v / Resolution);
                streams.SetVertex(vi, vertex);

                triangle.x += 1;
                triangle.y = triangle.z;
                triangle.z += v == Resolution - 1 ? zAddLast : zAdd;
                streams.SetTriangle(ti + 0, int3(triangle.x - 1, triangle.y, triangle.x));
                streams.SetTriangle(ti + 1, triangle);
            }
            streams.SetTriangle(ti, int3(
                triangle.x,
                triangle.z,
                side.TouchesMinimumPole ?
                    triangle.z + Resolution :
                    u == Resolution ? 1 : triangle.z + 1
            ));

        }
    }
}
