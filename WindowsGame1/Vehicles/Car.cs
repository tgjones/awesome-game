using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AwesomeGame.Vehicles
{
	public class Car : Mesh
	{
		public Vector3 velocity;

		public Car(Game game)
			: base(game, @"Models\Lessblockycar", 1, Matrix.CreateRotationY(MathHelper.ToRadians(270)))
		{

		}

		public override void Update(GameTime gameTime)
		{
			if (gameTime.ElapsedGameTime > TimeSpan.Zero)
			{
				float deltaTime = (float) (1.0f / gameTime.ElapsedGameTime.TotalMilliseconds);
				GamePadState state = GamePad.GetState(PlayerIndex.One);

				Vector3 acceleration = Vector3.Zero;
				acceleration.X = (state.Triggers.Left - state.Triggers.Right) * 0.5f;

				velocity += acceleration * deltaTime;
				position += velocity * deltaTime;
			}

			base.Update(gameTime);
		}
	}
}
