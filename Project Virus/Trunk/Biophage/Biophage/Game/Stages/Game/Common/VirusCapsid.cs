using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LNA.GameEngine.Resources.Drawable;
using LNA.GameEngine;
using LNA.GameEngine.Resources;
using LNA.GameEngine.Resources.Playable;
using LNA.GameEngine.Objects.GameObjects;
using LNA.GameEngine.Objects.Scenes;
using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using JigLibX.Math;

namespace Biophage.Game.Stages.Game.Common
{
    public class VirusCapsid : CollidableAsset
    {
        #region fields

        protected ModelResHandle m_capsidModel;

        public Microsoft.Xna.Framework.Vector3 Heading = Microsoft.Xna.Framework.Vector3.Forward;
        public Microsoft.Xna.Framework.Vector3 Up = Microsoft.Xna.Framework.Vector3.Up;
        public Microsoft.Xna.Framework.Vector3 Right = Microsoft.Xna.Framework.Vector3.Right;

        Microsoft.Xna.Framework.Vector3 OldPos = Microsoft.Xna.Framework.Vector3.Zero;

        public Virus m_virusOwner;

        public double remainingLifeSecs = GlobalConstants.GP_CAPSID_LIFESPAN_SECS;

        protected SoundResHandle m_sndInfectUCell;

//#if DEBUG
//        Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;
//        Microsoft.Xna.Framework.Graphics.VertexPositionColor[] m_colSkinVerticies;
//        Microsoft.Xna.Framework.Graphics.BasicEffect m_colSkinBasicEffect;
//#endif

        VirusCapsidBoundary m_virCapsidBoundary;

        Cells.UninfectedCell m_ucellTarget = null;
        float m_ucellLastDist = float.MaxValue;

        #endregion

        #region methods

        #region construction

