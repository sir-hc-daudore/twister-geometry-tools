using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister.Path
{
    [AddComponentMenu("Geometry Tools/Path/Polyline Path Tool")]
    public class PolylinePathTool : AbstractPathTool
    {
        [SerializeField]
        public TransformAnchor[] anchors = new TransformAnchor[0];

        public override ICurveEvaluator<TransformAnchor> GetEvaluator()
        {
            return new PolylineCurveEvaluator(anchors);
        }
    }
}
