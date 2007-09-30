using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace AwesomeGame
{
	public class Sunlight : GameObject
	{
		#region Fields

		private Matrix _view, _projection;
		private readonly Vector4 _diffuse;
		private readonly Vector4 _ambient;

		#endregion

		#region Properties

		public Matrix ViewMatrix
		{
			get { return _view; }
		}

		public Matrix ProjectionMatrix
		{
			get { return _projection; }
		}

		public Vector3 Direction
		{
			get { return Vector3.Normalize(new Vector3(2, -1, 1)); }
		}

		public Vector4 Diffuse
		{
			get { return _diffuse; }
		}

		public Vector4 Ambient
		{
			get { return _ambient; }
		}

		#endregion

		public Sunlight(Game game)
			: base(game)
		{
			game.Services.AddService(typeof(Sunlight), this);
			this.UpdateOrder = 2500;

			_diffuse = new Vector4(0.7f, 0.7f, 0.7f, 1);
			_ambient = new Vector4(0.25f, 0.25f, 0.25f, 1);
		}

		public override void Update(GameTime gameTime)
		{
			// create view matrix with position as the centre of the top face of the bounding box
			List<Vector3> positions = new List<Vector3>();
			foreach (GameComponent gameComponent in this.Game.Components)
				if (gameComponent is Vehicles.Car)
				{
					const int FUDGE_SHADOW_SIZE = 50;
					positions.Add(((GameObject) gameComponent).position - new Vector3(FUDGE_SHADOW_SIZE));
					positions.Add(((GameObject) gameComponent).position + new Vector3(FUDGE_SHADOW_SIZE));
				}
			BoundingBox sceneBoundingBox = BoundingBox.CreateFromPoints(positions);
			Vector3[] boundingBoxCorners = sceneBoundingBox.GetCorners();
			Vector3 boundingBoxCentre = Vector3.Zero;
			foreach (Vector3 boundingBoxCorner in boundingBoxCorners)
				boundingBoxCentre += boundingBoxCorner;
			boundingBoxCentre /= BoundingBox.CornerCount;

			_view = Matrix.CreateLookAt(
				boundingBoxCentre - (this.Direction),
				boundingBoxCentre,
				Vector3.Up);

			// transform bounding box vertices by light view matrix. this gives us the bounding box
			// in light view space, from which we can create an orthographic projection
			// based on the dimensions of this AABB
			Vector3[] transformedCorners = new Vector3[BoundingBox.CornerCount];
			Vector3.Transform(boundingBoxCorners, ref _view, transformedCorners);
			BoundingBox lightViewBoundingBox = BoundingBox.CreateFromPoints(transformedCorners);

			float farZ = lightViewBoundingBox.Max.Z - lightViewBoundingBox.Min.Z;

			// Essentially, for this situation you have to dynamically compute a temporary "working" position for your light.
			// One way to do it would be to transform the eight corners of the camera frustum into the light's "view" space.
			// By doing this you can compute the FarZ (far clip plane distance) which would be the difference between the max.z and min.z
			// corners of the camera frustum points in light view space.
			// You could then position the light by backing up from the centroid of the camera frustum in the opposite direction
			// of the light by the amount of max.z. Your near clip plane would be 0.0, your far clip plane would max.z - min.z,
			// your look at point would be the camera's frustum centroid, and your up vector would be anything orthogonal to
			// your light's direction. You would build an othographic projection and your max/min values would simply by max.x & max.y
			// and min.x & min.y.
			// Hopefully that makes some sense. :)

			_projection = Matrix.CreateOrthographicOffCenter(
				lightViewBoundingBox.Min.X,
				lightViewBoundingBox.Max.X,
				lightViewBoundingBox.Min.Y,
				lightViewBoundingBox.Max.Y,
				0,
				farZ + 2000);

			_view = Matrix.CreateLookAt(
				boundingBoxCentre - (this.Direction * lightViewBoundingBox.Max.Z),
				boundingBoxCentre,
				Vector3.Up);

			base.Update(gameTime);
		}
	}
}