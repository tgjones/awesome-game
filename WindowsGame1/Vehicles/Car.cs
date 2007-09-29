using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AwesomeGame.Vehicles
{
	public abstract class Car : Mesh
	{
		private int MESHIDX_REAR_AXLE;
		private int MESHIDX_FRONT_LEFT_WHEEL;
		private int MESHIDX_FRONT_RIGHT_WHEEL;

		private const float WHEELBASE_TRACK = 6;
		private const float WHEELBASE_LENGTH = 6;
		private const float FRONT_AXLE_POS = 3;
		private const float RIDE_HEIGHT = 1;
		private const float SUSPENSION_TRAVEL = 3;
		
		public Vector3 velocity;

		private GameObject nextCheckpoint;
		private GameObject nextCheckpointArrow;

		public Car(Game game, string modelName, int idxRearAxle, int idxFrontLeftWheel, int idxFrontRightWheel)
			: base(game, modelName, Matrix.CreateRotationY(MathHelper.ToRadians(90)))
		{
			MESHIDX_FRONT_LEFT_WHEEL = idxFrontLeftWheel;
			MESHIDX_FRONT_RIGHT_WHEEL = idxFrontRightWheel;
			MESHIDX_REAR_AXLE = idxRearAxle;
		}

		public override void Initialize()
		{
			base.Initialize();

			nextCheckpoint = this.GetService<Course>().getFirstCheckpoint();
		}

		public void setNextCheckpointArrow(GameObject arrow)
		{
			this.nextCheckpointArrow = arrow;
		}

		public override void Update(GameTime gameTime)
		{
			if (gameTime.ElapsedGameTime > TimeSpan.Zero)
			{
				float deltaTime = (float) (1.0f / gameTime.ElapsedGameTime.TotalMilliseconds);
				Vector2 controlState = GetControlState(PlayerIndex.One);

				//check that we are on/near the ground
				float landHeight = this.GetService<Terrain.SimpleTerrain>().GetHeight(position.X, position.Z);

				//stick it on the terrain
				if (position.Y > landHeight)
					velocity.Y -= (float)deltaTime * (float)deltaTime * 9.8f * 9.8f;
				if (position.Y < landHeight)
					position.Y = landHeight;

				double speed = Math.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z);
				double acceleration = 0;

				if (position.Y <= landHeight + 0.01f)
				{
					acceleration = controlState.X * 1.0f;
					if (acceleration < 0.0f) acceleration *= 3.0f;

					orientation.Y -= controlState.Y * (float)(speed > 10.0f ? 10.0f : speed) / 200f;
				}

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

				// Locate wheels
				float flHeight = GetGroundHeight(position, orientation.Y, true, true);
				float frHeight = GetGroundHeight(position, orientation.Y, true, false);
				float rlHeight = GetGroundHeight(position, orientation.Y, false, true);
				float rrHeight = GetGroundHeight(position, orientation.Y, false, false);
				if ((flHeight - frHeight) > SUSPENSION_TRAVEL) frHeight = flHeight - SUSPENSION_TRAVEL;
				if ((frHeight - flHeight) > SUSPENSION_TRAVEL) flHeight = frHeight - SUSPENSION_TRAVEL;
				if ((rlHeight - rrHeight) > SUSPENSION_TRAVEL) rrHeight = rlHeight - SUSPENSION_TRAVEL;
				if ((rrHeight - rlHeight) > SUSPENSION_TRAVEL) rlHeight = rrHeight - SUSPENSION_TRAVEL;

				// Configure the render
				ApplyWheelTransform(MESHIDX_FRONT_LEFT_WHEEL, position.Y - ((flHeight + frHeight) / 2), controlState.Y,
					(float)Math.Atan((flHeight - frHeight) / WHEELBASE_TRACK));
				ApplyWheelTransform(MESHIDX_FRONT_RIGHT_WHEEL, position.Y - ((flHeight + frHeight) / 2), controlState.Y,
					(float)Math.Atan((flHeight - frHeight) / WHEELBASE_TRACK));
				ApplyWheelTransform(MESHIDX_REAR_AXLE, position.Y - ((rlHeight + rrHeight) / 2), 0.0f,
					(float)Math.Atan((rlHeight - rrHeight) / WHEELBASE_TRACK));
				ApplyBodyTransform(
					Matrix.CreateRotationZ((float)Math.Atan((flHeight + rlHeight - (frHeight + rrHeight)) / WHEELBASE_TRACK / 2)) *
					Matrix.CreateRotationX((float)Math.Atan((rlHeight + rrHeight - (flHeight + frHeight)) / WHEELBASE_LENGTH / 2)) *
					Matrix.CreateTranslation(0.0f, RIDE_HEIGHT, 0.0f)
				);
			}

			if (false)
			{
				//we hit the checkpoint
				//nextCheckpoint = this.GetService<Course>().getNextCheckpoint(nextCheckpoint);
			}

			nextCheckpointArrow.position = this.position;
			nextCheckpointArrow.position.Y += 15;

			Vector3 toCheckpoint = this.position - nextCheckpoint.position;
			//nextCheckpointArrow.orientation.Y = (float)Math.Atan(toCheckpoint.Z / toCheckpoint.X);
			//nextCheckpointArrow.position = Vector3.Lerp(this.position, nextCheckpoint.position, 1f);
			//nextCheckpointArrow.position = Vector3.Clamp(this.position, nextCheckpointArrow.position + new Vector3(5.0f, 5.0f, 5.0f), nextCheckpointArrow.position - new Vector3(5.0f, 5.0f, 5.0f));
			nextCheckpointArrow.position = nextCheckpoint.position;

			nextCheckpointArrow.position.Y += 35;
			//nextCheckpointArrow.position = 
			//Matrix dif = Matrix.Identity;
			//dif = Vector3.Transform(this.position, 
			//nextCheckpointArrow.position.Y = this.position.Y + 15;

			//nextCheckpointArrow.Update(gameTime);

			base.Update(gameTime);
		}

		private float GetGroundHeight(Vector3 pos, float ori, bool front, bool left)
		{
			float x = FRONT_AXLE_POS;
			if (!front) x -= WHEELBASE_LENGTH;

			float y = WHEELBASE_TRACK / 2;
			if (left) y *= -1;

			return this.GetService<Terrain.SimpleTerrain>().GetHeight(
				(float)(pos.X
				+ x * Math.Cos(ori)
				+ y * Math.Sin(ori)),
				(float)(pos.Z
				- x * Math.Sin(ori)
				+ y * Math.Cos(ori)));
		}

		private void ApplyBodyTransform(Matrix transform)
		{
			for (int i = 0; i < _partTransformationMatrices.Count; i++)
				if (i != MESHIDX_REAR_AXLE &&
					i != MESHIDX_FRONT_LEFT_WHEEL &&
					i != MESHIDX_FRONT_RIGHT_WHEEL)
					_partTransformationMatrices[i] = transform;
		}

		private void ApplyWheelTransform(int index, float height, float steer, float yaw)
		{
			_partTransformationMatrices[index] =
				Matrix.CreateRotationZ(yaw) *
				Matrix.CreateTranslation(steer != 0 ? -FRONT_AXLE_POS : 0, 0, 0) *
				Matrix.CreateRotationY(steer * -0.1f) *
				Matrix.CreateTranslation(steer != 0 ? FRONT_AXLE_POS : 0, -height, 0);
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
