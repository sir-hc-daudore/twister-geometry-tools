using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Twister.Placing.EditorTools
{
    [CustomEditor(typeof(CurvePlacingTool))]
    public class CurvePlacingToolEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            // Render default GUI
            base.OnInspectorGUI();

            // Add button for updating
            CurvePlacingTool component = (CurvePlacingTool)target;

            if (GUILayout.Button("Update"))
            {
                component.UpdateInstancesTransforms();
            }
        }
    }
}
