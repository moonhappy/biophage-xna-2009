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
using Microsoft.Xna.Framework.Graphics;


namespace Biophage.Game.Stages.Game.Common
{
    #region attributes

    public struct VirusStateData
    {
        public Microsoft.Xna.Framework.Graphics.Color colour;
        public bool isBot;
        public bool isMine;
        public bool isAlive;
        public byte netPlayerId;

        public LinkedList<CellCluster> clusters;

        public int numInfectedCells;
        public double infectPercentage;
        public int rank;

        public string ownerName;
    }

    public class VirusAIDataSet
    {
        public double vir_total_numhybs =       0.0;
        public double vir_total_numclusters =   0.0;
        public double vir_medicalert =          0.0;
    }

    #endregion

    /// <summary>
    /// Represents the virus, used to manage infected cell clusters.
    /// Also holds other properties, like if the virus is an NPC (for
    /// AI routines), stats, and identity attributes.
    /// </summary>
    public class Virus : CollidableAsset
    {
        #region fields

        public SessionDetails m_sessionDetails;
        public VirusStateData virusStateData;

        public VirusCapsid m_virusCapsid;

        public BiophageGameBaseScn m_biophageScn;

        CellCluster m_selectedCluster = null;
        LinkedListNode<CellCluster> m_selectedClusterListNode;
        public CellCluster SelectedCluster
        {
            get { return m_selectedCluster; }
        }

        public PlayerCursor m_playerCursor;
        public PlayerCursor CursorRef
        {
            get { return m_playerCursor; }
        }

        public bool hasBeenRanked = false;

        public HUDOverlay m_hud;

        public double aiIQ = 0.5;
        private int aiAllowThoughtAlpha = 0;
        public VirusAI aiVirus;

        public bool m_underAttack = false;

        #endregion

        #region methods

        #region construction

        public Virus(   uint id, SessionDetails sessionDetails, 
                        VirusStateData setVirusStateData, Vector3 pos,
                        PlayerCursor playerCursor,
                        DebugManager debugMgr, ResourceManager resourceMgr,
                        BiophageGameBaseScn parentScn)
            : base(id, pos, debugMgr, resourceMgr, (Scene)parentScn, true)
        {
            m_sessionDetails = sessionDetails;
            virusStateData = setVirusStateData;
            m_playerCursor = playerCursor;
            aiVirus = new VirusAI(debugMgr, this);

            m_debugMgr.Assert(m_playerCursor != null,
                "Virus:Constructor - 'virus' is null.");

            m_virusCapsid = new VirusCapsid(id, pos, debugMgr, resourceMgr, this, parentScn.Stage.SceneMgr.Game.GraphicsMgr);

            Visible = true;
            m_biophageScn = parentScn;
            m_physBody = null;
        }

        #endregion

        #region field accessors

        public BiophageGameBaseScn GetBiophageScn
        {
            get { return m_biophageScn; }
        }

        public override Vector3 Position
        {
            get
            {
                if (m_selectedCluster != null)
                {
                    switch (m_selectedCluster.stateData.actionState)
                    {
                        case CellActionState.WAITING_FOR_ORDER:
                        case CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_UCELL_SELECTED:
                            return m_playerCursor.Position;
                        default:
                            return m_selectedCluster.Position;
                    }
                }

                return m_virusCapsid.Position;
            }
        }

        public override Vector3 ForwardDir
        {
            get
            {
                if (m_selectedCluster != null)
                {
                    switch (m_selectedCluster.stateData.actionState)
                    {
                        case CellActionState.WAITING_FOR_ORDER:
                        case CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_UCELL_SELECTED:
                            return m_playerCursor.ForwardDir;
                        default:
                            return m_selectedCluster.ForwardDir;
                    }
                }

                return m_virusCapsid.ForwardDir;
            }
        }

        public override Vector3 UpDir
        {
            get
            {
                if (m_selectedCluster != null)
                {
                    switch (m_selectedCluster.stateData.actionState)
                    {
                        case CellActionState.WAITING_FOR_ORDER:
                        case CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_UCELL_SELECTED:
                            return m_playerCursor.UpDir;
                        default:
                            return m_selectedCluster.UpDir;
                    }
                }

                return m_virusCapsid.UpDir;
            }
        }

