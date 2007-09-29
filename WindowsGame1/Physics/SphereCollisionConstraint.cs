using System;
using Microsoft.Xna.Framework;

namespace AwesomeGame.Physics
{
	public class SphereCollisionConstraint : Constraint
	{
		private Particle _particle;
		private BoundingSphere _boundingSphere;

		public SphereCollisionConstraint(ParticleSystem particleSystem, Particle particle, BoundingSphere boundingSphere, float stiffness)
			: base(particleSystem, stiffness)
		{
			_particle = particle;
			_boundingSphere = boundingSphere;
		}

		public SphereCollisionConstraint(ParticleSystem particleSystem, Particle particle, BoundingSphere boundingSphere)
			: this(particleSystem, particle, boundingSphere, 1)
		{

		}

		public override void Project()
		{
			if (_boundingSphere.Contains(_particle.CandidatePosition) == ContainmentType.Contains)
			{
				Vector3 direction = _particle.CandidatePosition - _boundingSphere.Center;
				direction.Normalize();
				_particle.CandidatePosition = _boundingSphere.Center + (_boundingSphere.Radius * direction);
			}
		}
	}
}
