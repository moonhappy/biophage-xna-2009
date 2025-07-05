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

using JigLibX.Physics;
using JigLibX.Geometry;
using JigLibX.Collision;
using Biophage.Game.Stages.Game.Common;
using Microsoft.Xna.Framework.Graphics.PackedVector;

namespace Biophage.Game.Stages.Game
{
    #region attributes

    public class SessionDetails
    {
        public LnaNetworkSessionComponent netSessionComponent = null;
        public Microsoft.Xna.Framework.Net.LocalNetworkGamer gamerMe = null;
        public bool isMultiplayer = false;
        public Microsoft.Xna.Framework.Net.NetworkSessionType netSessionType = Microsoft.Xna.Framework.Net.NetworkSessionType.SystemLink;
        public bool isHost = true;

        public byte myVirusId = 0;

        public GlobalConstants.GameplayType type = GlobalConstants.GameplayType.TIMED_MATCH;
        public byte numBots = 2;
        public byte typeSettings = 55;
        public GlobalConstants.GameLevel gameLevel = GlobalConstants.GameLevel.TRIAL;
    }

    /// <summary>
    /// This takes a long time to complete - so do the job asynchronously.
    /// </summary>
    /// <remarks>
    /// The biophage scene data will not be in contention so no need to fret.
    /// </remarks>
    public class SetSessionAsyncTask : AsyncSceneLoad
    {
        public override void DoWork()
        {
            //this loads the scene
            base.DoWork();

            //this initialises the scene
            AsyncSceneLoadParam param = (AsyncSceneLoadParam)m_param;
            param.scn.Init();

            //this sets the session
            ((BiophageGameBaseScn)param.scn).SetSession((SessionDetails)param.extras);

            //return
            m_asyncMgr.ProduceReturn(new AsyncReturnPackage(m_id, null));
        }
    }

    /// <summary>
    /// Bloom effect stage enum/indicator.
    /// </summary>
    /// <remarks>
    /// Copied from the XNA 'Bloom' tutorial.
    /// </remarks>
    public enum IntermediateBuffer
    {
        PreBloom,
        BlurredHorizontally,
        BlurredBothWays,
        FinalResult,
    }

    /// <summary>
    /// Allows the greater score to be listed at the top of the list.
    /// </summary>
    public class DoubleReverse : IComparer<double>
    {
        public int Compare(double x, double y)
        {
            int retVal = (int)(y - x);

            //resolve precision errors
            if (retVal == 0)
            {
                if (y > x)
                    return 1;
                else if (x > y)
                    return -1;
                else
                    return 0;
            }
            else
                return retVal;
        }
    }

    /// <summary>
    /// Allows the greater score to be listed at the top of the list.
    /// </summary>
    public class FloatReverse : IComparer<float>
    {
        public int Compare(float x, float y)
        {
            int retVal = (int)(y - x);

            //resolve precision errors
            if (retVal == 0)
            {
                if (y > x)
                    return 1;
                else if (x > y)
                    return -1;
                else
                    return 0;
            }
            else
                return retVal;
        }
    }

    #endregion

    /// <summary>
    /// Represents the base code for all Biophage game levels.
    /// </summary>
    public class BiophageGameBaseScn : Scene
    {
        #region fields

        protected Microsoft.Xna.Framework.GraphicsDeviceManager m_graphicsMgr;

        #region sounds and alerts
        private SoundResHandle m_sndImmuneCountDown;
        private double m_immuneCountDownStartSecs;

        private SoundResHandle m_sndMedicAlertCountDown;
        private double m_medicCountDownStartSecs;

        private SoundResHandle m_sndMedicAlert;
        private SoundResHandle m_sndGenAlert;
        private SoundResHandle m_sndSplitCluster;
        private SoundResHandle m_sndBattleWarning;
        #endregion

        #region bloom effect attrs
        Microsoft.Xna.Framework.Graphics.ResolveTexture2D m_resolveTarget;
        Microsoft.Xna.Framework.Graphics.RenderTarget2D m_renderTarget1;
        Microsoft.Xna.Framework.Graphics.RenderTarget2D m_renderTarget2;

        EffectResHandle m_bloomExtractEffect;
        EffectResHandle m_bloomCombineEffect;
        EffectResHandle m_gaussianBlurEffect;

        protected Microsoft.Xna.Framework.Graphics.SpriteBatch m_spriteBatch;

        IntermediateBuffer showBuffer = IntermediateBuffer.FinalResult;
        public IntermediateBuffer ShowBuffer
        {
            get { return showBuffer; }
            set { showBuffer = value; }
        }
        #endregion

        protected CameraGObj m_tutOverlayCam;
        protected SpriteFontResHandle m_waitFont;
        protected const string m_waitMsg = "Please wait...";
        Microsoft.Xna.Framework.Vector2 FontPos;
        Microsoft.Xna.Framework.Vector2 FontOrigin;

        //physics
        protected PhysicsSystem m_physicsWorld;

        public const uint m_levelEnvId = 770;
        public const uint m_assetIdOffset = 771;

        private int m_capsidLastTime = 0;

        #region UI

        protected Common.PlayerCursor m_cursor;
        public const uint m_cursorId = 768;
        public const uint m_camId = 769;

        #endregion

        #region state data

        //these lists map to the GObj Ids in child objs dictionary
        public const uint m_ucellIdOffset = 0;
        protected LinkedList<byte> m_uninfectedCells = new LinkedList<byte>();   //<- IDs between 0-255
        protected LinkedList<Common.Cells.UninfectedCell> m_ucellObjs = new LinkedList<Common.Cells.UninfectedCell>();
        public LinkedList<Common.Cells.UninfectedCell> UninfectCellsList
        {
            get { return m_ucellObjs; }
        }

        public const uint m_clustIdOffset = 256;
        protected LinkedList<byte> m_cellClusters = new LinkedList<byte>();      //<- IDs between 256-511
        protected LinkedList<Common.CellCluster> m_cellClusterObjs = new LinkedList<CellCluster>();
        public LinkedList<Common.CellCluster> CellClustersList
        {
            get { return m_cellClusterObjs; }
        }

        public const uint m_virusIdOffset = 512;
        protected LinkedList<byte> m_viruses = new LinkedList<byte>();           //<- IDs between 512-767 - only if host
        protected LinkedList<Virus> m_virusObjs = new LinkedList<Virus>();
        public LinkedList<Virus> VirusList
        {
            get { return m_virusObjs; }
        }

        protected SortedList<float, IGameObject> m_drawableObjs = new SortedList<float, IGameObject>(new FloatReverse());

        public StateCalculations gamestate;

        public Virus myVirus = null;

        protected FinishedRanksScreen m_finishedRanksScrn;

        TimeSpan m_timeGameStarted;
        TimeSpan m_timeSinceLastMed;
        TimeSpan m_timeSinceLastImmuneSysWave;
        int m_immuneSysWaveCount;
        protected LinkedList<CellCluster> m_whiteBloodCells = new LinkedList<CellCluster>();

        #endregion

        #region static values

        //these help with constructing cells
        public Common.Cells.CellStaticData m_rbcStaticData;
        public Common.Cells.CellStaticData m_pcStaticData;
        public Common.Cells.CellStaticData m_siloStaticData;
        public Common.Cells.CellStaticData m_tankStaticData;

        public Common.Cells.CellStaticData m_shyStaticData;
        public Common.Cells.CellStaticData m_mhyStaticData;
        public Common.Cells.CellStaticData m_bhyStaticData;

        public Common.Cells.CellStaticData m_wbcStaticData;

        #endregion

        #region networkdata

        TimeSpan m_lastUCupdateSent;    //<- last time uninfected cells update packet sent.
        TimeSpan m_lastClupdateSent;    //<- last time cluster update packet sent.

        public SessionDetails m_sessionDetails;
        protected bool m_sessionSet = false;

        protected bool m_gameStarted = false;
        protected bool m_clientSentReadyFlag = false;
        protected int m_numNotReadyPlayers = 0;

        #endregion

        #region level properties

        protected Microsoft.Xna.Framework.Vector3[] m_virusCapsidSpawnLocations = null;
        protected Microsoft.Xna.Framework.Vector3[] m_whiteBloodCellSpawnLocations = null;

        #endregion

        #endregion

        #region methods

        #region construction

        public BiophageGameBaseScn(uint id,
                                    DebugManager debugMgr, ResourceManager resourceMgr,
                                    Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                    Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                    SpriteFontResHandle sceneFont,
                                    Scene parent)
            : base(id,
                    debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                    Microsoft.Xna.Framework.Graphics.Color.Black,
                    sceneFont,
                    parent.Stage, parent, null)
        {
            //set fields
            m_graphicsMgr = graphicsMgr;
            m_spriteBatch = Stage.SceneMgr.Game.spriteBatch; //local reference

            #region bloom effect
            m_bloomExtractEffect = new EffectResHandle(m_debugMgr, resourceMgr, "Content\\Common\\Shaders\\", "BloomExtract");
            m_bloomCombineEffect = new EffectResHandle(m_debugMgr, resourceMgr, "Content\\Common\\Shaders\\", "BloomCombine");
            m_gaussianBlurEffect = new EffectResHandle(m_debugMgr, resourceMgr, "Content\\Common\\Shaders\\", "GaussianBlur");
            #endregion

            #region cell statics

            #region rbc
            m_rbcStaticData = new Common.Cells.CellStaticData();
            m_rbcStaticData.staticCellType = Common.Cells.CellTypeEnum.RED_BLOOD_CELL;
            m_rbcStaticData.theshMaxHealth = GlobalConstants.RBC_theshMaxHealth;
            m_rbcStaticData.threshMaxBattleDefence = GlobalConstants.RBC_threshMaxBattleDefence;
            m_rbcStaticData.threshMaxBattleOffense = GlobalConstants.RBC_threshMaxBattleOffence;
            m_rbcStaticData.threshMaxNStore = GlobalConstants.RBC_threshMaxNStore;
            m_rbcStaticData.threshNToDivide = GlobalConstants.RBC_threshNToDivide;
            m_rbcStaticData.rateNutrientIncome = GlobalConstants.RBC_rateNutrientIncome;
            m_rbcStaticData.rateMaxVelocity = GlobalConstants.RBC_rateMaxVelocity;
            #endregion

            #region pbc
            m_pcStaticData = new Common.Cells.CellStaticData();
            m_pcStaticData.staticCellType = Common.Cells.CellTypeEnum.PLATELET;
            m_pcStaticData.theshMaxHealth = GlobalConstants.PLATELET_theshMaxHealth;
            m_pcStaticData.threshMaxBattleDefence = GlobalConstants.PLATELET_threshMaxBattleDefence;
            m_pcStaticData.threshMaxBattleOffense = GlobalConstants.PLATELET_threshMaxBattleOffence;
            m_pcStaticData.threshMaxNStore = GlobalConstants.PLATELET_threshMaxNStore;
            m_pcStaticData.threshNToDivide = GlobalConstants.PLATELET_threshNToDivide;
            m_pcStaticData.rateNutrientIncome = GlobalConstants.PLATELET_rateNutrientIncome;
            m_pcStaticData.rateMaxVelocity = GlobalConstants.PLATELET_rateMaxVelocity;
            #endregion

            #region silo
            m_siloStaticData = new Common.Cells.CellStaticData();
            m_siloStaticData.staticCellType = Common.Cells.CellTypeEnum.BIG_CELL_SILO;
            m_siloStaticData.theshMaxHealth = GlobalConstants.SILO_theshMaxHealth;
            m_siloStaticData.threshMaxBattleDefence = GlobalConstants.SILO_threshMaxBattleDefence;
            m_siloStaticData.threshMaxBattleOffense = GlobalConstants.SILO_threshMaxBattleOffence;
            m_siloStaticData.threshMaxNStore = GlobalConstants.SILO_threshMaxNStore;
            m_siloStaticData.threshNToDivide = GlobalConstants.SILO_threshNToDivide;
            m_siloStaticData.rateNutrientIncome = GlobalConstants.SILO_rateNutrientIncome;
            m_siloStaticData.rateMaxVelocity = GlobalConstants.SILO_rateMaxVelocity;
            #endregion

            #region tank
            m_tankStaticData = new Common.Cells.CellStaticData();
            m_tankStaticData.staticCellType = Common.Cells.CellTypeEnum.BIG_CELL_TANK;
            m_tankStaticData.theshMaxHealth = GlobalConstants.TANK_theshMaxHealth;
            m_tankStaticData.threshMaxBattleDefence = GlobalConstants.TANK_threshMaxBattleDefence;
            m_tankStaticData.threshMaxBattleOffense = GlobalConstants.TANK_threshMaxBattleOffence;
            m_tankStaticData.threshMaxNStore = GlobalConstants.TANK_threshMaxNStore;
            m_tankStaticData.threshNToDivide = GlobalConstants.TANK_threshNToDivide;
            m_tankStaticData.rateNutrientIncome = GlobalConstants.TANK_rateNutrientIncome;
            m_tankStaticData.rateMaxVelocity = GlobalConstants.TANK_rateMaxVelocity;
            #endregion

            #region small hybrid
            m_shyStaticData = new Common.Cells.CellStaticData();
            m_shyStaticData.staticCellType = Common.Cells.CellTypeEnum.SMALL_HYBRID;
            m_shyStaticData.theshMaxHealth = GlobalConstants.HYSMALL_theshMaxHealth;
            m_shyStaticData.threshMaxBattleDefence = GlobalConstants.HYSMALL_threshMaxBattleDefence;
            m_shyStaticData.threshMaxBattleOffense = GlobalConstants.HYSMALL_threshMaxBattleOffence;
            m_shyStaticData.threshMaxNStore = GlobalConstants.HYSMALL_threshMaxNStore;
            m_shyStaticData.threshNToDivide = GlobalConstants.HYSMALL_threshNToDivide;
            m_shyStaticData.rateNutrientIncome = GlobalConstants.HYSMALL_rateNutrientIncome;
            m_shyStaticData.rateMaxVelocity = GlobalConstants.HYSMALL_rateMaxVelocity;
            #endregion

            #region medium hybrid
            m_mhyStaticData = new Common.Cells.CellStaticData();
            m_mhyStaticData.staticCellType = Common.Cells.CellTypeEnum.MED_HYBRID;
            m_mhyStaticData.theshMaxHealth = GlobalConstants.HYMED_theshMaxHealth;
            m_mhyStaticData.threshMaxBattleDefence = GlobalConstants.HYMED_threshMaxBattleDefence;
            m_mhyStaticData.threshMaxBattleOffense = GlobalConstants.HYMED_threshMaxBattleOffence;
            m_mhyStaticData.threshMaxNStore = GlobalConstants.HYMED_threshMaxNStore;
            m_mhyStaticData.threshNToDivide = GlobalConstants.HYMED_threshNToDivide;
            m_mhyStaticData.rateNutrientIncome = GlobalConstants.HYMED_rateNutrientIncome;
            m_mhyStaticData.rateMaxVelocity = GlobalConstants.HYMED_rateMaxVelocity;
            #endregion

            #region big hybrid
            m_mhyStaticData = new Common.Cells.CellStaticData();
            m_mhyStaticData.staticCellType = Common.Cells.CellTypeEnum.BIG_HYBRID;
            m_mhyStaticData.theshMaxHealth = GlobalConstants.HYBIG_theshMaxHealth;
            m_mhyStaticData.threshMaxBattleDefence = GlobalConstants.HYBIG_threshMaxBattleDefence;
            m_mhyStaticData.threshMaxBattleOffense = GlobalConstants.HYBIG_threshMaxBattleOffence;
            m_mhyStaticData.threshMaxNStore = GlobalConstants.HYBIG_threshMaxNStore;
            m_mhyStaticData.threshNToDivide = GlobalConstants.HYBIG_threshNToDivide;
            m_mhyStaticData.rateNutrientIncome = GlobalConstants.HYBIG_rateNutrientIncome;
            m_mhyStaticData.rateMaxVelocity = GlobalConstants.HYBIG_rateMaxVelocity;
            #endregion

            #region wbc
            m_wbcStaticData = new Common.Cells.CellStaticData();
            m_wbcStaticData.staticCellType = Common.Cells.CellTypeEnum.WHITE_BLOOD_CELL;
            m_wbcStaticData.theshMaxHealth = GlobalConstants.WBC_theshMaxHealth;
            m_wbcStaticData.threshMaxBattleDefence = GlobalConstants.WBC_threshMaxBattleDefence;
            m_wbcStaticData.threshMaxBattleOffense = GlobalConstants.WBC_threshMaxBattleOffence;
            m_wbcStaticData.threshMaxNStore = GlobalConstants.WBC_threshMaxNStore;
            m_wbcStaticData.threshNToDivide = GlobalConstants.WBC_threshNToDivide;
            m_wbcStaticData.rateNutrientIncome = GlobalConstants.WBC_rateNutrientIncome;
            m_wbcStaticData.rateMaxVelocity = GlobalConstants.WBC_rateMaxVelocity;
            #endregion

            #endregion

            gamestate = new Common.StateCalculations(m_debugMgr, this);

            m_lastClupdateSent = TimeSpan.Zero;
            m_lastUCupdateSent = TimeSpan.Zero;

            m_sndImmuneCountDown = new SoundResHandle(debugMgr, resourceMgr, "Content\\Sounds\\", "GameOkCountDown");
            m_sndMedicAlertCountDown = new SoundResHandle(debugMgr, resourceMgr, "Content\\Sounds\\", "GameBadCountDown");
            m_sndMedicAlert = new SoundResHandle(debugMgr, resourceMgr, "Content\\Sounds\\", "GameMedAlert");
            m_sndGenAlert = new SoundResHandle(debugMgr, resourceMgr, "Content\\Sounds\\", "GameGenAlert");
            m_sndSplitCluster = new SoundResHandle(debugMgr, resourceMgr, "Content\\Sounds\\", "GameSplitCluster");
            m_sndBattleWarning = new SoundResHandle(debugMgr, resourceMgr, "Content\\Sounds\\", "GameBattleAlarm");

            #region fade overlay
            m_tutOverlayCam = new CameraGObj(
                uint.MaxValue, debugMgr, resourceMgr, this,
                false, graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000.0f);

            m_fadeOverlay = new QuadAsset(
                uint.MaxValue, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, new Microsoft.Xna.Framework.Graphics.Color(0f, 0f, 0f, 0.75f),
                m_debugMgr, m_resMgr, this, false, m_graphicsMgr);
            #endregion

            m_waitFont = new SpriteFontResHandle(m_debugMgr, m_resMgr,
                "Content\\Fonts\\", "MenuFont");
            //calcs for centre align
            FontPos =
                new Microsoft.Xna.Framework.Vector2(
                    graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth / 2f,
                    graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight / 2f);
        }

