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
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Biophage.Game.Stages.Game.Common
{
    /// <summary>
    /// Represents the environment for the game level - IE model.
    /// </summary>
    public class LevelEnvironment : CollidableAsset
    {
        #region fields

        protected ModelResHandle m_envModel;

        protected TriangleMesh m_physTriangleMesh;
        protected MaterialProperties m_physMaterial;
        protected float m_physMass;

        #endregion

        #region methods

        #region construction

        public LevelEnvironment( uint id, ModelResHandle envModel,
                            DebugManager debugMgr, ResourceManager resMgr, Scene scn,
                            JigLibX.Collision.MaterialProperties phyMaterial,
                            float phyMass, Microsoft.Xna.Framework.Vector3 pos)
            : base(id, pos, debugMgr, resMgr, scn, true)
        {
            //set fields
            m_envModel = envModel;
            m_physMaterial = phyMaterial;
            m_physMass = phyMass;

            m_isVisible = true;
            m_isActive = true;
        }

        #endregion

        #region field accessors

        public ModelResHandle GetModel
        {
            get { return m_envModel; }
        }

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                //base and load (for mesh data)
                if ((!base.Init())||(!Load()))
                    retVal = false;

                m_physBody = new Body();
                RemoveAllPrimitives();
                m_physTriangleMesh = new TriangleMesh();

                //asserts
                m_debugMgr.Assert(m_physBody != null,
                    "LevelEnvironment:Init - 'm_physBody' is null.");

                m_physBody.CollisionSkin = this;

                List<Vector3> vertexList = new List<Vector3>();
                List<TriangleVertexIndices> indexList = new List<TriangleVertexIndices>();
                ExtractData(vertexList, indexList, (Model)m_envModel.GetResource);
                m_physTriangleMesh.CreateMesh(vertexList, indexList, 4, 1f);
                
                AddPrimitive(m_physTriangleMesh, m_physMaterial);

                Microsoft.Xna.Framework.Vector3 com = SetMass(m_physMass);
                m_physBody.MoveTo(Position, AssetTransform);
                ApplyLocalTransform(new Transform(-com, AssetTransform));
                m_physBody.EnableBody();

                //this object is immovable
                m_physBody.Immovable = true;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }
            return retVal;
        }

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
                if (!m_envModel.Load())
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
                if (!m_envModel.Unload())
                    retVal = false;

                if (retVal)
                    m_isLoaded = false;
                else
                    m_isLoaded = true;
            }

            return retVal;
        }

        #endregion

        public override bool Deinit()
        {
            bool retVal = true;
            if (m_isInit)
            {
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

        /// <summary>
        /// Helper Method to get the vertex and index List from the model.
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="indices"></param>
        /// <param name="model"></param>
        public void ExtractData(List<Vector3> vertices, List<TriangleVertexIndices> indices, Model model)
        {
            Matrix[] bones_ = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones_);
            foreach (ModelMesh mm in model.Meshes)
            {
                Matrix xform = bones_[mm.ParentBone.Index];
                foreach (ModelMeshPart mmp in mm.MeshParts)
                {
                    int offset = vertices.Count;
                    Vector3[] a = new Vector3[mmp.NumVertices];
                    mm.VertexBuffer.GetData<Vector3>(mmp.StreamOffset + mmp.BaseVertex * mmp.VertexStride,
                        a, 0, mmp.NumVertices, mmp.VertexStride);
                    for (int i = 0; i != a.Length; ++i)
                        Vector3.Transform(ref a[i], ref xform, out a[i]);
                    vertices.AddRange(a);

                    if (mm.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
                        throw new Exception(
                            String.Format("Model uses 32-bit indices, which are not supported."));
                    short[] s = new short[mmp.PrimitiveCount * 3];
                    mm.IndexBuffer.GetData<short>(mmp.StartIndex * 2, s, 0, mmp.PrimitiveCount * 3);
                    JigLibX.Geometry.TriangleVertexIndices[] tvi = new JigLibX.Geometry.TriangleVertexIndices[mmp.PrimitiveCount];
                    for (int i = 0; i != tvi.Length; ++i)
                    {
                        tvi[i].I0 = s[i * 3 + 2] + offset;
                        tvi[i].I1 = s[i * 3 + 1] + offset;
                        tvi[i].I2 = s[i * 3 + 0] + offset;
                    }
                    indices.AddRange(tvi);
                }
            }
        }

        Microsoft.Xna.Framework.Vector3 SetMass(float mass)
        {
            PrimitiveProperties primitiveProps = new PrimitiveProperties(
                PrimitiveProperties.MassDistributionEnum.Shell,
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

        #endregion

        #region game loop

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            
        }

        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            //render the model
            m_envModel.Draw(gameTime, m_worldTransform, camera.ProjectionMatrix, camera.ViewMatrix);
        }

        #endregion

        #endregion
    }
}
