using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Biophage.Game
{
    /// <summary>
    /// This class holds global constant to use through the game.
    /// </summary>
    public class GlobalConstants
    {
        #region construction ids

        #region main stage

        //IDs
        public const uint MAIN_STAGE_ID = 100;
            public const uint MAIN_COMMON_RES_SCN_ID = 10;
            public const uint START_SPLASH_SCN_ID = 11;
            public const uint MAIN_MENU_SCN_ID = 12;
            public const uint LOADING_SCN_ID = 13;
            public const uint CREDITS_SCN_ID = 14;
            public const uint PROPAGANDA_SCN_ID = 15;
            public const uint GAMEPLAY_SETTINGS_SCN_ID = 16;
            public const uint LOBBY_SCN_ID = 17;
            public const uint NET_LOADING_SCN_ID = 18;
            public const uint CLIENT_JOIN_SCN_ID = 19;

        public const uint MAIN_MENU_ID = 1;
            public const uint MMMAIN_WND_ID = 10;
                public const uint MMSP_BUT_ID = 0;
                public const uint MMMP_BUT_ID = 1;
                public const uint MMSETS_BUT_ID = 2;
                public const uint MMEXTR_BUT_ID = 3;
                public const uint MMEXIT_BUT_ID = 4;
            public const uint MMSP_WND_ID = 20;
                public const uint MMSPPLAY_BUT_ID = 0;
                public const uint MMSPTUTORIAL_BUT_ID = 1;
            public const uint MMMP_WND_ID = 30;
                public const uint MMMP_LAN_BUT_ID = 0;
                public const uint MMMP_NET_BUT_ID = 1;
                public const uint MMMP_LAN_WND_ID = 31;
                    public const uint MMMPLAN_HOST_BUT_ID = 0;
                    public const uint MMMPLAN_JOIN_BUT_ID = 1;
                public const uint MMMP_NET_WND_ID = 32;
                    public const uint MMMPNET_HOST_BUT_ID = 0;
                    public const uint MMMPNET_JOIN_BUT_ID = 1;
            public const uint MMEXTR_WND_ID = 50;
                public const uint MMEXTRCREDS_BUT_ID = 0;
            public const uint MMGOTOMP_WND_ID = 60;
                public const uint MMGTMARKET_LABEL_ID = 0;
                public const uint MMGTMARKET_BUT_YES_ID = 1;
                public const uint MMGTMARKET_BUT_NO_ID = 2;
            public const uint MMQUIT_PROMT_WND_ID = 70;
                public const uint MMQUIT_LABEL_ID = 0;
                public const uint MMQUIT_BUT_YES_ID = 1;
                public const uint MMQUIT_BUT_NO_ID = 2;

        public const uint SESSION_SETTINGS_MENU = 200;
            public const uint SS_MAIN_WND = 10;
                public const uint SSMAIN_GAMETYPE_LABEL = 1;
                public const uint SSMAIN_GAMETYPE_TGL = 2;
                public const uint SSMAIN_GTYPE_SETTINGS_BUT = 3;
                public const uint SSMAIN_LEVEL_LABEL = 4;
                public const uint SSMAIN_LEVEL_TGL = 5;
                public const uint SSMAIN_BOT_LABEL = 6;
                public const uint SSMAIN_BOTNUM_VAL = 7;
                public const uint SSMAIN_CONTINUE_LABEL = 8;    
                public const uint SSMAIN_PLAY_BUT = 9;
                public const uint SSMAIN_CANCEL_BUT = 10;    
            public const uint SSMAIN_TIMED_SWND = 20;
                public const uint SSMAIN_TIMED_LABEL = 1;
                public const uint SSMAIN_TIMED_VAL = 2;
            public const uint SSMAIN_ILL_SWND = 30;
                public const uint SSMAIN_ILL_LABEL = 1;
                public const uint SSMAIN_ILL_VAL = 2;

        #endregion

        #region game stage

        public const uint GAME_STAGE_ID = 200;
            //public const uint BLANK_SCN_ID = 20;
            public const uint GAME_MENU_SCN_ID = 21;
            public const uint COMMRES_SCN_ID = 22;
            public const uint TRIAL_LVL_SCN_ID = 23;
            public const uint TUTORIAL_LVL_SCN_ID = 24;

        public const uint GAME_MENU_ID = 2;
            public const uint GMMAIN_WND_ID = 10;
                public const uint GMMAIN_RESUME_BUT_ID = 0;
                public const uint GMMAIN_QUIT_BUT_ID = 1;
            public const uint GMQUIT_WND_ID = 20;
                public const uint GMQUIT_LABEL_ID = 0;
                public const uint GMQUIT_BUT_YES_ID = 1;
                public const uint GMQUIT_BUT_NO_ID = 2;

        #region in game options

        public const uint CLUSTER_MENU_ID = 3;

            public const uint CM_DIVIDE_WND_ID = 10;
                public const uint CMDIVIDE_TITLE_LAB = 1;
                public const uint CMDIVIDE_NUTSTORE_LAB = 2;
                public const uint CMDIVIDE_RBC_LAB = 3;
                public const uint CMDIVIDE_RBC_COUNT_VAL = 4;
                public const uint CMDIVIDE_PLA_LAB = 5;
                public const uint CMDIVIDE_PLA_COUNT_VAL = 6;
                public const uint CMDIVIDE_TNK_LAB = 7;
                public const uint CMDIVIDE_TNK_COUNT_VAL = 8;
                public const uint CMDIVIDE_SIL_LAB = 9;
                public const uint CMDIVIDE_SIL_COUNT_VAL = 10;
                public const uint CMDIVIDE_OK_BUT = 11;
                public const uint CMDIVIDE_CANCEL_BUT = 12;

            public const uint CM_SPLIT_WND_ID = 20;
                public const uint CMSPLIT_TITLE_LAB = 1;
                public const uint CMSPLIT_CELLCOUNT_LAB = 2;
                public const uint CMSPLIT_SHOWSMALLS_BUT = 3;
                public const uint CMSPLIT_SHOWBIGS_BUT = 4;
                public const uint CMSPLIT_SHOWHYBS_BUT = 5;
                public const uint CMSPLIT_OK_BUT = 6;
                public const uint CMSPLIT_CANCEL_BUT = 7;
                public const uint CM_SPLITHYBRIDS_WND_ID = 30;
                    public const uint CMSPLITHYBRIDS_SML_LAB = 1;
                    public const uint CMSPLITHYBRIDS_SML_COUNT_VAL = 2;
                    public const uint CMSPLITHYBRIDS_MED_LAB = 3;
                    public const uint CMSPLITHYBRIDS_MED_COUNT_VAL = 4;
                    public const uint CMSPLITHYBRIDS_BIG_LAB = 5;
                    public const uint CMSPLITHYBRIDS_BIG_COUNT_VAL = 6;
                    public const uint CMSPLITHYBRIDS_OK_BUT = 7;
                public const uint CM_SPLITBIGS_WND_ID = 40;
                    public const uint CMSPLITBIGS_TNK_LAB = 1;
                    public const uint CMSPLITBIGS_TNK_COUNT_VAL = 2;
                    public const uint CMSPLITBIGS_SIL_LAB = 3;
                    public const uint CMSPLITBIGS_SIL_COUNT_VAL = 4;
                    public const uint CMSPLITBIGS_OK_BUT = 5;
                public const uint CM_SPLITSMALL_WND_ID = 50;
                    public const uint CMSPLITSMALL_RBC_LAB = 1;
                    public const uint CMSPLITSMALL_RBC_COUNT_VAL = 2;
                    public const uint CMSPLITSMALL_PLT_LAB = 3;
                    public const uint CMSPLITSMALL_PLT_COUNT_VAL = 4;
                    public const uint CMSPLITSMALL_OK_BUT = 5;

            public const uint CM_HYBRIDS_WND_ID = 60;
                public const uint CMHYBRIDS_TITLE_LAB = 1;
                public const uint CMHYBRIDS_CELLA_TGL = 2;
                public const uint CMHYBRIDS_CELLB_TGL = 3;
                public const uint CMHYBRIDS_COUNT_LAB = 4;
                public const uint CMHYBRIDS_COUNT_VAL = 5;
                public const uint CMHYBRIDS_OK_BUT = 6;
                public const uint CMHYBRIDS_CANCEL_BUT = 7;

        #endregion

        #endregion

        #endregion

        #region scene timeouts

#if DEBUG
        public const int MIN_DISCLAIMER_TIMEOUT_SECS = 17;
        public const int MIN_SPLASH_TIMEOUT_SECS = 22;
#else
        public const int MIN_DISCLAIMER_TIMEOUT_SECS = 17;
        public const int MIN_SPLASH_TIMEOUT_SECS = 22;
#endif

        #endregion

        #region cell values

        public const float RBC_WIDTH = 1f;
        public const float RBC_HEIGHT = 0.4f;
        public const float RBC_DEPTH = 1f;
        public const short RBC_theshMaxHealth = 100;
        public const short RBC_threshMaxBattleDefence = 10;
        public const short RBC_threshMaxBattleOffence = 10;
        public const short RBC_threshMaxNStore = 10;
        public const short RBC_threshNToDivide = 10;
        public const float RBC_rateNutrientIncome = 0.5f;
        public const float RBC_rateMaxVelocity = 2f;

        public const float PLATELET_WIDTH = 1f;
        public const float PLATELET_HEIGHT = 0.4f;
        public const float PLATELET_DEPTH = 1f;
        public const short PLATELET_theshMaxHealth = 100;
        public const short PLATELET_threshMaxBattleDefence = 20;
        public const short PLATELET_threshMaxBattleOffence = 20;
        public const short PLATELET_threshMaxNStore = 15;
        public const short PLATELET_threshNToDivide = 15;
        public const float PLATELET_rateNutrientIncome = 0.25f;
        public const float PLATELET_rateMaxVelocity = 5f;

        public const float SILO_WIDTH = 4f;
        public const float SILO_HEIGHT = 4f;
        public const float SILO_DEPTH = 4f;
        public const short SILO_theshMaxHealth = 100;
        public const short SILO_threshMaxBattleDefence = 10;
        public const short SILO_threshMaxBattleOffence = 10;
        public const short SILO_threshMaxNStore = 40;
        public const short SILO_threshNToDivide = 30;
        public const float SILO_rateNutrientIncome = 1f;
        public const float SILO_rateMaxVelocity = 1f;

        public const float TANK_WIDTH = 2f;
        public const float TANK_HEIGHT = 2f;
        public const float TANK_DEPTH = 2f;
        public const short TANK_theshMaxHealth = 100;
        public const short TANK_threshMaxBattleDefence = 85;
        public const short TANK_threshMaxBattleOffence = 85;
        public const short TANK_threshMaxNStore = 20;
        public const short TANK_threshNToDivide = 25;
        public const float TANK_rateNutrientIncome = 0.25f;
        public const float TANK_rateMaxVelocity = 1f;

        public const float HYSMALL_WIDTH = 1f;
        public const float HYSMALL_HEIGHT = 0.4f;
        public const float HYSMALL_DEPTH = 1f;
        public const short HYSMALL_theshMaxHealth = 100;
        public const short HYSMALL_threshMaxBattleDefence = 20;
        public const short HYSMALL_threshMaxBattleOffence = 20;
        public const short HYSMALL_threshMaxNStore = 12;
        public const short HYSMALL_threshNToDivide = short.MaxValue;
        public const float HYSMALL_rateNutrientIncome = 0.35f;
        public const float HYSMALL_rateMaxVelocity = 3f;

        public const float HYMED_WIDTH = 2f;
        public const float HYMED_HEIGHT = 2f;
        public const float HYMED_DEPTH = 2f;
        public const short HYMED_theshMaxHealth = 100;
        public const short HYMED_threshMaxBattleDefence = 50;
        public const short HYMED_threshMaxBattleOffence = 50;
        public const short HYMED_threshMaxNStore = 20;
        public const short HYMED_threshNToDivide = short.MaxValue;
        public const float HYMED_rateNutrientIncome = 0.75f;
        public const float HYMED_rateMaxVelocity = 2f;

        public const float HYBIG_WIDTH = 4f;
        public const float HYBIG_HEIGHT = 4f;
        public const float HYBIG_DEPTH = 4f;
        public const short HYBIG_theshMaxHealth = 100;
        public const short HYBIG_threshMaxBattleDefence = 80;
        public const short HYBIG_threshMaxBattleOffence = 80;
        public const short HYBIG_threshMaxNStore = 30;
        public const short HYBIG_threshNToDivide = short.MaxValue;
        public const float HYBIG_rateNutrientIncome = 0.75f;
        public const float HYBIG_rateMaxVelocity =1f;

        public const float WBC_WIDTH = 5f;
        public const float WBC_HEIGHT = 5f;
        public const float WBC_DEPTH = 5f;
        public const short WBC_theshMaxHealth = 100;
        public const short WBC_threshMaxBattleDefence = 90;
        public const short WBC_threshMaxBattleOffence = 90;
        public const short WBC_threshMaxNStore = 1;
        public const short WBC_threshNToDivide = short.MaxValue;
        public const float WBC_rateNutrientIncome = 0.25f;
        public const float WBC_rateMaxVelocity = 3f;

        #endregion

        #region network packet ids

        public enum NETPACKET_IDS : byte
        {
            #region from server
            NETSERVER_UNINFECTED_CELLS_TRANS = 1,
            NETSERVER_CELL_CLUSTERS_TRANS,
            NETSERVER_BATTLE_WARNING,
            NETSERVER_BATTLE_UNWARNING,
            NETSERVER_NEWGAME_ID,
            NETSERVER_NEW_CLUSTER,
            NETSERVER_CLUSTER_UPDATE,
            NETSERVER_DIV_CLUST_CELLS,
            NETSERVER_HYB_CLUST_CELLS,
            NETSERVER_MEDICATION_DEPLOY,
            NETSERVER_GAME_OVER,
            NETSERVER_IMMUNE_COUNTDOWN,
            NETSERVER_MED_COUNTDOWN,

            NETSERVER_GAME_ISREADY,
            #endregion

            #region from client
            NETCLIENT_NEW_CLUSTER_UCELL,
            NETCLIENT_DIV_CLUST_CELLS,
            NETCLIENT_HYB_CLUST_CELLS,
            NETCLIENT_SPLIT_CLUSTER,
            NETCLIENT_CLUSTER_CHASE,
            NETCLIENT_CLUSTER_EVADE,
            NETCLIENT_CLUSTER_CANCEL_ACTION,

            NETCLIENT_ISREADY
            #endregion
        }

        #region from server

        //THESE will be dispatched 4 times every second
        public const int NETWAIT_UCELLS_TRANS_MSECS = 250;
        public const int NETWAIT_CLUST_TRANS_MSECS = 250;

        #endregion

        #endregion

        #region network session

        public const int NET_MAX_PLAYERS = 8;
        public const int NET_MAX_SESSION_SEARCHES = 8;

        #endregion

        #region game stuff

        public static string[] GP_TYPES_STR_ARRAY = { "Timed Match", "Illness", "Last Standing" };
        public static string[] GP_TYPES_DESC_ARRAY = 
        {
            "The virus with the\ngreatest infection after\na specific time will be\ndeclared as the winner.",
            "The first virus to reach\na specific infection\npercentage will be\ndeclared as the winner.",
            "The last virus remaining\nin the game will be\ndeclared as the winner."
        };
        public const byte GP_TIMED_MIN_TIME = 10;//10 MINUTES
        public const byte GP_TIMED_MAX_TIME = 250;
        public const byte GP_TIMED_DEF_TIME = 45;
        public const byte GP_ILL_MIN_INFECT = 50;
        public const byte GP_ILL_MAX_INFECT = 100;
        public const byte GP_ILL_DEF_INFECT = 80;
        public const byte GP_MAX_BOTS = 5;
        public const byte GP_MIN_BOTS = 0;

        public const double GP_CAPSID_LIFESPAN_SECS = 30.0;
        public const float GP_CAPSID_MAXVEL = 10f;
        public const double GP_IMMUNESYS_TIMEOUT_MINS = 0.6;
        public const double GP_IMMUNESYS_WARN_SECS = 5.0;
        public const double GP_MEDICATION_TIMEOUT_MINS = 2.3;
        public const double GP_MEDICATION_WARN_SECS = 10.0;

        public const float GP_WHITEBLOODCELL_ALERT_DIST = 60f;
        public const int GP_WHITEBLOODCELL_MAX_NUM = 15;
        public const byte GP_WHITE_BLOODCELL_VIRUS_ID = byte.MaxValue;

        public enum GameplayType
        {
            TIMED_MATCH,
            ILLNESS,
            LAST_STANDING
        }

        public static string[] GP_LEVELS_STR_ARRAY = { "Trial" };
        public static string[] GP_LEVELS_DESC_ARRAY = 
        {
            "Elevator pitch\n- Fun in a Cyst!"
        };

        public const float GP_CURSOR_LENGTH = 200f;

        public enum GameLevel : uint
        {
            TRIAL = 0,
            TUTORIAL = 1
        }

        #endregion

        #region AI stuff

        public const double AI_CLUSTER_UP_DELAY_SECS = 0.5;
        public const double AI_CLUST_THOUGHT_TIMEOUT_SECS = 1.0;
        public const double AI_VIR_THOUGHT_TIMEOUT_SECS = 5.0;

        public const double AI_IQ_EASY = 0.25;
        public const double AI_IQ_NORM = 0.5;
        public const double AI_IQ_HARD = 0.75;
        public const double AI_IQ_EXTREME = 1.0;

        #region fuzzy linguistic variable set thresholds

        #region istrue
        public const double AI_FLV_ISTRUE_YES_MIN = 0.0;
        public const double AI_FLV_ISTRUE_YES_PEAK = 1.0;
        public const double AI_FLV_ISTRUE_YES_MAX = 2.0;
        #endregion

        #region cluster power
        public const double AI_FLV_CLUST_POWER_WEAK_MIN = 0.0;
        public const double AI_FLV_CLUST_POWER_WEAK_PEAK = 3.0;
        public const double AI_FLV_CLUST_POWER_WEAK_MAX = 10.0;

        public const double AI_FLV_CLUST_POWER_STRONG_MIN = 3.0;
        public const double AI_FLV_CLUST_POWER_STRONG_PEAK = 10.0;
        public const double AI_FLV_CLUST_POWER_STRONG_MAX = 25.0;
        #endregion

        #region distance
        public const double AI_FLV_DIST_NEAR_MIN = 0.0;
        public const double AI_FLV_DIST_NEAR_PEAK = 50.0;
        public const double AI_FLV_DIST_NEAR_MAX = 75.0;

        public const double AI_FLV_DIST_FAR_MIN = 50.0;
        public const double AI_FLV_DIST_FAR_PEAK = 75.0;
        public const double AI_FLV_DIST_FAR_MAX = 100.0;
        #endregion

        #region cell count
        public const double AI_FLV_SCOUNT_LOW_MIN = 0.0;
        public const double AI_FLV_SCOUNT_LOW_PEAK = 2.0;
        public const double AI_FLV_SCOUNT_LOW_MAX = 5.0;

        public const double AI_FLV_SCOUNT_HIGH_MIN = 2.0;
        public const double AI_FLV_SCOUNT_HIGH_PEAK = 5.0;
        public const double AI_FLV_SCOUNT_HIGH_MAX = 20.0;

        public const double AI_FLV_OCOUNT_LOW_MIN = 0.0;
        public const double AI_FLV_OCOUNT_LOW_PEAK = 1.0;
        public const double AI_FLV_OCOUNT_LOW_MAX = 2.0;

        public const double AI_FLV_OCOUNT_HIGH_MIN = 1.0;
        public const double AI_FLV_OCOUNT_HIGH_PEAK = 2.0;
        public const double AI_FLV_OCOUNT_HIGH_MAX = 5.0;
        #endregion

        #region desirability variable

        public const double AI_CL_DESIRABLE_MIN = 25.0;
        public const double AI_CL_DESIRABLE_PEAK = 75.0;
        public const double AI_CL_DESIRABLE_MAX = 100.0;

        public const double AI_CL_UNDESIRABLE_MIN = 0.0;
        public const double AI_CL_UNDESIRABLE_PEAK = 25.0;
        public const double AI_CL_UNDESIRABLE_MAX = 75.0;

        #endregion

        #endregion

        #endregion

        #region bloom effect parameters

        public const float EFFECT_BLOOM_THRESHOLD = 0f;
        public const float EFFECT_BLUR_AMMOUNT = 3f;
        public const float EFFECT_BLOOM_INTENSITY = 1f;
        public const float EFFECT_BASE_INTENSITY = 1f;
        public const float EFFECT_BLOOM_SATURATION = 1f;
        public const float EFFECT_BASE_SATURATION = 1f;

        #endregion

        #region tutorial text

        public static string[] TUT_TITLES = 
        {
            "INTRODUCTION", "VIRUS CAPSID", "CELL CLUSTER",
            "NUTRIENTS", "CELL TYPES", "Red Blood Cells",
            "Platelet cells", "Tank cells", "Silo cells",
            "CHASE UNINFECTED CELL COMMAND", "ACTION SELECT OPTION",
            "ACTION SELECT OPTION", "DIVIDE CELLS", "DIVIDE CELLS",
            "SPLIT CELL CLUSTER", "HYBRID CELLS AND MEDICATION",
            "END TUTORIAL"
        };

        public static string[] TUT_TEXTS = 
        {
            "In Biophage, you take control of a virus infection within a blood stream.\n\n" + 
            "In doing so you will need to infect and divide blood cells so that your virus can\n" +
            "survive attacks from other viruses, immune system, and medication.",

            "At the start of a new game you control a virus capsid.\n\n" +
            "This virus capsid has been introduced into the blood stream from the outside and\n" +
            "needs you to manoeuvre it towards a blood cell. Once the virus capsid touches a blood\n" +
            "cell, it will infect that cell.\n\n" +
            "You must decide which blood cell type you want to infect first quickly otherwise the\n" +
            "virus capsid will dissolve and the game will end (a count down will indicate the time\n" +
            "you have left to achieve this task).\n\n" +
            "[A virus capsid is not an organic cell itself, but a complex protein structure used to\n" +
            "deliver virus DNA / RNA between cells]\n\n" +
            "You have 30 seconds to infect a cell, do so now.",

            "Infected cells belong in groups called 'cell clusters'.\n" +
            "In the game you control cell clusters with the following actions:\n\n" +
            "a) Divide cells (Mitosis): produces more cells in the cell cluster.\n\n" +
            "b) Chase uninfected cells: commands the cluster to 'hunt' down the uninfected cell\n" +
            "that you have chosen so that it may infect that cell and add it to the cluster.\n\n" +
            "c) Battle enemy cluster: commands the cluster to 'hunt' down an enemy cluster that\n" +
            "you have chosen to battle.\n\n" +
            "d) Evade enemy cluster: commands the cluster to 'evade' or 'retreat' from an enemy\n" +
            "cluster that you have chosen.\n\n" +
            "e) Split the cluster: forms a new cell cluster from the selected cluster. This allows\n" +
            "you to create many separate clusters that can be controlled independently from each\n" +
            "other.\n\n" +
            "f) Hybrid cells: Combines two different cell types into one hybrid cell type, more on\n" +
            "this later.",

            "The main economy in Biophage is nutrients. Nutrients are required to divide cells.\n\n" +
            "Each cell has a distinct nutrient income rate that gradually adds to the cell's nutrient\n" +
            "store. Cells get nutrients directly from the blood plasma and have a maximum nutrient\n" +
            "storage capacity.\n\n" +
            "Different cell types have different nutrient income rates, different maximum nutrient\n" +
            "storage capacities, and require different amounts of nutrients to divide. In a cluster,\n" +
            "nutrients are shared between all the cells.",

            "Different cell types exist in Biophage and each one has unique properties.",

            "Red blood cells are plentiful in the blood stream. They do not provide much battle\n" +
            "power individually, but they are cheap (in nutrients) to divide.\n\n" +
            "Red blood cells could be likened to a Pawn in Chess.",

            "Platelets are an aggressive cell type that are faster and have more battle power than\n" +
            "Red blood cells. However, they are not as plentiful in the blood stream and require\n" +
            "more nutrients to divide.",

            "Tanks are bigger and slower compared to Red blood cells and Platelets, but they\n" +
            "provide the greatest battle power than any other cell type (equivalent to ten times the\n" +
            "battle power of red blood cells). However, tanks require a lot of nutrients to divide.",

            "Silos are the biggest cell type in the game. They are also the slowest moving and the\n" +
            "most vulnerable in battle. However, due to their size, they can retain a greater amount\n" +
            "of nutrients and have a high nutrient income rate due to their large surface area.",

            "Now, let's command the cell cluster to infect a different type of cell to add it to the\n" +
            "cluster.\n\n" +
            "Enter the 'ACTION SELECT' option (X button) to bring up the cursor.",

            "In 'action select', you control a little white dot (called the cursor) that you can move\n" +
            "around the blood stream environment similar to the virus capsid.\n\n" +
            "Move the cursor over an uninfected cell until the 'INFECT' option appears, then\n" +
            "select to infect that cell (A button).",

            "Great, you have just mastered at giving your cell clusters commands.\n\n" +
            "Note that you command a cell cluster via the 'action select' option to battle or evade\n" +
            "enemy clusters, or to combine with a friendly cell cluster, in a similar manner to\n" +
            "infecting uninfected cells.",

            "Now, you will need to sure up the cell count in your cluster.\n\n" +
            "Enter the 'DIVIDE CELLS' option to bring up the DIVIDE CELLS menu.",

            "You can only divide cells if the cell type is in the cluster to divide from and that there\n" +
            "is enough nutrients in the cluster to permit the divide.\n\n" +
            "Try and divide some cells now.",

            "In the game you can control many cell clusters simultaneously.\n\n" +
            "At the moment you just have one cluster, so lets bring up the 'split cluster' menu to\n" +
            "separate this cluster's cells to form a second cluster.\n\n" +
            "Enter the 'SPLIT CLUSTER' option (B button).",

            "OK great, just one last topic, cell hybrids and medication.\n\n" +
            "If a cell cluster contains two or more different cell types, then you can hybrid two of\n" +
            "these cell types to form a 'hybrid' cell type.\n\n" +
            "Hybrid cells cannot divide but are 'immune' to medication.\n\n" +
            "After a gracious amount of time has elapsed, Medication will be administered into the\n" +
            "blood stream. This medication will eradicate all infected cells of the majority cell\n" +
            "type. If, for example, your virus infection consisted of only Platelet cells and Platelet\n" +
            "cells are the majority infected cell type, then your entire virus infection will be\n" +
            "eradicated by medication, ending the game.",

            "That is it...now go out there and make some puss!"
        };

        #endregion
    }
}
