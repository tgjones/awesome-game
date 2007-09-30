using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace AwesomeGame
{
	class Camera
	{
		private Vector3 position;
		private Vector3 lookAt;
		private Vector3 cameraUp;
		private Matrix _viewMatrix;
		private Matrix _projectionMatrix;
		private List<GameObject> viewObjects = new List<GameObject>();

		public void Initialize()
		{
			position = new Vector3(0, 20, 0);
			lookAt = Vector3.Zero;
			cameraUp = new Vector3(0, 0, -1);
		}

		public void Update(GameTime gameTime, GraphicsDevice graphicsDevice)
		{
			Vector3 position;
			float sumX = 0;
			float sumY = 0;
			float sumZ = 0;
			float maxX = 0, minX = 0;
			float maxY = 0, minY = 0;
			float maxZ = 0, minZ = 0;

			GameObject[] gameObjects = viewObjects.ToArray();
			maxX = minX = gameObjects[0].position.X;
			maxZ = minZ = gameObjects[0].position.Z;

			foreach(GameObject gameObject in viewObjects)
			{
				sumX += gameObject.position.X;
				sumY += gameObject.position.Y;
				sumZ += gameObject.position.Z;

				if (gameObject.position.X > maxX)
					maxX = gameObject.position.X;
				if (gameObject.position.Y > maxY)
					maxY = gameObject.position.Y;
				if (gameObject.position.Z > maxZ)
					maxZ = gameObject.position.Z;

				if (gameObject.position.X < minX)
					minX = gameObject.position.X;
				if (gameObject.position.Y < minY)
					minY = gameObject.position.Y;
				if (gameObject.position.Z < minZ)
					minZ = gameObject.position.Z;
			}

			float aspectRatio = (float) graphicsDevice.Viewport.Width / (float) graphicsDevice.Viewport.Height;
			position = new Vector3(
				(float)sumX / viewObjects.Count,
				(float) Math.Sqrt((float) ((maxX - minX) * (maxX - minX)) + ((maxZ - minZ) * (maxZ - minZ) * (aspectRatio * aspectRatio))) * (float) Math.Tan(MathHelper.ToRadians(45f)) + 20 + maxY + 250,
				(float)sumZ / viewObjects.Count + 30);
			lookAt = new Vector3(sumX / viewObjects.Count, maxY, sumZ / viewObjects.Count);
			cameraUp = new Vector3(0, 0, -1);

			_viewMatrix = Matrix.CreateLookAt(position, lookAt, cameraUp);
			_projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				(float)MathHelper.ToRadians(45),
				(float)graphicsDevice.Viewport.Width / (float)graphicsDevice.Viewport.Height,
				1,
				position.Y - minY + 8000);
		}

		public void AddViewObject(GameObject viewObject)
		{
			viewObjects.Add(viewObject);
		}

		public void RemoveViewObject(GameObject viewObject)
		{
			viewObjects.Remove(viewObject);
		}

		#region Properties
		public Matrix ViewMatrix
		{
			get
			{
				return _viewMatrix;
			}
		}

		public Matrix ProjectionMatrix
		{ 
			get 
			{
				return _projectionMatrix;
			}
		}
		#endregion
	}
}
