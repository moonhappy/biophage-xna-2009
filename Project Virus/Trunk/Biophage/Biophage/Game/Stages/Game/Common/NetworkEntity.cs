using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA;
using LNA.GameEngine;
using LNA.GameEngine.Core;
using LNA.GameEngine.Core.AsyncTasks;
using LNA.GameEngine.Objects;
using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.GameObjects.Assets;
using LNA.GameEngine.Objects.GameObjects.Sprites;
using LNA.GameEngine.Objects.UI;
using LNA.GameEngine.Objects.UI.Menu;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;
using LNA.GameEngine.Resources.Drawable;
using LNA.GameEngine.Resources.Playable;

using JigLibX.Math;
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;

namespace Biophage.Game.Stages.Game.Common
{
    /// <summary>
    /// This struct helps guide the server/client prediction/correction
    /// routines.
    /// </summary>
    public struct NetworkData
    {
        #region session details

        public SessionDetails sessionDetails;

        #endregion

        #region time values

        public TimeSpan timeOfLastCorrection;
        public TimeSpan timeOfLastPrediction;

        #endregion

        #region initial values

        public Microsoft.Xna.Framework.Vector3      initPosition;
        public Microsoft.Xna.Framework.Quaternion   initOrientationQuat;

        #endregion

        #region true values

        public float                                truePrevTime;
        public Microsoft.Xna.Framework.Vector3      truePrevPosition;
        public Microsoft.Xna.Framework.Quaternion   truePrevOrientation;

        public float                                trueLatestTime;
        public Microsoft.Xna.Framework.Vector3      trueLatestPosition;
        public Microsoft.Xna.Framework.Quaternion   trueLatestOrientation;

        #endregion

        #region estimated values

        #region position

        public Microsoft.Xna.Framework.Vector3  estTrueVelocity;
        public Microsoft.Xna.Framework.Vector3  estNextTruePosition;

        public Microsoft.Xna.Framework.Vector3  estVelocity;
        public Microsoft.Xna.Framework.Vector3  estPosition;

        #endregion

        #region orientations

        //taking a guess here
        public Microsoft.Xna.Framework.Quaternion estTrueAngularVelocity;
        public Microsoft.Xna.Framework.Quaternion estNextTrueOrientation;

        public Microsoft.Xna.Framework.Quaternion estOrientation;

        #endregion

        #endregion

        #region transforms

        public Microsoft.Xna.Framework.Matrix transModelTrans;
        public Microsoft.Xna.Framework.Matrix transWorldMat;

        #endregion
    }

    /// <summary>
    /// To help enforce method decalartions.
    /// </summary>
    public abstract class NetworkEntity : ColliableGObj
    {
        #region fields

        protected bool          isSessionDetailsSet = false;
        protected NetworkData   networkData;

        protected Primitive             m_physPrimitive;
        protected MaterialProperties    m_physMaterial;
        protected float                 m_physMass;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Don't forget to add skin primitives.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="debugMgr"></param>
        /// <param name="resourceMgr"></param>
        /// <param name="parentScn"></param>
        /// <param name="addToScene"></param>
        public NetworkEntity(   uint id,
                                DebugManager debugMgr, ResourceManager resourceMgr,
                                Scene parentScn, bool addToScene,
                                Primitive physPrimitive, MaterialProperties physMaterial,
                                float physMass,
                                Microsoft.Xna.Framework.Vector3 initPosition,
                                Microsoft.Xna.Framework.Quaternion initOrientation)
            : base(id, debugMgr, resourceMgr, parentScn, addToScene)
        {
            //set fields
            networkData = new NetworkData();
            PositionSimple = networkData.initPosition = initPosition;
            Orientation = networkData.initOrientationQuat = initOrientation;
            m_physPrimitive = physPrimitive;
            m_physMaterial = physMaterial;
            m_physMass = physMass;

            //asserts
            m_debugMgr.Assert(physPrimitive != null,
                "NetworkEntity:Constructor - 'physPrimitive' is null.");
        }

        #endregion

        #region field accessors

        public bool IsSessionDetailsSet
        {
            get { return isSessionDetailsSet; }
        }

