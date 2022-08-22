using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Twister.Path
{
    [CustomEditor(typeof(PolylinePathTool))]
    public class PolylineToolEditor : Editor
    {
        SerializedProperty anchors;

        #region Unity Editor Callbacks
        private void OnEnable()
        {
            anchors = serializedObject.FindProperty("anchors");
        }

        public override void OnInspectorGUI()
        {
            PolylinePathTool component = (PolylinePathTool)target;

            ShowGUI(component);
        }

        private void OnSceneGUI()
        {
            PolylinePathTool component = (PolylinePathTool)target;

            ShowUpdateHandles(component);
        }
        #endregion Unity Editor Callbacks

        #region Scene Handles methods
        private void ShowUpdateHandles(PolylinePathTool component)
        {
            EditorGUI.BeginChangeCheck();

            TransformAnchor[] updatedAnchors = new TransformAnchor[component.anchors.Length];
            Vector3[] lineVertices = new Vector3[component.anchors.Length];
            for (int i = 0; i < component.anchors.Length; i++)
            {
                updatedAnchors[i] = AnchorHandleHelper.GetAnchor(component.transform, component.anchors[i]);
                lineVertices[i] = updatedAnchors[i].position + component.transform.position;
            }

            Handles.DrawPolyLine(lineVertices);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(component, "Updated anchor points");
                component.anchors = updatedAnchors;
            }
        }
        #endregion Scene Handles methods

        #region GUI layout methods
        private void ShowGUI(PolylinePathTool component)
        {
            int previousAnchorsSize = anchors.arraySize;

            EditorGUILayout.PropertyField(anchors);

            // If an anchor was added, initialize the anchor rotation and scale with valid properties
            // This is required as structs can't be initialized by themselves
            if (anchors.arraySize > previousAnchorsSize)
            {
                SerializedProperty property = anchors.GetArrayElementAtIndex(previousAnchorsSize);

                if(component.anchors.Length > 2)
                {
                    SerializedProperty posProperty = property.FindPropertyRelative("position");
                    Vector3 direction =
                                (component.anchors[component.anchors.Length - 1].position
                                - component.anchors[component.anchors.Length - 2].position)
                                .normalized;
                    posProperty.vector3Value = component.anchors[component.anchors.Length - 1].position + direction;
                }
                property.FindPropertyRelative("rotation").quaternionValue = Quaternion.identity;
                property.FindPropertyRelative("scale").vector3Value = Vector3.one;
            }
            serializedObject.ApplyModifiedProperties();
        }
        #endregion GUI layout methods
    }
}
