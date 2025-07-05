using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LNA.GameEngine;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Objects.Scenes;
using JigLibX.Collision;
using JigLibX.Physics;

namespace Biophage.Game.Stages.Game.Common
{
    /// <summary>
    /// Represents a collidable asset.
    /// </summary>
    public abstract class CollidableAsset : ColliableGObj
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
        public CollidableAsset( uint id, Microsoft.Xna.Framework.Vector3 initialPosition,
                                DebugManager debugMgr, ResourceManager resourceMgr,
                                Scene scene, bool addToScene)
            : base(id, debugMgr, resourceMgr, scene, addToScene)
        {
            //set fields
            m_initPosition = initialPosition;
            m_initTransform = Microsoft.Xna.Framework.Matrix.Identity;
            m_physBody = new Body();
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

                //move collision skin to current
                m_physBody.MoveTo(m_position, AssetTransform);
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

                //move collision skin to current
                m_physBody.MoveTo(m_position, AssetTransform);
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

                //move collision skin to current
                m_physBody.MoveTo(m_position, AssetTransform);
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

                //move collision skin to current
                m_physBody.MoveTo(m_position, AssetTransform);
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

                //move collision skin to current
                m_physBody.MoveTo(m_position, AssetTransform);
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
