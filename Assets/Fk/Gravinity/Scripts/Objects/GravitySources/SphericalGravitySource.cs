using UnityEngine;

namespace FK
{
    namespace Gravinity
    {
        /// <summary>
        /// <para>A Spherical Gravity Source that can pull towards its center or push away from its center</para>
        ///
        /// v1.1 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        public class SphericalGravitySource
            : MonoBehaviour
            , IGravitySource
            , IGravitySourceBasePropertiesChangeListener
        {
            // ######################## PROPRETIES ######################## //
            #region PROPERTIES
            public GravitySourceBaseProperties Properties => _gravitySourceProperties;
            public bool Inverted => _inverted;

            /// <summary>
            /// Radius of the surface if the gravity source is an inverted sphere. Beyond this, the source has no influence
            /// Note that setting this causes recalculation of a few values if the source is an inverted sphere
            /// </summary>
            public float InvertedSphereSurfaceRadius
            {
                get => Inverted ? _invertedSphereSurfaceRadius : -1.0f;
                set
                {
                    if (Inverted)
                        return;

                    _invertedSphereSurfaceRadius = value;
                    UpdateSurfaceRadiusValues();
                }
            }
            #endregion


            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS
            [SerializeField] private GravitySourceBaseProperties _gravitySourceProperties;

            [Header("Sphere Properties")]
            [SerializeField]
            [Tooltip("If true, the source is an inverted sphere which has a gravitational pull away from its center")]
            public bool _inverted = false;
            [SerializeField]
            [Tooltip("Radius of the surface of the inverted sphere. Beyond this, the source has no influence")]
            private float _invertedSphereSurfaceRadius = 1.0f;
            #endregion


            // ######################## PRIVATE VARS ######################## //
            #region PRIVATE VARS
            private float _sqrInvertedSphereSurfaceRadius;
            private float _rangeRadius;
            private float _sqrRangeRadius;
            private float _falloffStartRangeRadius;
            private float _sqrFalloffStartRangeRadius;
            #endregion


            // ######################## CONSTRUCTORS/DESTRUCTORS ######################## //
            public SphericalGravitySource()
            {
                _gravitySourceProperties = new GravitySourceBaseProperties();
                _gravitySourceProperties.RegisterListener(this);
            }

            ~SphericalGravitySource()
            {
                _gravitySourceProperties.UnregisterListener(this);
            }


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

            private void OnValidate()
            {
                if (Inverted)
                {
                    UpdateSurfaceRadiusValues();
                }

                OnRangeChanged(_gravitySourceProperties.Range);
                OnFalloffStartRangeChanged(_gravitySourceProperties.FalloffStartRange);
            }

#if UNITY_EDITOR
            private void OnDrawGizmos()
            {
                if (Inverted)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(transform.position, _invertedSphereSurfaceRadius);
                }
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, _falloffStartRangeRadius);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, _rangeRadius);
            }
#endif
            #endregion


            // ######################## FUNCTIONALITY ######################## //
            #region FUNCTIONALITY
            #region IGravitySource
            public Vector3 GetGravityAtPosition(Vector3 position)
            {
                Vector3 toCenter = transform.position - position;
                float toCenterSqrMagnitude = toCenter.sqrMagnitude;

                if (Inverted && toCenterSqrMagnitude > _sqrInvertedSphereSurfaceRadius)
                    return Vector3.zero;

                if ((!Inverted && toCenterSqrMagnitude > _sqrRangeRadius) || (Inverted && toCenterSqrMagnitude < _sqrRangeRadius))
                    return Vector3.zero;

                Vector3 gravity = Inverted ? -toCenter.normalized : toCenter.normalized;
                if ((!Inverted && toCenterSqrMagnitude < _sqrFalloffStartRangeRadius) || (Inverted && toCenterSqrMagnitude > _sqrFalloffStartRangeRadius))
                    return gravity * _gravitySourceProperties.Strength;


                float distToEndRange = Mathf.Abs(_rangeRadius - toCenter.magnitude);
                float normalizeddistToEndRange = distToEndRange / _gravitySourceProperties.FalloffDistance;
                float interpolatedStrength = Mathf.Lerp(0.0f, _gravitySourceProperties.Strength, normalizeddistToEndRange);
                return gravity * interpolatedStrength;
            }

            public bool ShouldBeExclusive()
            {
                return _gravitySourceProperties.CapturePlayerExclusive;
            }

            public bool IsActive()
            {
                return gameObject.activeInHierarchy;
            }
            #endregion

            #region IGravitySourceBasePropertiesChangeListener
            public void OnRangeChanged(float newRange)
            {
                _rangeRadius = Inverted ? _invertedSphereSurfaceRadius - newRange : newRange;
                _sqrRangeRadius = _rangeRadius * _rangeRadius;
            }

            public void OnFalloffStartRangeChanged(float newFalloffStartRange)
            {
                _falloffStartRangeRadius = Inverted ? _invertedSphereSurfaceRadius - newFalloffStartRange : newFalloffStartRange;
                _sqrFalloffStartRangeRadius = _falloffStartRangeRadius * _falloffStartRangeRadius;
            }
            #endregion

            private void UpdateSurfaceRadiusValues()
            {
                _sqrInvertedSphereSurfaceRadius = _invertedSphereSurfaceRadius * _invertedSphereSurfaceRadius;

                if (_invertedSphereSurfaceRadius - _gravitySourceProperties.Range < 0.0f)
                    _gravitySourceProperties.SetRange(_invertedSphereSurfaceRadius);
            }
            #endregion
        }
    }
}