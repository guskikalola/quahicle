using System.Reflection;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DuckGame.Quahicle
{
    public class QuahicleCore : Thing, IEngineUpdatable
    {
        private static QuahicleCore _instance = null;

        public static QuahicleCore Instance
        {
            get
            {
                if (QuahicleCore._instance == null)
                    QuahicleCore._instance = new QuahicleCore();
                return QuahicleCore._instance;
            }
        }

        public QuahicleCore()
        {
            MonoMain.RegisterEngineUpdatable(this);
            (typeof(Game).GetField("drawableComponents", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic).GetValue(MonoMain.instance) as List<IDrawable>).Add(new CustomDraw());
        }

        public void PreUpdate()
        {
        }

        public override void Update()
        {
            base.Update();
        }

        public void PostUpdate()
        {

        }

        public void OnDrawLayer(Layer pLayer)
        {
        }

        public Duck GetCurrentDuck()
        {
            Duck duck;
            if (DuckNetwork.active && DuckNetwork.localProfile != null)
                duck = DuckNetwork.localProfile.duck;
            else
                duck = Profiles.DefaultPlayer1.duck;
            return duck;
        }

        public VehicleBase GetVehicleOf(Duck d)
        {
            List<Thing> things = Level.current.things.ToList();
            Thing match = things.Find(t =>
            {
                VehicleBase v = t as VehicleBase;
                if (v == null || v.Pilot == null) return false;
                return v.Pilot.Equals(d);
            });
            return (match as VehicleBase);
        }

        public VehicleBase GetCurrentVehicle()
        {
            Duck duck = this.GetCurrentDuck();
            if (duck == null)
                return null;

            return this.GetVehicleOf(duck);
        }
    }
}