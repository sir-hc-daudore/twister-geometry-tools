using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister
{
    /// <summary>
    /// Struct for anchors specifing transformations to apply
    /// </summary>
    [System.Serializable]
    public struct TransformAnchor
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
    }
}
