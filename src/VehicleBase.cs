namespace DuckGame.Quahicle
{
    public abstract class VehicleBase : PhysicsObject
    {
        protected float _health = 1.0f;
        protected string _vehicleName = "Unnamed Vehicle";
        protected float _speed = 1.0f;
        protected Duck _pilot = null;
        protected bool _allowPilotOverride = false;
        protected Vec2 _cockpitPosition = new Vec2(0, 0);
        protected bool _mounted = false;
        public StateBinding _mountedBinding = new StateBinding("_mounted");
        public float _directionAngle = 0f;
        public StateBinding _directionAngleBinding = new StateBinding("_directionAngle", -1, false, false);
        protected float _jumpPower = 0f;
        protected bool lockH = false;
        protected bool lockV = false;
        protected float maxHSpeed = 5.0f;
        protected float maxVSpeed = 5.0f;


        protected VehicleBase(float xval, float yval) : base(xval, yval)
        {
            this.layer = Layer.Blocks;
            this.isStatic = true;
            this.dontCrush = true;
            this.alpha = 0.99f;
            // this.gravMultiplier = 0f;
        }

        protected VehicleBase() : this(0, 0)
        {
        }

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


            if (i.Down("QUACK") && !this._mounted && this._pilot.Distance(this) < 15.0f)
                this.Mount();

            if (!this._mounted) return;

            if (i.Down("RAGDOLL"))
                this.UnMount();

            if (i.Down("JUMP"))
                this.Jump();

            // Handle direction
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            bool flag5 = false;
            if (!this.lockH && ((double)i.leftStick.x > (double)0.4f || i.Down("RIGHT")))
            {
                flag2 = true;
                this._pilot.offDir = (sbyte)1;
                this.offDir = (sbyte)1;
                this.graphic.flipV = false;
                this.graphic.flipH = false;

            }
            if (!this.lockH && ((double)i.leftStick.x < -(double)0.4f || i.Down("LEFT")))
            {
                flag3 = true;
                this._pilot.offDir = (sbyte)-1;
                this.offDir = (sbyte)-1;
                this.graphic.flipH = true;
                // this.graphic.flipV = true;

            }
            if (!this.lockV && ((double)i.leftStick.y > (double)0.4f || i.Down("UP")))
                flag4 = true;
            if (!this.lockV && ((double)i.leftStick.y < -(double)0.4f || i.Down("DOWN")))
                flag5 = true;

            this._directionAngle = !flag4 ? (!flag5 ? 0.0f : (!(flag3 | flag2) ? 90f : 45f)) : (!(flag3 | flag2) ? -90f : -45f);

            // Accelerate if any direction key is pressed
            if (flag2 || flag3 || flag4 || flag5)
                this.Accelerate();

        }

        public virtual void Accelerate()
        {
            this.hSpeed += (this.offDir >= (sbyte)0 ? 1 : -1) * _speed;
            this.vSpeed += (this._directionAngle == 0 ? 0 : this._directionAngle > 0 ? 1 : -1) * _speed;
        }

        public virtual void Jump()
        {
            if (this.grounded)
            {
                this.vSpeed -= this._jumpPower;
            }
        }

        public override void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
        {
            if (this._mounted && with == this._pilot) return;

            base.Impact(with, from, solidImpact);
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            this.hSpeed = MathHelper.Clamp(this.hSpeed, -this.maxHSpeed, this.maxHSpeed);
            
            // TODO: Search for an alternative to deal with jumpPower. If jumpPower exceeds maxVSpeed then the extra power is rendered useless.
            // by adding the jumpPower to the maxVSpeed this extra power is taken into account but can result in unexpected behaviour.
            this.vSpeed = MathHelper.Clamp(this.vSpeed, -(this._jumpPower + this.maxVSpeed), this.maxVSpeed); 
        }
    }
}