        public SessionDetails GetSessionDetails
        {
            get { return networkData.sessionDetails; }
        }

        #region initials

        public Microsoft.Xna.Framework.Vector3 InitialPosition
        {
            get { return networkData.initPosition; }
            set { networkData.initPosition = value; }
        }

        public Microsoft.Xna.Framework.Quaternion InitialOrientation
        {
            get { return networkData.initOrientationQuat; }
            set { networkData.initOrientationQuat = value; }
        }

        #endregion

        #region transforms

        /// <summary>
        /// Only changes the position field - doesn't update matricies
        /// </summary>
        public Microsoft.Xna.Framework.Vector3 PositionSimple
        {
            get
            {
                if (isSessionDetailsSet)
                {
                    if (networkData.sessionDetails.isHost)
                    {
                        //server returns the true position - physics aligned first
                        return networkData.trueLatestPosition;
                    }
                    else
                        //client returns estimated position
                        return networkData.estPosition;
                }
                else
                {
                    return networkData.trueLatestPosition;
                }
            }
            set
            {
                if (isSessionDetailsSet)
                {
                    if (networkData.sessionDetails.isHost)
                        //server
                        networkData.trueLatestPosition = value;
                    else
                        //client
                        networkData.estPosition = value;
                }
                else
                {
                    networkData.trueLatestPosition = value;
                    networkData.estPosition = value;
                }
            }
        }

        public Microsoft.Xna.Framework.Vector3 Position
        {
            get { return PositionSimple; }
            set
            {
                PositionSimple = value;

                if (isSessionDetailsSet)
                {
                    if (networkData.sessionDetails.isHost)
                        //server
                        networkData.transWorldMat =
                            networkData.transModelTrans *
                            Microsoft.Xna.Framework.Matrix.CreateTranslation(
                                networkData.trueLatestPosition);
                    else
                        //client
                        networkData.transWorldMat =
                            networkData.transModelTrans *
                            Microsoft.Xna.Framework.Matrix.CreateTranslation(
                                networkData.estPosition);
                }
                else
                {
                    //either
                    networkData.transWorldMat =
                            networkData.transModelTrans *
                            Microsoft.Xna.Framework.Matrix.CreateTranslation(
                                networkData.trueLatestPosition);
                }

                //set physics properties
                if (m_physBody != null)
                {
                    if (m_physBody.Immovable)
                        m_physBody.MoveTo(PositionSimple, ModelTransformSimple);
                }
            }
        }


        /// <summary>
        /// Only changes the orientation field - doesn't update matricies
        /// </summary>
        public Microsoft.Xna.Framework.Quaternion OrientationSimple
        {
            get
            {
                if (isSessionDetailsSet)
                {
                    if (networkData.sessionDetails.isHost)
                        //server returns the true orientation
                        return networkData.trueLatestOrientation;
                    else
                        //client returns estimated orientation
                        return networkData.estOrientation;
                }
                else
                {
                    return networkData.trueLatestOrientation;
                }
            }
            set
            {
                if (isSessionDetailsSet)
                {
                    if (networkData.sessionDetails.isHost)
                        //server
                        networkData.trueLatestOrientation = value;
                    else
                        //client
                        networkData.estOrientation = value;
                }
                else
                {
                    networkData.trueLatestOrientation = value;
                    networkData.estOrientation = value;
                }
            }
        }

