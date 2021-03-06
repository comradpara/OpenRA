#region Copyright & License Information
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
using System.Drawing;
using System.Linq;
using OpenRA.FileFormats;
using OpenRA.GameRules;
using OpenRA.Traits;

namespace OpenRA
{
	public static class WorldUtils
	{
		public static bool IsPathableCell(this World world, int2 a, UnitMovementType umt)
		{
			if (world.WorldActor.traits.Get<BuildingInfluence>().GetBuildingAt(a) != null) return false;
			if (world.WorldActor.traits.Get<UnitInfluence>().GetUnitsAt(a).Any()) return false;

			return world.Map.IsInMap(a.X, a.Y) &&
				Rules.TerrainTypes[world.TileSet.GetTerrainType(world.Map.MapTiles[a.X, a.Y])]
				.GetCost(umt) < float.PositiveInfinity;
		}
		
		public static bool IsCellBuildable(this World world, int2 a, bool waterBound)
		{
			return world.IsCellBuildable(a, waterBound, null);	
		}
		
		public static bool IsCellBuildable(this World world, int2 a, bool waterBound, Actor toIgnore)
		{
			if (world.WorldActor.traits.Get<BuildingInfluence>().GetBuildingAt(a) != null) return false;
			if (world.WorldActor.traits.Get<UnitInfluence>().GetUnitsAt(a).Any(b => b != toIgnore)) return false;
			
			if (waterBound)
				return world.Map.IsInMap(a.X,a.Y) && GetTerrainType(world,a) == TerrainType.Water;

			return world.Map.IsInMap(a.X, a.Y) &&
				Rules.TerrainTypes[world.TileSet.GetTerrainType(world.Map.MapTiles[a.X, a.Y])]
				.Buildable;
		}

		public static bool IsActorCrushableByActor(this World world, Actor a, Actor b)
		{
			var movement = b.traits.GetOrDefault<IMovement>();
			return movement != null && world.IsActorCrushableByMovementType(a, movement.GetMovementType());
		}
		
		public static bool IsActorPathableToCrush(this World world, Actor a, UnitMovementType umt)
		{
			return a != null &&
					a.traits.WithInterface<ICrushable>()
					.Any(c => c.IsPathableCrush(umt, a.Owner));
		}
		
		public static bool IsActorCrushableByMovementType(this World world, Actor a, UnitMovementType umt)
		{
			return a != null &&
				a.traits.WithInterface<ICrushable>()
				.Any(c => c.IsCrushableBy(umt, a.Owner));
		}

		public static IEnumerable<Actor> FindUnitsAtMouse(this World world, int2 mouseLocation)
		{
			var loc = mouseLocation + Game.viewport.Location;
			return FindUnits(world, loc, loc);
		}

		public static IEnumerable<Actor> FindUnits(this World world, float2 a, float2 b)
		{
			var u = float2.Min(a, b).ToInt2();
			var v = float2.Max(a, b).ToInt2();
			return world.WorldActor.traits.Get<SpatialBins>().ActorsInBox(u,v);
		}

		public static IEnumerable<Actor> FindUnitsInCircle(this World world, float2 a, float r)
		{
			var min = a - new float2(r, r);
			var max = a + new float2(r, r);

			var rect = new RectangleF(min.X, min.Y, max.X - min.X, max.Y - min.Y);

			var inBox = world.Actors.Where(x => x.GetBounds(false).IntersectsWith(rect));

			return inBox.Where(x => (x.CenterLocation - a).LengthSquared < r * r);
		}

		public static IEnumerable<int2> FindTilesInCircle(this World world, int2 a, int r)
		{
			var min = a - new int2(r, r);
			var max = a + new int2(r, r);
			if (min.X < world.Map.XOffset) min.X = world.Map.XOffset;
			if (min.Y < world.Map.YOffset) min.Y = world.Map.YOffset;
			if (max.X > world.Map.XOffset + world.Map.Width - 1) max.X = world.Map.XOffset + world.Map.Width - 1;
			if (max.Y > world.Map.YOffset + world.Map.Height - 1) max.Y = world.Map.YOffset + world.Map.Height - 1;

			for (var j = min.Y; j <= max.Y; j++)
				for (var i = min.X; i <= max.X; i++)
					if (r * r >= (new int2(i, j) - a).LengthSquared)
						yield return new int2(i, j);
		}

		public static IEnumerable<Actor> SelectActorsInBox(this World world, float2 a, float2 b)
		{
			return world.FindUnits(a, b)
				.Where( x => x.traits.Contains<Selectable>() && x.IsVisible() )
				.GroupBy(x => (x.Owner == world.LocalPlayer) ? x.Info.Traits.Get<SelectableInfo>().Priority : 0)
				.OrderByDescending(g => g.Key)
				.Select( g => g.AsEnumerable() )
				.DefaultIfEmpty( new Actor[] {} )
				.FirstOrDefault();
		}
		
