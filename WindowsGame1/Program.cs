using System;

namespace AwesomeGame
{
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
}