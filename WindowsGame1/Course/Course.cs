using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections;

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
			randomiseCheckpoints();
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
				int newCheckpoint = (current + 1) % checkpoints.Count;
				return checkpoints[newCheckpoint];
			}
		}

		public void randomiseCheckpoints()
		{
			checkpoints.Sort(new CheckpointRandomiser());
		}
	}

	public class CheckpointRandomiser : IComparer<GameObject>
	{
		public int Compare(GameObject cp1, GameObject cp2)
		{
			if (cp1 == cp2)
				return 0;
			else
			{
				Random random = new Random();
				return random.Next(-1, +2);
			}
		}
	}
}