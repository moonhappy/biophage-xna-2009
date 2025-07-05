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

namespace Biophage.Game.Stages.Main
{
    /// <summary>
    /// Provides an intermediate scene whilest async' loading game level.
    /// </summary>
    public class LoadingScn : Scene
    {
        #region fields

        private uint m_gameLevelScnToLoadId = GlobalConstants.TRIAL_LVL_SCN_ID;
        private bool m_gameLevelScnLoadSubmited = false;
        private bool m_gameLevelScnIsLoaded = false;

        private Stages.Game.SessionDetails m_sessionDetails;
        private uint m_taskId;

        #endregion

        #region methods

        #region construction

        public LoadingScn(uint id,
                            DebugManager debugMgr, ResourceManager resourceMgr,
                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                            SpriteFontResHandle sceneFont,
                            Scene parent)
            : base(id, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
            Microsoft.Xna.Framework.Graphics.Color.Black, sceneFont,
            parent.Stage, parent, null)
        {
            m_camera = new CameraGObj(
                uint.MaxValue, debugMgr, resourceMgr, this, false,
                graphicsMgr.GraphicsDevice.DisplayMode.AspectRatio,
                new Microsoft.Xna.Framework.Vector3(0f, 0f, 1.3f),
                Microsoft.Xna.Framework.Vector3.Zero,
                Microsoft.Xna.Framework.Vector3.Up,
                45f, 1f, 10000.0f);
        }

        #endregion

        #region creation

        public static LoadingScn Create(DebugManager debugMgr, ResourceManager resourceMgr,
                                            Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                            Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                            Scene parent)
        {
            LoadingScn scn = new LoadingScn(
                GlobalConstants.LOADING_SCN_ID, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                parent);

            TextureAsset backgroundWallLoad = new TextureAsset(
                1, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "Background",
                debugMgr, resourceMgr, scn, true,
                graphicsMgr);

            TextureAsset loadingTitle = new TextureAsset(
                2, Microsoft.Xna.Framework.Vector3.Zero,
                1.92f, 1.08f, "Content\\MainStage", "LoadingTitle",
                debugMgr, resourceMgr, scn, true,
                graphicsMgr);

            return scn;
        }

        public void SceneLoadDetails(Stages.Game.SessionDetails sessionDetails)
        {
            //asserts
            m_debugMgr.Assert(sessionDetails != null,
                "LoadingScn:SetLoadDetails - sessionDetails is null.");
            uint scnToLoadId = GlobalConstants.TRIAL_LVL_SCN_ID + (uint)sessionDetails.gameLevel;
            m_debugMgr.Assert(Stage.SceneMgr.GetStage(GlobalConstants.GAME_STAGE_ID).GetChildScene(scnToLoadId) != null,
                "LoadingScn:SetLoadDetails - scene Id isn't valid.");

            //set the stage to load params
            m_gameLevelScnToLoadId = scnToLoadId;
            m_gameLevelScnLoadSubmited = false;
            //m_gameLevelScnLoaded = false;

            //finaly, set the session details
            m_sessionDetails = sessionDetails;
        }

        #endregion

        #region game loop

        public override void Input(Microsoft.Xna.Framework.GameTime gameTime,
                                    ref Microsoft.Xna.Framework.Input.GamePadState newGPState
#if !XBOX
, ref Microsoft.Xna.Framework.Input.KeyboardState newKBState
#endif
)
        {
            //do nothing
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //don't show main menu
            if (GetMenu.Active)
                GetMenu.Active = false;

            //local refs of scene to load
            Stage toLoadStage = Stage.SceneMgr.GetStage(GlobalConstants.GAME_STAGE_ID);
            Stages.Game.BiophageGameBaseScn toLoadScene = (Stages.Game.BiophageGameBaseScn)toLoadStage.GetChildScene(m_gameLevelScnToLoadId);

            AsyncTaskManager asyncMgr = Stage.SceneMgr.AsyncTaskMgr;

            //load the game level requested
            if (!m_gameLevelScnLoadSubmited)
            {
                //initialise first
                toLoadScene.Init();

                //note scene load task submited
                m_gameLevelScnLoadSubmited = true;
                m_taskId = Stage.SceneMgr.AsyncTaskMgr.SubmitTask<Stages.Game.SetSessionAsyncTask>(
                    new AsyncSceneLoadParam(toLoadScene, m_sessionDetails));
                m_gameLevelScnIsLoaded = false;
            }
            else if (!m_gameLevelScnIsLoaded)
            {
                //see if scne has completed being loaded
                LinkedList<AsyncReturnPackage> retPackages = asyncMgr.ConsumeReturn(m_taskId);
                if (retPackages != null)
                {
                    foreach (AsyncReturnPackage retPack in retPackages)
                    {
                        if (retPack.ReturnData is AsyncSceneLoadReturn)
                            continue;
                        else
                            m_gameLevelScnIsLoaded = true;
                    }
                }
            }
            //all done? change to level
            else
            {
                //next, set the load scene as the current in its stage
                toLoadStage.SetCurrentScene(m_gameLevelScnToLoadId);
                //change current stage to load stage
                Stage.SceneMgr.SetCurrentStage(GlobalConstants.GAME_STAGE_ID);

                //set the session details
                //Stages.Game.BiophageGameBaseScn lvlScn = (Stages.Game.BiophageGameBaseScn)Stage.SceneMgr
                //    .CurrentStage.CurrentScene;
                //lvlScn.SetSession(m_sessionDetails);

                //allow next loads to occur
                m_gameLevelScnLoadSubmited = false;
                m_gameLevelScnIsLoaded = false;
            }
        }

        public override void PostUpdate(Microsoft.Xna.Framework.GameTime gameTime)
        {
            //do nothing
        }

        #endregion

        #endregion
    }
}
