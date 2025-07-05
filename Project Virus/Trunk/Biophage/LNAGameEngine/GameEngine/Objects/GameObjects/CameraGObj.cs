/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;

namespace LNA.GameEngine.Objects.GameObjects
{
    /// <summary>
    /// Represents a perspective type camera and includes all camera
    /// functionalty.
    /// </summary>
    public class CameraGObj : GameObject
    {
        #region fields

        #region vectors

        protected Microsoft.Xna.Framework.Vector3 m_initPosition;
        protected Microsoft.Xna.Framework.Vector3 m_position;

        protected Microsoft.Xna.Framework.Vector3 m_initLookAt;
        protected Microsoft.Xna.Framework.Vector3 m_lookAt;

        protected Microsoft.Xna.Framework.Vector3 m_initUpDirection;
        protected Microsoft.Xna.Framework.Vector3 m_upDirection;

        #endregion

        #region specs

        protected float m_initFovDegrees;
        protected float m_fovDegrees;

        protected float m_initAspectRatio;
        protected float m_aspectRatio;

        protected float m_initNearClip;
        protected float m_nearClipDistance;

        protected float m_initFarClip;
        protected float m_farClipDistance;

        #endregion

        protected Microsoft.Xna.Framework.Matrix m_viewMatrix;
        protected Microsoft.Xna.Framework.Matrix m_projMatrix;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Camera object Id.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to scene that camera belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        /// <param name="initAspectRatio">
        /// Camera aspect ratio.
        /// </param>
        public CameraGObj(  uint id,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Scene scene, bool addToScene,
                            float initAspectRatio)
            : base(id, debugMgr, resourceMgr, scene, addToScene)
        {
            //set fields
            m_initPosition = new Microsoft.Xna.Framework.Vector3(0.0f, 0.0f, 1.0f);
            m_position = new Microsoft.Xna.Framework.Vector3(m_initPosition.X, m_initPosition.Y, m_initPosition.Z);
            m_initLookAt = new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f);
            m_lookAt = new Microsoft.Xna.Framework.Vector3(m_initLookAt.X, m_initLookAt.Y, m_initLookAt.Z);
            m_initUpDirection = new Microsoft.Xna.Framework.Vector3(0f, 1f, 0f);
            m_upDirection = new Microsoft.Xna.Framework.Vector3(m_initUpDirection.X, m_initUpDirection.Y, m_initUpDirection.Z);

            m_fovDegrees = m_initFovDegrees = 45.0f;
            m_initAspectRatio = initAspectRatio;
            m_aspectRatio = m_initAspectRatio;
            m_nearClipDistance = m_initNearClip = 1.0f;
            m_farClipDistance = m_initFarClip = 10000.0f;

