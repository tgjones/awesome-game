using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AwesomeGame.Vehicles
{
	public class Blocky : Car
	{
		public Blocky(Game game) : base(game, @"Models\Lessblockycar2", 5, 7, 6) { }
	}

	public class Curvy : Car
	{
		public Curvy(Game game) : base(game, @"Models\Curvycar", 6, 3, 0) { }
	}

	public class SchoolBus : Car
	{
		public SchoolBus(Game game) : base(game, @"Models\Schoolbus", 6, 5, 4) { }
	}
}
