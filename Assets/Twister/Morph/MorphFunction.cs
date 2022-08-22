using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister.Morph
{
    /// <summary>
    /// Class providing mesh morph function based on curve evaluator
    /// </summary>
    public static class MorphFunction
    {
        /// <summary>
        /// Morphs a mesh vertices based on their relative positions towards the p0-p1 segment, 
        /// and transforms them by interpolation based on sent curve evaluator
        /// </summary>
        /// <param name="originalMesh">Mesh to morph</param>
        /// <param name="p0">First point of segment</param>
        /// <param name="p1">Last point of segment</param>
        /// <param name="curveEvaluator">Curve evaluator used for interpolating anchors</param>
        /// <returns>Morphed mesh linearly across the anchors</returns>
        public static Mesh MorphMesh(Mesh originalMesh, Vector3 p0, Vector3 p1, ICurveEvaluator<TransformAnchor> curveEvaluator)
        {

            Mesh newMesh = Object.Instantiate(originalMesh);
            newMesh.name = string.Concat(
                originalMesh.name,
                "_",
                Random.value.GetHashCode());

            Vector3[] newVertices = newMesh.vertices;
            Vector3[] currentVertices = originalMesh.vertices;

            Vector3 segmentDirection = p1 - p0;
            float segmentMagnitude = segmentDirection.magnitude;

            for (int i = 0; i < originalMesh.vertexCount; i++)
            {
                Vector3 vertex = currentVertices[i];

                // Get projection into path
                Vector3 vertexDirection = (vertex - p0) / segmentMagnitude;
                float t = Vector3.Dot(segmentDirection / segmentMagnitude, vertexDirection);

                // Interpolate base anchor
                Vector3 baseAnchorPosition = p0 * (1 - t) + p1 * t;

                // Calculate target anchor
                TransformAnchor targetAnchor = curveEvaluator.Evaluate(t);

                // Transform vertex to relative anchor postion
                Vector3 anchorSpaceVertex = vertex - baseAnchorPosition;

                // Interpolate target anchor
                Vector3 targetPosition = targetAnchor.position;
                Quaternion targetRotation = targetAnchor.rotation;
                Vector3 targetScale = targetAnchor.scale;

                // Transform vertex
                newVertices[i] = ((targetRotation * Vector3.Scale(anchorSpaceVertex, targetScale)) + targetPosition);
            }

            newMesh.SetVertices(newVertices);
            newMesh.RecalculateBounds();
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();

            return newMesh;
        }
    }
}
