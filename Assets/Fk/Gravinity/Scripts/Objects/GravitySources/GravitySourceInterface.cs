using UnityEngine;
using FK.Utility;
using System.Collections.Generic;

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
        /// <para>Event listener Interface for GravitySourceBaseProperties</para>
        ///
        /// v1.0 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        public interface IGravitySourceBasePropertiesChangeListener
        {
            void OnRangeChanged(float newRange);
            void OnFalloffStartRangeChanged(float newFalloffStartRange);
        }

        /// <summary>
        /// <para>Properties that are most likely shared by all gravity sources.</para>
        /// <para>These already include some utility values like squared ranges for faster distance comparisions that are calculated when the properties are deserialized or whenever SetRange or SetFalloffDistance are called</para>
        ///
        /// v1.1 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        [System.Serializable]
        public class GravitySourceBaseProperties : ISerializationCallbackReceiver
        {
            // ######################## PROPRETIES ######################## //
            #region PROPERTIES 
            /// <summary>
            /// Total range of the influence of the gravity source
            /// </summary>
            public float Range { get; private set; }

            /// <summary>
            /// The falloff distance within the range of the gravity source. This is guaranteed to always be less than or equal Range
            /// </summary>
            public float FalloffDistance { get; private set; }
            /// <summary>
            /// The actuall range where falloff starts. This is determined by the range and the falloff distance
            /// </summary>
            public float FalloffStartRange { get; private set; }
            #endregion

            // ######################## EXPOSED VARS ######################## //
            #region EXPOSED VARS
            /// <summary>
            /// If true and the player is inside the influence of this source, all other sources will be ignored.
            /// </summary>
            [Tooltip("If true and the player is inside the influence of this source, all other sources will be ignored.")]
            public bool CapturePlayerExclusive;

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
            private HashSet<IGravitySourceBasePropertiesChangeListener> _eventListeners;


            // ######################## INITS ######################## //
            #region CONSTRUCTORS
            public GravitySourceBaseProperties()
            {
                CapturePlayerExclusive = false;
                Strength = 9.81f;
                _range = 10.0f;
                _falloffDistance = 1.0f;
                _eventListeners = new HashSet<IGravitySourceBasePropertiesChangeListener>();
            }

            public GravitySourceBaseProperties(bool inCapturePlayerExclusive, bool inInverted, float inStrength, float inRange, float inFalloffDistance)
            {
                CapturePlayerExclusive = inCapturePlayerExclusive;
                Strength = inStrength;
                _range = inRange;
                _falloffDistance = inFalloffDistance;
                _eventListeners = new HashSet<IGravitySourceBasePropertiesChangeListener>();
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
                Range = _range;
                SetFalloffDistance_Internal(_falloffDistance, false);

                foreach (IGravitySourceBasePropertiesChangeListener listener in _eventListeners)
                {
                    listener.OnRangeChanged(Range);
                    listener.OnFalloffStartRangeChanged(FalloffStartRange);
                }
            }
            #endregion

            public void RegisterListener(IGravitySourceBasePropertiesChangeListener listener)
            {
                _eventListeners.Add(listener);
            }

            public void UnregisterListener(IGravitySourceBasePropertiesChangeListener listener)
            {
                _eventListeners.Remove(listener);
            }

            /// <summary>
            /// Sets the range, recalculating all values depending on it (This will call SetFalloffDistance with the current faloff distance to recalculate faloff values)
            /// </summary>
            /// <param name="value">New range value</param>
            public void SetRange(float value)
            {
#if UNITY_EDITOR
                _range = value;
#endif
                Range = value;
                SetFalloffDistance_Internal(FalloffDistance, false);

                foreach (IGravitySourceBasePropertiesChangeListener listener in _eventListeners)
                {
                    listener.OnRangeChanged(Range);
                    listener.OnFalloffStartRangeChanged(FalloffStartRange);
                }
            }

            /// <summary>
            /// Sets the falloff distance, recalculating all values depending on it
            /// </summary>
            /// <param name="value"></param>
            public void SetFalloffDistance(float value)
            {
                SetFalloffDistance_Internal(value, true);
            }

            private void SetFalloffDistance_Internal(float value, bool sendEvent)
            {
                FalloffDistance = value > Range ? Range : value;
#if UNITY_EDITOR
                _falloffDistance = FalloffDistance;
#endif
                FalloffStartRange = Range - FalloffDistance;

                if (!sendEvent)
                    return;

                foreach (IGravitySourceBasePropertiesChangeListener listener in _eventListeners)
                {
                    listener.OnFalloffStartRangeChanged(FalloffStartRange);
                }
            }
            #endregion
        }
    }
}