using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AwesomeGame.Vehicles
{
	public class Car : Mesh
	{
		public Vector3 velocity;

		public Car(Game game)
			: base(game, @"Models\Lessblockycar", 1, Matrix.CreateRotationX(MathHelper.ToRadians(180)) * Matrix.CreateRotationY(MathHelper.ToRadians(270)))
		{

		}

		public override void Update(GameTime gameTime)
		{
			if (gameTime.ElapsedGameTime > TimeSpan.Zero)
			{
				float deltaTime = (float) (1.0f / gameTime.ElapsedGameTime.TotalMilliseconds);
				Vector2 controlState = GetControlState(PlayerIndex.One);

				double acceleration = controlState.X * 1.0f;

				double speed = Math.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z);
				speed += acceleration * deltaTime;

				velocity.X = (float)(speed * Math.Cos(orientation.Y));
				velocity.Z = -(float)(speed * Math.Sin(orientation.Y));

				position += velocity * deltaTime;

				orientation.Y -= controlState.Y * 0.03f;
			}

			base.Update(gameTime);
		}

		public Vector2 GetControlState(PlayerIndex playerIndex)
		{
			Vector2 controlState = new Vector2();
			GamePadState padState = GamePad.GetState(playerIndex);
			KeyboardState keyState = Keyboard.GetState();

			// Right-trigger or keyboard-up to accelerate,
			// Left-trigger or keyboard-down to brake
			controlState.X = padState.Triggers.Right - padState.Triggers.Left;
			if (keyState.IsKeyDown(Keys.Up)) controlState.X += 1.0f;
			if (keyState.IsKeyDown(Keys.Down)) controlState.X -= 1.0f;

			// Left-stick or keyboard left-right arrows to steer
			controlState.Y = padState.ThumbSticks.Left.X;
			if (keyState.IsKeyDown(Keys.Right)) controlState.Y += 1.0f;
			if (keyState.IsKeyDown(Keys.Left)) controlState.Y -= 1.0f;

			return controlState;
		}
	}
}
