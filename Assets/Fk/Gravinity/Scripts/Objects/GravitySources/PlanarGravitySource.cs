using UnityEngine;
using FK.Utility;

namespace FK
{
    namespace Gravinity
    {
        /// <summary>
        /// <para>A planar gravity source that pulls anything above it towards its surface</para>
        ///
        /// v1.0 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        public class PlanarGravitySource
            : MonoBehaviour
            , IGravitySource
            , IGravitySourceBasePropertiesChangeListener
        {
            // ######################## PROPERTIES ######################## //
            #region PROPERTIES
            public GravitySourceBaseProperties Properties => _gravitySourceProperties;

            /// <summary>
            /// Extends of the plane. Setting this causes recalculations of the bounds of the gravity source
            /// </summary>
            public Vector2 Dimensions
            {
                get => _dimensions;
                set
                {
                    _dimensions = value;
                    RecalculateBounds();
                }
            }
            #endregion


            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS
            [SerializeField] private GravitySourceBaseProperties _gravitySourceProperties;

            [Header("Plane Properties")]
            [SerializeField]
            [Tooltip("Extends of the plane")]
            private Vector2 _dimensions = new Vector2(1.0f, 1.0f);
            #endregion


            // ######################## PRIVATE VARS ######################## //
            private OBB _bounds;


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

            private void OnDrawGizmos()
            {
                // use position and rotation of this object for the transform matrix of the gizmos, but not the scale
                Matrix4x4 restoreMatrix = Gizmos.matrix;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(Vector3.up * (_gravitySourceProperties.FalloffStartRange * 0.5f), new Vector3(_dimensions.x, _gravitySourceProperties.FalloffStartRange, _dimensions.y));
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(Vector3.up * (_gravitySourceProperties.Range * 0.5f), new Vector3(_dimensions.x, _gravitySourceProperties.Range, _dimensions.y));

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
                float heightOverSurface = localPosition.y;

                float falloffStartRange = _gravitySourceProperties.FalloffStartRange;
                if (heightOverSurface < falloffStartRange) // full gravity if below the falloff start range
                    return -transform.up * _gravitySourceProperties.Strength;

                float normalizedHeightInFalloffZone = (heightOverSurface - falloffStartRange) / _gravitySourceProperties.FalloffDistance;
                return -transform.up * (_gravitySourceProperties.Strength * normalizedHeightInFalloffZone);
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
                RecalculateBounds();
            }

            public void OnFalloffStartRangeChanged(float newFalloffStartRange)
            {
                // do nothing
            }
            #endregion

            private void RecalculateBounds()
            {
                RecalculateBoundsTransform();
                _bounds.SetSize(new Vector3(_dimensions.x, _gravitySourceProperties.Range, _dimensions.y));
            }

            private void RecalculateBoundsTransform()
            {
                _bounds.SetTransform(transform.position + transform.up * _gravitySourceProperties.Range * 0.5f, transform.rotation);
                transform.hasChanged = false;
            }

            #endregion
        }
    }
}