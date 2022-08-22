using Twister.Path;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Twister.Placing
{
    [RequireComponent(typeof(AbstractPathTool))]
    [AddComponentMenu("Geometry Tools/Placing/Curve Placing")]
    public class CurvePlacingTool : MonoBehaviour
    {
        [SerializeField]
        private int objectCount = 1;

        [SerializeField]
        private GameObject prefab = null;

        [SerializeField]
        [HideInInspector]
        private GameObject[] children = new GameObject[0];

        #region Unity Callbacks
        private void OnValidate()
        {
            objectCount = Mathf.Max(objectCount, 1);
        }
        #endregion

        public void UpdateInstancesTransforms()
        {
            if(prefab == null)
            {
                Debug.LogError(
                    string.Format(
                        "{0}: No prefab has been assigned.",
                        transform.name));
                return;
            }

            AbstractPathTool[] curveTools = GetComponents<AbstractPathTool>();
            if(curveTools.Length > 1)
            {
                Debug.LogWarning(
                    string.Format(
                        "{0}: There are {1} curve tools assigned to the same object, only one will be used.", 
                        transform.name, 
                        curveTools.Length));
            }

            // Clear all existing instances
            List<GameObject> childList = new(children);
            if(childList.Count > 0)
            {
                for(int i = 0; i < childList.Count; i++)
                {
                    DestroyImmediate(childList[i]);
                }

                childList.Clear();
            }

            // Interpolate and create new objects
            ICurveEvaluator<TransformAnchor> curveEvaluator = curveTools[0].GetEvaluator();
            for(int i = 0; i < objectCount; i++)
            {
                float value = objectCount == 1 ? 0 : i / (float)(objectCount - 1); // Allows to distribute evenly along all the curve's lenght
                TransformAnchor anchor = curveEvaluator.Evaluate(value);

                GameObject instance = Instantiate(prefab, transform);
                instance.name = string.Format("{0} Instance {1:000}",prefab.name, i);
                instance.transform.SetPositionAndRotation(
                    transform.localToWorldMatrix * anchor.position, anchor.rotation);
                instance.transform.localScale = anchor.scale;

                childList.Add(instance);
            }

            children = childList.ToArray();
        }
    }
}
