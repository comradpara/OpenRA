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

using OpenRA.Traits;

namespace OpenRA.Mods.RA
{
	class SpeedUpgradeCrateActionInfo : CrateActionInfo
	{
		public float Multiplier = 1.7f;
		public override object Create(Actor self) { return new SpeedUpgradeCrateAction(self, this); }
	}

	class SpeedUpgradeCrateAction : CrateAction
	{
		public SpeedUpgradeCrateAction(Actor self, SpeedUpgradeCrateActionInfo info)
			: base(self, info) {}
				
		public override void Activate(Actor collector)
		{
			collector.World.AddFrameEndTask(w => 
			{
				var multiplier = (info as SpeedUpgradeCrateActionInfo).Multiplier;
				collector.traits.Add(new SpeedUpgrade(multiplier));
			});
			base.Activate(collector);
		}
	}
	
	class SpeedUpgrade : ISpeedModifier
	{
		float multiplier;
		public SpeedUpgrade(float multiplier) {	this.multiplier = multiplier; }
		public float GetSpeedModifier()	{ return multiplier; }
	}
}
