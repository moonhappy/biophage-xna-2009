using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LNA;
using LNA.GameEngine;
using LNA.GameEngine.Core;
using LNA.GameEngine.Core.AsyncTasks;
using LNA.GameEngine.Modules.FuzzyLogic;
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
    #region attributes

    public struct ClusterStateData
    {
        #region game data

        public byte virusOwnerId;
        public Virus virusRef;

        public short attrHealth;
        public short attrNutrientStore;

        public float attrVelocity;
        public float attrNIncome;   //nut' per sec

        public byte numRBCs;
        public byte numPlatelets;
        public byte numSilos;
        public byte numTanks;
        public byte numSmallHybrids;    //hybrid of RBC & Platelet
        public byte numMediumHybrids;   //hybrid of RBC:Silo, RBC:Tank, PC:Silo, or PC:Tank
        public byte numBigHybrids;      //hybrid of Silo:Tank

        public byte numWhiteBloodCell; //only has one

        public short numCellsTotal;

        public BiophageGameBaseScn biophageScn;

        #endregion

        #region thresholds

        public short maxNutrientStore;
        public short maxBattleOffence;
        public short maxBattleDefence;
        public short maxHealth;
        public float maxVelocity;

        #endregion

        #region action state

        public CellActionState actionState;
        public NetworkEntity actionReObject;

        public bool attnUnderAttack;
        public CellCluster attnAttackingEnemy;

        #endregion
    }

    public struct ClusterModelContainer
    {
        public Microsoft.Xna.Framework.Vector3 mdlPosition;
        public Microsoft.Xna.Framework.Vector3 mdlUpDir;

        public ModelResHandle mdlResHandle;
    }

    public struct CellStack
    {
        public List<Microsoft.Xna.Framework.Vector3> positions;
        public float width;
        public float height;
        public float depth;
    }

    public enum CellActionState
    {
        IDLE,
        WAITING_FOR_ORDER, //IE: player is using the cursor
        WAITING_WITH_MY_CLUSTER_SELECTED,
        WAITING_WITH_ENEMY_CLUSTER_SELECTED,
        WAITING_WITH_UCELL_SELECTED,
        CHASING_UCELL_TOINFECT,
        CHASING_ENEMY_TO_BATTLE,
        CHASING_CLUST_TO_COMBINE,
        EVADING_ENEMY
    }

    public class ClusterAIdataset
    {
        #region logic variables

        public double cl_atckingenem_power =    double.MaxValue;
        public double cl_clst_enem_power =      double.MaxValue;
        public double cl_my_power =             0.0;

        public double cl_dist_atck_enem =       double.MaxValue;
        public double cl_dist_clst_enem =       double.MaxValue;
        public double cl_dist_clst_friend =     double.MaxValue;
        public double cl_dist_clst_rbc =        double.MaxValue;
        public double cl_dist_clst_plt =        double.MaxValue;
        public double cl_dist_clst_tnk =        double.MaxValue;
        public double cl_dist_clst_sil =        double.MaxValue;

        public double cl_rbc_divcount =         0.0;
        public double cl_plt_divcount =         0.0;
        public double cl_tnk_divcount =         0.0;
        public double cl_sil_divcount =         0.0;

        #endregion

        #region regarding object references

        public CellCluster cl_re_clst_enem =            null;
        public CellCluster cl_re_clst_friend =          null;

        public Cells.UninfectedCell cl_re_clst_rbc =    null;
        public Cells.UninfectedCell cl_re_clst_plt =    null;
        public Cells.UninfectedCell cl_re_clst_tnk =    null;
        public Cells.UninfectedCell cl_re_clst_sil =    null;

        #endregion
    }

    public enum ClusterAIReqestAction
    {
        NONE,

        BATTLE_ATCK_ENEM,   //RE: statedata.attnAttackingEnemy
        EVADE_ATCK_ENEM,    //RE: statedata.attnAttackingEnemy
        BATTLE_CLST_ENEM,   //RE: cl_re_clst_enem

        DIVCELLS_ANY,   //RE: the most benefacting
        DIVCELLS_RBC,   //RE: RedBloodCell
        DIVCELLS_PLT,   //RE: Platelet
        DIVCELLS_TNK,   //RE: Tank
        DIVCELLS_SIL,   //RE: Silo

        CHASE_RBC,  //RE: cl_re_clst_rbc
        CHASE_PLT,  //RE: cl_re_clst_plt
        CHASE_TNK,  //RE: cl_re_clst_tnk
        CHASE_SIL,  //RE: cl_re_clst_sil
    }

    public enum VirusOverrideRequestAction
    {
        NONE,
        VIR_CL_SPLIT,       //deterministic split
        VIR_CL_COMBINE,     //RE: cl_re_clst_friend
        VIR_CL_HYBRID,      //deterministic hybrid
        VIR_CL_KAMIKAZE     //RE: cl_re_clst_enem
    }

    #endregion

    #region predicates

    /// <summary>
    /// Predicate to ignore cursor as intersectable.
    /// </summary>
    public class CursorSkinPredicate : CollisionSkinPredicate1
    {
        public override bool ConsiderSkin(CollisionSkin skin0)
        {
            if ((skin0 is PlayerCursor) || (skin0 is WhiteBloodCellDetectParam))
                return false;
            else
                return true;
        }
    }

    /// <summary>
    /// Predicate to ignore all but environment.
    /// </summary>
    public class EnvironmentPredicate : CollisionSkinPredicate1
    {
        public override bool ConsiderSkin(CollisionSkin skin0)
        {
            if (skin0 is LevelEnvironment)
                return true;
            else
                return false;
        }
    }

    #endregion

    /// <summary>
    /// Represents a cluster of infected cells.
    /// </summary>
    public class CellCluster : NetworkEntity
    {
        #region fields

        public ClusterStateData stateData;
        public ClusterAI m_clusterAI;

        #region sounds

        protected SoundResHandle m_sndDivideCells;
        protected SoundResHandle m_sndInfectUCell;
        protected SoundResHandle m_sndHybridCells;

        #endregion

        protected Microsoft.Xna.Framework.Vector3 m_prevPos;

        #region dynamic model

        //have a handle for each model res
        protected ModelResHandle m_modelRBC;
        protected ModelResHandle m_modelPlatelet;
        protected ModelResHandle m_modelBigCellSilo;
        protected ModelResHandle m_modelBigCellTank;

        protected ModelResHandle m_modelSmallHybrid;
        protected ModelResHandle m_modelMediumHybrid;
        protected ModelResHandle m_modelBigHybrid;

        protected ModelResHandle m_modelWhiteBloodCell;

        protected SortedList<float, ClusterModelContainer> m_clusterDynModel;

        public Microsoft.Xna.Framework.Vector3 Heading = Microsoft.Xna.Framework.Vector3.Forward;
        public Microsoft.Xna.Framework.Vector3 Up = Microsoft.Xna.Framework.Vector3.Up;
        public Microsoft.Xna.Framework.Vector3 Right = Microsoft.Xna.Framework.Vector3.Right;

        Microsoft.Xna.Framework.Vector3 OldPos = Microsoft.Xna.Framework.Vector3.Zero;

        Microsoft.Xna.Framework.Graphics.Color m_ambientColour;
        Microsoft.Xna.Framework.Graphics.Color m_diffuseColour;
        Microsoft.Xna.Framework.Graphics.Color m_specularColour;

        #endregion

        #region camera and debug data

        float m_camDistance = 10f;
        public float CameraDistance
        {
            get { return m_camDistance; }
        }
        bool m_canHybreed = false;
        public bool CanHybreed
        {
            get { return m_canHybreed; }
        }
        double m_nutrientStoreOffset = 0f;

        protected FollowCamera m_cam;

        CursorSkinPredicate m_skinCursorPredicate = new CursorSkinPredicate();
        EnvironmentPredicate m_skinEnvironment = new EnvironmentPredicate();

        WhiteBloodCellDetectParam m_wbcDetectParameter = null;
        ResourceManager m_resMgr;
        public HUDOverlay m_hud;
        public bool doSafeModelReadjust = true;

//#if DEBUG
//        Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;
//        Microsoft.Xna.Framework.Graphics.VertexPositionColor[] m_colSkinVerticies;
//        Microsoft.Xna.Framework.Graphics.BasicEffect m_colSkinBasicEffect;
//#endif

        #endregion

        #endregion

        #region methods

        #region construction

        /// <summary>
        /// Clusters are dynamically created, so the cluster state and the
        /// session detail data can be assigned.
        /// </summary>
        /// <remarks>
        /// Dead clusters should be recycled, so that the cluster id range can always fit into
        /// the byte size field.
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="clusterState"></param>
        /// <param name="debugMgr"></param>
        /// <param name="resourceMgr"></param>
        /// <param name="parentScn"></param>
        public CellCluster(uint id, ClusterStateData clusterState,
                            Microsoft.Xna.Framework.Vector3 initPos, SessionDetails sesDetails,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            BiophageGameBaseScn parentScn, HUDOverlay hud)
            : base(id, debugMgr, resourceMgr, parentScn, true,
            new Box(Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Matrix.Identity,
                Microsoft.Xna.Framework.Vector3.One),
            new MaterialProperties(1f, 1f, 1f), 1f,
            initPos, Microsoft.Xna.Framework.Quaternion.Identity)
        {
            stateData = clusterState;
            SetSessionDetails(sesDetails); //this is called because cell clusters are dynamically added
            m_cam = (FollowCamera)parentScn.Camera;
            m_prevPos = initPos;

            m_resMgr = resourceMgr;
            m_hud = hud;

            m_sndDivideCells = new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "GameDivideCells");
            m_sndInfectUCell = new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "GameInfectUCell");
            m_sndHybridCells = new SoundResHandle(m_debugMgr, m_resMgr, "Content\\Sounds\\", "GameHybridCells");

