using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister
{
    /// <summary>
    /// Interpolator for a Bezier curve defined by BezierAnchors
    /// </summary>
    public class BezierCurveEvaluator : ICurveEvaluator<TransformAnchor>
    {
        /// <summary>
        /// Constant used for calculation of tangent vector
        /// </summary>
        private static readonly float E = 0.0001f;
        private static readonly float ARC_STEPS = 16f;

        private Vector3 up;
        /// <summary>
        /// Look up table (LUT) for anchor positions.
        /// </summary>
        private List<CurveNode<BezierAnchor>> anchorLut;

        public float ArcLength
        {
            get;
        }

        public BezierCurveEvaluator(Vector3 up, BezierAnchor[] anchors)
        {
            this.up = up;
            this.up.Normalize(); // Make sure is a normal vector

            // Calculate arc length and index bezier curve nodes into LUT based
            // on the accumulated length
            float arcLenght = 0;
            this.anchorLut = new(anchors.Length);

            this.anchorLut.Add(new CurveNode<BezierAnchor>(arcLenght, anchors[0]));
            BezierAnchor previousAnchor = anchors[0]; 
            for(int i = 1; i < anchors.Length; i++)
            {
                BezierAnchor currentAnchor = anchors[i];
                Vector3 lastPos = previousAnchor.position;
                Vector3[] controlPoints = new Vector3[]
                {
                        previousAnchor.position,
                        previousAnchor.GetLeftAnchorPosition(),
                        currentAnchor.GetRightAnchorPosition(),
                        currentAnchor.position
                };
                // Caluculate arc lenght for Bezier from previousAnchor to currentAnchor
                for (int j = 0; j < ARC_STEPS; j++)
                {
                    float t = j / ARC_STEPS;
                    Vector3 newPos = EvaluateBezier(controlPoints, t);
                    arcLenght += Vector3.Distance(lastPos, newPos);
                    lastPos = newPos;
                }
                // Add anchor to LUT
                this.anchorLut.Add(new CurveNode<BezierAnchor>(arcLenght, currentAnchor));
                previousAnchor = currentAnchor;
            }

            ArcLength = arcLenght;
        }

        public TransformAnchor Evaluate(float t)
        {
            t = Mathf.Clamp01(t);
            float desiredArcLenght = t * ArcLength;

            TransformAnchor anchor = new();

            // Find control points in the curve
            BezierAnchor a0, a1;
            Vector3 c0, c1, c2, c3;
            // Lookup in  the spline data structure
            int topIndexValue = GetCurveEndAnchorIndex(desiredArcLenght);
            int downIndexValue = Mathf.Max(topIndexValue - 1, 0);

            CurveNode<BezierAnchor> startNode = anchorLut[downIndexValue];
            CurveNode<BezierAnchor> endNode = anchorLut[topIndexValue];
            a0 = startNode.Content; 
            a1 = endNode.Content;
            c0 = a0.position; // Low end of the bezier curve
            c3 = a1.position; // High end of the bezier curve
            c1 = a0.GetLeftAnchorPosition(); // Low end control point for curve 
            c2 = a1.GetRightAnchorPosition(); // High end control point for curve 

            // For interpolation, we need to recalculate the t
            // based on the actual arc lenghts of each anchor node
            float localT = (desiredArcLenght - startNode.Value) / (endNode.Value - startNode.Value);

            // Evaulate bezier at t position
            anchor.position = EvaluateBezier(new Vector3[] { c0, c1, c2, c3}, localT);
            // Evaluate bezier at t + E position
            Vector3 closePosition = EvaluateBezier(new Vector3[] { c0, c1, c2, c3 }, localT + E);
            // Calculate tangent
            Vector3 tangent = EvaluateTangent(anchor.position, closePosition).normalized; // Make sure is a normal vector

            // Evaluate rotation
            anchor.rotation = Quaternion.LookRotation(tangent, up);

            // Linearly interpolate scale
            anchor.scale = Vector3.Lerp(a0.scale, a1.scale, localT);

            return anchor;
        }

        private int GetCurveEndAnchorIndex(float value)
        {
            int i = 0;

            for (; i < anchorLut.Count - 1; i++)
            {
                // Find anchor that has a V value greater than t
                if (anchorLut[i].Value > value)
                {
                    break;
                }
            }

            return i;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="c"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        private Vector3 EvaluateBezier(Vector3[] c, float t)
        {
            Vector3
                    AB = c[0] * (1 - t) + t * c[1],
                    BC = c[1] * (1 - t) + t * c[2],
                    CD = c[2] * (1 - t) + t * c[3],
                    ABC = AB * (1 - t) + t * BC,
                    BCD = BC * (1 - t) + t * CD,
                    P = ABC * (1 - t) + t * BCD;

            return P;
        }

        /// <summary>
        /// See: https://gamedev.stackexchange.com/questions/101402/how-to-calculate-normal-vector-of-a-b%C3%A9zier-curve
        /// and https://math.stackexchange.com/questions/1231603/finding-tangent-vector-for-a-curve-at-a-given-point
        /// and https://math.stackexchange.com/questions/517147/how-to-calculate-the-normal-vector-for-a-bezier-curve
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <returns></returns>
        private Vector3 EvaluateTangent(Vector3 p0, Vector3 p1)
        {
            return (p1 - p0) / Vector3.Distance(p0, p1);
        }
    }

}