            //m_debugMgr.WriteLogEntry("CameraObject:Constructor - done.");
        }

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Camera object Id.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to scene that camera belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        /// <param name="initPosition">
        /// Initial position of the camera.
        /// </param>
        /// <param name="initLookAt">
        /// Initial vector point the camera is looking centre towards.
        /// </param>
        /// <param name="initUpDirection">
        /// Initial up direction of the camera, part of orientation.
        /// </param>
        /// <param name="initAspectRatio">
        /// Initial camera aspect ratio.
        /// </param>
        /// <param name="initFieldOfViewDegrees">
        /// Initial angle of the field of view (in degrees).
        /// </param>
        /// <param name="initNearClip">
        /// Initial near clip plane distance from camera.
        /// </param>
        /// <param name="initFarClip">
        /// Initial far clip plane distance from camera.
        /// </param>
        public CameraGObj(  uint id,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Scene scene, bool addToScene,
                            float initAspectRatio,

                            Microsoft.Xna.Framework.Vector3 initPosition,
                            Microsoft.Xna.Framework.Vector3 initLookAt,
                            Microsoft.Xna.Framework.Vector3 initUpDirection,

                            float initFieldOfViewDegrees,
                            float initNearClip,
                            float initFarClip)
            : base(id, debugMgr, resourceMgr, scene, addToScene)
        {
            //set fields - not by reference (IE: deep copy)
            m_initPosition = new Microsoft.Xna.Framework.Vector3(initPosition.X, initPosition.Y, initPosition.Z);
            m_position = new Microsoft.Xna.Framework.Vector3(m_initPosition.X, m_initPosition.Y, m_initPosition.Z);
            m_initLookAt = new Microsoft.Xna.Framework.Vector3(initLookAt.X, initLookAt.Y, initLookAt.Z);
            m_lookAt = new Microsoft.Xna.Framework.Vector3(m_initLookAt.X, m_initLookAt.Y, m_initLookAt.Z);
            m_initUpDirection = new Microsoft.Xna.Framework.Vector3(initUpDirection.X, initUpDirection.Y, initUpDirection.Z);
            m_initUpDirection.Normalize();
            m_upDirection = new Microsoft.Xna.Framework.Vector3(m_initUpDirection.X, m_initUpDirection.Y, m_initUpDirection.Z);

            m_fovDegrees = m_initFovDegrees = initFieldOfViewDegrees;
            m_aspectRatio = m_initAspectRatio = initAspectRatio;
            m_nearClipDistance = m_initNearClip = initNearClip;
            m_farClipDistance = m_initFarClip = initFarClip;

            ResetViewMatrix();
            ResetProjMatrix();

            //m_debugMgr.WriteLogEntry("CameraObject:Constructor - done.");
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Update the camera to apply new attributes.
        /// </summary>
        /// <remarks>
        /// This method needs to be called whenever the camera position or
        /// looking at fields are changed manually (usefull if a lot of
        /// changes need to be made without the performance hit of many
        /// allocations).
        /// </remarks>
        public void UpdateCamera()
        {
            ResetViewMatrix();
            ResetProjMatrix();
        }

        /// <summary>
        /// Resets the view matrix.
        /// </summary>
        private void ResetViewMatrix()
        {
            m_viewMatrix = Microsoft.Xna.Framework.Matrix.CreateLookAt(
                m_position,
                m_lookAt,
                m_upDirection);
        }

        /// <summary>
        /// Resets the projection matrix.
        /// </summary>
        private void ResetProjMatrix()
        {
            m_projMatrix = Microsoft.Xna.Framework.Matrix.CreatePerspectiveFieldOfView(
                Microsoft.Xna.Framework.MathHelper.ToRadians(m_fovDegrees),
                m_aspectRatio,
                m_nearClipDistance,
                m_farClipDistance);
        }

        /// <summary>
        /// Camera position vertex.
        /// </summary>
        public Microsoft.Xna.Framework.Vector3 Position
        {
            get { return m_position; }
            set
            {
                m_position = value;
                ResetViewMatrix();
            }
        }

        /// <summary>
        /// Vertex position camera is aimed at.
        /// </summary>
        public Microsoft.Xna.Framework.Vector3 LookingAt
        {
            get { return m_lookAt; }
            set
            {
                m_lookAt = value;
                ResetViewMatrix();
            }
        }

        /// <summary>
        /// Orientation of camera (Up direction).
        /// </summary>
        public Microsoft.Xna.Framework.Vector3 UpDir
        {
            get { return m_upDirection; }
            set
            {
                m_upDirection = value;
                    //normalise
                    m_upDirection.Normalize();
                ResetViewMatrix();
            }
        }

        /// <summary>
        /// Camera field of view, in degrees.
        /// </summary>
        public float FeildOfViewDegrees
        {
            get { return m_fovDegrees; }
            set
            {
                m_fovDegrees = value;
                ResetProjMatrix();
            }
        }

        /// <summary>
        /// Camera aspect ratio.
        /// </summary>
        public float AspectRatio
        {
            get { return m_aspectRatio; }
            set
            {
                m_aspectRatio = value;
                ResetProjMatrix();
            }
        }

        /// <summary>
        /// Camera near clipping plane.
        /// </summary>
        public float NearClip
        {
            get { return m_nearClipDistance; }
            set
            {
                m_nearClipDistance = value;
                ResetProjMatrix();
            }
        }

        /// <summary>
        /// Camera far clipping plane.
        /// </summary>
        public float FarClip
        {
            get { return m_farClipDistance; }
            set
            {
                m_farClipDistance = value;
                ResetProjMatrix();
            }
        }

        /// <summary>
        /// Reference to the calculated projection matrix.
        /// </summary>
        public Microsoft.Xna.Framework.Matrix ProjectionMatrix
        {
            get { return m_projMatrix; }
        }

        /// <summary>
        /// Reference to the calculated view matrix.
        /// </summary>
        public Microsoft.Xna.Framework.Matrix ViewMatrix
        {
            get { return m_viewMatrix; }
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Initialises camera to its default position, orientation, field
        /// of view, aspect, and direction it is looking at.
        /// </summary>
        /// <returns>
        /// True is no error occured, otherwise false.
        /// </returns>
        public override bool Init()
        {
            //m_debugMgr.WriteLogEntry("CameraObject:Init - doing.");

            if (!m_isInit)
            {
                m_position = new Microsoft.Xna.Framework.Vector3(m_initPosition.X, m_initPosition.Y, m_initPosition.Z);
                m_lookAt = new Microsoft.Xna.Framework.Vector3(m_initLookAt.X, m_initLookAt.Y, m_initLookAt.Z);
                m_upDirection = new Microsoft.Xna.Framework.Vector3(m_initUpDirection.X, m_initUpDirection.Y, m_initUpDirection.Z);

                m_fovDegrees = m_initFovDegrees;
                m_aspectRatio = m_initAspectRatio;
                m_nearClipDistance = m_initNearClip;
                m_farClipDistance = m_initFarClip;

                ResetViewMatrix();
                ResetProjMatrix();

                m_isInit = true;
            }

            return true;
        }

        /// <summary>
        /// Reinitialises the camera to defaults.
        /// </summary>
        /// <returns>
        /// True is no error occured, otherwise false.
        /// </returns>
        public override bool Reinit()
        {
            //m_debugMgr.WriteLogEntry("CameraObject:Reinit - doing.");

            m_isInit = false;
            return Init();
        }

        #region loading

        /// <summary>
        /// Loads the camera, this does nothing interesting.
        /// </summary>
        /// <returns>
        /// True is no error occured, otherwise false.
        /// </returns>
        public override bool Load()
        {
            //m_debugMgr.WriteLogEntry("CameraObject:Load - doing.");

            m_isLoaded = true;
            return true;
        }

        /// <summary>
        /// Unloads the camera, this does nothing interesting.
        /// </summary>
        /// <returns>
        /// True is no error occured, otherwise false.
        /// </returns>
        public override bool Unload()
        {
            //m_debugMgr.WriteLogEntry("CameraObject:Unload - doing.");

            m_isLoaded = false;
            return true;
        }

        #endregion

        /// <summary>
        /// Deinitialises the camera, this does nothing interesting.
        /// </summary>
        /// <returns>
        /// True is no error occured, otherwise false.
        /// </returns>
        public override bool Deinit()
        {
            //m_debugMgr.WriteLogEntry("CameraObject:Deinit - doing.");

            m_isInit = false;
            return true;
        }

        #endregion

        #region game_loop

        /// <summary>
        /// Override this function to give the camera logic.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time value for the frame.
        /// </param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //m_debugMgr.WriteLogEntry("CameraObject:Update - doing.");
        }

        /// <summary>
        /// Override this function to give the camera animation logic.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time value for the frame.
        /// </param>
        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //m_debugMgr.WriteLogEntry("CameraObject:Animate - doing.");
        }

        /// <summary>
        /// Override this method if you wish to draw a camera source item.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time value for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// XNA graphics device.
        /// </param>
        /// <param name="camera">
        /// Scene camera, constains projection & view matricies.
        /// </param>
        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                                    CameraGObj camera)
        {
            //m_debugMgr.WriteLogEntry("CameraObject:Draw - doing.");
        }

        #endregion

        #endregion
    }
}