//#if DEBUG
//            m_graphicsMgr = parentScn.Stage.SceneMgr.Game.GraphicsMgr;
//            m_colSkinBasicEffect = new Microsoft.Xna.Framework.Graphics.BasicEffect(
//                m_graphicsMgr.GraphicsDevice, null);
//#endif

            ReadjustMaximums();

            #region models

            m_modelRBC = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\RedBloodCell\\", "RedBloodCell");
            m_modelPlatelet = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Platelet\\", "Platelet");
            m_modelBigCellSilo = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Silo\\", "Silo");
            m_modelBigCellTank = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Tank\\", "Tank");

            m_modelSmallHybrid = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Hybrids\\", "SmallHybrid");
            m_modelMediumHybrid = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Hybrids\\", "MediumHybrid");
            m_modelBigHybrid = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\Hybrids\\", "BigHybrid");

            m_modelWhiteBloodCell = new ModelResHandle(
                debugMgr, resourceMgr,
                "Content\\Models\\WhiteBloodCell\\", "WhiteBloodCell");

            #endregion

            #region init stuff

            m_clusterDynModel = new SortedList<float, ClusterModelContainer>((int)stateData.numCellsTotal + 1,
                new FloatReverse());

            Visible = true;

            //cluster colour properties
            if (stateData.numWhiteBloodCell == 0)
            {
                m_ambientColour = Microsoft.Xna.Framework.Graphics.Color.DarkSlateGray;
                m_diffuseColour = clusterState.virusRef.virusStateData.colour;
                m_specularColour = Microsoft.Xna.Framework.Graphics.Color.WhiteSmoke;
            }
            else
            {
                m_ambientColour = Microsoft.Xna.Framework.Graphics.Color.Gray;
                m_diffuseColour = Microsoft.Xna.Framework.Graphics.Color.WhiteSmoke;
                m_specularColour = Microsoft.Xna.Framework.Graphics.Color.White;

                if (networkData.sessionDetails.isHost)
                    m_wbcDetectParameter = new WhiteBloodCellDetectParam(this, m_debugMgr, m_resMgr, parentScn);
            }

            ReadjustVelocity();

            //test if cluster can hybrid
            ReadjustHybridCapability();

            Init();
            Load();

            callbackFn += CellCluster_callbackFn;

            m_clusterAI = new ClusterAI(m_debugMgr, this);

            #endregion
        }

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                m_isInit = true;

                m_physBody = new Body();
                RemoveAllPrimitives();
                Owner = m_physBody;

                //asserts
                m_debugMgr.Assert(m_physBody != null,
                    "CellCluster:Init - 'm_physBody' is null.");

                //physics
                m_physBody.CollisionSkin = this;

                if (m_wbcDetectParameter != null)
                    m_wbcDetectParameter.Init();

                ReadjustAll();

                m_prevPos = Position;
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
                if ((!m_modelRBC.Load()) ||
                        (!m_modelPlatelet.Load()) ||
                        (!m_modelBigCellSilo.Load()) ||
                        (!m_modelBigCellTank.Load()) ||
                        (!m_modelSmallHybrid.Load()) ||
                        (!m_modelMediumHybrid.Load()) ||
                        (!m_modelBigHybrid.Load()) ||
                        (!m_modelWhiteBloodCell.Load()) ||

                        (!m_sndDivideCells.Load()) ||
                        (!m_sndInfectUCell.Load()) ||
                    (!m_sndHybridCells.Load()))
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
                if ((!m_modelRBC.Unload()) ||
                        (!m_modelPlatelet.Unload()) ||
                        (!m_modelBigCellSilo.Unload()) ||
                        (!m_modelBigCellTank.Unload()) ||
                        (!m_modelSmallHybrid.Unload()) ||
                        (!m_modelMediumHybrid.Unload()) ||
                        (!m_modelBigHybrid.Unload()) ||
                        (!m_modelWhiteBloodCell.Unload()) ||
                    
                        (!m_sndDivideCells.Unload()) ||
                        (!m_sndInfectUCell.Unload()) ||
                    (!m_sndHybridCells.Unload()))
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
                if (m_wbcDetectParameter != null)
                    m_wbcDetectParameter.Deinit();

                if (!base.Deinit())
                    retVal = false;
            }
            return retVal;
        }

        #endregion

        #region state updates

        #region readjusts

        public void ReadjustAll()
        {
            ReadjustMaximums();

            if (stateData.numCellsTotal == 0)
                Active = false;

            if (Active)
            {
                ReadjustHybridCapability();
                ReadjustVelocity();
                doSafeModelReadjust = true;
            }
        }

        public void ReadjustMaximums()
        {
            if (stateData.numWhiteBloodCell != 0)
            {
                stateData.numCellsTotal = 1;
                stateData.maxNutrientStore = 0;
                stateData.maxBattleOffence = GlobalConstants.WBC_threshMaxBattleOffence;
                stateData.maxBattleDefence = GlobalConstants.WBC_threshMaxBattleDefence;
                stateData.attrNIncome = 0.0f;
                stateData.maxHealth = 100;
                stateData.maxVelocity = GlobalConstants.WBC_rateMaxVelocity;
            }
            else
            {
                stateData.numCellsTotal = (short)
                    (stateData.numRBCs + stateData.numPlatelets + stateData.numSilos + stateData.numTanks +
                    stateData.numSmallHybrids + stateData.numMediumHybrids + stateData.numBigHybrids);

                stateData.maxNutrientStore = (short)
                        ((stateData.numRBCs * GlobalConstants.RBC_threshMaxNStore) +
                        (stateData.numPlatelets * GlobalConstants.PLATELET_threshMaxNStore) +
                        (stateData.numSilos * GlobalConstants.SILO_threshMaxNStore) +
                        (stateData.numTanks * GlobalConstants.TANK_threshMaxNStore) +
                        (stateData.numSmallHybrids * GlobalConstants.HYSMALL_threshMaxNStore) +
                        (stateData.numMediumHybrids * GlobalConstants.HYMED_threshMaxNStore) +
                        (stateData.numBigHybrids * GlobalConstants.HYBIG_threshMaxNStore));

                stateData.maxBattleOffence = (short)
                    ((stateData.numRBCs * GlobalConstants.RBC_threshMaxBattleOffence) +
                    (stateData.numPlatelets * GlobalConstants.PLATELET_threshMaxBattleOffence) +
                    (stateData.numSilos * GlobalConstants.SILO_threshMaxBattleOffence) +
                    (stateData.numTanks * GlobalConstants.TANK_threshMaxBattleOffence) +
                    (stateData.numSmallHybrids * GlobalConstants.HYSMALL_threshMaxBattleOffence) +
                    (stateData.numMediumHybrids * GlobalConstants.HYMED_threshMaxBattleOffence) +
                    (stateData.numBigHybrids * GlobalConstants.HYBIG_threshMaxBattleOffence));

                stateData.maxBattleDefence = (short)
                    ((stateData.numRBCs * GlobalConstants.RBC_threshMaxBattleDefence) +
                    (stateData.numPlatelets * GlobalConstants.PLATELET_threshMaxBattleDefence) +
                    (stateData.numSilos * GlobalConstants.SILO_threshMaxBattleDefence) +
                    (stateData.numTanks * GlobalConstants.TANK_threshMaxBattleDefence) +
                    (stateData.numSmallHybrids * GlobalConstants.HYSMALL_threshMaxBattleDefence) +
                    (stateData.numMediumHybrids * GlobalConstants.HYMED_threshMaxBattleDefence) +
                    (stateData.numBigHybrids * GlobalConstants.HYBIG_threshMaxBattleDefence));

                stateData.attrNIncome = (float)
                    ((stateData.numRBCs * GlobalConstants.RBC_rateNutrientIncome) +
                    (stateData.numPlatelets * GlobalConstants.PLATELET_rateNutrientIncome) +
                    (stateData.numTanks * GlobalConstants.TANK_rateNutrientIncome) +
                    (stateData.numSilos * GlobalConstants.SILO_rateNutrientIncome) +
                    (stateData.numSmallHybrids * GlobalConstants.HYSMALL_rateNutrientIncome) +
                    (stateData.numMediumHybrids * GlobalConstants.HYMED_rateNutrientIncome) +
                    (stateData.numBigHybrids * GlobalConstants.HYBIG_rateNutrientIncome));

                stateData.maxHealth = (short)
                    (stateData.numCellsTotal * 100);

                stateData.maxVelocity = (float)
                    (((GlobalConstants.RBC_rateMaxVelocity * stateData.numRBCs) +
                    (GlobalConstants.PLATELET_rateMaxVelocity * stateData.numPlatelets) +
                    (GlobalConstants.SILO_rateMaxVelocity * stateData.numSilos) +
                    (GlobalConstants.TANK_rateMaxVelocity * stateData.numTanks) +
                    (GlobalConstants.HYSMALL_rateMaxVelocity * stateData.numSmallHybrids) +
                    (GlobalConstants.HYMED_rateMaxVelocity * stateData.numMediumHybrids) +
                    (GlobalConstants.HYBIG_rateMaxVelocity * stateData.numBigHybrids))
                    / stateData.numCellsTotal);
            }
        }

        //faster implementation of the linked list version
        public void ReadjustHybridCapability()
        {
            m_canHybreed = false;
            if (stateData.numWhiteBloodCell != 0)
                return;

            int avalCount = 0;
            if (stateData.numRBCs > 0)
                avalCount++;
            if (stateData.numPlatelets > 0)
                avalCount++;
            if (stateData.numSilos > 0)
                avalCount++;
            if (stateData.numTanks > 0)
                avalCount++;

            if (avalCount > 1)
                m_canHybreed = true;
        }

        public void ReadjustHybridCapability(out LinkedList<Cells.CellTypeEnum> avals)
        {
            m_canHybreed = false;
            avals = new LinkedList<Cells.CellTypeEnum>();

            if (stateData.numWhiteBloodCell != 0)
                return;

            if (stateData.numRBCs > 0)
                avals.AddLast(Cells.CellTypeEnum.RED_BLOOD_CELL);

            if (stateData.numPlatelets > 0)
                avals.AddLast(Cells.CellTypeEnum.PLATELET);

            if (stateData.numSilos > 0)
                avals.AddLast(Cells.CellTypeEnum.BIG_CELL_SILO);

            if (stateData.numTanks > 0)
                avals.AddLast(Cells.CellTypeEnum.BIG_CELL_TANK);

            if (avals.Count > 1)
                m_canHybreed = true;
        }

        public void ReadjustHybridCapability(out LinkedList<string> avalsStr)
        {
            m_canHybreed = false;
            avalsStr = new LinkedList<string>();

            if (stateData.numWhiteBloodCell != 0)
                return;

            if (stateData.numRBCs > 0)
                avalsStr.AddLast("Red Blood Cell");

            if (stateData.numPlatelets > 0)
                avalsStr.AddLast("Platelet");

            if (stateData.numTanks > 0)
                avalsStr.AddLast("Tank Cell");

            if (stateData.numSilos > 0)
                avalsStr.AddLast("Silo Cell");

            if (avalsStr.Count > 1)
                m_canHybreed = true;
        }

        public void ReadjustVelocity()
        {
            //each cell has 100 health points if healthy, we can lower the
            //  velocity average calculated by the lower health value.

            if (stateData.numWhiteBloodCell != 0)
            {
                m_debugMgr.Assert(stateData.numCellsTotal == 1,
                    "CellCluster:ReadjustVelocity - white blood cell cluster should only contain one white blood cell.");
                stateData.attrVelocity = GlobalConstants.WBC_rateMaxVelocity;
            }
            else
            {
                //should be dead if zero cell count
                m_debugMgr.Assert(stateData.numCellsTotal > 0f,
                    "CellCluster:ReadjustVelocity - cell count is zero or less, cluster should be dead.");

                double velocityHealthDelta = ((double)stateData.attrHealth / (double)stateData.maxHealth);

                //adjust for health ratio
                stateData.attrVelocity = (float)(stateData.maxVelocity * velocityHealthDelta);
            }
        }

        /// <summary>
        /// Call for any add/remove of cells.
        /// </summary>
        public void ReadjustClusterModel()
        {
            if (!IsInit)
                Init();

            doSafeModelReadjust = false;

            //clear sorted list - and body primitives
            //m_physBody.DisableBody();
            m_clusterDynModel.Clear();
            RemoveAllPrimitives();
            m_camDistance = 0f;
            float uniqueDist = 0f;
            ClusterModelContainer cmdlContainer;

            if (stateData.numWhiteBloodCell == 0)
            {
                #region create cell stacks
                CellStack csRBCs = MakeCellStack(
                    stateData.numRBCs,
                    GlobalConstants.RBC_WIDTH,
                    GlobalConstants.RBC_DEPTH,
                    GlobalConstants.RBC_HEIGHT);
                CellStack csPBCs = MakeCellStack(
                    stateData.numPlatelets,
                    GlobalConstants.PLATELET_WIDTH,
                    GlobalConstants.PLATELET_DEPTH,
                    GlobalConstants.PLATELET_HEIGHT);
                CellStack csSilos = MakeCellStack(
                    stateData.numSilos,
                    GlobalConstants.SILO_WIDTH,
                    GlobalConstants.SILO_DEPTH,
                    GlobalConstants.SILO_HEIGHT);
                CellStack csTanks = MakeCellStack(
                    stateData.numTanks,
                    GlobalConstants.TANK_WIDTH,
                    GlobalConstants.TANK_DEPTH,
                    GlobalConstants.TANK_HEIGHT);

                CellStack csSHy = MakeCellStack(
                    stateData.numSmallHybrids,
                    GlobalConstants.HYSMALL_WIDTH,
                    GlobalConstants.HYSMALL_DEPTH,
                    GlobalConstants.HYSMALL_HEIGHT);
                CellStack csMHy = MakeCellStack(
                    stateData.numMediumHybrids,
                    GlobalConstants.HYMED_WIDTH,
                    GlobalConstants.HYMED_DEPTH,
                    GlobalConstants.HYMED_HEIGHT);
                CellStack csBHy = MakeCellStack(
                    stateData.numBigHybrids,
                    GlobalConstants.HYBIG_WIDTH,
                    GlobalConstants.HYBIG_DEPTH,
                    GlobalConstants.HYBIG_HEIGHT);
                #endregion

                //x axis
                float absSide = 0f;

                //cam distance
                m_camDistance = csTanks.height + csMHy.height + csSilos.height + csBHy.height;

                #region transform smalls

                if (csRBCs.positions != null)
                {
                    Microsoft.Xna.Framework.Matrix matRBCs = Microsoft.Xna.Framework.Matrix
                    .CreateTranslation(
                    (GlobalConstants.RBC_WIDTH - csRBCs.width) / 2f,
                    (-GlobalConstants.RBC_HEIGHT - csPBCs.height) / 2f,
                    (GlobalConstants.RBC_DEPTH - csRBCs.depth) / 2f);
                    foreach (Microsoft.Xna.Framework.Vector3 cellPos in csRBCs.positions)
                    {
                        cmdlContainer = new ClusterModelContainer();
                        cmdlContainer.mdlResHandle = m_modelRBC;
                        cmdlContainer.mdlPosition = Microsoft.Xna.Framework.Vector3.Transform(cellPos, matRBCs);
                        cmdlContainer.mdlUpDir = Microsoft.Xna.Framework.Vector3.Down;
                        uniqueDist += 0.0000125f;
                        if (float.IsNaN(uniqueDist))
                            uniqueDist = 1f;
                        m_clusterDynModel.Add(uniqueDist, cmdlContainer);

                        AddPrimitive(new Sphere(
                            cmdlContainer.mdlPosition,
                            1f), (int)MaterialTable.MaterialID.NotBouncyNormal);
                    }
                }

                absSide = (csRBCs.width);


                if (csPBCs.positions != null)
                {
                    Microsoft.Xna.Framework.Matrix matPBCs = Microsoft.Xna.Framework.Matrix
                        .CreateTranslation(
                        (GlobalConstants.PLATELET_WIDTH - csPBCs.width) / 2f,
                        0f,
                        (GlobalConstants.PLATELET_DEPTH - csPBCs.depth) / 2f);
                    foreach (Microsoft.Xna.Framework.Vector3 cellPos in csPBCs.positions)
                    {
                        cmdlContainer = new ClusterModelContainer();
                        cmdlContainer.mdlResHandle = m_modelPlatelet;
                        cmdlContainer.mdlPosition = Microsoft.Xna.Framework.Vector3.Transform(cellPos, matPBCs);
                        cmdlContainer.mdlUpDir = Microsoft.Xna.Framework.Vector3.Up;
                        uniqueDist += 0.0000125f;
                        if (float.IsNaN(uniqueDist))
                            uniqueDist = 1f;
                        m_clusterDynModel.Add(uniqueDist, cmdlContainer);

                        AddPrimitive(new Sphere(
                            cmdlContainer.mdlPosition,
                            1f), (int)MaterialTable.MaterialID.NotBouncyNormal);
                    }
                }

                absSide = Math.Max(absSide, csPBCs.width);


                if (csSHy.positions != null)
                {
                    Microsoft.Xna.Framework.Matrix matSHYs = Microsoft.Xna.Framework.Matrix
                        .CreateTranslation(
                        (GlobalConstants.HYSMALL_WIDTH - csSHy.width) / 2f,
                        (csPBCs.height + GlobalConstants.HYSMALL_HEIGHT) / 2f,
                        (GlobalConstants.HYSMALL_DEPTH - csSHy.depth) / 2f);
                    foreach (Microsoft.Xna.Framework.Vector3 cellPos in csSHy.positions)
                    {
                        cmdlContainer = new ClusterModelContainer();
                        cmdlContainer.mdlResHandle = m_modelSmallHybrid;
                        cmdlContainer.mdlPosition = Microsoft.Xna.Framework.Vector3.Transform(cellPos, matSHYs);
                        cmdlContainer.mdlUpDir = Microsoft.Xna.Framework.Vector3.Up;
                        uniqueDist += 0.0000125f;
                        if (float.IsNaN(uniqueDist))
                            uniqueDist = 1f;
                        m_clusterDynModel.Add(uniqueDist, cmdlContainer);

                        AddPrimitive(new Sphere(
                            cmdlContainer.mdlPosition,
                            1f), (int)MaterialTable.MaterialID.NotBouncyNormal);
                    }
                }

                absSide = Math.Max(absSide, csSHy.width);

                //camera
                m_camDistance += absSide;
                m_camDistance *= 5f;//length modifier

                #endregion

                #region transform mediums

                Microsoft.Xna.Framework.Matrix matMedsGen = Microsoft.Xna.Framework.Matrix
                    .CreateRotationZ(-Microsoft.Xna.Framework.MathHelper.Pi) * Microsoft.Xna.Framework.Matrix
                    .CreateTranslation(0f, absSide, 0f);

                if (csTanks.positions != null)
                {
                    Microsoft.Xna.Framework.Matrix matTanks = Microsoft.Xna.Framework.Matrix
                    .CreateTranslation(
                    (GlobalConstants.TANK_WIDTH - csTanks.width) / 2f,
                    0f,
                    (GlobalConstants.TANK_DEPTH - csTanks.depth) / 2f) *
                    matMedsGen;
                    foreach (Microsoft.Xna.Framework.Vector3 cellPos in csTanks.positions)
                    {
                        cmdlContainer = new ClusterModelContainer();
                        cmdlContainer.mdlResHandle = m_modelBigCellTank;
                        cmdlContainer.mdlPosition = Microsoft.Xna.Framework.Vector3.Transform(cellPos, matTanks);
                        cmdlContainer.mdlUpDir = Microsoft.Xna.Framework.Vector3.Left;
                        uniqueDist += 0.0000125f;
                        if (float.IsNaN(uniqueDist))
                            uniqueDist = 1f;
                        m_clusterDynModel.Add(uniqueDist, cmdlContainer);

                        AddPrimitive(new Sphere(
                            cmdlContainer.mdlPosition,
                            2f),
                            (int)MaterialTable.MaterialID.NotBouncyNormal);
                    }
                }

                absSide = csTanks.width;


                if (csMHy.positions != null)
                {
                    Microsoft.Xna.Framework.Matrix matMedHy = Microsoft.Xna.Framework.Matrix
                    .CreateTranslation(
                    (GlobalConstants.HYMED_WIDTH - csMHy.width) / 2f,
                    csTanks.height + (GlobalConstants.HYMED_HEIGHT / 2f),
                    (GlobalConstants.HYMED_DEPTH - csMHy.depth) / 2f) *
                    matMedsGen;
                    foreach (Microsoft.Xna.Framework.Vector3 cellPos in csMHy.positions)
                    {
                        cmdlContainer = new ClusterModelContainer();
                        cmdlContainer.mdlResHandle = m_modelMediumHybrid;
                        cmdlContainer.mdlPosition = Microsoft.Xna.Framework.Vector3.Transform(cellPos, matMedHy);
                        cmdlContainer.mdlUpDir = Microsoft.Xna.Framework.Vector3.Left;
                        uniqueDist += 0.0000125f;
                        if (float.IsNaN(uniqueDist))
                            uniqueDist = 1f;
                        m_clusterDynModel.Add(uniqueDist, cmdlContainer);

                        AddPrimitive(new Sphere(
                            cmdlContainer.mdlPosition,
                            2f),
                            (int)MaterialTable.MaterialID.NotBouncyNormal);
                    }
                }

                absSide = Math.Max(absSide, csMHy.width);

                #endregion

                #region transform bigs

                Microsoft.Xna.Framework.Matrix matBigsGen = Microsoft.Xna.Framework.Matrix
                    .CreateRotationZ(Microsoft.Xna.Framework.MathHelper.Pi) * Microsoft.Xna.Framework.Matrix
                    .CreateTranslation(0f, absSide, 0f);

                if (csSilos.positions != null)
                {
                    Microsoft.Xna.Framework.Matrix matSilo = Microsoft.Xna.Framework.Matrix
                        .CreateTranslation(
                        (GlobalConstants.SILO_DEPTH - csSilos.width) / 2f,
                        0f,
                        (GlobalConstants.SILO_HEIGHT - csSilos.depth) / 2f) *
                        matBigsGen;
                    foreach (Microsoft.Xna.Framework.Vector3 cellPos in csSilos.positions)
                    {
                        cmdlContainer = new ClusterModelContainer();
                        cmdlContainer.mdlResHandle = m_modelBigCellSilo;
                        cmdlContainer.mdlPosition = Microsoft.Xna.Framework.Vector3.Transform(cellPos, matSilo);
                        cmdlContainer.mdlUpDir = Microsoft.Xna.Framework.Vector3.Right;
                        uniqueDist += 0.0000125f;
                        if (float.IsNaN(uniqueDist))
                            uniqueDist = 1f;
                        m_clusterDynModel.Add(uniqueDist, cmdlContainer);

                        AddPrimitive(new Sphere(
                            cmdlContainer.mdlPosition,
                            4f),
                            (int)MaterialTable.MaterialID.NotBouncyNormal);
                    }
                }

                absSide = csSilos.width;

                if (csBHy.positions != null)
                {
                    Microsoft.Xna.Framework.Matrix matBHy = Microsoft.Xna.Framework.Matrix
                        .CreateTranslation(
                        (GlobalConstants.HYBIG_WIDTH - csBHy.width) / 2f,
                        csSilos.height + (GlobalConstants.HYBIG_HEIGHT / 2f),
                        (GlobalConstants.HYBIG_DEPTH - csBHy.depth) / 2f) *
                        matBigsGen;
                    foreach (Microsoft.Xna.Framework.Vector3 cellPos in csBHy.positions)
                    {
                        cmdlContainer = new ClusterModelContainer();
                        cmdlContainer.mdlResHandle = m_modelBigHybrid;
                        cmdlContainer.mdlPosition = Microsoft.Xna.Framework.Vector3.Transform(cellPos, matBHy);
                        cmdlContainer.mdlUpDir = Microsoft.Xna.Framework.Vector3.Right;
                        uniqueDist += 0.0000125f;
                        if (float.IsNaN(uniqueDist))
                            uniqueDist = 1f;
                        m_clusterDynModel.Add(uniqueDist, cmdlContainer);

                        AddPrimitive(new Sphere(
                            cmdlContainer.mdlPosition,
                            4f),
                            (int)MaterialTable.MaterialID.NotBouncyNormal);
                    }
                }

                absSide = Math.Max(absSide, csBHy.width);

                #endregion
            }
            else
            {
                cmdlContainer = new ClusterModelContainer();
                cmdlContainer.mdlResHandle = m_modelWhiteBloodCell;
                cmdlContainer.mdlPosition = Microsoft.Xna.Framework.Vector3.Zero;
                cmdlContainer.mdlUpDir = Microsoft.Xna.Framework.Vector3.Up;
                uniqueDist += 0.0000125f;
                if (float.IsNaN(uniqueDist))
                    uniqueDist = 1f;
                m_clusterDynModel.Add(uniqueDist, cmdlContainer);

                //white blood cell physics primitive
                AddPrimitive(
                    new Sphere(Microsoft.Xna.Framework.Vector3.Zero, 5f),
                    (int)MaterialTable.MaterialID.NotBouncyNormal);
            }

            m_physBody.MoveTo(Position, ModelTransform);
            m_physBody.EnableBody();
            if (m_wbcDetectParameter == null)
                m_physBody.Immovable = true;
        }

        #endregion

        /// <summary>
        /// Produces a 3D stack of cells in 'unadjusted' model bone transform.
        /// </summary>
        CellStack MakeCellStack(byte numCells, float cellWidth, float cellDepth, float cellHeight)
        {
            CellStack cs = new CellStack();
            if (numCells == 0)
            {
                cs.positions = null;
                return cs;
            }

            cs.positions = new List<Microsoft.Xna.Framework.Vector3>((int)numCells);

            //determine the height/width/depth side lengths 'which are roughly equal'
            int sideLen = (int)(Math.Pow((double)numCells, (double)(1f / 3f)));

            cs.width = sideLen * cellWidth;
            cs.height = sideLen * cellHeight;
            cs.depth = sideLen * cellDepth;

            Microsoft.Xna.Framework.Vector3 nextPos = Microsoft.Xna.Framework.Vector3.Zero;

            for (int i = 0; i < numCells; ++i)
            {
                cs.positions.Add(nextPos);

                nextPos.X += cellWidth;
                if (nextPos.X == cs.width)
                {
                    nextPos.X = 0f;
                    nextPos.Z += cellDepth;
                    if (nextPos.Z == cs.depth)
                    {
                        nextPos.Z = 0f;
                        nextPos.Y += cellHeight;
                    }
                }
            }

            return cs;
        }

        public void DivideCells(byte addRBC, byte addPLA, byte addTNK, byte addSIL)
        {
            //add to currents
            stateData.numRBCs += addRBC;
            stateData.numPlatelets += addPLA;
            stateData.numTanks += addTNK;
            stateData.numSilos += addSIL;

            stateData.attrHealth += (short)((addRBC + addPLA + addTNK + addSIL) * 100);

            //decrement nutrients
            DivideCellsNutrientChanges(addRBC, addPLA, addTNK, addSIL);

            //gotta readjust
            ReadjustAll();

            //make sound
            if (stateData.virusRef.virusStateData.isMine)
                m_sndDivideCells.Play();
        }

        void DivideCellsNutrientChanges(byte addRBC, byte addPLA, byte addTNK, byte addSIL)
        {
            //decrement nutrient store
            stateData.attrNutrientStore -= (short)
                ((addRBC * GlobalConstants.RBC_threshNToDivide) +
                (addPLA * GlobalConstants.PLATELET_threshNToDivide) +
                (addTNK * GlobalConstants.TANK_threshNToDivide) +
                (addSIL * GlobalConstants.SILO_threshNToDivide));
        }

        public void HybridCells(Cells.CellTypeEnum hybType, byte hybCount,
            Cells.CellTypeEnum srcCellA, Cells.CellTypeEnum srcCellB)
        {
            //add to currents
            switch (hybType)
            {
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.SMALL_HYBRID:
                    stateData.numSmallHybrids += hybCount;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.MED_HYBRID:
                    stateData.numMediumHybrids += hybCount;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_HYBRID:
                    stateData.numBigHybrids += hybCount;
                    break;
            }

            //decrement cell A
            switch (srcCellA)
            {
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.RED_BLOOD_CELL:
                    m_debugMgr.Assert(stateData.numRBCs >= hybCount, "CellCluster:HybridCells - more hybrids than src RBCs.");
                    stateData.numRBCs -= hybCount;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.PLATELET:
                    m_debugMgr.Assert(stateData.numPlatelets >= hybCount, "CellCluster:HybridCells - more hybrids than src PLTs.");
                    stateData.numPlatelets -= hybCount;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_TANK:
                    m_debugMgr.Assert(stateData.numTanks >= hybCount, "CellCluster:HybridCells - more hybrids than src TNKs.");
                    stateData.numTanks -= hybCount;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_SILO:
                    m_debugMgr.Assert(stateData.numSilos >= hybCount, "CellCluster:HybridCells - more hybrids than src SILs.");
                    stateData.numSilos -= hybCount;
                    break;
            }

            //decrement cell B
            switch (srcCellB)
            {
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.RED_BLOOD_CELL:
                    m_debugMgr.Assert(stateData.numRBCs >= hybCount, "CellCluster:HybridCells - more hybrids than src RBCs.");
                    stateData.numRBCs -= hybCount;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.PLATELET:
                    m_debugMgr.Assert(stateData.numPlatelets >= hybCount, "CellCluster:HybridCells - more hybrids than src PLTs.");
                    stateData.numPlatelets -= hybCount;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_TANK:
                    m_debugMgr.Assert(stateData.numTanks >= hybCount, "CellCluster:HybridCells - more hybrids than src TNKs.");
                    stateData.numTanks -= hybCount;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_SILO:
                    m_debugMgr.Assert(stateData.numSilos >= hybCount, "CellCluster:HybridCells - more hybrids than src SILs.");
                    stateData.numSilos -= hybCount;
                    break;
            }

            //make sure health mod is ok
            double healthMod = (double)stateData.attrHealth / (double)stateData.maxHealth;

            //gotta readjust
            ReadjustAll();

            stateData.attrHealth = (short)((double)stateData.maxHealth * healthMod);

            //sound
            if (stateData.virusRef.virusStateData.isMine)
                m_sndHybridCells.Play();
        }

        public void InfectUCell(Cells.UninfectedCell ucell)
        {
            stateData.attrHealth += 100;
            stateData.attrNIncome += ucell.StaticData.rateNutrientIncome;
            stateData.attrNutrientStore += ucell.StaticData.threshMaxNStore;

            switch (ucell.StaticData.staticCellType)
            {
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.RED_BLOOD_CELL:
                    stateData.numRBCs += 1;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.PLATELET:
                    stateData.numPlatelets += 1;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_SILO:
                    stateData.numSilos += 1;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_TANK:
                    stateData.numTanks += 1;
                    break;
            }

            ReadjustAll();

            if (stateData.virusRef.virusStateData.isMine)
                m_sndInfectUCell.Play();
        }

        #endregion

        #region physics

        bool CellCluster_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        {
            //check case to collision
            if (!networkData.sessionDetails.isHost)
                return false;

            if ((stateData.actionState == CellActionState.CHASING_ENEMY_TO_BATTLE) ||
                (stateData.actionState == CellActionState.CHASING_UCELL_TOINFECT) ||
                (stateData.actionState == CellActionState.CHASING_CLUST_TO_COMBINE))
            {
                if ((skin0 == stateData.actionReObject) || (skin1 == stateData.actionReObject))
                {
                    switch (stateData.actionState)
                    {
                        case CellActionState.CHASING_UCELL_TOINFECT:
                            //infect that cell
                            stateData.biophageScn.HostClusterInfectUCell(
                                BiophageGameBaseScn.GobjIDUCell(stateData.actionReObject.Id),
                                BiophageGameBaseScn.GobjIDCluster(Id));

                            //go back to idle state
                            stateData.actionState = CellActionState.IDLE;
                            stateData.actionReObject = null;
                            break;
                        case CellActionState.CHASING_ENEMY_TO_BATTLE:
                            //battle that cluster
                            stateData.biophageScn.HostSubmitClusterBattle(
                                this, (CellCluster)stateData.actionReObject);

                            //go back to idle state
                            if (stateData.actionReObject != null)
                            {
                                stateData.biophageScn.HostUNWarnBattle((CellCluster)stateData.actionReObject);
                            }

                            stateData.actionState = CellActionState.IDLE;
                            stateData.actionReObject = null;
                            break;
                        case CellActionState.CHASING_CLUST_TO_COMBINE:
                            //combine with cluster
                            stateData.biophageScn.HostSubmitClusterCombine(
                                this, (CellCluster)stateData.actionReObject);

                            //go back to idle state
                            stateData.actionState = CellActionState.IDLE;
                            stateData.actionReObject = null;
                            break;
                    }
                    return false;
                }
            }

            //make sure i give up if i hit the environment
                if ((skin0 is LevelEnvironment) || (skin1 is LevelEnvironment))
                {
                    //go back a bit
                    Microsoft.Xna.Framework.Vector3 backDir = (m_prevPos - Position);
                    if (backDir.Length() != 0f)
                        backDir.Normalize();

                    //go back a bit
                    Position += backDir * (stateData.attrVelocity * 1.25f);

                    //make sure i give up doing the action i was
                    if ((stateData.actionState == CellActionState.CHASING_ENEMY_TO_BATTLE) && (stateData.actionReObject != null))
                    {
                        stateData.biophageScn.HostUNWarnBattle((CellCluster)stateData.actionReObject);
                    }
                    stateData.actionState = CellActionState.IDLE;
                    stateData.actionReObject = null;
                }

            return true;
        }

        #endregion

        #region game loop

        #region input handling

        #region input
        /// <summary>
        /// Input is mainly only for reorientating the camera.
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
            float elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (m_hud.m_clusterMenu.Active)
            {
                m_hud.m_clusterMenu.Input(ref newGPState
#if !XBOX
, ref newKBState
#endif
);
                m_hud.m_clusterMenu.Update(gameTime);
                m_hud.m_clusterMenu.Animate(gameTime);
            }
            else
            {

                #region camera panning

                if ((stateData.actionState == CellActionState.IDLE) ||
                    (stateData.actionState == CellActionState.CHASING_UCELL_TOINFECT) ||
                    (stateData.actionState == CellActionState.CHASING_ENEMY_TO_BATTLE) ||
                    (stateData.actionState == CellActionState.CHASING_CLUST_TO_COMBINE) ||
                    (stateData.actionState == CellActionState.EVADING_ENEMY))
                {
                    // Determine rotation amount from input
                    Microsoft.Xna.Framework.Vector2 rotationAmount = -newGPState.ThumbSticks.Right;
#if !XBOX
                    if (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
                        rotationAmount.X = 1.0f;
                    if (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
                        rotationAmount.X = -1.0f;
                    if (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
                        rotationAmount.Y = -1.0f;
                    if (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
                        rotationAmount.Y = 1.0f;
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

                    //ensure normaliseation
                    if (Up.Length() != 0f)
                        Up.Normalize();
                    if (Right.Length() != 0f)
                        Right.Normalize();

                    // Reconstruct transforms
                    Microsoft.Xna.Framework.Matrix newTrans = Microsoft.Xna.Framework.Matrix.Identity;
                    newTrans.Forward = Heading;
                    newTrans.Up = Up;
                    newTrans.Right = Right;
                    ModelTransform = newTrans;
                }
                else
                {
                    switch (stateData.actionState)
                    {
                        case CellActionState.WAITING_FOR_ORDER:
                        case CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_UCELL_SELECTED:
                            stateData.virusRef.m_playerCursor.Input(gameTime,
                                ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                            break;
                    }
                }

                #endregion

                #region state options

                switch (stateData.actionState)
                {
                    case CellActionState.IDLE:
                        InputIdle(gameTime,
                                ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                        break;
                    case CellActionState.WAITING_FOR_ORDER:
                        InputWaiting(gameTime,
                            ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                        break;
                    case CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED:
                        InputWaitingMyClustSelected(gameTime,
                            ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                        break;
                    case CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED:
                        InputWaitingEnemyClusterSelected(gameTime,
                            ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                        break;
                    case CellActionState.WAITING_WITH_UCELL_SELECTED:
                        InputWaitingUCellSelected(gameTime,
                            ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                        break;
                    case CellActionState.CHASING_UCELL_TOINFECT:
                        InputChasingUCell(gameTime,
                            ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                        break;
                    case CellActionState.CHASING_ENEMY_TO_BATTLE:
                    case CellActionState.EVADING_ENEMY:
                        InputChasingEvadingEnemy(gameTime,
                            ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                        break;
                    case CellActionState.CHASING_CLUST_TO_COMBINE:
                        InputChasingMyCluster(gameTime,
                            ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                        break;
                }
            }

                #endregion
        }
        #endregion

        #region idle input
        /// <summary>
        /// Handle input actions when cluster is idle.
        /// </summary>
        void InputIdle(Microsoft.Xna.Framework.GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            //divide cells option
            if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.A))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) &&
                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.A))
#endif
)
            {
                //show divide cells option menu window
                m_hud.m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_DIVIDE_WND_ID);
                m_hud.m_clusterMenu.Active = true;

                m_hud.m_clusterMenu.Cluster = this;
                m_hud.m_clusterMenu.ResetDivide();
            }

            //split cluster option
            else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) &&
                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.B))
#endif
)
            {
                //show split cluster menu window - if able
                if (stateData.numCellsTotal > 1)
                {
                    m_hud.m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_SPLIT_WND_ID);
                    m_hud.m_clusterMenu.Active = true;

                    m_hud.m_clusterMenu.Cluster = this;
                    m_hud.m_clusterMenu.ResetSplit();
                }
            }

            //hybreed cells option
            else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Y) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.Y))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y) &&
                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Y))
