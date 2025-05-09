#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Foolish.Utils.Editor
{

    [CustomEditor(typeof(RectTransform))]
    public class RectTransformExtensionEditor : DecoratorEditor
    {
        public RectTransformExtensionEditor() : base("RectTransformEditor")
        {
        }

        RectTransform rTransform;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            rTransform = (RectTransform)target;
            if (!rTransform.parent)
                return;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set custom anchors"))
            {
                Undo.RecordObject(target, "CalculateCustomAnchors");
                SetLayoutElements();
                CalculateCustomAnchors(rTransform.parent as RectTransform);
                UnsetLayoutElements();
            }

            if (GUILayout.Button("Fit in anchors"))
            {
                Undo.RecordObject(target, "FitPositionInAnchors");
                FitInAnchors();
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Mirror vertical"))
            {
                Undo.RecordObject(target, "MirrorVertical");
                MirrorVertical();
            }

            if (GUILayout.Button("Mirror horizontal"))
            {
                Undo.RecordObject(target, "MirrorHorizontal");
                MirrorHorizontal();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("fit in parent"))
            {
                Undo.RecordObject(target, "FitInParent");
                FitInParent();
            }

            EditorGUILayout.EndHorizontal();
        }

        void FitInAnchors()
        {
            rTransform.offsetMax = Vector2.zero;
            rTransform.offsetMin = Vector2.zero;
        }

        void CalculateCustomAnchors(RectTransform parent)
        {
            var rect = parent.rect;
            var rightAnchorOffset = rect.width * (1 - rTransform.anchorMax.x);
            var rightOffsetTarget = rect.width - (rightAnchorOffset - rTransform.offsetMax.x);
            var newRightAnchor = rightOffsetTarget / rect.width;

            var upperAnchorOffset = rect.height * (1 - rTransform.anchorMax.y);
            var upperOffsetTarget = rect.height - (upperAnchorOffset - rTransform.offsetMax.y);
            var newUpperAnchor = upperOffsetTarget / rect.height;
            rTransform.anchorMax = new Vector2(newRightAnchor, newUpperAnchor);
            rTransform.offsetMax = Vector2.zero;

            var leftAnchorOffset = rect.width * rTransform.anchorMin.x;
            var leftOffsetTarget = leftAnchorOffset + rTransform.offsetMin.x;
            var newLeftAnchor = leftOffsetTarget / rect.width;

            var lowerAnchorOffset = rect.height * rTransform.anchorMin.y;
            var lowerOffsetTarget = lowerAnchorOffset + rTransform.offsetMin.y;
            var newLowerAnchor = lowerOffsetTarget / rect.height;
            rTransform.anchorMin = new Vector2(newLeftAnchor, newLowerAnchor);
            rTransform.offsetMin = Vector2.zero;
        }

        void MirrorVertical() => RawMirror(false, true);

        void MirrorHorizontal() => RawMirror(true, false);

        void RawMirror(bool isHorizontal, bool isVertical)
        {
            var pivot = rTransform.pivot;
            rTransform.pivot = Vector2.one * .5f;
            SetLayoutElements();
            Vector2 anchorMax = new Vector2(
                isVertical ? 1f - rTransform.anchorMax.x : rTransform.anchorMax.x,
                isHorizontal ? 1f - rTransform.anchorMax.y : rTransform.anchorMax.y);
            Vector2 anchorMin = new Vector2(
                isVertical ? 1f - rTransform.anchorMin.x : rTransform.anchorMin.x,
                isHorizontal ? 1f - rTransform.anchorMin.y : rTransform.anchorMin.y);
            if (anchorMax.x < anchorMin.x)
                (anchorMax.x, anchorMin.x) = (anchorMin.x, anchorMax.x);
            if (anchorMax.y < anchorMin.y)
                (anchorMax.y, anchorMin.y) = (anchorMin.y, anchorMax.y);
            rTransform.anchorMax = anchorMax;
            rTransform.anchorMin = anchorMin;
            rTransform.anchoredPosition = new Vector2(rTransform.anchoredPosition.x * (isVertical ? -1f : 1f),
                rTransform.anchoredPosition.y * (isHorizontal ? -1f : 1f));
            UnsetLayoutElements();
            rTransform.pivot =
                new Vector2(isHorizontal ? pivot.x : 1f - pivot.x, isHorizontal ? 1f - pivot.y : pivot.y);
        }

        List<KeyValuePair<bool, Behaviour>> _layoutComponents = new List<KeyValuePair<bool, Behaviour>>();

        void SetLayoutElements()
        {
            _layoutComponents!.Clear();
            if (rTransform.TryGetComponent<AspectRatioFitter>(out var aspectRatioFitter))
            {
                _layoutComponents.Add(new KeyValuePair<bool, Behaviour>(aspectRatioFitter.enabled, aspectRatioFitter));
                aspectRatioFitter.enabled = false;
            }

            if (rTransform.TryGetComponent<ContentSizeFitter>(out var contentSizeFitter))
            {
                _layoutComponents.Add(new KeyValuePair<bool, Behaviour>(contentSizeFitter.enabled, contentSizeFitter));
                contentSizeFitter.enabled = false;
            }
        }

        void UnsetLayoutElements() => _layoutComponents.ForEach(c => c.Value.enabled = c.Key);

        void FitInParent()
        {
            rTransform.anchorMax = Vector2.one;
            rTransform.anchorMin = Vector2.zero;
            rTransform.offsetMax = Vector2.zero;
            rTransform.offsetMin = Vector2.zero;
        }
    }
}
#endif