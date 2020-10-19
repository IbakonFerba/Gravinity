using UnityEngine;
using FK.Utility;

namespace FK
{
    namespace Gravinity
    {
        /// <summary>
        /// <para>Anything implementing this interface can be a Gravinity Gravity Source.</para>
        /// <para>Any Source needs to register itself in the Gravinity GravityManager</para>
        ///
        /// v1.0 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        public interface IGravitySource
        {
            /// <summary>
            /// Returns the gravity vector of the source at the given position
            /// </summary>
            /// <param name="position"></param>
            /// <returns></returns>
            Vector3 GetGravityAtPosition(Vector3 position);
            /// <summary>
            /// If this is true, no other gravinity sources should be taken into account if this one has a gravity not equal to zero
            /// </summary>
            /// <returns></returns>
            bool ShouldBeExclusive();
            /// <summary>
            /// If this returns false the source is not taken into account
            /// </summary>
            /// <returns></returns>
            bool IsActive();
        }

        /// <summary>
        /// <para>Properties that are most likely shared by all gravity sources.</para>
        /// <para>These already include some utility values like squared ranges for faster distance comparisions that are calculated when the properties are deserialized or whenever SetRange or SetFalloffDistance are called</para>
        ///
        /// v1.0 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        [System.Serializable]
        public class GravitySourceBaseProperties : ISerializationCallbackReceiver
        {
            // ######################## PROPRETIES ######################## //
            #region PROPERTIES 
            /// <summary>
            /// Depending on the actual implementation of the gravity source, this can have different effects. Its effect in the properties is that the falloff start range will equal Range-FalloffDistance if false and Range+FalloffDistance if true
            /// </summary>
            public bool Inverted => _inverted;

            /// <summary>
            /// Total range of the influence of the gravity source
            /// </summary>
            public float Range { get; private set; }
            /// <summary>
            /// Square of Range for faster distance comparisions
            /// </summary>
            public float SqrRange { get; private set; }

            /// <summary>
            /// The falloff distance within the range of the gravity source. This is guaranteed to always be less than or equal Range
            /// </summary>
            public float FalloffDistance { get; private set; }
            /// <summary>
            /// The actuall range where falloff starts. This is determined by the range and the falloff distance
            /// </summary>
            public float FalloffStartRange { get; private set; }
            /// <summary>
            /// Square of FalloffStartRange for faster distance comparisions
            /// </summary>
            public float SqrfalloffStartRange { get; private set; }
            #endregion

            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS
            /// <summary>
            /// If true and the player is inside the influence of this source, all other sources will be ignored.
            /// </summary>
            [Tooltip("If true and the player is inside the influence of this source, all other sources will be ignored.")]
            public bool CapturePlayerExclusive;

            [SerializeField]
            [HideInInspector]
            private bool _canBeInverted;

            [Tooltip("Should the shape of the source be inverted?")]
            [SerializeField]
            private bool _inverted;

            /// <summary>
            /// Strenght of the gravitational pull
            /// </summary>
            [Tooltip("Strenght of the gravitational pull")]
            public float Strength;

            [SerializeField]
            [PositiveValue]
            [Tooltip("Total range of the gravitation field as a radius in Unity Units. Any object outside this range will not be affected by this source\nThis range is represented by the cyan gizmo.")]
            private float _range;

            [SerializeField]
            [PositiveValue]
            [Tooltip("Offset from the total range (towards the inclusion zone of the range) in which the gravity falls off so there can be smooth transitions between multiple sources\nThis range is represented by the blue gizmo.")]
            private float _falloffDistance;
            #endregion


            // ######################## PRIVATE VARS ######################## //
            private bool _initialized;


            // ######################## INITS ######################## //
            #region CONSTRUCTORS
            public GravitySourceBaseProperties()
            {
                _canBeInverted = true;
                CapturePlayerExclusive = false;
                _inverted = false;
                Strength = 9.81f;
                _range = 10.0f;
                _falloffDistance = 1.0f;
                _initialized = false;
            }

            public GravitySourceBaseProperties(bool inCanBeInverted)
            {
                _canBeInverted = inCanBeInverted;
                CapturePlayerExclusive = false;
                _inverted = false;
                Strength = 9.81f;
                _range = 10.0f;
                _falloffDistance = 1.0f;
                _initialized = false;
            }

            public GravitySourceBaseProperties(bool inCapturePlayerExclusive, bool inInverted, float inStrength, float inRange, float inFalloffDistance, bool inCanBeInverted)
            {
                _canBeInverted = inCanBeInverted;
                CapturePlayerExclusive = inCapturePlayerExclusive;
                _inverted = inInverted;
                Strength = inStrength;
                _range = inRange;
                _falloffDistance = inFalloffDistance;
                _initialized = false;
            }
            #endregion


            // ######################## FUNCTIONALITY ######################## //
            #region FUNCTIONALITY
            #region ISerializationCallbackReceiver
            public void OnBeforeSerialize()
            {
                // do nothing
            }

            public void OnAfterDeserialize()
            {
                SetRange(_range, false);
                SetFalloffDistance(_falloffDistance);
                _initialized = true;
            }
            #endregion

            /// <summary>
            /// Sets the range, recalculating all values depending on it (This will call SetFalloffDistance with the current faloff distance to recalculate faloff values)
            /// </summary>
            /// <param name="value">New range value</param>
            public void SetRange(float value)
            {
                SetRange(value, true);
            }

            private void SetRange(float value, bool recalculateFalloffRange)
            {
                Range = value;
                SqrRange = Range * Range;

                if (recalculateFalloffRange)
                    SetFalloffDistance(FalloffDistance);
            }

            /// <summary>
            /// Sets the falloff distance, recalculating all values depending on it
            /// </summary>
            /// <param name="value"></param>
            public void SetFalloffDistance(float value)
            {
                FalloffDistance = value > Range ? Range : value;
                FalloffStartRange = _inverted ? Range + FalloffDistance : Range - FalloffDistance;
                SqrfalloffStartRange = FalloffStartRange * FalloffStartRange;
            }
            #endregion
        }
    }
}