#endif
)
            {
                //show hybrid cells menu window
                if (CanHybreed)
                {
                    m_hud.m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_HYBRIDS_WND_ID);
                    m_hud.m_clusterMenu.Active = true;

                    m_hud.m_clusterMenu.Cluster = this;
                    m_hud.m_clusterMenu.SetOptionsForHybrids();
                }
            }

            //action option
            else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.X))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X) &&
                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.X))
#endif
)
            {
                //change to waiting state and show cursor
                stateData.actionState = CellActionState.WAITING_FOR_ORDER;

                stateData.virusRef.m_playerCursor.Visible = true;
                stateData.virusRef.m_playerCursor.Position = Position;
                stateData.virusRef.m_playerCursor.PhysBody.EnableBody();
            }
        }
        #endregion

        #region waiting input
        void InputWaiting(Microsoft.Xna.Framework.GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            //cancel option
            if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) &&
                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.B))
#endif
)
            {
                //disable cursor and switch back to idle state
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                stateData.virusRef.m_playerCursor.Visible = false;
                stateData.virusRef.m_playerCursor.PhysBody.DisableBody();
                return;
            }

            //we must check what collisions have occured with the cursor ray
            Segment cursSeg = new Segment(
                m_cam.Position,
                stateData.virusRef.m_playerCursor.ForwardDir * GlobalConstants.GP_CURSOR_LENGTH);

            float colDist;
            CollisionSkin colSkin;
            Microsoft.Xna.Framework.Vector3 colPos;
            Microsoft.Xna.Framework.Vector3 colNormal;

            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.SegmentIntersect(
                out colDist, out colSkin, out colPos, out colNormal, cursSeg, m_skinCursorPredicate);

            if (colSkin != null)
            {
                //check is cluster
                if (colSkin is CellCluster)
                {
                    CellCluster colCluster = (CellCluster)colSkin;

                    //ignore current cluster
                    if (colCluster == this)
                    {
                        stateData.actionState = CellActionState.WAITING_FOR_ORDER;
                        stateData.actionReObject = null;
                    }
                    //show options
                    else if (colCluster.stateData.virusOwnerId == stateData.virusOwnerId)
                    {
                        //my cluster
                        stateData.actionState = CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED;
                        stateData.actionReObject = colCluster;
                    }
                    else
                    {
                        //enemy cluster
                        stateData.actionState = CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED;
                        stateData.actionReObject = colCluster;
                    }
                }
                else if (colSkin is Cells.UninfectedCell)
                {
                    Cells.UninfectedCell colUCell = (Cells.UninfectedCell)colSkin;

                    //show options
                    stateData.actionState = CellActionState.WAITING_WITH_UCELL_SELECTED;
                    stateData.actionReObject = colUCell;
                }
            }
        }
        #endregion

        #region waiting with my clust selected input
        void InputWaitingMyClustSelected(Microsoft.Xna.Framework.GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                                                        ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            //cancel option
            if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) &&
                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.B))
