using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace AwesomeGame.Models
{
	public class CheckpointArrow : Mesh
	{
		public CheckpointArrow(Game game) : base(game, @"Models\DirectionArrow") { }

		public override void UpdateEffects()
		{
			base.UpdateEffects();

			for (int i = 0; i < _modelMeshPartEffects.Count; i++)
			{
				BasicEffect effect = _modelMeshPartEffects[i];
				effect.EmissiveColor = new Vector3(0.0f, 0.0f, 0.8f);
				effect.DiffuseColor = new Vector3(0.1f, 0.1f, 0.2f);
			}
		}
	}
}
