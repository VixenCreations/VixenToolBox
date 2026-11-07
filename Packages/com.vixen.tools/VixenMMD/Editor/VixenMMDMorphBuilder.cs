using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VixenTools.MMD.Editor
{
    internal static class VixenMMDMorphBuilder
    {
        internal sealed class Problem
        {
            public string Morph;
            public string Detail;
        }

        internal sealed class Result
        {
            public Mesh Mesh;
            public int Built;
            public int Replaced;
            public readonly List<Problem> Skipped = new List<Problem>();
        }

        private sealed class Frames
        {
            public float[] Weights;
            public Vector3[][] V;
            public Vector3[][] N;
            public Vector3[][] T;
        }

        internal static List<VixenMMDMorph> ValidMorphs(Mesh mesh, IEnumerable<VixenMMDMorph> morphs, List<Problem> problems)
        {
            var names = new HashSet<string>();
            for (int i = 0; i < mesh.blendShapeCount; i++) names.Add(mesh.GetBlendShapeName(i));
            var seen = new HashSet<string>();
            var list = new List<VixenMMDMorph>();
            foreach (var m in morphs)
            {
                if (m == null || !m.enabled || string.IsNullOrEmpty(m.name)) continue;
                if (!seen.Add(m.name))
                {
                    problems?.Add(new Problem { Morph = m.name, Detail = VixenMMDText.L("This name is listed twice. Only the first one is built.") });
                    continue;
                }
                var sources = (m.sources ?? new List<VixenMMDSource>()).Where(s => s != null && !string.IsNullOrEmpty(s.shape)).ToList();
                if (sources.Count == 0) continue;
                var missing = sources.Where(s => !names.Contains(s.shape)).Select(s => s.shape).Distinct().ToList();
                if (missing.Count > 0)
                {
                    problems?.Add(new Problem { Morph = m.name, Detail = VixenMMDText.L("The face mesh has no shape called {0}. Another tool may have removed it. This MMD shape is not built.", string.Join(", ", missing)) });
                    continue;
                }
                list.Add(m);
            }
            return list;
        }

        internal static Result Build(SkinnedMeshRenderer smr, IEnumerable<VixenMMDMorph> morphs)
        {
            var result = new Result();
            var src = smr != null ? smr.sharedMesh : null;
            if (src == null) return result;

            var jobs = ValidMorphs(src, morphs, result.Skipped);
            if (jobs.Count == 0) return result;

            var nameToIndex = new Dictionary<string, int>();
            for (int i = 0; i < src.blendShapeCount; i++)
            {
                string n = src.GetBlendShapeName(i);
                if (!nameToIndex.ContainsKey(n)) nameToIndex[n] = i;
            }
            var weights = new Dictionary<string, float>();
            for (int i = 0; i < src.blendShapeCount; i++)
            {
                string n = src.GetBlendShapeName(i);
                if (!weights.ContainsKey(n)) weights[n] = smr.GetBlendShapeWeight(i);
            }

            var byName = jobs.ToDictionary(j => j.name);
            var cache = new Dictionary<int, Frames>();
            var mesh = Object.Instantiate(src);
            mesh.name = src.name;
            bool replace = jobs.Any(j => nameToIndex.ContainsKey(j.name));
            var done = new HashSet<string>();

            if (replace)
            {
                mesh.ClearBlendShapes();
                int vc = src.vertexCount;
                var bv = new Vector3[vc];
                var bn = new Vector3[vc];
                var bt = new Vector3[vc];
                var written = new HashSet<string>();
                for (int i = 0; i < src.blendShapeCount; i++)
                {
                    string n = src.GetBlendShapeName(i);
                    if (!written.Add(n)) continue;
                    if (byName.TryGetValue(n, out var job))
                    {
                        WriteMorph(src, mesh, job, nameToIndex, cache);
                        done.Add(n);
                        result.Replaced++;
                    }
                    else
                    {
                        int frames = src.GetBlendShapeFrameCount(i);
                        for (int k = 0; k < frames; k++)
                        {
                            src.GetBlendShapeFrameVertices(i, k, bv, bn, bt);
                            mesh.AddBlendShapeFrame(n, src.GetBlendShapeFrameWeight(i, k), bv, bn, bt);
                        }
                    }
                }
            }
            foreach (var job in jobs)
            {
                if (done.Contains(job.name)) continue;
                WriteMorph(src, mesh, job, nameToIndex, cache);
                done.Add(job.name);
            }

            smr.sharedMesh = mesh;
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string n = mesh.GetBlendShapeName(i);
                smr.SetBlendShapeWeight(i, weights.TryGetValue(n, out float w) ? w : 0f);
            }
            result.Mesh = mesh;
            result.Built = done.Count;
            return result;
        }

        private static Frames Load(Mesh src, int index, Dictionary<int, Frames> cache)
        {
            if (cache.TryGetValue(index, out var f)) return f;
            int vc = src.vertexCount;
            int count = src.GetBlendShapeFrameCount(index);
            f = new Frames { Weights = new float[count], V = new Vector3[count][], N = new Vector3[count][], T = new Vector3[count][] };
            for (int k = 0; k < count; k++)
            {
                f.Weights[k] = src.GetBlendShapeFrameWeight(index, k);
                f.V[k] = new Vector3[vc];
                f.N[k] = new Vector3[vc];
                f.T[k] = new Vector3[vc];
                src.GetBlendShapeFrameVertices(index, k, f.V[k], f.N[k], f.T[k]);
            }
            cache[index] = f;
            return f;
        }

        private static void WriteFrames(Mesh mesh, string name, Frames f, float scale)
        {
            for (int k = 0; k < f.Weights.Length; k++)
            {
                if (scale == 1f)
                {
                    mesh.AddBlendShapeFrame(name, f.Weights[k], f.V[k], f.N[k], f.T[k]);
                    continue;
                }
                int vc = f.V[k].Length;
                var v = new Vector3[vc];
                var n = new Vector3[vc];
                var t = new Vector3[vc];
                for (int i = 0; i < vc; i++)
                {
                    v[i] = f.V[k][i] * scale;
                    n[i] = f.N[k][i] * scale;
                    t[i] = f.T[k][i] * scale;
                }
                mesh.AddBlendShapeFrame(name, f.Weights[k], v, n, t);
            }
        }

        private static void WriteMorph(Mesh src, Mesh mesh, VixenMMDMorph job, Dictionary<string, int> nameToIndex, Dictionary<int, Frames> cache)
        {
            var parts = new List<(Frames frames, float scale)>();
            var merged = new Dictionary<string, float>();
            var order = new List<string>();
            foreach (var s in job.sources)
            {
                if (s == null || string.IsNullOrEmpty(s.shape)) continue;
                if (!merged.ContainsKey(s.shape))
                {
                    merged[s.shape] = 0f;
                    order.Add(s.shape);
                }
                merged[s.shape] += s.scale;
            }
            foreach (var shape in order) parts.Add((Load(src, nameToIndex[shape], cache), merged[shape]));

            if (parts.Count == 1)
            {
                WriteFrames(mesh, job.name, parts[0].frames, parts[0].scale);
                return;
            }

            int vc = src.vertexCount;
            bool allSingle = parts.All(p => p.frames.Weights.Length == 1);
            var points = allSingle
                ? new List<float> { 100f }
                : parts.SelectMany(p => p.frames.Weights).Where(w => w > 0f).Distinct().OrderBy(w => w).ToList();
            if (points.Count == 0) points.Add(100f);

            foreach (float u in points)
            {
                var v = new Vector3[vc];
                var n = new Vector3[vc];
                var t = new Vector3[vc];
                foreach (var (frames, scale) in parts)
                {
                    Accumulate(frames, u, scale, v, n, t);
                }
                mesh.AddBlendShapeFrame(job.name, u, v, n, t);
            }
        }

        private static void Accumulate(Frames f, float u, float scale, Vector3[] v, Vector3[] n, Vector3[] t)
        {
            int count = f.Weights.Length;
            if (count == 0) return;
            int lo = -1, hi = -1;
            float a = 0f, b = 0f;
            if (count == 1 || u <= f.Weights[0])
            {
                hi = 0;
                b = f.Weights[0] != 0f ? u / f.Weights[0] : 0f;
            }
            else if (u >= f.Weights[count - 1])
            {
                lo = count - 2;
                hi = count - 1;
                float span = f.Weights[hi] - f.Weights[lo];
                b = span != 0f ? 1f + (u - f.Weights[hi]) / span : 1f;
                a = 1f - b;
            }
            else
            {
                for (int k = 0; k < count - 1; k++)
                {
                    if (u >= f.Weights[k] && u <= f.Weights[k + 1])
                    {
                        lo = k;
                        hi = k + 1;
                        float span = f.Weights[hi] - f.Weights[lo];
                        b = span != 0f ? (u - f.Weights[lo]) / span : 1f;
                        a = 1f - b;
                        break;
                    }
                }
            }
            a *= scale;
            b *= scale;
            int vc = v.Length;
            for (int i = 0; i < vc; i++)
            {
                if (hi >= 0)
                {
                    v[i] += f.V[hi][i] * b;
                    n[i] += f.N[hi][i] * b;
                    t[i] += f.T[hi][i] * b;
                }
                if (lo >= 0)
                {
                    v[i] += f.V[lo][i] * a;
                    n[i] += f.N[lo][i] * a;
                    t[i] += f.T[lo][i] * a;
                }
            }
        }
    }
}