#endif
)
            {
                //disable cursor and switch back to idle state
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                stateData.virusRef.m_playerCursor.Visible = false;
                stateData.virusRef.m_playerCursor.PhysBody.DisableBody();
                return;
            }

            //we must check what collisions have occured with the cursor ray
            Segment cursSeg = new Segment(
                m_cam.Position,
                stateData.virusRef.m_playerCursor.ForwardDir * GlobalConstants.GP_CURSOR_LENGTH);

            float colDist;
            CollisionSkin colSkin;
            Microsoft.Xna.Framework.Vector3 colPos;
            Microsoft.Xna.Framework.Vector3 colNormal;

            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.SegmentIntersect(
                out colDist, out colSkin, out colPos, out colNormal, cursSeg, m_skinCursorPredicate);

            if (colSkin != null)
            {
                //check is cluster
                if (colSkin is CellCluster)
                {
                    CellCluster colCluster = (CellCluster)colSkin;

                    //ignore self
                    if (colCluster == this)
                    {
                        stateData.actionState = CellActionState.WAITING_FOR_ORDER;
                        stateData.actionReObject = null;
                    }
                    //continue
                    else if (colCluster == stateData.actionReObject)
                    {
                        //chase to combine option
                        if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) &&
                            oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.A))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) &&
                             oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.A))
