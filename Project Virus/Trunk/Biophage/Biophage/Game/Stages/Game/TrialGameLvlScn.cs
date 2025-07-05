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

namespace Biophage.Game.Stages.Game
{
    /// <summary>
    /// The trial Biophage game level.
    /// </summary>
    public class TrialGameLvlScn : BiophageGameBaseScn
    {
        #region methods

        #region construction

        public TrialGameLvlScn( uint id,
                                DebugManager debugMgr, ResourceManager resourceMgr,
                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                SpriteFontResHandle sceneFont,
                                Scene parent)
            : base(id, debugMgr, resourceMgr, graphicsMgr, spriteBatch, sceneFont, parent)
        {
            //set locations where virus capsids can spawn - do uniform random locations
            m_virusCapsidSpawnLocations = new Microsoft.Xna.Framework.Vector3[4];

            Random rnd = new Random(0);

            m_virusCapsidSpawnLocations[0] = NewRndSpawnPos(rnd);
            m_virusCapsidSpawnLocations[1] = NewRndSpawnPos(rnd);
            m_virusCapsidSpawnLocations[2] = NewRndSpawnPos(rnd);
            m_virusCapsidSpawnLocations[3] = NewRndSpawnPos(rnd);

            //white blood cells should be born from the top
            m_whiteBloodCellSpawnLocations = new Microsoft.Xna.Framework.Vector3[4];
            m_whiteBloodCellSpawnLocations[0] = NewRndSpawnPos(rnd) * 0.5f;
            m_whiteBloodCellSpawnLocations[1] = NewRndSpawnPos(rnd) * 0.5f;
            m_whiteBloodCellSpawnLocations[2] = NewRndSpawnPos(rnd) * 0.5f;
            m_whiteBloodCellSpawnLocations[3] = NewRndSpawnPos(rnd) * 0.5f;
        }

        private Microsoft.Xna.Framework.Vector3 NewRndSpawnPos(Random rnd)
        {
            Microsoft.Xna.Framework.Vector3 newPos = new Microsoft.Xna.Framework.Vector3(
                (float)rnd.NextDouble() * ((rnd.Next(2) == 0) ? -1f : 1f),
                (float)rnd.NextDouble() * ((rnd.Next(2) == 0) ? -1f : 1f),
                (float)rnd.NextDouble() * ((rnd.Next(2) == 0) ? -1f : 1f));

            if (newPos.Length() != 0)
                newPos.Normalize();

            newPos *= rnd.Next(90, 95);
            return newPos;
        }

        #endregion

        #region creation

        public static TrialGameLvlScn Create(DebugManager debugMgr, ResourceManager resourceMgr,
                                                Microsoft.Xna.Framework.GraphicsDeviceManager graphicsMgr,
                                                Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch,
                                                Scene parent)
        {
            TrialGameLvlScn scn = new TrialGameLvlScn(
                GlobalConstants.TRIAL_LVL_SCN_ID, debugMgr, resourceMgr, graphicsMgr, spriteBatch,
                new SpriteFontResHandle(debugMgr, resourceMgr, "Content\\Fonts\\", "PromptFont"),
                parent);

            return scn;
        }

        #endregion

        #region initialisation

