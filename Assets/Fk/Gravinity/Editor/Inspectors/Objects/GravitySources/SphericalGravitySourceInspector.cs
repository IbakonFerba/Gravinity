using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FK
{
    namespace Gravinity
    {
        namespace Editor
        {
            /// <summary>
            /// <para>Class Info</para>
            ///
            /// v1.0 mm/20yy
            /// Written by Fabian Kober
            /// fabian-kober@gmx.net
            /// </summary>
            [CustomEditor(typeof(SphericalGravitySource))]
            [CanEditMultipleObjects]
            public class SphericalGravitySourceInspector : UnityEditor.Editor
            {
                // ######################## STRUCTS & CLASSES ######################## //
                #region STRUCTS & CLASSES

                #endregion


                // ######################## ENUMS & DELEGATES ######################## //
                #region ENUMS & DELEGATES

                #endregion


                // ######################## EVENTS ######################## //
                #region EVENTS

                #endregion


                // ######################## PROPERTIES ######################## //
                #region PROPERTIES

                #endregion


                // ######################## EXPOSED VARS ######################## //
                #region EXPOSED VARS

                #endregion


                // ######################## PUBLIC VARS ######################## //
                #region PUBLIC VARS

                #endregion


                // ######################## PROTECTED VARS ######################## //
                #region PROTECTED VARS

                #endregion


                // ######################## PRIVATE VARS ######################## //
                #region PRIVATE VARS
                private SerializedProperty _gravitySourcePropertiesProperty;
                private SerializedProperty _invertedProperty;
                private SerializedProperty _invertedSphereSurfaceRadiusProperty;
                #endregion


                // ######################## INITS ######################## //
                #region CONSTRUCTORS

                #endregion

                #region INITS



                #endregion


                // ######################## UNITY EVENT FUNCTIONS ######################## //
                #region UNITY EVENT FUNCTIONS
                private void OnEnable()
                {
                    _gravitySourcePropertiesProperty = serializedObject.FindProperty("_gravitySourceProperties");
                    _invertedProperty = serializedObject.FindProperty("Inverted");
                    _invertedSphereSurfaceRadiusProperty = serializedObject.FindProperty("_invertedSphereSurfaceRadius");
                }

                public override void OnInspectorGUI()
                {
                    serializedObject.Update();
                    EditorGUILayout.PropertyField(_gravitySourcePropertiesProperty);
                    EditorGUILayout.PropertyField(_invertedProperty);
                    if(_invertedProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(_invertedSphereSurfaceRadiusProperty);
                    }
                    serializedObject.ApplyModifiedProperties();
                }


                #endregion


                // ######################## FUNCTIONALITY ######################## //
                #region FUNCTIONALITY

                #endregion


                // ######################## COROUTINES ######################## //
                #region COROUTINES 

                #endregion


                // ######################## UTILITIES ######################## //
                #region UTILITIES

                #endregion
            }

        }
    }
}
