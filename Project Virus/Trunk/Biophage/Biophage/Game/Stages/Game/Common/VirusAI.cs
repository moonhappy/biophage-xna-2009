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
    public class VirusAI
    {
        #region fields

        private DebugManager m_debugMgr;
        private Virus m_virus;

        protected VirusAIDataSet m_aiDataSet = new VirusAIDataSet();

        public double aiSecSinceLastReq = 0.0;

        #region fuzzy modules

        private FuzzyModule m_fmSplitCluster;
        private FuzzyModule m_fmCombineClusters;
        private FuzzyModule m_fmHybridCells;
        private FuzzyModule m_fmUrgentHybridCells;
        private FuzzyModule m_fmKamikaze;

        private SortedList<double, VirusOverrideRequestAction> m_scoreRankings;

        #endregion

        #endregion

        #region properties

        public VirusAIDataSet AIDataSet
        {
            get { return m_aiDataSet; }
        }

        #endregion

        #region methods

        #region construction

        public VirusAI(DebugManager debugMgr, Virus virus)
        {
            m_debugMgr = debugMgr;
            m_virus = virus;
            m_scoreRankings = new SortedList<double, VirusOverrideRequestAction>(5, new DoubleReverse());

            aiSecSinceLastReq = 0.0;

            InitFzMSplitCluster();
            InitFzMCombineCluster();
            InitFzMHybridCells();
            InitFzMUrgentHybridCells();
            InitFzMKamikaze();
        }

        private void InitFzMSplitCluster()
        {
            m_fmSplitCluster = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_clust_count = m_fmSplitCluster.CreateFuzzyVar("flv_clust_count");
            FuzzySetProxy set_clust_count_low = flv_clust_count.AddLeftShoulderSet(
                "set_clust_count_low",
                GlobalConstants.AI_FLV_SCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_SCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_LOW_MAX);
            FuzzySetProxy set_clust_count_high = flv_clust_count.AddRightShoulderSet(
                "set_clust_count_high",
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MAX);

            FuzzyVariable flv_cl_cellcount_iszero = m_fmSplitCluster.CreateFuzzyVar("flv_cl_cellcount_iszero");
            FuzzySetProxy set_cl_cellcount_iszero_true = flv_cl_cellcount_iszero.AddSingletonSet(
                "set_cl_cellcount_iszero_true",
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);
        

            //desirability
            FuzzyVariable vir_cl_split = m_fmSplitCluster.CreateFuzzyVar("vir_cl_split");
            FuzzySetProxy vir_cl_split_undesirable = vir_cl_split.AddLeftShoulderSet(
                "vir_cl_split_undesirable", 
                GlobalConstants.AI_CL_UNDESIRABLE_MIN, 
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK, 
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy vir_cl_split_desirable = vir_cl_split.AddRightShoulderSet(
                "vir_cl_split_desirable", 
                GlobalConstants.AI_CL_DESIRABLE_MIN, 
                GlobalConstants.AI_CL_DESIRABLE_PEAK, 
                GlobalConstants.AI_CL_DESIRABLE_MAX);
        
            //Rules
            m_fmSplitCluster.AddRule(new FzAND(set_clust_count_low, new FzNOT(set_cl_cellcount_iszero_true)), vir_cl_split_desirable);
            m_fmSplitCluster.AddRule(new FzAND(set_clust_count_low, set_cl_cellcount_iszero_true), vir_cl_split_undesirable);

            m_fmSplitCluster.AddRule(new FzAND(set_clust_count_high, new FzNOT(set_cl_cellcount_iszero_true)), vir_cl_split_undesirable);
            m_fmSplitCluster.AddRule(new FzAND(set_clust_count_high, set_cl_cellcount_iszero_true), vir_cl_split_undesirable);
        }

        private void InitFzMCombineCluster()
        {
            m_fmCombineClusters = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_clust_count = m_fmCombineClusters.CreateFuzzyVar("flv_clust_count");
            FuzzySetProxy set_clust_count_low = flv_clust_count.AddLeftShoulderSet(
                "set_clust_count_low",
                GlobalConstants.AI_FLV_SCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_SCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_LOW_MAX);
            FuzzySetProxy set_clust_count_high = flv_clust_count.AddRightShoulderSet(
                "set_clust_count_high",
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MAX);

            FuzzyVariable flv_dist = m_fmCombineClusters.CreateFuzzyVar("flv_dist");
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
            FuzzyVariable vir_cl_combine = m_fmCombineClusters.CreateFuzzyVar("vir_cl_combine");
            FuzzySetProxy vir_cl_combine_undesirable = vir_cl_combine.AddLeftShoulderSet(
                "vir_cl_combine_undesirable", 
                GlobalConstants.AI_CL_UNDESIRABLE_MIN, 
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK, 
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy vir_cl_combine_desirable = vir_cl_combine.AddRightShoulderSet(
                "vir_cl_combine_desirable", 
                GlobalConstants.AI_CL_DESIRABLE_MIN, 
                GlobalConstants.AI_CL_DESIRABLE_PEAK, 
                GlobalConstants.AI_CL_DESIRABLE_MAX);
        
            //Rules
            m_fmCombineClusters.AddRule(new FzAND(new FzVERY(set_clust_count_low), new FzNOT(set_dist_near)), vir_cl_combine_desirable);
            m_fmCombineClusters.AddRule(new FzAND(new FzVERY(set_clust_count_low), new FzNOT(set_dist_far)), vir_cl_combine_undesirable);

            m_fmCombineClusters.AddRule(new FzAND(new FzVERY(set_clust_count_high), new FzNOT(set_dist_near)), vir_cl_combine_undesirable);
            m_fmCombineClusters.AddRule(new FzAND(new FzVERY(set_clust_count_high), new FzNOT(set_dist_far)), vir_cl_combine_undesirable);
        }

        private void InitFzMHybridCells()
        {
            m_fmHybridCells = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_hyb_count = m_fmHybridCells.CreateFuzzyVar("flv_hyb_count");
            FuzzySetProxy set_hyb_count_low = flv_hyb_count.AddLeftShoulderSet(
                "set_hyb_count_low",
                GlobalConstants.AI_FLV_SCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_SCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_LOW_MAX);
            FuzzySetProxy set_hyb_count_high = flv_hyb_count.AddRightShoulderSet(
                "set_hyb_count_high",
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MAX);

            FuzzyVariable flv_canhybrid = m_fmHybridCells.CreateFuzzyVar("flv_canhybrid");
            FuzzySetProxy set_canhybrid_true = flv_canhybrid.AddSingletonSet(
                "set_canhybrid_true",
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_PEAK,
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);


            //desirability
            FuzzyVariable vir_cl_hybrid = m_fmHybridCells.CreateFuzzyVar("vir_cl_hybrid");
            FuzzySetProxy vir_cl_hybrid_undesirable = vir_cl_hybrid.AddLeftShoulderSet(
                "vir_cl_hybrid_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy vir_cl_hybrid_desirable = vir_cl_hybrid.AddRightShoulderSet(
                "vir_cl_hybrid_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmHybridCells.AddRule(new FzAND(new FzVERY(set_hyb_count_low), set_canhybrid_true), vir_cl_hybrid_desirable);
            m_fmHybridCells.AddRule(new FzAND(new FzVERY(set_hyb_count_low), new FzNOT(set_canhybrid_true)), vir_cl_hybrid_undesirable);

            m_fmHybridCells.AddRule(new FzAND(new FzVERY(set_hyb_count_high), set_canhybrid_true), vir_cl_hybrid_undesirable);
            m_fmHybridCells.AddRule(new FzAND(new FzVERY(set_hyb_count_high), new FzNOT(set_canhybrid_true)), vir_cl_hybrid_undesirable);
        }

        private void InitFzMUrgentHybridCells()
        {
            m_fmUrgentHybridCells = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_hyb_count = m_fmUrgentHybridCells.CreateFuzzyVar("flv_hyb_count");
            FuzzySetProxy set_hyb_count_low = flv_hyb_count.AddLeftShoulderSet(
                "set_hyb_count_low",
                GlobalConstants.AI_FLV_SCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_SCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_LOW_MAX);
            FuzzySetProxy set_hyb_count_high = flv_hyb_count.AddRightShoulderSet(
                "set_hyb_count_high",
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MAX);

            FuzzyVariable flv_canhybrid = m_fmUrgentHybridCells.CreateFuzzyVar("flv_canhybrid");
            FuzzySetProxy set_canhybrid_true = flv_canhybrid.AddSingletonSet(
                "set_canhybrid_true",
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_PEAK,
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);

            FuzzyVariable flv_medicwarn = m_fmUrgentHybridCells.CreateFuzzyVar("flv_medicwarn");
            FuzzySetProxy set_medicwarn_true = flv_medicwarn.AddSingletonSet(
                "set_medicwarn_true",
                GlobalConstants.AI_FLV_ISTRUE_YES_MIN,
                GlobalConstants.AI_FLV_ISTRUE_YES_PEAK,
                GlobalConstants.AI_FLV_ISTRUE_YES_MAX);


            //desirability
            FuzzyVariable vir_cl_hybrid = m_fmUrgentHybridCells.CreateFuzzyVar("vir_cl_hybrid");
            FuzzySetProxy vir_cl_hybrid_undesirable = vir_cl_hybrid.AddLeftShoulderSet(
                "vir_cl_hybrid_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy vir_cl_hybrid_desirable = vir_cl_hybrid.AddRightShoulderSet(
                "vir_cl_hybrid_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmUrgentHybridCells.AddRule(new FzAND(set_hyb_count_low, set_medicwarn_true, set_canhybrid_true), vir_cl_hybrid_desirable);
            m_fmUrgentHybridCells.AddRule(new FzAND(set_hyb_count_low, set_medicwarn_true, new FzNOT(set_canhybrid_true)), vir_cl_hybrid_undesirable);
            m_fmUrgentHybridCells.AddRule(new FzAND(set_hyb_count_low, new FzNOT(set_medicwarn_true), set_canhybrid_true), vir_cl_hybrid_undesirable);
            m_fmUrgentHybridCells.AddRule(new FzAND(set_hyb_count_low, new FzNOT(set_medicwarn_true), new FzNOT(set_canhybrid_true)), vir_cl_hybrid_undesirable);

            m_fmUrgentHybridCells.AddRule(new FzAND(set_hyb_count_high, set_medicwarn_true, set_canhybrid_true), vir_cl_hybrid_undesirable);
            m_fmUrgentHybridCells.AddRule(new FzAND(set_hyb_count_high, set_medicwarn_true, new FzNOT(set_canhybrid_true)), vir_cl_hybrid_undesirable);
            m_fmUrgentHybridCells.AddRule(new FzAND(set_hyb_count_high, new FzNOT(set_medicwarn_true), set_canhybrid_true), vir_cl_hybrid_undesirable);
            m_fmUrgentHybridCells.AddRule(new FzAND(set_hyb_count_high, new FzNOT(set_medicwarn_true), new FzNOT(set_canhybrid_true)), vir_cl_hybrid_undesirable);
        }

        private void InitFzMKamikaze()
        {
            m_fmKamikaze = new FuzzyModule(m_debugMgr);

            //FLVs
            FuzzyVariable flv_clust_count = m_fmKamikaze.CreateFuzzyVar("flv_clust_count");
            FuzzySetProxy set_clust_count_low = flv_clust_count.AddLeftShoulderSet(
                "set_clust_count_low",
                GlobalConstants.AI_FLV_SCOUNT_LOW_MIN,
                GlobalConstants.AI_FLV_SCOUNT_LOW_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_LOW_MAX);
            FuzzySetProxy set_clust_count_high = flv_clust_count.AddRightShoulderSet(
                "set_clust_count_high",
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MIN,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_PEAK,
                GlobalConstants.AI_FLV_SCOUNT_HIGH_MAX);

            FuzzyVariable flv_dist = m_fmKamikaze.CreateFuzzyVar("flv_dist");
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
            FuzzyVariable vir_cl_kamikaze = m_fmKamikaze.CreateFuzzyVar("vir_cl_kamikaze");
            FuzzySetProxy vir_cl_kamikaze_undesirable = vir_cl_kamikaze.AddLeftShoulderSet(
                "vir_cl_kamikaze_undesirable",
                GlobalConstants.AI_CL_UNDESIRABLE_MIN,
                GlobalConstants.AI_CL_UNDESIRABLE_PEAK,
                GlobalConstants.AI_CL_UNDESIRABLE_MAX);
            FuzzySetProxy vir_cl_kamikaze_desirable = vir_cl_kamikaze.AddRightShoulderSet(
                "vir_cl_kamikaze_desirable",
                GlobalConstants.AI_CL_DESIRABLE_MIN,
                GlobalConstants.AI_CL_DESIRABLE_PEAK,
                GlobalConstants.AI_CL_DESIRABLE_MAX);

            //Rules
            m_fmKamikaze.AddRule(new FzAND(set_clust_count_high, new FzFAIRLY(set_dist_near)), vir_cl_kamikaze_desirable);
            m_fmKamikaze.AddRule(new FzAND(set_clust_count_high, new FzFAIRLY(set_dist_far)), vir_cl_kamikaze_undesirable);

            m_fmKamikaze.AddRule(new FzAND(set_clust_count_low, new FzFAIRLY(set_dist_near)), vir_cl_kamikaze_undesirable);
            m_fmKamikaze.AddRule(new FzAND(set_clust_count_low, new FzFAIRLY(set_dist_far)), vir_cl_kamikaze_undesirable);
        }

        #endregion

        #region AI

        public void AIUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            // 1. update ai dataset
            AIUpdateDataSet();

            aiSecSinceLastReq += gameTime.ElapsedRealTime.TotalSeconds;
            if (aiSecSinceLastReq > GlobalConstants.AI_VIR_THOUGHT_TIMEOUT_SECS)
            {
                //2. update 'thought'
                UpdateThought();
            }
        }

        #region internals

        /// <summary>
        /// This updates the AI backbuffer dataset with data from the frount buffer
        /// (to make accurate copy) and with the latest scene state data (singular 
        /// itteration updates for performance sake).
        /// </summary>
        private void AIUpdateDataSet()
        {
            m_aiDataSet.vir_total_numclusters = (double)m_virus.virusStateData.clusters.Count;
            m_aiDataSet.vir_total_numhybs = 0.0;
            foreach (CellCluster cluster in m_virus.virusStateData.clusters)
            {
                m_aiDataSet.vir_total_numhybs += cluster.stateData.numSmallHybrids +
                    cluster.stateData.numMediumHybrids + cluster.stateData.numBigHybrids;
            }

            if (m_virus.m_biophageScn.GetHUD.showMedicationAlert)
                m_aiDataSet.vir_medicalert = 1.0;
            else
                m_aiDataSet.vir_medicalert = 0.0;
        }

        private void UpdateThought()
        {
            foreach (CellCluster cluster in m_virus.virusStateData.clusters)
            {
                m_scoreRankings.Clear();

                m_scoreRankings[AISplitCluster(cluster)] = VirusOverrideRequestAction.VIR_CL_SPLIT;
                m_scoreRankings[AIHybridCells(cluster)] = VirusOverrideRequestAction.VIR_CL_HYBRID;
                m_scoreRankings[AIUrgentHybridCells(cluster)] = VirusOverrideRequestAction.VIR_CL_HYBRID;  

                if (cluster.stateData.actionState == CellActionState.IDLE)
                {
                    m_scoreRankings[AICombineCluster(cluster)] = VirusOverrideRequestAction.VIR_CL_COMBINE;
                    m_scoreRankings[AIKamikaze(cluster)] = VirusOverrideRequestAction.VIR_CL_KAMIKAZE;
                }

                //check that the winner is viable
                if (m_scoreRankings.First().Key > 50.0)
                {
                    cluster.m_clusterAI.m_virusOverrideReqAction = m_scoreRankings.First().Value;
                }
            }
        }

        private double AISplitCluster(CellCluster cl)
        {
            m_fmSplitCluster.Fuzzify("flv_clust_count", m_aiDataSet.vir_total_numclusters);
            m_fmSplitCluster.Fuzzify("flv_cl_cellcount_iszero", cl.stateData.numCellsTotal);

            //inflate value
            m_aiDataSet.vir_total_numclusters += 1.0;

            return m_fmSplitCluster.DeFuzzify("vir_cl_split");
        }

        private double AICombineCluster(CellCluster cl)
        {
            m_fmCombineClusters.Fuzzify("flv_clust_count", m_aiDataSet.vir_total_numclusters);
            m_fmCombineClusters.Fuzzify("flv_dist", cl.m_clusterAI.AIDataSet.cl_dist_clst_friend);

            //inflate
            m_aiDataSet.vir_total_numclusters += 1.0;

            return m_fmCombineClusters.DeFuzzify("vir_cl_combine");
        }

        private double AIHybridCells(CellCluster cl)
        {
            m_fmHybridCells.Fuzzify("flv_hyb_count", m_aiDataSet.vir_total_numhybs);
            m_fmHybridCells.Fuzzify("flv_canhybrid", ((cl.CanHybreed) ? 1.0 : 0.0));

            //inflate
            m_aiDataSet.vir_total_numhybs += 1.0;

            return m_fmHybridCells.DeFuzzify("vir_cl_hybrid");
        }

        private double AIUrgentHybridCells(CellCluster cl)
        {
            m_fmUrgentHybridCells.Fuzzify("flv_hyb_count", m_aiDataSet.vir_total_numhybs);
            m_fmUrgentHybridCells.Fuzzify("flv_canhybrid", ((cl.CanHybreed) ? 1.0 : 0.0));
            m_fmUrgentHybridCells.Fuzzify("flv_medicwarn", m_aiDataSet.vir_medicalert);

            //inflate
            m_aiDataSet.vir_total_numhybs += 1.0;

            return m_fmUrgentHybridCells.DeFuzzify("vir_cl_hybrid");
        }

        private double AIKamikaze(CellCluster cl)
        {
            m_fmKamikaze.Fuzzify("flv_clust_count", m_aiDataSet.vir_total_numclusters);
            m_fmKamikaze.Fuzzify("flv_dist", cl.m_clusterAI.AIDataSet.cl_dist_clst_enem);

            //inflate
            m_aiDataSet.vir_total_numclusters -= 1.0;

            return m_fmKamikaze.DeFuzzify("vir_cl_kamikaze");
        }

        #endregion

        #endregion

        #endregion
    }
}
