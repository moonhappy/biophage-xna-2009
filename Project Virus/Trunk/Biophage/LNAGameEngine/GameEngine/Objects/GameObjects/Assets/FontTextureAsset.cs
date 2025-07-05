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
    public class FontTextureAsset : Asset
    {
        #region fields

        protected Microsoft.Xna.Framework.Graphics.VertexDeclaration m_vertexDeclaration;
        protected Microsoft.Xna.Framework.Graphics.VertexPositionNormalTexture[] m_verticies;
        protected Microsoft.Xna.Framework.Graphics.VertexBuffer m_vertexBuffer;

        protected Microsoft.Xna.Framework.Graphics.BasicEffect m_texEffect;

        protected float m_initWidth;
        protected float m_width;
        protected float m_initHeight;
        protected float m_height;

        protected string m_string;
        protected SpriteFontResHandle m_font;
        protected bool m_alignCentre;
        //protected Microsoft.Xna.Framework.Graphics.Texture2D m_texture;
        protected Microsoft.Xna.Framework.Graphics.Color m_initColour;
        protected Microsoft.Xna.Framework.Graphics.Color m_colour;
        protected RenderTargetResHandle m_renderTarget;

        private Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;

        protected Microsoft.Xna.Framework.Graphics.SpriteBatch m_spriteBatch;

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
        /// <param name="initialColour">
        /// Initial colour of the string.
        /// </param>
        /// <param name="toString">
        /// String to render to the quad.
        /// </param>
        /// <param name="fontType">
        /// The sprite font of the string.
        /// </param>
        /// <param name="centreAlign">
        /// If true, the string will be rendered centre aligned.
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
        public FontTextureAsset(    uint id, Microsoft.Xna.Framework.Vector3 initialPosition,
                                    float initialWidth, float initialHeight,
                                    Microsoft.Xna.Framework.Graphics.Color initialColour,
                                    string toString, SpriteFontResHandle fontType, bool centreAlign,
                                    DebugManager debugMgr, ResourceManager resourceMgr,
                                    Scene scene, bool addToScene,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                    Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
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
            m_texEffect = new Microsoft.Xna.Framework.Graphics.BasicEffect(graphicsMgr.GraphicsDevice, null);

            m_initWidth = initialWidth;
            m_width = initialWidth;
            m_initHeight = initialHeight;
            m_height = initialHeight;

            m_string = toString;
            m_font = fontType;
            m_alignCentre = centreAlign;
            m_colour = m_initColour = initialColour;

            m_graphicsMgr = graphicsMgr;
            m_spriteBatch = spriteBatch;

            m_renderTarget = new RenderTargetResHandle(debugMgr, resourceMgr, "FontTextureAsset");

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

        /// <summary>
        /// Colour applied to string.
        /// </summary>
        public Microsoft.Xna.Framework.Graphics.Color Colour
        {
            get { return m_colour; }
            set 
            { 
                m_colour = value;

                //if (IsLoaded)
                //    RenderStringToTexture();
            }
        }

        public string GetString
        {
            get { return m_string; }
            set 
            {
                m_string = value;

                //if (IsLoaded)
                //    RenderStringToTexture();
            }
        }

        #endregion

        #region initialisation

        /// <summary>
        /// Initialises the asset
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

                //colour back
                m_colour = m_initColour;

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
                if (!m_renderTarget.Load())
                    retVal = false;

                if (retVal)
                {
                    //RenderStringToTexture();
                    m_isLoaded = true;
                }
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
                if (!m_renderTarget.Unload())
                    retVal = false;

                //if (m_texture != null)
                //{
                //    m_texture.Dispose();
                //    m_texture = null;
                //}

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
            //do render of the string
            Microsoft.Xna.Framework.Graphics.Texture2D texture = RenderStringToTexture();

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
            m_texEffect.Texture = texture;

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

        /// <summary>
        /// Renders the string to a 2D texture resource.
        /// </summary>
        protected Microsoft.Xna.Framework.Graphics.Texture2D RenderStringToTexture()
        {
            //dispose of old texture
            //if (m_texture != null)
            //    m_texture.Dispose();
            Microsoft.Xna.Framework.Graphics.RenderTarget2D prevRT = (Microsoft.Xna.Framework.Graphics.RenderTarget2D)
                m_graphicsMgr.GraphicsDevice.GetRenderTarget(0);

            //load the font
            if (!m_font.IsLoaded)
                m_debugMgr.Assert(m_font.Load(), "FontTextureAsset:RenderStringToTexture - loading spritefont failed.");

            Microsoft.Xna.Framework.Graphics.SpriteFont sfont = (Microsoft.Xna.Framework.Graphics.SpriteFont)m_font.GetResource;

            //render string
            Microsoft.Xna.Framework.Graphics.RenderTarget2D rtString =
                (Microsoft.Xna.Framework.Graphics.RenderTarget2D)m_renderTarget.GetResource;

            //calcs for centre align
            Microsoft.Xna.Framework.Vector2 FontPos =
                new Microsoft.Xna.Framework.Vector2(rtString.Width / 2f, rtString.Height / 2f);
            Microsoft.Xna.Framework.Vector2 FontOrigin = sfont.MeasureString(m_string) / 2;

            m_graphicsMgr.GraphicsDevice.SetRenderTarget(0, rtString);
            {
                m_graphicsMgr.GraphicsDevice.Clear(Microsoft.Xna.Framework.Graphics.Color.TransparentBlack);

                m_spriteBatch.Begin(
                    Microsoft.Xna.Framework.Graphics.SpriteBlendMode.AlphaBlend,
                    Microsoft.Xna.Framework.Graphics.SpriteSortMode.Immediate,
                    Microsoft.Xna.Framework.Graphics.SaveStateMode.None);

                if (m_alignCentre)
                    m_spriteBatch.DrawString(sfont, m_string, FontPos, m_colour,
                        0f, FontOrigin, 1f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0.5f);
                else
                    m_spriteBatch.DrawString(sfont, m_string, Microsoft.Xna.Framework.Vector2.Zero, m_colour);

                m_spriteBatch.End();
            }

            //set the render target back to default
            m_graphicsMgr.GraphicsDevice.SetRenderTarget(0, prevRT);

            //set to texture
            //Microsoft.Xna.Framework.Graphics.Color[] retTextData = 
            //    new Microsoft.Xna.Framework.Graphics.Color[rtString.Width * rtString.Height];

            //rtString.GetTexture().GetData<Microsoft.Xna.Framework.Graphics.Color>(retTextData);
            //m_texture = new Microsoft.Xna.Framework.Graphics.Texture2D(m_graphicsMgr.GraphicsDevice, 
            //    rtString.Width, rtString.Height);
            //m_texture.SetData<Microsoft.Xna.Framework.Graphics.Color>(retTextData);
            return rtString.GetTexture();
        }

        #endregion

        #endregion
    }
}