#endif
)
                        {
                            //disable cursor and switch back to idle state
                            stateData.actionState = CellActionState.CHASING_CLUST_TO_COMBINE;
                            stateData.virusRef.m_playerCursor.Visible = false;
                            stateData.virusRef.m_playerCursor.PhysBody.DisableBody();
                            if ((networkData.sessionDetails.isMultiplayer) && (!networkData.sessionDetails.isHost))
                                stateData.biophageScn.ClientSendClusterChase(this, stateData.actionReObject);
                        }
                    }
                    else if (colCluster.stateData.virusOwnerId == stateData.virusOwnerId)
                    {
                        //cluster has changed to another of mine
                        stateData.actionState = CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED;
                        stateData.actionReObject = colCluster;
                    }
                    else
                    {
                        //enemy cluster
                        stateData.actionState = CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED;
                        stateData.actionReObject = colCluster;
                    }
                }
                else
                {
                    stateData.actionState = CellActionState.WAITING_FOR_ORDER;
                    stateData.actionReObject = null;
                }
            }
        }
        #endregion

        #region waiting with enemy cluster selected input
        void InputWaitingEnemyClusterSelected(Microsoft.Xna.Framework.GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            //cancel option
            if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) &&
                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.B))
#endif
)
            {
                //disable cursor and switch back to idle state
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                stateData.virusRef.m_playerCursor.Visible = false;
                stateData.virusRef.m_playerCursor.PhysBody.DisableBody();
                return;
            }

            //we must check what collisions have occured with the cursor ray
            Segment cursSeg = new Segment(
                m_cam.Position,
                stateData.virusRef.m_playerCursor.ForwardDir * GlobalConstants.GP_CURSOR_LENGTH);

            float colDist;
            CollisionSkin colSkin;
            Microsoft.Xna.Framework.Vector3 colPos;
            Microsoft.Xna.Framework.Vector3 colNormal;

            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.SegmentIntersect(
                out colDist, out colSkin, out colPos, out colNormal, cursSeg, m_skinCursorPredicate);

            if (colSkin != null)
            {
                //check is cluster
                if (colSkin is CellCluster)
                {
                    CellCluster colCluster = (CellCluster)colSkin;

                    //ignore self
                    if (colCluster == this)
                    {
                        stateData.actionState = CellActionState.WAITING_FOR_ORDER;
                        stateData.actionReObject = null;
                    }
                    //continue
                    else if (colCluster == stateData.actionReObject)
                    {
                        //chase to battle option
                        if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) &&
                            oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.A))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) &&
                             oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.A))
