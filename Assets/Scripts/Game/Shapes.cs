using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace GauntletRun.Game
{
    /// <summary>
    /// Builds every model in the game from Unity's built-in shapes and a few flat-shaded low-poly
    /// meshes made in code. Nothing gets a collider: all collisions are plain maths in the core.
    /// </summary>
    public static class Shapes
    {
        static GameArt art;

        public static void Init(GameArt gameArt) => art = gameArt;

        public static Mesh MeshOf(PrimitiveType type)
        {
            switch (type)
            {
                case PrimitiveType.Sphere: return art.SphereMesh;
                case PrimitiveType.Cylinder: return art.CylinderMesh;
                case PrimitiveType.Capsule: return art.CapsuleMesh;
                case PrimitiveType.Quad: return art.QuadMesh;
                default: return art.CubeMesh;
            }
        }

        public static Transform Part(string name, Transform parent, PrimitiveType type, Material material,
            Vector3 position, Vector3 euler, Vector3 scale, bool shadows = true) =>
            Part(name, parent, MeshOf(type), material, position, euler, scale, shadows);

        public static Transform Part(string name, Transform parent, Mesh mesh, Material material,
            Vector3 position, Vector3 euler, Vector3 scale, bool shadows = true)
        {
            var part = new GameObject(name);
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localEulerAngles = euler;
            part.transform.localScale = scale;
            part.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = part.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = shadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = shadows;
            return part.transform;
        }

        /// <summary>A soft dark disc on the floor under a character.</summary>
        public static Transform Shadow(Transform parent, float radius) =>
            Part("Shadow", parent, PrimitiveType.Quad, art.BlobShadow, new Vector3(0f, 0.02f, 0f),
                new Vector3(90f, 0f, 0f), new Vector3(radius * 2.4f, radius * 2.4f, 1f), shadows: false);

        /// <summary>A flat additive glow lying on the floor, tinted by its own material instance.</summary>
        public static Transform Glow(Transform parent, Material tinted, float size, float height = 0.05f) =>
            Part("Glow", parent, PrimitiveType.Quad, tinted, new Vector3(0f, height, 0f),
                new Vector3(90f, 0f, 0f), new Vector3(size, size, 1f), shadows: false);

        /// <summary>A copy of the additive particle material in another colour (for glows).</summary>
        public static Material Tinted(Color color)
        {
            var material = new Material(art.ParticleAdditive);
            material.SetColor("_TintColor", color);
            return material;
        }

        /// <summary>A copy of a lit material in another colour (for skins).</summary>
        public static Material Recolored(Material source, Color color)
        {
            var material = new Material(source) { color = color };
            return material;
        }

        public static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.magenta;

        /// <summary>A tapered n-sided column standing on the origin, <paramref name="height"/> tall.</summary>
        public static Mesh Prism(int sides, float bottomRadius, float topRadius, float height, float jitter = 0f, int seed = 0)
        {
            var random = new System.Random(seed);
            var bottom = new Vector3[sides];
            var top = new Vector3[sides];
            float turn = (float)random.NextDouble() * Mathf.PI * 2f;
            for (int i = 0; i < sides; i++)
            {
                float angle = turn + i * Mathf.PI * 2f / sides;
                var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                bottom[i] = direction * (bottomRadius * (1f + Jitter(random, jitter)));
                top[i] = direction * (topRadius * (1f + Jitter(random, jitter))) + Vector3.up * height;
            }
            var triangles = new List<Vector3>();
            for (int i = 0; i < sides; i++)
            {
                int j = (i + 1) % sides;
                Quad(triangles, bottom[i], bottom[j], top[j], top[i]);
                if (topRadius > 0f)
                {
                    triangles.Add(Vector3.up * height);
                    triangles.Add(top[i]);
                    triangles.Add(top[j]);
                }
                triangles.Add(Vector3.zero);
                triangles.Add(bottom[j]);
                triangles.Add(bottom[i]);
            }
            return Build(triangles, "Prism");
        }

        /// <summary>A rough ellipsoid: a coarse sphere whose points are pushed in and out at random.</summary>
        public static Mesh Blob(int rings = 4, int segments = 7, float jitter = 0.12f, int seed = 0)
        {
            var random = new System.Random(seed);
            var points = new Vector3[rings + 1, segments];
            for (int r = 0; r <= rings; r++)
            {
                float polar = Mathf.PI * r / rings;
                for (int s = 0; s < segments; s++)
                {
                    float azimuth = Mathf.PI * 2f * s / segments + (r % 2) * Mathf.PI / segments;
                    var point = new Vector3(Mathf.Sin(polar) * Mathf.Cos(azimuth), Mathf.Cos(polar), Mathf.Sin(polar) * Mathf.Sin(azimuth));
                    bool pole = r == 0 || r == rings;
                    points[r, s] = pole ? point : point * (1f + Jitter(random, jitter));
                }
            }
            var triangles = new List<Vector3>();
            for (int r = 0; r < rings; r++)
            {
                for (int s = 0; s < segments; s++)
                {
                    int t = (s + 1) % segments;
                    Vector3 a = points[r, s], b = points[r, t], c = points[r + 1, t], d = points[r + 1, s];
                    if (r > 0)
                    {
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(d);
                    }
                    if (r < rings - 1)
                    {
                        triangles.Add(b);
                        triangles.Add(c);
                        triangles.Add(d);
                    }
                }
            }
            return Build(triangles, "Blob");
        }

        static void Quad(List<Vector3> triangles, Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            triangles.Add(a);
            triangles.Add(c);
            triangles.Add(b);
            triangles.Add(a);
            triangles.Add(d);
            triangles.Add(c);
        }

        /// <summary>Makes a flat-shaded mesh from a triangle list, turning every face away from the centre.</summary>
        static Mesh Build(List<Vector3> vertices, string name)
        {
            Vector3 centre = Vector3.zero;
            foreach (Vector3 v in vertices)
            {
                centre += v;
            }
            centre /= Mathf.Max(vertices.Count, 1);
            for (int i = 0; i + 2 < vertices.Count; i += 3)
            {
                Vector3 a = vertices[i], b = vertices[i + 1], c = vertices[i + 2];
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), (a + b + c) / 3f - centre) < 0f)
                {
                    vertices[i + 1] = c;
                    vertices[i + 2] = b;
                }
            }
            var indices = new int[vertices.Count];
            for (int i = 0; i < indices.Length; i++)
            {
                indices[i] = i;
            }
            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        static float Jitter(System.Random random, float amount) => ((float)random.NextDouble() * 2f - 1f) * amount;
    }
}
