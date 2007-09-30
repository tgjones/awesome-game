using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;


namespace AwesomeGame.Vehicles
{
	public abstract class Car : Mesh
	{
		private struct CarControlState
		{
			public float Accel;
			public float Brake;
			public float Steer;
			public bool Horn;
			public bool Insult;
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
		private const float MAX_GRIP = 10.0f;
		private const float MASS = 20;
		private const float AERO_EFFICIENCY = 0.95f;

		private const int WHEEL_FL = 0;
		private const int WHEEL_FR = 1;
		private const int WHEEL_RL = 2;
		private const int WHEEL_RR = 3;

		private PlayerIndex playerIndex;
		private CarControlState previousControlState;
		public Vector3 velocity;
		public Vector3 rotation;

		private GameObject nextCheckpoint;
		private GameObject nextCheckpointArrow;

		private Cue checkpointCheer;
		protected Cue horn;
		private Cue insult;

		private SpriteBatch _spriteBatch;
		private SpriteFont _titleFont;
		private string _messageToDraw;

		public Car(Game game, PlayerIndex player, string modelName, int idxRearAxle, int idxFrontLeftWheel, int idxFrontRightWheel)
			: base(game, modelName, Matrix.CreateRotationY(MathHelper.ToRadians(90)))
		{
			MESHIDX_FRONT_LEFT_WHEEL = idxFrontLeftWheel;
			MESHIDX_FRONT_RIGHT_WHEEL = idxFrontRightWheel;
			MESHIDX_REAR_AXLE = idxRearAxle;
			playerIndex = player;
		}

		public override void Initialize()
		{
			base.Initialize();

			nextCheckpoint = this.GetService<Course>().getFirstCheckpoint();
			//this.GetService<Camera>().AddViewObject(this.nextCheckpoint);
		}

		public void setNextCheckpointArrow(GameObject arrow)
		{
			this.nextCheckpointArrow = arrow;
		}

		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			base.LoadGraphicsContent(loadAllContent);
			if (loadAllContent)
			{
				_spriteBatch = new SpriteBatch(this.GraphicsDevice);
				_titleFont = GetService<ContentManager>().Load<SpriteFont>(@"Fonts\VictoryFont");
			}
		}

