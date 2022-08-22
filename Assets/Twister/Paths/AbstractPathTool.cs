using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister.Path
{
    public abstract class AbstractPathTool : MonoBehaviour
    {
        public abstract ICurveEvaluator<TransformAnchor> GetEvaluator();
    }
}