        #endregion

        #region create session

        public virtual void SetSession(SessionDetails sessionDetails)
        {
            m_sessionDetails = sessionDetails;
            m_sessionSet = true;
            m_cursor.Visible = false;

            m_timeGameStarted = TimeSpan.Zero;
            m_timeSinceLastMed = TimeSpan.Zero;
            m_timeSinceLastImmuneSysWave = TimeSpan.Zero;
            m_immuneSysWaveCount = 0;

            m_finishedRanksScrn = new FinishedRanksScreen(m_debugMgr, m_resMgr, this);

            //simulate internet
            //if (GlobalConstants.NET_SIM_INTERNET && m_sessionDetails.isMultiplayer)
            //{
            //    m_sessionDetails.netSessionComponent.GetNetworkSession.SimulatedLatency =
            //        TimeSpan.FromMilliseconds(GlobalConstants.NET_SIM_LAG_MILSECS);
            //    m_sessionDetails.netSessionComponent.GetNetworkSession.SimulatedPacketLoss =
            //        GlobalConstants.NET_SIM_PACKETLOSS_RATIO;
            //}

            //allow the game to wait for everyone to be ready
            m_clientSentReadyFlag = false;
            if (m_sessionDetails.isMultiplayer)
            {
                m_gameStarted = false;
                m_numNotReadyPlayers = m_sessionDetails.netSessionComponent.GetNetworkSession.AllGamers.Count - 1;
                IsPaused = true;
            }
            else
            {
                m_gameStarted = true;
                IsPaused = false;
            }

            //go through all gobjs setting each network entities' sessionDetails
            foreach (KeyValuePair<uint, IGameObject> gobjKVP in m_gameObjects)
            {
                if (gobjKVP.Value is Common.NetworkEntity)
                    ((Common.NetworkEntity)gobjKVP.Value).SetSessionDetails(sessionDetails);
            }

            //create viruses
            CreateUserViruses(CreateBotViruses());

            //create hud - with quick assert
            if (((CommonGameResScn)ParentScene).m_hud != null)
            {
                ((CommonGameResScn)ParentScene).m_hud.Unload();
                ((CommonGameResScn)ParentScene).m_hud.Deinit();
            }
            else
            {
                ((CommonGameResScn)ParentScene).m_hud = new HUDOverlay(m_debugMgr, m_resMgr, ParentScene, m_graphicsMgr,
                Stage.SceneMgr.Game.spriteBatch);
                m_gameObjects.Add(((CommonGameResScn)ParentScene).m_hud.Id, ((CommonGameResScn)ParentScene).m_hud);
            }

            ((CommonGameResScn)ParentScene).m_hud.BioScene = this;

            m_debugMgr.Assert(m_gameObjects[VirusGobjID(m_sessionDetails.myVirusId)] != null, "BiophageScn:SetSess - my virus doesn't exist.");
            m_debugMgr.Assert(m_gameObjects[VirusGobjID(m_sessionDetails.myVirusId)] is Common.Virus, "BiophageScn:SetSess - my virus isn't a virus obj.");
            m_debugMgr.Assert(((Common.Virus)m_gameObjects[VirusGobjID(m_sessionDetails.myVirusId)]).virusStateData.isMine,
                "BiophageScn:SetSess - my virus hasn't been set as mine.");

            ((CommonGameResScn)ParentScene).m_hud.m_myVirus = myVirus;
            myVirus.m_hud = ((CommonGameResScn)ParentScene).m_hud;

            ((CommonGameResScn)ParentScene).m_hud.Init();
            ((CommonGameResScn)ParentScene).m_hud.Load();
        }

        protected virtual void CreatePhysics()
        {
            //reinitialise the physics
            m_physicsWorld = new PhysicsSystem();
            m_physicsWorld.CollisionSystem = new CollisionSystemSAP();

            m_physicsWorld.SolverType = PhysicsSystem.Solver.Normal;

            m_physicsWorld.Gravity = new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f);
        }

        /// <returns>
        /// Next free virus Id to use.
        /// </returns>
        byte CreateBotViruses()
        {
            //create virus entities - bots first
            Common.Virus virus;

            Microsoft.Xna.Framework.Graphics.Color virColour = Microsoft.Xna.Framework.Graphics.Color.DarkSlateGray;
            byte virId = 0;
            int virPosLookup = 0;
            for (virId = 0; virId < m_sessionDetails.numBots; virId++)
            {
                VirusStateData virData = new VirusStateData();

                virColour.R += (byte)15; virColour.G += (byte)15; virColour.B += (byte)15;
                virData.clusters = new LinkedList<Common.CellCluster>();
                virData.infectPercentage = 0f;
                virData.isAlive = true;
                virData.isBot = true;
                virData.isMine = false;
                virData.colour = virColour;
                virData.numInfectedCells = 0;
                virData.rank = 0;

                virData.ownerName = "Bot " + virId.ToString();

                virPosLookup = virId % m_virusCapsidSpawnLocations.Length;
                virus = new Common.Virus(VirusGobjID(virId), m_sessionDetails,
                    virData,
                    m_virusCapsidSpawnLocations[virPosLookup],
                    m_cursor, m_debugMgr, m_resMgr, this);

                virus.Init();
                virus.Load();

                //add to virus to list
                m_viruses.AddLast(virId);
                m_virusObjs.AddLast(virus);
            }

            return virId;
        }

        void CreateUserViruses(byte startId)
        {
            byte virusId = startId;
            Common.Virus virus;

            //now the users
            int virPosLookup = 0;
            if (m_sessionDetails.isMultiplayer)
            {
                foreach (Microsoft.Xna.Framework.Net.NetworkGamer gamer in
                    m_sessionDetails.netSessionComponent.GetNetworkSession.AllGamers)
                {
                    VirusStateData virData = new VirusStateData();

                    virData.clusters = new LinkedList<Common.CellCluster>();
                    virData.infectPercentage = 0f;
                    virData.isAlive = true;
                    virData.isBot = false;
                    virData.isMine = false;
                    virData.colour = GamerColour(gamer.Id);
                    virData.numInfectedCells = 0;
                    virData.rank = 0;
                    virData.netPlayerId = gamer.Id;

                    virData.ownerName = gamer.Gamertag;

                    //set as own if tag match
                    if (m_sessionDetails.gamerMe.Id == gamer.Id)
                    {
                        m_sessionDetails.myVirusId = virusId;
                        virData.isMine = true;

                        virPosLookup = virusId % m_virusCapsidSpawnLocations.Length;
                        virus = new Common.Virus(VirusGobjID(virusId), m_sessionDetails,
                            virData,
                            m_virusCapsidSpawnLocations[virPosLookup],
                            m_cursor, m_debugMgr, m_resMgr, this);

                        myVirus = virus;
                    }
                    else
                    {
                        virPosLookup = virusId % m_virusCapsidSpawnLocations.Length;
                        virus = new Common.Virus(VirusGobjID(virusId), m_sessionDetails,
                            virData,
                            m_virusCapsidSpawnLocations[virPosLookup],
                            m_cursor, m_debugMgr, m_resMgr, this);
                    }

                    virus.Init();
                    virus.Load();

                    //add to virus to list
                    m_viruses.AddLast(virusId);
                    m_virusObjs.AddLast(virus);

                    virusId++;
                }
            }
            else
            {
                VirusStateData virData = new VirusStateData();

                virData.clusters = new LinkedList<Common.CellCluster>();
                virData.infectPercentage = 0f;
                virData.isAlive = true;
                virData.isBot = false;
                virData.isMine = true;
                virData.colour = Microsoft.Xna.Framework.Graphics.Color.GreenYellow;
                virData.numInfectedCells = 0;
                virData.rank = 0;
                virData.netPlayerId = 0;

                //set as own if tag match
                m_sessionDetails.myVirusId = virusId;
                virPosLookup = virusId % m_virusCapsidSpawnLocations.Length;
                virus = new Common.Virus(VirusGobjID(virusId), m_sessionDetails,
                    virData,
                    m_virusCapsidSpawnLocations[virPosLookup],
                    m_cursor, m_debugMgr, m_resMgr, this);

                myVirus = virus;

                virus.Init();
                virus.Load();

                //add to virus to list
                m_viruses.AddLast(virusId);
                m_virusObjs.AddLast(virus);
            }
        }

        #endregion

        #region field accessors

        public SessionDetails GetSessionDetails
        {
            get { return m_sessionDetails; }
            set { m_sessionDetails = value; }
        }

        public HUDOverlay GetHUD
        {
            get { return ((CommonGameResScn)ParentScene).m_hud; }
        }

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                m_camera = new Common.FollowCamera(
                    m_camId, m_debugMgr, m_resMgr, this,
                    m_graphicsMgr,
                    Microsoft.Xna.Framework.Vector3.Zero,
                    Microsoft.Xna.Framework.Vector3.Up,
                    Microsoft.Xna.Framework.Vector3.Forward,
                    0.0125f, 220f, 10f);

                m_cursor = new Common.PlayerCursor(
                    m_cursorId, Microsoft.Xna.Framework.Vector3.Zero,
                    m_debugMgr, m_resMgr, this);

                if ((!m_tutOverlayCam.Init()) ||
                    (!m_fadeOverlay.Init()))
                    retVal = false;

