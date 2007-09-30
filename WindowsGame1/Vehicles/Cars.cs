using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace AwesomeGame.Vehicles
{
	public class Blocky : Car
	{
		public Blocky(Game game) : base(game, @"Models\Lessblockycar2", 5, 7, 6) { }

		public override void UpdateEffects()
		{
			base.UpdateEffects();

			for (int i = 0; i < _modelMeshPartEffects.Count; i++)
			{
				BasicEffect effect = _modelMeshPartEffects[i];
				if (i == 0)
				{
					effect.DiffuseColor = new Vector3(0.0f, 0.0f, 1.0f);
					effect.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
					effect.SpecularPower = 1000.0f;
				}
				else if (i == 1)
				{
					effect.DiffuseColor = new Vector3(0.0f, 0.0f, 0.0f);
					effect.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.SpecularPower = 1000.0f;
				}
				else if (i == 3)
				{
					effect.EmissiveColor = new Vector3(1.0f, 1.0f, 1.0f);
				}
				else if (i == 4)
				{
					effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
					effect.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.SpecularPower = 1000.0f;
				}
				else
				{
					effect.DiffuseColor = new Vector3(0.02f, 0.02f, 0.02f);
					effect.SpecularColor = new Vector3(0.15f, 0.15f, 0.15f);
					effect.SpecularPower = 1.0f;
				}
			}
		}

		public override void PlayHorn()
		{
			this.horn = Sound.Play("HornBlocky");
		}
	}

	public class Curvy : Car
	{
		public Curvy(Game game) : base(game, @"Models\Curvycar", 6, 3, 0) { }

		public override void UpdateEffects()
		{
			base.UpdateEffects();

			for (int i = 0; i < _modelMeshPartEffects.Count; i++)
			{
				BasicEffect effect = _modelMeshPartEffects[i];
				if (i == 2)
				{
					effect.DiffuseColor = new Vector3(1.0f, 0.2f, 0.2f);
					effect.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
					effect.SpecularPower = 1000.0f;
				}
				else if (i == 5)
				{
					effect.DiffuseColor = new Vector3(0.0f, 0.0f, 0.0f);
					effect.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.SpecularPower = 1000.0f;
				}
				else if (i == 4 || i == 1)
				{
					effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
					effect.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.SpecularPower = 1000.0f;
				}
				else if (i == 7)
				{
					effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.0f);
					effect.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
					effect.SpecularPower = 1000.0f;
				}
				else
				{
					effect.DiffuseColor = new Vector3(0.02f, 0.02f, 0.02f);
					effect.SpecularColor = new Vector3(0.15f, 0.15f, 0.15f);
					effect.SpecularPower = 1.0f;
				}
			}
		}
	}

	public class SchoolBus : Car
	{
		public SchoolBus(Game game) : base(game, @"Models\Schoolbus", 6, 5, 4) { }

		public override void UpdateEffects()
		{
			base.UpdateEffects();

			for (int i = 0; i < _modelMeshPartEffects.Count; i++)
			{
				BasicEffect effect = _modelMeshPartEffects[i];
				if (i == 0)
				{
					effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.5f);
					effect.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.SpecularPower = 1000.0f;
				}
				else if (i == 1)
				{
					effect.DiffuseColor = new Vector3(0.0f, 0.0f, 0.0f);
					effect.SpecularColor = new Vector3(1.0f, 1.0f, 1.0f);
					effect.SpecularPower = 1000.0f;
				}
				else if (i == 3)
				{
					effect.EmissiveColor = new Vector3(1.0f, 1.0f, 1.0f);
				}
				else if (i == 2)
				{
					effect.DiffuseColor = new Vector3(0.5f, 0.5f, 0.0f);
					effect.SpecularColor = new Vector3(0.5f, 0.5f, 0.5f);
					effect.SpecularPower = 1000.0f;
				}
				else
				{
					effect.DiffuseColor = new Vector3(0.02f, 0.02f, 0.02f);
					effect.SpecularColor = new Vector3(0.15f, 0.15f, 0.15f);
					effect.SpecularPower = 1.0f;
				}
			}
		}
	}
}
