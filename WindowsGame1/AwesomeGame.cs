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

		public AwesomeGame()
		{
			graphics = new GraphicsDeviceManager(this);
			content = new ContentManager(Services);
			
			this.Services.AddService(typeof(ContentManager), content);

			//create the camera and add it as a service
			camera = new Camera();
			this.Services.AddService(typeof(Camera), camera);

			//this.Components.Add(new Terrain.SimpleTerrain(this, 8, @"Terrain\Textures\grass"));
			Terrain.SimpleTerrain gameTerrain = new Terrain.SimpleTerrain(this, @"Terrain\Textures\level1_heightmap", @"Terrain\Textures\level1_texture");
			this.Components.Add(gameTerrain);							//add terrain to component manager
			this.Services.AddService(typeof(Terrain.SimpleTerrain), gameTerrain);		//make terrain available as a service.

			//this.Components.Add(new Triangle(this));

			GameObject car = new Vehicles.SchoolBus(this);
			this.Components.Add(car);
			camera.AddViewObject(car);

			GameObject axes = new Mesh(this, @"Models\Axes");
			this.Components.Add(axes);
			//camera.AddViewObject(axes);
			
			this.Components.Add(new Mesh(this, @"Models\Cone", Matrix.CreateTranslation(new Vector3(5.0f, 0.0f, 5.0f))));
			this.Components.Add(new Mesh(this, @"Models\Barrel", Matrix.CreateTranslation(new Vector3(10.0f, 0.0f, 10.0f))));
			
			// Get some sort of checkpoint based course going on
			course = new Course(this);
			this.Services.AddService(typeof(Course), course);
			
			Mesh checkpoint;
			checkpoint = new Mesh(this, @"Models\Checkpoint", Matrix.CreateTranslation(new Vector3(-20.0f, 15.0f, -2.0f)));
			course.addCheckpoint(checkpoint);
			this.Components.Add(checkpoint);
			checkpoint = new Mesh(this, @"Models\Checkpoint", Matrix.CreateRotationY(MathHelper.ToRadians(-70)) * Matrix.CreateTranslation(new Vector3(-145.0f, 6.0f, -130.0f)));
			course.addCheckpoint(checkpoint);
			this.Components.Add(checkpoint);

			this.Components.Add(new Physics.ParticleSystem(this, @"Physics\Box.xml"));
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
			// Allows the game to exit
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
				this.Exit();

			//update the camera
			camera.Update(gameTime, graphics.GraphicsDevice);

			base.Update(gameTime);
		}


		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
			base.Draw(gameTime);
		}
	}
}
