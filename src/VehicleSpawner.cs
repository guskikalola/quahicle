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
        public StateBinding _vehicleBinding = new StateBinding("_vehicle");
        private bool _spawned = false;
        public StateBinding _spawnedBinding = new StateBinding("_spawned");
        public Type contains { get; set; }

        public VehicleSpawner(float xval, float yval) : base(xval, yval)
        {
            this._sprite = new SpriteMap(GetPath("sprites/vehicleSpawner"), 16, 16);
            this.graphic = _sprite;
            this.center = new Vec2(8f, 10f);
            this.collisionOffset = new Vec2(-5f, -10f);
            this.collisionSize = new Vec2(11f, 16f);
            this.dontCrush = true;
            this.graphic.color = Color.CadetBlue;

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
                Graphics.DrawFancyString("Random vehicle", this.position + new Vec2(0, -this.height), Color.White);
                return;
            }

            // string vehicleName = this.GetVehicle().VehicleName;
            // float offsetX = Graphics.GetStringWidth(vehicleName) / 2;
            // Vec2 posText = this.position + new Vec2(-offsetX, -this.height);
            // Vec2 posSprite = posText + new Vec2(0f, -6f);
            // Graphics.DrawFancyString(vehicleName, posText, Color.White);

            // TODO : Find how to centre the text properly. Meanwhile only preview is visible.

            Vec2 posSprite = this.position + new Vec2(0f, -this.height - 6f);
            Sprite preview = this.GetVehicle().graphic.Clone();
            preview.scale = new Vec2(0.2f, 0.2f);
            Graphics.Draw(preview, posSprite.x, posSprite.y); // TODO: Experimental

        }

        public override void OnHoldAction()
        {
            base.OnHoldAction();
            if (!this._spawned)
            {

                this._spawned = true;
                this.graphic.color = Color.White;

                this.SpawnVehicle(this.duck);

                if (this.duck != null)
                    this.duck.Disarm(this);
            }
        }

        public VehicleBase GetVehicle()
        {

            if (Level.current is DuckGameEditor && this.contains == null) return null;

            if (this.contains == null) // No vehicle assigned, then get random 
                this._vehicle = this.GetRandomVehicle();
            else if (this._vehicle == null)
                this._vehicle = Editor.CreateThing(contains) as VehicleBase;

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
            // Unmount from current vehicle
            VehicleBase prevVehicle = Quahicle.Core.GetCurrentVehicle();
            if (prevVehicle != null && prevVehicle.Pilot.Equals(pilot) && prevVehicle.Mounted) prevVehicle.UnMount();

            if (isServerForObject)
            {
                VehicleBase v = this.GetVehicle();
                v.position = this.position + new Vec2(0, -v.height);

                v.SetPilot(pilot);
                Level.Add(v);

                v.Mount();
            }
        }

        // TODO: Fix this setting not affecting spawned vehicle nor being saved
        // This field is not being serialized, figure out how to do that
        public override ContextMenu GetContextMenu()
        {
            FieldBinding binding = new FieldBinding(this, "contains");
            EditorGroupMenu obj = base.GetContextMenu() as EditorGroupMenu;
            obj.InitializeGroups(new EditorGroup(typeof(VehicleBase)), binding);
            return obj;
        }

        public override string GetDetailsString()
        {
            string containString = "EMPTY";
            if (contains != null)
            {
                containString = contains.Name;
            }
            if (contains == null)
            {
                return base.GetDetailsString();
            }
            return base.GetDetailsString() + "Contains: " + containString;
        }

        public override void DrawHoverInfo()
        {
            string containString = "EMPTY";
            if (contains != null)
            {
                containString = contains.Name;
            }
            Graphics.DrawString(containString, position + new Vec2((0f - Graphics.GetStringWidth(containString)) / 2f, -16f), Color.White, 0.9f);
        }
    }
}