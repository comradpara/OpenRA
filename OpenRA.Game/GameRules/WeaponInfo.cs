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

namespace OpenRA.GameRules
{
	public class WeaponInfo
	{
		public readonly int Burst = 1;
		public readonly bool Charges = false;
		public readonly int Damage = 0;
		public readonly string Projectile = "Invisible";
		public readonly int ROF = 1; // in 1/15 second units.
		public readonly float Range = 0;
		public readonly string Report = null;
		public readonly int Speed = -1;
		public readonly bool TurboBoost = false;
		public readonly string Warhead = null;

		public readonly bool RenderAsTesla = false;
		public readonly bool RenderAsLaser = false;
		public readonly bool UsePlayerColor = true;
		public readonly int BeamRadius = 1;
	}
}