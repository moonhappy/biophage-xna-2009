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
    public class InGameMenu : Menu
    {
        #region fields

        CellCluster m_cluster;
        public CellCluster Cluster
        {
            get { return m_cluster; }
            set
            {
                m_cluster = value;
                if (m_cluster != null)
                    RecalcClusterAttrs();
            }
        }

        //local cluster state values
        int m_nutrientsAval = 0;
        int m_cellCount = 0;
        string[] m_cellTypesAval;
        bool m_canHybrid = false;

        public BiophageGameBaseScn m_scn;

        #region menu items

        //dynamic values
        MenuWindow divideCellsWnd;
        MenuWindow splitClusterWnd;
        MenuWindow splitClusterHyWnd;
        MenuWindow splitClusterBigWnd;
        MenuWindow splitClusterSmallWnd;
        MenuWindow hybridCellsWnd;

        MenuLabel divNutsAvalLab;
        MenuValue divRBCVal;
        MenuValue divPLTVal;
        MenuValue divTNKVal;
        MenuValue divSILVal;

        MenuLabel splCellsAvalLab;
        MenuValue splRBCVal;
        MenuValue splPLTVal;
        MenuValue splTNKVal;
        MenuValue splSILVal;
        MenuValue splSHYVal;
        MenuValue splMHYVal;
        MenuValue splBHYVal;

        MenuToggle hybCellATgl;
        MenuToggle hybCellBTgl;
        MenuValue hybCountVal;

        #endregion

        #endregion

        #region methods

        #region construction

        public InGameMenu(DebugManager dbgMgr, ResourceManager resMgr,
            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
            Scene parentScn)
            : base(GlobalConstants.CLUSTER_MENU_ID, dbgMgr, resMgr,
            graphicsMgr, spriteBatch,
            new SpriteFontResHandle(dbgMgr, resMgr, "Content\\Fonts", "HUDFont"),
            parentScn.Stage.SceneMgr.Game.LeadPlayerIndex)
        {
            m_cluster = null;
            m_graphicsMgr = graphicsMgr;
            m_spriteBatch = spriteBatch;

            SpriteFontResHandle menuFont = new SpriteFontResHandle(
                dbgMgr, resMgr, "Content\\Fonts\\", "MenuFont");

            m_cellTypesAval = new string[1];
            m_cellTypesAval[0] = "";

            #region divide cells window

            divideCellsWnd = new MenuWindow(
                GlobalConstants.CM_DIVIDE_WND_ID, dbgMgr, resMgr,
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuBack"),
                this, true,
                graphicsMgr, spriteBatch);

            MenuLabel divTitleLab = new MenuLabel(
                GlobalConstants.CMDIVIDE_TITLE_LAB, "DIVIDE CELLS",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);

            divNutsAvalLab = new MenuLabel(
                GlobalConstants.CMDIVIDE_NUTSTORE_LAB, "Nutrients: " + m_nutrientsAval,
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);

            MenuLabel divRBCLab = new MenuLabel(
                GlobalConstants.CMDIVIDE_RBC_LAB, "Red Blood Cells",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);

            divRBCVal = new MenuValue(
                GlobalConstants.CMDIVIDE_RBC_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);
            divRBCVal.UIAction += divideTgl_UIAction;

            MenuLabel divPLTLab = new MenuLabel(
                GlobalConstants.CMDIVIDE_PLA_LAB, "Platelets",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);

            divPLTVal = new MenuValue(
                GlobalConstants.CMDIVIDE_PLA_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);
            divPLTVal.UIAction += divideTgl_UIAction;

            MenuLabel divTNKLab = new MenuLabel(
                GlobalConstants.CMDIVIDE_TNK_LAB, "Tank Cells",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);

            divTNKVal = new MenuValue(
                GlobalConstants.CMDIVIDE_TNK_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);
            divTNKVal.UIAction += divideTgl_UIAction;

            MenuLabel divSILLab = new MenuLabel(
                GlobalConstants.CMDIVIDE_SIL_LAB, "Silo Cells",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);

            divSILVal = new MenuValue(
                GlobalConstants.CMDIVIDE_SIL_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);
            divSILVal.UIAction += divideTgl_UIAction;

            MenuButton divOKBut = new MenuButton(
                GlobalConstants.CMDIVIDE_OK_BUT, "OK", "",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);
            divOKBut.UIAction += divOKBut_UIAction;

            MenuButton divCancelBut = new MenuButton(
                GlobalConstants.CMDIVIDE_CANCEL_BUT, "CANCEL", "",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                divideCellsWnd, true);
            divCancelBut.UIAction += cancelBut_UIAction;

            #endregion

            #region split cluster window

            splitClusterWnd = new MenuWindow(
                GlobalConstants.CM_SPLIT_WND_ID, dbgMgr, resMgr,
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuBack"),
                this, true, graphicsMgr, spriteBatch);

            MenuLabel splTitleLab = new MenuLabel(
                GlobalConstants.CMSPLIT_TITLE_LAB, "SPLIT CLUSTER",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterWnd, true);

            splCellsAvalLab = new MenuLabel(
                GlobalConstants.CMSPLIT_CELLCOUNT_LAB, "Cell Count: " + m_cellCount,
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterWnd, true);

            MenuButton splSmallCellsBut = new MenuButton(
                GlobalConstants.CMSPLIT_SHOWSMALLS_BUT, "Small Cells", "",
                menuFont, dbgMgr, resMgr, graphicsMgr, spriteBatch,
                splitClusterWnd, true);
            splSmallCellsBut.UIAction += splSmallCellsBut_UIAction;

            #region split smalls

            splitClusterSmallWnd = new MenuWindow(
                GlobalConstants.CM_SPLITSMALL_WND_ID, dbgMgr, resMgr,
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuBack"),
                this, true, graphicsMgr, spriteBatch);

            MenuLabel splRBCLab = new MenuLabel(
                GlobalConstants.CMSPLITSMALL_RBC_LAB, "Red Blood Cells",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterSmallWnd, true);

            splRBCVal = new MenuValue(
                GlobalConstants.CMSPLITSMALL_RBC_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterSmallWnd, true);
            splRBCVal.UIAction += splitTgl_UIAction;

            MenuLabel splPLTLab = new MenuLabel(
                GlobalConstants.CMSPLITSMALL_PLT_LAB, "Platelets",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterSmallWnd, true);

            splPLTVal = new MenuValue(
                GlobalConstants.CMSPLITSMALL_PLT_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterSmallWnd, true);
            splPLTVal.UIAction += splitTgl_UIAction;

            MenuButton splSmallOKBut = new MenuButton(
                GlobalConstants.CMSPLITSMALL_OK_BUT, "OK", "",
                menuFont, dbgMgr, resMgr, graphicsMgr,
                spriteBatch, splitClusterSmallWnd, true);
            splSmallOKBut.UIAction += splSubOKBut_UIAction;

            #endregion

            MenuButton splBigCellsBut = new MenuButton(
                GlobalConstants.CMSPLIT_SHOWBIGS_BUT, "Big Cells", "",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterWnd, true);
            splBigCellsBut.UIAction += splBigCellsBut_UIAction;

            #region split bigs

            splitClusterBigWnd = new MenuWindow(
                GlobalConstants.CM_SPLITBIGS_WND_ID, dbgMgr, resMgr,
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuBack"),
                this, true, graphicsMgr, spriteBatch);

            MenuLabel splTNKLab = new MenuLabel(
                GlobalConstants.CMSPLITBIGS_TNK_LAB, "Tank Cells",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterBigWnd, true);

            splTNKVal = new MenuValue(
                GlobalConstants.CMSPLITBIGS_TNK_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterBigWnd, true);
            splTNKVal.UIAction += splitTgl_UIAction;

            MenuLabel splSILLab = new MenuLabel(
                GlobalConstants.CMSPLITBIGS_SIL_LAB, "Silo Cells",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterBigWnd, true);

            splSILVal = new MenuValue(
                GlobalConstants.CMSPLITBIGS_SIL_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterBigWnd, true);
            splSILVal.UIAction += splitTgl_UIAction;

            MenuButton splBigOKBut = new MenuButton(
                GlobalConstants.CMSPLITBIGS_OK_BUT, "OK", "",
                menuFont, dbgMgr, resMgr, graphicsMgr,
                spriteBatch, splitClusterBigWnd, true);
            splBigOKBut.UIAction += splSubOKBut_UIAction;

            #endregion

            MenuButton splHybridsBut = new MenuButton(
                GlobalConstants.CMSPLIT_SHOWHYBS_BUT, "Hybrids", "",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterWnd, true);
            splHybridsBut.UIAction += splHybridsBut_UIAction;

            #region split hybrids

            splitClusterHyWnd = new MenuWindow(
                GlobalConstants.CM_SPLITHYBRIDS_WND_ID,
                dbgMgr, resMgr,
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuBack"),
                this, true, graphicsMgr, spriteBatch);

            MenuLabel splHySmallLab = new MenuLabel(
                GlobalConstants.CMSPLITHYBRIDS_SML_LAB, "Small Hybrids",
                menuFont, dbgMgr, resMgr, graphicsMgr, spriteBatch,
                splitClusterHyWnd, true);

            splSHYVal = new MenuValue(
                GlobalConstants.CMSPLITHYBRIDS_SML_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr, graphicsMgr, spriteBatch,
                splitClusterHyWnd, true);
            splSHYVal.UIAction += splitTgl_UIAction;

            MenuLabel splHyMedLab = new MenuLabel(
                GlobalConstants.CMSPLITHYBRIDS_MED_LAB, "Medium Hybrids",
                menuFont, dbgMgr, resMgr, graphicsMgr, spriteBatch,
                splitClusterHyWnd, true);

            splMHYVal = new MenuValue(
                GlobalConstants.CMSPLITHYBRIDS_MED_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr, graphicsMgr, spriteBatch,
                splitClusterHyWnd, true);
            splMHYVal.UIAction += splitTgl_UIAction;

            MenuLabel splHyBigLab = new MenuLabel(
                GlobalConstants.CMSPLITHYBRIDS_BIG_LAB, "Big Hybrids",
                menuFont, dbgMgr, resMgr, graphicsMgr, spriteBatch,
                splitClusterHyWnd, true);

            splBHYVal = new MenuValue(
                GlobalConstants.CMSPLITHYBRIDS_BIG_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr, graphicsMgr, spriteBatch,
                splitClusterHyWnd, true);
            splBHYVal.UIAction += splitTgl_UIAction;

            MenuButton splHybOKBut = new MenuButton(
                GlobalConstants.CMSPLITHYBRIDS_OK_BUT, "OK", "",
                menuFont, dbgMgr, resMgr, graphicsMgr,
                spriteBatch, splitClusterHyWnd, true);
            splHybOKBut.UIAction += splSubOKBut_UIAction;

            #endregion

            MenuButton splOKBut = new MenuButton(
                GlobalConstants.CMSPLIT_OK_BUT, "OK", "",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterWnd, true);
            splOKBut.UIAction += splOKBut_UIAction;

            MenuButton splCancelBut = new MenuButton(
                GlobalConstants.CMSPLIT_CANCEL_BUT, "CANCEL", "",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                splitClusterWnd, true);
            splCancelBut.UIAction += cancelBut_UIAction;

            #endregion

            #region hybrid cells windows

            hybridCellsWnd = new MenuWindow(
                GlobalConstants.CM_HYBRIDS_WND_ID, dbgMgr, resMgr,
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuSelect"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuBack"),
                this, true, graphicsMgr, spriteBatch);

            MenuLabel hybTitleLab = new MenuLabel(
                GlobalConstants.CMHYBRIDS_TITLE_LAB, "HYBRID CELLS",
                menuFont, dbgMgr, resMgr, graphicsMgr, spriteBatch,
                hybridCellsWnd, true);

            hybCellATgl = new MenuToggle(
                GlobalConstants.CMHYBRIDS_CELLA_TGL, m_cellTypesAval, 0,
                new Dictionary<string, string>(),
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr, graphicsMgr, spriteBatch,
                hybridCellsWnd, true);
            hybCellATgl.NextButton.UIAction += HybNextA_UIAction;
            hybCellATgl.PrevButton.UIAction += HybPrevA_UIAction;

            hybCellBTgl = new MenuToggle(
                GlobalConstants.CMHYBRIDS_CELLB_TGL, m_cellTypesAval, 0,
                new Dictionary<string, string>(),
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr, graphicsMgr, spriteBatch,
                hybridCellsWnd, true);
            hybCellBTgl.NextButton.UIAction += HybNextB_UIAction;
            hybCellBTgl.PrevButton.UIAction += HybPrevB_UIAction;

            MenuLabel hybCountLab = new MenuLabel(
                GlobalConstants.CMHYBRIDS_COUNT_LAB, "Hybrid Count",
                menuFont, dbgMgr, resMgr, graphicsMgr, spriteBatch,
                hybridCellsWnd, true);

            hybCountVal = new MenuValue(
                GlobalConstants.CMHYBRIDS_COUNT_VAL, 0, 0, 0, "",
                menuFont,
                new SoundResHandle(m_debugMgr, resMgr, "Content\\Sounds\\", "MenuMove"),
                dbgMgr, resMgr, graphicsMgr, spriteBatch,
                hybridCellsWnd, true);

            MenuButton hybOKBut = new MenuButton(
                GlobalConstants.CMHYBRIDS_OK_BUT, "OK", "",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                hybridCellsWnd, true);
            hybOKBut.UIAction += hybOKBut_UIAction;

            MenuButton hybCancelBut = new MenuButton(
                GlobalConstants.CMHYBRIDS_CANCEL_BUT, "CANCEL", "",
                menuFont, dbgMgr, resMgr,
                graphicsMgr, spriteBatch,
                hybridCellsWnd, true);
            hybCancelBut.UIAction += cancelBut_UIAction;

            #endregion
        }

        #endregion

        #region actions

        void cancelBut_UIAction(object sender, EventArgs e)
        {
            //cancel - set this to inactive
            Active = false;
        }

        #region divide cells

        void divideTgl_UIAction(object sender, EventArgs e)
        {
            //recalc maimums
            SetMaximumsForDivide();
        }

        void divOKBut_UIAction(object sender, EventArgs e)
        {
            //get settings
            byte addRBC = (byte)divRBCVal.CurrentValue;
            byte addPLT = (byte)divPLTVal.CurrentValue;
            byte addTNK = (byte)divTNKVal.CurrentValue;
            byte addSIL = (byte)divSILVal.CurrentValue;

            //do the action - tell cluster
            if ((addRBC + addPLT + addTNK + addSIL) > 0)
            {
                if (m_cluster.GetSessionDetails.isHost)
                {
                    //send packet to everyone
                    m_scn.HostClusterDivideCells(m_cluster,
                        addRBC, addPLT, addTNK, addSIL);
                }
                else
                {
                    //i'm a client and should tell server and wait for approval
                    m_scn.ClientSendClusterDividedCells(m_cluster,
                        addRBC, addPLT, addTNK, addSIL);
                }
            }

            Active = false;
        }

        #endregion

        #region split cluster

        void splitTgl_UIAction(object sender, EventArgs e)
        {
            //recalc maximums
            SetMaximumsForSplit();
        }

        void splOKBut_UIAction(object sender, EventArgs e)
        {
            //split cluster into two clusters
            //get settings
            byte splRBC = (byte)splRBCVal.CurrentValue;
            byte splPLT = (byte)splPLTVal.CurrentValue;
            byte splTNK = (byte)splTNKVal.CurrentValue;
            byte splSIL = (byte)splSILVal.CurrentValue;
            byte splSHY = (byte)splSHYVal.CurrentValue;
            byte splMHY = (byte)splMHYVal.CurrentValue;
            byte splBHY = (byte)splBHYVal.CurrentValue;

            //do the action - tell cluster
            if ((splRBC + splPLT + splTNK + splSIL + splSHY + splMHY + splBHY) > 0)
            {
                if (m_cluster.GetSessionDetails.isHost)
                {
                    //send packet to everyone
                    m_scn.HostSplitCluster(m_cluster,
                        splRBC, splPLT, splTNK, splSIL,
                        splSHY, splMHY, splBHY);
                }
                else
                {
                    //i'm a client and should tell server and wait for approval
                    m_scn.ClientSendSplitCluster(m_cluster,
                        splRBC, splPLT, splTNK, splSIL,
                        splSHY, splMHY, splBHY);
                }
            }

            Active = false;
        }

        void splSubOKBut_UIAction(object sender, EventArgs e)
        {
            SetCurrentToPreviousWindow();
        }

        void splHybridsBut_UIAction(object sender, EventArgs e)
        {
            //show hybrids window
            SetCurrentWindow(GlobalConstants.CM_SPLITHYBRIDS_WND_ID);
        }

        void splBigCellsBut_UIAction(object sender, EventArgs e)
        {
            //show big cells window
            SetCurrentWindow(GlobalConstants.CM_SPLITBIGS_WND_ID);
        }

        void splSmallCellsBut_UIAction(object sender, EventArgs e)
        {
            //show small cells window
            SetCurrentWindow(GlobalConstants.CM_SPLITSMALL_WND_ID);
        }

        #endregion

        #region hybrid cells

        void HybNextA_UIAction(object sender, EventArgs e)
        {
            //change to unique as it goes
            if (hybCellATgl.CurrentValue == hybCellBTgl.CurrentValue)
            {
                hybCellATgl.ToggleNextValue();
                if (hybCellATgl.CurrentValue == hybCellBTgl.CurrentValue)
                    hybCellATgl.ToggleStart();
            }
            m_debugMgr.Assert(hybCellATgl.CurrentValue != hybCellBTgl.CurrentValue,
                "InGameMenu:HybNext_UIAction - current values are the same.");

            SetOptionsForHybrids();
        }

        void HybPrevA_UIAction(object sender, EventArgs e)
        {
            //change to unique as it goes
            if (hybCellATgl.CurrentValue == hybCellBTgl.CurrentValue)
            {
                hybCellATgl.TogglePrevValue();
                if (hybCellATgl.CurrentValue == hybCellBTgl.CurrentValue)
                    hybCellATgl.ToggleEnd();
            }
            m_debugMgr.Assert(hybCellATgl.CurrentValue != hybCellBTgl.CurrentValue,
                "InGameMenu:HybNext_UIAction - current values are the same.");

            SetOptionsForHybrids();
        }

        void HybNextB_UIAction(object sender, EventArgs e)
        {
            //change to unique as it goes
            if (hybCellATgl.CurrentValue == hybCellBTgl.CurrentValue)
            {
                hybCellBTgl.ToggleNextValue();
                if (hybCellATgl.CurrentValue == hybCellBTgl.CurrentValue)
                    hybCellBTgl.ToggleStart();
            }
            m_debugMgr.Assert(hybCellATgl.CurrentValue != hybCellBTgl.CurrentValue,
                "InGameMenu:HybNext_UIAction - current values are the same.");

            SetOptionsForHybrids();
        }

        void HybPrevB_UIAction(object sender, EventArgs e)
        {
            //change to unique as it goes
            if (hybCellATgl.CurrentValue == hybCellBTgl.CurrentValue)
            {
                hybCellBTgl.TogglePrevValue();
                if (hybCellATgl.CurrentValue == hybCellBTgl.CurrentValue)
                    hybCellBTgl.ToggleEnd();
            }
            m_debugMgr.Assert(hybCellATgl.CurrentValue != hybCellBTgl.CurrentValue,
                "InGameMenu:HybNext_UIAction - current values are the same.");

            SetOptionsForHybrids();
        }

        void hybOKBut_UIAction(object sender, EventArgs e)
        {
            //hybreed cells
            //get settings
            byte hybCount = (byte)hybCountVal.CurrentValue;
            Cells.CellTypeEnum hybType = Cells.CellTypeEnum.SMALL_HYBRID;

            Cells.CellTypeEnum srcCellA = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.RED_BLOOD_CELL;
            Cells.CellTypeEnum srcCellB = Biophage.Game.Stages.Game.Common.Cells.CellTypeEnum.RED_BLOOD_CELL;

            switch (hybCellATgl.CurrentValue)
            {
                case "Red Blood Cell":
                    srcCellA = Cells.CellTypeEnum.RED_BLOOD_CELL;
                    hybType = Cells.CellTypeEnum.SMALL_HYBRID;
                    break;
                case "Platelet":
                    srcCellA = Cells.CellTypeEnum.PLATELET;
                    hybType = Cells.CellTypeEnum.SMALL_HYBRID;
                    break;
                case "Tank Cell":
                    srcCellA = Cells.CellTypeEnum.BIG_CELL_TANK;
                    hybType = Cells.CellTypeEnum.BIG_HYBRID;
                    break;
                case "Silo Cell":
                    srcCellA = Cells.CellTypeEnum.BIG_CELL_SILO;
                    hybType = Cells.CellTypeEnum.BIG_HYBRID;
                    break;
            }

            switch (hybCellBTgl.CurrentValue)
            {
                case "Red Blood Cell":
                    srcCellB = Cells.CellTypeEnum.RED_BLOOD_CELL;
                    if (hybType == Cells.CellTypeEnum.BIG_HYBRID)
                        hybType = Cells.CellTypeEnum.MED_HYBRID;
                    break;
                case "Platelet":
                    srcCellB = Cells.CellTypeEnum.PLATELET;
                    if (hybType == Cells.CellTypeEnum.BIG_HYBRID)
                        hybType = Cells.CellTypeEnum.MED_HYBRID;
                    break;
                case "Tank Cell":
                    srcCellB = Cells.CellTypeEnum.BIG_CELL_TANK;
                    if (hybType == Cells.CellTypeEnum.SMALL_HYBRID)
                        hybType = Cells.CellTypeEnum.MED_HYBRID;
                    break;
                case "Silo Cell":
                    srcCellB = Cells.CellTypeEnum.BIG_CELL_SILO;
                    if (hybType == Cells.CellTypeEnum.SMALL_HYBRID)
                        hybType = Cells.CellTypeEnum.MED_HYBRID;
                    break;
            }

            //do the action - tell cluster
            if (hybCount > 0)
            {
                if (m_cluster.GetSessionDetails.isHost)
                    //send packet to everyone
                    m_scn.HostClusterHybridCells(m_cluster, hybCount, srcCellA, srcCellB);
                else
                    //i'm a client and should tell server and wait for approval
                    m_scn.ClientSendClusterHybrids(m_cluster, hybCount, srcCellA, srcCellB);
            }

            Active = false;
        }

        #endregion

        #endregion

        #region helper calcs

        /// <summary>
        /// Updates current cluster cell count and nutrient store attributes.
        /// </summary>
        public void RecalcClusterAttrs()
        {
            m_cellCount = (int)m_cluster.stateData.numCellsTotal;

            LinkedList<string> avalsStrings;
            m_cluster.ReadjustHybridCapability(out avalsStrings);
            m_nutrientsAval = (int)m_cluster.stateData.attrNutrientStore;

            m_canHybrid = m_cluster.CanHybreed;

            if (avalsStrings.Count == 0)
                avalsStrings.AddLast("");

            m_cellTypesAval = avalsStrings.ToArray();
        }

        /// <summary>
        /// Resets all the maximums for the divide window.
        /// </summary>
        public void SetMaximumsForDivide()
        {
            int nutrientsLeft = m_nutrientsAval;

            //determine current avaliable nutrients
            nutrientsLeft -= (int)(divRBCVal.CurrentValue * GlobalConstants.RBC_threshNToDivide);
            nutrientsLeft -= (int)(divPLTVal.CurrentValue * GlobalConstants.PLATELET_threshNToDivide);
            nutrientsLeft -= (int)(divTNKVal.CurrentValue * GlobalConstants.TANK_threshNToDivide);
            nutrientsLeft -= (int)(divSILVal.CurrentValue * GlobalConstants.SILO_threshNToDivide);

            //set new maximum values
            divRBCVal.MaxValue = (m_cluster.stateData.numRBCs > 0) ? 
                divRBCVal.CurrentValue + (nutrientsLeft / GlobalConstants.RBC_threshNToDivide) : 0;
            divPLTVal.MaxValue = (m_cluster.stateData.numPlatelets > 0) ? 
                divPLTVal.CurrentValue + (nutrientsLeft / GlobalConstants.PLATELET_threshNToDivide) : 0;
            divTNKVal.MaxValue = (m_cluster.stateData.numTanks > 0) ? 
                divTNKVal.CurrentValue + (nutrientsLeft / GlobalConstants.TANK_threshNToDivide) : 0;
            divSILVal.MaxValue = (m_cluster.stateData.numSilos > 0) ? 
                divSILVal.CurrentValue + (nutrientsLeft / GlobalConstants.SILO_threshNToDivide) : 0;

            //update aval label
            divNutsAvalLab.LabelString = "Nutrients: " + nutrientsLeft;
        }

        public void ResetDivide()
        {
            divRBCVal.CurrentValue = divPLTVal.CurrentValue = divTNKVal.CurrentValue = divSILVal.CurrentValue = 0;

            SetMaximumsForDivide();
        }

        /// <summary>
        /// Resets all the maximums for the divide window.
        /// </summary>
        public void SetMaximumsForSplit()
        {
            int cellsLeft = m_cellCount;
            cellsLeft--;//must have at least one cell in one cluster

            //determine number of cells left after changes
            cellsLeft -= splRBCVal.CurrentValue;
            cellsLeft -= splPLTVal.CurrentValue;
            cellsLeft -= splTNKVal.CurrentValue;
            cellsLeft -= splSILVal.CurrentValue;
            cellsLeft -= splSHYVal.CurrentValue;
            cellsLeft -= splMHYVal.CurrentValue;
            cellsLeft -= splBHYVal.CurrentValue;

            //set new maximum values
            if (cellsLeft > 0)
            {
                splRBCVal.MaxValue = m_cluster.stateData.numRBCs;
                splPLTVal.MaxValue = m_cluster.stateData.numPlatelets;
                splTNKVal.MaxValue = m_cluster.stateData.numTanks;
                splSILVal.MaxValue = m_cluster.stateData.numSilos;
                splSHYVal.MaxValue = m_cluster.stateData.numSmallHybrids;
                splMHYVal.MaxValue = m_cluster.stateData.numMediumHybrids;
                splBHYVal.MaxValue = m_cluster.stateData.numBigHybrids;
            }
            else
            {
                splRBCVal.MaxValue = splRBCVal.CurrentValue;
                splPLTVal.MaxValue = splPLTVal.CurrentValue;
                splTNKVal.MaxValue = splTNKVal.CurrentValue;
                splSILVal.MaxValue = splSILVal.CurrentValue;
                splSHYVal.MaxValue = splSHYVal.CurrentValue;
                splMHYVal.MaxValue = splMHYVal.CurrentValue;
                splBHYVal.MaxValue = splBHYVal.CurrentValue;
            }

            

            splCellsAvalLab.LabelString = "Cell Count: " + cellsLeft;
        }

        public void ResetSplit()
        {
            splRBCVal.CurrentValue = splPLTVal.CurrentValue = splTNKVal.CurrentValue = splSILVal.CurrentValue =
                splSHYVal.CurrentValue = splMHYVal.CurrentValue = splBHYVal.CurrentValue;

            SetMaximumsForSplit();
        }

        /// <summary>
        /// Resets hybrid options.
        /// </summary>
        public void SetOptionsForHybrids()
        {
            //check equivalency
            if (hybCellATgl.ValuesArray != m_cellTypesAval)
            {
                //new string array - set to with defaults and return
                hybCellATgl.ValuesArray = m_cellTypesAval;
                hybCellBTgl.ValuesArray = m_cellTypesAval;
                hybCellBTgl.ToggleNextValue();
            }

            //update the max number of hybrids to make
            int numCellA = 0;
            int numCellB = 0;

            switch (hybCellATgl.CurrentValue)
            {
                case "Red Blood Cell":
                    numCellA = m_cluster.stateData.numRBCs;
                    break;
                case "Platelet":
                    numCellA = m_cluster.stateData.numPlatelets;
                    break;
                case "Tank Cell":
                    numCellA = m_cluster.stateData.numTanks;
                    break;
                case "Silo Cell":
                    numCellA = m_cluster.stateData.numSilos;
                    break;
            }

            switch (hybCellBTgl.CurrentValue)
            {
                case "Red Blood Cell":
                    numCellB = m_cluster.stateData.numRBCs;
                    break;
                case "Platelet":
                    numCellB = m_cluster.stateData.numPlatelets;
                    break;
                case "Tank Cell":
                    numCellB = m_cluster.stateData.numTanks;
                    break;
                case "Silo Cell":
                    numCellB = m_cluster.stateData.numSilos;
                    break;
            }

            int hybMax = Math.Min(numCellA, numCellB);
            if (hybMax < hybCountVal.CurrentValue)
                hybCountVal.CurrentValue = hybMax;

            hybCountVal.MaxValue = hybMax;
        }

        #endregion

        #endregion
    }
}
