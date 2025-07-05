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

namespace Biophage.Game.Stages.Game.Common
{
    /// <summary>
    /// Provides a framework between the cell cluster and its fuzzy logic
    /// routines.
    /// </summary>
    public class ClusterAI
    {
        #region fields

        private DebugManager m_debugMgr;
        private CellCluster m_cluster;

        private ClusterAIdataset m_aiDataFrount = new ClusterAIdataset();
        private ClusterAIdataset m_aiDataBack = new ClusterAIdataset();

        private double m_aiSecSinceLastAIUpdate;
        private bool m_aiUpFocusOnUCells = true;

        public bool m_aiAllowThought = true;
        public double m_aiSecSinceLastRequest = 0.0;

        private ClusterAIReqestAction m_aiReqAction = new ClusterAIReqestAction();
        public VirusOverrideRequestAction m_virusOverrideReqAction = VirusOverrideRequestAction.NONE;

        #region fuzzy modules

        private FuzzyModule m_fmEvadeAttackingEnem;
        private FuzzyModule m_fmChaseAttackingEnem;
        private FuzzyModule m_fmChaseEnemBattle;
        private FuzzyModule m_fmChaseUCellRBC;
        private FuzzyModule m_fmChaseUCellPLT;
        private FuzzyModule m_fmChaseUCellTNK;
        private FuzzyModule m_fmChaseUCellSIL;
        private FuzzyModule m_fmDivCellANY;
        private FuzzyModule m_fmDivCellRBC;
        private FuzzyModule m_fmDivCellPLT;
        private FuzzyModule m_fmDivCellTNK;
        private FuzzyModule m_fmDivCellSIL;

        private SortedList<double, ClusterAIReqestAction> m_scoreRankings;

        #endregion

        #endregion

        #region properties

        public ClusterAIdataset AIDataSet
        {
            get { return m_aiDataFrount; }
        }

        public ClusterAIReqestAction RequestedAction
        {
            get { return m_aiReqAction; }
        }

        #endregion

        #region methods

        #region construction

        public ClusterAI(DebugManager debugMgr, CellCluster cluster)
        {
            m_debugMgr = debugMgr;
            m_cluster = cluster;
            m_scoreRankings = new SortedList<double, ClusterAIReqestAction>(10, new DoubleReverse());

            //allow insequence updates between bot clusters for better efficiancy
            Random rnd = new Random();
            m_aiSecSinceLastAIUpdate = rnd.NextDouble() * 0.5;

            InitFzMEvadeAttackingEnem();
            InitFzMChaseAttackingEnem();
            InitFzMChaseEnemyBattle();
            InitFzMChaseRBC();
            InitFzMChasePLT();
            InitFzMChaseTNK();
            InitFzMChaseSIL();
            InitFzMDivideANY();
            InitFzMDivideRBC();
            InitFzMDividePLT();
            InitFzMDivideTNK();
            InitFzMDivideSIL();
        }

