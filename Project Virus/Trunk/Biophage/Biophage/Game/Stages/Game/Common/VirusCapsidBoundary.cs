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

using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Math;
using JigLibX.Physics;
using Microsoft.Xna.Framework;

namespace Biophage.Game.Stages.Game.Common
{
    public class VirusCapsidBoundary : CollidableAsset
    {
        #region fields

        VirusCapsid m_virusCapsid;

//#if DEBUG
//        Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;
//        Microsoft.Xna.Framework.Graphics.VertexPositionColor[] m_colSkinVerticies;
//        Microsoft.Xna.Framework.Graphics.BasicEffect m_colSkinBasicEffect;
//#endif

        Microsoft.Xna.Framework.Vector3 m_prevPos;

        #endregion

        #region methods

        #region construction

        public VirusCapsidBoundary(VirusCapsid virusCapsid, DebugManager dbgMgr, ResourceManager resMgr,
            Scene scn, Microsoft.Xna.Framework.GraphicsDeviceManager graphiccMgr)
            : base(uint.MaxValue, virusCapsid.InitialPosition, dbgMgr, resMgr, scn, false)
        {
            m_virusCapsid = virusCapsid;
            m_prevPos = InitialPosition;

//#if DEBUG
//            m_graphicsMgr = graphiccMgr;
//            m_colSkinBasicEffect = new Microsoft.Xna.Framework.Graphics.BasicEffect(
//                m_graphicsMgr.GraphicsDevice, null);
//#endif

            callbackFn += VirusCapsidBoundry_callbackFn;
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
                AddPrimitive(
                    new Sphere(m_virusCapsid.Position, 1f), 
                    (int)MaterialTable.MaterialID.NotBouncyNormal);

                Microsoft.Xna.Framework.Vector3 com = SetMass(1f);
                m_physBody.MoveTo(Position, AssetTransform);
                ApplyLocalTransform(new Transform(-com, AssetTransform));
                m_physBody.EnableBody();

                m_physBody.Immovable = true;

                m_prevPos = m_virusCapsid.Position;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        #region loading

        public override bool Load()
        {
            m_isLoaded = true;
            return true;
        }

        public override bool Unload()
        {
            m_isLoaded = false;
            return true;
        }

        #endregion

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

        bool VirusCapsidBoundry_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            //if we identify that the skin is the level, obey the collision.
            if (Active)
            {
                if ((m_virusCapsid.m_virusOwner.virusStateData.isMine) ||
                    (m_virusCapsid.m_virusOwner.m_sessionDetails.isHost && 
                        m_virusCapsid.m_virusOwner.virusStateData.isBot))
                {
                    if ((skin0 is LevelEnvironment) || (skin1 is LevelEnvironment))
                    {
                        //go back a bit
                        Vector3 backDir = (m_prevPos - Position);
                        if (backDir.Length() != 0f)
                            backDir.Normalize();

                        m_virusCapsid.Position += backDir;
                    }
                }
            }

            //indicate that we don't want to generate contact points
            return false;
        }

        #endregion

        #region game loop

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //move to cluster position
            m_prevPos = Position;
            Position = m_virusCapsid.Position;
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
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
//            //m_colSkinVerticies = this.GetLocalSkinWireframe();
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
