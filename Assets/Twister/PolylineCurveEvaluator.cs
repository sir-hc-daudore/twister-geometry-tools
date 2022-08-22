using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister
{
    /// <summary>
    /// Interpolator for a polyline curve defined by MorphAnchors
    /// </summary>
    public class PolylineCurveEvaluator : ICurveEvaluator<TransformAnchor>
    {
        public float Length
        {
            get; private set;
        } = 0.0f;

        private List<CurveNode<TransformAnchor>> anchorLut;

        public PolylineCurveEvaluator(TransformAnchor[] anchors)
        {
            // Index anchors based on how much length these provide to the total length
            this.anchorLut = new List<CurveNode<TransformAnchor>>
            {
                new CurveNode<TransformAnchor>(0.0f, anchors[0])
            };

            // Index nodes based on the length of the polyline up to that anchor
            for (int i = 1; i < anchors.Length; i++)
            {
                Length += Vector3.Distance(anchors[i - 1].position, anchors[i].position);
                this.anchorLut.Add(new CurveNode<TransformAnchor>(Length, anchors[i]));
            }
        }

        public TransformAnchor Evaluate(float t)
        {
            t = Mathf.Clamp01(t);

            TransformAnchor result = new();
            float lookupLength = t * Length;

            // Search for boundary values
            // Find anchor that has a V value greater than t
            int i = GetCurveEndAnchorIndex(lookupLength);
            int j = Mathf.Max(i - 1, 0);

            CurveNode<TransformAnchor> previous = anchorLut[j];
            CurveNode<TransformAnchor> next = anchorLut[i];

            // Interpolate between previous and current anchor
            float u = (lookupLength - previous.Value) / (next.Value - previous.Value);
            result.position = Vector3.Lerp(previous.Content.position, next.Content.position, u);
            result.rotation = Quaternion.Lerp(previous.Content.rotation, next.Content.rotation, u);
            result.scale = Vector3.Lerp(previous.Content.scale, next.Content.scale, u);

            return result;
        }

        private int GetCurveEndAnchorIndex(float value)
        {
            int i = 0;

            for (; i < anchorLut.Count - 1; i++)
            {
                // Find anchor that has the smallest V value greater than t
                if (value < anchorLut[i].Value)
                {
                    break;
                }
            }

            return i;
        }
    }
}