        private void InitFzMEvadeAttackingEnem()
        {
            m_fmEvadeAttackingEnem = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_underattack = m_fmEvadeAttackingEnem.CreateFuzzyVar("flv_underattack");
            FuzzySetProxy set_underattack_yes = flv_underattack.AddSingletonSet(
                "set_underattack_yes", 
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN, 
                GlobalConstants.AI_FLV_ISTRUE_YES_PEAK, 
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);

            FuzzyVariable flv_atckenemy_power = m_fmEvadeAttackingEnem.CreateFuzzyVar("flv_atckenemy_power");
            FuzzySetProxy set_atckenemy_power_weak = flv_atckenemy_power.AddLeftShoulderSet(
                "set_atckenemy_power_weak", 
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MIN, 
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_PEAK, 
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MAX);
            FuzzySetProxy set_atckenemy_power_strong = flv_atckenemy_power.AddRightShoulderSet(
                "set_atckenemy_power_strong", 
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MIN, 
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_PEAK, 
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MAX);
        

            //desirability
            FuzzyVariable cl_evade_enemy = m_fmEvadeAttackingEnem.CreateFuzzyVar("cl_evade_enemy");
            FuzzySetProxy cl_evade_enemy_undesirable = cl_evade_enemy.AddLeftShoulderSet(
                "cl_evade_enemy_undesirable", 
                GlobalConstants.AI_CL_UNDESIRABLE_MIN, 
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK, 
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_evade_enemy_desirable = cl_evade_enemy.AddRightShoulderSet(
                "cl_evade_enemy_desirable", 
                GlobalConstants.AI_CL_DESIRABLE_MIN, 
                GlobalConstants.AI_CL_DESIRABLE_PEAK, 
                GlobalConstants.AI_CL_DESIRABLE_MAX);
        
            //Rules
            m_fmEvadeAttackingEnem.AddRule(new FzAND(set_underattack_yes, set_atckenemy_power_strong), cl_evade_enemy_desirable);
            m_fmEvadeAttackingEnem.AddRule(new FzAND(set_underattack_yes, set_atckenemy_power_weak), cl_evade_enemy_undesirable);

            m_fmEvadeAttackingEnem.AddRule(new FzAND(new FzNOT(set_underattack_yes), set_atckenemy_power_strong), cl_evade_enemy_undesirable);
            m_fmEvadeAttackingEnem.AddRule(new FzAND(new FzNOT(set_underattack_yes), set_atckenemy_power_weak), cl_evade_enemy_undesirable);
        }

        private void InitFzMChaseAttackingEnem()
        {
            m_fmChaseAttackingEnem = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_underattack = m_fmChaseAttackingEnem.CreateFuzzyVar("flv_underattack");
            FuzzySetProxy set_underattack_yes = flv_underattack.AddSingletonSet(
                "set_underattack_yes", 
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN, 
                GlobalConstants.AI_FLV_ISTRUE_YES_PEAK, 
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);

            FuzzyVariable flv_atckenemy_power = m_fmChaseAttackingEnem.CreateFuzzyVar("flv_atckenemy_power");
            FuzzySetProxy set_atckenemy_power_weak = flv_atckenemy_power.AddLeftShoulderSet(
                "set_atckenemy_power_weak", 
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MIN, 
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_PEAK, 
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MAX);
            FuzzySetProxy set_atckenemy_power_strong = flv_atckenemy_power.AddRightShoulderSet(
                "set_atckenemy_power_strong", 
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MIN, 
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_PEAK, 
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MAX);

            FuzzyVariable flv_myclust_power = m_fmChaseAttackingEnem.CreateFuzzyVar("flv_myclust_power");
            FuzzySetProxy set_myclust_power_weak = flv_myclust_power.AddLeftShoulderSet(
                "set_myclust_power_weak",
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MAX);
            FuzzySetProxy set_myclust_power_strong = flv_myclust_power.AddRightShoulderSet(
                "set_myclust_power_strong",
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MAX);

            //desirability
            FuzzyVariable cl_chase_atckenemy = m_fmChaseAttackingEnem.CreateFuzzyVar("cl_chase_atckenemy");
            FuzzySetProxy cl_chase_atckenemy_undesirable = cl_chase_atckenemy.AddLeftShoulderSet(
                "cl_chase_atckenemy_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_chase_atckenemy_desirable = cl_chase_atckenemy.AddRightShoulderSet(
                "cl_chase_atckenemy_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmChaseAttackingEnem.AddRule(new FzAND(set_underattack_yes, set_myclust_power_strong, set_atckenemy_power_weak), cl_chase_atckenemy_desirable);
            m_fmChaseAttackingEnem.AddRule(new FzAND(set_underattack_yes, set_myclust_power_strong, set_atckenemy_power_strong), cl_chase_atckenemy_undesirable);
            m_fmChaseAttackingEnem.AddRule(new FzAND(set_underattack_yes, set_myclust_power_weak, set_atckenemy_power_weak), cl_chase_atckenemy_undesirable);
            m_fmChaseAttackingEnem.AddRule(new FzAND(set_underattack_yes, set_myclust_power_weak, set_atckenemy_power_strong), cl_chase_atckenemy_undesirable);

            m_fmChaseAttackingEnem.AddRule(new FzAND(new FzNOT(set_underattack_yes), set_myclust_power_strong, set_atckenemy_power_weak), cl_chase_atckenemy_undesirable);
            m_fmChaseAttackingEnem.AddRule(new FzAND(new FzNOT(set_underattack_yes), set_myclust_power_strong, set_atckenemy_power_strong), cl_chase_atckenemy_undesirable);
            m_fmChaseAttackingEnem.AddRule(new FzAND(new FzNOT(set_underattack_yes), set_myclust_power_weak, set_atckenemy_power_weak), cl_chase_atckenemy_undesirable);
            m_fmChaseAttackingEnem.AddRule(new FzAND(new FzNOT(set_underattack_yes), set_myclust_power_weak, set_atckenemy_power_strong), cl_chase_atckenemy_undesirable);
        }

        private void InitFzMChaseEnemyBattle()
        {
            m_fmChaseEnemBattle = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_clstenemy_power = m_fmChaseEnemBattle.CreateFuzzyVar("flv_clstenemy_power");
            FuzzySetProxy set_clstenemy_power_weak = flv_clstenemy_power.AddLeftShoulderSet(
                "set_clstenemy_power_weak",
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MAX);
            FuzzySetProxy set_clstenemy_power_strong = flv_clstenemy_power.AddRightShoulderSet(
                "set_clstenemy_power_strong",
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MAX);

            FuzzyVariable flv_myclust_power = m_fmChaseEnemBattle.CreateFuzzyVar("flv_myclust_power");
            FuzzySetProxy set_myclust_power_weak = flv_myclust_power.AddLeftShoulderSet(
                "set_myclust_power_weak",
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MAX);
            FuzzySetProxy set_myclust_power_strong = flv_myclust_power.AddRightShoulderSet(
                "set_myclust_power_strong",
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MAX);

            FuzzyVariable flv_dist = m_fmChaseEnemBattle.CreateFuzzyVar("flv_dist");
            FuzzySetProxy set_dist_near = flv_dist.AddLeftShoulderSet(
                "set_dist_near",
                GlobalConstants.AI_FLV_DIST_NEAR_MIN,
                GlobalConstants.AI_FLV_DIST_NEAR_PEAK,
                GlobalConstants.AI_FLV_DIST_NEAR_MAX);
            FuzzySetProxy set_dist_far = flv_dist.AddRightShoulderSet(
                "set_dist_far",
                GlobalConstants.AI_FLV_DIST_FAR_MIN,
                GlobalConstants.AI_FLV_DIST_FAR_PEAK,
                GlobalConstants.AI_FLV_DIST_FAR_MAX);

            //desirability
            FuzzyVariable cl_chase_clstenemy = m_fmChaseEnemBattle.CreateFuzzyVar("cl_chase_clstenemy");
            FuzzySetProxy cl_chase_clstenemy_undesirable = cl_chase_clstenemy.AddLeftShoulderSet(
                "cl_chase_clstenemy_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_chase_clstenemy_desirable = cl_chase_clstenemy.AddRightShoulderSet(
                "cl_chase_clstenemy_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmChaseEnemBattle.AddRule(new FzAND(new FzFAIRLY(set_dist_near), new FzFAIRLY(set_clstenemy_power_weak), set_myclust_power_strong), cl_chase_clstenemy_desirable);
            m_fmChaseEnemBattle.AddRule(new FzAND(new FzFAIRLY(set_dist_near), new FzFAIRLY(set_clstenemy_power_weak), set_myclust_power_weak), cl_chase_clstenemy_undesirable);
            m_fmChaseEnemBattle.AddRule(new FzAND(new FzFAIRLY(set_dist_near), new FzFAIRLY(set_clstenemy_power_strong), set_myclust_power_strong), cl_chase_clstenemy_undesirable);
            m_fmChaseEnemBattle.AddRule(new FzAND(new FzFAIRLY(set_dist_near), new FzFAIRLY(set_clstenemy_power_strong), set_myclust_power_weak), cl_chase_clstenemy_undesirable);

            m_fmChaseEnemBattle.AddRule(new FzAND(new FzFAIRLY(set_dist_far), new FzFAIRLY(set_clstenemy_power_weak), set_myclust_power_strong), cl_chase_clstenemy_undesirable);
            m_fmChaseEnemBattle.AddRule(new FzAND(new FzFAIRLY(set_dist_far), new FzFAIRLY(set_clstenemy_power_weak), set_myclust_power_weak), cl_chase_clstenemy_undesirable);
            m_fmChaseEnemBattle.AddRule(new FzAND(new FzFAIRLY(set_dist_far), new FzFAIRLY(set_clstenemy_power_strong), set_myclust_power_strong), cl_chase_clstenemy_undesirable);
            m_fmChaseEnemBattle.AddRule(new FzAND(new FzFAIRLY(set_dist_far), new FzFAIRLY(set_clstenemy_power_strong), set_myclust_power_weak), cl_chase_clstenemy_undesirable);
        }

        private void InitFzMChaseRBC()
        {
            m_fmChaseUCellRBC = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_rbc_count = m_fmChaseUCellRBC.CreateFuzzyVar("flv_rbc_count");
            FuzzySetProxy set_rbc_count_low = flv_rbc_count.AddLeftShoulderSet(
                "set_rbc_count_low",
                GlobalConstants.AI_FLV_SCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_SCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_LOW_MAX);
            FuzzySetProxy set_rbc_count_high = flv_rbc_count.AddRightShoulderSet(
                "set_rbc_count_high",
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MAX);

            FuzzyVariable flv_dist = m_fmChaseUCellRBC.CreateFuzzyVar("flv_dist");
            FuzzySetProxy set_dist_near = flv_dist.AddLeftShoulderSet(
                "set_dist_near",
                GlobalConstants.AI_FLV_DIST_NEAR_MIN,
                GlobalConstants.AI_FLV_DIST_NEAR_PEAK,
                GlobalConstants.AI_FLV_DIST_NEAR_MAX);
            FuzzySetProxy set_dist_far = flv_dist.AddRightShoulderSet(
                "set_dist_far",
                GlobalConstants.AI_FLV_DIST_FAR_MIN,
                GlobalConstants.AI_FLV_DIST_FAR_PEAK,
                GlobalConstants.AI_FLV_DIST_FAR_MAX);

            //desirability
            FuzzyVariable cl_chase_rbc = m_fmChaseUCellRBC.CreateFuzzyVar("cl_chase_rbc");
            FuzzySetProxy cl_chase_rbc_undesirable = cl_chase_rbc.AddLeftShoulderSet(
                "cl_chase_rbc_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_chase_rbc_desirable = cl_chase_rbc.AddRightShoulderSet(
                "cl_chase_rbc_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmChaseUCellRBC.AddRule(new FzAND(set_rbc_count_low, set_dist_near), cl_chase_rbc_desirable);
            m_fmChaseUCellRBC.AddRule(new FzAND(set_rbc_count_low, set_dist_far), cl_chase_rbc_desirable);

            m_fmChaseUCellRBC.AddRule(new FzAND(set_rbc_count_high, set_dist_near), cl_chase_rbc_undesirable);
            m_fmChaseUCellRBC.AddRule(new FzAND(set_rbc_count_high, set_dist_far), cl_chase_rbc_undesirable);
        }

        private void InitFzMChasePLT()
        {
            m_fmChaseUCellPLT = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_plt_count = m_fmChaseUCellPLT.CreateFuzzyVar("flv_plt_count");
            FuzzySetProxy set_plt_count_low = flv_plt_count.AddLeftShoulderSet(
                "set_plt_count_low",
                GlobalConstants.AI_FLV_SCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_SCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_LOW_MAX);
            FuzzySetProxy set_plt_count_high = flv_plt_count.AddRightShoulderSet(
                "set_plt_count_high",
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MAX);

            FuzzyVariable flv_dist = m_fmChaseUCellPLT.CreateFuzzyVar("flv_dist");
            FuzzySetProxy set_dist_near = flv_dist.AddLeftShoulderSet(
                "set_dist_near",
                GlobalConstants.AI_FLV_DIST_NEAR_MIN,
                GlobalConstants.AI_FLV_DIST_NEAR_PEAK,
                GlobalConstants.AI_FLV_DIST_NEAR_MAX);
            FuzzySetProxy set_dist_far = flv_dist.AddRightShoulderSet(
                "set_dist_far",
                GlobalConstants.AI_FLV_DIST_FAR_MIN,
                GlobalConstants.AI_FLV_DIST_FAR_PEAK,
                GlobalConstants.AI_FLV_DIST_FAR_MAX);

            //desirability
            FuzzyVariable cl_chase_plt = m_fmChaseUCellPLT.CreateFuzzyVar("cl_chase_plt");
            FuzzySetProxy cl_chase_plt_undesirable = cl_chase_plt.AddLeftShoulderSet(
                "cl_chase_plt_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_chase_plt_desirable = cl_chase_plt.AddRightShoulderSet(
                "cl_chase_plt_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmChaseUCellPLT.AddRule(new FzAND(set_plt_count_low, set_dist_near), cl_chase_plt_desirable);
            m_fmChaseUCellPLT.AddRule(new FzAND(set_plt_count_low, set_dist_far), cl_chase_plt_desirable);

            m_fmChaseUCellPLT.AddRule(new FzAND(set_plt_count_high, set_dist_near), cl_chase_plt_undesirable);
            m_fmChaseUCellPLT.AddRule(new FzAND(set_plt_count_high, set_dist_far), cl_chase_plt_undesirable);
        }

        private void InitFzMChaseTNK()
        {
            m_fmChaseUCellTNK = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_tnk_count = m_fmChaseUCellTNK.CreateFuzzyVar("flv_tnk_count");
            FuzzySetProxy set_tnk_count_low = flv_tnk_count.AddLeftShoulderSet(
                "set_tnk_count_low",
                GlobalConstants.AI_FLV_OCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_OCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_OCOUNT_LOW_MAX);
            FuzzySetProxy set_tnk_count_high = flv_tnk_count.AddRightShoulderSet(
                "set_tnk_count_high",
                GlobalConstants.AI_FLV_OCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_OCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_OCOUNT_HIGH_MAX);

            FuzzyVariable flv_dist = m_fmChaseUCellTNK.CreateFuzzyVar("flv_dist");
            FuzzySetProxy set_dist_near = flv_dist.AddLeftShoulderSet(
                "set_dist_near",
                GlobalConstants.AI_FLV_DIST_NEAR_MIN,
                GlobalConstants.AI_FLV_DIST_NEAR_PEAK,
                GlobalConstants.AI_FLV_DIST_NEAR_MAX);
            FuzzySetProxy set_dist_far = flv_dist.AddRightShoulderSet(
                "set_dist_far",
                GlobalConstants.AI_FLV_DIST_FAR_MIN,
                GlobalConstants.AI_FLV_DIST_FAR_PEAK,
                GlobalConstants.AI_FLV_DIST_FAR_MAX);

            //desirability
            FuzzyVariable cl_chase_tnk = m_fmChaseUCellTNK.CreateFuzzyVar("cl_chase_tnk");
            FuzzySetProxy cl_chase_tnk_undesirable = cl_chase_tnk.AddLeftShoulderSet(
                "cl_chase_tnk_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_chase_tnk_desirable = cl_chase_tnk.AddRightShoulderSet(
                "cl_chase_tnk_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmChaseUCellTNK.AddRule(new FzAND(set_tnk_count_low, set_dist_near), cl_chase_tnk_desirable);
            m_fmChaseUCellTNK.AddRule(new FzAND(set_tnk_count_low, set_dist_far), cl_chase_tnk_desirable);

            m_fmChaseUCellTNK.AddRule(new FzAND(set_tnk_count_high, set_dist_near), cl_chase_tnk_undesirable);
            m_fmChaseUCellTNK.AddRule(new FzAND(set_tnk_count_high, set_dist_far), cl_chase_tnk_undesirable);
        }

        private void InitFzMChaseSIL()
        {
            m_fmChaseUCellSIL = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_sil_count = m_fmChaseUCellSIL.CreateFuzzyVar("flv_sil_count");
            FuzzySetProxy set_sil_count_low = flv_sil_count.AddLeftShoulderSet(
                "set_sil_count_low",
                GlobalConstants.AI_FLV_OCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_OCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_OCOUNT_LOW_MAX);
            FuzzySetProxy set_sil_count_high = flv_sil_count.AddRightShoulderSet(
                "set_sil_count_high",
                GlobalConstants.AI_FLV_OCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_OCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_OCOUNT_HIGH_MAX);

            FuzzyVariable flv_dist = m_fmChaseUCellSIL.CreateFuzzyVar("flv_dist");
            FuzzySetProxy set_dist_near = flv_dist.AddLeftShoulderSet(
                "set_dist_near",
                GlobalConstants.AI_FLV_DIST_NEAR_MIN,
                GlobalConstants.AI_FLV_DIST_NEAR_PEAK,
                GlobalConstants.AI_FLV_DIST_NEAR_MAX);
            FuzzySetProxy set_dist_far = flv_dist.AddRightShoulderSet(
                "set_dist_far",
                GlobalConstants.AI_FLV_DIST_FAR_MIN,
                GlobalConstants.AI_FLV_DIST_FAR_PEAK,
                GlobalConstants.AI_FLV_DIST_FAR_MAX);

            //desirability
            FuzzyVariable cl_chase_sil = m_fmChaseUCellSIL.CreateFuzzyVar("cl_chase_sil");
            FuzzySetProxy cl_chase_sil_undesirable = cl_chase_sil.AddLeftShoulderSet(
                "cl_chase_sil_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_chase_sil_desirable = cl_chase_sil.AddRightShoulderSet(
                "cl_chase_sil_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmChaseUCellSIL.AddRule(new FzAND(set_sil_count_low, set_dist_near), cl_chase_sil_desirable);
            m_fmChaseUCellSIL.AddRule(new FzAND(set_sil_count_low, set_dist_far), cl_chase_sil_desirable);

            m_fmChaseUCellSIL.AddRule(new FzAND(set_sil_count_high, set_dist_near), cl_chase_sil_undesirable);
            m_fmChaseUCellSIL.AddRule(new FzAND(set_sil_count_high, set_dist_far), cl_chase_sil_undesirable);
        }

        private void InitFzMDivideANY()
        {
            m_fmDivCellANY = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_underattack = m_fmDivCellANY.CreateFuzzyVar("flv_underattack");
            FuzzySetProxy set_underattack_yes = flv_underattack.AddSingletonSet(
                "set_underattack_yes",
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_PEAK,
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);

            FuzzyVariable flv_atckenemy_power = m_fmDivCellANY.CreateFuzzyVar("flv_atckenemy_power");
            FuzzySetProxy set_atckenemy_power_weak = flv_atckenemy_power.AddLeftShoulderSet(
                "set_atckenemy_power_weak",
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MAX);
            FuzzySetProxy set_atckenemy_power_strong = flv_atckenemy_power.AddRightShoulderSet(
                "set_atckenemy_power_strong",
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MAX);

            FuzzyVariable flv_myclust_power = m_fmDivCellANY.CreateFuzzyVar("flv_myclust_power");
            FuzzySetProxy set_myclust_power_weak = flv_myclust_power.AddLeftShoulderSet(
                "set_myclust_power_weak",
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_WEAK_MAX);
            FuzzySetProxy set_myclust_power_strong = flv_myclust_power.AddRightShoulderSet(
                "set_myclust_power_strong",
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MIN,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_PEAK,
                GlobalConstants.AI_FLV_CLUST_POWER_STRONG_MAX);

            //desirability
            FuzzyVariable cl_divide_any = m_fmDivCellANY.CreateFuzzyVar("cl_divide_any");
            FuzzySetProxy cl_divide_any_undesirable = cl_divide_any.AddLeftShoulderSet(
                "cl_divide_any_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_divide_any_desirable = cl_divide_any.AddRightShoulderSet(
                "cl_divide_any_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmDivCellANY.AddRule(new FzAND(set_underattack_yes, new FzVERY(set_atckenemy_power_strong), new FzVERY(set_myclust_power_weak)), cl_divide_any_desirable);
            m_fmDivCellANY.AddRule(new FzAND(set_underattack_yes, new FzVERY(set_atckenemy_power_strong), new FzVERY(set_myclust_power_strong)), cl_divide_any_undesirable);
            m_fmDivCellANY.AddRule(new FzAND(set_underattack_yes, new FzVERY(set_atckenemy_power_weak), new FzVERY(set_myclust_power_weak)), cl_divide_any_undesirable);
            m_fmDivCellANY.AddRule(new FzAND(set_underattack_yes, new FzVERY(set_atckenemy_power_weak), new FzVERY(set_myclust_power_strong)), cl_divide_any_undesirable);

            m_fmDivCellANY.AddRule(new FzAND(new FzNOT(set_underattack_yes), new FzVERY(set_atckenemy_power_strong), new FzVERY(set_myclust_power_weak)), cl_divide_any_undesirable);
            m_fmDivCellANY.AddRule(new FzAND(new FzNOT(set_underattack_yes), new FzVERY(set_atckenemy_power_strong), new FzVERY(set_myclust_power_strong)), cl_divide_any_undesirable);
            m_fmDivCellANY.AddRule(new FzAND(new FzNOT(set_underattack_yes), new FzVERY(set_atckenemy_power_weak), new FzVERY(set_myclust_power_weak)), cl_divide_any_undesirable);
            m_fmDivCellANY.AddRule(new FzAND(new FzNOT(set_underattack_yes), new FzVERY(set_atckenemy_power_weak), new FzVERY(set_myclust_power_strong)), cl_divide_any_undesirable);
        }

        private void InitFzMDivideRBC()
        {
            m_fmDivCellRBC = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_rbc_count = m_fmDivCellRBC.CreateFuzzyVar("flv_rbc_count");
            FuzzySetProxy set_rbc_count_low = flv_rbc_count.AddLeftShoulderSet(
                "set_rbc_count_low",
                GlobalConstants.AI_FLV_SCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_SCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_LOW_MAX);
            FuzzySetProxy set_rbc_count_high = flv_rbc_count.AddRightShoulderSet(
                "set_rbc_count_high",
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MAX);

            FuzzyVariable flv_rbcdivcount_iszero = m_fmDivCellRBC.CreateFuzzyVar("flv_rbcdivcount_iszero");
            FuzzySetProxy set_rbcdivcount_iszero_true = flv_rbcdivcount_iszero.AddSingletonSet(
                "set_rbcdivcount_iszero_true",
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);

            //desirability
            FuzzyVariable cl_divide_rbc = m_fmDivCellRBC.CreateFuzzyVar("cl_divide_rbc");
            FuzzySetProxy cl_divide_rbc_undesirable = cl_divide_rbc.AddLeftShoulderSet(
                "cl_divide_rbc_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_divide_rbc_desirable = cl_divide_rbc.AddRightShoulderSet(
                "cl_divide_rbc_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmDivCellRBC.AddRule(new FzAND(new FzNOT(set_rbcdivcount_iszero_true), set_rbc_count_low), cl_divide_rbc_desirable);
            m_fmDivCellRBC.AddRule(new FzAND(new FzNOT(set_rbcdivcount_iszero_true), set_rbc_count_high), cl_divide_rbc_undesirable);

            m_fmDivCellRBC.AddRule(new FzAND(set_rbcdivcount_iszero_true, set_rbc_count_low), cl_divide_rbc_undesirable);
            m_fmDivCellRBC.AddRule(new FzAND(set_rbcdivcount_iszero_true, set_rbc_count_high), cl_divide_rbc_undesirable);
        }

        private void InitFzMDividePLT()
        {
            m_fmDivCellPLT = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_plt_count = m_fmDivCellPLT.CreateFuzzyVar("flv_plt_count");
            FuzzySetProxy set_plt_count_low = flv_plt_count.AddLeftShoulderSet(
                "set_plt_count_low",
                GlobalConstants.AI_FLV_SCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_SCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_LOW_MAX);
            FuzzySetProxy set_plt_count_high = flv_plt_count.AddRightShoulderSet(
                "set_plt_count_high",
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MAX);

            FuzzyVariable flv_pltdivcount_iszero = m_fmDivCellPLT.CreateFuzzyVar("flv_pltdivcount_iszero");
            FuzzySetProxy set_pltdivcount_iszero_true = flv_pltdivcount_iszero.AddSingletonSet(
                "set_pltdivcount_iszero_true",
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);

            //desirability
            FuzzyVariable cl_divide_plt = m_fmDivCellPLT.CreateFuzzyVar("cl_divide_plt");
            FuzzySetProxy cl_divide_plt_undesirable = cl_divide_plt.AddLeftShoulderSet(
                "cl_divide_plt_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_divide_plt_desirable = cl_divide_plt.AddRightShoulderSet(
                "cl_divide_plt_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmDivCellPLT.AddRule(new FzAND(new FzNOT(set_pltdivcount_iszero_true), set_plt_count_low), cl_divide_plt_desirable);
            m_fmDivCellPLT.AddRule(new FzAND(new FzNOT(set_pltdivcount_iszero_true), set_plt_count_high), cl_divide_plt_undesirable);

            m_fmDivCellPLT.AddRule(new FzAND(set_pltdivcount_iszero_true, set_plt_count_low), cl_divide_plt_undesirable);
            m_fmDivCellPLT.AddRule(new FzAND(set_pltdivcount_iszero_true, set_plt_count_high), cl_divide_plt_undesirable);
        }

        private void InitFzMDivideTNK()
        {
            m_fmDivCellTNK = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_tnk_count = m_fmDivCellTNK.CreateFuzzyVar("flv_tnk_count");
            FuzzySetProxy set_tnk_count_low = flv_tnk_count.AddLeftShoulderSet(
                "set_tnk_count_low",
                GlobalConstants.AI_FLV_OCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_OCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_OCOUNT_LOW_MAX);
            FuzzySetProxy set_tnk_count_high = flv_tnk_count.AddRightShoulderSet(
                "set_tnk_count_high",
                GlobalConstants.AI_FLV_OCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_OCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_OCOUNT_HIGH_MAX);

            FuzzyVariable flv_tnkdivcount_iszero = m_fmDivCellTNK.CreateFuzzyVar("flv_tnkdivcount_iszero");
            FuzzySetProxy set_tnkdivcount_iszero_true = flv_tnkdivcount_iszero.AddSingletonSet(
                "set_tnkdivcount_iszero_true",
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);

            //desirability
            FuzzyVariable cl_divide_tnk = m_fmDivCellTNK.CreateFuzzyVar("cl_divide_tnk");
            FuzzySetProxy cl_divide_tnk_undesirable = cl_divide_tnk.AddLeftShoulderSet(
                "cl_divide_tnk_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_divide_tnk_desirable = cl_divide_tnk.AddRightShoulderSet(
                "cl_divide_tnk_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmDivCellTNK.AddRule(new FzAND(new FzNOT(set_tnkdivcount_iszero_true), set_tnk_count_low), cl_divide_tnk_desirable);
            m_fmDivCellTNK.AddRule(new FzAND(new FzNOT(set_tnkdivcount_iszero_true), set_tnk_count_high), cl_divide_tnk_undesirable);

            m_fmDivCellTNK.AddRule(new FzAND(set_tnkdivcount_iszero_true, set_tnk_count_low), cl_divide_tnk_undesirable);
            m_fmDivCellTNK.AddRule(new FzAND(set_tnkdivcount_iszero_true, set_tnk_count_high), cl_divide_tnk_undesirable);
        }

        private void InitFzMDivideSIL()
        {
            m_fmDivCellSIL = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_sil_count = m_fmDivCellSIL.CreateFuzzyVar("flv_sil_count");
            FuzzySetProxy set_sil_count_low = flv_sil_count.AddLeftShoulderSet(
                "set_sil_count_low",
                GlobalConstants.AI_FLV_OCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_OCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_OCOUNT_LOW_MAX);
            FuzzySetProxy set_sil_count_high = flv_sil_count.AddRightShoulderSet(
                "set_sil_count_high",
                GlobalConstants.AI_FLV_OCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_OCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_OCOUNT_HIGH_MAX);

            FuzzyVariable flv_sildivcount_iszero = m_fmDivCellSIL.CreateFuzzyVar("flv_sildivcount_iszero");
            FuzzySetProxy set_sildivcount_iszero_true = flv_sildivcount_iszero.AddSingletonSet(
                "set_sildivcount_iszero_true",
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);

            //desirability
            FuzzyVariable cl_divide_sil = m_fmDivCellSIL.CreateFuzzyVar("cl_divide_sil");
            FuzzySetProxy cl_divide_sil_undesirable = cl_divide_sil.AddLeftShoulderSet(
                "cl_divide_sil_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy cl_divide_sil_desirable = cl_divide_sil.AddRightShoulderSet(
                "cl_divide_sil_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmDivCellSIL.AddRule(new FzAND(new FzNOT(set_sildivcount_iszero_true), set_sil_count_low), cl_divide_sil_desirable);
            m_fmDivCellSIL.AddRule(new FzAND(new FzNOT(set_sildivcount_iszero_true), set_sil_count_high), cl_divide_sil_undesirable);

            m_fmDivCellSIL.AddRule(new FzAND(set_sildivcount_iszero_true, set_sil_count_low), cl_divide_sil_undesirable);
            m_fmDivCellSIL.AddRule(new FzAND(set_sildivcount_iszero_true, set_sil_count_high), cl_divide_sil_undesirable);
        }

        #endregion

        #region AI

        public void DoAIUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //update state
            UpdateAIDataSet(gameTime);

            m_aiSecSinceLastRequest += gameTime.ElapsedRealTime.TotalSeconds;
            if (m_aiSecSinceLastRequest > GlobalConstants.AI_CLUST_THOUGHT_TIMEOUT_SECS)
            {
                //1. make sure no decision has been made
                ClearReqAIActions();

                //2. do important thought
                UpdateAIMajor(gameTime);

                //3. do minor thought
                if (m_aiAllowThought)
                    UpdateAIMinor(gameTime);
            }
        }

        public void ClearReqAIActions()
        {
            m_aiReqAction = ClusterAIReqestAction.NONE;
            m_virusOverrideReqAction = VirusOverrideRequestAction.NONE;
        }

        /// <summary>
        /// Flips the AI dataset buffer inverting the roles of the frount
        /// and back buffer copies.
        /// </summary>
        public void FlipAIDataSets()
        {
            ClusterAIdataset tempDS = m_aiDataFrount;
            m_aiDataFrount = m_aiDataBack;
            m_aiDataBack = tempDS;
        }

        #region internals

        #region AI major

        /// <summary>
        /// This will always be called, it is ment for urgent responce.
        /// </summary>
        private void UpdateAIMajor(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_scoreRankings.Clear();

            m_scoreRankings[AIMajorDivideCellsAny()] = ClusterAIReqestAction.DIVCELLS_ANY;

            if (m_cluster.stateData.actionState == CellActionState.IDLE)
            {
                m_scoreRankings[AIMajorEvadeAtckEnemy()] = ClusterAIReqestAction.EVADE_ATCK_ENEM;
                m_scoreRankings[AIMajorChaseAtckEnemy()] = ClusterAIReqestAction.BATTLE_ATCK_ENEM;
            }

            //check that the winner is viable
            if (m_scoreRankings.First().Key > 50.0)
            {
                m_aiReqAction = m_scoreRankings.First().Value;
            }
        }

        private double AIMajorEvadeAtckEnemy()
        {
            m_fmEvadeAttackingEnem.Fuzzify("flv_underattack", ((m_cluster.stateData.attnUnderAttack) ? 1.0 : 0.0));
            m_fmEvadeAttackingEnem.Fuzzify("flv_atckenemy_power", AIDataSet.cl_atckingenem_power);
            return m_fmEvadeAttackingEnem.DeFuzzify("cl_evade_enemy");
        }

        private double AIMajorChaseAtckEnemy()
        {
            m_fmChaseAttackingEnem.Fuzzify("flv_underattack", ((m_cluster.stateData.attnUnderAttack) ? 1.0 : 0.0));
            m_fmChaseAttackingEnem.Fuzzify("flv_atckenemy_power", AIDataSet.cl_atckingenem_power);
            m_fmChaseAttackingEnem.Fuzzify("flv_myclust_power", AIDataSet.cl_my_power);
            return m_fmChaseAttackingEnem.DeFuzzify("cl_chase_atckenemy");
        }

        private double AIMajorDivideCellsAny()
        {
            m_fmDivCellANY.Fuzzify("flv_underattack", ((m_cluster.stateData.attnUnderAttack) ? 1.0 : 0.0));
            m_fmDivCellANY.Fuzzify("flv_atckenemy_power", AIDataSet.cl_atckingenem_power);
            m_fmDivCellANY.Fuzzify("flv_myclust_power", AIDataSet.cl_my_power);
            return m_fmDivCellANY.DeFuzzify("cl_divide_any");
        }

        #endregion

        #region AI minor

        /// <summary>
        /// This will only be called if the cluster has been signaled to allow ai thought.
        /// </summary>
        private void UpdateAIMinor(Microsoft.Xna.Framework.GameTime gameTime)
        {
            if (m_aiReqAction == ClusterAIReqestAction.NONE)
            {
                m_scoreRankings.Clear();

                //chase is prefered to cell division - as is small cells to big cells
                m_scoreRankings[AIMinorDivideSIL()] = ClusterAIReqestAction.DIVCELLS_SIL;
                m_scoreRankings[AIMinorChaseSIL()] = ClusterAIReqestAction.CHASE_SIL;
                m_scoreRankings[AIMinorDivideTNK()] = ClusterAIReqestAction.DIVCELLS_TNK;
                m_scoreRankings[AIMinorChaseTNK()] = ClusterAIReqestAction.CHASE_TNK;
                m_scoreRankings[AIMinorDivideRBC()] = ClusterAIReqestAction.DIVCELLS_RBC;
                m_scoreRankings[AIMinorChaseRBC()] = ClusterAIReqestAction.CHASE_RBC;
                m_scoreRankings[AIMinorDividePLT()] = ClusterAIReqestAction.DIVCELLS_PLT;
                m_scoreRankings[AIMinorChasePLT()] = ClusterAIReqestAction.CHASE_PLT;

                m_scoreRankings[AIMinorChaseEnemy()] = ClusterAIReqestAction.BATTLE_CLST_ENEM;

                //check that the prefered action to request is viable
                if (m_scoreRankings.First().Key > 50.0)
                {
                    m_aiReqAction = m_scoreRankings.First().Value;
                }
            }
        }

        private double AIMinorChaseEnemy()
        {
            if (m_cluster.stateData.actionState == CellActionState.IDLE)
            {
                m_fmChaseEnemBattle.Fuzzify("flv_clstenemy_power", AIDataSet.cl_clst_enem_power);
                m_fmChaseEnemBattle.Fuzzify("flv_myclust_power", AIDataSet.cl_my_power);
                m_fmChaseEnemBattle.Fuzzify("flv_dist", AIDataSet.cl_dist_clst_enem);
                return m_fmChaseEnemBattle.DeFuzzify("cl_chase_clstenemy");
            }
            else
                return 0.0;
        }

        private double AIMinorChaseRBC()
        {
            if (m_cluster.stateData.actionState == CellActionState.IDLE)
            {
                m_fmChaseUCellRBC.Fuzzify("flv_rbc_count", m_cluster.stateData.numRBCs);
                m_fmChaseUCellRBC.Fuzzify("flv_dist", AIDataSet.cl_dist_clst_rbc);
                return m_fmChaseUCellRBC.DeFuzzify("cl_chase_rbc");
            }
            else
                return 0.0;
        }

        private double AIMinorChasePLT()
        {
            if (m_cluster.stateData.actionState == CellActionState.IDLE)
            {
                m_fmChaseUCellPLT.Fuzzify("flv_plt_count", m_cluster.stateData.numPlatelets);
                m_fmChaseUCellPLT.Fuzzify("flv_dist", AIDataSet.cl_dist_clst_plt);
                return m_fmChaseUCellPLT.DeFuzzify("cl_chase_plt");
            }
            else
                return 0.0;
        }

        private double AIMinorChaseTNK()
        {
            if (m_cluster.stateData.actionState == CellActionState.IDLE)
            {
                m_fmChaseUCellTNK.Fuzzify("flv_tnk_count", m_cluster.stateData.numTanks);
                m_fmChaseUCellTNK.Fuzzify("flv_dist", AIDataSet.cl_dist_clst_tnk);
                return m_fmChaseUCellTNK.DeFuzzify("cl_chase_tnk");
            }
            else
                return 0.0;
        }

        private double AIMinorChaseSIL()
        {
            if (m_cluster.stateData.actionState == CellActionState.IDLE)
            {
                m_fmChaseUCellSIL.Fuzzify("flv_sil_count", m_cluster.stateData.numSilos);
                m_fmChaseUCellSIL.Fuzzify("flv_dist", AIDataSet.cl_dist_clst_sil);
                return m_fmChaseUCellSIL.DeFuzzify("cl_chase_sil");
            }
            else
                return 0.0;
        }


        private double AIMinorDivideRBC()
        {
            m_fmDivCellRBC.Fuzzify("flv_rbc_count", m_cluster.stateData.numRBCs);
            m_fmDivCellRBC.Fuzzify("flv_rbcdivcount_iszero", AIDataSet.cl_rbc_divcount);
            return m_fmDivCellRBC.DeFuzzify("cl_divide_rbc");
        }

        private double AIMinorDividePLT()
        {
            m_fmDivCellPLT.Fuzzify("flv_plt_count", m_cluster.stateData.numPlatelets);
            m_fmDivCellPLT.Fuzzify("flv_pltdivcount_iszero", AIDataSet.cl_plt_divcount);
            return m_fmDivCellPLT.DeFuzzify("cl_divide_plt");
        }

        private double AIMinorDivideTNK()
        {
            m_fmDivCellTNK.Fuzzify("flv_tnk_count", m_cluster.stateData.numTanks);
            m_fmDivCellTNK.Fuzzify("flv_tnkdivcount_iszero", AIDataSet.cl_tnk_divcount);
            return m_fmDivCellTNK.DeFuzzify("cl_divide_tnk");
        }

        private double AIMinorDivideSIL()
        {
            m_fmDivCellSIL.Fuzzify("flv_sil_count", m_cluster.stateData.numSilos);
            m_fmDivCellSIL.Fuzzify("flv_sildivcount_iszero", AIDataSet.cl_sil_divcount);
            return m_fmDivCellSIL.DeFuzzify("cl_divide_sil");
        }

        #endregion

        /// <summary>
        /// This updates the AI backbuffer dataset with data from the frount buffer
        /// (to make accurate copy) and with the latest scene state data (singular 
        /// itteration updates for performance sake).
        /// </summary>
        private void UpdateAIDataSet(Microsoft.Xna.Framework.GameTime gameTime)
        {
            m_aiSecSinceLastAIUpdate += gameTime.ElapsedRealTime.TotalSeconds;

            #region copy performance wise frount buffer data to back buffer
            m_aiDataBack.cl_atckingenem_power = double.MaxValue;
            m_aiDataBack.cl_clst_enem_power = m_aiDataFrount.cl_clst_enem_power;

            m_aiDataBack.cl_dist_atck_enem = m_aiDataFrount.cl_dist_atck_enem;
            m_aiDataBack.cl_dist_clst_enem = m_aiDataFrount.cl_dist_clst_enem;
            m_aiDataBack.cl_dist_clst_friend = m_aiDataFrount.cl_dist_clst_friend;

            m_aiDataBack.cl_re_clst_enem = m_aiDataFrount.cl_re_clst_enem;
            m_aiDataBack.cl_re_clst_friend = m_aiDataFrount.cl_re_clst_friend;
            m_aiDataBack.cl_dist_clst_rbc = m_aiDataFrount.cl_dist_clst_rbc;
            m_aiDataBack.cl_dist_clst_plt = m_aiDataFrount.cl_dist_clst_plt;
            m_aiDataBack.cl_dist_clst_tnk = m_aiDataFrount.cl_dist_clst_tnk;
            m_aiDataBack.cl_dist_clst_sil = m_aiDataFrount.cl_dist_clst_sil;

            m_aiDataBack.cl_re_clst_rbc = (m_aiDataFrount.cl_re_clst_rbc != null) ?
                ((m_aiDataFrount.cl_re_clst_rbc.Active) ? m_aiDataFrount.cl_re_clst_rbc : null) : null;
            m_aiDataBack.cl_re_clst_plt = (m_aiDataFrount.cl_re_clst_plt != null) ?
                ((m_aiDataFrount.cl_re_clst_plt.Active) ? m_aiDataFrount.cl_re_clst_plt : null) : null;
            m_aiDataBack.cl_re_clst_tnk = (m_aiDataFrount.cl_re_clst_tnk != null) ?
                ((m_aiDataFrount.cl_re_clst_tnk.Active) ? m_aiDataFrount.cl_re_clst_tnk : null) : null;
            m_aiDataBack.cl_re_clst_sil = (m_aiDataFrount.cl_re_clst_sil != null) ?
                ((m_aiDataFrount.cl_re_clst_sil.Active) ? m_aiDataFrount.cl_re_clst_sil : null) : null;
            #endregion

            #region do lattest updates

            //div counts
            m_aiDataBack.cl_rbc_divcount = (m_cluster.stateData.numRBCs > 0) ?
                ((double)m_cluster.stateData.attrNutrientStore / (double)GlobalConstants.RBC_threshNToDivide) : 0.0;

            m_aiDataBack.cl_plt_divcount = (m_cluster.stateData.numPlatelets > 0) ?
                ((double)m_cluster.stateData.attrNutrientStore / (double)GlobalConstants.PLATELET_threshNToDivide) : 0.0;

            m_aiDataBack.cl_tnk_divcount = (m_cluster.stateData.numTanks > 0) ?
                ((double)m_cluster.stateData.attrNutrientStore / (double)GlobalConstants.TANK_threshNToDivide) : 0.0;

            m_aiDataBack.cl_sil_divcount = (m_cluster.stateData.numSilos > 0) ?
                ((double)m_cluster.stateData.attrNutrientStore / (double)GlobalConstants.SILO_threshNToDivide) : 0.0;

            if (m_aiSecSinceLastAIUpdate > GlobalConstants.AI_CLUSTER_UP_DELAY_SECS)
            {
                m_aiSecSinceLastAIUpdate = 0.0;
                double tempLength = double.MaxValue;

                if (m_aiUpFocusOnUCells)
                {
                    #region scan ucells
                    foreach (Cells.UninfectedCell uCell in m_cluster.stateData.biophageScn.UninfectCellsList)
                    {
                        if (!uCell.Active)
                            continue;

                        tempLength = (double)Microsoft.Xna.Framework.Vector3.Distance(uCell.Position, m_cluster.Position);
                        switch (uCell.StaticData.staticCellType)
                        {
                            case Cells.CellTypeEnum.RED_BLOOD_CELL:
                                if (m_aiDataBack.cl_re_clst_rbc == null)
                                {
                                    m_aiDataBack.cl_re_clst_rbc = uCell;
                                    m_aiDataBack.cl_dist_clst_rbc = tempLength;
                                }
                                else if (tempLength < m_aiDataBack.cl_dist_clst_rbc)
                                {
                                    m_aiDataBack.cl_re_clst_rbc = uCell;
                                    m_aiDataBack.cl_dist_clst_rbc = tempLength;
                                }
                                break;
                            case Cells.CellTypeEnum.PLATELET:
                                if (m_aiDataBack.cl_re_clst_plt == null)
                                {
                                    m_aiDataBack.cl_re_clst_plt = uCell;
                                    m_aiDataBack.cl_dist_clst_plt = tempLength;
                                }
                                else if (tempLength < m_aiDataBack.cl_dist_clst_plt)
                                {
                                    m_aiDataBack.cl_re_clst_plt = uCell;
                                    m_aiDataBack.cl_dist_clst_plt = tempLength;
                                }
                                break;
                            case Cells.CellTypeEnum.BIG_CELL_TANK:
                                if (m_aiDataBack.cl_re_clst_tnk == null)
                                {
                                    m_aiDataBack.cl_re_clst_tnk = uCell;
                                    m_aiDataBack.cl_dist_clst_tnk = tempLength;
                                }
                                else if (tempLength < m_aiDataBack.cl_dist_clst_tnk)
                                {
                                    m_aiDataBack.cl_re_clst_tnk = uCell;
                                    m_aiDataBack.cl_dist_clst_tnk = tempLength;
                                }
                                break;
                            case Cells.CellTypeEnum.BIG_CELL_SILO:
                                if (m_aiDataBack.cl_re_clst_sil == null)
                                {
                                    m_aiDataBack.cl_re_clst_sil = uCell;
                                    m_aiDataBack.cl_dist_clst_sil = tempLength;
                                }
                                else if (tempLength < m_aiDataBack.cl_dist_clst_sil)
                                {
                                    m_aiDataBack.cl_re_clst_sil = uCell;
                                    m_aiDataBack.cl_dist_clst_sil = tempLength;
                                }
                                break;
                        }
                    }
                    #endregion
                }
                else
                {
                    //scan clusters
                    #region scan clusters
                    foreach (CellCluster cluster in m_cluster.stateData.biophageScn.CellClustersList)
                    {
                        if (!cluster.Active)
                            continue;

                        tempLength = (double)Microsoft.Xna.Framework.Vector3.Distance(cluster.Position, m_cluster.Position);
                        if (cluster.stateData.virusOwnerId != m_cluster.stateData.virusOwnerId)
                        {
                            //enemy
                            if (m_aiDataBack.cl_re_clst_enem == null)
                            {
                                m_aiDataBack.cl_re_clst_enem = cluster;
                                m_aiDataBack.cl_dist_clst_enem = tempLength;
                            }
                            else if (tempLength < m_aiDataBack.cl_dist_clst_enem)
                            {
                                m_aiDataBack.cl_re_clst_enem = cluster;
                                m_aiDataBack.cl_dist_clst_enem = tempLength;
                            }
                        }
                        else
                        {
                            //friend
                            if (m_aiDataBack.cl_re_clst_friend == null)
                            {
                                m_aiDataBack.cl_re_clst_friend = cluster;
                                m_aiDataBack.cl_dist_clst_friend = tempLength;
                            }
                            else if (tempLength < m_aiDataBack.cl_dist_clst_friend)
                            {
                                m_aiDataBack.cl_re_clst_friend = cluster;
                                m_aiDataBack.cl_dist_clst_friend = tempLength;
                            }
                        }
                    }
                    #endregion
                }

                m_aiUpFocusOnUCells = !m_aiUpFocusOnUCells;
            }

            if (m_cluster.stateData.attnAttackingEnemy != null)
                m_aiDataBack.cl_dist_atck_enem = Microsoft.Xna.Framework.Vector3.Distance(
                    m_cluster.Position,
                    m_cluster.stateData.attnAttackingEnemy.Position);

            //power vals
            m_aiDataBack.cl_my_power = (double)m_cluster.stateData.maxBattleOffence / (double)GlobalConstants.RBC_threshMaxBattleOffence;

            if (m_cluster.stateData.attnUnderAttack && (m_cluster.stateData.attnAttackingEnemy != null) &&
                (m_cluster.stateData.attnAttackingEnemy.stateData.maxBattleOffence != 0.0))
                m_aiDataBack.cl_atckingenem_power = (double)m_cluster.stateData.attnAttackingEnemy.stateData.maxBattleOffence /
                    (double)GlobalConstants.RBC_threshMaxBattleOffence;

            if (m_aiDataBack.cl_re_clst_enem != null)
                m_aiDataBack.cl_clst_enem_power = (double)m_aiDataBack.cl_re_clst_enem.stateData.maxBattleOffence / (double)GlobalConstants.RBC_threshMaxBattleOffence;

            #endregion
        }

        #endregion

        #endregion

        #endregion
    }
}
