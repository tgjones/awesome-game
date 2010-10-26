using System;
using Microsoft.Xna.Framework;

namespace AwesomeGame.Terrain
{
	/// <summary>
	/// Summary description for NormalMap.
	/// </summary>
	public class NormalMap
	{
		#region Fields

		private SimpleTerrain _terrain;

		#endregion

		#region Constructor

		public NormalMap(SimpleTerrain terrain)
		{
			_terrain = terrain;
		}

		#endregion

		#region Methods

		public Vector3 GetNormal(int x, int z)
		{
			return CalculateNormal(
				_terrain.GetPosition(x - 1, z - 1),
				_terrain.GetPosition(x - 0, z - 1),
				_terrain.GetPosition(x + 1, z - 1),
				_terrain.GetPosition(x - 1, z - 0),
				_terrain.GetPosition(x - 0, z - 0),
				_terrain.GetPosition(x + 1, z - 0),
				_terrain.GetPosition(x - 1, z + 1),
				_terrain.GetPosition(x - 0, z + 1),
				_terrain.GetPosition(x + 1, z + 1));
		}

		/*public Vector3 CalculateNormal(float fMapX, float fMapY)
		{
			// convert coordinates to heightmap scale
			fMapX *= m_nWidth - 1;
			fMapY *= m_nHeight - 1;

			// calculate integer and fractional parts of coordinates
			int nIntX0 = (int) Math.Floor(fMapX);
			int nIntY0 = (int) Math.Floor(fMapY);
			float fFractionalX = fMapX - nIntX0;
			float fFractionalY = fMapY - nIntY0;

			// get coordinates for "other" side of quad
			int nIntX1 = (int) MathHelper.Clamp(nIntX0 + 1, 0, m_nWidth - 1);
			int nIntY1 = (int) MathHelper.Clamp(nIntY0 + 1, 0, m_nHeight - 1);

			// read normals for vertices
			Vector3 t0 = m_pNormals[nIntX0, nIntY0];
			Vector3 t1 = m_pNormals[nIntX1, nIntY0];
			Vector3 t2 = m_pNormals[nIntX0, nIntY1];
			Vector3 t3 = m_pNormals[nIntX1, nIntY1];

			// average the results
			Vector3 tAverageLo = (t1 * fFractionalX) + (t0 * (1.0f - fFractionalX));
			Vector3 tAverageHi = (t3 * fFractionalX) + (t2 * (1.0f - fFractionalX));

			// calculate normal
			Vector3 tNormal = (tAverageHi * fFractionalY) + (tAverageLo * (1.0f - fFractionalY));

			// renormalise
			tNormal.Normalize();

			return tNormal;
		}*/

		private Vector3 CalculateNormal(
			Vector3 tVertex0,
			Vector3 tVertex1,
			Vector3 tVertex2,
			Vector3 tVertex3,
			Vector3 tVertex4,
			Vector3 tVertex5,
			Vector3 tVertex6,
			Vector3 tVertex7,
			Vector3 tVertex8)
		{
			// calculate face normals
			Vector3 tFace0 = Vector3.Cross(tVertex3 - tVertex4, tVertex0 - tVertex4);
			Vector3 tFace1 = Vector3.Cross(tVertex0 - tVertex4, tVertex1 - tVertex4);
			Vector3 tFace2 = Vector3.Cross(tVertex1 - tVertex4, tVertex2 - tVertex4);
			Vector3 tFace3 = Vector3.Cross(tVertex2 - tVertex4, tVertex5 - tVertex4);
			Vector3 tFace4 = Vector3.Cross(tVertex5 - tVertex4, tVertex8 - tVertex4);
			Vector3 tFace5 = Vector3.Cross(tVertex8 - tVertex4, tVertex7 - tVertex4);
			Vector3 tFace6 = Vector3.Cross(tVertex7 - tVertex4, tVertex6 - tVertex4);
			Vector3 tFace7 = Vector3.Cross(tVertex6 - tVertex4, tVertex3 - tVertex4);

			tFace0.Normalize();
			tFace1.Normalize();
			tFace2.Normalize();
			tFace3.Normalize();
			tFace4.Normalize();
			tFace5.Normalize();
			tFace6.Normalize();
			tFace7.Normalize();

			// add face normals
			Vector3 tNormal = tFace0 + tFace1 + tFace2 + tFace3
				+ tFace4 + tFace5 + tFace6 + tFace7;

			// normalise vector
			tNormal.Normalize();

			// return vertex normal
			return tNormal;
		}

		#endregion
	}
}
