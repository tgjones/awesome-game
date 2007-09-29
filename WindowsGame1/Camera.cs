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

		public void Initialize()
		{
			position = new Vector3(0, 20, 0);
			lookAt = Vector3.Zero;
			cameraUp = new Vector3(0, 0, -1);
		}

		public void Update(GameTime gameTime, GraphicsDevice graphicsDevice)
		{
			//rotate the camera around
			position = Vector3.Transform(new Vector3(20, 20, 0), Matrix.CreateRotationY((float)gameTime.TotalRealTime.TotalMilliseconds * 0.0001f));
			cameraUp = new Vector3(0, 0, -1);

			_viewMatrix = Matrix.CreateLookAt(position, lookAt, cameraUp);
			_projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
				(float)MathHelper.ToRadians(45),
				(float)graphicsDevice.Viewport.Width / (float)graphicsDevice.Viewport.Height,
				1,
				100);
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
