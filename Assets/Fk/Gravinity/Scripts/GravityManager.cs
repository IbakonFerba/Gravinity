using System.Collections.Generic;
using UnityEngine;

namespace FK
{
    namespace Gravinity
    {
        /// <summary>
        /// <para>Central Manager of the Gravinity tool. Use this to get gravinity source dependant gravity.</para>
        ///
        /// v1.0 10/2020
        /// Written by Fabian Kober
        /// fabian-kober@gmx.net
        /// </summary>
        public static class GravityManager
        {
            // ######################## STRUCTS & CLASSES ######################## //

            #region STRUCTS & CLASSES

            #endregion


            // ######################## ENUMS & DELEGATES ######################## //

            #region ENUMS & DELEGATES

            #endregion


            // ######################## EVENTS ######################## //

            #region EVENTS

            #endregion


            // ######################## PROPERTIES ######################## //

            #region PROPERTIES

            #endregion


            // ######################## EXPOSED VARS ######################## //

            #region EXPOSED VARS

            #endregion


            // ######################## PUBLIC VARS ######################## //

            #region PUBLIC VARS

            #endregion


            // ######################## PROTECTED VARS ######################## //

            #region PROTECTED VARS

            #endregion


            // ######################## PRIVATE VARS ######################## //

            #region PRIVATE VARS

            private static readonly List<IGravitySource> _gravitySources = new List<IGravitySource>();
            #endregion


            // ######################## INITS ######################## //

            #region CONSTRUCTORS

            #endregion

            #region INITS

            ///<summary>
            /// Does the Init for this Behaviour
            ///</summary>
            private static void Init()
            {

            }

            #endregion


            // ######################## UNITY EVENT FUNCTIONS ######################## //

            #region UNITY EVENT FUNCTIONS

            #endregion


            // ######################## FUNCTIONALITY ######################## //

            #region FUNCTIONALITY
            /// <summary>
            /// Calculates the gravity vector at the given point, taking all active gravinity sources into account
            /// </summary>
            /// <param name="position"></param>
            /// <returns></returns>
            public static Vector3 CalculateGravityAtPosition(Vector3 position)
            {
                Vector3 cumulativeGravity = Vector3.zero;

                foreach (IGravitySource gravitySource in _gravitySources)
                {
                    if (!gravitySource.IsActive())
                        continue;

                    Vector3 sourceGravity = gravitySource.GetGravityAtPosition(position);
                    if(gravitySource.ShouldBeExclusive() && sourceGravity != Vector3.zero)
                    {
                        cumulativeGravity = sourceGravity;
                        break;
                    }
                    cumulativeGravity += sourceGravity;
                }

                return cumulativeGravity;
            }

            public static void RegisterGravitySource(IGravitySource gravitySource)
            {
                _gravitySources.Add(gravitySource);
            }

            public static void UnregisterGravitySource(IGravitySource gravitySource)
            {
                if (!_gravitySources.Contains(gravitySource))
                    return;

                _gravitySources.Remove(gravitySource);
            }

            #endregion


            // ######################## COROUTINES ######################## //

            #region COROUTINES 

            #endregion


            // ######################## UTILITIES ######################## //

            #region UTILITIES

            #endregion
        }
    }
}
