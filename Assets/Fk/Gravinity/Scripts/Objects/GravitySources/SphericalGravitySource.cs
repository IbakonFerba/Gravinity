using UnityEngine;

namespace FK
{
    namespace Gravinity
    {
        /// <summary>
        /// <para>A Spherical Gravity Source that can pull towards its center or push away from its center</para>
        ///
        /// v1.0 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        public class SphericalGravitySource : MonoBehaviour, IGravitySource
        {
            // ######################## PROPRETIES ######################## //
            public GravitySourceBaseProperties Properties => _gravitySourceProperties;


            // ######################## EXPOSED VARS ######################## //
            [SerializeField] private GravitySourceBaseProperties _gravitySourceProperties;


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

#if UNITY_EDITOR
            private void OnDrawGizmos()
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, _gravitySourceProperties.FalloffStartRange);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(transform.position, _gravitySourceProperties.Range);
            }
#endif
            #endregion


            // ######################## FUNCTIONALITY ######################## //
            #region IGravitySource
            public Vector3 GetGravityAtPosition(Vector3 position)
            {
                bool invertedSphere = _gravitySourceProperties.Inverted;
                Vector3 toCenter = transform.position - position;
                Vector3 gravity = invertedSphere ? -toCenter.normalized : toCenter.normalized;
                float toCenterSqrMagnitude = toCenter.sqrMagnitude;

                if ((!invertedSphere && toCenterSqrMagnitude > _gravitySourceProperties.SqrRange) || (invertedSphere && toCenterSqrMagnitude < _gravitySourceProperties.SqrRange))
                    return Vector3.zero;

                if ((!invertedSphere && toCenterSqrMagnitude < _gravitySourceProperties.SqrfalloffStartRange) || (invertedSphere && toCenterSqrMagnitude > _gravitySourceProperties.SqrfalloffStartRange))
                    return gravity * _gravitySourceProperties.Strength;


                float distToEndRange = Mathf.Abs(_gravitySourceProperties.Range - toCenter.magnitude);
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
        }
    }
}