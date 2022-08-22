using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Twister
{
    public static class AnchorHandleHelper
    {
        /// <summary>
        /// Helper method to draw transform handles for morph anchors
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        public static TransformAnchor GetAnchor(Transform componentTransform, TransformAnchor anchor)
        {
            TransformAnchor tempAnchor = anchor;
            // Transform to world space to visually align the scene mesh with the handle
            tempAnchor.position += componentTransform.position;
            Handles.TransformHandle(ref tempAnchor.position, ref tempAnchor.rotation, ref tempAnchor.scale);
            // Transform back to model space
            tempAnchor.position -= componentTransform.position;
            return tempAnchor;
        }
    }
}
