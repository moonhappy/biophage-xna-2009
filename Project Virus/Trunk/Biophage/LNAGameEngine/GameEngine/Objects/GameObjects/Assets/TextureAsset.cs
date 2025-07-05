/* Copyright 2009 Phillip Cooper
 
    This file is part of LNA.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Applyable;
using LNA.GameEngine.Objects.Scenes;

namespace LNA.GameEngine.Objects.GameObjects.Assets
{
    /// <summary>
    /// Represents a textured plane.
    /// </summary>
    public class TextureAsset : Asset
    {
        #region fields

        protected Microsoft.Xna.Framework.Graphics.VertexDeclaration m_vertexDeclaration;
        protected Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture[] m_verticies;
        protected Microsoft.Xna.Framework.Graphics.VertexBuffer m_vertexBuffer;

        protected Microsoft.Xna.Framework.Graphics.BasicEffect m_texEffect;
        protected TextureResHandle m_texResource;

        protected float m_initWidth;
        protected float m_width;
        protected float m_initHeight;
        protected float m_height;

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Argument constructor,
        /// </summary>
        /// <param name="id">
        /// Game object Id.
        /// </param>
        /// <param name="initialPosition">
        /// The initial position for the quad.
        /// </param>
        /// <param name="initialWidth">
        /// Initial width of the quad.
        /// </param>
        /// <param name="initialHeight">
        /// Initial height of the quad.
        /// </param>
        /// <param name="resFDir">
        /// Texture resource file directory.
        /// </param>
        /// <param name="resFName">
        /// Texture resource file name.
        /// </param>
        /// <param name="debugMgr">
        /// Reference to the debug manager.
        /// </param>
        /// <param name="resourceMgr">
        /// Reference to the resource manager.
        /// </param>
        /// <param name="scene">
        /// Reference to the scene this game object belongs to.
        /// </param>
        /// <param name="addToScene">
        /// If true, the game object will automatically be added to the
        /// scene.
        /// </param>
        /// <param name="graphicsMgr">
        /// Reference to the XNA graphics device manager.
        /// </param>
        public TextureAsset(uint id, Microsoft.Xna.Framework.Vector3 initialPosition,
                            float initialWidth, float initialHeight,
                            string resFDir, string resFName,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Scene scene, bool addToScene,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr)
            : base(id, initialPosition, debugMgr, resourceMgr, scene, addToScene)
        {
            //set the verticies - note: clockwise winding in XNA
            m_verticies = new Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture[6];

            //bottom-left
            m_verticies[0] = new Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture(
                new Microsoft.Xna.Framework.Vector3(-1f, -1f, 0f),
                new Microsoft.Xna.Framework.Vector3( 0f, 0f, 1f),
                new Microsoft.Xna.Framework.Vector2( 0f, 1f));

            //top-left
            m_verticies[1] = new Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture(
                new Microsoft.Xna.Framework.Vector3(-1f, 1f, 0f),
                new Microsoft.Xna.Framework.Vector3( 0f, 0f, 1f),
                new Microsoft.Xna.Framework.Vector2( 0f, 0f));

            //bottom-right
            m_verticies[2] = new Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture(
                new Microsoft.Xna.Framework.Vector3( 1f,-1f, 0f),
                new Microsoft.Xna.Framework.Vector3( 0f, 0f, 1f),
                new Microsoft.Xna.Framework.Vector2( 1f, 1f));

            //bottom-right
            m_verticies[3] = new Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture(
                new Microsoft.Xna.Framework.Vector3( 1f,-1f, 0f),
                new Microsoft.Xna.Framework.Vector3( 0f, 0f, 1f),
                new Microsoft.Xna.Framework.Vector2( 1f, 1f));

            //top-left
            m_verticies[4] = new Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture(
                new Microsoft.Xna.Framework.Vector3(-1f, 1f, 0f),
                new Microsoft.Xna.Framework.Vector3( 0f, 0f, 1f),
                new Microsoft.Xna.Framework.Vector2( 0f, 0f));

            //top-right
            m_verticies[5] = new Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture(
                new Microsoft.Xna.Framework.Vector3( 1f, 1f, 0f),
                new Microsoft.Xna.Framework.Vector3( 0f, 0f, 1f),
                new Microsoft.Xna.Framework.Vector2( 1f, 0f));


            m_vertexDeclaration = new Microsoft.Xna.Framework.Graphics.VertexDeclaration(
                graphicsMgr.GraphicsDevice, 
                Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.VertexElements);

            m_vertexBuffer = new Microsoft.Xna.Framework.Graphics.VertexBuffer(
                graphicsMgr.GraphicsDevice,
                Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.SizeInBytes * m_verticies.Length,
                Microsoft.Xna.Framework.Graphics.BufferUsage.None);

            m_vertexBuffer.SetData<Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture>(m_verticies);

            //set other fields
            m_texResource = new TextureResHandle(debugMgr, resourceMgr, resFDir, resFName);
            m_texEffect = new Microsoft.Xna.Framework.Graphics.BasicEffect(graphicsMgr.GraphicsDevice, null);

            m_initWidth = initialWidth;
            m_width = initialWidth;
            m_initHeight = initialHeight;
            m_height = initialHeight;

            //m_debugMgr.WriteLogEntry("TextureAsset:Constructor - done.");
        }

        #endregion

        #region field_accessors

        /// <summary>
        /// Width of the texture plane.
        /// </summary>
        public virtual float Width
        {
            get { return m_width; }
            set
            {
                m_width = value;

                //scale matrix - X axis
                Microsoft.Xna.Framework.Matrix mScaleX =
                    Microsoft.Xna.Framework.Matrix.Identity;
                mScaleX.M11 = m_width / 2f;
                AssetTransform = mScaleX * AssetTransform;
            }
        }

        /// <summary>
        /// Height of the texture plane.
        /// </summary>
        public virtual float Height
        {
            get { return m_height; }
            set
            {
                m_height = value;

                //scale matrix - Y axis
                Microsoft.Xna.Framework.Matrix mScaleY =
                    Microsoft.Xna.Framework.Matrix.Identity;
                mScaleY.M22 = m_height / 2f;
                AssetTransform = mScaleY * AssetTransform;
            }
        }

        /// <summary>
        /// Reference to the XNA basic effect shader.
        /// </summary>
        public virtual Microsoft.Xna.Framework.Graphics.BasicEffect GetBasicEffect
        {
            get { return m_texEffect; }
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Initialises the assest
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                //set width and height back
                m_width = m_initWidth;
                m_height = m_initHeight;

                //reinit the base
                retVal = base.Init();

                //set effect
                m_texEffect.LightingEnabled = false;
                m_texEffect.TextureEnabled = true;
                m_texEffect.VertexColorEnabled = false;
                m_texEffect.FogEnabled = false;

                //make adjustments to asset transform
                Microsoft.Xna.Framework.Matrix mScale =
                    Microsoft.Xna.Framework.Matrix.Identity;
                mScale.M11 = m_width / 2f;
                mScale.M22 = m_height / 2f;
                AssetTransform = mScale * AssetTransform;
            }

            return retVal;
        }

        #region loading

        /// <summary>
        /// Loads the game object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                //load the texture
                retVal = m_texResource.Load();
                m_texEffect.Texture = (Microsoft.Xna.Framework.Graphics.Texture2D)m_texResource.GetResource;

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        /// <summary>
        /// Unloads the game object.
        /// </summary>
        /// <returns>
        /// True if no error occured, otherwise false.
        /// </returns>
        public override bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                retVal = m_texResource.Unload();

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        #endregion

        #region game_loop

        /// <summary>
        /// Update routine for the game object.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        /// <summary>
        /// Animation routine for the game object.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        /// <summary>
        /// Draws the texture plane.
        /// </summary>
        /// <param name="gameTime">
        /// XNA game time for the frame.
        /// </param>
        /// <param name="graphicsDevice">
        /// Reference to the XNA graphics device.
        /// </param>
        /// <param name="camera">
        /// Reference to the scene camera.
        /// </param>
        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                                    CameraGObj camera)
        {
            //set verticies to graphics device
            graphicsDevice.VertexDeclaration = m_vertexDeclaration;
            graphicsDevice.Vertices[0].SetSource(
                m_vertexBuffer,
                0,
                Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture.SizeInBytes);

            //set effect params
            m_texEffect.World = m_worldTransform;
            m_texEffect.View = camera.ViewMatrix;
            m_texEffect.Projection = camera.ProjectionMatrix;


            //use the effect to draw the circle to the plane - assume current technique is set.
            m_texEffect.Begin();
            foreach (Microsoft.Xna.Framework.Graphics.EffectPass pass in m_texEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                graphicsDevice.DrawPrimitives(Microsoft.Xna.Framework.Graphics.PrimitiveType.TriangleList, 0, 2);

                pass.End();
            }
            m_texEffect.End();
        }

        #endregion

        #endregion
    }
}
