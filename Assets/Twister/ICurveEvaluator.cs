using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister
{
    /// <summary>
    /// Interface for evaluating curves
    /// </summary>
    /// <typeparam name="T">Type returned by the evaluation of curve</typeparam>
    public interface ICurveEvaluator<T>
    {
        T Evaluate(float t);
    }
}