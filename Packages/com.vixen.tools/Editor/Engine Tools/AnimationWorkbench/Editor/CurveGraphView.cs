#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vixenlicious.AnimationWorkbench
{
    public class CurveGraphView : VisualElement
    {
        private readonly IMGUIContainer _imgui;
        private readonly Action<Dictionary<EditorCurveBinding, AnimationCurve>> _onCurvesChanged;

        private Dictionary<EditorCurveBinding, AnimationCurve> _curves =
            new Dictionary<EditorCurveBinding, AnimationCurve>();

        private float _logicalStart = 0;
        private float _logicalEnd = 1;

        private float _viewStart = 0;
        private float _viewEnd = 1;

        private float _zoom = 1.0f;
        private Rect _graphRect;

        private bool _isPanning = false;
        private Vector2 _panAnchor;

        // Cached rendering bounds for mouse hit detection
        private float _minV = 0f;
        private float _maxV = 1f;

        public CurveGraphView() : this(null) { }

        public CurveGraphView(Action<Dictionary<EditorCurveBinding, AnimationCurve>> changed)
        {
            _onCurvesChanged = changed;

            style.flexGrow = 1;

            _imgui = new IMGUIContainer(OnGUI);
            _imgui.StretchToParentSize();
            Add(_imgui);

            RegisterCallback<MouseDownEvent>(OnMouseDown);
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);
            RegisterCallback<MouseLeaveEvent>(_ => _isPanning = false);
        }

        public void SetRange(float start, float end)
        {
            _logicalStart = Mathf.Min(start, end);
            _logicalEnd = Mathf.Max(start, end);

            RecomputeZoomedView();
            MarkDirtyRepaint();
        }

        public void SetZoomFactor(float zoom)
        {
            _zoom = Mathf.Clamp(zoom, 0.1f, 10f);
            RecomputeZoomedView();
            MarkDirtyRepaint();
        }

        public void SetCurveSet(Dictionary<EditorCurveBinding, AnimationCurve> curves)
        {
            _curves = new Dictionary<EditorCurveBinding, AnimationCurve>();
            if (curves != null)
            {
                foreach (var kv in curves)
                {
                    _curves[kv.Key] =
                        kv.Value != null
                            ? new AnimationCurve(kv.Value.keys)
                            : new AnimationCurve();
                }
            }

            MarkDirtyRepaint();
        }

        private void RecomputeZoomedView()
        {
            float span = Mathf.Max(0.0001f, _logicalEnd - _logicalStart);
            float viewSpan = span / Mathf.Max(_zoom, 0.001f);
            float center = (_logicalStart + _logicalEnd) * 0.5f;

            _viewStart = center - viewSpan * 0.5f;
            _viewEnd = center + viewSpan * 0.5f;
        }

        private void OnMouseDown(MouseDownEvent e)
        {
            if (!_graphRect.Contains(e.localMousePosition)) return;

            if (e.button == 2) // Middle Click: Pan
            {
                _isPanning = true;
                _panAnchor = e.localMousePosition;
                e.StopPropagation();
            }
            else if (e.button == 0 && e.clickCount == 2) // Double Left Click: Add Key
            {
                AddKeyframeAtMouse(e.localMousePosition);
                e.StopPropagation();
            }
            else if (e.button == 1) // Right Click: Delete Key
            {
                if (TryDeleteKeyframeAtMouse(e.localMousePosition))
                {
                    e.StopPropagation();
                }
            }
        }

        private void AddKeyframeAtMouse(Vector2 mousePos)
        {
            if (_curves == null || _curves.Count == 0) return;

            float tx = (mousePos.x - _graphRect.xMin) / _graphRect.width;
            float time = Mathf.Lerp(_viewStart, _viewEnd, tx);
            bool changed = false;

            foreach (var kv in _curves)
            {
                var curve = kv.Value;
                if (curve == null) continue;

                float val = curve.Evaluate(time);
                int idx = curve.AddKey(new Keyframe(time, val));
                if (idx >= 0)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, idx, AnimationUtility.TangentMode.Auto);
                    AnimationUtility.SetKeyRightTangentMode(curve, idx, AnimationUtility.TangentMode.Auto);
                    changed = true;
                }
            }

            if (changed && _onCurvesChanged != null)
            {
                _onCurvesChanged.Invoke(_curves);
                MarkDirtyRepaint();
            }
        }

        private bool TryDeleteKeyframeAtMouse(Vector2 mousePos)
        {
            if (_curves == null || _curves.Count == 0) return false;

            bool deleted = false;
            float hitThreshold = 10f;

            foreach (var kv in _curves)
            {
                var curve = kv.Value;
                if (curve == null || curve.keys.Length == 0) continue;

                for (int i = 0; i < curve.keys.Length; i++)
                {
                    var k = curve.keys[i];
                    if (k.time < _viewStart || k.time > _viewEnd) continue;

                    Vector2 p = ToGraphPoint(_graphRect, k.time, k.value, _minV, _maxV);
                    if (Vector2.Distance(p, mousePos) <= hitThreshold)
                    {
                        curve.RemoveKey(i);
                        deleted = true;
                        break;
                    }
                }
            }

            if (deleted && _onCurvesChanged != null)
            {
                _onCurvesChanged.Invoke(_curves);
                MarkDirtyRepaint();
            }

            return deleted;
        }

        private void OnMouseMove(MouseMoveEvent e)
        {
            if (!_isPanning) return;

            float dx = e.localMousePosition.x - _panAnchor.x;
            float timeSpan = _viewEnd - _viewStart;
            float deltaTime = -dx / Mathf.Max(1f, _graphRect.width) * timeSpan;
            float logicalSpan = _logicalEnd - _logicalStart;
            float logicalCenter = (_logicalStart + _logicalEnd) * 0.5f;

            logicalCenter += deltaTime;

            _logicalStart = logicalCenter - logicalSpan * 0.5f;
            _logicalEnd = logicalCenter + logicalSpan * 0.5f;

            RecomputeZoomedView();

            _panAnchor = e.localMousePosition;
            MarkDirtyRepaint();
            e.StopPropagation();
        }

        private void OnMouseUp(MouseUpEvent e)
        {
            if (e.button == 2)
            {
                _isPanning = false;
                e.StopPropagation();
            }
        }

        private void OnGUI()
        {
            _graphRect = new Rect(4, 4, _imgui.contentRect.width - 8, _imgui.contentRect.height - 8);

            if (_graphRect.width <= 2 || _graphRect.height <= 2) return;

            EditorGUI.DrawRect(_graphRect, new Color(0.08f, 0.08f, 0.10f));
            DrawGrid(_graphRect);

            if (_curves == null || _curves.Count == 0)
            {
                DrawRulers(_graphRect);
                return;
            }

            float minV = float.MaxValue;
            float maxV = float.MinValue;

            foreach (var curve in _curves.Values)
            {
                if (curve == null || curve.keys.Length == 0) continue;

                foreach (var k in curve.keys)
                {
                    minV = Mathf.Min(minV, k.value);
                    maxV = Mathf.Max(maxV, k.value);
                }
            }

            if (!float.IsFinite(minV) || !float.IsFinite(maxV))
            {
                minV = -1;
                maxV = 1;
            }

            if (Mathf.Approximately(minV, maxV))
            {
                minV -= 1;
                maxV += 1;
            }

            // Cache for hit detection
            _minV = minV;
            _maxV = maxV;

            foreach (var kv in _curves)
            {
                var c = kv.Value;
                if (c == null || c.keys.Length == 0) continue;

                float hue = (kv.Key.propertyName.GetHashCode() & 0xFFFF) / 65535f;
                Color col = Color.HSVToRGB(hue, 0.75f, 0.95f);

                DrawCurve(_graphRect, c, minV, maxV, col);
                DrawKeys(_graphRect, c, minV, maxV, col * 0.9f);
            }

            DrawRulers(_graphRect);
        }

        private void DrawGrid(Rect rect)
        {
            Handles.color = new Color(1f, 1f, 1f, 0.03f);

            for (int i = 0; i <= 10; ++i)
            {
                float x = rect.xMin + rect.width * (i / 10f);
                Handles.DrawLine(new Vector3(x, rect.yMin), new Vector3(x, rect.yMax));
            }

            for (int i = 0; i <= 4; ++i)
            {
                float y = rect.yMin + rect.height * (i / 4f);
                Handles.DrawLine(new Vector3(rect.xMin, y), new Vector3(rect.xMax, y));
            }
        }

        private void DrawCurve(Rect rect, AnimationCurve c, float minV, float maxV, Color col)
        {
            Handles.color = col;

            int steps = Mathf.Clamp(Mathf.RoundToInt(rect.width), 32, 512);
            Vector3 prev = Vector3.zero;
            bool hasPrev = false;

            for (int i = 0; i <= steps; i++)
            {
                float u = i / (float)steps;
                float t = Mathf.Lerp(_viewStart, _viewEnd, u);
                float v = c.Evaluate(t);

                Vector2 p = ToGraphPoint(rect, t, v, minV, maxV);

                if (hasPrev)
                    Handles.DrawLine(prev, p);

                prev = p;
                hasPrev = true;
            }
        }

        private void DrawKeys(Rect rect, AnimationCurve c, float minV, float maxV, Color col)
        {
            foreach (var k in c.keys)
            {
                if (k.time < _viewStart || k.time > _viewEnd) continue;

                Vector2 p = ToGraphPoint(rect, k.time, k.value, minV, maxV);

                Rect r = new Rect(p.x - 3, p.y - 3, 6, 6);
                EditorGUI.DrawRect(r, Color.black);
                EditorGUI.DrawRect(new Rect(r.x + 1, r.y + 1, r.width - 2, r.height - 2), col);
            }
        }

        private Vector2 ToGraphPoint(Rect rect, float time, float value, float minV, float maxV)
        {
            float tx = Mathf.InverseLerp(_viewStart, _viewEnd, time);
            float x = rect.xMin + tx * rect.width;

            float ty = Mathf.InverseLerp(minV, maxV, value);
            float y = rect.yMax - ty * rect.height;

            return new Vector2(x, y);
        }

        private void DrawRulers(Rect rect)
        {
            Handles.color = new Color(1f, 1f, 1f, 0.12f);

            for (int i = 0; i <= 8; i++)
            {
                float u = i / 8f;
                float t = Mathf.Lerp(_viewStart, _viewEnd, u);
                float x = rect.xMin + u * rect.width;

                Handles.DrawLine(new Vector3(x, rect.yMax - 5), new Vector3(x, rect.yMax));
                GUI.Label(new Rect(x + 2, rect.yMax - 18, 50, 16), t.ToString("0.000"));
            }
        }
    }
}
#endif