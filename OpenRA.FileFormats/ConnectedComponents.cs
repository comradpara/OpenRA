﻿#region Copyright & License Information
/*
 * Copyright 2007,2009,2010 Chris Forbes, Robert Pepperell, Matthew Bowra-Dean, Paul Chote, Alli Witheford.
 * This file is part of OpenRA.
 * 
 *  OpenRA is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 * 
 *  OpenRA is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 * 
 *  You should have received a copy of the GNU General Public License
 *  along with OpenRA.  If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using System;
using System.Collections.Generic;

namespace OpenRA.FileFormats
{
	public static class ConnectedComponents
	{
		static readonly int2[] Neighbors 
			= { new int2(-1, -1), new int2(0, -1), new int2(1, -1), new int2(-1, 0) };

		public static int[,] Extract(Map m, Func<int, int, int> f)
		{
			var result = new int[m.MapSize, m.MapSize];
			var types = new int[m.MapSize, m.MapSize];
			var d = new Dictionary<int, Node>();
			var n = 1;

			for( var j = m.YOffset; j < m.YOffset + m.Height; j++ )
				for (var i = m.XOffset; i < m.XOffset + m.Width; i++)
				{
					types[i, j] = f(i, j);

					var k = n;

					foreach (var a in Neighbors)
						if (types[i + a.X, j + a.Y] == types[i, j])
							k = (k == n) 
								? result[i + a.X, j + a.Y]
								: Union( d, k, result[i+a.X, j+a.Y] );

					result[i,j] = k;
					if (k == n) MakeSet(d, n++);
				}

			for (var j = m.YOffset; j < m.YOffset + m.Height; j++)
				for (var i = m.XOffset; i < m.XOffset + m.Width; i++)
					result[i, j] = Find(d, result[i, j]);

			return result;
		}

		// disjoint-set forest stuff

		class Node { public int a, b; public Node(int a, int b) { this.a = a; this.b = b; } }

		static int MakeSet(Dictionary<int, Node> d, int x)
		{
			d[x] = new Node(x, 0);
			return x;
		}

		static int Union(Dictionary<int, Node> d, int x, int y)
		{
			var xr = Find(d, x);
			var yr = Find(d, y);
			var xa = d[xr].b;
			var ya = d[yr].b;

			if (xa > ya) d[yr].a = xr;
			else if (xa < ya) d[xr].a = yr;
			else if (xr != yr)
			{
				d[yr].a = xr;
				++d[xr].b;
			}

			return xr;
		}

		static int Find(Dictionary<int, Node> d, int x)
		{
			if (d[x].a == x) return x;
			return d[x].a = Find(d, d[x].a);
		}

	}
}
