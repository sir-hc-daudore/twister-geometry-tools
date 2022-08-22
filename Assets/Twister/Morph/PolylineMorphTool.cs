using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister.Morph
{
    [ExecuteInEditMode]
    [AddComponentMenu("Geometry Tools/Morph/Polyline Morph")]
    [RequireComponent(typeof(MeshFilter))]
    public class PolylineMorphTool : MonoBehaviour
    {
        public int editionMode = 0;

        public Vector3 p0 = new(0, 0, -0.5f);
        public Vector3 p1 = new(0, 0, 0.5f);

        public TransformAnchor[] anchors = new TransformAnchor[0];

        public Mesh originalMesh;

        public Mesh currentMesh;

        private void OnDrawGizmosSelected()
        {
            if (editionMode == 0 && originalMesh)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireMesh(originalMesh, transform.position, transform.rotation, transform.localScale);
            }
        }
    }
}
