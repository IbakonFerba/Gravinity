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
        public class PlanarGravitySource : MonoBehaviour, IGravitySource
        {
            // ######################## PROPERTIES ######################## //
            public GravitySourceBaseProperties Properties => _gravitySourceProperties;


            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS
            [SerializeField] private GravitySourceBaseProperties _gravitySourceProperties = new GravitySourceBaseProperties(false);

            [Header("Plane Properties")]
            [Tooltip("Extends of the plane")]
            public Vector2 Dimensions = new Vector2(1.0f, 1.0f);
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

                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(Vector3.up * (_gravitySourceProperties.FalloffStartRange * 0.5f), new Vector3(Dimensions.x, _gravitySourceProperties.FalloffStartRange, Dimensions.y));
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(Vector3.up * (_gravitySourceProperties.Range * 0.5f), new Vector3(Dimensions.x, _gravitySourceProperties.Range, Dimensions.y));

                // reset the matrix
                Gizmos.matrix = restoreMatrix;
            }
            #endregion


            // ######################## FUNCTIONALITY ######################## //
            #region IGravitySource
            public Vector3 GetGravityAtPosition(Vector3 position)
            {
                float range = _gravitySourceProperties.Range;
                OBB rangeBounds = new OBB(transform.position + transform.up * range * 0.5f, new Vector3(Dimensions.x, range, Dimensions.y), transform.rotation);
                if (!rangeBounds.Contains(position))
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
        }
    }
}