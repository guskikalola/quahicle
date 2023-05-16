using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame.Quahicle
{
    [EditorGroup("Quahicle|Tools")]
    public class VehicleSpawner : Holdable, IContainAThing
    {
        private SpriteMap _sprite;
        private VehicleBase _vehicle;
        private bool _spawned = false;
        public StateBinding _spawnedBinding = new StateBinding("_spawned");
        public Type contains { get; set; }

        public VehicleSpawner(float xval, float yval) : base(xval, yval)
        {
            this._sprite = new SpriteMap("pistol", 18, 10);
            this.graphic = _sprite;
            this.center = new Vec2(10f, 3f);
            this.collisionOffset = new Vec2(-8f, -3f);
            this.collisionSize = new Vec2(16f, 9f);

            this._editorName = "Vehicle Spawner";
            this.editorTooltip = "A whole vehicle is contained inside!";
        }

        public override void Draw()
        {
            base.Draw();

            if (this._spawned)
                return;

            if (this.GetVehicle() == null)
            {
                Graphics.DrawFancyString("Random vehicle", this.position + new Vec2(0, this.height), Color.White);
                return;
            }

            string vehicleName = this.GetVehicle().VehicleName;

            Graphics.DrawFancyString(vehicleName, this.position + new Vec2(0, this.height), Color.White);
        }

        public override void OnHoldAction()
        {
            base.OnHoldAction();
            if (!this._spawned)
            {

                this._spawned = true;
                this.SpawnVehicle(this.duck);

                if (this.duck != null)
                    this.duck.Disarm(this);
            }
        }

        public VehicleBase GetVehicle()
        {

            if (Level.current is DuckGameEditor && this.contains == null) return null;

            if (this._vehicle == null) // No vehicle assigned, then get random 
                this._vehicle = this.GetRandomVehicle();

            return this._vehicle;
        }

        private VehicleBase GetRandomVehicle()
        {
            List<Type> vehicles = this.GetVehicles();
            contains = vehicles[Rando.Int(vehicles.Count - 1)];
            VehicleBase newVehicle = Editor.CreateThing(contains) as VehicleBase;
            return newVehicle;
        }

        private List<Type> GetVehicles()
        {
            return Editor.ThingTypes.Where(delegate (Type t)
            {
                if (t.IsAbstract || !t.IsSubclassOf(typeof(VehicleBase)))
                {
                    return false;
                }
                if (t.GetCustomAttributes(typeof(EditorGroupAttribute), inherit: false).Length == 0)
                {
                    return false;
                }
                IReadOnlyPropertyBag bag = ContentProperties.GetBag(t);
                return (bag.GetOrDefault("canSpawn", defaultValue: true) && (!Network.isActive || !bag.GetOrDefault("noRandomSpawningOnline", defaultValue: false)) && (!Network.isActive || bag.GetOrDefault("isOnlineCapable", defaultValue: true)) && (Main.isDemo || !bag.GetOrDefault("onlySpawnInDemo", defaultValue: false))) ? true : false;
            }).ToList();
        }

        private void SpawnVehicle(Duck pilot)
        {
            if (isServerForObject)
            {
                VehicleBase v = this.GetVehicle();
                v.position = this.position + new Vec2(0,v.height);
                v.SetPilot(pilot);
                Level.Add(v);

                v.Mount();
            }
        }

        public override ContextMenu GetContextMenu()
        {
            FieldBinding binding = new FieldBinding(this, "contains");
            EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
            obj.InitializeGroups(new EditorGroup(typeof(VehicleBase)), binding);
            return obj;
        }
    }
}