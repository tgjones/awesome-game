using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;


namespace AwesomeGame.Vehicles
{
	public abstract class Car : Mesh
	{
		private struct CarControlState
		{
			public float Accel;
			public float Brake;
			public float Steer;
		}
		
		private int MESHIDX_REAR_AXLE;
		private int MESHIDX_FRONT_LEFT_WHEEL;
		private int MESHIDX_FRONT_RIGHT_WHEEL;

		private const float WHEELBASE_TRACK = 6;
		private const float WHEELBASE_LENGTH = 6;
		private const float FRONT_AXLE_POS = 3;
		private const float RIDE_HEIGHT = 0.5f;
		private const float SUSPENSION_TRAVEL = 1.5f;
		private const float ENGINE_TORQUE = 200;
		private const float FRONT_BRAKE_TORQUE = 300;
		private const float REAR_BRAKE_TORQUE = 200;
		private const float MAX_GRIP = 15.0f;
		private const float MASS = 20;
		private const float AERO_EFFICIENCY = 0.95f;

		private const int WHEEL_FL = 0;
		private const int WHEEL_FR = 0;
		private const int WHEEL_RL = 0;
		private const int WHEEL_RR = 0;
		
		public Vector3 velocity;
		public Vector3 rotation;

		private GameObject nextCheckpoint;
		private GameObject nextCheckpointArrow;

		private Cue checkpointCheer;
		protected Cue horn;

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
			this.GetService<Camera>().AddViewObject(this.nextCheckpoint);
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
				CarControlState controlState = GetControlState(PlayerIndex.One);

				// Map engine and brake forces
				Vector3[] wheelForces = new Vector3[4];
				wheelForces[WHEEL_RL].X += controlState.Accel * ENGINE_TORQUE / 2;
				wheelForces[WHEEL_RR].X += controlState.Accel * ENGINE_TORQUE / 2;
				wheelForces[WHEEL_RL].X -= controlState.Brake * ENGINE_TORQUE / 2;
				wheelForces[WHEEL_RR].X -= controlState.Brake * ENGINE_TORQUE / 2;
				/*wheelForces[WHEEL_FL].X -= controlState.Brake * FRONT_BRAKE_TORQUE;
				wheelForces[WHEEL_FR].X -= controlState.Brake * FRONT_BRAKE_TORQUE;
				wheelForces[WHEEL_RL].X -= controlState.Brake * REAR_BRAKE_TORQUE;
				wheelForces[WHEEL_RR].X -= controlState.Brake * REAR_BRAKE_TORQUE;*/
				
				// Check vehicle is in contact with the ground
				Vector3 acceleration = new Vector3();
				float groundHeight = GetService<Terrain.SimpleTerrain>().GetHeight(position.X, position.Z);
				if (position.Y <= groundHeight + SUSPENSION_TRAVEL)
				{
					// Evaluate the steering effect
					double speed = Math.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z);
					rotation.Y = -controlState.Steer * (float)(speed > 10.0f ? 10.0f : speed) / 200f;

					// Evaluate sideslip
					for (int i = 0; i < 4; i++)
					{
						float slipVelocity = velocity.X * (float)Math.Sin(orientation.Y) + velocity.Z * (float)Math.Cos(orientation.Y);
						wheelForces[i].Z -= MASS * slipVelocity;
					}

					// Evaluate grip limits
					for (int i = 0; i < 4; i++)
					{
						if (wheelForces[i].Z < -MAX_GRIP * MASS / 4)
							wheelForces[i].Z = -MAX_GRIP * MASS / 4;
						if (wheelForces[i].Z > MAX_GRIP * MASS / 4)
							wheelForces[i].Z = MAX_GRIP * MASS / 4;
					}

					// Evaluate the total acceleration
					for (int i = 0; i < 4; i++)
						acceleration += wheelForces[i] / MASS;

					velocity.X += deltaTime * (float)
						(acceleration.X * Math.Cos(orientation.Y) + acceleration.Z * Math.Sin(orientation.Y));
					velocity.Z += deltaTime * (float)
						(acceleration.Z * Math.Cos(orientation.Y) - acceleration.X * Math.Sin(orientation.Y));
				}

				// Deteriorate momentum
				rotation *= (float)Math.Pow(AERO_EFFICIENCY, deltaTime);
				velocity *= (float)Math.Pow(AERO_EFFICIENCY, deltaTime);

				orientation.Y += rotation.Y;
				velocity.Y -= 9.8f * deltaTime; // Gravity
				position += velocity * deltaTime;

				// Check we haven't fallen through
				groundHeight = GetService<Terrain.SimpleTerrain>().GetHeight(position.X, position.Z);
				if (groundHeight > position.Y)
				{
					// Work out the vector length
					double speed = Math.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z);
					speed = Math.Sqrt(speed * speed + velocity.Y * velocity.Y);
					
					// Redirect the vector to compensate for the collision
					velocity.Y += (groundHeight - position.Y) / deltaTime;
					double newSpeed = Math.Sqrt(velocity.X * velocity.X + velocity.Z * velocity.Z);
					newSpeed = Math.Sqrt(newSpeed * newSpeed + velocity.Y * velocity.Y);
					velocity *= (float)(speed / newSpeed);