#endif
)
                        {
                            //disable cursor and switch back to idle state
                            //stateData.actionState = CellActionState.CHASING_ENEMY_TO_BATTLE;
                            if (networkData.sessionDetails.isHost)
                                stateData.biophageScn.HostClusterChase(this, stateData.actionReObject);

                            stateData.virusRef.m_playerCursor.Visible = false;
                            stateData.virusRef.m_playerCursor.PhysBody.DisableBody();
                            if ((networkData.sessionDetails.isMultiplayer) && (!networkData.sessionDetails.isHost))
                                stateData.biophageScn.ClientSendClusterChase(this, stateData.actionReObject);
                        }
                        //evade option
                        else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.X) &&
                            oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.X))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.X) &&
                             oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.X))
#endif
)
                        {
                            //disable cursor and switch back to idle state
                            stateData.actionState = CellActionState.EVADING_ENEMY;
                            stateData.virusRef.m_playerCursor.Visible = false;
                            stateData.virusRef.m_playerCursor.PhysBody.DisableBody();
                            if ((networkData.sessionDetails.isMultiplayer) && (!networkData.sessionDetails.isHost))
                                stateData.biophageScn.ClientSendClusterEvade(this, stateData.actionReObject);
                        }
                    }
                    else if (colCluster.stateData.virusOwnerId == stateData.virusOwnerId)
                    {
                        //cluster has changed to one of mine
                        stateData.actionState = CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED;
                        stateData.actionReObject = colCluster;
                    }
                    else
                    {
                        //enemy cluster
                        stateData.actionState = CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED;
                        stateData.actionReObject = colCluster;
                    }
                }
                else
                {
                    stateData.actionState = CellActionState.WAITING_FOR_ORDER;
                    stateData.actionReObject = null;
                }
            }
        }
        #endregion

        #region waiting with ucell selected input
        void InputWaitingUCellSelected(Microsoft.Xna.Framework.GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            //cancel option
            if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) &&
                                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.B))
