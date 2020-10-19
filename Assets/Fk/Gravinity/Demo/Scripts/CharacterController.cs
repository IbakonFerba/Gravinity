using UnityEngine;
using FK.Utility;

namespace FK
{
    namespace Gravinity
    {
        /// <summary>
        /// <para>A very simple Character Controller using Gravinity gravity</para>
        ///
        /// v1.0 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        [RequireComponent(typeof(Rigidbody))]
        public class CharacterController : MonoBehaviour
        {
            // ######################## PROPERTIES ######################## //
            #region PROPERTIES
            public bool IsGrounded { get; private set; }

            private Vector3 _worldSpaceFeetPosition => transform.TransformPoint(_feetOffset);
            #endregion


            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS

            [SerializeField]
            [PositiveValue]
            [Tooltip("Max speed the player can move with")]
            public float Speed = 2.0f;
            [SerializeField]
            [PositiveValue]
            [Tooltip("Force that is applied to the player when it gets a jump input")]
            public float JumpForce = 2.0f;
            [SerializeField]
            [Tooltip("Mutliplier for the horizontal mouse delta input used to calculate the rotation of the player")]
            private float _mouseSensitivity = 1.0f;

            [Header("Ground Detection")]
            [SerializeField]
            [Tooltip("The feet offset defines the position at which ground detection is performed. It is an offset from the GameObject pivot")]
            private Vector3 _feetOffset = new Vector3(0.0f, -1.0f, 0.0f);
            [SerializeField]
            [PositiveValue]
            [Tooltip("Radius of the sphere around the feet position that is used for ground detection")]
            private float _groundDetectionRadius = 0.1f;
            [SerializeField]
            [Tooltip("These layers will be ignored by the ground detection. At the very least you should ignore the layer your player is on")]
            private LayerMask _ignoreLayers = 0;

            [Header("Visuals")]
            [SerializeField]
            [Tooltip("Transform of the visuals of your player. These should be on another GameObject as their transform will get a smoothing applied when gravity changes apruptly")]
            private Transform _visualsTransform;
            [SerializeField]
            [Range(0.0f, 1.0f)]
            [Tooltip("Smoothing that is applied to the up vector of the Visuals Transform to animate the transition between very different gravity directions")]
            private float _upVectorSmoothing = 0.8f;
            #endregion


            // ######################## PRIVATE VARS ######################## //
            #region PRIVATE VARS
            private Rigidbody _rigidbody;

            private Vector3 _prevUp;
            private Vector2 _playerInput;
            private float _rotationInput;
            private bool _jump;
            #endregion


            // ######################## UNITY EVENT FUNCTIONS ######################## //
            #region UNITY EVENT FUNCTIONS
            private void Start()
            {
                _rigidbody = GetComponent<Rigidbody>();
                _rigidbody.useGravity = false;

                Cursor.lockState = CursorLockMode.Locked;
            }

            private void Update()
            {
                IsGrounded = Physics.CheckSphere(_worldSpaceFeetPosition, _groundDetectionRadius, ~_ignoreLayers);
                GetInput();

                Vector3 normalizedGravity = GravityManager.CalculateGravityAtPosition(transform.position).normalized;
                ApplyRotation(normalizedGravity);
            }

            private void FixedUpdate()
            {
                Move();
            }

            private void OnDrawGizmosSelected()
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(_worldSpaceFeetPosition, _groundDetectionRadius);
            }
            #endregion


            // ######################## FUNCTIONALITY ######################## //
            #region FUNCTIONALITY
            private void ApplyRotation(Vector3 normalizedGravity)
            {
                Transform trans = transform;

                Vector3 up = normalizedGravity.sqrMagnitude > float.Epsilon ? -normalizedGravity : trans.up;
                Vector3 smoothedUp = Vector3.Slerp(_prevUp, up, 1.0f - _upVectorSmoothing);

                Quaternion newRotation = Quaternion.AngleAxis(_rotationInput, up) * Quaternion.FromToRotation(trans.up, up) * trans.rotation;
                Quaternion newModelRotation = Quaternion.AngleAxis(_rotationInput, smoothedUp) * Quaternion.FromToRotation(trans.up, smoothedUp) * trans.rotation;

                trans.rotation = newRotation;
                _visualsTransform.rotation = newModelRotation;

                _prevUp = smoothedUp;
            }

            private void Move()
            {
                Transform trans = transform;

                // this is the y component of the velocity in local space
                float localVerticalVelocity = (Quaternion.Inverse(trans.rotation) * _rigidbody.velocity).y;

                if (_jump)
                {
                    localVerticalVelocity += JumpForce;
                    _jump = false;
                }

                Vector3 input = _playerInput * Speed;
                Vector3 orientedInputVelocity = trans.rotation * new Vector3(input.x, localVerticalVelocity, input.y);

                Vector3 gravity = GravityManager.CalculateGravityAtPosition(trans.position);
                _rigidbody.velocity = orientedInputVelocity + gravity * Time.fixedDeltaTime;
            }

            private void GetInput()
            {
                _playerInput = Vector2.ClampMagnitude(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")), 1.0f);

                _rotationInput = Input.GetAxis("Mouse X") * _mouseSensitivity;
                _jump |= IsGrounded && Input.GetButtonDown("Jump");
            }
            #endregion
        }

    }
}