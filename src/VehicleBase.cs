namespace DuckGame.Quahicle
{
    public abstract class VehicleBase : PhysicsObject
    {
        protected float _health = 1.0f;
        protected string _vehicleName = "Unnamed Vehicle";
        protected Duck _pilot = null;
        protected bool _allowPilotOverride = false;
        protected Vec2 _cockpitPosition = new Vec2(0, 0);
        protected bool _mounted = false;
        public StateBinding _mountedBinding = new StateBinding("_mounted");
        public string VehicleName { get => _vehicleName; }
        public Duck Pilot { get => _pilot; }
        public float Health { get => _health; }


        public virtual void SetPilot(Duck d)
        {
            if (this._pilot != null && !this._allowPilotOverride)
                return;

            this._pilot = d;

            d.Fondle(this);
        }

        public virtual void Mount()
        {
            if (this._pilot == null || this._pilot.dead)
                return;

            this._mounted = true;

            this._pilot.moveLock = true;
            this._pilot.CancelFlapping();
        }

        public virtual void UnMount()
        {
            this._mounted = false;

            if (this._pilot == null || this._pilot.dead)
                return;

            this._pilot.moveLock = false;
        }

        public override void Update()
        {
            base.Update();
            UpdatePilot();
            UpdateInput();
        }

        public virtual void UpdatePilot()
        {
            if (this._pilot == null || !this._pilot.isServerForObject)
                return;

            if (!this._mounted)
                return;

            this._pilot.position = this.position + this._cockpitPosition;
        }

        public virtual void UpdateInput()
        {
            if (this._pilot == null || !isServerForObject)
                return;

            InputProfile i = this._pilot.inputProfile;
            if (i == null)
                return;

            // TODO: Temporal. Handle input differently


            if (i.Down("QUACK") && !this._mounted)
            {
                this.Mount();
            }
            if (!this._mounted) return;

            if (i.Down("RAGDOLL"))
            {
                this.UnMount();
            }

            if (i.Down("UP"))
            {
                this.vSpeed -= 1f;
            }

            if (i.Down("DOWN"))
            {
                this.vSpeed += 1f;
            }

            if (i.Down("RIGHT"))
            {
                this.hSpeed += 1f;
            }

            if (i.Down("LEFT"))
            {
                this.hSpeed -= 1f;
            }

            // if (i.Down("STRAFE"))
            // {

            // }
        }

        public override void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
        {
            if (this._mounted && with == this._pilot) return;

            DevConsole.Log(with.ToString());
            base.Impact(with, from, solidImpact);
        }
    }
}