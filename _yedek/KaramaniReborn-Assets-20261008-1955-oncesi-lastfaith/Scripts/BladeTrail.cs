using System.Collections.Generic;
using UnityEngine;

namespace KaramaniReborn
{
    // Kılıcın süpürdüğü alanı dolduran, kavisli ve solan iz. Bıçağın kök/uç konumları her simülasyon
    // adımında örneklenir; noktalar arası Catmull-Rom ile yumuşatılıp bir üçgen şeridine dönüştürülür.
    public sealed class BladeTrail
    {
        const int Subdivisions = 3;
        static Material material;
        readonly GameObject gameObject;
        readonly Mesh mesh;
        readonly List<Sample> samples = new();
        readonly List<Vector3> vertices = new();
        readonly List<Color> colors = new();
        readonly List<Vector2> uvs = new();
        readonly List<int> triangles = new();
        struct Sample { public Vector2 root, tip; public float age; }
        public float lifetime = .13f;
        public Color color;
        public int Count => samples.Count;

        public BladeTrail(Transform parent, Color trailColor, int sortingOrder)
        {
            if (!material) material = new Material(Resources.Load<Shader>("Shaders/BladeTrail")) { name = "Blade trail" };
            gameObject = new GameObject("Blade trail"); gameObject.transform.SetParent(parent, false);
            mesh = new Mesh { name = "Blade trail" }; mesh.MarkDynamic();
            gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material; renderer.sortingOrder = sortingOrder;
            color = trailColor;
        }

        public void Step(Vector2 root, Vector2 tip, bool emit, float dt)
        {
            for (int i = samples.Count - 1; i >= 0; i--)
            {
                var s = samples[i]; s.age += dt; samples[i] = s;
                if (s.age > lifetime) samples.RemoveAt(i);
            }
            if (emit) samples.Add(new Sample { root = root, tip = tip });
            Rebuild();
        }

        public void Clear() { samples.Clear(); mesh.Clear(); }

        void Rebuild()
        {
            mesh.Clear(); vertices.Clear(); colors.Clear(); uvs.Clear(); triangles.Clear();
            if (samples.Count < 2) return;
            int n = samples.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int k = 0; k < Subdivisions; k++)
                {
                    float t = k / (float)Subdivisions;
                    AddPair(i, t, n);
                }
            }
            AddPair(n - 1, 0, n);
            for (int i = 0; i < vertices.Count / 2 - 1; i++)
            {
                int a = i * 2;
                triangles.Add(a); triangles.Add(a + 1); triangles.Add(a + 2);
                triangles.Add(a + 1); triangles.Add(a + 3); triangles.Add(a + 2);
            }
            mesh.SetVertices(vertices); mesh.SetColors(colors); mesh.SetUVs(0, uvs); mesh.SetTriangles(triangles, 0);
        }

        void AddPair(int i, float t, int n)
        {
            Sample p0 = samples[Mathf.Max(0, i - 1)], p1 = samples[i], p2 = samples[Mathf.Min(n - 1, i + 1)], p3 = samples[Mathf.Min(n - 1, i + 2)];
            Vector2 tip = CatmullRom(p0.tip, p1.tip, p2.tip, p3.tip, t);
            Vector2 root = CatmullRom(p0.root, p1.root, p2.root, p3.root, t);
            float age = Mathf.Lerp(p1.age, p2.age, t);
            float fade = Mathf.Pow(Mathf.Clamp01(1 - age / lifetime), 1.6f);
            float along = (i + t) / Mathf.Max(1, n - 1);
            vertices.Add(root); vertices.Add(tip);
            Color inner = color; inner.a *= fade * .25f;
            Color outer = color; outer.a *= fade;
            colors.Add(inner); colors.Add(outer);
            uvs.Add(new Vector2(along, 0)); uvs.Add(new Vector2(along, 1));
        }

        static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t, t3 = t2 * t;
            return .5f * (2 * p1 + (-p0 + p2) * t + (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 + (-p0 + 3 * p1 - 3 * p2 + p3) * t3);
        }

        public void Destroy()
        {
            if (Application.isPlaying) { Object.Destroy(gameObject); Object.Destroy(mesh); }
            else { Object.DestroyImmediate(gameObject); Object.DestroyImmediate(mesh); }
        }
    }
}
