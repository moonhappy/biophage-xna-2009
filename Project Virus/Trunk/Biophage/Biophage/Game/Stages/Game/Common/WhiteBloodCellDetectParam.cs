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

namespace Biophage.Game.Stages.Game.Common
{
    public class WhiteBloodCellDetectParam : CollidableAsset
    {
        #region fields

        CellCluster m_cluster;

//#if DEBUG
//        Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;
//        Microsoft.Xna.Framework.Graphics.VertexPositionColor[] m_colSkinVerticies;
//        Microsoft.Xna.Framework.Graphics.BasicEffect m_colSkinBasicEffect;
//#endif

        #endregion

        #region methods

        #region construction

        public WhiteBloodCellDetectParam(CellCluster cluster, DebugManager dbgMgr, ResourceManager resMgr,
            Scene scn)
            : base(uint.MaxValue, cluster.Position, dbgMgr, resMgr, scn, false)
        {
            m_debugMgr.Assert(cluster.GetSessionDetails.isHost,
                "WhiteBloodCellDetectParam:Contructor - only host can provide WBC AI.");

            m_cluster = cluster;

//#if DEBUG
//            m_graphicsMgr = scn.Stage.SceneMgr.Game.GraphicsMgr;
//            m_colSkinBasicEffect = new Microsoft.Xna.Framework.Graphics.BasicEffect(
//                m_graphicsMgr.GraphicsDevice, null);
//#endif

            callbackFn += WhiteBloodCellDetectParam_callbackFn;
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
                    new Sphere(m_cluster.Position, GlobalConstants.GP_WHITEBLOODCELL_ALERT_DIST), 
                    (int)MaterialTable.MaterialID.NotBouncyNormal);

                Microsoft.Xna.Framework.Vector3 com = SetMass(1f);
                m_physBody.MoveTo(Position, AssetTransform);
                ApplyLocalTransform(new Transform(-com, AssetTransform));
                m_physBody.EnableBody();

                m_physBody.Immovable = true;

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

        bool WhiteBloodCellDetectParam_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            //only do something interesting if a cluster is collided
            CellCluster detectedCluster;

            if ((skin0 is CellCluster) && (skin0 != m_cluster))
                detectedCluster = (CellCluster)skin0;
            else if ((skin1 is CellCluster) && (skin1 != m_cluster))
                detectedCluster = (CellCluster)skin1;
            else
                return false;

            //check cluster isn't white blood cell
            if (detectedCluster.stateData.numWhiteBloodCell != 0)
                return false;

            //chase first detected cluster
            if (m_cluster.stateData.actionState != CellActionState.CHASING_ENEMY_TO_BATTLE)
            {
                m_cluster.stateData.actionReObject = detectedCluster;
                m_cluster.stateData.biophageScn.HostClusterChase(m_cluster, m_cluster.stateData.actionReObject);
                m_cluster.PhysBody.Immovable = true;
            }

            //never do physics
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
            Position = m_cluster.Position;
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
