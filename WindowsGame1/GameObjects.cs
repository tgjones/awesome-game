using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace AwesomeGame
{
	public abstract class GameObject : Microsoft.Xna.Framework.DrawableGameComponent
	{
		public Vector3 position;
		public Vector3 velocity;

		public GameObject(Game game)
			: base(game)
		{

		}

		protected T GetService<T>()
		{
			return (T) this.Game.Services.GetService(typeof(T));
		}
	}

	public class Triangle : GameObject
	{
		private VertexBuffer vertexBuffer;
		private BasicEffect basicEffect;
		private VertexDeclaration vertexDeclaration;
		private Matrix worldMatrix = Matrix.Identity;

		public Triangle(Game game)
			: base(game)
		{

		}

		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			base.LoadGraphicsContent(loadAllContent);

			if (loadAllContent)
			{
				//Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10.0f), Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f));
				//Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				//	MathHelper.ToRadians(45),
					//(float) this.GraphicsDevice.Viewport.Width / (float) this.GraphicsDevice.Viewport.Height,
					//1.0f, 20.0f);

				basicEffect = new BasicEffect(this.GraphicsDevice, null);
				basicEffect.World = worldMatrix;
				basicEffect.View = this.GetService<Camera>().ViewMatrix;
				basicEffect.Projection = this.GetService<Camera>().ProjectionMatrix;
				basicEffect.VertexColorEnabled = true;

				vertexBuffer = new VertexBuffer(this.GraphicsDevice, typeof(VertexPositionColor), 3, ResourceUsage.None);
				vertexDeclaration = new VertexDeclaration(this.GraphicsDevice, VertexPositionColor.VertexElements);
				VertexPositionColor[] vertices = new VertexPositionColor[3];
				vertices[0] = new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f), Color.Blue);
				vertices[1] = new VertexPositionColor(new Vector3(0.0f, 1.0f, 0.0f), Color.Blue);
				vertices[2] = new VertexPositionColor(new Vector3(1.0f, 0.0f, 0.0f), Color.Blue);
				vertexBuffer.SetData<VertexPositionColor>(vertices);
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			worldMatrix = Matrix.CreateRotationZ((float) gameTime.TotalRealTime.TotalMilliseconds * 0.0001f);
			basicEffect.World = worldMatrix;
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			this.GraphicsDevice.VertexDeclaration = vertexDeclaration;
			basicEffect.Begin();
			foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
			{
				pass.Begin();
				this.GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionColor.SizeInBytes);
				this.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
				pass.End();
			}
			basicEffect.End();
		}
	}

	public class Mesh : GameObject
	{
		private string _modelAssetName;
		private Model _model;
		private Matrix _worldMatrix = Matrix.Identity;
		private float _scaleFactor;
		private Matrix _rotationMatrix;

		public Mesh(Game game, string modelAssetName, float scaleFactor, Matrix rotationMatrix)
			: base(game)
		{
			_modelAssetName = modelAssetName;
			_scaleFactor = scaleFactor;
			_rotationMatrix = rotationMatrix;
		}

		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			base.LoadGraphicsContent(loadAllContent);

			_model = this.GetService<ContentManager>().Load<Model>(_modelAssetName);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			//Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10.0f), Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f));
			//Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
			//    MathHelper.ToRadians(45),
			//    (float) this.GraphicsDevice.Viewport.Width / (float) this.GraphicsDevice.Viewport.Height,
			//    1.0f, 20.0f);
			Matrix viewMatrix = this.GetService<Camera>().ViewMatrix;
			Matrix projectionMatrix = this.GetService<Camera>().ProjectionMatrix;

			foreach (ModelMesh mm in _model.Meshes)
			{
				foreach (ModelMeshPart mmp in mm.MeshParts)
				{
					((BasicEffect) mmp.Effect).World = _worldMatrix * _rotationMatrix * Matrix.CreateScale(_scaleFactor);
					((BasicEffect) mmp.Effect).View = viewMatrix;
					((BasicEffect) mmp.Effect).Projection = projectionMatrix;
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			//graphicsDevice.VertexDeclaration = vertexDeclaration;
			this.GraphicsDevice.RenderState.CullMode = CullMode.None;
			//this.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
			foreach (ModelMesh mm in _model.Meshes)
			{
				mm.Draw();
			}
		}
	}
}