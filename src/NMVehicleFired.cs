using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DuckGame.Quahicle
{
    class NMVehicleFired : NMEvent
    {
        public Duck _pilot;
        public NMVehicleFired(Duck pilot)
        {
            _pilot = pilot;
        }

        public NMVehicleFired()
        {
        }

        public override void Activate()
        {
            VehicleBase v = Quahicle.Core.GetVehicleOf(this._pilot);
            if (v == null) return;

            v.Fire();
        }
    }
}
