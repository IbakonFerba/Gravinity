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
        public class BoxGravitySource
            : MonoBehaviour
            , IGravitySource
            , IGravitySourceBasePropertiesChangeListener
        {
            // ######################## PROPERTIES ######################## //
            #region PROPERTIES
            public GravitySourceBaseProperties Properties => _gravitySourceProperties;

            /// <summary>
            /// Dimensions of the surface of the box. Setting this causes recalculation of the bounds used to determine whether this source has an effect at a given position as well as some cached values
            /// </summary>
            public Vector3 SurfaceDimensions
            {
                get => _surfaceDimensions;
                set
                {
                    _surfaceDimensions = value;
                    _halfSurfaceDimensions = _surfaceDimensions * 0.5f;
                    RecalculateBoundsSize();
                }
            }
            #endregion


            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS
            [SerializeField] private GravitySourceBaseProperties _gravitySourceProperties;

            [Header("Box Properties")]
            [SerializeField]
            [Tooltip("If true, the gravity of the box will be inside it, pushing outwards to its faces. Otherwise the gavity will be outside the box and pull towards its faces in the form of a rounded box")]
            private bool _inverted = false;
            [SerializeField]
            [Tooltip("Dimensions of the surface of the box.\nIndicated by the yellow Gizmo")]
            private Vector3 _surfaceDimensions = new Vector3(1.0f, 1.0f, 1.0f);
            #endregion


            // ######################## PRIVATE VARS ######################## //
            #region PRIVATE VARS
            private OBB _bounds;

            private Vector3 _halfSurfaceDimensions;
            #endregion


            // ######################## UNITY EVENT FUNCTIONS ######################## //
            #region UNITY EVENT FUNCTIONS

            private void Start()
            {
                GravityManager.RegisterGravitySource(this);
                _gravitySourceProperties.RegisterListener(this);
                RecalculateBounds();
            }

            private void OnDestroy()
            {
                GravityManager.UnregisterGravitySource(this);
                _gravitySourceProperties.UnregisterListener(this);
            }

            private void OnValidate()
            {
                _halfSurfaceDimensions = _surfaceDimensions * 0.5f;
                RecalculateBoundsSize();
            }

            private void OnDrawGizmos()
            {
                // use position and rotation of this object for the transform matrix of the gizmos, but not the scale
                Matrix4x4 restoreMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

                // surface
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(Vector3.zero, _surfaceDimensions);

                // falloff start range
                float falloffStartRange = _gravitySourceProperties.FalloffStartRange;
                float doubleFalloffStartRange = falloffStartRange * 2.0f;
                if (!_inverted)
                {
                    GizmosUtility.DrawRoundedBox(Vector3.zero, new Vector3(_surfaceDimensions.x + doubleFalloffStartRange, _surfaceDimensions.y + doubleFalloffStartRange, _surfaceDimensions.z + doubleFalloffStartRange), falloffStartRange, Color.blue);
                }
                else
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(_surfaceDimensions.x - doubleFalloffStartRange, _surfaceDimensions.y - doubleFalloffStartRange, _surfaceDimensions.z - doubleFalloffStartRange));
                }

                // range
                float range = _gravitySourceProperties.Range;
                float doubleRange = range * 2.0f;
                if (!_inverted)
                {
                    GizmosUtility.DrawRoundedBox(Vector3.zero, new Vector3(_surfaceDimensions.x + doubleRange, _surfaceDimensions.y + doubleRange, _surfaceDimensions.z + doubleRange), range, Color.cyan);
                }
                else
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireCube(Vector3.zero, new Vector3(_surfaceDimensions.x - doubleRange, _surfaceDimensions.y - doubleRange, _surfaceDimensions.z - doubleRange));
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
                if (transform.hasChanged)
                    RecalculateBoundsTransform();

                if (!_bounds.Contains(position))
                    return Vector3.zero;

                Vector3 localPosition = transform.InverseTransformPoint(position);

                if (_inverted)
                {
                    // TODO: THere is some issue with the inverted box range
                    Vector3 distancesToFaces;
                    distancesToFaces.x = _halfSurfaceDimensions.x - Mathf.Abs(localPosition.x);
                    distancesToFaces.y = _halfSurfaceDimensions.y - Mathf.Abs(localPosition.y);
                    distancesToFaces.z = _halfSurfaceDimensions.z - Mathf.Abs(localPosition.z);

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

                    if (localPosition.x > _halfSurfaceDimensions.x)
                    {
                        toSurface.x = _halfSurfaceDimensions.x - localPosition.x;
                        ++outsideFaceCount;
                    }
                    else if (localPosition.x < -_halfSurfaceDimensions.x)
                    {
                        toSurface.x = -_halfSurfaceDimensions.x - localPosition.x;
                        ++outsideFaceCount;
                    }

                    if (localPosition.y > _halfSurfaceDimensions.y)
                    {
                        toSurface.y = _halfSurfaceDimensions.y - localPosition.y;
                        ++outsideFaceCount;
                    }
                    else if (localPosition.y < -_halfSurfaceDimensions.y)
                    {
                        toSurface.y = -_halfSurfaceDimensions.y - localPosition.y;
                        ++outsideFaceCount;
                    }

                    if (localPosition.z > _halfSurfaceDimensions.z)
                    {
                        toSurface.z = _halfSurfaceDimensions.z - localPosition.z;
                        ++outsideFaceCount;
                    }
                    else if (localPosition.z < -_halfSurfaceDimensions.z)
                    {
                        toSurface.z = -_halfSurfaceDimensions.z - localPosition.z;
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

            #region IGravitySourceBasePropertiesChangeListener
            public void OnRangeChanged(float newRange)
            {
                RecalculateBoundsSize();
            }

            public void OnFalloffStartRangeChanged(float newFalloffStartRange)
            {
                // do nothing
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

            private void RecalculateBounds()
            {
                RecalculateBoundsTransform();
                RecalculateBoundsSize();
            }

            private void RecalculateBoundsTransform()
            {
                _bounds.SetTransform(transform.position, transform.rotation);
                transform.hasChanged = false;
            }

            private void RecalculateBoundsSize()
            {
                if (_inverted)
                {
                    _bounds.SetSize(_surfaceDimensions);
                }
                else
                {
                    float doubleRange = _gravitySourceProperties.Range * 2.0f;
                    _bounds.SetSize(new Vector3(_surfaceDimensions.x + doubleRange, _surfaceDimensions.y + doubleRange, _surfaceDimensions.z + doubleRange));
                }
            }
            #endregion
        }

    }
}