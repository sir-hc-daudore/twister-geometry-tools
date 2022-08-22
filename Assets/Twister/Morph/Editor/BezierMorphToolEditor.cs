using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Twister.Morph
{
    [CustomEditor(typeof(BezierMorphTool))]
    public class BezierMorphToolEditor : Editor
    {
        private static readonly string[] TAB_OPTIONS = new string[] { "Setup Mode", "Modifier Mode" };
        private const int HANDLE_BEZIER_SEGMENTS = 16;

        SerializedProperty p0, p1, anchors;
        SerializedProperty originalMesh;

        #region Unity Editor Callbacks
        private void OnEnable()
        {
            p0 = serializedObject.FindProperty("p0");
            p1 = serializedObject.FindProperty("p1");
            anchors = serializedObject.FindProperty("anchors");
            originalMesh = serializedObject.FindProperty("originalMesh");
        }

        /// <summary>
        /// Draws handles for edition anchors
        /// </summary>
        private void OnSceneGUI()
        {
            BezierMorphTool component = (BezierMorphTool)target;

            switch (component.editionMode)
            {
                case 0:
                    ShowSetupHandles(component);
                    break;
                case 1:
                    ShowUpdateHandles(component);
                    break;
            }
        }

        /// <summary>
        /// Draws custom inspector for component
        /// </summary>
        public override void OnInspectorGUI()
        {
            BezierMorphTool component = (BezierMorphTool)target;

            GUILayout.BeginHorizontal();
            component.editionMode = GUILayout.Toolbar(component.editionMode, TAB_OPTIONS);
            GUILayout.EndHorizontal();

            GUILayout.BeginVertical();
            switch (component.editionMode)
            {
                case 0:
                    ShowSetupGUI();
                    break;
                case 1:
                    ShowModifierGUI(component);
                    break;
            }
            GUILayout.Space(20);
            ShowButtons(component);
            GUILayout.EndVertical();
        }
        #endregion Unity Editor Callbacks

        #region Scene Handles methods
        private void ShowSetupHandles(BezierMorphTool component)
        {
            EditorGUI.BeginChangeCheck();
            Vector3 worldP0Position =
                Handles.PositionHandle(component.p0 + component.transform.position, Quaternion.identity);
            Vector3 worldP1Position =
                Handles.PositionHandle(component.p1 + component.transform.position, Quaternion.identity);

            Handles.DrawLine(worldP0Position, worldP1Position, 2.0f);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(component, "Updated morph setup");
                component.p0 = worldP0Position - component.transform.position;
                component.p1 = worldP1Position - component.transform.position;
            }
        }

        private void ShowUpdateHandles(BezierMorphTool component)
        {
            Color originalHandleColor = Handles.color;

            EditorGUI.BeginChangeCheck();

            // Draw bezier anchors
            if(component.anchors.Length > 0)
            {
                BezierAnchor[] updatedAnchors = new BezierAnchor[component.anchors.Length];
                BezierAnchor previousAnchor = component.anchors[0];
                Vector3 transformPos = component.transform.position;
                for (int i = 0; i < component.anchors.Length; i++)
                {
                    updatedAnchors[i] = GetAnchor(component.transform, component.anchors[i]);

                    if (i > 0)
                    {
                        Vector3[] bezierPoints =
                            Handles.MakeBezierPoints(
                                previousAnchor.position + transformPos,
                                updatedAnchors[i].position + transformPos,
                                previousAnchor.GetLeftAnchorPosition() + transformPos,
                                updatedAnchors[i].GetRightAnchorPosition() + transformPos,
                                HANDLE_BEZIER_SEGMENTS);
                        Handles.color = Color.white;
                        Handles.DrawAAPolyLine(4.0f, bezierPoints);
                    }

                    Handles.color = Color.yellow;
                    Handles.DrawLine(updatedAnchors[i].GetLeftAnchorPosition() + transformPos,
                        updatedAnchors[i].GetRightAnchorPosition() + transformPos, 2.0f);

                    previousAnchor = updatedAnchors[i];
                }

                // Validate and save changes
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(component, "Updated anchor points");
                    component.anchors = updatedAnchors;
                }
            }

            Handles.color = originalHandleColor;
        }
        #endregion Scene Handles methods

        #region GUI layout methods
        private void ShowSetupGUI()
        {
            EditorGUILayout.PropertyField(originalMesh);
            EditorGUILayout.PropertyField(p0);
            EditorGUILayout.PropertyField(p1);

            serializedObject.ApplyModifiedProperties();
        }

        private void ShowModifierGUI(BezierMorphTool component)
        {
            int previousAnchorsSize = anchors.arraySize;

            EditorGUILayout.PropertyField(anchors);

            // If an anchor was added, initialize the anchor rotation and scale with valid properties
            // This is required as structs can't be initialized by themselves
            if (anchors.arraySize > previousAnchorsSize)
            {
                SerializedProperty property = anchors.GetArrayElementAtIndex(previousAnchorsSize);

                SerializedProperty posProperty = property.FindPropertyRelative("position");
                switch (previousAnchorsSize)
                {
                    case 0:
                        posProperty.vector3Value = component.p0;
                        break;
                    case 1:
                        posProperty.vector3Value = component.p1;
                        break;
                    default:
                        Vector3 direction =
                            (component.anchors[component.anchors.Length - 1].position
                            - component.anchors[component.anchors.Length - 2].position)
                            .normalized;
                        posProperty.vector3Value = component.anchors[component.anchors.Length - 1].position + direction;
                        property.FindPropertyRelative("controlAnchor").vector3Value = 
                            (posProperty.vector3Value 
                            - component.anchors[component.anchors.Length - 1].position)
                            .normalized;
                        break;
                }
                property.FindPropertyRelative("scale").vector3Value = Vector3.one;
            }
            serializedObject.ApplyModifiedProperties();
        }

        public void ShowButtons(BezierMorphTool component)
        {
            if (GUILayout.Button("Update mesh"))
            {
                if (component.anchors.Length < 2)
                {
                    Debug.LogWarning("At least two anchors are required for morphing a mesh.");
                }
                else
                {
                    // Get curve evaluator
                    ICurveEvaluator<TransformAnchor> curveEvaluator = new BezierCurveEvaluator(Vector3.up, component.anchors);

                    Mesh newMesh = MorphFunction.MorphMesh(
                        component.originalMesh,
                        component.p0, component.p1,
                        curveEvaluator);

                    Undo.RecordObject(component, "Updated mesh morph");
                    DeleteModifiedMesh(component); // Need to delete the previous mesh
                    SetUpdatedMeshInComponent(component, newMesh);
                    UpdateComponentsInTool(component, newMesh);
                }
            }

            if (GUILayout.Button("Reset mesh"))
            {
                if (component.originalMesh)
                {
                    Undo.RecordObject(component, "Reset mesh morph");
                    DeleteModifiedMesh(component);
                    UpdateComponentsInTool(component, component.originalMesh);
                }
                else
                {
                    Debug.LogWarning("Reset mesh requires that an original mesh is set under Setup Mode");
                }
            }
        }

        #endregion GUI layout methods

        /// <summary>
        /// Helper method to draw transform handles for morph anchors
        /// </summary>
        /// <param name="anchor"></param>
        /// <returns></returns>
        private BezierAnchor GetAnchor(Transform componentTransform, BezierAnchor anchor)
        {
            BezierAnchor tempAnchor = anchor;
            Vector3 transformPos = componentTransform.position;
            // Transform to world space to visually align the scene mesh with the handle
            tempAnchor.position += transformPos;
            tempAnchor.position = 
                Handles.PositionHandle(tempAnchor.position, Quaternion.identity);
            tempAnchor.controlAnchor = 
                Handles.PositionHandle(tempAnchor.GetLeftAnchorPosition(), Quaternion.identity) - tempAnchor.position;
            tempAnchor.controlAnchor =
                -(Handles.PositionHandle(tempAnchor.GetRightAnchorPosition(), Quaternion.identity) - tempAnchor.position);
            // Transform back to model space
            tempAnchor.position -= componentTransform.position;
            return tempAnchor;
        }

        private void SetUpdatedMeshInComponent(BezierMorphTool component, Mesh newMesh)
        {
            if (component.currentMesh != component.originalMesh && component.currentMesh != null)
            {
                DestroyImmediate(component.currentMesh);
            }

            component.currentMesh = newMesh;
        }

        private void DeleteModifiedMesh(BezierMorphTool component)
        {
            if (component.currentMesh != component.originalMesh && component.currentMesh != null)
            {
                DestroyImmediate(component.currentMesh);
            }

            component.currentMesh = null;
        }

        private void UpdateComponentsInTool(BezierMorphTool component, Mesh newMesh)
        {
            // Update Unity mesh dependencies used in the same filter
            MeshFilter renderer = component.GetComponent<MeshFilter>();
            if (renderer)
            {
                renderer.mesh = newMesh;
            }

            MeshCollider collider = component.GetComponent<MeshCollider>();
            if (collider)
            {
                collider.sharedMesh = newMesh;
            }
        }
    }
}
