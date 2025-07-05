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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Physics;

namespace Biophage.Game.Stages.Game.Common
{
    public class PlayerCursor : CollidableModelAsset
    {
        #region fields

        Vector3 Heading = Vector3.Forward;
        Vector3 Up = Vector3.Up;
        Vector3 Right = Vector3.Right;

        Vector3 CursorOldPos = Vector3.Zero;

//#if DEBUG
//        Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;
//        Microsoft.Xna.Framework.Graphics.VertexPositionColor[] m_colSkinVerticies;
//        Microsoft.Xna.Framework.Graphics.BasicEffect m_colSkinBasicEffect;
//#endif

        #endregion

        #region methods

        #region construction

        public PlayerCursor(uint id,
                                Microsoft.Xna.Framework.Vector3 initPos,
                                DebugManager debugMgr, ResourceManager resMgr,
                                Scene scn)
            : base(id, initPos, "Content\\Models\\Cursor\\", "Cursor", debugMgr, resMgr, scn, true)
        {
            callbackFn += m_physSkin_callbackFn;

//#if DEBUG
//            m_graphicsMgr = scn.Stage.SceneMgr.Game.GraphicsMgr;
//            m_colSkinBasicEffect = new Microsoft.Xna.Framework.Graphics.BasicEffect(
//                m_graphicsMgr.GraphicsDevice, null);
//#endif
        }

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                if (!base.Init())
                    retVal = false;

                m_physBody = new Body();
                RemoveAllPrimitives();
                Owner = m_physBody;

                //physics
                m_physBody.CollisionSkin = this;
                AddPrimitive(new Sphere(Position, 1f), (int)MaterialTable.MaterialID.NotBouncyNormal);

                m_physBody.MoveTo(Position, AssetTransform);
                m_physBody.EnableBody();

                m_physBody.Immovable = true;

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
                if (!base.Deinit())
                    retVal = false;

                m_physBody.DisableBody();
                
                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        #endregion

        #region physics

        bool m_physSkin_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            //if we identify that the skin is the level, obey the collision,
            //  otherwise ignor.
            if ((skin0 is LevelEnvironment) || (skin1 is LevelEnvironment))
            {
                //go back a bit
                Vector3 backDir = (CursorOldPos - Position);
                if (backDir.Length() != 0)
                    backDir.Normalize();

                Position += backDir;
            }

            //indicate that we don't want to generate contact points
            return false;
        }

        #endregion

        #region game loop

        public void Input(GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;


            // Determine rotation amount from input
            Vector2 rotationAmount = -newGPState.ThumbSticks.Right;
#if !XBOX
            if (newKBState.IsKeyDown(Keys.Left))
                rotationAmount.X = 1.0f;
            if (newKBState.IsKeyDown(Keys.Right))
                rotationAmount.X = -1.0f;
            if (newKBState.IsKeyDown(Keys.Up))
                rotationAmount.Y = 1.0f;
            if (newKBState.IsKeyDown(Keys.Down))
                rotationAmount.Y = -1.0f;
#endif

            // Scale rotation amount to radians per second
            rotationAmount = rotationAmount * 1.5f * elapsed;

            // Correct the X axis steering when the ship is upside down
            if (Up.Y < 0)
                rotationAmount.X = -rotationAmount.X;


            // Create rotation matrix from rotation amount
            Matrix rotationMatrix =
                Matrix.CreateFromAxisAngle(Right, rotationAmount.Y) *
                Matrix.CreateRotationY(rotationAmount.X);

            // Rotate orientation vectors
            Heading = Vector3.TransformNormal(Heading, rotationMatrix);
            Up = Vector3.TransformNormal(Up, rotationMatrix);

            // Re-normalize orientation vectors
            // Without this, the matrix transformations may introduce small rounding
            // errors which add up over time and could destabilize the ship.
            if (Heading.Length() != 0)
                Heading.Normalize();
            if (Up.Length() != 0)
                Up.Normalize();

            // Re-calculate Right
            Right = Vector3.Cross(Heading, Up);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            Up = Vector3.Cross(Right, Heading);


            // Determine thrust amount from input
            float thrustAmount = newGPState.ThumbSticks.Left.Y * 10f;
#if !XBOX
            if (newKBState.IsKeyDown(Keys.S))
                thrustAmount = -10.0f;
            if (newKBState.IsKeyDown(Keys.W))
                thrustAmount = 10.0f;
#endif

            // Calculate pos delta
            Vector3 PosDelta = Heading * (thrustAmount);

            CursorOldPos = Position;
            PositionSimple += PosDelta * elapsed;


            // Reconstruct transforms
            Matrix newTrans = Matrix.Identity;
            newTrans.Forward = Heading;
            newTrans.Up = Up;
            newTrans.Right = Right;
            AssetTransform = newTrans;
        }

        public override void Draw(  GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            base.Draw(gameTime, graphicsDevice, camera);

//#if DEBUG
//            DrawCollisionSkin(gameTime, graphicsDevice, camera);
//#endif
        }

//#if DEBUG
//        private void DrawCollisionSkin(Microsoft.Xna.Framework.GameTime gameTime,
//                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
//                                    CameraGObj camera)
//        {
//            //set verticies to graphics device
//            m_colSkinVerticies = this.GetLocalSkinWireframe();
//            m_physBody.TransformWireframe(m_colSkinVerticies);


//            //set effect params
//            m_colSkinBasicEffect.World = Microsoft.Xna.Framework.Matrix.Identity;
//            m_colSkinBasicEffect.View = camera.ViewMatrix;
//            m_colSkinBasicEffect.Projection = camera.ProjectionMatrix;


//            //use the effect to draw the circle to the plane
//            if (m_colSkinVerticies.Length > 0)
//            {
//                m_colSkinBasicEffect.Begin();
//                foreach (Microsoft.Xna.Framework.Graphics.EffectPass pass in m_colSkinBasicEffect.CurrentTechnique.Passes)
//                {
//                    pass.Begin();

//                    graphicsDevice.DrawUserPrimitives<Microsoft.Xna.Framework.Graphics.VertexPositionColor>
//                        (Microsoft.Xna.Framework.Graphics.PrimitiveType.LineStrip,
//                        m_colSkinVerticies, 0, m_colSkinVerticies.Length - 1);

//                    pass.End();
//                }
//                m_colSkinBasicEffect.End();
//            }
//        }
//#endif

        #endregion

        #endregion
    }
}