                if (!base.Init())
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }

            return retVal;
        }

        public override bool Reinit()
        {
            bool retVal = true;

            retVal = base.Reinit();

            if ((!m_tutOverlayCam.Reinit()) ||
                (!m_fadeOverlay.Reinit()))
                retVal = false;

            if (retVal)
                m_isInit = true;
            else
                m_isInit = false;

            return retVal;
        }

        #region loading

        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if (!base.Load())
                    retVal = false;

                if ((!m_tutOverlayCam.Load()) ||
                    (!m_fadeOverlay.Load()) ||

                    (!m_waitFont.Load()) ||
                    
                    (!m_sndImmuneCountDown.Load()) ||
                    (!m_sndMedicAlertCountDown.Load()) ||
                    (!m_sndGenAlert.Load()) ||
                    (!m_sndMedicAlert.Load()) ||
                    (!m_sndSplitCluster.Load()) ||
                    (!m_sndBattleWarning.Load()) ||

                    (!m_bloomExtractEffect.Load()) ||
                    (!m_bloomCombineEffect.Load()) ||
                    (!m_gaussianBlurEffect.Load()))
                    retVal = false;

                #region bloom effect
                //resolve target for bloom effect
                m_resolveTarget = new Microsoft.Xna.Framework.Graphics.ResolveTexture2D(
                    m_graphicsMgr.GraphicsDevice,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight,
                    1,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferFormat);

                // Create two rendertargets for the bloom processing. These are half the
                // size of the backbuffer, in order to minimize fillrate costs. Reducing
                // the resolution in this way doesn't hurt quality, because we are going
                // to be blurring the bloom images in any case.
                m_renderTarget1 = new Microsoft.Xna.Framework.Graphics.RenderTarget2D(
                    m_graphicsMgr.GraphicsDevice,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth / 2,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight / 2,
                    1,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferFormat);

                m_renderTarget2 = new Microsoft.Xna.Framework.Graphics.RenderTarget2D(
                    m_graphicsMgr.GraphicsDevice,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferWidth / 2,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferHeight / 2,
                    1,
                    m_graphicsMgr.GraphicsDevice.PresentationParameters.BackBufferFormat);
                #endregion

                FontOrigin = ((Microsoft.Xna.Framework.Graphics.SpriteFont)m_waitFont.GetResource)
                    .MeasureString(m_waitMsg) / 2;

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
                if (!base.Unload())
                    retVal = false;

                if ((!m_tutOverlayCam.Unload()) ||
                    (!m_fadeOverlay.Unload()) ||

                    (!m_waitFont.Unload()) ||
                    
                    (!m_sndImmuneCountDown.Unload()) ||
                    (!m_sndMedicAlertCountDown.Unload()) ||
                    (!m_sndGenAlert.Unload()) ||
                    (!m_sndMedicAlert.Unload()) ||
                    (!m_sndSplitCluster.Unload()) ||
                    (!m_sndBattleWarning.Unload()) ||

                    (!m_bloomExtractEffect.Unload()) ||
                    (!m_bloomCombineEffect.Unload()) ||
                    (!m_gaussianBlurEffect.Unload()))
                    retVal = false;

                //dispose resolvetargets
                m_resolveTarget.Dispose();
                m_renderTarget1.Dispose();
                m_renderTarget2.Dispose();

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

                if ((!m_tutOverlayCam.Deinit()) ||
                    (!m_fadeOverlay.Deinit()))
                    retVal = false;

                m_sessionSet = false;
                m_menu.SetDefaultWindow(GlobalConstants.GMMAIN_WND_ID);
                m_menu.Active = false;

                //clear out the viruses
                foreach (byte virId in m_viruses)
                {
                    m_debugMgr.Assert(m_gameObjects[VirusGobjID(virId)] != null,
                        "BiophageScn:Deinit - vir id to obj ref is null.");
                    m_debugMgr.Assert(m_gameObjects[VirusGobjID(virId)] is Common.Virus,
                        "BiophageScn:Deinit - vir id to obj ref is not a virus type.");

                    m_gameObjects.Remove(VirusGobjID(virId));
                }
                m_viruses.Clear();
                m_virusObjs.Clear();

                //clear out the clusters
                foreach (byte clusterId in m_cellClusters)
                {
                    m_debugMgr.Assert(m_gameObjects[ClusterGobjID(clusterId)] != null,
                        "BiophageScn:Deinit - cluster id to obj ref is null.");
                    m_debugMgr.Assert(m_gameObjects[ClusterGobjID(clusterId)] is Common.CellCluster,
                        "BiophageScn:Deinit - cluster id to obj ref is not a cluster type.");

                    m_gameObjects.Remove(ClusterGobjID(clusterId));
                }
                m_cellClusters.Clear();
                m_whiteBloodCells.Clear();
                m_cellClusterObjs.Clear();

                //clear out the uninfected cells
                foreach (byte uCellId in m_uninfectedCells)
                {
                    m_debugMgr.Assert(m_gameObjects[UCellGobjID(uCellId)] != null,
                        "BiophageScn:Deinit - ucell id to obj ref is null.");
                    m_debugMgr.Assert(m_gameObjects[UCellGobjID(uCellId)] is Common.Cells.UninfectedCell,
                        "BiophageScn:Deinit - ucell id to obj ref is not a ucell type.");

                    m_gameObjects.Remove(UCellGobjID(uCellId));
                }
                m_uninfectedCells.Clear();
                m_ucellObjs.Clear();

                //clear out hud
                m_gameObjects.Remove(((CommonGameResScn)ParentScene).m_hud.Id);

                //clear out environment
                m_gameObjects.Remove(m_levelEnvId);

                //and whatever else
                m_gameObjects.Clear();

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        #endregion

        #region virus methods

        #region UCell stuff

        public void UCellRemove(byte ucellId)
        {
            //asserts
            m_debugMgr.Assert(m_gameObjects[UCellGobjID(ucellId)] != null,
                "BiophageScn:UCellRemove - game object doesn't exist.");
            m_debugMgr.Assert(m_gameObjects[UCellGobjID(ucellId)] is Common.Cells.UninfectedCell,
                "BiophageScn:UCellRemove - game object isn't ucell type.");

            Common.Cells.UninfectedCell ucell = (Common.Cells.UninfectedCell)m_gameObjects[UCellGobjID(ucellId)];

            //deactivate, unload, deinit, and remove
            ucell.Active = false;
            ucell.Unload();
            ucell.Deinit();
            m_uninfectedCells.Remove(ucellId);
            m_ucellObjs.Remove(ucell);
            m_gameObjects.Remove(UCellGobjID(ucellId));
        }

        public Common.ClusterStateData UCellToClusterData(Common.Cells.UninfectedCell ucell, byte virusOwnerId)
        {
            Common.ClusterStateData clData = new Common.ClusterStateData();
            clData.attrHealth = 100;
            clData.attrNIncome = ucell.StaticData.rateNutrientIncome;
            clData.attrNutrientStore = ucell.StaticData.threshMaxNStore;
            clData.attrVelocity = ucell.StaticData.rateMaxVelocity;
            clData.virusOwnerId = virusOwnerId;
            clData.virusRef = (Common.Virus)m_gameObjects[VirusGobjID(virusOwnerId)];
            clData.biophageScn = this;

            switch (ucell.StaticData.staticCellType)
            {
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.RED_BLOOD_CELL:
                    clData.numRBCs = 1;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.PLATELET:
                    clData.numPlatelets = 1;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_SILO:
                    clData.numSilos = 1;
                    break;
                case Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_TANK:
                    clData.numTanks = 1;
                    break;
            }
            clData.numCellsTotal = 1;

            clData.actionState = CellActionState.IDLE;
            clData.actionReObject = null;

            return clData;
        }

        #endregion

        #region cluster stuff

        public void ClusterDie(byte clusterId)
        {
            m_debugMgr.Assert(m_cellClusters.Contains(clusterId),
                "BaseGameScene:ClusterKill - no cluster Id match found.");
            m_debugMgr.Assert(m_gameObjects[ClusterGobjID(clusterId)] is Common.CellCluster,
                "BaseGameScene:ClusterKill - cluster Id is not a CellCluster class object.");

            Common.CellCluster cluster = (Common.CellCluster)m_gameObjects[ClusterGobjID(clusterId)];

            //just remove cluster - from virus, cluster, and scene
            if (cluster.stateData.numWhiteBloodCell == 0)
                cluster.stateData.virusRef.virusStateData.clusters.Remove(cluster);
            else
                m_whiteBloodCells.Remove(cluster);

            m_cellClusters.Remove(clusterId);
            m_cellClusterObjs.Remove(cluster);
            cluster.Unload();
            cluster.Deinit();
            m_gameObjects.Remove(ClusterGobjID(clusterId));
        }

        Common.CellCluster HostClusterNew(Common.ClusterStateData clusterData, Microsoft.Xna.Framework.Vector3 pos)
        {
            //this can only be done by server to stop conflicts
            m_debugMgr.Assert(m_sessionDetails.isHost,
                "GameBase:NewCluster - should only be called by server.");

            //see if there are any inactive cluster Id's to recycle
            byte newClusterId = 0;
            bool noSpace = true;
            while (noSpace)
            {
                if (m_cellClusters.Contains(newClusterId))
                {
                    newClusterId++;
                    m_debugMgr.Assert(newClusterId < byte.MaxValue,
                        "BiophageScn:NewCluster - max number cluster reached.");
                }
                else
                    noSpace = false;
            }

            Common.CellCluster newCluster = new Common.CellCluster(
                    ClusterGobjID(newClusterId),
                    clusterData, pos, m_sessionDetails,
                    m_debugMgr, m_resMgr, this,
                    ((CommonGameResScn)ParentScene).m_hud);

            m_cellClusters.AddLast(newClusterId);
            m_cellClusterObjs.AddLast(newCluster);

            //return
            return newCluster;
        }

        Common.CellCluster HostNewWhiteBloodCell(Microsoft.Xna.Framework.Vector3 pos)
        {
            //this can only be done by server to stop conflicts
            m_debugMgr.Assert(m_sessionDetails.isHost,
                "GameBase:HostNewWhiteBloodCell - should only be called by server.");

            //see if there are any inactive clusters to recycle
            byte newClusterId = 0;
            bool noSpace = true;
            while (noSpace)
            {
                if (m_cellClusters.Contains(newClusterId))
                {
                    newClusterId++;
                    m_debugMgr.Assert(newClusterId < byte.MaxValue,
                        "BiophageScn:NewCluster - max number cluster reached.");
                }
                else
                    noSpace = false;
            }

            Common.ClusterStateData clData = new ClusterStateData();
            clData.numWhiteBloodCell = 1;
            clData.maxHealth = 100;
            clData.maxNutrientStore = 0;
            clData.maxVelocity = m_wbcStaticData.rateMaxVelocity;
            clData.numCellsTotal = 1;
            clData.virusOwnerId = GlobalConstants.GP_WHITE_BLOODCELL_VIRUS_ID;
            clData.virusRef = null;
            clData.biophageScn = this;
            clData.actionState = CellActionState.IDLE;
            clData.actionReObject = null;
            clData.attrHealth = 100;
            clData.attrNIncome = 0.0f;
            clData.attrNutrientStore = 0;
            clData.attrVelocity = m_wbcStaticData.rateMaxVelocity;

            Common.CellCluster newCluster = new Common.CellCluster(
                    ClusterGobjID(newClusterId),
                    clData, pos, m_sessionDetails,
                    m_debugMgr, m_resMgr, this,
                    ((CommonGameResScn)ParentScene).m_hud);

            m_cellClusters.AddLast(newClusterId);
            m_whiteBloodCells.AddLast(newCluster);
            m_cellClusterObjs.AddLast(newCluster);

            //return
            return newCluster;
        }

        public void CombineClusters(Common.CellCluster clusterA, Common.CellCluster clusterB)
        {
            #region asserts
            m_debugMgr.Assert(clusterA.Active,
                "BaseGameScene:CombineClusters - cluster A is dead (inactive).");
            m_debugMgr.Assert(clusterB.Active,
                "BaseGameScene:CombineClusters - cluster B is dead (inactive).");

            m_debugMgr.Assert(clusterA.stateData.virusOwnerId == clusterB.stateData.virusOwnerId,
                "BaseGameScene:CombineCells - clusters are not from the same virus.");
            #endregion

            //add values
            clusterA.stateData.attrHealth += clusterB.stateData.attrHealth;
            clusterA.stateData.attrNutrientStore += clusterB.stateData.attrNutrientStore;

            clusterA.stateData.numRBCs += clusterB.stateData.numRBCs;
            clusterA.stateData.numPlatelets += clusterB.stateData.numPlatelets;
            clusterA.stateData.numSilos += clusterB.stateData.numSilos;
            clusterA.stateData.numTanks += clusterB.stateData.numTanks;
            clusterA.stateData.numSmallHybrids += clusterB.stateData.numSmallHybrids;
            clusterA.stateData.numMediumHybrids += clusterB.stateData.numMediumHybrids;
            clusterA.stateData.numBigHybrids += clusterB.stateData.numBigHybrids;

            //readjust cluster data
            clusterA.ReadjustAll();

            //now 'kill' the added cluster so that it can be recycled
            clusterB.Active = false;
        }

        Common.CellCluster HostClustersBattle(Common.CellCluster clusterA, Common.CellCluster clusterB)
        {
            #region asserts
            m_debugMgr.Assert(m_sessionDetails.isHost,
                "BiophageScn:ClustersBattle - only host can call this method.");

            m_debugMgr.Assert(clusterA.stateData.virusOwnerId != clusterB.stateData.virusOwnerId,
                "BaseGameScene:BattleClusters - clusters are from the same virus.");
            #endregion

            int AOffence = clusterA.stateData.maxBattleOffence - clusterB.stateData.maxBattleDefence;
            int BOffence = clusterB.stateData.maxBattleOffence - clusterA.stateData.maxBattleDefence;
            double winHealthMod = 1.0;

            if (AOffence < 0)
                AOffence = 0;
            if (BOffence < 0)
                BOffence = 0;

            Common.CellCluster winner, loser;

            if (AOffence > BOffence)
            {
                winner = clusterA;
                loser = clusterB;
                if (AOffence == 0)
                    AOffence++;
                winHealthMod = 1.0 - (BOffence / AOffence);
            }
            else if (BOffence > AOffence)
            {
                winner = clusterB;
                loser = clusterA;
                if (BOffence == 0)
                    BOffence++;
                winHealthMod = 1.0 - (AOffence / BOffence);
            }
            else
            {
                //let cluster A be the winner since it was the one whom initiated the attack
                winner = clusterA;
                loser = clusterB;
                winHealthMod = 0.2;
            }

            //conform win mod
            if (winHealthMod > 0.9)
                winHealthMod = 0.9;
            if (winHealthMod <= 0.0)
                winHealthMod = 0.2;

            //though if both are equal, the other will be drastically hurt
            winner.stateData.attrHealth = (short)(winHealthMod * (double)winner.stateData.attrHealth);
            ReadjustMaxHealth(winner);
            ClusterRegulateHealth(winner);
            winner.ReadjustAll();

            //kill loser cluster
            loser.Active = false;

            return winner;
        }

        private void ReadjustMaxHealth(CellCluster winner)
        {
            winner.stateData.maxHealth = (short)(winner.stateData.numCellsTotal * 100);
        }

        //removes cells from cluster to make it comply with new health value.
        void ClusterRegulateHealth(CellCluster cluster)
        {
            //Determine the number of cells that need to be removed
            int numCellsRemove = (int)((double)(cluster.stateData.maxHealth - cluster.stateData.attrHealth) * 0.01);

            m_debugMgr.Assert(numCellsRemove < cluster.stateData.numCellsTotal,
                "BiophageScn:ClusterRegulateHealth - number of cells to remove is greater than total cell count.");

            if (numCellsRemove == 0)
                return;

            if (cluster.stateData.numWhiteBloodCell != 0)
            {
                cluster.stateData.attrHealth = 0;
                cluster.stateData.numCellsTotal = 0;
            }

            #region initial removes
            //remove RBCs
            if ((cluster.stateData.numRBCs < numCellsRemove) &&
                (cluster.stateData.numRBCs > 1))
            {
                numCellsRemove -= (ushort)(cluster.stateData.numRBCs - 1);
                cluster.stateData.numRBCs = 1;
            }
            else if (cluster.stateData.numRBCs > 1)
            {
                cluster.stateData.numRBCs -= (byte)numCellsRemove;
                return;
            }

            //remove Platelets
            if ((cluster.stateData.numPlatelets < numCellsRemove) &&
                (cluster.stateData.numPlatelets > 1))
            {
                numCellsRemove -= (ushort)(cluster.stateData.numPlatelets - 1);
                cluster.stateData.numPlatelets = 1;
            }
            else if (cluster.stateData.numPlatelets > 1)
            {
                cluster.stateData.numPlatelets -= (byte)numCellsRemove;
                return;
            }

            //remove small hybrids
            if ((cluster.stateData.numSmallHybrids < numCellsRemove) &&
                (cluster.stateData.numSmallHybrids > 1))
            {
                numCellsRemove -= (ushort)(cluster.stateData.numSmallHybrids - 1);
                cluster.stateData.numSmallHybrids = 1;
            }
            else if (cluster.stateData.numSmallHybrids > 1)
            {
                cluster.stateData.numSmallHybrids -= (byte)numCellsRemove;
                return;
            }

            //remove medium hybrids
            if ((cluster.stateData.numMediumHybrids < numCellsRemove) &&
                (cluster.stateData.numMediumHybrids > 1))
            {
                numCellsRemove -= (ushort)(cluster.stateData.numMediumHybrids - 1);
                cluster.stateData.numMediumHybrids = 1;
            }
            else if (cluster.stateData.numMediumHybrids > 1)
            {
                cluster.stateData.numMediumHybrids -= (byte)numCellsRemove;
                return;
            }

            //remove tanks
            if ((cluster.stateData.numTanks < numCellsRemove) &&
                (cluster.stateData.numTanks > 1))
            {
                numCellsRemove -= (ushort)(cluster.stateData.numTanks - 1);
                cluster.stateData.numTanks = 1;
            }
            else if (cluster.stateData.numTanks > 1)
            {
                cluster.stateData.numTanks -= (byte)numCellsRemove;
                return;
            }

            //remove big hybrids
            if ((cluster.stateData.numBigHybrids < numCellsRemove) &&
                (cluster.stateData.numBigHybrids > 1))
            {
                numCellsRemove -= (ushort)(cluster.stateData.numBigHybrids - 1);
                cluster.stateData.numBigHybrids = 1;
            }
            else if (cluster.stateData.numBigHybrids > 1)
            {
                cluster.stateData.numBigHybrids -= (byte)numCellsRemove;
                return;
            }

            //remove silos
            if ((cluster.stateData.numSilos < numCellsRemove) &&
                (cluster.stateData.numSilos > 1))
            {
                numCellsRemove -= (ushort)(cluster.stateData.numSilos - 1);
                cluster.stateData.numSilos = 1;
            }
            else if (cluster.stateData.numSilos > 1)
            {
                cluster.stateData.numSilos -= (byte)numCellsRemove;
                return;
            }
            #endregion

            #region clearout leftovers

            if (cluster.stateData.numRBCs == 1)
            {
                cluster.stateData.numRBCs = 0; numCellsRemove--;
                if (numCellsRemove <= 0)
                    return;
            }
            if (cluster.stateData.numPlatelets == 1)
            {
                cluster.stateData.numPlatelets = 0; numCellsRemove--;
                if (numCellsRemove <= 0)
                    return;
            }
            if (cluster.stateData.numSmallHybrids == 1)
            {
                cluster.stateData.numSmallHybrids = 0; numCellsRemove--;
                if (numCellsRemove <= 0)
                    return;
            }
            if (cluster.stateData.numMediumHybrids == 1)
            {
                cluster.stateData.numMediumHybrids = 0; numCellsRemove--;
                if (numCellsRemove <= 0)
                    return;
            }
            if (cluster.stateData.numTanks == 1)
            {
                cluster.stateData.numTanks = 0; numCellsRemove--;
                if (numCellsRemove <= 0)
                    return;
            }
            if (cluster.stateData.numBigHybrids == 1)
            {
                cluster.stateData.numBigHybrids = 0; numCellsRemove--;
                if (numCellsRemove <= 0)
                    return;
            }
            if (cluster.stateData.numSilos == 1)
            {
                cluster.stateData.numSilos = 0; numCellsRemove--;
                if (numCellsRemove <= 0)
                    return;
            }

            #endregion
        }

        /// <summary>
        /// Generates cluster state data for a new cluster from a split command.
        /// Also readjusts src cluster.
        /// </summary>
        public Common.ClusterStateData SplitClusterToData(Common.CellCluster srcCluster,
            byte splRBC, byte splPLT, byte splTNK, byte splSIL,
            byte splSHY, byte splMHY, byte splBHY)
        {
            #region asserts

            m_debugMgr.Assert(srcCluster.stateData.numRBCs >= splRBC,
                "BiophageScn:SplitClusterToData - split RBC count is more than numRBCs.");
            m_debugMgr.Assert(srcCluster.stateData.numPlatelets >= splPLT,
                "BiophageScn:SplitClusterToData - split PLT count is more than numPLTs.");
            m_debugMgr.Assert(srcCluster.stateData.numTanks >= splTNK,
                "BiophageScn:SplitClusterToData - split TNK count is more than numTNKs.");
            m_debugMgr.Assert(srcCluster.stateData.numSilos >= splSIL,
                "BiophageScn:SplitClusterToData - split SIL count is more than numSILs.");
            m_debugMgr.Assert(srcCluster.stateData.numSmallHybrids >= splSHY,
                "BiophageScn:SplitClusterToData - split SHY count is more than numSHYs.");
            m_debugMgr.Assert(srcCluster.stateData.numMediumHybrids >= splMHY,
                "BiophageScn:SplitClusterToData - split MHY count is more than numMHYs.");
            m_debugMgr.Assert(srcCluster.stateData.numBigHybrids >= splBHY,
                "BiophageScn:SplitClusterToData - split BHY count is more than numBHYs.");

            #endregion

            Common.ClusterStateData clData = new Common.ClusterStateData();

            //health is a bit complex, so do it first
            double dHealth = (double)srcCluster.stateData.attrHealth / (double)srcCluster.stateData.maxHealth;

            clData.numCellsTotal = 0;
            clData.numRBCs = splRBC; srcCluster.stateData.numRBCs -= splRBC;
            clData.numPlatelets = splPLT; srcCluster.stateData.numPlatelets -= splPLT;
            clData.numTanks = splTNK; srcCluster.stateData.numTanks -= splTNK;
            clData.numSilos = splSIL; srcCluster.stateData.numSilos -= splSIL;

            clData.numSmallHybrids = splSHY; srcCluster.stateData.numSmallHybrids -= splSHY;
            clData.numMediumHybrids = splMHY; srcCluster.stateData.numMediumHybrids -= splMHY;
            clData.numBigHybrids = splBHY; srcCluster.stateData.numBigHybrids -= splBHY;

            ReadjustMaximums(ref clData);
            srcCluster.ReadjustMaximums();

            clData.attrHealth = (short)(dHealth * (double)clData.maxHealth);
            srcCluster.stateData.attrHealth = (short)(dHealth * (double)srcCluster.stateData.maxHealth);

            srcCluster.ReadjustAll();

            clData.attrNutrientStore = 0;
            if (srcCluster.stateData.attrNutrientStore > srcCluster.stateData.maxNutrientStore)
                srcCluster.stateData.attrNutrientStore = srcCluster.stateData.maxNutrientStore;

            clData.virusOwnerId = srcCluster.stateData.virusOwnerId;
            clData.virusRef = srcCluster.stateData.virusRef;

            clData.actionState = CellActionState.IDLE;
            clData.actionReObject = null;

            clData.biophageScn = this;

            return clData;
        }

        void ReadjustMaximums(ref Common.ClusterStateData stateData)
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

        public void ClusterInfectCell(byte clusterId, byte cellId)
        {
            #region asserts
            m_debugMgr.Assert(m_cellClusters.Contains(clusterId),
                "BaseGameScene:ClusterInfectCell - no cluster match found.");
            m_debugMgr.Assert(m_uninfectedCells.Contains(cellId),
                "BaseGameScene:ClusterInfectCell - no cell match found.");

            m_debugMgr.Assert(m_gameObjects[ClusterGobjID(clusterId)] is Common.CellCluster,
                "BaseGameScene:ClusterInfectCell - cluster id is not a CellCluster class object.");
            m_debugMgr.Assert(m_gameObjects[UCellGobjID(cellId)] is Common.Cells.UninfectedCell,
                "BaseGameScene:ClusterInfectCell - cell id is not a cell class object.");

            Common.CellCluster cluster = (Common.CellCluster)m_gameObjects[ClusterGobjID(clusterId)];
            Common.Cells.UninfectedCell cell = (Common.Cells.UninfectedCell)m_gameObjects[UCellGobjID(cellId)];

            m_debugMgr.Assert(cluster.Active,
                "BaseGameScene:ClusterInfectCell - cluster is dead (inactive).");
            m_debugMgr.Assert(cell.Active,
                "BaseGameScene:ClusterInfectCell - cell is gone (inactive).");
            #endregion

            //take on that cell's abilities
            switch (cell.StaticData.staticCellType)
            {
                case Common.Cells.CellTypeEnum.RED_BLOOD_CELL:
                    cluster.stateData.numRBCs += 1;
                    break;
                case Common.Cells.CellTypeEnum.PLATELET:
                    cluster.stateData.numPlatelets += 1;
                    break;
                case Common.Cells.CellTypeEnum.BIG_CELL_SILO:
                    cluster.stateData.numSilos += 1;
                    break;
                case Common.Cells.CellTypeEnum.BIG_CELL_TANK:
                    cluster.stateData.numTanks += 1;
                    break;
            }
            cluster.stateData.attrHealth += cell.StaticData.theshMaxHealth;
            cluster.stateData.attrNIncome += cell.StaticData.rateNutrientIncome;
            cluster.ReadjustAll();

            //kill cell
            cell.Active = false;
        }

        public void ClusterMedication(Common.Cells.CellTypeEnum cellTarget)
        {
            byte[] clustIdArray = m_cellClusters.ToArray();
            for (int i = 0; i < clustIdArray.Length; ++i)
            {
                if (m_cellClusters.Contains(clustIdArray[i]))
                {
                    Common.CellCluster clust = (Common.CellCluster)m_gameObjects[ClusterGobjID(clustIdArray[i])];

                    switch (cellTarget)
                    {
                        case Common.Cells.CellTypeEnum.RED_BLOOD_CELL:
                            clust.stateData.numRBCs = 0;
                            break;
                        case Common.Cells.CellTypeEnum.PLATELET:
                            clust.stateData.numPlatelets = 0;
                            break;
                        case Common.Cells.CellTypeEnum.BIG_CELL_TANK:
                            clust.stateData.numTanks = 0;
                            break;
                        case Common.Cells.CellTypeEnum.BIG_CELL_SILO:
                            clust.stateData.numSilos = 0;
                            break;
                    }

                    clust.ReadjustAll();
                }
            }
        }

        #endregion

        #endregion

        #region game loop

        public override void Input(Microsoft.Xna.Framework.GameTime gameTime,
                                    ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
)
        {
            #region game menu

            if (newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.Start)
#if !XBOX
 || newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape)
