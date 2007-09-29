using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace AwesomeGame
{
	class Course
	{
		private List<GameObject> checkpoints;

		public Course(Game game)
		{
			checkpoints = new List<GameObject>();
		}

		public void addCheckpoint(GameObject checkpoint)
		{
			checkpoints.Add(checkpoint);
		}

		public GameObject getFirstCheckpoint()
		{
			return checkpoints.Find(new Predicate<GameObject>(indexFind));
		}

		private bool indexFind(GameObject go)
		{
			return true;
		}

		public GameObject getNextCheckpoint(GameObject currentCheckpoint)
		{
			if (currentCheckpoint == null)
			{
				return checkpoints.Find(new Predicate<GameObject>(indexFind));
			}
			else
			{
				int current = checkpoints.IndexOf(currentCheckpoint);
				//return checkpoints.FindLast(new Predicate<GameObject>(blah));
				int newCheckpoint =  (current + 1) % checkpoints.Count;
				return checkpoints[newCheckpoint];
				
			}
		}
	}
}
