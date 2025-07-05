/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;
using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.Scenes;

//TODO: asset doesn't change via asset transform.

namespace LNA.GameEngine.Objects.GameObjects.Assets
{
    /// <summary>
    /// Represents a quad that has rounded corners.
    /// </summary>
    public class RoundCornerQuadAsset : Asset
    {
        #region fields

        //Corners
        protected CircleAsset m_topLeftCorner;
        protected CircleAsset m_topRightCorner;
        protected CircleAsset m_bottomLeftCorner;
        protected CircleAsset m_bottomRightCorner;

        //sides
        protected QuadAsset m_topSide;
        protected QuadAsset m_leftSide;
        protected QuadAsset m_rightSide;
        protected QuadAsset m_bottomSide;

        //middle
        protected QuadAsset m_middleQuad;

        //attributes
        protected float m_initWidth;
        protected float m_initHeight;
        protected float m_initRadius;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor.
        /// </summary>
        /// <param name="id">
        /// Asset Id.
        /// </param>
        /// <param name="initialPosition">
        /// Initial position.
        /// </param>
        /// <param name="initialWidth">
        /// Initial width, must be greater than the corner radius.
        /// </param>
        /// <param name="initialHeight">
        /// Initial height, must be greater than the corner radius.
        /// </param>
        /// <param name="initialCornerRadius">
        /// Initial corner radius, must be less than the width and 
        /// height.
        /// </param>
        /// <param name="initialColour">
        /// Initial colour of asset.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the scene the asset belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the asset will be automatically added to the scene.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics manager.
        /// </param>
        public RoundCornerQuadAsset(uint id, 
                                    Microsoft.Xna.Framework.Vector3 initialPosition,
                                    float initialWidth, float initialHeight, float initialCornerRadius,
                                    Microsoft.Xna.Framework.Graphics.Color initialColour,
                                    DebugManager debugMgr, ResourceManager resourceMgr,
                                    Scene scene, bool addToScene,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr)
            : base(id, initialPosition, debugMgr, resourceMgr, scene, addToScene)
        {
            //preliminarys
            m_initWidth = initialWidth;
            m_initHeight = initialHeight;
            m_initRadius = initialCornerRadius;
   
            float diameter = initialCornerRadius * 2f;
            debugMgr.Assert(diameter < initialWidth, "RoundCornerQuadAsset:Constructor - radius is too big.");
            debugMgr.Assert(diameter < initialHeight, "RoundCornerQuadAsset:Constructor - radius is too big.");
            float halfWidth = 0.5f * initialWidth;
            float halfHeight = 0.5f * initialHeight;

            //set quads
            m_middleQuad = new QuadAsset(
                uint.MaxValue - id,
                initialPosition, 
                initialWidth, initialHeight, initialColour,
                debugMgr, resourceMgr, scene, false, graphicsMgr);
            m_topSide = new QuadAsset(
                uint.MaxValue - id - (uint)1,
                new Microsoft.Xna.Framework.Vector3(initialPosition.X, initialPosition.Y + halfHeight, initialPosition.Z),
                initialWidth, diameter, initialColour,
                debugMgr, resourceMgr, scene, false, graphicsMgr);
            m_leftSide = new QuadAsset(
                uint.MaxValue - id - (uint)2,
                new Microsoft.Xna.Framework.Vector3(initialPosition.X - halfWidth, initialPosition.Y, initialPosition.Z),
                diameter, initialHeight, initialColour,
                debugMgr, resourceMgr, scene, false, graphicsMgr);
            m_rightSide = new QuadAsset(
                uint.MaxValue - id - (uint)3,
                new Microsoft.Xna.Framework.Vector3(initialPosition.X + halfWidth, initialPosition.Y, initialPosition.Z),
                diameter, initialHeight, initialColour,
                debugMgr, resourceMgr, scene, false, graphicsMgr);
            m_bottomSide = new QuadAsset(
                uint.MaxValue - id - (uint)4,
                new Microsoft.Xna.Framework.Vector3(initialPosition.X, initialPosition.Y - halfHeight, initialPosition.Z),
                initialWidth, diameter, initialColour,
                debugMgr, resourceMgr, scene, false, graphicsMgr);

            //set corners
            m_topLeftCorner = new CircleAsset(
                uint.MaxValue - id - (uint)5,
                new Microsoft.Xna.Framework.Vector3(initialPosition.X - halfWidth, initialPosition.Y + halfHeight, initialPosition.Z),
                initialCornerRadius, initialColour,
                debugMgr, resourceMgr, scene, false, graphicsMgr);
            m_topRightCorner = new CircleAsset(
                uint.MaxValue - id - (uint)6,
                new Microsoft.Xna.Framework.Vector3(initialPosition.X + halfWidth, initialPosition.Y + halfHeight, initialPosition.Z),
                initialCornerRadius, initialColour,
                debugMgr, resourceMgr, scene, false, graphicsMgr);
            m_bottomLeftCorner = new CircleAsset(
                uint.MaxValue - id - (uint)7,
                new Microsoft.Xna.Framework.Vector3(initialPosition.X - halfWidth, initialPosition.Y - halfHeight, initialPosition.Z),
                initialCornerRadius, initialColour,
                debugMgr, resourceMgr, scene, false, graphicsMgr);
            m_bottomRightCorner = new CircleAsset(
                uint.MaxValue - id - (uint)8,
                new Microsoft.Xna.Framework.Vector3(initialPosition.X + halfWidth, initialPosition.Y - halfHeight, initialPosition.Z),
                initialCornerRadius, initialColour,
                debugMgr, resourceMgr, scene, false, graphicsMgr);
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Refreshes the corner quad layout.
        /// </summary>
        /// <param name="width">
        /// New width of the quad.
        /// </param>
        /// <param name="height">
        /// New height of the quad.
        /// </param>
        /// <param name="radius">
        /// New corner radius, must not allow diameter to be more than
        /// the height or width.
        /// </param>
        private void Refresh(float width, float height, float radius)
        {
            //preliminaries
            float diameter = radius * 2f;
            float halfWidth = 0.5f * width;
            float halfHeight = 0.5f * height;

            //corners
            m_topLeftCorner.Radius = m_topRightCorner.Radius =
                m_bottomLeftCorner.Radius = m_bottomRightCorner.Radius = radius;

            m_topLeftCorner.Position = new Microsoft.Xna.Framework.Vector3(
                m_position.X - halfWidth, m_position.Y + halfHeight, m_position.Z);
            m_topRightCorner.Position = new Microsoft.Xna.Framework.Vector3(
                m_position.X + halfWidth, m_position.Y + halfHeight, m_position.Z);
            m_bottomLeftCorner.Position = new Microsoft.Xna.Framework.Vector3(
                m_position.X - halfWidth, m_position.Y - halfHeight, m_position.Z);
            m_bottomRightCorner.Position = new Microsoft.Xna.Framework.Vector3(
                m_position.X + halfWidth, m_position.Y - halfHeight, m_position.Z);

            //sides
            m_topSide.Width = width; m_topSide.Height = diameter;
            m_topSide.Position = new Microsoft.Xna.Framework.Vector3(
                m_position.X, m_position.Y + halfHeight, m_position.Z);

            m_leftSide.Width = diameter; m_leftSide.Height = height;
            m_leftSide.Position = new Microsoft.Xna.Framework.Vector3(
                m_position.X - halfWidth, m_position.Y, m_position.Z);

            m_rightSide.Width = diameter; m_rightSide.Height = height;
            m_rightSide.Position = new Microsoft.Xna.Framework.Vector3(
                m_position.X + halfWidth, m_position.Y, m_position.Z);

            m_bottomSide.Width = width; m_bottomSide.Height = diameter;
            m_bottomSide.Position = new Microsoft.Xna.Framework.Vector3(
                m_position.X, m_position.Y - halfHeight, m_position.Z);

            //middle
            m_middleQuad.Width = width; m_middleQuad.Height = height;
        }

        /// <summary>
        /// Position of the quad.
        /// </summary>
        public override Microsoft.Xna.Framework.Vector3 Position
        {
            get
            {
                return base.Position;
            }
            set
            {
                base.Position = value;
                
                //preliminaries
                float diameter = m_topLeftCorner.Radius * 2f;
                float halfWidth = 0.5f * m_middleQuad.Width;
                float halfHeight = 0.5f * m_middleQuad.Height;

                //change positions
                m_topLeftCorner.Position = new Microsoft.Xna.Framework.Vector3(
                m_position.X - halfWidth, m_position.Y + halfHeight, m_position.Z);
                m_topRightCorner.Position = new Microsoft.Xna.Framework.Vector3(
                    m_position.X + halfWidth, m_position.Y + halfHeight, m_position.Z);
                m_bottomLeftCorner.Position = new Microsoft.Xna.Framework.Vector3(
                    m_position.X - halfWidth, m_position.Y - halfHeight, m_position.Z);
                m_bottomRightCorner.Position = new Microsoft.Xna.Framework.Vector3(
                    m_position.X + halfWidth, m_position.Y - halfHeight, m_position.Z);

                //sides
                m_topSide.Position = new Microsoft.Xna.Framework.Vector3(
                    m_position.X, m_position.Y + halfHeight, m_position.Z);

                m_leftSide.Position = new Microsoft.Xna.Framework.Vector3(
                    m_position.X - halfWidth, m_position.Y, m_position.Z);

                m_rightSide.Position = new Microsoft.Xna.Framework.Vector3(
                    m_position.X + halfWidth, m_position.Y, m_position.Z);

                m_bottomSide.Position = new Microsoft.Xna.Framework.Vector3(
                    m_position.X, m_position.Y - halfHeight, m_position.Z);

                //middle
                m_middleQuad.Position = new Microsoft.Xna.Framework.Vector3(
                    m_position.X, m_position.Y, m_position.Z);
            }
        }

        /// <summary>
        /// Width of quad.
        /// </summary>
        public float Width
        {
            get { return m_middleQuad.Width; }
            set
            {
                //validate
                float diameter = m_topLeftCorner.Radius * 2f;
                if (value > diameter)
                {
                    Refresh(value, m_middleQuad.Height, m_topLeftCorner.Radius);
                }
                else
                    m_debugMgr.WriteLogEntry("RoundCornerQuadAsset:Width - width too small.");
            }
        }

        /// <summary>
        /// Height of quad.
        /// </summary>
        public float Height
        {
            get { return m_middleQuad.Height; }
            set
            {
                //validate
                float diameter = m_topLeftCorner.Radius * 2f;
                if (value > diameter)
                {
                    Refresh(m_middleQuad.Width, value, m_topLeftCorner.Radius);
                }
                else
                    m_debugMgr.WriteLogEntry("RoundCornerQuadAsset:Height - height too small.");
            }
        }

        /// <summary>
        /// Radius of corners.
        /// </summary>
        public float CornerRadius
        {
            get { return m_topLeftCorner.Radius; }
            set
            {
                Refresh(m_middleQuad.Width, m_middleQuad.Height, value);
            }
        }

        /// <summary>
        /// Colour of the quad.
        /// </summary>
        public Microsoft.Xna.Framework.Graphics.Color Colour
        {
            get { return m_middleQuad.Color; }
            set
            {
                m_middleQuad.Color =
                    m_topLeftCorner.Color =
                    m_topRightCorner.Color =
                    m_bottomLeftCorner.Color =
                    m_bottomRightCorner.Color =
                    m_topSide.Color =
                    m_leftSide.Color =
                    m_rightSide.Color =
                    m_bottomSide.Color =
                    value;
            }
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Initialises the asset.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                if (    (!m_topLeftCorner.Init()) ||
                        (!m_topRightCorner.Init()) ||
                        (!m_bottomLeftCorner.Init()) ||
                        (!m_bottomRightCorner.Init()) ||

                        (!m_topSide.Init()) ||
                        (!m_leftSide.Init()) ||
                        (!m_rightSide.Init()) ||
                        (!m_bottomSide.Init()) ||

                        (!m_middleQuad.Init()))

                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        #region loading

        /// <summary>
        /// Loads the asset.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if (    (!m_topLeftCorner.Load()) ||
                        (!m_topRightCorner.Load()) ||
                        (!m_bottomLeftCorner.Load()) ||
                        (!m_bottomRightCorner.Load()) ||

                        (!m_topSide.Load()) ||
                        (!m_leftSide.Load()) ||
                        (!m_rightSide.Load()) ||
                        (!m_bottomSide.Load()) ||
                    
                        (!m_middleQuad.Load()) )

                    retVal = false;

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        /// <summary>
        /// Unloads the asset.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                if (    (!m_topLeftCorner.Unload()) ||
                        (!m_topRightCorner.Unload()) ||
                        (!m_bottomLeftCorner.Unload()) ||
                        (!m_bottomRightCorner.Unload()) ||

                        (!m_topSide.Unload()) ||
                        (!m_leftSide.Unload()) ||
                        (!m_rightSide.Unload()) ||
                        (!m_bottomSide.Unload()) ||

                        (!m_middleQuad.Unload()))

