#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using AwesomeGame;
using System.Collections.Specialized;
#endregion

namespace AwesomeGame
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class AwesomeGame : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		ContentManager content;
		Camera camera;
		Course course;
		GameState _gameState = GameState.ChooseCar;

		private SpriteBatch _spriteBatch;
		private SpriteFont _titleFont, _font;
		private List<string> _availableCars;
		private int _selectedCarIndex;
		private GamePadState _lastGamePad1State;
		private KeyboardState _lastKeyboardState;

		private Terrain.SimpleTerrain _terrain;
		private Vehicles.Car _car1, _car2;
		private Mesh _checkpointArrow1, _checkpointArrow2;
		private Mesh _barrel;
		private Sunlight _sunlight;
		private ShadowMap _shadowMap;

		public AwesomeGame()
		{
			graphics = new GraphicsDeviceManager(this);
			content = new ContentManager(Services);

			//graphics.IsFullScreen = true;
			
			this.Services.AddService(typeof(ContentManager), content);

			//create the camera and add it as a service
			camera = new Camera();
			this.Services.AddService(typeof(Camera), camera);

			//this.Components.Add(new Terrain.SimpleTerrain(this, 8, @"Terrain\Textures\grass"));
			_terrain = new Terrain.SimpleTerrain(this, @"Terrain\Textures\level1_heightmap", @"Terrain\Textures\level1_texture", @"Terrain\Textures\level1_gameobjects");
			_terrain.collidable = false;
			this.Services.AddService(typeof(Terrain.SimpleTerrain), _terrain);		//make terrain available as a service.

			_barrel = new Mesh(this, @"Models\Barrel", Matrix.CreateTranslation(new Vector3(10.0f, 0.0f, 10.0f)));

			// Get some sort of checkpoint based course going on
			course = new Course(this);
			this.Services.AddService(typeof(Course), course);

			//this.Components.Add(new Physics.ParticleSystem(this, @"Physics\Cone.xml", new Vector3(0,200,0)));
			if (Environment.MachineName != "BARNEY-DESKTOP")
			{
				_sunlight = new Sunlight(this);
				_shadowMap = new ShadowMap(this);
			}

			_availableCars = new List<string>();
			_availableCars.Add("Less Blocky Car 2");
			_availableCars.Add("Curvy Car");
			_availableCars.Add("Police Car");
			_availableCars.Add("Schoolbus");
			_availableCars.Add("Trike");
		}

		public bool CheckForCollisions(Mesh object1, Mesh object2 )
		{
			for (int i = 0; i < object1._model.Meshes.Count; i++)
			{
				// Check whether the bounding boxes of the two cubes intersect.
				BoundingSphere object1BoundingSphere = object1._model.Meshes[i].BoundingSphere;
				object1BoundingSphere.Center += object1.position;

				for (int j = 0; j < object2._model.Meshes.Count; j++)
				{
					BoundingSphere object2BoundingSphere = object2._model.Meshes[j].BoundingSphere;
					object2BoundingSphere.Center += object2.position;

					if (object1BoundingSphere.Intersects(object2BoundingSphere))
					{
						//c2.ReverseVelocity();
						//c1.Backup();
						//c1.ReverseVelocity();
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			camera.Initialize();
			Sound.Initialize();
			base.Initialize();
		}


		/// <summary>
		/// Load your graphics content.  If loadAllContent is true, you should
		/// load content from both ResourceManagementMode pools.  Otherwise, just
		/// load ResourceManagementMode.Manual content.
		/// </summary>
		/// <param name="loadAllContent">Which type of content to load.</param>
		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			if (loadAllContent)
			{
				_spriteBatch = new SpriteBatch(this.graphics.GraphicsDevice);
				_titleFont = content.Load<SpriteFont>(@"Fonts\TitleFont");
				_font = content.Load<SpriteFont>(@"Fonts\MenuFont");

				/*// TODO: Load any ResourceManagementMode.Automatic content
				blockyCarModel = content.Load<Model>(@"Models\blocky car");

				Mesh test = new WindowsGame1.BlockyCar();
				test.Initialize(graphics.GraphicsDevice);
				test.model = blockyCarModel;

				objects.AddObject(test);*/
			}

			// TODO: Load any ResourceManagementMode.Manual content
		}


		/// <summary>
		/// Unload your graphics content.  If unloadAllContent is true, you should
		/// unload content from both ResourceManagementMode pools.  Otherwise, just
		/// unload ResourceManagementMode.Manual content.  Manual content will get
		/// Disposed by the GraphicsDevice during a Reset.
		/// </summary>
		/// <param name="unloadAllContent">Which type of content to unload.</param>
		protected override void UnloadGraphicsContent(bool unloadAllContent)
		{
			if (unloadAllContent)
			{
				// TODO: Unload any ResourceManagementMode.Automatic content
				content.Unload();
			}

			// TODO: Unload any ResourceManagementMode.Manual content
		}


		/// <summary>
		/// Allows the game to run logic such as updating the world,
		/// checking for collisions, gathering input and playing audio.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Update(GameTime gameTime)
		{
			KeyboardState keyboardState = Keyboard.GetState();
			GamePadState gamePad1State = GamePad.GetState(PlayerIndex.One);

			switch (_gameState)
			{
				case GameState.ChooseCar :
					if (gamePad1State.DPad.Down == ButtonState.Pressed && (_lastGamePad1State == null || _lastGamePad1State.DPad.Down == ButtonState.Released))
						_selectedCarIndex = Math.Min(_selectedCarIndex + 1, _availableCars.Count - 1);
					if (gamePad1State.DPad.Up == ButtonState.Pressed && (_lastGamePad1State == null || _lastGamePad1State.DPad.Up == ButtonState.Released))
						_selectedCarIndex = Math.Max(_selectedCarIndex - 1, 0);
					if (keyboardState.IsKeyDown(Keys.Down) && (_lastKeyboardState == null || !_lastKeyboardState.IsKeyDown(Keys.Down)))
						_selectedCarIndex = Math.Min(_selectedCarIndex + 1, _availableCars.Count - 1);
					if (keyboardState.IsKeyDown(Keys.Up) && (_lastKeyboardState == null || !_lastKeyboardState.IsKeyDown(Keys.Up)))
						_selectedCarIndex = Math.Max(_selectedCarIndex + 1, 0);

					if (gamePad1State.Buttons.A == ButtonState.Pressed || gamePad1State.Buttons.Start == ButtonState.Pressed
						|| keyboardState.IsKeyDown(Keys.Enter))
					{
						this.Components.Add(_terrain);							//add terrain to component manager

						// Add first car
						_car1 = CreateCar(_selectedCarIndex, PlayerIndex.One);
						_car1.position.Y = 110.0f;
						_car1.position.Z = -10.0f;
						_car1.collidable = true;
						_car1.moveable = true;
						this.Components.Add(_car1);
						camera.AddViewObject(_car1);

						_checkpointArrow1 = new Models.CheckpointArrow(this);
						_car1.setNextCheckpointArrow(_checkpointArrow1);
						_checkpointArrow1.CastsShadow = false;
						_checkpointArrow1.collidable = false;
						this.Components.Add(_checkpointArrow1);

						if (false)
						{
							// Add second car
							_car2 = new Vehicles.Trike(this, PlayerIndex.Two);
							_car2.position.Y = 110.0f;
							_car2.position.Z = 10.0f;
							_car2.collidable = true;
							_car2.moveable = true;
							this.Components.Add(_car2);
							camera.AddViewObject(_car2);

							_checkpointArrow2 = new Models.CheckpointArrow(this);
							_car2.setNextCheckpointArrow(_checkpointArrow2);
							_checkpointArrow2.CastsShadow = false;
							_checkpointArrow2.collidable = false;
							this.Components.Add(_checkpointArrow2);
						}

						this.Components.Add(_barrel);

						if (_sunlight != null)
							this.Components.Add(_sunlight);

						if (_shadowMap != null)
							this.Components.Add(_shadowMap);

						_gameState = GameState.Game;
					}
					break;
				case GameState.Game :
					//update the camera
					camera.Update(gameTime, graphics.GraphicsDevice);

					Sound.Update();

					break;
			}

			// Allows the game to exit
			if (gamePad1State.Buttons.Back == ButtonState.Pressed)
				this.Exit();

			if (keyboardState.IsKeyDown(Keys.Escape))
				this.Exit();

			_lastGamePad1State = gamePad1State;
			_lastKeyboardState = keyboardState;

			base.Update(gameTime);
		}

		private Vehicles.Car CreateCar(int selectedIndex, PlayerIndex player)
		{
			switch (selectedIndex)
			{
				case 0:
					return new Vehicles.Blocky(this, player);
				case 1:
					return new Vehicles.Curvy(this, player);
				case 2:
					return new Vehicles.Police(this, player);
				case 3:
					return new Vehicles.SchoolBus(this, player);
				case 4:
					return new Vehicles.Trike(this, player);
				default :
					throw new NotImplementedException();
			}
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			_frameCount++;

			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			switch (_gameState)
			{
				case GameState.ChooseCar :
					_spriteBatch.Begin();
					_spriteBatch.DrawString(_titleFont, "AWESOME GAME", new Vector2(40, 100), Color.Blue);
					int currentY = 160; int delta = 25; int counter = 0;
					foreach (string carName in _availableCars)
						DrawString(carName, GetRandomOffset(currentY += delta), (counter++ == _selectedCarIndex));
					_spriteBatch.End();

					break;
			}
			base.Draw(gameTime);
		}

		private long _frameCount;

		private int GetRandomOffset(int value)
		{
			if (_frameCount % 5 == 0)
				return value + new Random(Environment.TickCount * 1000 + value).Next(-2, 3);
			else
				return value;
		}

		private void DrawString(string value, int y, bool selected)
		{
			_spriteBatch.DrawString(_font, value, new Vector2(GetRandomOffset(50), y), (selected) ? Color.Red : Color.White);
		}
	}

	public enum GameState
	{
		ChooseCar,
		Game
	}
}
