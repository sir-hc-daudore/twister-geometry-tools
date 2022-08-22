using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister
{
    /// <summary>
    /// Node for storing polyline control points, 
    /// based on their 0..1 value relative to the curve's normal length
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CurveNode<T>
    {
        public CurveNode(float value, T content)
        {
            Value = value;
            Content = content;
        }

        public float Value
        {
            get;
        }

        public T Content
        {
            get;
        }
    }
}