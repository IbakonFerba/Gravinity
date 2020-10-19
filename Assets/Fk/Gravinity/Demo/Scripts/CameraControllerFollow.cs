using UnityEngine;

namespace FK
{
    namespace Gravinity
    {
        /// <summary>
        /// <para>A simple follow camera that can deal with the changing up/down directions of a gravinity scene</para>
        ///
        /// v1.0 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        public class CameraControllerFollow : MonoBehaviour
        {
            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS
            [SerializeField]
            [Tooltip("Transform to follow")]
            public Transform FollowTarget;
            /// <summary>
            /// Offset of the follow target from the pivot of the Follow Target GameObject in its local space
            /// </summary>
            [SerializeField]
            [Tooltip("Offset of the follow target from the pivot of the Follow Target GameObject in its local space")]
            public Vector3 LocalTargetOffset;
            [SerializeField]
            [Tooltip("Desired distance from the target the camera attempts to keep")]
            private float FollowDistance = 10.0f;

            [Header("Pitch")]
            [SerializeField]
            [Tooltip("Mutliplier for the vertical mouse delta input used to calculate the pitch angle")]
            private float _pitchMouseSensitivity = 3.0f;
            [SerializeField]
            [Tooltip("Smallest allowed pitch angle (-90 would be perfectly top down)")]
            private float _pitchMin = -80.0f;
            [SerializeField]
            [Tooltip("Biggest allowed pitch angle (90 would be perfectly bottom up)")]
            private float _pitchMax = 50.0f;
            [SerializeField]
            [Tooltip("Pitch at startup")]
            private float _startPitch = 0.0f;

            [Header("Smoothing")]
            [SerializeField]
            [Tooltip("Smoothing time used to smooth the position of the camera")]
            private float _smoothingTime = 0.2f;
            [SerializeField]
            [Range(0.0f, 1.0f)]
            [Tooltip("Smoothing factor used to smooth the roll component of the rotation of the camera")]
            private float _rollSmoothingFactor = 0.75f;

            [Header("Obstacle avoidance")]
            [SerializeField]
            [Tooltip("Radius of the sphere that is used for the Spherecast determining whether there is an obstacle between the follow target and the camera")]
            private float _cameraCollisionRadius = 0.5f;
            [SerializeField]
            [Tooltip("These layers are ignored by the obstacle detection")]
            private LayerMask _ignoreLayers = 0;
            #endregion

            // ######################## PRIVATE VARS ######################## //
            #region PRIVATE VARS
            private float _pitch = 0.0f;

            private Vector3 _positionSmoothDampVelocity = Vector3.zero;
            #endregion


            // ######################## UNITY EVENT FUNCTIONS ######################## //
            #region UNITY EVENT FUNCTIONS
            private void Awake()
            {
                SnapToDesiredPosition(_startPitch);
            }

            private void Update()
            {
                float mouseYDelta = Input.GetAxis("Mouse Y") * _pitchMouseSensitivity;
                _pitch = Mathf.Clamp(_pitch + mouseYDelta, _pitchMin, _pitchMax);
            }

            private void FixedUpdate()
            {
                if (!FollowTarget)
                {
                    Debug.LogError("No Follow Target set!", this);
                    return;
                }

                Vector3 followPosition = CalculateFollowPosition();
                Vector3 desiredPosition = CalculateDesiredPosition(followPosition);
                Vector3 followPosToDesiredPos = desiredPosition - followPosition;

                // make sure nothing is between the target and the camera, move the camera closer if that is the case
                RaycastHit hitInfo;
                if (Physics.SphereCast(followPosition, _cameraCollisionRadius, followPosToDesiredPos, out hitInfo, followPosToDesiredPos.magnitude, ~_ignoreLayers.value))
                {
                    Vector3 followPosToHit = hitInfo.point - followPosition;
                    // technically, this does not provide the most accurate position, as the vector to the hit point is not nessecarily aligned with the vector to the camera, but for demo purposes this is good enough
                    desiredPosition = followPosition + followPosToDesiredPos.normalized * followPosToHit.magnitude;
                }

                Transform trans = transform;
                trans.position = Vector3.SmoothDamp(trans.position, desiredPosition, ref _positionSmoothDampVelocity, _smoothingTime);

                // for the rotation, we only want to interpolate the roll rotation of the camera (for a nicer effect when the direction of gravity shifts a lot). 
                // To do this we need to transform the desired rotation into the local space of the camera to get a relative rotation of which we then can interpolate the z angle
                Quaternion desiredRotation = CalculateDesiredRotation(followPosition);
                Quaternion localDesiredRotation = Quaternion.Inverse(trans.rotation) * desiredRotation;
                float smoothedRelativeRollAngle = Mathf.LerpAngle(0, localDesiredRotation.eulerAngles.z, 1.0f - _rollSmoothingFactor); // as we are in a relative rotation here, 0 means the z angle the transform currently has
                trans.rotation *= Quaternion.Euler(localDesiredRotation.eulerAngles.x, localDesiredRotation.eulerAngles.y, smoothedRelativeRollAngle);
            }

            private void OnDrawGizmosSelected()
            {
                Transform trans = transform;
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(trans.position, _cameraCollisionRadius);

                if (FollowTarget)
                {
                    Vector3 followPosition = CalculateFollowPosition();
                    Gizmos.color = new Color(0.0f, 0.5f, 0.0f);
                    Gizmos.DrawLine(trans.position, followPosition);

                    Gizmos.DrawSphere(followPosition, 0.1f);
                }
            }

            #endregion

            // ######################## FUNCTIONALITY ######################## //
            #region FUNCTIONALITY
            /// <summary>
            /// Sets the camera to its desired position and rotation instantly without smoothing
            /// </summary>
            /// <param name="pitch"></param>
            public void SnapToDesiredPosition(float pitch)
            {
                if (!FollowTarget)
                {
                    Debug.LogError("Could not reset the camera, no Follow Target set!", this);
                    return;
                }

                _pitch = Mathf.Clamp(pitch, _pitchMin, _pitchMax);

                Vector3 followPosition = CalculateFollowPosition();

                Transform trans = transform;
                trans.position = CalculateDesiredPosition(followPosition);
                trans.rotation = CalculateDesiredRotation(followPosition);
            }

            public Vector3 CalculateFollowPosition()
            {
                if (!FollowTarget)
                {
                    Debug.LogError("Could not calculate follow position, no Follow Target set!", this);
                    return Vector3.zero;
                }

                return FollowTarget.position + FollowTarget.rotation * LocalTargetOffset;
            }

            public Quaternion CalculateDesiredRotation(Vector3 followPosition)
            {
                if (!FollowTarget)
                {
                    Debug.LogError("Could not calculate desired rotation, no Follow Target set!", this);
                    return Quaternion.identity;
                }

                return Quaternion.LookRotation(followPosition - transform.position, FollowTarget.up);
            }

            private Vector3 CalculateDesiredPosition(Vector3 followPosition)
            {
                Quaternion orbitRotation = FollowTarget.rotation * Quaternion.AngleAxis(_pitch, Vector3.left);
                return followPosition - (orbitRotation * Vector3.forward) * FollowDistance;
            }
            #endregion
        }
    }
}