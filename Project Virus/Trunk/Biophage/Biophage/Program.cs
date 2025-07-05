using System;
using System.Collections.Generic;

namespace Biophage
{
    /// <summary>
    /// OS entry class - aka 'main'.
    /// </summary>
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Game.BiophageGame game = new Game.BiophageGame())
            {
                game.Run();
            }
        }
    }
}
