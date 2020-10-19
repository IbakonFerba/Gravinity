using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FK.Utility;

namespace FK
{
    namespace Gravinity
    {
        /// <summary>
        /// <para>Class Info</para>
        ///
        /// v1.0 mm/20yy
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        public class BoxGravitySource : MonoBehaviour, IGravitySource
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
            public GravitySourceBaseProperties Properties => _gravitySourceProperties;
            #endregion


            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS
            [SerializeField] private GravitySourceBaseProperties _gravitySourceProperties;

            [Header("Box Properties")]
            [SerializeField] private Vector3 _surfaceDimensions = new Vector3(1.0f, 1.0f, 1.0f);
            #endregion


            // ######################## PUBLIC VARS ######################## //
            #region PUBLIC VARS

            #endregion


            // ######################## PROTECTED VARS ######################## //
            #region PROTECTED VARS

            #endregion


            // ######################## PRIVATE VARS ######################## //
            #region PRIVATE VARS

            #endregion


            // ######################## INITS ######################## //
            #region CONSTRUCTORS

            #endregion

            #region INITS

            ///<summary>
            /// Does the Init for this Behaviour
            ///</summary>
            private void Init()
            {

            }

            #endregion


            // ######################## UNITY EVENT FUNCTIONS ######################## //
            #region UNITY EVENT FUNCTIONS

            private void Start()
            {
                GravityManager.RegisterGravitySource(this);
            }

            private void OnDestroy()
            {
                GravityManager.UnregisterGravitySource(this);
            }

            private void OnDrawGizmos()
            {
                bool inverted = _gravitySourceProperties.Inverted;

                // use position and rotation of this object for the transform matrix of the gizmos, but not the scale
                Matrix4x4 restoreMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(Vector3.zero, _surfaceDimensions);

                float falloffStartRange = _gravitySourceProperties.FalloffStartRange;
                float doubleFalloffStartRange = falloffStartRange * 2.0f;
                GizmosUtility.DrawRoundedBox(Vector3.zero, new Vector3(_surfaceDimensions.x + doubleFalloffStartRange, _surfaceDimensions.y + doubleFalloffStartRange, _surfaceDimensions.z + doubleFalloffStartRange), falloffStartRange, Color.blue);

                float range = _gravitySourceProperties.Range;
                float doubleRange = range * 2.0f;
                GizmosUtility.DrawRoundedBox(Vector3.zero, new Vector3(_surfaceDimensions.x+ doubleRange, _surfaceDimensions.y + doubleRange, _surfaceDimensions.z + doubleRange), range, Color.cyan);

                // reset the matrix
                Gizmos.matrix = restoreMatrix;
            }

            #endregion


            // ######################## FUNCTIONALITY ######################## //
            #region FUNCTIONALITY
            #region IGravitySource
            public Vector3 GetGravityAtPosition(Vector3 position)
            {
                // TODO: inverted box
                Vector3 localPosition = transform.InverseTransformPoint(position);
                Vector3 toSurface = Vector3.zero;
                int outsideFaceCount = 0;

                Vector3 halfDimensions = _surfaceDimensions * 0.5f;
                if(localPosition.x > halfDimensions.x)
                {
                    toSurface.x = halfDimensions.x - localPosition.x;
                    ++outsideFaceCount;
                } else if(localPosition.x < -halfDimensions.x)
                {
                    toSurface.x = -halfDimensions.x - localPosition.x;
                    ++outsideFaceCount;
                }

                if (localPosition.y > halfDimensions.y)
                {
                    toSurface.y = halfDimensions.y - localPosition.y;
                    ++outsideFaceCount;
                }
                else if (localPosition.y < -halfDimensions.y)
                {
                    toSurface.y = -halfDimensions.y - localPosition.y;
                    ++outsideFaceCount;
                }

                if (localPosition.z > halfDimensions.z)
                {
                    toSurface.z = halfDimensions.z - localPosition.z;
                    ++outsideFaceCount;
                }
                else if (localPosition.z < -halfDimensions.z)
                {
                    toSurface.z = -halfDimensions.z - localPosition.z;
                    ++outsideFaceCount;
                }

                if (outsideFaceCount == 0)
                    return (transform.position - position).normalized * _gravitySourceProperties.Strength;

                float distanceToSurface = outsideFaceCount == 1 ? Mathf.Abs(toSurface.x + toSurface.y + toSurface.z) : toSurface.magnitude;

                if (distanceToSurface > _gravitySourceProperties.Range)
                    return Vector3.zero;

                float falloffStartRange = _gravitySourceProperties.FalloffStartRange;
                if (distanceToSurface < falloffStartRange)
                    return transform.TransformDirection(toSurface.normalized * _gravitySourceProperties.Strength);

                float normalizedPositionInFalloffRange = (distanceToSurface - falloffStartRange) / _gravitySourceProperties.FalloffDistance;
                return transform.TransformDirection(toSurface.normalized * _gravitySourceProperties.Strength * normalizedPositionInFalloffRange);
            }

            public bool IsActive()
            {
                return gameObject.activeInHierarchy;
            }

            public bool ShouldBeExclusive()
            {
                return _gravitySourceProperties.CapturePlayerExclusive;
            }
            #endregion
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