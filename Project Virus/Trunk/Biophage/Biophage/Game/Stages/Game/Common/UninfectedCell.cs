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

namespace Biophage.Game.Stages.Game.Common.Cells
{
    #region attributes

    public enum CellTypeEnum
    {
        RED_BLOOD_CELL,
        PLATELET,
        BIG_CELL_SILO,
        BIG_CELL_TANK,

        SMALL_HYBRID,
        MED_HYBRID,
        BIG_HYBRID,

        WHITE_BLOOD_CELL
    }

    /// <summary>
    /// Represents a cell's static data/conditions.
    /// </summary>
    public struct CellStaticData
    {
        public CellTypeEnum staticCellType;

        #region thresholds

        public short theshMaxHealth;
        public short threshMaxNStore;
        public short threshNToDivide;
        public short threshMaxBattleOffense;
        public short threshMaxBattleDefence;

        #endregion

        #region rates

        public float rateNutrientIncome;
        public float rateMaxVelocity;

        #endregion
    }

    #endregion

    /// <summary>
    /// Represents an uninfected cell.
    /// </summary>
    public class UninfectedCell : NetworkEntity
    {
        #region fields

        protected CellStaticData    m_cellStaticData;
        protected ModelResHandle    m_cellModel;

        //#if DEBUG
        //Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;
        //Microsoft.Xna.Framework.Graphics.VertexPositionColor[] m_colSkinVerticies;
        //Microsoft.Xna.Framework.Graphics.BasicEffect m_colSkinBasicEffect;
        //#endif

        #endregion

        #region methods

        #region construction

        public UninfectedCell(uint id, CellStaticData staticData,
                                ModelResHandle modelResHandle,
                                DebugManager debugMgr, ResourceManager resourceMgr,
                                Scene scene, bool addToScene,
                                Microsoft.Xna.Framework.Vector3 initPosition,
                                Microsoft.Xna.Framework.Quaternion initOrientation,
                                JigLibX.Geometry.Primitive physPrimitive,
                                JigLibX.Collision.MaterialProperties physMaterialProps,
                                float physMass)
            : base(id, debugMgr, resourceMgr, scene, addToScene,
            physPrimitive, physMaterialProps,
            physMass, initPosition, initOrientation)
        {
            //set fields
            m_cellStaticData = staticData;
            m_cellModel = modelResHandle;

            //unique settings - we only want to render the cell if
            //  it is active. But we only want to render the cell
            //  via the batch process for better efficiency.
            m_isVisible = true;
            m_isActive = true;

//#if DEBUG
//            m_graphicsMgr = scene.Stage.SceneMgr.Game.GraphicsMgr;
//            m_colSkinBasicEffect = new Microsoft.Xna.Framework.Graphics.BasicEffect(
//                m_graphicsMgr.GraphicsDevice, null);
//#endif
        }

        #endregion

        #region field accessors

        public CellStaticData StaticData
        {
            get { return m_cellStaticData; }
        }

        public ModelResHandle Model
        {
            get { return m_cellModel; }
        }

        #endregion

        #region initialisation

        public override bool Reinit()
        {
            if (m_isInit)
                Deinit();

            m_isInit = false;

            return Init();
        }

        #region loading

        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                //load cell
                if (!m_cellModel.Load())
                    retVal = false;

                if (retVal)
                    m_isLoaded = true;
                else
                    m_isLoaded = false;
            }

            return retVal;
        }

        public override bool Unload()
        {
            bool retVal = true;
            if (m_isLoaded)
            {
                //unload cell
                if (!m_cellModel.Unload())
                    retVal = false;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        #endregion

        #region game loop

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            base.Update(gameTime);

            if (networkData.sessionDetails.isHost)
            {
                //server updates
            }
            else
            {
                //client updates
            }
        }

        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (networkData.sessionDetails.isHost)
            {
                //server updates
            }
            else
            {
                //client updates
            }
        }

        //Cells should only be drawn via scene
        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            Model.Draw(gameTime, WorldTransform, camera.ProjectionMatrix, camera.ViewMatrix);

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
