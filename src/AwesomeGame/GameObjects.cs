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
		public bool collidable;
		public bool moveable;

		public GameObject(Game game)
			: base(game)
		{

		}

		protected T GetService<T>()
		{
			return (T)this.Game.Services.GetService(typeof(T));
		}
	}

	public class Triangle : GameObject
	{
		private VertexBuffer vertexBuffer;
		private BasicEffect basicEffect;
		private Matrix worldMatrix = Matrix.Identity;

		public Triangle(Game game)
			: base(game)
		{

		}

		protected override void LoadContent()
		{
			base.LoadContent();

			//Matrix viewMatrix = Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10.0f), Vector3.Zero, new Vector3(0.0f, 1.0f, 0.0f));
			//Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
			//	MathHelper.ToRadians(45),
			//(float) this.GraphicsDevice.Viewport.Width / (float) this.GraphicsDevice.Viewport.Height,
			//1.0f, 20.0f);

			basicEffect = new BasicEffect(this.GraphicsDevice);
			basicEffect.World = worldMatrix;
			basicEffect.View = this.GetService<Camera>().ViewMatrix;
			basicEffect.Projection = this.GetService<Camera>().ProjectionMatrix;
			basicEffect.VertexColorEnabled = true;

			vertexBuffer = new VertexBuffer(this.GraphicsDevice, VertexPositionColor.VertexDeclaration, 3, BufferUsage.None);
			VertexPositionColor[] vertices = new VertexPositionColor[3];
			vertices[0] = new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f), Color.Blue);
			vertices[1] = new VertexPositionColor(new Vector3(0.0f, 1.0f, 0.0f), Color.Blue);
			vertices[2] = new VertexPositionColor(new Vector3(1.0f, 0.0f, 0.0f), Color.Blue);
			vertexBuffer.SetData<VertexPositionColor>(vertices);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			worldMatrix = Matrix.CreateRotationZ((float)gameTime.TotalGameTime.TotalMilliseconds * 0.0001f);
			basicEffect.World = worldMatrix;
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				this.GraphicsDevice.SetVertexBuffer(vertexBuffer);
				this.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 1);
			}
		}
	}

	public class Mesh : GameObject
	{
		private string _modelAssetName;
		public Model _model;
		public Matrix WorldMatrix = Matrix.Identity;
		protected List<Matrix> _partTransformationMatrices;
		protected List<BoundingBox> _partBoundingBoxes;
		protected List<BasicEffect> _modelMeshPartEffects;
		private Matrix _initialTranformationMatrix;
		private List<Effect> _shadowEffects;

		private BoundingSphere _boundingSphere;

		public bool CastsShadow = true;

		public Matrix InitialTransformationMatrix
		{
			get { return _initialTranformationMatrix; }
			set { _initialTranformationMatrix = value; }
		}

		public BoundingSphere BoundingSphere
		{
			get
			{
				//BoundingSphere boundingSphere = new BoundingSphere();
				//foreach (ModelMesh mm in _model.Meshes)
				//	boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mm.BoundingSphere);
				//boundingSphere = new BoundingSphere(this.position, boundingSphere.Radius);
				//return boundingSphere;

				//return new BoundingSphere(this.position, 4.5f);

				return _boundingSphere;
			}
		}

		public Mesh(Game game, string modelAssetName)
			: this(game, modelAssetName, Matrix.Identity)
		{

			_partBoundingBoxes = new List<BoundingBox>();
		}

		public Mesh(Game game, string modelAssetName, Matrix initialTransformationMatrix)
			: base(game)
		{
			_modelAssetName = modelAssetName;
			_initialTranformationMatrix = initialTransformationMatrix;
			_partTransformationMatrices = new List<Matrix>();
			_partBoundingBoxes = new List<BoundingBox>();
			_modelMeshPartEffects = new List<BasicEffect>();
			_shadowEffects = new List<Effect>();
			_boundingSphere = new BoundingSphere();

			Vector3 scale;
			Quaternion rotation;
			initialTransformationMatrix.Decompose(out scale, out rotation, out position);
			_initialTranformationMatrix = Matrix.CreateFromQuaternion(rotation) * Matrix.CreateScale(scale);

			this.DrawOrder = 5;
		}

		protected override void LoadContent()
		{
			base.LoadContent();

			_model = this.GetService<ContentManager>().Load<Model>(_modelAssetName);

			Effect shadowEffect = GetService<ContentManager>().Load<Effect>(@"Shaders\ShadowMapping");
			shadowEffect.CurrentTechnique = shadowEffect.Techniques[0];

			_partTransformationMatrices.Clear();
			_modelMeshPartEffects.Clear();
			_shadowEffects.Clear();
			foreach (ModelMesh mesh in _model.Meshes)
			{
				foreach (ModelMeshPart part in mesh.MeshParts)
				{
					// Common lighting parameters
					BasicEffect effect = part.Effect.Clone() as BasicEffect;
					effect.AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f);
					effect.EmissiveColor = new Vector3(0.0f, 0.0f, 0.0f);
					effect.DirectionalLight0.DiffuseColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.DirectionalLight0.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.DirectionalLight0.Direction = new Vector3(1.0f, -1.0f, 0.0f);
					effect.DirectionalLight0.Direction.Normalize();
					effect.DirectionalLight0.Enabled = true;
					effect.LightingEnabled = true;
					effect.PreferPerPixelLighting = true;

					_modelMeshPartEffects.Add(effect);
					_partTransformationMatrices.Add(Matrix.Identity);
					_shadowEffects.Add(shadowEffect.Clone());
				}
			}

			UpdateEffects();
		}

		public void DrawShadowMap(GameTime gameTime)
		{
			if (CastsShadow)
			{
				WorldMatrix = Matrix.CreateRotationY(orientation.Y) * Matrix.CreateTranslation(position);
				Sunlight light = GetService<Sunlight>();
				this.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

				int counter = 0;
				foreach (ModelMesh mm in _model.Meshes)
					foreach (ModelMeshPart mmp in mm.MeshParts)
					{
						mmp.Effect = _shadowEffects[counter];
						mmp.Effect.Parameters["LightWorldViewProjection"].SetValue(_partTransformationMatrices[counter++] * _initialTranformationMatrix * WorldMatrix * light.ViewMatrix * light.ProjectionMatrix);
					}

				DrawComponent();
			}
		}

		public virtual void UpdateEffects() { }

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			WorldMatrix = Matrix.CreateRotationY(orientation.Y) * Matrix.CreateTranslation(position);
			Matrix viewMatrix = this.GetService<Camera>().ViewMatrix;
			Matrix projectionMatrix = this.GetService<Camera>().ProjectionMatrix;

			int counter = 0;
			foreach (ModelMesh mm in _model.Meshes)
			{
				foreach (ModelMeshPart mmp in mm.MeshParts)
				{
					BasicEffect effect = _modelMeshPartEffects[counter++];
				}
			}
		}

		public override void Draw(GameTime gameTime)
		{
			base.Draw(gameTime);

			WorldMatrix = Matrix.CreateRotationY(orientation.Y) * Matrix.CreateTranslation(position);
			Matrix viewMatrix = this.GetService<Camera>().ViewMatrix;
			Matrix projectionMatrix = this.GetService<Camera>().ProjectionMatrix;

			int counter = 0;
			BoundingSphere newBoundingSphere = new BoundingSphere();
			foreach (ModelMesh mm in _model.Meshes)
			{
				foreach (ModelMeshPart mmp in mm.MeshParts)
				{
					BasicEffect effect = _modelMeshPartEffects[counter];
					effect.World = _partTransformationMatrices[counter++] * _initialTranformationMatrix * WorldMatrix;
					effect.View = viewMatrix;
					effect.Projection = projectionMatrix;
					mmp.Effect = effect;
				}
				if (this._boundingSphere.Radius == 0.0f)
				{
					newBoundingSphere = BoundingSphere.CreateMerged(newBoundingSphere, mm.BoundingSphere);
				}
				if (newBoundingSphere.Radius == 0.0f)
				{
					newBoundingSphere = mm.BoundingSphere;
				}
			}

			if (this._boundingSphere.Radius == 0.0f)
				this._boundingSphere = new BoundingSphere(this.position, newBoundingSphere.Radius);

			this._boundingSphere.Center = this.position;
			//foreach (ModelMesh mm in _model.Meshes)
			//	boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mm.BoundingSphere);
			//boundingSphere = new BoundingSphere(this.position, boundingSphere.Radius);

			this.GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
			DrawComponent();
		}

		private void DrawComponent()
		{
			//graphicsDevice.VertexDeclaration = vertexDeclaration;
			//this.GraphicsDevice.RenderState.CullMode = CullMode.None;
			//this.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
			foreach (ModelMesh mm in _model.Meshes)
				mm.Draw();
		}
	}
}
