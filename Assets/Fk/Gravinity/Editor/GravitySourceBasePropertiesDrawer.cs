using UnityEngine;
using UnityEditor;

namespace FK
{
    namespace Gravinity
    {
        namespace Editor
        {
            /// <summary>
            /// <para>Custom Property drawer for GravitySourceBaseProperties</para>
            ///
            /// v1.0 10/2020
            /// Written by Fabian Kober
            /// fabian-kober@gmx.net
            /// </summary>
            [CustomPropertyDrawer(typeof(GravitySourceBaseProperties))]
            public class GravitySourceBasePropertiesDrawer : PropertyDrawer
            {
                // ######################## UNITY EVENT FUNCTIONS ######################## //
                #region UNITY EVENT FUNCTIONS
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    bool canBeInverted = property.FindPropertyRelative("_canBeInverted").boolValue;

                    SerializedProperty currentProperty = property.Copy();
                    Rect currentPosition = position;
                    while(currentProperty.NextVisible(true) && !SerializedProperty.EqualContents(property.GetEndProperty(), currentProperty))
                    {
                        if(currentProperty.name == "_inverted" && !canBeInverted)
                        {
                            continue;
                        }
                        currentPosition.height = EditorGUI.GetPropertyHeight(currentProperty);
                        EditorGUI.PropertyField(currentPosition, currentProperty);
                        currentPosition.y += EditorGUI.GetPropertyHeight(currentProperty);
                    }
                }
                #endregion


                // ######################## FUNCTIONALITY ######################## //
                #region FUNCTIONALITY
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    bool canBeInverted = property.FindPropertyRelative("_canBeInverted").boolValue;

                    float height = 0.0f;
                    SerializedProperty currentProperty = property.Copy();
                    while (currentProperty.NextVisible(true) && !SerializedProperty.EqualContents(property.GetEndProperty(), currentProperty))
                    {
                        if (currentProperty.name == "_inverted" && !canBeInverted)
                        {
                            continue;
                        }

                        height += EditorGUI.GetPropertyHeight(currentProperty, label, false);
                    } 
                    return height;
                }
                #endregion
            }
        }
    }
}
