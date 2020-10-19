using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FK.Utility;

namespace FK
{
    namespace Gravinity
    {
        /// <summary>
        /// <para>A Box shaped gravity source that pulls straight down on its faces.</para>
        /// <para>On the outside its edges are rounded, on the inside they are square</para>
        ///
        /// v1.0 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        public class BoxGravitySource : MonoBehaviour, IGravitySource
        {
            // ######################## PROPERTIES ######################## //
            #region PROPERTIES
            public GravitySourceBaseProperties Properties => _gravitySourceProperties;
            #endregion


            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS
            [SerializeField] private GravitySourceBaseProperties _gravitySourceProperties;

            [Header("Box Properties")]
            [SerializeField] private bool _inverted = false;
            public Vector3 SurfaceDimensions = new Vector3(1.0f, 1.0f, 1.0f);
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
                // use position and rotation of this object for the transform matrix of the gizmos, but not the scale
                Matrix4x4 restoreMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

                // surface
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(Vector3.zero, SurfaceDimensions);

                // falloff start range
                float falloffStartRange = _gravitySourceProperties.FalloffStartRange;
                float doubleFalloffStartRange = falloffStartRange * 2.0f;
                if (!_inverted)
                {
                    GizmosUtility.DrawRoundedBox(Vector3.zero, new Vector3(SurfaceDimensions.x + doubleFalloffStartRange, SurfaceDimensions.y + doubleFalloffStartRange, SurfaceDimensions.z + doubleFalloffStartRange), falloffStartRange, Color.blue);
                } else
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(SurfaceDimensions.x - doubleFalloffStartRange, SurfaceDimensions.y - doubleFalloffStartRange, SurfaceDimensions.z - doubleFalloffStartRange));
                }

                // range
                float range = _gravitySourceProperties.Range;
                float doubleRange = range * 2.0f;
                if (!_inverted)
                {
                    GizmosUtility.DrawRoundedBox(Vector3.zero, new Vector3(SurfaceDimensions.x + doubleRange, SurfaceDimensions.y + doubleRange, SurfaceDimensions.z + doubleRange), range, Color.cyan);
                } else
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(SurfaceDimensions.x - doubleRange, SurfaceDimensions.y - doubleRange, SurfaceDimensions.z - doubleRange));
                }

                // reset the matrix
                Gizmos.matrix = restoreMatrix;
            }

            #endregion


            // ######################## FUNCTIONALITY ######################## //
            #region FUNCTIONALITY
            #region IGravitySource
            public Vector3 GetGravityAtPosition(Vector3 position)
            {
                Vector3 localPosition = transform.InverseTransformPoint(position);
                Vector3 halfDimensions = SurfaceDimensions * 0.5f;

                if (_inverted)
                {
                    Vector3 distancesToFaces;
                    distancesToFaces.x = halfDimensions.x - Mathf.Abs(localPosition.x);
                    distancesToFaces.y = halfDimensions.y - Mathf.Abs(localPosition.y);
                    distancesToFaces.z = halfDimensions.z - Mathf.Abs(localPosition.z);

                    Vector3 gravity = Vector3.zero;
                    if (distancesToFaces.x < distancesToFaces.y)
                    {
                        if (distancesToFaces.x < distancesToFaces.z)
                        {
                            gravity.x = GetInsideGravityOverFace(localPosition.x, distancesToFaces.x);
                        }
                        else
                        {
                            gravity.z = GetInsideGravityOverFace(localPosition.z, distancesToFaces.z);

                        }
                    }
                    else if (distancesToFaces.y < distancesToFaces.z)
                    {
                        gravity.y = GetInsideGravityOverFace(localPosition.y, distancesToFaces.y);

                    }
                    else
                    {
                        gravity.z = GetInsideGravityOverFace(localPosition.z, distancesToFaces.z);

                    }

                    return transform.TransformDirection(gravity);
                }
                else
                {
                    Vector3 toSurface = Vector3.zero;
                    int outsideFaceCount = 0;

                    if (localPosition.x > halfDimensions.x)
                    {
                        toSurface.x = halfDimensions.x - localPosition.x;
                        ++outsideFaceCount;
                    }
                    else if (localPosition.x < -halfDimensions.x)
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

                    // no gravity inside the box
                    if (outsideFaceCount == 0) 
                        return Vector3.zero;

                    float distanceToSurface = outsideFaceCount == 1 ? Mathf.Abs(toSurface.x + toSurface.y + toSurface.z) : toSurface.magnitude;

                    if (distanceToSurface > _gravitySourceProperties.Range)
                        return Vector3.zero;

                    float falloffStartRange = _gravitySourceProperties.FalloffStartRange;
                    if (distanceToSurface < falloffStartRange)
                        return transform.TransformDirection(toSurface.normalized * _gravitySourceProperties.Strength);

                    float normalizedPositionInFalloffRange = (distanceToSurface - falloffStartRange) / _gravitySourceProperties.FalloffDistance;
                    return transform.TransformDirection(toSurface.normalized * _gravitySourceProperties.Strength * normalizedPositionInFalloffRange);
                }
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

            private float GetInsideGravityOverFace(float coordinate, float distance)
            {
                if (distance > _gravitySourceProperties.Range)
                    return 0.0f;

                float falloffStartRange = _gravitySourceProperties.FalloffStartRange;
                if (distance < falloffStartRange)
                    return coordinate > 0.0f ? _gravitySourceProperties.Strength : -_gravitySourceProperties.Strength;

                float normalizedPositionInFalloffRange = (distance - falloffStartRange) / _gravitySourceProperties.FalloffDistance;
                float gravity = normalizedPositionInFalloffRange * _gravitySourceProperties.Strength;
                return coordinate > 0.0f ? gravity : -gravity;
            }
            #endregion
        }

    }
}