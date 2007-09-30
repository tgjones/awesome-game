using System;
using System.Collections.Generic;
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
		private string _heightMapName;
		private string _objectMapName;

		private Texture2D _texture;
		private Texture2D _normalMap;
		private float[] _heightMap;		//height map

		private Matrix translationMatrix = Matrix.Identity;		//where to put the terrain (so it gets centered)
		private Effect _effect;
		
		private Vector3 _mapScale = Vector3.One;	//scale texture map data by this
		private Vector3 _mapOffset = Vector3.Zero;	//offset texture map data by this

		private int _numVertices;
		private int _numIndices;

		private Matrix _textureScaleAndOffsetMatrix;

		public int Size
		{
			get { return _size; }
		}

		public SimpleTerrain(Game game, int size, string textureAssetName)
			: base(game)
		{
			_size = size;

			_textureAssetName = textureAssetName;
		}

		public SimpleTerrain(Game game, string heightMapAssetName , string textureAssetName, string objectMapName)
			: base(game)
		{
			_textureAssetName = textureAssetName;
			_heightMapName = heightMapAssetName;
			_objectMapName = objectMapName;
		}

		public Vector3 GetPosition(int x, int z)
		{
			return (new Vector3(x, GetHeight(x, z), z) * _mapScale) + _mapOffset;
		}

		protected override void LoadGraphicsContent(bool loadAllContent)
		{
			base.LoadGraphicsContent(loadAllContent);

			if (loadAllContent)
			{
				//grab a handle on the content manager
				ContentManager content = (ContentManager)this.Game.Services.GetService(typeof(ContentManager));

				_effect = this.GetService<ContentManager>().Load<Effect>(@"Terrain\SimpleTerrain");
				foreach (EffectTechnique technique in _effect.Techniques.GetValidTechniques())
				{
					_effect.CurrentTechnique = technique;
					break;
				}

				// if we have a height map, use this for the dimensions
				if (_heightMapName != null)
				{
					//read the height data from the height map
					Texture2D _heightmapTexture = content.Load<Texture2D>(_heightMapName);
					//take the size from the height map (we're assuming it is square)
					_size = _heightmapTexture.Width;

					//translationMatrix = Matrix.CreateTranslation(-_size / 2, 0, -_size / 2);
					//translationMatrix = Matrix.CreateTranslation(0, 0, 0);

					//get the heights from the height map
					Color[] heights = new Color[_size * _size];
					_heightmapTexture.GetData<Color>(heights);

					_heightMap = new float[_size * _size];
					//take the red values for height data
					for (int i = 0; i < _size * _size; i++)
					{
						_heightMap[i] = heights[i].R;
					}
				}

				_numVertices = _size * _size;
				int numInternalRows = _size - 2;
				_numIndices = (2 * _size * (1 + numInternalRows)) + (2 * numInternalRows);

				//our map is square
				const int MAPDIMENSION = 2048;
				_mapScale = new Vector3(MAPDIMENSION / (float)_size, 1.0f, MAPDIMENSION / (float)_size);
				_mapOffset = new Vector3(-MAPDIMENSION / 2, 1, -MAPDIMENSION / 2);	//move to origin

				//get our map objects
				Texture2D objectMapTexture = content.Load<Texture2D>(_objectMapName);
				int objectMapSize = objectMapTexture.Width;
				Color[] objects = new Color[objectMapSize * objectMapSize];
				objectMapTexture.GetData<Color>(objects);
				Vector3 mapObjectScale = new Vector3(MAPDIMENSION / (float)objectMapSize, 1.0f, MAPDIMENSION / (float)objectMapSize);

				//list which model to use for each object
				string[] objectModel = new string[256];
				float[] objectScale = new float[256];
				objectModel[255] = "Cone";		objectScale[255] = 1.0f;
				objectModel[254] = "Building1"; objectScale[254] = 4.0f;
				objectModel[253] = "Building2"; objectScale[253] = 4.0f;
				objectModel[252] = "Building3"; objectScale[252] = 4.0f;
				objectModel[251] = "Building4"; objectScale[251] = 5.0f;
				objectModel[250] = "Building5"; objectScale[250] = 8.0f;
				objectModel[249] = "Building6"; objectScale[249] = 4.0f;
				objectModel[248] = "Building7"; objectScale[248] = 4.0f;
				objectModel[247] = "Building8"; objectScale[247] = 4.0f;
				objectModel[246] = "Checkpoint"; objectScale[246] = 1.0f;
				objectModel[245] = "bridge";	objectScale[245] = 1.0f;
				objectModel[244] = "sheep";		objectScale[244] = 1.0f;

				//take the red values for height data
				for (int i = 0; i < objectMapSize * objectMapSize; i++)
				{
					int objectIndex = objects[i].R;
					if (objectModel[objectIndex] != null)
					{
						Vector3 newObjectPos = new Vector3(i % objectMapSize - 10, 0.0f, (int)(i / objectMapSize) - 10) * mapObjectScale + _mapOffset;
						newObjectPos.Y = GetHeight(newObjectPos.X, newObjectPos.Z);
						float rot = MathHelper.ToRadians((float)objects[i].G / 256.0f * 360.0f);
						Matrix trans = Matrix.CreateRotationY(rot) * Matrix.CreateScale(objectScale[objectIndex]) * Matrix.CreateTranslation(newObjectPos);

						GameObject newObject;
						newObject = CreateMesh(this.Game, objectModel[objects[i].R], trans);

						if (objectIndex == 255)
						{
							// cone
							Physics.ParticleSystem newCone = new Physics.ParticleSystem(this.Game, global::AwesomeGame.Physics.enumPhysicsObjects.Cone, newObjectPos);
							newCone.graphicObject = newObject;	//tell it to use the new graphics object for display
							this.Game.Components.Add(newCone);	//the physics need to be added to the components
						}
						if (objectIndex == 244)
						{
							// sheep
							Physics.ParticleSystem newCone = new Physics.ParticleSystem(this.Game, global::AwesomeGame.Physics.enumPhysicsObjects.Sheep, newObjectPos);
							newCone.graphicObject = newObject;	//tell it to use the new graphics object for display
							this.Game.Components.Add(newCone);	//the physics need to be added to the components
						}

						//add the objects to the game components
						this.Game.Components.Add(newObject);

						//add a checkpoint to the course
						if (objectIndex == 246) ((Course)this.Game.Services.GetService(typeof(Course))).addCheckpoint(newObject);
					}
				}

				//generate texture vertices
				NormalMap normalMap = new NormalMap(this);
				Color[] normals = new Color[_size * _size];
				VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[_numVertices];
				for (int z = 0; z < _size; z++)
				{
					for (int x = 0; x < _size; x++)
					{
						Vector3 normal = normalMap.GetNormal(x, z);
						vertices[GetIndex(x, z)] = new VertexPositionNormalTexture(
							GetPosition(x, z),
							normal,
							new Vector2(x / (float) (_size - 1), z / (float) (_size - 1)));
							//new Vector2(2.0f * x / _size , 2.0f * z / _size ));

						normal /= 2;
						normal += new Vector3(0.5f);
						normals[GetIndex(x, z)] = new Color(normal);
					}
				}

				_vertexBuffer = new VertexBuffer(
					this.GraphicsDevice,
					typeof(VertexPositionNormalTexture),
					vertices.Length,
					ResourceUsage.WriteOnly,
					ResourceManagementMode.Automatic);
				_vertexBuffer.SetData<VertexPositionNormalTexture>(vertices);

				int[] indices = new int[_numIndices]; int indexCounter = 0;

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
					typeof(int),
					indices.Length,
					ResourceUsage.WriteOnly,
					ResourceManagementMode.Automatic);
				_indexBuffer.SetData<int>(indices);

				_vertexDeclaration = new VertexDeclaration(
					this.GraphicsDevice, VertexPositionNormalTexture.VertexElements);

				if (_textureAssetName != null)
				{
					_texture = content.Load<Texture2D>(_textureAssetName);
				}

				_effect.Parameters["GrassTexture"].SetValue(_texture);
				_effect.Parameters["TerrainSize"].SetValue(_size);

				_normalMap = new Texture2D(this.GraphicsDevice, _size, _size, 0, ResourceUsage.None, SurfaceFormat.Color);
				_normalMap.SetData<Color>(normals);
				_effect.Parameters["NormalMapTexture"].SetValue(_normalMap);

				ShadowMap shadowMap = GetService<ShadowMap>();
				if (shadowMap != null)
				{
					_effect.Parameters["ShadowMapSize"].SetValue(shadowMap.ShadowMapSize);
					_effect.Parameters["ShadowMapSizeInverse"].SetValue(1.0f / (float) shadowMap.ShadowMapSize);

					float offset = 0.5f + (0.5f / (float) shadowMap.ShadowMapSize);
					_textureScaleAndOffsetMatrix = new Matrix(
						0.5f, 0.0f, 0.0f, 0.0f,
						0.0f, -0.5f, 0.0f, 0.0f,
						0.0f, 0.0f, 1.0f, 0.0f,
						offset, offset, 0.0f, 1.0f);
				}
			}
		}

		private Mesh CreateMesh(Game game, string filename, Matrix trans)
		{
			if (filename == "Cone")
				return new Models.Cone(game, trans);
			else if (filename == "Checkpoint")
				return new Models.Checkpoint(game, trans);
			else
				return new Mesh(game, @"Models\" + filename, trans);
		}

		private int GetIndex(int x, int z)
		{
			return (int) ((z * _size) + x);
		}

		public override void Update(GameTime gameTime)
		{
			base.Update(gameTime);

			Matrix viewMatrix = this.GetService<Camera>().ViewMatrix;
			Matrix projectionMatrix = this.GetService<Camera>().ProjectionMatrix;

			_effect.Parameters["WorldViewProjection"].SetValue(translationMatrix * viewMatrix * projectionMatrix);
		}

		public override void Draw(GameTime gameTime)
		{
			Sunlight light = GetService<Sunlight>();
			if (light != null)
				_effect.Parameters["ShadowMapProjector"].SetValue(light.ViewMatrix * light.ProjectionMatrix * _textureScaleAndOffsetMatrix);

			// render geometry with shadow
			ShadowMap shadowMap = GetService<ShadowMap>();
			if (shadowMap != null)
				_effect.Parameters["ShadowMap"].SetValue(shadowMap.ShadowMapTexture);

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

		public float GetHeight(float x, float z)
		{
			//this takes coordinates in world geometry
			
			//convert to heightmap coordinate space
			x = (x - _mapOffset.X) / _mapScale.X;
			z = (z - _mapOffset.Z) / _mapScale.Z;
			
			int integerX = (int)Math.Floor(x);
			int integerZ = (int)Math.Floor(z);
			float fractionalX = x - integerX;
			float fractionalZ = z - integerZ;

			float v1 = GetHeight(integerX, integerZ);
			float v2 = GetHeight(integerX + 1, integerZ);
			float v3 = GetHeight(integerX, integerZ + 1);
			float v4 = GetHeight(integerX + 1, integerZ + 1);

			//float i1 = v1 + (v1, v2, fractionalX);
			//float i2 = PerlinNoise.Interpolate(v3, v4, fractionalX);
			//return PerlinNoise.Interpolate(i1, i2, fractionalZ);

			float i1 = v1 + ((v2 - v1) * fractionalX);
			float i2 = v3 + ((v4 - v3) * fractionalX);

			//convert back to world coordinates
			float height = i1 + ((i2 - i1) * fractionalZ);
			height = (height * _mapScale.Y) + _mapOffset.Y;

			return height;
		}

		private float GetHeight(int x, int z)
		{
			//this takes coordinates in the height map geometry
			if (x >= 0 && x < _size && z >= 0 && z < _size)
			{
				return _heightMap[GetIndex(x, z)];
			}
			else
			{
				// car is off the map
				return 0;
			}	
		}

		public Vector3 GetNormal(float x, float z)
		{
			return new NormalMap(this).GetNormal((int)x, (int)z);
		}
	}
}