        public float CamDistance
        {
            get
            {
                if (m_selectedCluster != null)
                {
                    switch (m_selectedCluster.stateData.actionState)
                    {
                        case CellActionState.WAITING_FOR_ORDER:
                        case CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_UCELL_SELECTED:
                            return 10f;
                        default:
                            return m_selectedCluster.CameraDistance;
                    }
                }

                return 10f;
            }
        }

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                if (!m_virusCapsid.Init())
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
            Deinit();

            return Init();
        }

        #region loading

        public override bool Load()
        {
            bool retVal = true;
            if (!m_isLoaded)
            {
                if (!m_virusCapsid.Load())
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
                if (!m_virusCapsid.Unload())
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
                if (!m_virusCapsid.Deinit())
                    retVal = false;

                if (retVal)
                    m_isInit = false;
                else
                    m_isInit = true;
            }

            return retVal;
        }

        #endregion

        #region ai

        public void AIModifyClusterIQ()
        {
            if (virusStateData.isBot)
            {
                //determine the dividend of the intelligence quotient
                int iqDividend = (int)(1.0 / aiIQ);
                if (iqDividend == 0)
                    iqDividend = 4;

                //incase iqDividend is less than count so at least one will be given intelligence
                if (virusStateData.clusters.Count < iqDividend)
                    aiAllowThoughtAlpha = 0;

                int index = aiAllowThoughtAlpha;
                foreach (CellCluster cluster in virusStateData.clusters)
                {
                    cluster.m_clusterAI.m_aiAllowThought = false;
                    if ((index % iqDividend) == 0)
                        cluster.m_clusterAI.m_aiAllowThought = true;

                    index++;
                }

                aiAllowThoughtAlpha++;
                if (aiAllowThoughtAlpha > iqDividend)
                    aiAllowThoughtAlpha = 1;
            }
        }

        #endregion

        #region game loop

        /// <summary>
        /// Only handle input if still alive with no clusters.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <param name="newGPState"></param>
        /// <param name="oldGPState"></param>
        /// <param name="newKBState"></param>
        /// <param name="oldKBState"></param>
        public void Input(GameTime gameTime,

                            ref Microsoft.Xna.Framework.Input.GamePadState newGPState,
                            ref Microsoft.Xna.Framework.Input.GamePadState oldGPState
#if !XBOX
                            ,ref Microsoft.Xna.Framework.Input.KeyboardState newKBState,
                            ref Microsoft.Xna.Framework.Input.KeyboardState oldKBState
#endif
            )
        {
            m_debugMgr.Assert(virusStateData.isMine,
                "Virus:Input - bot's shouldn't call Input routine.");

            if (Active)
            {
                if (m_virusCapsid.Active)
                    m_virusCapsid.Input(gameTime,
                        ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                else if ((m_selectedCluster == null) && (virusStateData.clusters.Count > 0))
                {
                    m_selectedClusterListNode = virusStateData.clusters.First;
                    m_selectedCluster = m_selectedClusterListNode.Value;
                    m_hud.m_clusterMenu.Cluster = m_selectedCluster;
                }

                else if ((m_selectedCluster != null) && (virusStateData.clusters.Contains(m_selectedCluster)))
                {
                    //do cluster input
                    m_selectedCluster.Input(gameTime,
                        ref newGPState, ref oldGPState
#if !XBOX
, ref newKBState, ref oldKBState
#endif
);
                    #region action state input
                    switch (m_selectedCluster.stateData.actionState)
                    {
                        case CellActionState.WAITING_FOR_ORDER:
                        case CellActionState.WAITING_WITH_ENEMY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_MY_CLUSTER_SELECTED:
                        case CellActionState.WAITING_WITH_UCELL_SELECTED:
                            break;
                        default:
                            if (virusStateData.clusters.Count > 1)
                            {
                                //select previous cluster
                                if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.LeftShoulder) &&
                                oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.LeftShoulder))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q) &&
                                oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Q))
#endif
)
                                {
                                    //select previous cluster
                                    if (m_selectedClusterListNode.Previous != null)
                                        m_selectedClusterListNode = m_selectedClusterListNode.Previous;
                                    else
                                        m_selectedClusterListNode = virusStateData.clusters.Last;

                                    m_selectedCluster = m_selectedClusterListNode.Value;
                                    m_hud.m_clusterMenu.Cluster = m_selectedCluster;
                                }

                                //select next cluster
                                else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.RightShoulder) &&
                                    oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.RightShoulder))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.E) &&
             oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.E))