        public override bool Init()
        {
            bool retVal = true;
            if (!m_isInit)
            {
                CreatePhysics();

                m_physicsWorld.Gravity = new Microsoft.Xna.Framework.Vector3(0f, 0f, 0f);

                Common.LevelEnvironment env = new Common.LevelEnvironment(
                    m_levelEnvId,
                    new ModelResHandle(m_debugMgr, m_resMgr, "Content\\Models\\Levels\\", "TrialLevel"),
                    m_debugMgr, m_resMgr, this,
                    new JigLibX.Collision.MaterialProperties(0.8f, 0.8f, 0.7f),
                    100f, Microsoft.Xna.Framework.Vector3.Zero);

                //set psuedo random cell positions
                Random rnd = new Random(0);
                Microsoft.Xna.Framework.Vector3 cellLoc;

                //uninfected cells - 10 rbcs above one another
                Common.Cells.UninfectedCell uCell;
                for (byte i = 0; i < 10; i++)
                {
                    cellLoc = new Microsoft.Xna.Framework.Vector3(
                        (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                    if (cellLoc.Length() != 0)
                        cellLoc.Normalize();
                    cellLoc *= rnd.Next(-50, 50);

                    uCell = new Common.Cells.UninfectedCell(
                        UCellGobjID(i),
                        m_rbcStaticData,
                        new ModelResHandle(m_debugMgr, m_resMgr, "Content\\Models\\RedBloodCell\\", "RedBloodCell"),
                        m_debugMgr, m_resMgr, this, true, cellLoc,
                        Microsoft.Xna.Framework.Quaternion.Identity,
                        new JigLibX.Geometry.Box(cellLoc, Microsoft.Xna.Framework.Matrix.Identity,
                            new Microsoft.Xna.Framework.Vector3(2f,0.5f,2f)),
                        new JigLibX.Collision.MaterialProperties(0.8f,0.8f,0.7f),
                        1f);

                    m_uninfectedCells.AddLast(i);
                    m_ucellObjs.AddLast(uCell);
                }
                for (byte i = 10; i < 20; i++)
                {
                    cellLoc = new Microsoft.Xna.Framework.Vector3(
                        (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                    if (cellLoc.Length() != 0)
                        cellLoc.Normalize();
                    cellLoc *= rnd.Next(-50, 50);

                    uCell = new Common.Cells.UninfectedCell(
                        UCellGobjID(i),
                        m_pcStaticData,
                        new ModelResHandle(m_debugMgr, m_resMgr, "Content\\Models\\Platelet\\", "Platelet"),
                        m_debugMgr, m_resMgr, this, true, cellLoc,
                        Microsoft.Xna.Framework.Quaternion.Identity,
                        new JigLibX.Geometry.Box(cellLoc, Microsoft.Xna.Framework.Matrix.Identity,
                            new Microsoft.Xna.Framework.Vector3(2f, 0.5f, 2f)),
                        new JigLibX.Collision.MaterialProperties(0.8f, 0.8f, 0.7f),
                        1f);

                    m_uninfectedCells.AddLast(i);
                    m_ucellObjs.AddLast(uCell);
                }
                for (byte i = 20; i < 25; i++)
                {
                    cellLoc = new Microsoft.Xna.Framework.Vector3(
                        (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                    if (cellLoc.Length() != 0)
                        cellLoc.Normalize();
                    cellLoc *= rnd.Next(-70, 70);

                    uCell = new Common.Cells.UninfectedCell(
                        UCellGobjID(i),
                        m_siloStaticData,
                        new ModelResHandle(m_debugMgr, m_resMgr, "Content\\Models\\Silo\\", "Silo"),
                        m_debugMgr, m_resMgr, this, true, cellLoc,
                        Microsoft.Xna.Framework.Quaternion.Identity,
                        new JigLibX.Geometry.Sphere(cellLoc, 4f),
                        new JigLibX.Collision.MaterialProperties(0.8f, 0.8f, 0.7f),
                        4f);

                    m_uninfectedCells.AddLast(i);
                    m_ucellObjs.AddLast(uCell);
                }
                for (byte i = 25; i < 30; i++)
                {
                    cellLoc = new Microsoft.Xna.Framework.Vector3(
                        (float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble());
                    if (cellLoc.Length() != 0)
                        cellLoc.Normalize();
                    cellLoc *= rnd.Next(-70, 70);

                    uCell = new Common.Cells.UninfectedCell(
                        UCellGobjID(i),
                        m_tankStaticData,
                        new ModelResHandle(m_debugMgr, m_resMgr, "Content\\Models\\Tank\\", "Tank"),
                        m_debugMgr, m_resMgr, this, true, cellLoc,
                        Microsoft.Xna.Framework.Quaternion.Identity,
                        new JigLibX.Geometry.Sphere(cellLoc, 2f),
                        new JigLibX.Collision.MaterialProperties(0.8f, 0.8f, 0.7f),
                        2f);

                    m_uninfectedCells.AddLast(i);
                    m_ucellObjs.AddLast(uCell);
                }

                if (!base.Init())
                    retVal = false;

                if (retVal)
                    m_isInit = true;
                else
                    m_isInit = false;
            }
            return retVal;
        }

        #endregion

        #region game loop

        #endregion

        #endregion
    }
}
