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
		public Vector3 orientation;

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
		public Matrix WorldMatrix = Matrix.Identity;
		protected List<Matrix> _partTransformationMatrices;
		private Matrix _initialTranformationMatrix;
		private List<BasicEffect> _modelMeshPartEffects;

		public Matrix InitialTransformationMatrix
		{
			get { return _initialTranformationMatrix; }
			set { _initialTranformationMatrix = value; }
		}

		public BoundingSphere BoundingSphere
		{
			get
			{
				BoundingSphere boundingSphere = new BoundingSphere(this.position, 4.5f);
				return boundingSphere;
			}
		}

		public Mesh(Game game, string modelAssetName)
			: this(game, modelAssetName, Matrix.Identity)
		{
			
		}

		public Mesh(Game game, string modelAssetName, Matrix initialTransformationMatrix)
			: base(game)
		{
			_modelAssetName = modelAssetName;
			_initialTranformationMatrix = initialTransformationMatrix;
			_partTransformationMatrices = new List<Matrix>();
			_modelMeshPartEffects = new List<BasicEffect>();

			Vector3 scale;
			Quaternion rotation;
			initialTransformationMatrix.Decompose(out scale, out rotation, out position);
		}

		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			base.LoadGraphicsContent(loadAllContent);

			_model = this.GetService<ContentManager>().Load<Model>(_modelAssetName);

			_partTransformationMatrices.Clear();
			_modelMeshPartEffects.Clear();
			foreach (ModelMesh mesh in _model.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					_partTransformationMatrices.Add(Matrix.Identity);
					_modelMeshPartEffects.Add(part.Effect.Clone(this.GraphicsDevice) as BasicEffect);
				}
			}
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			//Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10.0f), Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f));
			//Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
			//    MathHelper.ToRadians(45),
			//    (float) this.GraphicsDevice.Viewport.Width / (float) this.GraphicsDevice.Viewport.Height,
			//    1.0f, 20.0f);
			WorldMatrix = Matrix.CreateRotationY(orientation.Y) * Matrix.CreateTranslation(position);
			Matrix viewMatrix = this.GetService<Camera>().ViewMatrix;
			Matrix projectionMatrix = this.GetService<Camera>().ProjectionMatrix;

			int meshPartIndex = 0; int counter = 0;
			foreach (ModelMesh mm in _model.Meshes)
			{
				foreach (ModelMeshPart mmp in mm.MeshParts)
				{
					BasicEffect effect = _modelMeshPartEffects[counter++];
					effect.World = _partTransformationMatrices[meshPartIndex++] * _initialTranformationMatrix * WorldMatrix;
					effect.View = viewMatrix;
					effect.Projection = projectionMatrix;

					effect.EmissiveColor = new Vector3(0.0f, 0.0f, 5.0f);
					if (mm.MeshParts.IndexOf(mmp) != 0)
						effect.EmissiveColor = new Vector3(0.1f, 0.1f, 0.1f);

					effect.DiffuseColor = effect.EmissiveColor;
					effect.SpecularColor = new Vector3(0.2f, 0.2f, 0.2f);
					effect.SpecularPower = 1.0f;

					effect.DirectionalLight0.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.DirectionalLight0.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.DirectionalLight0.Direction = new Vector3(1.0f, 0.0f, 0.0f);
					effect.DirectionalLight0.Enabled = true;
					effect.LightingEnabled = true;
					effect.PreferPerPixelLighting = true;
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			int counter = 0;
			foreach (ModelMesh mm in _model.Meshes)
				foreach (ModelMeshPart mmp in mm.MeshParts)
					mmp.Effect = _modelMeshPartEffects[counter++];

			//graphicsDevice.VertexDeclaration = vertexDeclaration;
			this.GraphicsDevice.RenderState.CullMode = CullMode.None;
			this.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
			foreach (ModelMesh mm in _model.Meshes)
			{
				mm.Draw();
			}
		}
	}
}