#endif
)
            {
                GetMenu.Active = true;
                //if single player - pause game
                if (!m_sessionDetails.isMultiplayer)
                    IsPaused = true;
            }

            #endregion

            //ignore input until game has started
            if (m_gameStarted)
            {
                #region for player's virus

                if ((myVirus != null) && (myVirus.Active))
                {
                    myVirus.Input(gameTime,
                            ref newGPState, ref m_prevGPState
#if !XBOX
, ref newKBState, ref m_prevKBState
#endif
);
                }
                else
                {
                    if (!m_cursor.Visible)
                    {
                        m_cursor.Visible = true;
                        m_cursor.Position = Microsoft.Xna.Framework.Vector3.Zero;
                        m_cursor.PhysBody.EnableBody();
                    }

                    m_cursor.Input(gameTime,
                            ref newGPState, ref m_prevGPState
#if !XBOX
, ref newKBState, ref m_prevKBState
#endif
);
                }

                #endregion
            }
            else
            {
                m_debugMgr.Assert(m_sessionDetails.isMultiplayer,
                    "BiophageScn:Update - m_gameStarted is false yet this is not a multiplayer session (which is implied).");

                //do network update - awaiting ready state
                if (m_sessionDetails.isHost)
                    HostNetworkUpdate(gameTime);
                else
                    ClientNetworkUpdate(gameTime);
            }
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_debugMgr.Assert(m_sessionSet,
                "GameBaseScene:Update - game has not been flaged as started.");

            if (m_gameStarted)
            {
                if (!m_finishedRanksScrn.isGameOver)
                {
                    //remove inactive virus capsids
                    foreach (Virus vir in m_virusObjs)
                    {
                        if (!vir.m_virusCapsid.Active)
                            vir.m_virusCapsid.Deinit();

                        //signal not under attack
                        vir.m_underAttack = false;
                    }

                    if (m_sessionDetails.isHost)
                    {
                        #region physics step

                        float timeStep = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
                        m_physicsWorld.Integrate(timeStep);

                        #endregion
                    }

                    //cluster linear update
                    byte[] clustIdArray = m_cellClusters.ToArray();
                    for (int i = 0; i < clustIdArray.Length; ++i)
                    {
                        if (m_cellClusters.Contains(clustIdArray[i]))
                        {
                            Common.CellCluster clust = (Common.CellCluster)m_gameObjects[ClusterGobjID(clustIdArray[i])];
                            clust.LinearUpdate(gameTime);

                            if ((clust.stateData.virusRef != null) && (clust.stateData.attnUnderAttack))
                                clust.stateData.virusRef.m_underAttack = true;
                        }
                    }

                    //do network update
                    if ((m_sessionDetails.isMultiplayer) && (m_sessionDetails.isHost))
                        HostNetworkUpdate(gameTime);


                    #region is client

                    else
                    {
                        #region physics collision

                        m_physicsWorld.IntegrateCollisions();

                        #endregion

                        #region recieve network

                        if (m_sessionDetails.isMultiplayer)
                            ClientNetworkUpdate(gameTime);

                        #endregion
                    }

                    #endregion
                }
            }
        }

        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (!m_finishedRanksScrn.isGameOver)
            {
                #region is host

                if ((m_sessionDetails.isHost) && (m_gameStarted))
                {
                    ModerateAI(gameTime);

                    #region remove inactive clusters
                    //remove inactive clusters - note that cluster can die during this, so work singularly
                    byte[] idArray = m_cellClusters.ToArray();
                    for (int i = 0; i < idArray.Length; ++i)
                    {
                        if (m_cellClusters.Contains(idArray[i]))
                        {
                            Common.CellCluster clust = (Common.CellCluster)m_gameObjects[ClusterGobjID(idArray[i])];
                            if (!clust.Active)
                                ClusterDie(idArray[i]);
                        }
                    }
                    #endregion

                    #region remove inactive ucells
                    idArray = m_uninfectedCells.ToArray();
                    for (int i = 0; i < idArray.Length; ++i)
                    {
                        if (m_uninfectedCells.Contains(idArray[i]))
                        {
                            Common.Cells.UninfectedCell ucell = (Common.Cells.UninfectedCell)m_gameObjects[UCellGobjID(idArray[i])];
                            if (!ucell.Active)
                                UCellRemove(idArray[i]);
                        }
                    }
                    #endregion

                    #region check session conditions

                    //check for dead viruses
                    foreach (Virus virus in m_virusObjs)
                    {
                        if ((!virus.Active) && (!virus.hasBeenRanked))
                        {
                            //add dead virus to the rank stack
                            m_finishedRanksScrn.finishVirusRanks.Push(GobjIDVirus(virus.Id));
                            virus.hasBeenRanked = true;
                        }
                    }

                    //check game conditions
                    HostCheckGameConditions(gameTime);

                    #endregion
                }

                #endregion

                #region update alerts

                if (((CommonGameResScn)ParentScene).m_hud.showImmuneAlert)
                {
                    int lastTime = ((CommonGameResScn)ParentScene).m_hud.immuneAlertCountDown;
                    ((CommonGameResScn)ParentScene).m_hud.immuneAlertCountDown = (int)(GlobalConstants.GP_IMMUNESYS_WARN_SECS -
                        (gameTime.TotalRealTime.TotalSeconds - m_immuneCountDownStartSecs));
                    if (((CommonGameResScn)ParentScene).m_hud.immuneAlertCountDown < 0)
                        ((CommonGameResScn)ParentScene).m_hud.immuneAlertCountDown = 0;
                    if (((CommonGameResScn)ParentScene).m_hud.immuneAlertCountDown != lastTime)
                        m_sndImmuneCountDown.Play();
                }
                if (((CommonGameResScn)ParentScene).m_hud.showMedicationAlert)
                {
                    int lastTime = ((CommonGameResScn)ParentScene).m_hud.medicationAlertCountDown;
                    ((CommonGameResScn)ParentScene).m_hud.medicationAlertCountDown = (int)(GlobalConstants.GP_MEDICATION_WARN_SECS -
                        (gameTime.TotalRealTime.TotalSeconds - m_medicCountDownStartSecs));
                    if (((CommonGameResScn)ParentScene).m_hud.medicationAlertCountDown < 0)
                        ((CommonGameResScn)ParentScene).m_hud.medicationAlertCountDown = 0;
                    if (((CommonGameResScn)ParentScene).m_hud.medicationAlertCountDown != lastTime)
                        m_sndMedicAlertCountDown.Play();
                }
                if (((CommonGameResScn)ParentScene).m_hud.ShowCapsidAlert)
                {
                    if (m_capsidLastTime != ((CommonGameResScn)ParentScene).m_hud.CapsidCountDown)
                    {
                        m_capsidLastTime = ((CommonGameResScn)ParentScene).m_hud.CapsidCountDown;
                        m_sndImmuneCountDown.Play();
                    }
                }

                #endregion

                //fix draw orders - uses container swap to avoid gc overheads
                UpdateCamera();

                float uniqueKey;

                #region resort draw depth
                m_drawableObjs.Clear();
                foreach (Common.Cells.UninfectedCell uCell in m_ucellObjs)
                {
                    uniqueKey = Microsoft.Xna.Framework.Vector3.Distance(Camera.Position, uCell.Position);
                    if (float.IsNaN(uniqueKey))
                        uniqueKey = 1f;
                    while (m_drawableObjs.ContainsKey(uniqueKey))
                    {
                        uniqueKey += (uniqueKey * 0.05f) + float.Epsilon;
                        if (float.IsNaN(uniqueKey))
                            uniqueKey = 1f;
                    }

                    m_drawableObjs.Add(uniqueKey, uCell);
                }
                foreach (CellCluster cluster in m_cellClusterObjs)
                {
                    uniqueKey = Microsoft.Xna.Framework.Vector3.Distance(Camera.Position, cluster.Position);
                    if (float.IsNaN(uniqueKey))
                        uniqueKey = 1f;
                    while (m_drawableObjs.ContainsKey(uniqueKey))
                    {
                        uniqueKey += (uniqueKey * 0.05f) + float.Epsilon;
                        if (float.IsNaN(uniqueKey))
                            uniqueKey = 1f;
                    }

                    m_drawableObjs.Add(uniqueKey, cluster);
                }

                //shove in virus
                uniqueKey = Microsoft.Xna.Framework.Vector3.Distance(Camera.Position, myVirus.Position);
                if (float.IsNaN(uniqueKey))
                    uniqueKey = 1f;
                while (m_drawableObjs.ContainsKey(uniqueKey))
                {
                    uniqueKey += (uniqueKey * 0.05f) + float.Epsilon;
                    if (float.IsNaN(uniqueKey))
                        uniqueKey = 1f;
                }

                m_drawableObjs.Add(uniqueKey, myVirus);
                #endregion

                //update hud info
                gamestate.UpdateState();
            }
        }

        #region AI

        private void ModerateAI(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_debugMgr.Assert(m_sessionDetails.isHost,
                "BiophageScn:MaderateAI - only host can call AI routines.");

            //handle cluster ai
            byte[] idArray = m_cellClusters.ToArray();
            for (int i = 0; i < idArray.Length; ++i)
            {
                if (m_cellClusters.Contains(idArray[i]))
                {
                    Common.CellCluster clust = (Common.CellCluster)m_gameObjects[ClusterGobjID(idArray[i])];
                    if (clust.stateData.numWhiteBloodCell == 0)
                    {
                        if (clust.stateData.virusRef.virusStateData.isBot)
                        {
                            if ((!DoClusterVirusAction(gameTime, clust)) &&
                                clust.m_clusterAI.RequestedAction != ClusterAIReqestAction.NONE)
                            {
                                DoClusterAction(gameTime, clust);
                            }

                            clust.m_clusterAI.FlipAIDataSets();
                            clust.m_clusterAI.ClearReqAIActions();
                        }
                    }
                }
            }

            //moderate IQ 
            foreach (Virus virus in m_virusObjs)
            {
                virus.AIModifyClusterIQ();
            }
        }

        //Returns true if virus request was granted - otherwise false
        private bool DoClusterVirusAction(Microsoft.Xna.Framework.GameTime gameTime, CellCluster cluster)
        {
            if (cluster.m_clusterAI.m_virusOverrideReqAction == VirusOverrideRequestAction.NONE)
                return false;

            bool retVal = false;
            if (cluster.stateData.actionState == CellActionState.IDLE)
            {
                switch (cluster.m_clusterAI.m_virusOverrideReqAction)
                {
                    case VirusOverrideRequestAction.VIR_CL_COMBINE:
                        //always check
                        if (cluster.m_clusterAI.AIDataSet.cl_re_clst_friend != null)
                        {
                            cluster.stateData.actionState = CellActionState.CHASING_CLUST_TO_COMBINE;
                            cluster.stateData.actionReObject = (NetworkEntity)cluster.m_clusterAI.AIDataSet.cl_re_clst_friend;
                            cluster.stateData.virusRef.aiVirus.aiSecSinceLastReq = 0.0;
                            retVal = true;
                        }
                        break;

                    case VirusOverrideRequestAction.VIR_CL_KAMIKAZE:
                        //always check
                        if (cluster.m_clusterAI.AIDataSet.cl_re_clst_enem != null)
                        {
                            cluster.stateData.actionReObject = (NetworkEntity)cluster.m_clusterAI.AIDataSet.cl_re_clst_enem;
                            HostClusterChase(cluster, cluster.stateData.actionReObject);

                            cluster.stateData.virusRef.aiVirus.aiSecSinceLastReq = 0.0;
                            retVal = true;
                        }
                        break;

                    case VirusOverrideRequestAction.VIR_CL_SPLIT:
                        retVal = AIClusterDetermSplit(cluster);
                        break;

                    default:
                        break;
                }
            }
            else if (cluster.m_clusterAI.m_virusOverrideReqAction == VirusOverrideRequestAction.VIR_CL_HYBRID)
            {
                retVal = AIClusterDetermHybrid(cluster);
            }

            return retVal;
        }

        private bool AIClusterDetermHybrid(CellCluster cluster)
        {
            if (cluster.CanHybreed)
            {
                Common.Cells.CellTypeEnum srcCellA = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.RED_BLOOD_CELL;
                Common.Cells.CellTypeEnum srcCellB = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.PLATELET;
                byte hybCount = Math.Min(cluster.stateData.numRBCs, cluster.stateData.numPlatelets);

                if (hybCount < Math.Min(cluster.stateData.numRBCs, cluster.stateData.numTanks))
                {
                    srcCellB = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_TANK;
                    hybCount = Math.Min(cluster.stateData.numRBCs, cluster.stateData.numTanks);
                }

                if (hybCount < Math.Min(cluster.stateData.numRBCs, cluster.stateData.numSilos))
                {
                    srcCellB = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_SILO;
                    hybCount = Math.Min(cluster.stateData.numRBCs, cluster.stateData.numSilos);
                }

                if (hybCount < Math.Min(cluster.stateData.numPlatelets, cluster.stateData.numTanks))
                {
                    srcCellA = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.PLATELET;
                    srcCellB = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_TANK;
                    hybCount = Math.Min(cluster.stateData.numPlatelets, cluster.stateData.numTanks);
                }

                if (hybCount < Math.Min(cluster.stateData.numPlatelets, cluster.stateData.numSilos))
                {
                    srcCellA = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.PLATELET;
                    srcCellB = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_SILO;
                    hybCount = Math.Min(cluster.stateData.numPlatelets, cluster.stateData.numSilos);
                }

                if (hybCount < Math.Min(cluster.stateData.numTanks, cluster.stateData.numSilos))
                {
                    srcCellA = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_TANK;
                    srcCellB = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.BIG_CELL_SILO;
                    hybCount = Math.Min(cluster.stateData.numTanks, cluster.stateData.numSilos);
                }

                if (hybCount > 0)
                {
                    HostClusterHybridCells(cluster, hybCount, srcCellA, srcCellB);
                    cluster.stateData.virusRef.aiVirus.aiSecSinceLastReq = 0.0;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This does a straight division of the cluster in half.
        /// </summary>
        private bool AIClusterDetermSplit(CellCluster cluster)
        {
            byte splRBC = (byte)(cluster.stateData.numRBCs / 2);
            byte splPLT = (byte)(cluster.stateData.numPlatelets / 2);
            byte splTNK = (byte)(cluster.stateData.numTanks / 2);
            byte splSIL = (byte)(cluster.stateData.numSilos / 2);

            byte splSHY = (byte)(cluster.stateData.numSmallHybrids / 2);
            byte splMHY = (byte)(cluster.stateData.numMediumHybrids / 2);
            byte splBHY = (byte)(cluster.stateData.numBigHybrids / 2);

            if ((splRBC + splPLT + splTNK + splSIL + splSHY + splMHY + splBHY) > 0)
            {
                HostSplitCluster(cluster,
                            splRBC, splPLT, splTNK, splSIL,
                            splSHY, splMHY, splBHY);

                cluster.stateData.virusRef.aiVirus.aiSecSinceLastReq = 0.0;
                return true;
            }
            else
                return false;
        }

        private void DoClusterAction(Microsoft.Xna.Framework.GameTime gameTime, CellCluster cluster)
        {
            if (cluster.stateData.actionState == CellActionState.IDLE)
            {
                switch (cluster.m_clusterAI.RequestedAction)
                {
                    case ClusterAIReqestAction.BATTLE_ATCK_ENEM:
                        //always check
                        if (cluster.stateData.attnAttackingEnemy != null)
                        {
                            cluster.stateData.actionReObject = (NetworkEntity)cluster.stateData.attnAttackingEnemy;
                            HostClusterChase(cluster, cluster.stateData.actionReObject);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.BATTLE_CLST_ENEM:
                        //always check
                        if (cluster.m_clusterAI.AIDataSet.cl_re_clst_enem != null)
                        {
                            cluster.stateData.actionReObject = (NetworkEntity)cluster.m_clusterAI.AIDataSet.cl_re_clst_enem;
                            HostClusterChase(cluster, cluster.stateData.actionReObject);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.CHASE_PLT:
                        //always check
                        if (cluster.m_clusterAI.AIDataSet.cl_re_clst_plt != null)
                        {
                            cluster.stateData.actionState = CellActionState.CHASING_UCELL_TOINFECT;
                            cluster.stateData.actionReObject = (NetworkEntity)cluster.m_clusterAI.AIDataSet.cl_re_clst_plt;
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.CHASE_RBC:
                        //always check
                        if (cluster.m_clusterAI.AIDataSet.cl_re_clst_rbc != null)
                        {
                            cluster.stateData.actionState = CellActionState.CHASING_UCELL_TOINFECT;
                            cluster.stateData.actionReObject = (NetworkEntity)cluster.m_clusterAI.AIDataSet.cl_re_clst_rbc;
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.CHASE_SIL:
                        //always check
                        if (cluster.m_clusterAI.AIDataSet.cl_re_clst_sil != null)
                        {
                            cluster.stateData.actionState = CellActionState.CHASING_UCELL_TOINFECT;
                            cluster.stateData.actionReObject = (NetworkEntity)cluster.m_clusterAI.AIDataSet.cl_re_clst_sil;
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.CHASE_TNK:
                        //always check
                        if (cluster.m_clusterAI.AIDataSet.cl_re_clst_tnk != null)
                        {
                            cluster.stateData.actionState = CellActionState.CHASING_UCELL_TOINFECT;
                            cluster.stateData.actionReObject = (NetworkEntity)cluster.m_clusterAI.AIDataSet.cl_re_clst_tnk;
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.DIVCELLS_ANY:
                        AIClusterDetermDivide(cluster);
                        break;

                    case ClusterAIReqestAction.DIVCELLS_PLT:
                        if ((cluster.stateData.numPlatelets > 0) && (cluster.m_clusterAI.AIDataSet.cl_plt_divcount >= 1.0))
                        {
                            HostClusterDivideCells(cluster,
                                0, 1, 0, 0);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.DIVCELLS_RBC:
                        if ((cluster.stateData.numRBCs > 0) && (cluster.m_clusterAI.AIDataSet.cl_rbc_divcount >= 1.0))
                        {
                            HostClusterDivideCells(cluster,
                                1, 0, 0, 0);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.DIVCELLS_SIL:
                        if ((cluster.stateData.numSilos > 0) && (cluster.m_clusterAI.AIDataSet.cl_sil_divcount >= 1.0))
                        {
                            HostClusterDivideCells(cluster,
                                0, 0, 0, 1);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.DIVCELLS_TNK:
                        if ((cluster.stateData.numTanks > 0) && (cluster.m_clusterAI.AIDataSet.cl_tnk_divcount >= 1.0))
                        {
                            HostClusterDivideCells(cluster,
                                0, 0, 1, 0);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.EVADE_ATCK_ENEM:
                        //always check
                        if (cluster.stateData.attnAttackingEnemy != null)
                        {
                            cluster.stateData.actionState = CellActionState.EVADING_ENEMY;
                            cluster.stateData.actionReObject = (NetworkEntity)cluster.stateData.attnAttackingEnemy;
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    default:
                        break;
                }
            }
            else
            {
                switch (cluster.m_clusterAI.RequestedAction)
                {
                    case ClusterAIReqestAction.DIVCELLS_ANY:
                        AIClusterDetermDivide(cluster);
                        break;

                    case ClusterAIReqestAction.DIVCELLS_PLT:
                        if ((cluster.stateData.numPlatelets > 0) && (cluster.m_clusterAI.AIDataSet.cl_plt_divcount >= 1.0))
                        {
                            HostClusterDivideCells(cluster,
                                0, 1, 0, 0);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.DIVCELLS_RBC:
                        if ((cluster.stateData.numRBCs > 0) && (cluster.m_clusterAI.AIDataSet.cl_rbc_divcount >= 1.0))
                        {
                            HostClusterDivideCells(cluster,
                                1, 0, 0, 0);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.DIVCELLS_SIL:
                        if ((cluster.stateData.numSilos > 0) && (cluster.m_clusterAI.AIDataSet.cl_sil_divcount >= 1.0))
                        {
                            HostClusterDivideCells(cluster,
                                0, 0, 0, 1);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    case ClusterAIReqestAction.DIVCELLS_TNK:
                        if ((cluster.stateData.numTanks > 0) && (cluster.m_clusterAI.AIDataSet.cl_tnk_divcount >= 1.0))
                        {
                            HostClusterDivideCells(cluster,
                                0, 0, 1, 0);
                            cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                        }
                        break;

                    default:
                        break;
                }
            }
        }

        private void AIClusterDetermDivide(CellCluster cluster)
        {
            //determine the maximum
            Common.Cells.CellTypeEnum srcCell = Common.Cells.CellTypeEnum.RED_BLOOD_CELL;
            byte cellCount = (byte)cluster.m_clusterAI.AIDataSet.cl_rbc_divcount;

            if (cellCount < (byte)cluster.m_clusterAI.AIDataSet.cl_plt_divcount)
            {
                srcCell = Common.Cells.CellTypeEnum.PLATELET;
                cellCount = (byte)cluster.m_clusterAI.AIDataSet.cl_plt_divcount;
            }

            if (cellCount < (byte)cluster.m_clusterAI.AIDataSet.cl_tnk_divcount)
            {
                srcCell = Common.Cells.CellTypeEnum.BIG_CELL_TANK;
                cellCount = (byte)cluster.m_clusterAI.AIDataSet.cl_tnk_divcount;
            }

            if (cellCount < (byte)cluster.m_clusterAI.AIDataSet.cl_sil_divcount)
            {
                srcCell = Common.Cells.CellTypeEnum.BIG_CELL_SILO;
                cellCount = (byte)cluster.m_clusterAI.AIDataSet.cl_sil_divcount;
            }

            //double check before proceeding
            switch (srcCell)
            {
                case Common.Cells.CellTypeEnum.RED_BLOOD_CELL:
                    if (cluster.m_clusterAI.AIDataSet.cl_rbc_divcount >= 1.0)
                    {
                        HostClusterDivideCells(cluster,
                                        1, 0, 0, 0);
                        cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                    }
                    break;
                case Common.Cells.CellTypeEnum.PLATELET:
                    if (cluster.m_clusterAI.AIDataSet.cl_plt_divcount >= 1.0)
                    {
                        HostClusterDivideCells(cluster,
                                        0, 1, 0, 0);
                        cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                    }
                    break;
                case Common.Cells.CellTypeEnum.BIG_CELL_TANK:
                    if (cluster.m_clusterAI.AIDataSet.cl_tnk_divcount >= 1.0)
                    {
                        HostClusterDivideCells(cluster,
                                        0, 0, 1, 0);
                        cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                    }
                    break;
                case Common.Cells.CellTypeEnum.BIG_CELL_SILO:
                    if (cluster.m_clusterAI.AIDataSet.cl_sil_divcount >= 1.0)
                    {
                        HostClusterDivideCells(cluster,
                                        0, 0, 0, 1);
                        cluster.m_clusterAI.m_aiSecSinceLastRequest = 0.0;
                    }
                    break;
            }
        }

        #endregion

        #region update routines

        private void UpdateCamera()
        {
            Common.FollowCamera cam = (Common.FollowCamera)m_camera;

            //check if virus exists and is not dead - otherwise observer with cursor
            if ((myVirus != null) && (myVirus.Active))
            {
                cam.LookingAt = myVirus.Position;
                cam.ForwardDir = myVirus.ForwardDir;
                cam.UpDir = myVirus.UpDir;

                cam.FollowDistance = myVirus.CamDistance;
            }
            else
            {
                if (!m_cursor.Visible)
                {
                    m_cursor.Visible = true;
                    m_cursor.Position = Microsoft.Xna.Framework.Vector3.Zero;
                    m_cursor.PhysBody.EnableBody();
                }

                cam.LookingAt = m_cursor.Position;
                cam.ForwardDir = m_cursor.ForwardDir;
                cam.UpDir = m_cursor.UpDir;

                cam.FollowDistance = 10f;
            }

            if (float.IsNaN(Camera.Position.X) || float.IsNaN(Camera.Position.Y) || float.IsNaN(Camera.Position.Z))
                Camera.Position = Microsoft.Xna.Framework.Vector3.Zero;

            if (float.IsNaN(Camera.LookingAt.X) || float.IsNaN(Camera.LookingAt.Y) || float.IsNaN(Camera.LookingAt.Z))
                Camera.LookingAt = Microsoft.Xna.Framework.Vector3.Zero;

            if (float.IsNaN(m_cursor.Position.X) || float.IsNaN(m_cursor.Position.Y) || float.IsNaN(m_cursor.Position.Z))
                m_cursor.Position = Microsoft.Xna.Framework.Vector3.Zero;
        }

        protected virtual void HostCheckGameConditions(Microsoft.Xna.Framework.GameTime gameTime)
        {
            #region check game over conditions

            //set times
            if (m_timeGameStarted.Equals(TimeSpan.Zero))
                m_timeGameStarted = gameTime.TotalRealTime;

            if (m_timeSinceLastMed.Equals(TimeSpan.Zero))
                m_timeSinceLastMed = gameTime.TotalRealTime;

            if (m_timeSinceLastImmuneSysWave.Equals(TimeSpan.Zero))
                m_timeSinceLastImmuneSysWave = gameTime.TotalRealTime;

            //check if all viruses died
            if (m_finishedRanksScrn.finishVirusRanks.Count == m_viruses.Count)
                HostSubmitGameOver();
            else if (m_finishedRanksScrn.finishVirusRanks.Count == (m_viruses.Count - 1))
            {
                //add last virus and end - this also implies LAST STANDING
                foreach (Virus virus in m_virusObjs)
                {
                    if (!virus.hasBeenRanked)
                    {
                        //add virus to the rank stack
                        m_finishedRanksScrn.finishVirusRanks.Push(GobjIDVirus(virus.Id));
                        virus.hasBeenRanked = true;
                        break;
                    }
                }
                HostSubmitGameOver();
            }

            //check if single player game finished
            if ((!m_sessionDetails.isMultiplayer) && (!myVirus.Active))
                HostSubmitGameOver();

            //check if multiplayer has finished
            if (m_sessionDetails.isMultiplayer)
            {
                int numDeadPlayers = 0;
                foreach (Virus virus in m_virusObjs)
                {
                    if ((!virus.virusStateData.isBot) && (!virus.Active))
                        numDeadPlayers++;
                }
                if (numDeadPlayers == m_sessionDetails.netSessionComponent.GetNetworkSession.AllGamers.Count)
                    HostSubmitGameOver();
            }

            //check if time is up if timed game session
            if (m_sessionDetails.type == GlobalConstants.GameplayType.TIMED_MATCH)
            {
                if ((gameTime.TotalRealTime - m_timeGameStarted).TotalMinutes >= (double)m_sessionDetails.typeSettings)
                    HostSubmitGameOver();
            }
            else if (m_sessionDetails.type == GlobalConstants.GameplayType.ILLNESS)
            {
                //go through each virus and end if a virus with highest infection is greater/eq than condition
                foreach (Virus virus in m_virusObjs)
                {
                    if (virus.virusStateData.infectPercentage >= (double)m_sessionDetails.typeSettings)
                    {
                        HostSubmitGameOver();
                        break;
                    }
                }
            }
            #endregion

            #region do immune system and medications
            if (!m_finishedRanksScrn.isGameOver)
            {
                HostImmuneSystem(gameTime);
                HostMedications(gameTime);
            }
            #endregion
        }

        /// <summary>
        /// Note that hybrids are 'immune' to medication, but they cannot divide.
        /// </summary>
        /// <param name="gameTime"></param>
        private void HostMedications(Microsoft.Xna.Framework.GameTime gameTime)
        {
            double minsElapsed = (gameTime.TotalRealTime - m_timeSinceLastMed).TotalMinutes;
            Common.Cells.CellTypeEnum cellTypeToKill;

            if (!((CommonGameResScn)ParentScene).m_hud.showMedicationAlert)
            {
                if (minsElapsed >=
                    (GlobalConstants.GP_MEDICATION_TIMEOUT_MINS - (GlobalConstants.GP_MEDICATION_WARN_SECS / 60.0)))
                    HostSubmitMedicCountDown(gameTime);
            }

            if (minsElapsed >= GlobalConstants.GP_MEDICATION_TIMEOUT_MINS)
            {
                m_timeSinceLastMed = gameTime.TotalRealTime;

                //deploy medication - kill greatest infected cell type
                int rbcInfectTotal, pltInfectTotal, tnkInfectTotal, silInfectTotal;
                rbcInfectTotal = pltInfectTotal = tnkInfectTotal = silInfectTotal = 0;

                foreach (CellCluster lCluster in m_cellClusterObjs)
                {
                    rbcInfectTotal += lCluster.stateData.numRBCs;
                    pltInfectTotal += lCluster.stateData.numPlatelets;
                    tnkInfectTotal += lCluster.stateData.numTanks;
                    silInfectTotal += lCluster.stateData.numSilos;
                }

                //determine greatest type
                int greatestNum = Math.Max(rbcInfectTotal, pltInfectTotal);
                greatestNum = Math.Max(greatestNum, tnkInfectTotal);
                greatestNum = Math.Max(greatestNum, silInfectTotal);
                if (greatestNum == rbcInfectTotal)
                    cellTypeToKill = Common.Cells.CellTypeEnum.RED_BLOOD_CELL;
                else if (greatestNum == pltInfectTotal)
                    cellTypeToKill = Common.Cells.CellTypeEnum.PLATELET;
                else if (greatestNum == tnkInfectTotal)
                    cellTypeToKill = Common.Cells.CellTypeEnum.BIG_CELL_TANK;
                else if (greatestNum == silInfectTotal)
                    cellTypeToKill = Common.Cells.CellTypeEnum.BIG_CELL_SILO;
                else
                {
                    HostSubmitGameOver();
                    return; //possible if all dead - make game end
                }

                //kill cells in clusters
                HostSubmitMedication(cellTypeToKill);
            }
        }

        private void HostImmuneSystem(Microsoft.Xna.Framework.GameTime gameTime)
        {
            double minsElapsed = (gameTime.TotalRealTime - m_timeSinceLastImmuneSysWave).TotalMinutes;

            if (!((CommonGameResScn)ParentScene).m_hud.showImmuneAlert)
            {
                if (minsElapsed >=
                    (GlobalConstants.GP_IMMUNESYS_TIMEOUT_MINS - (GlobalConstants.GP_IMMUNESYS_WARN_SECS / 60.0)))
                    HostSubmitImmuneCountDown(gameTime);
            }

            Common.CellCluster wbcCluster;
            if (minsElapsed >= GlobalConstants.GP_IMMUNESYS_TIMEOUT_MINS)
            {
                //alert sound and up hud
                ((CommonGameResScn)ParentScene).m_hud.showImmuneAlert = false;
                ((CommonGameResScn)ParentScene).m_hud.immuneAlertCountDown = 0;
                m_sndGenAlert.Play();

                m_timeSinceLastImmuneSysWave = gameTime.TotalRealTime;
                m_immuneSysWaveCount++;
                if ((m_immuneSysWaveCount + m_whiteBloodCells.Count) > GlobalConstants.GP_WHITEBLOODCELL_MAX_NUM)
                    m_immuneSysWaveCount = GlobalConstants.GP_WHITEBLOODCELL_MAX_NUM - m_whiteBloodCells.Count;

                //dispatch white blood cells in waves
                int wbcPosSpawnIndex = 0;
                for (int i = 0; i < m_immuneSysWaveCount; ++i)
                {
                    wbcPosSpawnIndex = i % m_whiteBloodCellSpawnLocations.Length;
                    wbcCluster = HostNewWhiteBloodCell(m_whiteBloodCellSpawnLocations[wbcPosSpawnIndex]);
                    if (m_sessionDetails.isMultiplayer)
                        HostSubmitNewCluster(wbcCluster);
                }
            }
        }

        #endregion

        public override void Draw(Microsoft.Xna.Framework.GameTime gameTime,
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            graphicsDevice.Clear(m_clearColour);

            //draw level
            m_gameObjects[m_levelEnvId].DoDraw(gameTime, graphicsDevice, Camera);

            //draw the sorted gobjs
            foreach (KeyValuePair<float, IGameObject> gobjKVP in m_drawableObjs)
            {
                gobjKVP.Value.DoDraw(gameTime, graphicsDevice, Camera);
            }

            //draw cursor
            m_cursor.DoDraw(gameTime, graphicsDevice, Camera);

            //do the bloom effect
            DrawBloomEffect(gameTime, graphicsDevice);

            //draw the HUD
            if (!m_finishedRanksScrn.isGameOver)
                ((CommonGameResScn)ParentScene).m_hud.Draw(gameTime, graphicsDevice, m_camera);

            //draw cluster menu - if avaliable
            if (myVirus.SelectedCluster != null)
                if (((CommonGameResScn)ParentScene).m_hud.m_clusterMenu.Active)
                {
                    //make sure cluster is not dead
                    if (((CommonGameResScn)ParentScene).m_hud.m_clusterMenu.Cluster.Active)
                        ((CommonGameResScn)ParentScene).m_hud.m_clusterMenu.DoDraw(gameTime, graphicsDevice, m_camera);
                    else
                    {
                        ((CommonGameResScn)ParentScene).m_hud.m_clusterMenu.Active = false;
                    }
                }

            //draw the finished ranks screen - if game is over
            if (m_finishedRanksScrn.isGameOver)
                m_finishedRanksScrn.Draw(gameTime, graphicsDevice, Camera);

            //darw the 'please wait screen is game hasn't started
            if (!m_gameStarted)
                DrawGameNotStartedScreen(gameTime, graphicsDevice);

            //draw the menu
            if (m_menu.Active)
                m_menu.DoDraw(gameTime, graphicsDevice, m_camera);
        }

        private void DrawGameNotStartedScreen(  Microsoft.Xna.Framework.GameTime gameTime,
                                                Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            //draw overlay
            m_fadeOverlay.DoDraw(gameTime, graphicsDevice, m_tutOverlayCam);

            //draw writings
            m_spriteBatch.Begin();

            m_spriteBatch.DrawString((Microsoft.Xna.Framework.Graphics.SpriteFont)m_waitFont.GetResource,
                m_waitMsg, FontPos, Microsoft.Xna.Framework.Graphics.Color.White,
                        0f, FontOrigin, 1f, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0.5f);

            m_spriteBatch.End();
        }

        #region bloom effect

        /// <summary>
        /// This method is based on the XNA 'Bloom' tutorial.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="graphicsDevice"></param>
        protected void DrawBloomEffect(Microsoft.Xna.Framework.GameTime gameTime,
                                        Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice)
        {
            //make sure local references to objects
            Microsoft.Xna.Framework.Graphics.Effect bloomExtractEffect =
                (Microsoft.Xna.Framework.Graphics.Effect)m_bloomExtractEffect.GetResource;
            Microsoft.Xna.Framework.Graphics.Effect bloomCombineEffect =
                (Microsoft.Xna.Framework.Graphics.Effect)m_bloomCombineEffect.GetResource;
            Microsoft.Xna.Framework.Graphics.Effect gaussianBlurEffect =
                (Microsoft.Xna.Framework.Graphics.Effect)m_gaussianBlurEffect.GetResource;

            // Resolve the scene into a texture, so we can
            // use it as input data for the bloom processing.
            graphicsDevice.ResolveBackBuffer(m_resolveTarget);

            // Pass 1: draw the scene into rendertarget 1, using a
            // shader that extracts only the brightest parts of the image.

            bloomExtractEffect.Parameters["BloomThreshold"].SetValue(
                GlobalConstants.EFFECT_BLOOM_THRESHOLD);

            DrawFullscreenQuad(m_resolveTarget, m_renderTarget1,
                               bloomExtractEffect,
                               IntermediateBuffer.PreBloom);

            // Pass 2: draw from rendertarget 1 into rendertarget 2,
            // using a shader to apply a horizontal gaussian blur filter.
            SetBlurEffectParameters(1.0f / (float)m_renderTarget1.Width, 0);

            DrawFullscreenQuad(m_renderTarget1.GetTexture(), m_renderTarget2,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredHorizontally);

            // Pass 3: draw from rendertarget 2 back into rendertarget 1,
            // using a shader to apply a vertical gaussian blur filter.
            SetBlurEffectParameters(0, 1.0f / (float)m_renderTarget1.Height);

            DrawFullscreenQuad(m_renderTarget2.GetTexture(), m_renderTarget1,
                               gaussianBlurEffect,
                               IntermediateBuffer.BlurredBothWays);

            // Pass 4: draw both rendertarget 1 and the original scene
            // image back into the main backbuffer, using a shader that
            // combines them to produce the final bloomed result.
            graphicsDevice.SetRenderTarget(0, null);

            Microsoft.Xna.Framework.Graphics.EffectParameterCollection parameters = bloomCombineEffect.Parameters;

            parameters["BloomIntensity"].SetValue(GlobalConstants.EFFECT_BLOOM_INTENSITY);
            parameters["BaseIntensity"].SetValue(GlobalConstants.EFFECT_BASE_INTENSITY);
            parameters["BloomSaturation"].SetValue(GlobalConstants.EFFECT_BLOOM_SATURATION);
            parameters["BaseSaturation"].SetValue(GlobalConstants.EFFECT_BASE_SATURATION);

            graphicsDevice.Textures[1] = m_resolveTarget;

            Microsoft.Xna.Framework.Graphics.Viewport viewport = graphicsDevice.Viewport;

            DrawFullscreenQuad(m_renderTarget1.GetTexture(),
                               viewport.Width, viewport.Height,
                               bloomCombineEffect,
                               IntermediateBuffer.FinalResult);
        }

        /// <summary>
        /// Helper for drawing a texture into a rendertarget, using
        /// a custom shader to apply postprocessing effects.
        /// </summary>
        /// <remarks>
        /// Copied from XNA 'Bloom' tutorial.
        /// </remarks>
        protected void DrawFullscreenQuad(Microsoft.Xna.Framework.Graphics.Texture2D texture,
                                    Microsoft.Xna.Framework.Graphics.RenderTarget2D renderTarget,
                                    Microsoft.Xna.Framework.Graphics.Effect effect,
                                    IntermediateBuffer currentBuffer)
        {
            m_graphicsMgr.GraphicsDevice.SetRenderTarget(0, renderTarget);

            DrawFullscreenQuad(texture,
                               renderTarget.Width, renderTarget.Height,
                               effect, currentBuffer);

            m_graphicsMgr.GraphicsDevice.SetRenderTarget(0, null);
        }

        /// <summary>
        /// Helper for drawing a texture into the current rendertarget,
        /// using a custom shader to apply postprocessing effects.
        /// </summary>
        /// <remarks>
        /// Copied from XNA 'Bloom' tutorial.
        /// </remarks>
        protected void DrawFullscreenQuad(Microsoft.Xna.Framework.Graphics.Texture2D texture,
                                    int width, int height,
                                    Microsoft.Xna.Framework.Graphics.Effect effect,
                                    IntermediateBuffer currentBuffer)
        {
            m_spriteBatch.Begin(Microsoft.Xna.Framework.Graphics.SpriteBlendMode.None,
                              Microsoft.Xna.Framework.Graphics.SpriteSortMode.Immediate,
                              Microsoft.Xna.Framework.Graphics.SaveStateMode.None);

            // Begin the custom effect, if it is currently enabled. If the user
            // has selected one of the show intermediate buffer options, we still
            // draw the quad to make sure the image will end up on the screen,
            // but might need to skip applying the custom pixel shader.
            if (showBuffer >= currentBuffer)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }

            // Draw the quad.
            m_spriteBatch.Draw(texture,
                new Microsoft.Xna.Framework.Rectangle(0, 0, width, height),
                Microsoft.Xna.Framework.Graphics.Color.White);

            m_spriteBatch.End();

            // End the custom effect.
            if (showBuffer >= currentBuffer)
            {
                effect.CurrentTechnique.Passes[0].End();
                effect.End();
            }
        }

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        /// <remarks>
        /// Copied from XNA 'Bloom' tutorial.
        /// </remarks>
        protected void SetBlurEffectParameters(float dx, float dy)
        {
            Microsoft.Xna.Framework.Graphics.Effect gaussianBlurEffect =
                (Microsoft.Xna.Framework.Graphics.Effect)m_gaussianBlurEffect.GetResource;

            // Look up the sample weight and offset effect parameters.
            Microsoft.Xna.Framework.Graphics.EffectParameter weightsParameter, offsetsParameter;

            weightsParameter = gaussianBlurEffect.Parameters["SampleWeights"];
            offsetsParameter = gaussianBlurEffect.Parameters["SampleOffsets"];

            // Look up how many samples our gaussian blur effect supports.
            int sampleCount = weightsParameter.Elements.Count;

            // Create temporary arrays for computing our filter settings.
            float[] sampleWeights = new float[sampleCount];
            Microsoft.Xna.Framework.Vector2[] sampleOffsets = new Microsoft.Xna.Framework.Vector2[sampleCount];

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Microsoft.Xna.Framework.Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Microsoft.Xna.Framework.Vector2 delta = new Microsoft.Xna.Framework.Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter settings.
            weightsParameter.SetValue(sampleWeights);
            offsetsParameter.SetValue(sampleOffsets);
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        /// <remarks>
        /// Copied from XNA 'Bloom' tutorial.
        /// </remarks>
        protected float ComputeGaussian(float n)
        {
            float theta = GlobalConstants.EFFECT_BLUR_AMMOUNT;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) *
                           Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        #endregion

        #endregion

        #region statics

        public static Microsoft.Xna.Framework.Graphics.Color GamerColour(byte gamerId)
        {
            Microsoft.Xna.Framework.Graphics.Color virColour = Microsoft.Xna.Framework.Graphics.Color.Black;

            Random rand = new Random((int)gamerId);

            virColour.R += (byte)rand.Next(50, 200);
            virColour.G += (byte)rand.Next(100, 255);
            virColour.B += (byte)rand.Next(50, 100);

            return virColour;
        }

        public static uint UCellGobjID(byte uCellId)
        {
            return ((uint)uCellId + m_ucellIdOffset);
        }

        public static byte GobjIDUCell(uint uCellId)
        {
            return (byte)(uCellId - m_ucellIdOffset);
        }

        public static uint VirusGobjID(byte virId)
        {
            return ((uint)virId + m_virusIdOffset);
        }

        public static byte GobjIDVirus(uint virusId)
        {
            return (byte)(virusId - m_virusIdOffset);
        }

        public static uint ClusterGobjID(byte clusterId)
        {
            return ((uint)clusterId + m_clustIdOffset);
        }

        public static byte GobjIDCluster(uint clusterId)
        {
            return (byte)(clusterId - m_clustIdOffset);
        }

        #endregion

        #region networking

        #region host

        public void HostNetworkUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_debugMgr.Assert(m_sessionDetails.isHost,
                "BiophageScn:HostGeneralUpdate - only host can call this method.");

            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            #region recieve from clients

            Microsoft.Xna.Framework.Net.NetworkGamer sender;
            byte clustId;
            while (m_sessionDetails.gamerMe.IsDataAvailable)
            {
                m_sessionDetails.gamerMe.ReceiveData(netComp.packetReader, out sender);
                GlobalConstants.NETPACKET_IDS packetId = (GlobalConstants.NETPACKET_IDS)netComp.packetReader.ReadByte();

                if (sender.Id != m_sessionDetails.gamerMe.Id)
                {
                    switch (packetId)
                    {
                        case GlobalConstants.NETPACKET_IDS.NETCLIENT_NEW_CLUSTER_UCELL:
                            byte ucellId = netComp.packetReader.ReadByte();
                            if (m_uninfectedCells.Contains(ucellId))
                            {
                                foreach (Virus vir in m_virusObjs)
                                {
                                    if ((!vir.virusStateData.isBot) && (vir.virusStateData.netPlayerId == sender.Id))
                                    {
                                        HostCreateNewClusterFromUCell(ucellId, GobjIDVirus(vir.Id));
                                        break;
                                    }
                                }
                            }
                            break;

                        case GlobalConstants.NETPACKET_IDS.NETCLIENT_DIV_CLUST_CELLS:
                            clustId = netComp.packetReader.ReadByte();
                            byte addRBC = netComp.packetReader.ReadByte();
                            byte addPLT = netComp.packetReader.ReadByte();
                            byte addTNK = netComp.packetReader.ReadByte();
                            byte addSIL = netComp.packetReader.ReadByte();
                            if (m_cellClusters.Contains(clustId))
                            {
                                HostClusterDivideCells((Common.CellCluster)m_gameObjects[ClusterGobjID(clustId)],
                                    addRBC, addPLT, addTNK, addSIL);
                            }
                            break;

                        case GlobalConstants.NETPACKET_IDS.NETCLIENT_HYB_CLUST_CELLS:
                            clustId = netComp.packetReader.ReadByte();
                            byte hybCount = netComp.packetReader.ReadByte();
                            Common.Cells.CellTypeEnum srcCellA = (Common.Cells.CellTypeEnum)netComp.packetReader.ReadByte();
                            Common.Cells.CellTypeEnum srcCellB = (Common.Cells.CellTypeEnum)netComp.packetReader.ReadByte();
                            if (m_cellClusters.Contains(clustId))
                            {
                                HostClusterHybridCells((CellCluster)m_gameObjects[ClusterGobjID(clustId)],
                                    hybCount, srcCellA, srcCellB);
                            }
                            break;

                        case GlobalConstants.NETPACKET_IDS.NETCLIENT_SPLIT_CLUSTER:
                            clustId = netComp.packetReader.ReadByte();
                            byte splRBC = netComp.packetReader.ReadByte();
                            byte splPLT = netComp.packetReader.ReadByte();
                            byte splTNK = netComp.packetReader.ReadByte();
                            byte splSIL = netComp.packetReader.ReadByte();
                            byte splSHY = netComp.packetReader.ReadByte();
                            byte splMHY = netComp.packetReader.ReadByte();
                            byte splBHY = netComp.packetReader.ReadByte();
                            if (m_cellClusters.Contains(clustId))
                            {
                                HostSplitCluster((CellCluster)m_gameObjects[ClusterGobjID(clustId)],
                                    splRBC, splPLT, splTNK, splSIL, splSHY, splMHY, splBHY);
                            }
                            break;

                        case GlobalConstants.NETPACKET_IDS.NETCLIENT_CLUSTER_CHASE:
                            clustId = netComp.packetReader.ReadByte();
                            uint targetId = (uint)netComp.packetReader.ReadUInt16();
                            if (m_cellClusters.Contains(clustId))
                            {
                                HostClusterChase((CellCluster)m_gameObjects[ClusterGobjID(clustId)],
                                    (NetworkEntity)m_gameObjects[targetId]);
                            }
                            break;

                        case GlobalConstants.NETPACKET_IDS.NETCLIENT_CLUSTER_EVADE:
                            clustId = netComp.packetReader.ReadByte();
                            byte avoidId = netComp.packetReader.ReadByte();
                            if (m_cellClusters.Contains(clustId))
                            {
                                HostClusterEvade((CellCluster)m_gameObjects[ClusterGobjID(clustId)],
                                    (CellCluster)m_gameObjects[ClusterGobjID(avoidId)]);
                            }
                            break;

                        case GlobalConstants.NETPACKET_IDS.NETCLIENT_CLUSTER_CANCEL_ACTION:
                            clustId = netComp.packetReader.ReadByte();
                            if (m_cellClusters.Contains(clustId))
                            {
                                CellCluster cluster = (CellCluster)m_gameObjects[ClusterGobjID(clustId)];

                                if ((cluster.stateData.actionState == CellActionState.CHASING_ENEMY_TO_BATTLE) &&
                                    (cluster.stateData.actionReObject != null))
                                {
                                    HostUNWarnBattle((CellCluster)cluster.stateData.actionReObject);
                                }

                                cluster.stateData.actionState = CellActionState.IDLE;
                                cluster.stateData.actionReObject = null;
                            }
                            break;

                        case GlobalConstants.NETPACKET_IDS.NETCLIENT_ISREADY:
                            m_numNotReadyPlayers--;
                            if (m_numNotReadyPlayers == 0)
                            {
                                m_gameStarted = true;
                                IsPaused = false;
                                netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_GAME_ISREADY);
                                
                                //send packet
                                m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                                    Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);
                            }
                            break;

                        //ignores
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_UNINFECTED_CELLS_TRANS:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_CELL_CLUSTERS_TRANS:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_NEW_CLUSTER:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_DIV_CLUST_CELLS:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_HYB_CLUST_CELLS:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_CLUSTER_UPDATE:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_MEDICATION_DEPLOY:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_GAME_OVER:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_IMMUNE_COUNTDOWN:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_MED_COUNTDOWN:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_BATTLE_WARNING:
                        case GlobalConstants.NETPACKET_IDS.NETSERVER_BATTLE_UNWARNING:
                            break;
                        default:
                            m_debugMgr.WriteLogEntry("BiophageScn:ClientUpdate - hmm..I don't know this packet. ID=" + packetId);
                            break;
                    }
                }
            }

            #endregion

            #region submit

            #region uninfected cells

            if ((gameTime.TotalRealTime.TotalMilliseconds - m_lastUCupdateSent.TotalMilliseconds) >=
                (double)GlobalConstants.NETWAIT_UCELLS_TRANS_MSECS)
            {
                Microsoft.Xna.Framework.Vector3 normdPos;
                float posLen = 0f;

                Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4 netPos;
                Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4 netOrient;

                netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_UNINFECTED_CELLS_TRANS);   //Packet ID
                netComp.packetWriter.Write((float)gameTime.TotalRealTime.TotalSeconds);         //TimeStamp (seconds fraction)
                foreach (Common.Cells.UninfectedCell uCell in m_ucellObjs)
                {
                    normdPos = uCell.Position;
                    posLen = normdPos.Length();
                    if (posLen != 0f)
                        normdPos.Normalize();
                    if (uCell.Orientation.Length() != 0f)
                        uCell.Orientation.Normalize();

                    netPos = new Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4(
                        normdPos.X, normdPos.Y, normdPos.Z, posLen);
                    netOrient = new Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4(
                        uCell.Orientation.X, uCell.Orientation.Y, uCell.Orientation.Z, uCell.Orientation.W);

                    netComp.packetWriter.Write(GobjIDUCell(uCell.Id));            //cell Id - byte inset
                    netComp.packetWriter.Write(netPos.PackedValue);     //cell position
                    netComp.packetWriter.Write(netOrient.PackedValue);  //cell orientation
                }
                m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                    Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);

                //update time
                m_lastUCupdateSent = gameTime.TotalRealTime;
                if (m_lastClupdateSent.TotalSeconds == 0.0)
                {
                    m_lastClupdateSent = gameTime.TotalRealTime;
                    TimeSpan ts = new TimeSpan(0, 0, 0, 0, 125);
                    m_lastClupdateSent += ts; //this allows the cluster to be updated out of sync to u cells
                }
            }

            #endregion

            #region clusters

            if ((gameTime.TotalRealTime.TotalMilliseconds - m_lastClupdateSent.TotalMilliseconds) >=
                (double)GlobalConstants.NETWAIT_CLUST_TRANS_MSECS)
            {
                Microsoft.Xna.Framework.Vector3 normdPos;
                float posLen = 0f;

                Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4 netPos;

                netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_CELL_CLUSTERS_TRANS);  //Packet ID
                netComp.packetWriter.Write((float)gameTime.TotalRealTime.TotalSeconds);     //TimeStamp (seconds fraction)
                foreach (CellCluster lCluster in m_cellClusterObjs)
                {
                    normdPos = lCluster.Position;
                    posLen = normdPos.Length();
                    if (posLen != 0f)
                        normdPos.Normalize();

                    netPos = new Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4(
                        normdPos.X, normdPos.Y, normdPos.Z, posLen);

                    netComp.packetWriter.Write(GobjIDCluster(lCluster.Id));            //cell Id - byte inset
                    netComp.packetWriter.Write(netPos.PackedValue);   //cell position
                }
                m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                    Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);

                //update time
                m_lastClupdateSent = gameTime.TotalRealTime;
            }

            #endregion

            #endregion
        }

        #region submit

        public virtual void HostCreateNewClusterFromUCell(byte ucellId, byte virusId)
        {
            m_debugMgr.Assert(m_sessionDetails.isHost,
                "BiophageScn:HostCreateNewClusterFromUCell - only host can call this method.");

            //get the ucell
            m_debugMgr.Assert(m_gameObjects[UCellGobjID(ucellId)] != null,
                "BiophageScn:HostCreateNewClusterFromUCell - ucell doesn't exist.");
            m_debugMgr.Assert(m_gameObjects[UCellGobjID(ucellId)] is Common.Cells.UninfectedCell,
                "BiophageScn:HostCreateNewClusterFromUCell - gobj is not a ucell type.");

            m_debugMgr.Assert(m_gameObjects[VirusGobjID(virusId)] != null,
                "BiophageScn:HostCreateNewClusterFromUCell - virus doesn't exist.");
            m_debugMgr.Assert(m_gameObjects[VirusGobjID(virusId)] is Common.Virus,
                "BiophageScn:HostCreateNewClusterFromUCell - gobj is not a virus type.");

            Common.Cells.UninfectedCell ucell = (Common.Cells.UninfectedCell)m_gameObjects[UCellGobjID(ucellId)];
            Common.Virus virus = (Common.Virus)m_gameObjects[VirusGobjID(virusId)];

            ClusterStateData clData = UCellToClusterData(ucell, virusId);
            Common.CellCluster cluster = HostClusterNew(clData, ucell.Position);
            virus.virusStateData.clusters.AddLast(cluster);

            //remove the ucell
            ucell.Active = false;

            //send packet - client will automatically remove ucell from ucell updates packet
            if (m_sessionDetails.isMultiplayer)
                HostSubmitNewCluster(cluster);
        }

        public virtual void HostClusterInfectUCell(byte ucellId, byte clusterId)
        {
            m_debugMgr.Assert(m_sessionDetails.isHost,
                "BiophageScn:HostClusterInfectUCell - only host can call this method.");

            //get the ucell
            m_debugMgr.Assert(m_gameObjects[UCellGobjID(ucellId)] != null,
                "BiophageScn:HostClusterInfectUCell - ucell doesn't exist.");
            m_debugMgr.Assert(m_gameObjects[UCellGobjID(ucellId)] is Common.Cells.UninfectedCell,
                "BiophageScn:HostClusterInfectUCell - gobj is not a ucell type.");

            m_debugMgr.Assert(m_gameObjects[ClusterGobjID(clusterId)] != null,
                "BiophageScn:HostClusterInfectUCell - cluster doesn't exist.");
            m_debugMgr.Assert(m_gameObjects[ClusterGobjID(clusterId)] is Common.CellCluster,
                "BiophageScn:HostClusterInfectUCell - gobj is not a cluster type.");

            Common.Cells.UninfectedCell ucell = (Common.Cells.UninfectedCell)m_gameObjects[UCellGobjID(ucellId)];
            Common.CellCluster cluster = (Common.CellCluster)m_gameObjects[ClusterGobjID(clusterId)];

            cluster.InfectUCell(ucell);

            //remove the ucell
            ucell.Active = false;

            cluster.stateData.actionState = CellActionState.IDLE;
            cluster.stateData.actionReObject = null;

            //send packet - client will automatically remove ucell from ucell updates packet
            if (m_sessionDetails.isMultiplayer)
                HostSubmitClusterUpdate(cluster);
        }

        void HostSubmitNewCluster(Common.CellCluster cluster)
        {
            //broadcast that a new cluster has been created.
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_NEW_CLUSTER);
            netComp.packetWriter.Write(GobjIDCluster(cluster.Id));
            netComp.packetWriter.Write(cluster.stateData.virusOwnerId);

            Microsoft.Xna.Framework.Vector3 normdPos = cluster.Position;
            float lenPos = cluster.Position.Length();
            if (lenPos != 0)
                normdPos.Normalize();

            HalfVector4 hvPos = new HalfVector4(
                normdPos.X, normdPos.Y, normdPos.Z,
                lenPos);
            netComp.packetWriter.Write(hvPos.PackedValue);

            netComp.packetWriter.Write(cluster.stateData.numWhiteBloodCell);

            netComp.packetWriter.Write(cluster.stateData.attrHealth);
            netComp.packetWriter.Write(cluster.stateData.attrNutrientStore);

            netComp.packetWriter.Write(cluster.stateData.numRBCs);
            netComp.packetWriter.Write(cluster.stateData.numPlatelets);
            netComp.packetWriter.Write(cluster.stateData.numSilos);
            netComp.packetWriter.Write(cluster.stateData.numTanks);
            netComp.packetWriter.Write(cluster.stateData.numSmallHybrids);
            netComp.packetWriter.Write(cluster.stateData.numMediumHybrids);
            netComp.packetWriter.Write(cluster.stateData.numBigHybrids);

            //send packet
            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);
        }

        public void HostSubmitClusterUpdate(Common.CellCluster cluster)
        {
            //broadcast that a new cluster has been created.
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_CLUSTER_UPDATE);
            netComp.packetWriter.Write(GobjIDCluster(cluster.Id));

            netComp.packetWriter.Write((byte)cluster.stateData.actionState);

            netComp.packetWriter.Write(cluster.stateData.attrHealth);
            netComp.packetWriter.Write(cluster.stateData.attrNutrientStore);

            netComp.packetWriter.Write(cluster.stateData.numRBCs);
            netComp.packetWriter.Write(cluster.stateData.numPlatelets);
            netComp.packetWriter.Write(cluster.stateData.numSilos);
            netComp.packetWriter.Write(cluster.stateData.numTanks);
            netComp.packetWriter.Write(cluster.stateData.numSmallHybrids);
            netComp.packetWriter.Write(cluster.stateData.numMediumHybrids);
            netComp.packetWriter.Write(cluster.stateData.numBigHybrids);

            //send packet
            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);
        }

        public virtual void HostClusterDivideCells(Common.CellCluster cluster,
            byte addRBC, byte addPLT, byte addTNK, byte addSIL)
        {
            //update local copy
            cluster.DivideCells(addRBC, addPLT, addTNK, addSIL);

            //send packet to everyone
            if (m_sessionDetails.isMultiplayer)
            {
                LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

                netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_DIV_CLUST_CELLS);
                netComp.packetWriter.Write(GobjIDCluster(cluster.Id));
                netComp.packetWriter.Write(addRBC);
                netComp.packetWriter.Write(addPLT);
                netComp.packetWriter.Write(addTNK);
                netComp.packetWriter.Write(addSIL);

                //send packet
                m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                    Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);
            }
        }

        public void HostClusterHybridCells(Common.CellCluster cluster,
            byte hybCount, Common.Cells.CellTypeEnum srcCellA,
            Common.Cells.CellTypeEnum srcCellB)
        {
            //update local copy
            Common.Cells.CellTypeEnum hybType = Common.Cells.CellTypeEnum.SMALL_HYBRID;
            if ((srcCellA == Common.Cells.CellTypeEnum.RED_BLOOD_CELL)
                || (srcCellA == Common.Cells.CellTypeEnum.PLATELET))
            {
                if ((srcCellB == Common.Cells.CellTypeEnum.RED_BLOOD_CELL)
                || (srcCellB == Common.Cells.CellTypeEnum.PLATELET))
                    hybType = Common.Cells.CellTypeEnum.SMALL_HYBRID;
                else
                    hybType = Common.Cells.CellTypeEnum.MED_HYBRID;
            }
            else
            {
                if ((srcCellB == Common.Cells.CellTypeEnum.RED_BLOOD_CELL)
                || (srcCellB == Common.Cells.CellTypeEnum.PLATELET))
                    hybType = Common.Cells.CellTypeEnum.MED_HYBRID;
                else
                    hybType = Common.Cells.CellTypeEnum.BIG_HYBRID;
            }

            cluster.HybridCells(hybType, hybCount, srcCellA, srcCellB);

            //send packet to everyone
            if (m_sessionDetails.isMultiplayer)
            {
                LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

                netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_HYB_CLUST_CELLS);
                netComp.packetWriter.Write(GobjIDCluster(cluster.Id));
                netComp.packetWriter.Write(hybCount);
                netComp.packetWriter.Write((byte)srcCellA);
                netComp.packetWriter.Write((byte)srcCellB);

                //send packet
                m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                    Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);
            }
        }

        public virtual void HostSplitCluster(Common.CellCluster srcCluster,
            byte splRBC, byte splPLT, byte splTNK, byte splSIL,
            byte splSHY, byte splMHY, byte splBHY)
        {
            //1. create a new cluster
            ClusterStateData clData = SplitClusterToData(srcCluster,
                splRBC, splPLT, splTNK, splSIL, splSHY, splMHY, splBHY);
            Common.CellCluster newCluster = HostClusterNew(clData,
                srcCluster.Position + Microsoft.Xna.Framework.Vector3.Forward);

            newCluster.ReadjustAll();

            if (srcCluster.stateData.virusRef.virusStateData.isMine)
                m_sndSplitCluster.Play();

            //2. add to virus
            srcCluster.stateData.virusRef.virusStateData.clusters.AddLast(newCluster);

            //3. send new cluster packet - and update source cluster
            if (m_sessionDetails.isMultiplayer)
            {
                HostSubmitNewCluster(newCluster);
                HostSubmitClusterUpdate(srcCluster);
            }
        }

        public void HostSubmitClusterBattle(Common.CellCluster clusterA, Common.CellCluster clusterB)
        {
            //1. Verse each cluster against each other - one must die
            Common.CellCluster winner = HostClustersBattle(clusterA, clusterB);

            if (winner.stateData.attnUnderAttack)
                HostUNWarnBattle((CellCluster)winner);

            winner.stateData.actionState = CellActionState.IDLE;
            winner.stateData.actionReObject = null;

            //2. if it is this player's cluster that just got attacked:
            //  make sure any in game menus are made inactive - potential error could occur
            if (((CommonGameResScn)ParentScene).m_hud.m_clusterMenu.Active)
            {
                if (clusterA.stateData.virusRef != null)
                {
                    if ((clusterA.stateData.virusRef.virusStateData.isMine) &&
                        (clusterA.stateData.virusRef.SelectedCluster == clusterA))
                        ((CommonGameResScn)ParentScene).m_hud.m_clusterMenu.Active = false;
                }
                if (clusterB.stateData.virusRef != null)
                {
                    if ((clusterB.stateData.virusRef.virusStateData.isMine) &&
                        (clusterB.stateData.virusRef.SelectedCluster == clusterB))
                        ((CommonGameResScn)ParentScene).m_hud.m_clusterMenu.Active = false;
                }
            }

            //2. Send cluster update packet to clients
            if (m_sessionDetails.isMultiplayer)
                HostSubmitClusterUpdate(winner);
        }

        public void HostSubmitClusterCombine(Common.CellCluster clusterA, Common.CellCluster clusterB)
        {
            //1. assert that clusters are of the same virus
            m_debugMgr.Assert(clusterA.stateData.virusOwnerId == clusterB.stateData.virusOwnerId,
                "BiophageScn:HostSubmitClusterCombine - clusters are not of the same virus.");

            //2. update cluster A - remove cluster B
            CombineClusters(clusterA, clusterB);

            clusterA.stateData.actionState = CellActionState.IDLE;
            clusterA.stateData.actionReObject = null;

            //2. Send cluster update packet to clients
            if (m_sessionDetails.isMultiplayer)
                HostSubmitClusterUpdate(clusterA);
        }

        private void HostClusterEvade(CellCluster srcCluster, CellCluster avoidCluster)
        {
            //1. change cluster action state and regarding
            srcCluster.stateData.actionReObject = avoidCluster;
            srcCluster.stateData.actionState = CellActionState.EVADING_ENEMY;

            //2. Send cluster update packet to clients
            if (m_sessionDetails.isMultiplayer)
                HostSubmitClusterUpdate(srcCluster);
        }

        public void HostClusterChase(CellCluster srcCluster, NetworkEntity networkEntity)
        {
            m_debugMgr.Assert(m_sessionDetails.isHost, "BiophageScn:HostClusterChase - only callable by host.");

            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            //1. change cluster action state and regarding
            srcCluster.stateData.actionReObject = networkEntity;
            
            if (networkEntity != null)
            {
                if (networkEntity is CellCluster)
                {
                    CellCluster targetCluster = (CellCluster)networkEntity;
                    if (targetCluster.stateData.virusOwnerId == srcCluster.stateData.virusOwnerId)
                        srcCluster.stateData.actionState = CellActionState.CHASING_CLUST_TO_COMBINE;
                    else
                    {
                        srcCluster.stateData.actionState = CellActionState.CHASING_ENEMY_TO_BATTLE;

                        if (targetCluster.stateData.numWhiteBloodCell == 0)
                        {
                            if (!targetCluster.stateData.virusRef.virusStateData.isBot)
                            {
                                if (!targetCluster.stateData.attnUnderAttack)
                                {
                                    if (targetCluster.stateData.virusRef.virusStateData.isMine)
                                        m_sndBattleWarning.Play();
                                    else
                                    {
                                        netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_BATTLE_WARNING);
                                        netComp.packetWriter.Write((byte)GobjIDCluster(targetCluster.Id));

                                        //send packet
                                        Microsoft.Xna.Framework.Net.NetworkGamer sendTo = netComp.GetNetworkSession.FindGamerById(
                                            targetCluster.stateData.virusRef.virusStateData.netPlayerId);
                                        if (sendTo != null)
                                        {
                                            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                                                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                                                sendTo);
                                        }
                                    }
                                }
                            }

                            targetCluster.stateData.attnUnderAttack = true;
                            targetCluster.stateData.attnAttackingEnemy = srcCluster;
                        }
                    }
                }
                else
                {
                    srcCluster.stateData.actionState = CellActionState.CHASING_UCELL_TOINFECT;
                }
            }


            //3. Send cluster update packet to clients
            if (m_sessionDetails.isMultiplayer)
                HostSubmitClusterUpdate(srcCluster);
        }

        public void HostUNWarnBattle(CellCluster cluster)
        {
            m_debugMgr.Assert(m_sessionDetails.isHost, "BiophageScn:HostUNWarnBattle - only callable by host.");

            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            cluster.stateData.attnUnderAttack = false;
            cluster.stateData.attnAttackingEnemy = null;

            //2. battle unwarning info
            if (cluster.stateData.numWhiteBloodCell == 0)
            {
                if (!cluster.stateData.virusRef.virusStateData.isBot)
                {
                    if (!cluster.stateData.virusRef.virusStateData.isMine)
                    {
                        netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_BATTLE_UNWARNING);
                        netComp.packetWriter.Write((byte)GobjIDCluster(cluster.Id));

                        //send packet
                        Microsoft.Xna.Framework.Net.NetworkGamer sendTo = netComp.GetNetworkSession.FindGamerById(
                                            cluster.stateData.virusRef.virusStateData.netPlayerId);
                        if (sendTo != null)
                        {
                            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                                sendTo);
                        }
                    }
                }
            }
        }

        private void HostSubmitMedication(Common.Cells.CellTypeEnum cellTypeToKill)
        {
            // 1. Administer medication
            ClusterMedication(cellTypeToKill);

            // 2. alert sound and update hud
            ((CommonGameResScn)ParentScene).m_hud.showMedicationAlert = false;
            ((CommonGameResScn)ParentScene).m_hud.medicationAlertCountDown = 0;
            m_sndMedicAlert.Play();

            // 3. Send packet
            if (m_sessionDetails.isMultiplayer)
            {
                LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

                netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_MEDICATION_DEPLOY);
                netComp.packetWriter.Write((byte)cellTypeToKill);

                //send packet
                m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                    Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);
            }
        }

        /// <summary>
        /// Basically ends the game.
        /// </summary>
        private void HostSubmitGameOver()
        {
            if (!m_finishedRanksScrn.isGameOver)
            {
                m_finishedRanksScrn.isGameOver = true;

                //make sure all viruses have a rank
                EndRanking();

                //submit final ranks to clients
                if (m_sessionDetails.isMultiplayer)
                {
                    LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

                    netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_GAME_OVER);
                    foreach (byte virusId in m_finishedRanksScrn.finishVirusRanks.Reverse())
                    {
                        netComp.packetWriter.Write(virusId);
                    }

                    //send packet
                    m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                        Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);
                }
            }
        }

        private void EndRanking()
        {
            if (m_finishedRanksScrn.finishVirusRanks.Count == m_viruses.Count)
                return;

            //rank the remaining viruses
            SortedList<double, Common.Virus> sortedRanks = new SortedList<double, Virus>();
            foreach (Virus virus in m_virusObjs)
            {
                if (virus.Active)
                {
                    double uniqueKey = virus.virusStateData.infectPercentage;
                    if (double.IsNaN(uniqueKey))
                        uniqueKey = 1f;
                    while (sortedRanks.ContainsKey(uniqueKey))
                    {
                        uniqueKey -= (uniqueKey * 0.05f) + double.Epsilon;
                        if (double.IsNaN(uniqueKey))
                            uniqueKey = 1f;
                    }

                    sortedRanks.Add(uniqueKey, virus);
                }
            }
            foreach (KeyValuePair<double, Virus> vrankKVP in sortedRanks)
            {
                m_finishedRanksScrn.finishVirusRanks.Push(GobjIDVirus(vrankKVP.Value.Id));
            }

            //asserts
            m_debugMgr.Assert(m_finishedRanksScrn.finishVirusRanks.Count == m_viruses.Count,
                "BiophageScn:EndRanking - finished ranks doesn't contain the correct number of viruses.");
        }

        private void HostSubmitImmuneCountDown(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // 1. set count down timer
            m_immuneCountDownStartSecs = gameTime.TotalRealTime.TotalSeconds;
            //activate hud alert
            ((CommonGameResScn)ParentScene).m_hud.showImmuneAlert = true;
            ((CommonGameResScn)ParentScene).m_hud.immuneAlertCountDown = (int)(GlobalConstants.GP_IMMUNESYS_TIMEOUT_MINS * 60.0);

            // 2. alert all clients
            if (m_sessionDetails.isMultiplayer)
            {
                LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

                netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_IMMUNE_COUNTDOWN);

                //send packet
                m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                    Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);
            }
        }

        private void HostSubmitMedicCountDown(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // 1. set count down timer
            m_medicCountDownStartSecs = gameTime.TotalRealTime.TotalSeconds;
            //activate hud alert
            ((CommonGameResScn)ParentScene).m_hud.showMedicationAlert = true;
            ((CommonGameResScn)ParentScene).m_hud.medicationAlertCountDown = (int)(GlobalConstants.GP_MEDICATION_TIMEOUT_MINS * 60.0);

            // 2. alert all clients
            if (m_sessionDetails.isMultiplayer)
            {
                LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

                netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETSERVER_MED_COUNTDOWN);

                //send packet
                m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                    Microsoft.Xna.Framework.Net.SendDataOptions.Reliable);
            }
        }

        #endregion

        #endregion

        #region client

        #region recieve

        public void ClientNetworkUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_debugMgr.Assert(!m_sessionDetails.isHost,
                "BiophageScn:ClientGeneralUpdate - only client can call this method.");

            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            #region recieve host data

            Microsoft.Xna.Framework.Net.NetworkGamer sender;
            while (m_sessionDetails.gamerMe.IsDataAvailable)
            {
                m_sessionDetails.gamerMe.ReceiveData(netComp.packetReader, out sender);
                GlobalConstants.NETPACKET_IDS packetId = (GlobalConstants.NETPACKET_IDS)netComp.packetReader.ReadByte();

                switch (packetId)
                {
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_UNINFECTED_CELLS_TRANS:
                        ClientUpUninfectCells(gameTime);
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_CELL_CLUSTERS_TRANS:
                        ClientUpClusters(gameTime);
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_NEW_CLUSTER:
                        ClientCreateNewCluster();
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_DIV_CLUST_CELLS:
                        ClientDivideClusterCells();
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_HYB_CLUST_CELLS:
                        ClientClusterHybridCells();
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_CLUSTER_UPDATE:
                        ClientUpdateCluster();
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_MEDICATION_DEPLOY:
                        ClientDeployMedication();
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_GAME_OVER:
                        ClientGameOver();
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_IMMUNE_COUNTDOWN:
                        ClientStartImmuneCountDown(gameTime);
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_MED_COUNTDOWN:
                        clientStartMedicCountDown(gameTime);
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_GAME_ISREADY:
                        m_gameStarted = true;
                        IsPaused = false;
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_BATTLE_WARNING:
                        ClientBattleWarning();
                        break;
                    case GlobalConstants.NETPACKET_IDS.NETSERVER_BATTLE_UNWARNING:
                        ClientBattleUnWarning();
                        break;


                    //ignores
                    case GlobalConstants.NETPACKET_IDS.NETCLIENT_NEW_CLUSTER_UCELL:
                    case GlobalConstants.NETPACKET_IDS.NETCLIENT_DIV_CLUST_CELLS:
                    case GlobalConstants.NETPACKET_IDS.NETCLIENT_HYB_CLUST_CELLS:
                    case GlobalConstants.NETPACKET_IDS.NETCLIENT_SPLIT_CLUSTER:
                    case GlobalConstants.NETPACKET_IDS.NETCLIENT_CLUSTER_CHASE:
                    case GlobalConstants.NETPACKET_IDS.NETCLIENT_CLUSTER_EVADE:
                    case GlobalConstants.NETPACKET_IDS.NETCLIENT_CLUSTER_CANCEL_ACTION:
                    case GlobalConstants.NETPACKET_IDS.NETCLIENT_ISREADY:
                        break;
                    default:
                        m_debugMgr.WriteLogEntry("BiophageScn:ClientUpdate - hmm..I don't know this packet. ID=" + packetId);
                        break;
                }
            }

            #endregion

            //tell host that I'm ready - if not done.
            if (!m_clientSentReadyFlag)
            {
                m_clientSentReadyFlag = true;

                netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETCLIENT_ISREADY);
                //send packet
                m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                    Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                    netComp.GetNetworkSession.Host);
            }
        }

        private void ClientBattleUnWarning()
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            //set warning
            byte clustId = netComp.packetReader.ReadByte();
            CellCluster cluster = (CellCluster)m_gameObjects[ClusterGobjID(clustId)];

            if (cluster != null)
                cluster.stateData.attnUnderAttack = false;
        }

        private void ClientBattleWarning()
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            //set warning
            byte clustId = netComp.packetReader.ReadByte();
            CellCluster cluster = (CellCluster)m_gameObjects[ClusterGobjID(clustId)];

            if (cluster != null)
            {
                cluster.stateData.attnUnderAttack = true;
                m_sndBattleWarning.Play();
            }
        }

        private void clientStartMedicCountDown(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_medicCountDownStartSecs = gameTime.TotalRealTime.TotalSeconds;
            ((CommonGameResScn)ParentScene).m_hud.showMedicationAlert = true;
            ((CommonGameResScn)ParentScene).m_hud.medicationAlertCountDown = (int)(GlobalConstants.GP_MEDICATION_WARN_SECS - 1.0);
        }

        private void ClientStartImmuneCountDown(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_immuneCountDownStartSecs = gameTime.TotalRealTime.TotalSeconds;
            ((CommonGameResScn)ParentScene).m_hud.showImmuneAlert = true;
            ((CommonGameResScn)ParentScene).m_hud.immuneAlertCountDown = (int)(GlobalConstants.GP_IMMUNESYS_WARN_SECS - 1.0);
        }

        private void ClientGameOver()
        {
            m_finishedRanksScrn.isGameOver = true;

            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;
            m_finishedRanksScrn.finishVirusRanks = new Stack<byte>(m_viruses.Count);
            foreach (byte virusId in m_viruses)
            {
                m_finishedRanksScrn.finishVirusRanks.Push(netComp.packetReader.ReadByte());
            }
        }

        void ClientUpUninfectCells(Microsoft.Xna.Framework.GameTime gameTime)
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            float timeStamp = netComp.packetReader.ReadSingle();

            LinkedList<byte> uCellsNotUpdated = new LinkedList<byte>(m_uninfectedCells);
            Common.Cells.UninfectedCell uCell;

            Microsoft.Xna.Framework.Vector3 cellNewPos;
            Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4 cellNetPosHVec
                = new Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4();

            Microsoft.Xna.Framework.Quaternion cellNewOrient;
            Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4 cellNetOrientHVec
                = new Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4();

            while (netComp.packetReader.Position < netComp.packetReader.Length)
            {
                byte cellNetId = netComp.packetReader.ReadByte();
                ulong cellNetPos = netComp.packetReader.ReadUInt64();
                ulong cellNetOrient = netComp.packetReader.ReadUInt64();

                //remove from 'not updated' list of cells
                uCellsNotUpdated.Remove(cellNetId);

                //get ref
                if (m_gameObjects.ContainsKey(UCellGobjID(cellNetId)))
                {
                    uCell = (Common.Cells.UninfectedCell)m_gameObjects[UCellGobjID(cellNetId)];

                    //unpack values
                    cellNetPosHVec.PackedValue = cellNetPos;
                    cellNewPos = new Microsoft.Xna.Framework.Vector3(
                        cellNetPosHVec.ToVector4().X,
                        cellNetPosHVec.ToVector4().Y,
                        cellNetPosHVec.ToVector4().Z);
                    cellNewPos *= cellNetPosHVec.ToVector4().W; //apply scalar

                    cellNetOrientHVec.PackedValue = cellNetOrient;
                    cellNewOrient = new Microsoft.Xna.Framework.Quaternion(
                        cellNetOrientHVec.ToVector4().X,
                        cellNetOrientHVec.ToVector4().Y,
                        cellNetOrientHVec.ToVector4().Z,
                        cellNetOrientHVec.ToVector4().W);

                    //correct predictions
                    uCell.Correct(gameTime, timeStamp, cellNewPos, cellNewOrient);
                }
            }

            //remove inactive ucells
            foreach (byte inactiveUCellId in uCellsNotUpdated)
            {
                UCellRemove(inactiveUCellId);
            }
        }

        void ClientUpClusters(Microsoft.Xna.Framework.GameTime gameTime)
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            float timeStamp = netComp.packetReader.ReadSingle();

            LinkedList<byte> clustersNotUpdated = new LinkedList<byte>(m_cellClusters);
            Common.CellCluster cluster;

            Microsoft.Xna.Framework.Vector3 clusterNewPos;
            Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4 clusterNetPosHVec
                = new Microsoft.Xna.Framework.Graphics.PackedVector.HalfVector4();

            while (netComp.packetReader.Position < netComp.packetReader.Length)
            {
                byte clusterNetId = netComp.packetReader.ReadByte();
                ulong clusterNetPos = netComp.packetReader.ReadUInt64();

                //remove from 'not updated' list of cells
                clustersNotUpdated.Remove(clusterNetId);

                //get ref
                if (m_gameObjects.ContainsKey(ClusterGobjID(clusterNetId)))
                {
                    cluster = (Common.CellCluster)m_gameObjects[ClusterGobjID(clusterNetId)];

                    //unpack values
                    clusterNetPosHVec.PackedValue = clusterNetPos;
                    clusterNewPos = new Microsoft.Xna.Framework.Vector3(
                        clusterNetPosHVec.ToVector4().X,
                        clusterNetPosHVec.ToVector4().Y,
                        clusterNetPosHVec.ToVector4().Z);
                    clusterNewPos *= clusterNetPosHVec.ToVector4().W; //apply scalar

                    //correct predictions
                    cluster.Correct(gameTime, timeStamp, clusterNewPos,
                        Microsoft.Xna.Framework.Quaternion.Identity);
                }
            }

            //remove all inactive clusters
            foreach (byte deadClustId in clustersNotUpdated)
            {
                ClusterDie(deadClustId);
            }
        }

        void ClientCreateNewCluster()
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            //make new state data - parse packet data into struct
            ClusterStateData clData = new ClusterStateData();
            byte clusterId = netComp.packetReader.ReadByte();
            clData.virusOwnerId = netComp.packetReader.ReadByte();
            if (clData.virusOwnerId == GlobalConstants.GP_WHITE_BLOODCELL_VIRUS_ID)
            {
                //cluster is white blood cell - so alert sound and update hud
                if (((CommonGameResScn)ParentScene).m_hud.showImmuneAlert)
                {
                    ((CommonGameResScn)ParentScene).m_hud.showImmuneAlert = false;
                    ((CommonGameResScn)ParentScene).m_hud.immuneAlertCountDown = 0;
                    m_sndGenAlert.Play();
                }
                clData.virusRef = null;
            }
            else
                clData.virusRef = (Common.Virus)m_gameObjects[VirusGobjID(clData.virusOwnerId)];
            clData.biophageScn = this;

            HalfVector4 hvPos = new HalfVector4();
            hvPos.PackedValue = netComp.packetReader.ReadUInt64();
            Microsoft.Xna.Framework.Vector3 clPos = new Microsoft.Xna.Framework.Vector3(
                hvPos.ToVector4().X, hvPos.ToVector4().Y, hvPos.ToVector4().Z);
            clPos *= hvPos.ToVector4().W;

            clData.numWhiteBloodCell = netComp.packetReader.ReadByte();

            clData.attrHealth = netComp.packetReader.ReadInt16();
            clData.attrNutrientStore = netComp.packetReader.ReadInt16();

            clData.numRBCs = netComp.packetReader.ReadByte();
            clData.numPlatelets = netComp.packetReader.ReadByte();
            clData.numSilos = netComp.packetReader.ReadByte();
            clData.numTanks = netComp.packetReader.ReadByte();
            clData.numSmallHybrids = netComp.packetReader.ReadByte();
            clData.numMediumHybrids = netComp.packetReader.ReadByte();
            clData.numBigHybrids = netComp.packetReader.ReadByte();

            clData.numCellsTotal = (short)(
                (short)clData.numRBCs + (short)clData.numPlatelets +
                (short)clData.numSilos + (short)clData.numTanks +
                (short)clData.numSmallHybrids + (short)clData.numMediumHybrids +
                (short)clData.numBigHybrids + (short)clData.numWhiteBloodCell);

            clData.actionState = CellActionState.IDLE;
            clData.actionReObject = null;

            Common.CellCluster cluster = new Common.CellCluster(
                    ClusterGobjID(clusterId),
                    clData, clPos, m_sessionDetails,
                    m_debugMgr, m_resMgr, this,
                    ((CommonGameResScn)ParentScene).m_hud);

            m_cellClusters.AddLast(GobjIDCluster(cluster.Id));
            m_cellClusterObjs.AddLast(cluster);

            //add to virus proper
            if ((clData.numWhiteBloodCell == 0) && (clData.virusRef != null))
            {
                Common.Virus virus = clData.virusRef;
                virus.virusStateData.clusters.AddLast(cluster);

                if ((virus.virusStateData.isMine) && (virus.virusStateData.clusters.Count > 1))
                    m_sndSplitCluster.Play();
            }
        }

        void ClientUpdateCluster()
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            //make new state data - parse packet data into struct
            byte clusterId = netComp.packetReader.ReadByte();
            Common.CellCluster cluster = null;
            if (m_gameObjects.ContainsKey(ClusterGobjID(clusterId)))
                cluster = (Common.CellCluster)m_gameObjects[ClusterGobjID(clusterId)];

            byte clActionState = netComp.packetReader.ReadByte();
            short clAttrHealth = netComp.packetReader.ReadInt16();
            short clAttrNStore = netComp.packetReader.ReadInt16();
            byte clNumRBCs = netComp.packetReader.ReadByte();
            byte clNumPLTs = netComp.packetReader.ReadByte();
            byte clNumSILs = netComp.packetReader.ReadByte();
            byte clNumTNKS = netComp.packetReader.ReadByte();
            byte clNumSmHY = netComp.packetReader.ReadByte();
            byte clNumMedHy = netComp.packetReader.ReadByte();
            byte clNumBigHY = netComp.packetReader.ReadByte();

            if (cluster != null)
            {
                cluster.stateData.actionState = (CellActionState)clActionState;

                cluster.stateData.attrHealth = clAttrHealth;
                cluster.stateData.attrNutrientStore = clAttrNStore;

                cluster.stateData.numRBCs = clNumRBCs;
                cluster.stateData.numPlatelets = clNumPLTs;
                cluster.stateData.numSilos = clNumSILs;
                cluster.stateData.numTanks = clNumTNKS;
                cluster.stateData.numSmallHybrids = clNumSmHY;
                cluster.stateData.numMediumHybrids = clNumMedHy;
                cluster.stateData.numBigHybrids = clNumBigHY;
                cluster.stateData.actionReObject = null;

                cluster.ReadjustAll();

                //if this player's cluster that just got attacked:
                //  make sure any in game menus are made inactive - potential error could occur
                if (((CommonGameResScn)ParentScene).m_hud.m_clusterMenu.Active)
                {
                    if (cluster.stateData.virusRef != null)
                    {
                        if ((cluster.stateData.virusRef.virusStateData.isMine) &&
                            (cluster.stateData.virusRef.SelectedCluster == cluster))
                            ((CommonGameResScn)ParentScene).m_hud.m_clusterMenu.Active = false;
                    }
                }
            }
        }

        void ClientDivideClusterCells()
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            Common.CellCluster cluster = (Common.CellCluster)m_gameObjects[ClusterGobjID(netComp.packetReader.ReadByte())];

            byte addRBCs = netComp.packetReader.ReadByte();
            byte addPLTs = netComp.packetReader.ReadByte();
            byte addTNKs = netComp.packetReader.ReadByte();
            byte addSILs = netComp.packetReader.ReadByte();

            if (cluster != null)
                cluster.DivideCells(addRBCs, addPLTs, addTNKs, addSILs);
        }

        void ClientClusterHybridCells()
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            Common.CellCluster cluster = (Common.CellCluster)m_gameObjects[ClusterGobjID(netComp.packetReader.ReadByte())];

            byte hybCount = netComp.packetReader.ReadByte();
            Common.Cells.CellTypeEnum srcCellA = (Common.Cells.CellTypeEnum)netComp.packetReader.ReadByte();
            Common.Cells.CellTypeEnum srcCellB = (Common.Cells.CellTypeEnum)netComp.packetReader.ReadByte();

            Common.Cells.CellTypeEnum hybType = Common.Cells.CellTypeEnum.SMALL_HYBRID;
            if ((srcCellA == Common.Cells.CellTypeEnum.RED_BLOOD_CELL)
                || (srcCellA == Common.Cells.CellTypeEnum.PLATELET))
            {
                if ((srcCellB == Common.Cells.CellTypeEnum.RED_BLOOD_CELL)
                || (srcCellB == Common.Cells.CellTypeEnum.PLATELET))
                    hybType = Common.Cells.CellTypeEnum.SMALL_HYBRID;
                else
                    hybType = Common.Cells.CellTypeEnum.MED_HYBRID;
            }
            else
            {
                if ((srcCellB == Common.Cells.CellTypeEnum.RED_BLOOD_CELL)
                || (srcCellB == Common.Cells.CellTypeEnum.PLATELET))
                    hybType = Common.Cells.CellTypeEnum.MED_HYBRID;
                else
                    hybType = Common.Cells.CellTypeEnum.BIG_HYBRID;
            }

            if (cluster != null)
                cluster.HybridCells(hybType, hybCount, srcCellA, srcCellB);
        }

        void ClientDeployMedication()
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            //1. Run the medication routine
            ClusterMedication((Common.Cells.CellTypeEnum)netComp.packetReader.ReadByte());

            // 2. alert sound and up hud
            ((CommonGameResScn)ParentScene).m_hud.showMedicationAlert = false;
            ((CommonGameResScn)ParentScene).m_hud.medicationAlertCountDown = 0;
            m_sndMedicAlert.Play();
        }

        #endregion

        #region submit

        public void ClientSendNewClusterFromUCell(byte ucellId)
        {
            //just gotta send ucell id - rest can be worked out from server
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETCLIENT_NEW_CLUSTER_UCELL);
            netComp.packetWriter.Write(ucellId);
            //send packet
            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                netComp.GetNetworkSession.Host);
        }

        public void ClientSendClusterDividedCells(Common.CellCluster cluster,
            byte addRBC, byte addPLT, byte addTNK, byte addSIL)
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETCLIENT_DIV_CLUST_CELLS);
            netComp.packetWriter.Write(GobjIDCluster(cluster.Id));

            netComp.packetWriter.Write(addRBC);
            netComp.packetWriter.Write(addPLT);
            netComp.packetWriter.Write(addTNK);
            netComp.packetWriter.Write(addSIL);

            //note: hybrids can't divide

            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                netComp.GetNetworkSession.Host);
        }

        public void ClientSendClusterHybrids(Common.CellCluster cluster,
            byte hybCount, Common.Cells.CellTypeEnum srcCellA, Common.Cells.CellTypeEnum srcCellB)
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETCLIENT_HYB_CLUST_CELLS);
            netComp.packetWriter.Write(GobjIDCluster(cluster.Id));
            netComp.packetWriter.Write(hybCount);
            netComp.packetWriter.Write((byte)srcCellA);
            netComp.packetWriter.Write((byte)srcCellB);

            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                netComp.GetNetworkSession.Host);
        }

        public void ClientSendSplitCluster(Common.CellCluster srcCluster,
            byte splRBC, byte splPLT, byte splTNK, byte splSIL,
            byte splSHY, byte splMHY, byte splBHY)
        {
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETCLIENT_SPLIT_CLUSTER);
            netComp.packetWriter.Write(GobjIDCluster(srcCluster.Id));
            netComp.packetWriter.Write(splRBC);
            netComp.packetWriter.Write(splPLT);
            netComp.packetWriter.Write(splTNK);
            netComp.packetWriter.Write(splSIL);
            netComp.packetWriter.Write(splSHY);
            netComp.packetWriter.Write(splMHY);
            netComp.packetWriter.Write(splBHY);

            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                netComp.GetNetworkSession.Host);
        }

        public void ClientSendClusterChase(CellCluster srcCluster, NetworkEntity target)
        {
            //just gotta send ucell id - rest can be worked out from server
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETCLIENT_CLUSTER_CHASE);
            netComp.packetWriter.Write(GobjIDCluster(srcCluster.Id));
            netComp.packetWriter.Write((ushort)target.Id);
            //send packet
            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                netComp.GetNetworkSession.Host);
        }

        public void ClientSendClusterEvade(CellCluster srcCluster, NetworkEntity avoiding)
        {
            //just gotta send ucell id - rest can be worked out from server
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETCLIENT_CLUSTER_EVADE);
            netComp.packetWriter.Write(GobjIDCluster(srcCluster.Id));
            netComp.packetWriter.Write(GobjIDCluster(avoiding.Id));
            //send packet
            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                netComp.GetNetworkSession.Host);
        }

        public void ClientSendClusterCancelAction(CellCluster srcCluster)
        {
            //just gotta send ucell id - rest can be worked out from server
            LnaNetworkSessionComponent netComp = m_sessionDetails.netSessionComponent;

            netComp.packetWriter.Write((byte)GlobalConstants.NETPACKET_IDS.NETCLIENT_CLUSTER_CANCEL_ACTION);
            netComp.packetWriter.Write(GobjIDCluster(srcCluster.Id));
            //send packet
            m_sessionDetails.gamerMe.SendData(netComp.packetWriter,
                Microsoft.Xna.Framework.Net.SendDataOptions.Reliable,
                netComp.GetNetworkSession.Host);
        }

        #endregion

        #endregion

        #endregion

        #endregion
    }
}