#endif
)
            {
                //disable cursor and switch back to idle state
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                stateData.virusRef.m_playerCursor.Visible = false;
                stateData.virusRef.m_playerCursor.PhysBody.DisableBody();
                return;
            }

            //we must check what collisions have occured with the cursor ray
            Segment cursSeg = new Segment(
                m_cam.Position,
                stateData.virusRef.m_playerCursor.ForwardDir * GlobalConstants.GP_CURSOR_LENGTH);

            float colDist;
            CollisionSkin colSkin;
            Microsoft.Xna.Framework.Vector3 colPos;
            Microsoft.Xna.Framework.Vector3 colNormal;

            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.SegmentIntersect(
                out colDist, out colSkin, out colPos, out colNormal, cursSeg, m_skinCursorPredicate);

            if (colSkin != null)
            {
                //check is cluster
                if (colSkin is Cells.UninfectedCell)
                {
                    Cells.UninfectedCell colUCell = (Cells.UninfectedCell)colSkin;

                    //continue
                    if (colUCell == stateData.actionReObject)
                    {
                        //chase to infect option
                        if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) &&
                            oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.A))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) &&
                             oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.A))
#endif
)
                        {
                            //disable cursor and switch back to idle state
                            stateData.actionState = CellActionState.CHASING_UCELL_TOINFECT;
                            stateData.virusRef.m_playerCursor.Visible = false;
                            stateData.virusRef.m_playerCursor.PhysBody.DisableBody();
                            if ((networkData.sessionDetails.isMultiplayer) && (!networkData.sessionDetails.isHost))
                                stateData.biophageScn.ClientSendClusterChase(this, stateData.actionReObject);
                        }
                    }
                    else
                    {
                        //enemy cluster
                        stateData.actionState = CellActionState.WAITING_WITH_UCELL_SELECTED;
                        stateData.actionReObject = colUCell;
                    }
                }
                else
                {
                    stateData.actionState = CellActionState.WAITING_FOR_ORDER;
                    stateData.actionReObject = null;
                }
            }
        }
        #endregion

        #region chasing ucell input
        void InputChasingUCell(Microsoft.Xna.Framework.GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            //cancel option
            if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) &&
                                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.B))
#endif
)
            {
                //disable cursor and switch back to idle state
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;

                //send flag to host if client
                if ((networkData.sessionDetails.isMultiplayer) && (!networkData.sessionDetails.isHost))
                    stateData.biophageScn.ClientSendClusterCancelAction(this);

                return;
            }

            //do as similar to idle options
            //divide cells option
            else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.A))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) &&
                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.A))
#endif
)
            {
                //show divide cells option menu window
                m_hud.m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_DIVIDE_WND_ID);
                m_hud.m_clusterMenu.Active = true;

                m_hud.m_clusterMenu.Cluster = this;
                m_hud.m_clusterMenu.ResetDivide();
            }


            //hybreed cells option
            else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Y) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.Y))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y) &&
                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Y))
#endif
)
            {
                //show hybrid cells menu window
                if (CanHybreed)
                {
                    m_hud.m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_HYBRIDS_WND_ID);
                    m_hud.m_clusterMenu.Active = true;

                    m_hud.m_clusterMenu.Cluster = this;
                    m_hud.m_clusterMenu.SetOptionsForHybrids();
                }
            }
        }
        #endregion

        #region chasing or evading enemy input
        void InputChasingEvadingEnemy(Microsoft.Xna.Framework.GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            //cancel option
            if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) &&
                                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.B))
#endif
)
            {
                if ((stateData.actionState == CellActionState.CHASING_ENEMY_TO_BATTLE)&&
                    (networkData.sessionDetails.isHost))
                {
                    stateData.biophageScn.HostUNWarnBattle((CellCluster)stateData.actionReObject);
                }

                //disable cursor and switch back to idle state
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;

                //send flag to host if client
                if ((networkData.sessionDetails.isMultiplayer) && (!networkData.sessionDetails.isHost))
                    stateData.biophageScn.ClientSendClusterCancelAction(this);

                return;
            }

            //do as similar to idle options
            //divide cells option
            else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.A))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) &&
                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.A))
#endif
)
            {
                //show divide cells option menu window
                m_hud.m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_DIVIDE_WND_ID);
                m_hud.m_clusterMenu.Active = true;

                m_hud.m_clusterMenu.Cluster = this;
                m_hud.m_clusterMenu.ResetDivide();
            }

            //hybreed cells option
            else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Y) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.Y))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y) &&
                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Y))
#endif
)
            {
                //show hybrid cells menu window
                if (CanHybreed)
                {
                    m_hud.m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_HYBRIDS_WND_ID);
                    m_hud.m_clusterMenu.Active = true;

                    m_hud.m_clusterMenu.Cluster = this;
                    m_hud.m_clusterMenu.RecalcClusterAttrs();
                    m_hud.m_clusterMenu.SetOptionsForHybrids();
                }
            }
        }
        #endregion

        #region chasing my cluster input
        void InputChasingMyCluster(Microsoft.Xna.Framework.GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
)
        {
            //cancel option
            if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.B) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.B))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.B) &&
                                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.B))
#endif
)
            {
                //disable cursor and switch back to idle state
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;

                //send flag to host if client
                if ((networkData.sessionDetails.isMultiplayer) && (!networkData.sessionDetails.isHost))
                    stateData.biophageScn.ClientSendClusterCancelAction(this);

                return;
            }

            //do as similar to idle options
            //divide cells option
            else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.A) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.A))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A) &&
                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.A))
#endif
)
            {
                //show divide cells option menu window
                m_hud.m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_DIVIDE_WND_ID);
                m_hud.m_clusterMenu.Active = true;

                m_hud.m_clusterMenu.RecalcClusterAttrs();
                m_hud.m_clusterMenu.ResetDivide();
            }

            //hybreed cells option
            else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Y) &&
                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.Y))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y) &&
                                    oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Y))
