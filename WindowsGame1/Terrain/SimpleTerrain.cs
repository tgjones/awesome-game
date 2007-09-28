using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AwesomeGame.Terrain
{
	public class SimpleTerrain : GameObject
	{
		private VertexDeclaration _vertexDeclaration;
		private VertexBuffer _vertexBuffer;
		private IndexBuffer _indexBuffer;
		private int _size;
		private string _textureAssetName;
		private Texture2D _texture;

		private Effect _effect;

		private readonly int _numVertices;
		private readonly int _numIndices;

		/*public override BoundingBox BoundingBox
		{
			get { return new BoundingBox(new Vector3(0, 0, -_size), new Vector3(_size, 1, 0)); }
		}*/

		public SimpleTerrain(Game game, int size, string textureAssetName)
			: base(game)
		{
			_size = size;

			_numVertices = _size * _size;

			int numInternalRows = _size - 2;
			_numIndices = (2 * _size * (1 + numInternalRows)) + (2 * numInternalRows);

			_textureAssetName = textureAssetName;
		}

		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			base.LoadGraphicsContent(loadAllContent);

			if (loadAllContent)
			{
				_effect = this.GetService<ContentManager>().Load<Effect>(@"Terrain\SimpleTerrain");

				VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[_numVertices];
				for (int z = 0; z < _size; z++)
				{
					for (int x = 0; x < _size; x++)
					{
						//float height = GetHeight(x, z);
						float height = 0;

						vertices[GetIndex(x, z)] = new VertexPositionNormalTexture(
							new Vector3(x, height, -z), new Vector3(0, 1, 0),
							new Vector2(x / (float) (_size - 1), z / (float) (_size - 1)));
					}
				}

				_vertexBuffer = new VertexBuffer(
					this.GraphicsDevice,
					typeof(VertexPositionNormalTexture),
					vertices.Length,
					ResourceUsage.WriteOnly,
					ResourceManagementMode.Automatic);
				_vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

				short[] indices = new short[_numIndices]; int indexCounter = 0;
				for (int z = 0; z < _size - 1; z++)
				{
					// insert index for degenerate triangle
					if (z > 0)
						indices[indexCounter++] = GetIndex(0, z);

					for (int x = 0; x < _size; x++)
					{
						indices[indexCounter++] = GetIndex(x, z);
						indices[indexCounter++] = GetIndex(x, z + 1);
					}

					// insert index for degenerate triangle
					if (z < _size - 2)
						indices[indexCounter++] = GetIndex(_size - 1, z);
				}

				_indexBuffer = new IndexBuffer(
					this.GraphicsDevice,
					typeof(short),
					indices.Length,
					ResourceUsage.WriteOnly,
					ResourceManagementMode.Automatic);
				_indexBuffer.SetData<short>(indices);

				_vertexDeclaration = new VertexDeclaration(
					this.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

				if (_textureAssetName != null)
				{
					ContentManager content = (ContentManager) this.Game.Services.GetService(typeof(ContentManager));
					_texture = content.Load<Texture2D>(_textureAssetName);
				}

				_effect.Parameters["GrassTexture"].SetValue(_texture);
				_effect.Parameters["TerrainSize"].SetValue(_size);
			}
		}

		private short GetIndex(int x, int z)
		{
			return (short) ((z * _size) + x);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			Matrix viewMatrix = Matrix.CreateLookAt(
				new Vector3(0.0f, 10.0f, 1),
				Vector3.Zero,
				new Vector3(0.0f, 1.0f, 0.0f));
			Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				MathHelper.ToRadians(45),
				(float) this.GraphicsDevice.Viewport.Width / (float) this.GraphicsDevice.Viewport.Height,
				1.0f, 200.0f);
			_effect.Parameters["WorldViewProjection"].SetValue(viewMatrix * projectionMatrix);
		}

		public override void Draw(GameTime gameTime)
		{
			//this.GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;
			this.GraphicsDevice.VertexDeclaration = _vertexDeclaration;
			this.GraphicsDevice.Vertices[0].SetSource(_vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
			this.GraphicsDevice.Indices = _indexBuffer;

			_effect.Begin();
			foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
			{
				pass.Begin();

				this.GraphicsDevice.DrawIndexedPrimitives(
					PrimitiveType.TriangleStrip,
					0,
					0,
					_numVertices,
					0,
					_numIndices - 2);

				pass.End();
			}
			_effect.End();
		}

		/*public override float GetHeight(float x, float z)
		{
			int integerX = MathsHelper.FloorToInt(x);
			int integerZ = MathsHelper.FloorToInt(z);
			float fractionalX = x - integerX;
			float fractionalZ = z - integerZ;

			float v1 = GetHeight(integerX, integerZ);
			float v2 = GetHeight(integerX + 1, integerZ);
			float v3 = GetHeight(integerX, integerZ + 1);
			float v4 = GetHeight(integerX + 1, integerZ + 1);

			float i1 = PerlinNoise.Interpolate(v1, v2, fractionalX);
			float i2 = PerlinNoise.Interpolate(v3, v4, fractionalX);

			return PerlinNoise.Interpolate(i1, i2, fractionalZ);
		}

		public float GetHeight(int x, int z)
		{
			return 0;
			return _perlinNoise.GetPerlinNoise(x, z);
		}*/
	}
}