		public static TerrainType GetTerrainType(this World world, int2 cell)
		{
			return (TerrainType)world.TileSet.GetTerrainType(world.Map.MapTiles[cell.X, cell.Y]);
		}
		
		public static bool CanPlaceBuilding(this World world, string name, BuildingInfo building, int2 topLeft, Actor toIgnore)
		{
			var res = world.WorldActor.traits.Get<ResourceLayer>();
			return !Footprint.Tiles(name, building, topLeft).Any(
				t => !world.Map.IsInMap(t.X, t.Y) || res.GetResource(t) != null || !world.IsCellBuildable(t,
					building.WaterBound, toIgnore));
		}

		public static bool IsVisible(this Actor a)
		{
			var shroud = a.World.WorldActor.traits.Get<Shroud>();
			return a.traits.Contains<Unit>()
				? Shroud.GetVisOrigins(a).Any(o => shroud.visibleCells[o.X, o.Y] > 0)
				: Shroud.GetVisOrigins(a).Any(o => shroud.exploredCells[o.X, o.Y]);
		}

		public static bool IsCloseEnoughToBase(this World world, Player p, string buildingName, BuildingInfo bi, int2 topLeft)
		{
			var buildingMaxBounds = bi.Dimensions;
			if( bi.Bib )
				buildingMaxBounds.Y += 1;

			var scanStart = world.ClampToWorld( topLeft - new int2( bi.Adjacent, bi.Adjacent ) );
			var scanEnd = world.ClampToWorld( topLeft + buildingMaxBounds + new int2( bi.Adjacent, bi.Adjacent ) );

			var nearnessCandidates = new List<int2>();

			for( int y = scanStart.Y ; y < scanEnd.Y ; y++ )
			{
				for( int x = scanStart.X ; x < scanEnd.X ; x++ )
				{
					var at = world.WorldActor.traits.Get<BuildingInfluence>().GetBuildingAt( new int2( x, y ) );
					if( at != null && at.Owner == p && at.Info.Traits.Get<BuildingInfo>().BaseNormal)
						nearnessCandidates.Add( new int2( x, y ) );
				}
			}
			var buildingTiles = Footprint.Tiles( buildingName, bi, topLeft ).ToList();
			return nearnessCandidates
				.Any( a => buildingTiles
					.Any( b => Math.Abs( a.X - b.X ) <= bi.Adjacent
							&& Math.Abs( a.Y - b.Y ) <= bi.Adjacent ) );
		}

		static int2 ClampToWorld( this World world, int2 xy )
		{
			var mapStart = world.Map.Offset;
			var mapEnd = world.Map.Offset + world.Map.Size;
			if( xy.X < mapStart.X )
				xy.X = mapStart.X;
			if( xy.X > mapEnd.X )
				xy.X = mapEnd.X;

			if( xy.Y < mapStart.Y )
				xy.Y = mapStart.Y;
			if( xy.Y > mapEnd.Y )
				xy.Y = mapEnd.Y;

			return xy;
		}

		public static int2 ChooseRandomEdgeCell(this World w)
		{
			var isX = w.SharedRandom.Next(2) == 0;
			var edge = w.SharedRandom.Next(2) == 0;

			return new int2(
				isX ? w.SharedRandom.Next(w.Map.XOffset, w.Map.XOffset + w.Map.Width)
					: (edge ? w.Map.XOffset : w.Map.XOffset + w.Map.Width),
				!isX ? w.SharedRandom.Next(w.Map.YOffset, w.Map.YOffset + w.Map.Height)
					: (edge ? w.Map.YOffset : w.Map.YOffset + w.Map.Height));
		}

		public static int2 ChooseRandomCell(this World w, Thirdparty.Random r)
		{
			return new int2(
				r.Next(w.Map.XOffset, w.Map.XOffset + w.Map.Width),
				r.Next(w.Map.YOffset, w.Map.YOffset + w.Map.Height));
		}

		public static IEnumerable<CountryInfo> GetCountries(this World w)
		{
			return w.WorldActor.Info.Traits.WithInterface<CountryInfo>();
		}

		public static float Gauss1D(this Thirdparty.Random r, int samples)
		{
			var xs = Graphics.Util.MakeArray(samples, _ => (float)r.NextDouble() * 2 - 1);
			return xs.Sum() / samples;
		}

		// Returns a random offset in the range [-1..1,-1..1] with a separable 
		// Gauss distribution with 'samples' values taken for each axis
		public static float2 Gauss2D(this Thirdparty.Random r, int samples)
		{
			return new float2(Gauss1D(r, samples), Gauss1D(r, samples));
		}
	}
}
