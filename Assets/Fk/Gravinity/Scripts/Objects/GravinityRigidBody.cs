using UnityEngine;

/// <summary>
/// <para>Rigidbody extension for objects that should use gravinity gravity instead of normal Unity gravity.</para>
/// <para>This disables gravity on the rigidbody component and replaces it with a custom gravity calculation</para>
///
/// v1.0 10/2020
/// Written by Fabian Kober
/// fabian-kober@gmx.net
/// </summary>
namespace FK
{
	namespace Gravinity
	{
		[RequireComponent(typeof(Rigidbody))]
		public class GravinityRigidBody : MonoBehaviour
        { 
			// ######################## PRIVATE VARS ######################## //
			#region PRIVATE VARS
			private Rigidbody _rigidbody;

			private float _sleepThreshold;
			private bool _useGravityInternal;
			private bool _rbWasAwake = false;
			#endregion
	
	
			// ######################## UNITY EVENT FUNCTIONS ######################## //
			#region UNITY EVENT FUNCTIONS
			private void Awake () {
                _rigidbody = GetComponent<Rigidbody>();
                _useGravityInternal = _rigidbody.useGravity;
                _sleepThreshold = _rigidbody.sleepThreshold;

                _rigidbody.useGravity = false;
            }
		
			private void FixedUpdate ()
			{
				bool rbIsAwake = !_rigidbody.IsSleeping();
				bool awake = _rigidbody.velocity.magnitude > _sleepThreshold || (!_rbWasAwake && rbIsAwake);
				if (_useGravityInternal && awake)
				{
					_rigidbody.AddForce(GravityManager.CalculateGravityAtPosition(transform.position), ForceMode.Acceleration);
				}

				_rbWasAwake = rbIsAwake;
			}
            #endregion
        }
	}
}