		public override void Update(GameTime gameTime)
		{
			if (gameTime.ElapsedGameTime > TimeSpan.Zero)
			{
				float deltaTime = (float) (1.0f / gameTime.ElapsedGameTime.TotalMilliseconds);
				CarControlState controlState = GetControlState(playerIndex, deltaTime);

				// Map engine and brake forces
				Vector3[] wheelForces = new Vector3[4];
				wheelForces[WHEEL_RL].X += controlState.Accel * ENGINE_TORQUE / 2;
				wheelForces[WHEEL_RR].X += controlState.Accel * ENGINE_TORQUE / 2;
				wheelForces[WHEEL_FL].X -= controlState.Brake * ENGINE_TORQUE / 4;
				wheelForces[WHEEL_FR].X -= controlState.Brake * ENGINE_TORQUE / 4;
				wheelForces[WHEEL_RL].X -= controlState.Brake * ENGINE_TORQUE / 4;
				wheelForces[WHEEL_RR].X -= controlState.Brake * ENGINE_TORQUE / 4;
				
				// Steer the wheels
				float[] wheelYaws = new float[4];
				wheelYaws[WHEEL_FL] = wheelYaws[WHEEL_FR] = (float)Math.Asin(controlState.Steer);

				// Deteriorate momentum
				rotation *= (float)Math.Pow(AERO_EFFICIENCY, deltaTime);
				velocity *= (float)Math.Pow(AERO_EFFICIENCY, deltaTime);

				// Check vehicle is in contact with the ground
				Vector3 acceleration = new Vector3();
				float groundHeight = GetService<Terrain.SimpleTerrain>().GetHeight(position.X, position.Z);
				if (position.Y <= groundHeight + SUSPENSION_TRAVEL)
				{
					// Evaluate sideslip
					for (int i = 0; i < 4; i++)
					{
						float slipVelocity = velocity.X * (float)Math.Sin(orientation.Y + wheelYaws[i])
											+ velocity.Z * (float)Math.Cos(orientation.Y + wheelYaws[i]);

						wheelForces[i].Z -= MASS *
							(slipVelocity +
							((float)Math.Sin(rotation.Y) * MASS * WHEELBASE_LENGTH / (i > 1 ? -2 : 2)));
					}

					// Evaluate grip limits
					for (int i = 0; i < 4; i++)
						if (wheelForces[i].Length() > MAX_GRIP * MASS / 4)
							wheelForces[i] *= MAX_GRIP * MASS / 4 / wheelForces[i].Length();

					// Evaluate the total acceleration
					for (int i = 0; i < 4; i++)
						acceleration += wheelForces[i] / MASS;

					// Evaluation the total rotational acceleration
					rotation.Y += deltaTime / 500.0f * (
						(wheelForces[WHEEL_FL].Z + wheelForces[WHEEL_FR].Z) * WHEELBASE_LENGTH / 2 -
						(wheelForces[WHEEL_RL].Z + wheelForces[WHEEL_RR].Z) * WHEELBASE_LENGTH / 2);

					velocity.X += deltaTime * (float)
						(acceleration.X * Math.Cos(orientation.Y) + acceleration.Z * Math.Sin(orientation.Y));
					velocity.Z += deltaTime * (float)
						(acceleration.Z * Math.Cos(orientation.Y) - acceleration.X * Math.Sin(orientation.Y));
				}

				foreach (GameComponent anObject in this.Game.Components)
				{
					if (anObject.Equals(this))
						continue;
					if (anObject is GameObject)
					{
						if (((GameObject)anObject).collidable)
						{
							if (((AwesomeGame)this.Game).CheckForCollisions((Mesh)this, (Mesh)anObject))
							{
								// We've found an object in the list if things,
								// We've hit it
								// It can be hit
								Vector3 direction = this.position - ((Mesh)anObject).BoundingSphere.Center;
								if (((Mesh)anObject).BoundingSphere.Contains(this.BoundingSphere) > 0)
								{
									// Help!  We're stuck inside the bounding sphere!  Get us out!
									direction.Normalize();
									direction *= ((Mesh)anObject).BoundingSphere.Radius + this.BoundingSphere.Radius;
									this.position = ((Mesh)anObject).BoundingSphere.Center + direction;
								}

								// Bounce off
								velocity = Vector3.Reflect(velocity, Vector3.Normalize(direction));
							}
						}
					}
				}

				orientation.Y += rotation.Y;
				velocity.Y -= 9.8f * deltaTime; // Gravity
				position += velocity * deltaTime;

				// Check we haven't fallen through
				groundHeight = GetService<Terrain.SimpleTerrain>().GetHeight(position.X, position.Z);
				if (groundHeight > position.Y)
				{
					// Work out the change the normal force would have produced
					float speed = velocity.Length();
					Vector3 normal = GetService<Terrain.SimpleTerrain>().GetNormal(position.X, position.Z);
					Vector3 positionChange = normal * (groundHeight - position.Y) / normal.Y;

					// Apply the change retrospectively
					velocity += positionChange / deltaTime;
					position += positionChange;

					// Limit the effect of this when on unrealistic inclines
					if (velocity.Length() > speed)
						velocity *= speed / velocity.Length();
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
					+ 3.0f * (float)Math.Asin(-acceleration.Z / MASS / WHEELBASE_TRACK);
				orientation.X = (float)Math.Atan((rlHeight + rrHeight - (flHeight + frHeight)) / WHEELBASE_LENGTH / 2)
					+ 3.0f * (float)Math.Asin(-acceleration.X / MASS / WHEELBASE_LENGTH);

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

				if (controlState.Horn)
					this.PlayHorn();

				if (controlState.Insult)
				{
					if ((this.insult == null) || (!this.insult.IsPlaying))
					{
						Random random = new Random(gameTime.TotalRealTime.Milliseconds);
						int whichInsult = random.Next(1, 4);
						switch (whichInsult)
						{
							case 1:
								this.insult = Sound.Play("Fuck-off");
								break;
							case 2:
								this.insult = Sound.Play("Wanker");
								break;
							case 3:
								this.insult = Sound.Play("You Nobber");
								break;
						}
					}
				}
			// End if some time has lapsed
			}

			if (((AwesomeGame)this.Game).CheckForCollisions((Mesh)this, (Mesh)this.nextCheckpoint))
			{
				// If we hit the checkpoint
				//this.GetService<Camera>().RemoveViewObject(this.nextCheckpoint);
				bool won = false;
				this.nextCheckpoint = this.GetService<Course>().getNextCheckpoint(nextCheckpoint, out won);
				//this.GetService<Camera>().AddViewObject(this.nextCheckpoint);

				if (won)
				{
					_messageToDraw = "a";
					Sound.Play("Victory");
				}
				else
				{
					// Play a sound!
					checkpointCheer = Sound.Play("Congrats");
				}
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

			if (_messageToDraw != null)
			{
				_spriteBatch.Begin();
				_spriteBatch.DrawString(_titleFont, "Congratulations", new Vector2(GetRandomOffset(100), GetRandomOffset(100)), Color.Red);
				_spriteBatch.DrawString(_titleFont, "Player " + this.playerIndex.ToString(), new Vector2(GetRandomOffset(100), GetRandomOffset(150)), Color.Red);
				_spriteBatch.DrawString(_titleFont, "You are AWESOME.", new Vector2(GetRandomOffset(100), GetRandomOffset(400)), Color.White);
				_spriteBatch.End();
			}

			_frameCount++;
		}

		private long _frameCount;
		private int GetRandomOffset(int value)
		{
			if (_frameCount % 5 == 0)
				return value + new Random(Environment.TickCount * 1000 + value).Next(-2, 3);
			else
				return value;
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
				Matrix.CreateRotationY(-steer) *
				Matrix.CreateTranslation(steer != 0 ? FRONT_AXLE_POS : 0, -height, 0);
		}

		private CarControlState GetControlState(PlayerIndex playerIndex, float deltaTime)
		{
			CarControlState controlState = new CarControlState();
			GamePadState padState = GamePad.GetState(playerIndex);
			KeyboardState keyState = Keyboard.GetState();

			// Right-trigger or keyboard-up to accelerate,
			// Left-trigger or keyboard-down to brake
			controlState.Accel = padState.Triggers.Right;
			controlState.Brake = padState.Triggers.Left;
			if (playerIndex == PlayerIndex.One)
			{
				if (keyState.IsKeyDown(Keys.Up)) controlState.Accel = 1.0f;
				if (keyState.IsKeyDown(Keys.Down)) controlState.Brake = 1.0f;
			}
			else
			{
				if (keyState.IsKeyDown(Keys.W)) controlState.Accel = 1.0f;
				if (keyState.IsKeyDown(Keys.S)) controlState.Brake = 1.0f;
			}

			// Left-stick or keyboard left-right arrows to steer
			float steer = padState.ThumbSticks.Left.X;
			if (playerIndex == PlayerIndex.One)
			{
				if (keyState.IsKeyDown(Keys.Right)) steer += 1.0f;
				if (keyState.IsKeyDown(Keys.Left)) steer -= 1.0f;
			}
			else
			{
				if (keyState.IsKeyDown(Keys.D)) steer += 1.0f;
				if (keyState.IsKeyDown(Keys.A)) steer -= 1.0f;
			}
			steer *= 0.3f;

			controlState.Steer = previousControlState.Steer +
				(steer - previousControlState.Steer) * Math.Min(deltaTime * 2.0f, 1.0f);

			// Horn on button B or right control
			// Insult on button Y or End
			controlState.Horn = padState.Buttons.B == ButtonState.Pressed;
			controlState.Insult = padState.Buttons.RightShoulder == ButtonState.Pressed;
			if (playerIndex == PlayerIndex.One)
			{
				if (keyState.IsKeyDown(Keys.RightControl)) controlState.Horn = true;
				if (keyState.IsKeyDown(Keys.End)) controlState.Insult = true;
			}
			else
			{
				if (keyState.IsKeyDown(Keys.LeftShift)) controlState.Horn = true;
				if (keyState.IsKeyDown(Keys.D3)) controlState.Insult = true;
			}

			previousControlState = controlState;
			return controlState;
		}
	}
}
