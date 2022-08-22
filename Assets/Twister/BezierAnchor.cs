using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister
{
    /// <summary>
    /// Struct for bezier anchor, with position and control anchor
    /// </summary>
    [System.Serializable]
    public struct BezierAnchor
    {
        public Vector3 position;
        public Vector3 controlAnchor;
        public Vector3 scale;

        public Vector3 GetLeftAnchorPosition()
        {
            return position + controlAnchor;
        }

        public Vector3 GetRightAnchorPosition()
        {
            return position - controlAnchor;
        }
    }
}
