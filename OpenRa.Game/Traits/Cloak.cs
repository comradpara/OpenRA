﻿using System.Collections.Generic;
using System.Linq;
using OpenRa.Game.Graphics;

namespace OpenRa.Game.Traits
{
	class Cloak : IRenderModifier, INotifyAttack, ITick
	{
		int remainingUncloakTime = 2;		/* setup for initial cloak */

		public Cloak(Actor self) {}

		public void Attacking(Actor self)
		{
            if (remainingUncloakTime <= 0)
				OnCloak();

            remainingUncloakTime = (int)(Rules.General.SubmergeDelay * 60 * 25);
		}

		public IEnumerable<Renderable>
			ModifyRender(Actor self, IEnumerable<Renderable> rs)
		{
            if (remainingUncloakTime > 0)
				return rs;

			if (self.Owner == Game.LocalPlayer)
				return rs.Select(a => a.WithPalette(PaletteType.Shadow));
			else
				return new Renderable[] { };
		}

		public void Tick(Actor self)
		{
            if (remainingUncloakTime > 0)
                if (--remainingUncloakTime <= 0)
					OnUncloak();
		}

		void OnCloak()
		{
			Sound.Play("ironcur9.aud");
		}

		void OnUncloak()
		{
			Sound.Play("ironcur9.aud");		/* is this the right sound?? */
		}
	}
}