					position.Y = groundHeight;
				}

				// Locate wheels
				bool[] freeWheels = new bool[4];
				float flHeight = GetGroundHeight(position, orientation.Y, true, true);
				float frHeight = GetGroundHeight(position, orientation.Y, true, false);
				float rlHeight = GetGroundHeight(position, orientation.Y, false, true);
				float rrHeight = GetGroundHeight(position, orientation.Y, false, false);
				if (freeWheels[WHEEL_FR] = (position.Y - frHeight) > SUSPENSION_TRAVEL) frHeight = position.Y - SUSPENSION_TRAVEL;
				if (freeWheels[WHEEL_FL] = (position.Y - flHeight) > SUSPENSION_TRAVEL) flHeight = position.Y - SUSPENSION_TRAVEL;
				if (freeWheels[WHEEL_RR] = (position.Y - rrHeight) > SUSPENSION_TRAVEL) rrHeight = position.Y - SUSPENSION_TRAVEL;
				if (freeWheels[WHEEL_RL] = (position.Y - rlHeight) > SUSPENSION_TRAVEL) rlHeight = position.Y - SUSPENSION_TRAVEL;

				// Locate body
				orientation.Z = (float)Math.Atan((flHeight + rlHeight - (frHeight + rrHeight)) / WHEELBASE_TRACK / 2)
					+ (float)Math.Asin(-acceleration.Z / MASS / WHEELBASE_TRACK);
				orientation.X = (float)Math.Atan((rlHeight + rrHeight - (flHeight + frHeight)) / WHEELBASE_LENGTH / 2)
					+ (float)Math.Asin(-acceleration.X / MASS / WHEELBASE_LENGTH);

				// Configure the render
				ApplyWheelTransform(MESHIDX_FRONT_LEFT_WHEEL, position.Y - ((flHeight + frHeight) / 2), controlState.Steer,
					(float)Math.Atan((flHeight - frHeight) / WHEELBASE_TRACK));
				ApplyWheelTransform(MESHIDX_FRONT_RIGHT_WHEEL, position.Y - ((flHeight + frHeight) / 2), controlState.Steer,
					(float)Math.Atan((flHeight - frHeight) / WHEELBASE_TRACK));
				ApplyWheelTransform(MESHIDX_REAR_AXLE, position.Y - ((rlHeight + rrHeight) / 2), 0.0f,
					(float)Math.Atan((rlHeight - rrHeight) / WHEELBASE_TRACK));
				ApplyBodyTransform(
					Matrix.CreateRotationZ(orientation.Z) *
					Matrix.CreateRotationX(orientation.X) *
					Matrix.CreateTranslation(0.0f, RIDE_HEIGHT, 0.0f)
				);
			}

			if (((AwesomeGame)this.Game).CheckForCollisions((Mesh)this, (Mesh)this.nextCheckpoint))
			{
				// If we hit the checkpoint
				this.GetService<Camera>().RemoveViewObject(this.nextCheckpoint);
				this.nextCheckpoint = this.GetService<Course>().getNextCheckpoint(nextCheckpoint);
				this.GetService<Camera>().AddViewObject(this.nextCheckpoint);

				// Play a sound!
				checkpointCheer = Sound.Play("Congrats");
			}

			this.nextCheckpointArrow.position = this.position;
			Vector3 toCheckpoint = this.nextCheckpoint.position - this.position;
			this.nextCheckpointArrow.position.Y += 15;

			if (toCheckpoint.Z >=0 )
				this.nextCheckpointArrow.orientation.Y = (float)Math.Atan(toCheckpoint.X / toCheckpoint.Z);
			else
				this.nextCheckpointArrow.orientation.Y = (float)Math.Atan(toCheckpoint.X / toCheckpoint.Z) + MathHelper.ToRadians(180);

			this.nextCheckpointArrow.Update(gameTime);


			base.Update(gameTime);
		}

		public virtual void PlayHorn()
		{
		}

		public override void Draw(GameTime gameTime)
		{
			this.nextCheckpointArrow.Draw(gameTime);
			base.Draw(gameTime);
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

		private CarControlState GetControlState(PlayerIndex playerIndex)
		{
			CarControlState controlState = new CarControlState();
			GamePadState padState = GamePad.GetState(playerIndex);
			KeyboardState keyState = Keyboard.GetState();

			// Right-trigger or keyboard-up to accelerate,
			// Left-trigger or keyboard-down to brake
			controlState.Accel = padState.Triggers.Right;
			controlState.Brake = padState.Triggers.Left;
			if (keyState.IsKeyDown(Keys.Up)) controlState.Accel = 1.0f;
			if (keyState.IsKeyDown(Keys.Down)) controlState.Brake = 1.0f;

			// Left-stick or keyboard left-right arrows to steer
			controlState.Steer = padState.ThumbSticks.Left.X;
			if (keyState.IsKeyDown(Keys.Right)) controlState.Steer += 1.0f;
			if (keyState.IsKeyDown(Keys.Left)) controlState.Steer -= 1.0f;

			return controlState;
		}
	}
}