#endif
)
            {
                //show hybrid cells menu window
                if (CanHybreed)
                {
                    m_hud.m_clusterMenu.SetDefaultWindow(GlobalConstants.CM_HYBRIDS_WND_ID);
                    m_hud.m_clusterMenu.Active = true;

                    m_hud.m_clusterMenu.RecalcClusterAttrs();
                    m_hud.m_clusterMenu.SetOptionsForHybrids();
                }
            }
        }
        #endregion

        #endregion

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (IsInit)
            {
                if (networkData.sessionDetails.isHost)
                {
                    if (m_wbcDetectParameter != null)
                    {
                        PhysicsStep();
                        m_wbcDetectParameter.Update(gameTime);
                        if ((stateData.actionState == CellActionState.IDLE) && (PhysBody.Immovable))
                            PhysBody.Immovable = false;
                    }

                    //check if dead
                    if (stateData.numCellsTotal == 0)
                        Active = false;

                    //bot ai
                    if (stateData.numWhiteBloodCell == 0)
                    {
                        if (Active && stateData.virusRef.virusStateData.isBot)
                            m_clusterAI.DoAIUpdate(gameTime);
                    }
                }
                else
                    Predict(gameTime);

                //general updates allow UI feedback for all players...host will be the most up-to-date
                //  and accept any changes
                //  -nutrients
                if (stateData.numWhiteBloodCell == 0)
                {
                    m_nutrientStoreOffset += (stateData.attrNIncome * gameTime.ElapsedRealTime.TotalSeconds);
                    if (m_nutrientStoreOffset > 1f)
                    {
                        stateData.attrNutrientStore += (short)Math.Round(m_nutrientStoreOffset);
                        m_nutrientStoreOffset -= Math.Round(m_nutrientStoreOffset);
                    }

                    if (stateData.attrNutrientStore > stateData.maxNutrientStore)
                        stateData.attrNutrientStore = stateData.maxNutrientStore;

                    //  -can hybrid
                    ReadjustHybridCapability();
                }
            }
        }

        #region update funcs

        public void LinearUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do safe readjustment of cluster
            if (doSafeModelReadjust)
                ReadjustClusterModel();

            if (networkData.sessionDetails.isHost)
            {
                if ((stateData.actionState == CellActionState.CHASING_UCELL_TOINFECT) ||
                    (stateData.actionState == CellActionState.CHASING_ENEMY_TO_BATTLE) ||
                    (stateData.actionState == CellActionState.CHASING_CLUST_TO_COMBINE))
                    Chase(gameTime);
                else if (stateData.actionState == CellActionState.EVADING_ENEMY)
                    Evade(gameTime);
            }
        }

        void Evade(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //check RE object didn't die in the mean time - otherwise go back to idle
            if (stateData.actionReObject == null)
            {
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                if (m_wbcDetectParameter != null)
                    PhysBody.Immovable = false;
                return;
            }
            else if (!stateData.actionReObject.Active)
            {
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                if (m_wbcDetectParameter != null)
                    PhysBody.Immovable = false;
                return;
            }

            //modify heading direction
            Microsoft.Xna.Framework.Vector3 headingDir = Position - stateData.actionReObject.Position;
            float enemyDist = headingDir.Length();
            if (enemyDist > 60f)
            {
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                if (m_wbcDetectParameter != null)
                    PhysBody.Immovable = false;
                return;
            }

            float velocityScale = (float)(stateData.attrVelocity * gameTime.ElapsedRealTime.TotalSeconds);

            if (headingDir.Length() != 0)
                headingDir.Normalize();

            //check environement is not in the way
            Segment cursSeg = new Segment(
                Position,
                stateData.actionReObject.Position - Position);

            float colDist;
            CollisionSkin colSkin;
            Microsoft.Xna.Framework.Vector3 colPos;
            Microsoft.Xna.Framework.Vector3 colNormal;

            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.SegmentIntersect(
                out colDist, out colSkin, out colPos, out colNormal, cursSeg, m_skinEnvironment);

            if (colSkin != null)
            {
                //environment is in the way, give up
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                if (m_wbcDetectParameter != null)
                    PhysBody.Immovable = false;
                return;
            }

            //head further from target
            Position += (headingDir * velocityScale);
        }

        void Chase(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //check RE object didn't die in the mean time - otherwise go back to idle
            if (stateData.actionReObject == null)
            {
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                if (m_wbcDetectParameter != null)
                    PhysBody.Immovable = false;

                //gotta tell everyone
                if (stateData.biophageScn.m_sessionDetails.isMultiplayer)
                    stateData.biophageScn.HostSubmitClusterUpdate(this);
                return;
            }
            else if (!stateData.actionReObject.Active)
            {
                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                if (m_wbcDetectParameter != null)
                    PhysBody.Immovable = false;

                //gotta tell everyone
                if (stateData.biophageScn.m_sessionDetails.isMultiplayer)
                    stateData.biophageScn.HostSubmitClusterUpdate(this);
                return;
            }

            //modify heading direction
            Microsoft.Xna.Framework.Vector3 headingDir = stateData.actionReObject.Position - Position;

            if (headingDir.Length() != 0f)
                headingDir.Normalize();

            //check environement is not in the way
            Segment cursSeg = new Segment(
                Position,
                stateData.actionReObject.Position - Position);

            float colDist;
            CollisionSkin colSkin;
            Microsoft.Xna.Framework.Vector3 colPos;
            Microsoft.Xna.Framework.Vector3 colNormal;

            PhysicsSystem.CurrentPhysicsSystem.CollisionSystem.SegmentIntersect(
                out colDist, out colSkin, out colPos, out colNormal, cursSeg, m_skinEnvironment);

            if (colSkin != null)
            {
                //environment is in the way, so give up
                if ((stateData.actionState == CellActionState.CHASING_ENEMY_TO_BATTLE)&&
                    (stateData.actionReObject != null))
                {
                    stateData.biophageScn.HostUNWarnBattle((CellCluster)stateData.actionReObject);
                }

                stateData.actionState = CellActionState.IDLE;
                stateData.actionReObject = null;
                if (m_wbcDetectParameter != null)
                    PhysBody.Immovable = false;
                return;
            }

            //check plausable velocity

            //head closer to target
            Position += (headingDir * (float)(stateData.attrVelocity * gameTime.ElapsedRealTime.TotalSeconds));
        }

        #endregion

        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (m_wbcDetectParameter != null)
                m_wbcDetectParameter.Animate(gameTime);

            m_prevPos = Position;
        }

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice,
                                    CameraGObj camera)
        {
            if (!IsInit)
                Init();
            if (!IsLoaded)
                Load();

            //draw cluster
            ResortModel(camera);

            Microsoft.Xna.Framework.Graphics.Model model;

            Microsoft.Xna.Framework.Vector3 mdlAmbient = Microsoft.Xna.Framework.Graphics.Color.Black.ToVector3();
            Microsoft.Xna.Framework.Vector3 mdlDiffuse = Microsoft.Xna.Framework.Graphics.Color.DarkRed.ToVector3();
            Microsoft.Xna.Framework.Vector3 mdlSpecular = Microsoft.Xna.Framework.Graphics.Color.Plum.ToVector3();

            foreach (KeyValuePair<float, ClusterModelContainer> cMdlKVP in m_clusterDynModel)
            {
                model = (Microsoft.Xna.Framework.Graphics.Model)cMdlKVP.Value.mdlResHandle.GetResource;
                foreach (Microsoft.Xna.Framework.Graphics.ModelMesh mesh in model.Meshes)
                {
                    foreach (Microsoft.Xna.Framework.Graphics.BasicEffect effect in mesh.Effects)
                    {
                        //Lighting
                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;

                        mdlAmbient = effect.AmbientLightColor;
                        mdlDiffuse = effect.DiffuseColor;
                        mdlSpecular = effect.SpecularColor;

                        effect.AmbientLightColor = m_ambientColour.ToVector3();
                        effect.DiffuseColor = m_diffuseColour.ToVector3();
                        effect.SpecularColor = m_specularColour.ToVector3();

                        //load drawspace translation maticies
                        effect.World = Microsoft.Xna.Framework.Matrix.CreateWorld(
                            cMdlKVP.Value.mdlPosition, Microsoft.Xna.Framework.Vector3.Forward, cMdlKVP.Value.mdlUpDir) *
                            WorldTransform;

                        effect.Projection = camera.ProjectionMatrix;
                        effect.View = camera.ViewMatrix;
                    }

                    mesh.Draw();

                    //reset model colours
                    foreach (Microsoft.Xna.Framework.Graphics.BasicEffect effect in mesh.Effects)
                    {
                        effect.AmbientLightColor = mdlAmbient;
                        effect.DiffuseColor = mdlDiffuse;
                        effect.SpecularColor = mdlSpecular;
                    }
                }
            }

//#if DEBUG
//            DrawCollisionSkin(gameTime, graphicsDevice, camera);
//#endif
            if (m_wbcDetectParameter != null)
                m_wbcDetectParameter.Draw(gameTime, graphicsDevice, camera);
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

        void ResortModel(CameraGObj camera)
        {
            SortedList<float, ClusterModelContainer> newModelList = new SortedList<float, ClusterModelContainer>(m_clusterDynModel.Count);

            float uniqueKey = 0f;
            foreach (KeyValuePair<float, ClusterModelContainer> mdlKVP in m_clusterDynModel)
            {
                uniqueKey = Microsoft.Xna.Framework.Vector3.Distance(mdlKVP.Value.mdlPosition, camera.Position);
                if (float.IsNaN(uniqueKey))
                    uniqueKey = 1f;
                while (newModelList.ContainsKey(uniqueKey))
                {
                    uniqueKey += (uniqueKey * 0.05f) + float.Epsilon;
                    if (float.IsNaN(uniqueKey))
                        uniqueKey = 1f;
                }

                newModelList.Add(uniqueKey, mdlKVP.Value);
            }

            //clear and swap
            m_clusterDynModel = newModelList;
        }

        #endregion

        #endregion
    }
}
