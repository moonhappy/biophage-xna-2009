/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.Scenes;
using LNA.GameEngine.Resources;

namespace LNA.GameEngine.Objects.GameObjects.Assets
{
    /// <summary>
    /// Repesents a game object that includes a position vector and can
    /// be transformed via a world matrix.
    /// </summary>
    public abstract class Asset : GameObject
    {
        #region fields

        protected Microsoft.Xna.Framework.Vector3   m_initPosition;
        protected Microsoft.Xna.Framework.Vector3   m_position;

        protected Microsoft.Xna.Framework.Vector3   m_forward = Microsoft.Xna.Framework.Vector3.Forward;
        protected Microsoft.Xna.Framework.Vector3   m_up = Microsoft.Xna.Framework.Vector3.Up;
        protected Microsoft.Xna.Framework.Vector3   m_right = Microsoft.Xna.Framework.Vector3.Right;

        protected Microsoft.Xna.Framework.Matrix    m_initTransform;
        protected Microsoft.Xna.Framework.Matrix    m_assetTransform;
        protected Microsoft.Xna.Framework.Matrix    m_worldTransform;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Asset Id.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug mnager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the scene the asset belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        public Asset(   uint id, Microsoft.Xna.Framework.Vector3 initialPosition,
                        DebugManager debugMgr, ResourceManager resourceMgr,
                        Scene scene, bool addToScene)
            : base(id, debugMgr, resourceMgr, scene, addToScene)
        {
            //set fields
            m_initPosition = initialPosition;
            m_initTransform = Microsoft.Xna.Framework.Matrix.Identity;
        }

        #endregion

        #region field_accessors

        public virtual Microsoft.Xna.Framework.Vector3 InitialPosition
        {
            get { return m_initPosition; }
            set { m_initPosition = value; }
        }

        public Microsoft.Xna.Framework.Vector3 PositionSimple
        {
            get { return m_position; }
            set { m_position = value; }
        }
        public virtual Microsoft.Xna.Framework.Vector3 Position
        {
            get { return m_position; }
            set
            {
                //as struct, deep copy is implicit
                m_position = value;

                RecalculateWorldMatrix();
            }
        }

        public virtual Microsoft.Xna.Framework.Matrix InitialTransform
        {
            get { return m_initTransform; }
            set { m_initTransform = value; }
        }

        public Microsoft.Xna.Framework.Matrix AssetTransformSimple
        {
            get { return m_assetTransform; }
            set { m_assetTransform = value; }
        }
        public virtual Microsoft.Xna.Framework.Matrix AssetTransform
        {
            get { return m_assetTransform; }
            set
            {
                AssetTransformSimple = value;
                m_forward = m_assetTransform.Forward;
                m_right = m_assetTransform.Right;
                m_up = m_assetTransform.Up;
                RecalculateWorldMatrix();
            }
        }

        public Microsoft.Xna.Framework.Vector3 ForwardDirSimple
        {
            get { return m_forward; }
            set { m_forward = value; }
        }
        public virtual Microsoft.Xna.Framework.Vector3 ForwardDir
        {
            get { return m_forward; }
            set 
            { 
                ForwardDirSimple = value;
                m_assetTransform.Forward = value;
                RecalculateWorldMatrix();
            }
        }

        public Microsoft.Xna.Framework.Vector3 UpDirSimple
        {
            get { return m_up; }
            set { m_up = value; }
        }
        public virtual Microsoft.Xna.Framework.Vector3 UpDir
        {
            get { return m_up; }
            set
            {
                UpDirSimple = value;
                m_assetTransform.Up = value;
                RecalculateWorldMatrix();
            }
        }

        public Microsoft.Xna.Framework.Vector3 RightDirSimple
        {
            get { return m_right; }
            set { m_right = value; }
        }
        public virtual Microsoft.Xna.Framework.Vector3 RightDir
        {
            get { return m_right; }
            set
            {
                RightDirSimple = value;
                m_assetTransform.Right = value;
                RecalculateWorldMatrix();
            }
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Recalculates the world transform matrix from the current asset
        /// transform matrix and postion values.
        /// </summary>
        public virtual void RecalculateWorldMatrix()
        {
            m_worldTransform = Microsoft.Xna.Framework.Matrix.Multiply(
                m_assetTransform,
                Microsoft.Xna.Framework.Matrix.CreateTranslation(m_position));
        }

        /// <summary>
        /// Initialises the asset.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Init()
        {
            if (!m_isInit)
            {
                m_position = m_initPosition;
                m_assetTransform = m_initTransform;
                m_worldTransform = Microsoft.Xna.Framework.Matrix.CreateTranslation(m_position);
                m_isInit = true;
            }

            return true;
        }

        /// <summary>
        /// Reinitialises the asset.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Reinit()
        {
            m_isInit = false;
            return Init();
        }

        /// <summary>
        /// Deinitialises the asset.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Deinit()
        {
            m_isInit = false;
            return true;
        }

        #endregion

        #endregion
    }
}
