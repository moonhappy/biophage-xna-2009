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

namespace Biophage.Game.Stages.Game.Common
{
    /// <summary>
    /// Does calculation opperations
    /// </summary>
    public class StateCalculations
    {
        #region fields

        protected DebugManager m_debugMgr;
        protected BiophageGameBaseScn m_scn;

        int totalNumCells = 0;
        int totalNumInfectedCells = 0;

        SortedList<double, Common.Virus> m_sortedRanks = new SortedList<double, Virus>(new DoubleReverse());

        #endregion

        #region methods

        public StateCalculations(DebugManager debugMgr, BiophageGameBaseScn scn)
        {
            m_debugMgr = debugMgr;
            m_scn = scn;
        }

        public void UpdateState()
        {
            //set all to zero
            totalNumCells = totalNumInfectedCells = 0;
            m_sortedRanks.Clear();

            //  - count total num of uninfected
            totalNumCells += m_scn.UninfectCellsList.Count;

            //  - count total num of infected cells
            foreach (CellCluster cluster in m_scn.CellClustersList)
            {
                totalNumInfectedCells += cluster.stateData.numCellsTotal;
            }
            totalNumCells += totalNumInfectedCells;

            //  - calc percentages and ranks
            foreach (Virus virus in m_scn.VirusList)
            {
                //calc num infect cells
                virus.virusStateData.numInfectedCells = 0;
                foreach (Common.CellCluster cluster in virus.virusStateData.clusters)
                {
                    virus.virusStateData.numInfectedCells +=
                        cluster.stateData.numCellsTotal;
                }

                m_debugMgr.Assert(totalNumCells > 0,
                    "StateCalculations:UpdateState - total number of cells is zero, game should have ended.");
                virus.virusStateData.infectPercentage = ((double)virus.virusStateData.numInfectedCells / (double)totalNumCells) * 100.0;

                double uniqueKey = virus.virusStateData.infectPercentage;
                if (double.IsNaN(uniqueKey))
                    uniqueKey = 1f;
                while (m_sortedRanks.ContainsKey(uniqueKey))
                {
                    uniqueKey -= (uniqueKey * 0.05f) + double.Epsilon;
                    if (double.IsNaN(uniqueKey))
                        uniqueKey = 1f;
                }

                m_sortedRanks.Add(uniqueKey, virus);
            }
            int ranking = 1;
            foreach (KeyValuePair<double, Common.Virus> virusRankKVP in m_sortedRanks)
            {
                virusRankKVP.Value.virusStateData.rank = ranking;
                ranking++;
            }
        }

        #endregion
    }
}