        public Microsoft.Xna.Framework.Quaternion Orientation
        {
            get { return OrientationSimple; }
            set
            {
                OrientationSimple = value;

                if (isSessionDetailsSet)
                {
                    if (networkData.sessionDetails.isHost)
                    {
                        //server
                        //update world transform - don't leave out local transforms!
                        networkData.transModelTrans =
                            Microsoft.Xna.Framework.Matrix.CreateFromQuaternion(
                                networkData.trueLatestOrientation);

                        networkData.transWorldMat =
                            networkData.transModelTrans *
                            Microsoft.Xna.Framework.Matrix.CreateTranslation(
                                networkData.trueLatestPosition);
                    }
                    else
                    {
                        //client
                        //update world transform - don't leave out local transforms!
                        networkData.transModelTrans =
                            Microsoft.Xna.Framework.Matrix.CreateFromQuaternion(
                                networkData.estOrientation);

                        networkData.transWorldMat =
                            networkData.transModelTrans *
                            Microsoft.Xna.Framework.Matrix.CreateTranslation(
                                networkData.estPosition);
                    }
                }
                else
                {
                    //either
                    //update world transform - don't leave out local transforms!
                    networkData.transModelTrans =
                        Microsoft.Xna.Framework.Matrix.CreateFromQuaternion(
                            networkData.trueLatestOrientation);

                    networkData.transWorldMat =
                        networkData.transModelTrans *
                        Microsoft.Xna.Framework.Matrix.CreateTranslation(
                            networkData.trueLatestPosition);
                }

                //set physics properties
                if ((m_physBody != null) && (!networkData.sessionDetails.isHost))
                    m_physBody.MoveTo(PositionSimple, ModelTransformSimple);
            }
        }


        /// <summary>
        /// Only changes the model transform matrix - doesn't update world transform matrix.
        /// </summary>
        public Microsoft.Xna.Framework.Matrix ModelTransformSimple
        {
            get { return networkData.transModelTrans; }
            set { networkData.transModelTrans = value; }
        }

        public Microsoft.Xna.Framework.Matrix ModelTransform
        {
            get { return networkData.transModelTrans; }
            set
            {
                //these will be discarded
                Microsoft.Xna.Framework.Vector3 scale, translation;

                networkData.transModelTrans = value;

                if (isSessionDetailsSet)
                {
                    if (networkData.sessionDetails.isHost)
                    {
                        //server
                        //update world matrix
                        networkData.transWorldMat =
                            networkData.transModelTrans *
                            Microsoft.Xna.Framework.Matrix.CreateTranslation(
                                networkData.trueLatestPosition);

                        //parse rotation quaternion - model transform shouldn't be concerned with position
                        value.Decompose(
                            out scale,
                            out networkData.trueLatestOrientation,
                            out translation);
                    }
                    else
                    {
                        //client
                        //update world matrix
                        networkData.transWorldMat =
                            networkData.transModelTrans *
                            Microsoft.Xna.Framework.Matrix.CreateTranslation(
                                networkData.estPosition);

                        //parse rotation quaternion - model transform shouldn't be concerned with position
                        value.Decompose(
                            out scale,
                            out networkData.estOrientation,
                            out translation);
                    }
                }
                else
                {
                    //either
                    //update world matrix
                    networkData.transWorldMat =
                        networkData.transModelTrans *
                        Microsoft.Xna.Framework.Matrix.CreateTranslation(
                            networkData.trueLatestPosition);

                    //parse rotation quaternion - model transform shouldn't be concerned with position
                    value.Decompose(
                        out scale,
                        out networkData.trueLatestOrientation,
                        out translation);
                }

                //set physics properties
                if ((m_physBody != null) && (!networkData.sessionDetails.isHost))
                    m_physBody.MoveTo(PositionSimple, ModelTransformSimple);
            }
        }

        public Microsoft.Xna.Framework.Vector3 ForwardDir
        {
            get { return networkData.transModelTrans.Forward; }
        }

        public Microsoft.Xna.Framework.Vector3 UpDir
        {
            get { return networkData.transModelTrans.Up; }
        }

        public Microsoft.Xna.Framework.Vector3 RightDir
        {
            get { return networkData.transModelTrans.Right; }
        }


        /// <summary>
        /// Only use if world matrix is set externally (eg: by physics engine), this will
        /// parse and update the position and orientation fields automatically.
        /// </summary>
        public Microsoft.Xna.Framework.Matrix WorldTransform
        {
            get { return networkData.transWorldMat; }
            set
            {
                //this will be discarded - scale is only trans not handled
                Microsoft.Xna.Framework.Vector3 scale;

                //world is set externally
                networkData.transWorldMat = value;

                if (isSessionDetailsSet)
                {
                    if (networkData.sessionDetails.isHost)
                        //parse position and orientation
                        value.Decompose(
                            out scale,
                            out networkData.trueLatestOrientation,
                            out networkData.trueLatestPosition);
                    else
                        value.Decompose(
                            out scale,
                            out networkData.estOrientation,
                            out networkData.estPosition);
                }
                else
                {
                    //either
                    value.Decompose(
                            out scale,
                            out networkData.trueLatestOrientation,
                            out networkData.trueLatestPosition);
                }

                //set physics properties
                if ((m_physBody != null) && (!networkData.sessionDetails.isHost))
                    m_physBody.MoveTo(PositionSimple, ModelTransformSimple);
            }
        }