#endif
)
                                {
                                    //select next cluster
                                    if (m_selectedClusterListNode.Next != null)
                                        m_selectedClusterListNode = m_selectedClusterListNode.Next;
                                    else
                                        m_selectedClusterListNode = virusStateData.clusters.First;

                                    m_selectedCluster = m_selectedClusterListNode.Value;
                                    m_hud.m_clusterMenu.Cluster = m_selectedCluster;
                                }

                                //select next cluster under attack
                                else if ((newGPState.IsButtonDown(Microsoft.Xna.Framework.Input.Buttons.RightStick) &&
                                    oldGPState.IsButtonUp(Microsoft.Xna.Framework.Input.Buttons.RightStick))
#if !XBOX
 || (newKBState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space) &&
             oldKBState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Space))
#endif
)
                                {
                                    //select next cluster that is under attack
                                    LinkedListNode<CellCluster> nextClust = m_selectedClusterListNode;
                                    bool found = false;

                                    while (!found)
                                    {
                                        if (nextClust.Next == null)
                                            nextClust = virusStateData.clusters.First;
                                        else
                                            nextClust = nextClust.Next;

                                        if ((nextClust.Value.stateData.attnUnderAttack) ||
                                            (nextClust == m_selectedClusterListNode))
                                            found = true;
                                    }

                                    m_selectedClusterListNode = nextClust;

                                    m_selectedCluster = m_selectedClusterListNode.Value;
                                    m_hud.m_clusterMenu.Cluster = m_selectedCluster;
                                }
                            }
                            break;
                    }
                    #endregion
                }

                else if (m_selectedCluster != null) // selected cluster has died
                {
                    if (virusStateData.clusters.Count == 0)
                    {
                        m_selectedClusterListNode = null;
                        m_selectedCluster = null;
                        m_hud.m_clusterMenu.Cluster = null;
                    }
                    else
                    {
                        m_selectedClusterListNode = virusStateData.clusters.First;
                        m_selectedCluster = m_selectedClusterListNode.Value;
                        m_hud.m_clusterMenu.Cluster = m_selectedCluster;
                    }
                }
            }
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //only do update if server
            if (m_sessionDetails.isHost)
            {
                if (virusStateData.isBot)
                {
                    //do NPC strategy based AI
                    aiVirus.AIUpdate(gameTime);
                }
            }

            if ((!m_virusCapsid.Active) && (m_sessionDetails.isHost))
                CheckIfLoser(gameTime);
            else
            {
                //capsid has a set lifespan - so make sure lifespan is not up
                m_virusCapsid.Update(gameTime);
                m_virusCapsid.Animate(gameTime);
            }
        }

        void CheckIfLoser(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //check if cluster has been removed since last update
            if (!virusStateData.clusters.Contains(m_selectedCluster))
            {
                if ((virusStateData.clusters.Count == 0)&&(m_sessionDetails.isHost))
                    VirusDied(gameTime);
                else
                {
                    //select a cluster
                    m_selectedClusterListNode = virusStateData.clusters.First;
                    m_selectedCluster = m_selectedClusterListNode.Value;
                    if (m_hud != null)
                        m_hud.m_clusterMenu.Cluster = m_selectedCluster;
                }
            }
        }

        private void VirusDied(GameTime gameTime)
        {
            m_debugMgr.Assert(m_sessionDetails.isHost,
                "Virus:VirusDied - method can only be called by host.");

            Active = false;

            if (virusStateData.isMine)
            {
                m_playerCursor.Visible = true;
                m_playerCursor.Position = Microsoft.Xna.Framework.Vector3.Zero;
                m_playerCursor.PhysBody.EnableBody();
            }
        }

        public override void Animate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        public override void Draw(  Microsoft.Xna.Framework.GameTime gameTime, 
                                    Microsoft.Xna.Framework.Graphics.GraphicsDevice graphicsDevice, 
                                    CameraGObj camera)
        {
            //render the virus capsid if active
            if (m_virusCapsid.Active)
                m_virusCapsid.DoDraw(gameTime, graphicsDevice, camera);
        }

        #endregion

        #endregion
    }
}
