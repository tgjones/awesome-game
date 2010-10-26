using System;

namespace AwesomeGame
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
					using (AwesomeGame game = new AwesomeGame())
					{
						game.Run();
					}
        }
    }
#endif
}