                    retVal = false;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        /// <summary>
        /// Deinitialises the asset.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Deinit()
        {
            bool retVal = true;
            if (m_isInit)
            {
                if (    (!m_topLeftCorner.Deinit()) ||
                        (!m_topRightCorner.Deinit()) ||
                        (!m_bottomLeftCorner.Deinit()) ||
                        (!m_bottomRightCorner.Deinit()) ||

                        (!m_topSide.Deinit()) ||
                        (!m_leftSide.Deinit()) ||
                        (!m_rightSide.Deinit()) ||
                        (!m_bottomSide.Deinit()) ||

                        (!m_middleQuad.Deinit()))

                    retVal = false;

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        #endregion

        #region game_loop

        /// <summary>
        /// Updates the asset.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing.
        }

        /// <summary>
        /// Animation type update procedure for the asset.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing.
        }

        /// <summary>
        /// Draw routine for the asset.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// Reference to the XNA graphics device manager.
        /// </param>
        /// <param name="camera">
        /// Reference to the scene's camera state.
        /// </param>
        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            //draw corners first
            m_topLeftCorner.DoDraw(gameTime, graphicsDevice, camera);
            m_topRightCorner.DoDraw(gameTime, graphicsDevice, camera);
            m_bottomLeftCorner.DoDraw(gameTime, graphicsDevice, camera);
            m_bottomRightCorner.DoDraw(gameTime, graphicsDevice, camera);

            //then the sides
            m_topSide.DoDraw(gameTime, graphicsDevice, camera);
            m_leftSide.DoDraw(gameTime, graphicsDevice, camera);
            m_rightSide.DoDraw(gameTime, graphicsDevice, camera);
            m_bottomSide.DoDraw(gameTime, graphicsDevice, camera);

            //then finally, the middle
            m_middleQuad.DoDraw(gameTime, graphicsDevice, camera);
        }

        #endregion

        #endregion
    }
}