        #endregion

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                m_physBody = new Body();
                RemoveAllPrimitives();
                Owner = m_physBody;

                //asserts
                m_debugMgr.Assert(m_physBody != null,
                    "NetworkEntity:Init - 'm_physBody' is null.");

                //physics
                m_physBody.CollisionSkin = this;
                AddPrimitive(m_physPrimitive, m_physMaterial);

                Microsoft.Xna.Framework.Vector3 com = SetMass(m_physMass);
                m_physBody.MoveTo(networkData.initPosition, ModelTransform);
                ApplyLocalTransform(new Transform(-com, ModelTransform));
                m_physBody.EnableBody();

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        public override bool Deinit()
        {
            bool retVal = true;
            if (m_isInit)
            {
                m_physBody.DisableBody();

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        public void SetSessionDetails(SessionDetails sesDetails)
        {
            networkData.sessionDetails = sesDetails;
            isSessionDetailsSet = true;
        }

        #endregion

        #region physics

        protected Microsoft.Xna.Framework.Vector3 SetMass(float mass)
        {
            PrimitiveProperties primitiveProps = new PrimitiveProperties(
                PrimitiveProperties.MassDistributionEnum.Solid,
                PrimitiveProperties.MassTypeEnum.Mass,
                mass);

            float junk;
            Microsoft.Xna.Framework.Vector3 centreOfMass;
            Microsoft.Xna.Framework.Matrix inertia;
            Microsoft.Xna.Framework.Matrix inertiaCoM;

            GetMassProperties(
                primitiveProps,
                out junk, out centreOfMass, out inertia, out inertiaCoM);

            m_physBody.BodyInertia = inertiaCoM;
            m_physBody.Mass = junk;

            return centreOfMass;
        }

        protected void PhysicsStep()
        {
            m_debugMgr.Assert(networkData.sessionDetails.isHost,
                "NetworkEntity:PhysicsStep - should only be called by host.");

            //dummys
            Microsoft.Xna.Framework.Vector3 dummyPos;
            Microsoft.Xna.Framework.Vector3 dummyScale;
            Microsoft.Xna.Framework.Quaternion tempOrient;

            //set new values
            ModelTransformSimple = GetPrimitiveLocal(0).Transform.Orientation *
                m_physBody.Orientation;
            m_physBody.Orientation.Decompose(out dummyScale, out tempOrient, out dummyPos);
            OrientationSimple = tempOrient;

            Position = m_physBody.Position;
        }

        #endregion

        #region prediction and correction

        /// <summary>
        /// Call this during update to predict transforms. This should only be called
        /// by a client - servers 'scram'!
        /// </summary>
        public void Predict(Microsoft.Xna.Framework.GameTime clientGameTime)
        {
            //check session details have been set
            m_debugMgr.Assert(IsSessionDetailsSet,
                "NetworkEntity:Predict - session details have not been set.");

            //servers - 'scram'!
            if (networkData.sessionDetails.isHost)
            {
                m_debugMgr.WriteLogEntry("NetworkEntity:Predict - server shouldn't be here.");
                return;
            }

            if (networkData.timeOfLastCorrection == TimeSpan.Zero)
                //no corrections yet
                return;

            if (networkData.timeOfLastPrediction != TimeSpan.Zero)
            {
                //times
                float timeSinceLastCorrection =
                    (float)(clientGameTime.TotalRealTime - networkData.timeOfLastCorrection).TotalSeconds;
                float timeSinceLastPrediction =
                    (float)(clientGameTime.TotalRealTime - networkData.timeOfLastPrediction).TotalSeconds;

                #region position predictions

                networkData.estNextTruePosition = networkData.trueLatestPosition +
                    (networkData.estTrueVelocity * timeSinceLastCorrection);

                //this vector interpolates from the previous heading to the next one,
                //  this helps ease into the new velocity heading
                Microsoft.Xna.Framework.Vector3 interpolatedVelocity =
                    networkData.estVelocity + networkData.estTrueVelocity;

                //the new estimated velocity must now use this interpolated velocity heading
                //  and help correct errors from previous positions
                networkData.estVelocity = (interpolatedVelocity +
                    (networkData.estNextTruePosition - networkData.estPosition)) / 2f;

                //converge new estimated position
                PositionSimple += networkData.estVelocity * timeSinceLastPrediction;

                #endregion

                #region orientation predictions

                //xyz can be treated as "flip", w as "spin"
                networkData.estNextTrueOrientation = networkData.trueLatestOrientation +
                    (networkData.estTrueAngularVelocity * timeSinceLastCorrection);

                //fix drifts
                if (networkData.estNextTrueOrientation.Length() != 0)
                    networkData.estNextTrueOrientation.Normalize();

                //converge to predicted intermediate orientation
                Microsoft.Xna.Framework.Quaternion newEstOrientation =
                    Microsoft.Xna.Framework.Quaternion.Slerp(Orientation, networkData.estNextTrueOrientation,
                    networkData.estTrueAngularVelocity.Length());

                //fix drift
                if (newEstOrientation.Length() != 0)
                    newEstOrientation.Normalize();

                //update - will also update transforms
                Orientation = newEstOrientation;

                #endregion

            }

            //update time
            networkData.timeOfLastPrediction = clientGameTime.TotalRealTime;
        }

        /// <summary>
        /// Call this when new network packet is recieved. This should only be called
        /// by a client - servers 'scram'!
        /// </summary>
        /// <remarks>
        /// Remember to discard packets older than previous on recieved.
        /// </remarks>
        /// <param name="clientGameTime">
        /// The this client's XNA game time for the game frame.
        /// </param>
        /// <param name="latestTrueTimeStamp">
        /// Fraction of seconds elapsed since start of session.
        /// </param>
        /// <param name="latestTruePosition"></param>
        /// <param name="latestTrueOrientation"></param>
        public void Correct(    Microsoft.Xna.Framework.GameTime clientGameTime,
                                float latestTrueTimeStamp,
                                Microsoft.Xna.Framework.Vector3 latestTruePosition,
                                Microsoft.Xna.Framework.Quaternion latestTrueOrientation)
        {
            //check session details have been set
            m_debugMgr.Assert(IsSessionDetailsSet,
                "NetworkEntity:Correct - session details have not been set.");

            //servers - 'scram'!
            if (networkData.sessionDetails.isHost)
            {
                m_debugMgr.WriteLogEntry("NetworkEntity:Correct - server shouldn't be here.");
                return;
            }

            if (networkData.timeOfLastCorrection != TimeSpan.Zero)
            {

                //update true fields
                networkData.truePrevTime = networkData.trueLatestTime;
                networkData.truePrevPosition = networkData.trueLatestPosition;
                networkData.truePrevOrientation = networkData.trueLatestOrientation;

                networkData.trueLatestTime = latestTrueTimeStamp;
                networkData.trueLatestPosition = latestTruePosition;
                networkData.trueLatestOrientation = latestTrueOrientation;

                //update 'true' predictions
                networkData.estTrueVelocity =
                    (networkData.trueLatestPosition - networkData.truePrevPosition) /
                    (networkData.trueLatestTime - networkData.truePrevTime);

                //must keep angular velocity magnitudes
                networkData.estTrueAngularVelocity = Microsoft.Xna.Framework.Quaternion.Multiply(
                    Microsoft.Xna.Framework.Quaternion.Subtract(
                        networkData.trueLatestOrientation,
                        networkData.truePrevOrientation),
                    networkData.trueLatestTime - networkData.truePrevTime);
            }

            //update time stamp
            networkData.timeOfLastCorrection = clientGameTime.TotalRealTime;
        }

        #endregion

        #region game loop

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (networkData.sessionDetails.isHost)
                PhysicsStep();
            else
                Predict(gameTime);
        }

        #endregion

        #endregion
    }
}