        public VirusCapsid(uint id, Microsoft.Xna.Framework.Vector3 pos,
                        DebugManager debugMgr, ResourceManager resourceMgr,
                        Virus virusOwner, 
                        Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr)
            : base(id, pos, debugMgr, resourceMgr, (Scene)virusOwner.GetBiophageScn, false)
        {
            m_capsidModel = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Virus\\", "Virus");

            Visible = true;
            Active = true;
            m_virusOwner = virusOwner;

            if (m_virusOwner.virusStateData.isMine || (m_virusOwner.m_sessionDetails.isHost && m_virusOwner.virusStateData.isBot))
                callbackFn += CapsidCallbackFn;

            m_sndInfectUCell = new SoundResHandle(m_debugMgr, resourceMgr, "Content\\Sounds\\", "GameInfectUCell");

//#if DEBUG
//            m_graphicsMgr = graphicsMgr;
//            m_colSkinBasicEffect = new Microsoft.Xna.Framework.Graphics.BasicEffect(
//                m_graphicsMgr.GraphicsDevice, null);
//#endif

            m_virCapsidBoundary = new VirusCapsidBoundary(this, debugMgr, resourceMgr, 
                (Scene)virusOwner.GetBiophageScn, graphicsMgr);
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

                if (m_virusOwner.virusStateData.isMine ||
                    (m_virusOwner.m_sessionDetails.isHost && m_virusOwner.virusStateData.isBot))
                {
                    Visible = true;
                    Active = true;

                    m_physBody = new Body();
                    RemoveAllPrimitives();
                    Owner = m_physBody;

                    //physics
                    m_physBody.CollisionSkin = this;
                    AddPrimitive(new Sphere(Position, 0.1f), (int)MaterialTable.MaterialID.NotBouncyNormal);

                    Microsoft.Xna.Framework.Vector3 com = SetMass(1f);
                    m_physBody.MoveTo(Position, AssetTransform);
                    ApplyLocalTransform(new Transform(-com, AssetTransform));
                    m_physBody.EnableBody();

                    m_physBody.Immovable = true;

                    m_virCapsidBoundary.Init();
                }

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        public override bool Reinit()
        {
            Deinit();

            return Init();
        }

        #region loading

        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if ((!m_capsidModel.Load()) ||
                    (!m_sndInfectUCell.Load()))
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
                if ((!m_capsidModel.Unload()) ||
                    (!m_sndInfectUCell.Unload()))
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
                if (!base.Deinit())
                    retVal = false;

                m_physBody.DisableBody();

                if (m_virCapsidBoundary.IsInit)
                    m_virCapsidBoundary.Deinit();

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

        public bool CapsidCallbackFn(JigLibX.Collision.CollisionSkin skin0, JigLibX.Collision.CollisionSkin skin1)
        {
            //if we identify that the skin is the level, obey the collision.
            if (Active)
            {
                if ((m_virusOwner.virusStateData.isMine) ||
                    (m_virusOwner.m_sessionDetails.isHost && m_virusOwner.virusStateData.isBot))
                {
                    //virus boundary should trap environment collisions

                    //infect the first uninfected cell - IE: make a new cluster of it
                    bool infected = false;
                    Common.Cells.UninfectedCell ucell = null;
                    if (skin0 is Common.Cells.UninfectedCell)
                    {
                        ucell = (Common.Cells.UninfectedCell)skin0;
                        infected = true;
                    }
                    else if (skin1 is Common.Cells.UninfectedCell)
                    {
                        ucell = (Common.Cells.UninfectedCell)skin1;
                        infected = true;
                    }

                    if (infected)
                    {
                        //if host, create and submit new cluster - otherwise tell host
                        if (m_virusOwner.m_sessionDetails.isHost)
                        {
                            m_virusOwner.m_biophageScn.HostCreateNewClusterFromUCell(
                                BiophageGameBaseScn.GobjIDUCell(ucell.Id),
                                BiophageGameBaseScn.GobjIDVirus(Id));
                        }
                        else
                        {
                            //tell host to create me - please, wait for acknowledgment
                            //  -multiplayer is implied
                            m_virusOwner.m_biophageScn.ClientSendNewClusterFromUCell(BiophageGameBaseScn.GobjIDUCell(ucell.Id));
                        }
                        //unactivate self
                        Active = false;

                        //make sound
                        if (m_virusOwner.virusStateData.isMine)
                            m_sndInfectUCell.Play();
                    }
                }
            }

            //indicate that we don't want to generate contact points
            return false;
        }

        #endregion

        #region game loop

        /// <summary>
        /// Only handle input if still alive with no clusters.
        /// </summary>
        public void Input(Microsoft.Xna.Framework.GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            m_debugMgr.Assert(m_virusOwner.virusStateData.isMine,
                "VirusCapsid:Input - bot's shouldn't call Input routine.");

            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;


            // Determine rotation amount from input
            Microsoft.Xna.Framework.Vector2 rotationAmount = -newGPState.ThumbSticks.Right;
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

            // Correct the X axis steering when the virus is upside down
            if (Up.Y < 0)
                rotationAmount.X = -rotationAmount.X;


            // Create rotation matrix from rotation amount
            Microsoft.Xna.Framework.Matrix rotationMatrix =
                Microsoft.Xna.Framework.Matrix.CreateFromAxisAngle(Right, rotationAmount.Y) *
                Microsoft.Xna.Framework.Matrix.CreateRotationY(rotationAmount.X);

            // Rotate orientation vectors
            Heading = Microsoft.Xna.Framework.Vector3.TransformNormal(Heading, rotationMatrix);
            Up = Microsoft.Xna.Framework.Vector3.TransformNormal(Up, rotationMatrix);

            // Re-normalize orientation vectors
            // Without this, the matrix transformations may introduce small rounding
            // errors which add up over time and could destabilize the ship.
            if (Heading.Length() != 0f)
                Heading.Normalize();
            if (Up.Length() != 0f)
                Up.Normalize();

            // Re-calculate Right
            Right = Microsoft.Xna.Framework.Vector3.Cross(Heading, Up);

            // The same instability may cause the 3 orientation vectors may
            // also diverge. Either the Up or Direction vector needs to be
            // re-computed with a cross product to ensure orthagonality
            Up = Microsoft.Xna.Framework.Vector3.Cross(Right, Heading);


            // Determine thrust amount from input
            float thrustAmount = newGPState.ThumbSticks.Left.Y;
#if !XBOX
            if (newKBState.IsKeyDown(Keys.S))
                thrustAmount = -1.0f;
            if (newKBState.IsKeyDown(Keys.W))
                thrustAmount = 1.0f;
#endif

            // Calculate pos delta
            Microsoft.Xna.Framework.Vector3 PosDelta = Heading * (thrustAmount * GlobalConstants.GP_CAPSID_MAXVEL);

            OldPos = Position;
            PositionSimple += PosDelta * elapsed;

            // Reconstruct transforms
            Microsoft.Xna.Framework.Matrix newTrans = Microsoft.Xna.Framework.Matrix.Identity;
            newTrans.Forward = Heading;
            newTrans.Up = Up;
            newTrans.Right = Right;
            AssetTransform = newTrans;
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            remainingLifeSecs -= gameTime.ElapsedRealTime.TotalSeconds;

            if (remainingLifeSecs <= 0.0)
                Active = false;

            //If bot - chase closest ucell
            if (m_virusOwner.m_sessionDetails.isHost && m_virusOwner.virusStateData.isBot)
            {
                if (m_ucellTarget == null)
                    FindNextClosestUCell(null);
                else if (!m_ucellTarget.Active)
                    FindNextClosestUCell(null);
                else
                    ChaseUCell(gameTime);
            }
        }

        private void ChaseUCell(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //modify heading direction
            Microsoft.Xna.Framework.Vector3 headingDir = m_ucellTarget.Position - Position;

            //check that ucell isn't moving away faster than capsid is chasing
            if (headingDir.Length() > m_ucellLastDist)
            {
                //change to another cell type - don't return so that ucell is given a chance
                FindNextClosestUCell(m_ucellTarget);
            }

            m_ucellLastDist = headingDir.Length();

            if (m_ucellLastDist != 0f)
                headingDir.Normalize();

            //don't worry if environment is in the way - others can't see capsid and
            //will achieve goal properly.

            //head closer to target
            Position += (headingDir * (float)(GlobalConstants.GP_CAPSID_MAXVEL * gameTime.ElapsedRealTime.TotalSeconds));
        }

        private void FindNextClosestUCell(Cells.UninfectedCell notThisUCell)
        {
            //this is safe as ucell positions won't change during this read
            // and thie method doesn't change ucells or ucell states.
            Microsoft.Xna.Framework.Vector3 pathTo;
            float lastPathToLength = float.MaxValue;
            foreach (Cells.UninfectedCell uCell in m_virusOwner.m_biophageScn.UninfectCellsList)
            {
                if (m_ucellTarget == null)
                {
                    m_ucellTarget = uCell;
                    pathTo = uCell.Position - Position;
                    lastPathToLength = pathTo.Length();
                }
                else if (uCell != notThisUCell)
                {
                    pathTo = uCell.Position - Position;
                    if (pathTo.Length() < lastPathToLength)
                    {
                        m_ucellTarget = uCell;
                        lastPathToLength = pathTo.Length();
                    }
                }
            }
        }

        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //update boundary
            m_virCapsidBoundary.Animate(gameTime);
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                                    CameraGObj camera)
        {
            //render the virus capsid
            foreach (Microsoft.Xna.Framework.Graphics.ModelMesh mesh in ((Model)m_capsidModel.GetResource).Meshes)
            {
                foreach (Microsoft.Xna.Framework.Graphics.BasicEffect effect in mesh.Effects)
                {
                    //Lighting
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;

                    //load drawspace translation maticies
                    effect.World = m_worldTransform;
                    effect.Projection = camera.ProjectionMatrix;
                    effect.View = camera.ViewMatrix;

                    effect.AmbientLightColor = m_virusOwner.virusStateData.colour.ToVector3();
                }

                mesh.Draw();
            }

//#if DEBUG
//            DrawCollisionSkin(gameTime, graphicsDevice, camera);
//#endif
            if (m_virCapsidBoundary.IsInit)
                m_virCapsidBoundary.Draw(gameTime, graphicsDevice, camera);
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
