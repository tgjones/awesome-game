using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AwesomeGame.Vehicles
{
	public class Car : Mesh
	{
		private const int MESHIDX_REAR_AXLE = 5;
		private const int MESHIDX_FRONT_LEFT_WHEEL = 7;
		private const int MESHIDX_FRONT_RIGHT_WHEEL = 6;

		private const float WHEELBASE_TRACK = 6;
		private const float WHEELBASE_LENGTH = 6;
		private const float FRONT_AXLE_POS = 3;
		private const float RIDE_HEIGHT = 1.5f;
		
		public Vector3 velocity;

		public Car(Game game)
			: base(game, @"Models\Lessblockycar2", Matrix.CreateRotationY(MathHelper.ToRadians(90)))
		{

		}

		public override void Update(GameTime gameTime)
		{
			if (gameTime.ElapsedGameTime > TimeSpan.Zero)
			{
				float deltaTime = (float) (1.0f / gameTime.ElapsedGameTime.TotalMilliseconds);
				Vector2 controlState = GetControlState(PlayerIndex.One);

				//check that we are on/near the ground
				float landHeight = RIDE_HEIGHT + this.GetService<Terrain.SimpleTerrain>().GetHeight(position.X, position.Z);

				//stick it on the terrain
				if (position.Y > landHeight)
					velocity.Y -= (float)deltaTime * (float)deltaTime * 9.8f * 9.8f;
				if (position.Y < landHeight)
					position.Y = landHeight;

				double acceleration = 0;
				if (position.Y <= landHeight + 0.01f)
				{
					acceleration = controlState.X * 1.0f;
					if (acceleration < 0.0f) acceleration *= 3.0f;
					orientation.Y -= controlState.Y * 0.03f;
				}

				double speed = Math.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z);
				speed += acceleration * deltaTime;

				velocity.X = (float)(speed * Math.Cos(orientation.Y));
				velocity.Z = -(float)(speed * Math.Sin(orientation.Y));

				Vector3 newPosition = position + velocity * deltaTime;
				float newLandHeight = this.GetService<Terrain.SimpleTerrain>().GetHeight(newPosition.X, newPosition.Z);
				if (newLandHeight < landHeight + 1.5)
					// Not a huge rise, we can make it up
					position = newPosition;
				else
					velocity = Vector3.Zero;

				// Locate front left wheel position
				float flHeight = this.GetService<Terrain.SimpleTerrain>().GetHeight(
					(float)(position.X
					+ FRONT_AXLE_POS * Math.Cos(orientation.Y)
					- WHEELBASE_TRACK * Math.Sin(orientation.Y) / 2),
					(float)(position.Z
					- FRONT_AXLE_POS * Math.Sin(orientation.Y)
					+ WHEELBASE_TRACK * Math.Cos(orientation.Y) / 2));

				// Locate front right wheel position
				float frHeight = this.GetService<Terrain.SimpleTerrain>().GetHeight(
					(float)(position.X
					+ FRONT_AXLE_POS * Math.Cos(orientation.Y)
					- WHEELBASE_TRACK * Math.Sin(orientation.Y) / -2),
					(float)(position.Z
					- FRONT_AXLE_POS * Math.Sin(orientation.Y)
					+ WHEELBASE_TRACK * Math.Cos(orientation.Y) / -2));

				// Locate rear wheel positions
				float rHeight = this.GetService<Terrain.SimpleTerrain>().GetHeight(
					(float)(position.X + (FRONT_AXLE_POS - WHEELBASE_LENGTH) * Math.Cos(orientation.Y)),
					(float)(position.Z - (FRONT_AXLE_POS - WHEELBASE_LENGTH) * Math.Sin(orientation.Y)));

				ApplyWheelTransform(MESHIDX_REAR_AXLE, position.Y - rHeight, 0.0f);
				ApplyWheelTransform(MESHIDX_FRONT_LEFT_WHEEL, position.Y - flHeight, controlState.Y);
				ApplyWheelTransform(MESHIDX_FRONT_RIGHT_WHEEL, position.Y - frHeight, controlState.Y);
				ApplyBodyTransform(
					Matrix.CreateRotationZ((float)Math.Atan((flHeight - frHeight)) / WHEELBASE_TRACK) *
					Matrix.CreateRotationX((float)Math.Atan((rHeight - (flHeight + frHeight) / 2) / WHEELBASE_LENGTH)));
			}

			position.Y = this.GetService<Terrain.SimpleTerrain>().GetHeight(position.X, position.Z);
			base.Update(gameTime);
		}

		private void ApplyBodyTransform(Matrix transform)
		{
			_partTransformationMatrices[0] =
			_partTransformationMatrices[1] =
			_partTransformationMatrices[2] =
			_partTransformationMatrices[3] =
			_partTransformationMatrices[4] = transform;
		}

		private void ApplyWheelTransform(int index, float height, float steer)
		{
			_partTransformationMatrices[index] =
				Matrix.CreateTranslation(0.0f, -height, 0.0f) *
				Matrix.CreateRotationY(steer * -0.2f);
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
