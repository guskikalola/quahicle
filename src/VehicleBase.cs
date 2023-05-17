using System;

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
        protected bool _accelerating = false;
        public StateBinding _acceleratingBinding = new StateBinding("_accelerating");
        public float _directionAngle = 0f;
        public StateBinding _directionAngleBinding = new StateBinding("_directionAngle", -1, false, false);
        protected float _jumpPower = 0f;
        protected bool lockH = false;
        protected bool lockV = false;
        protected float maxHSpeed = 5.0f;
        protected float maxVSpeed = 5.0f;
        protected float acceleration = 1.0f;
        protected bool keepDirectionUnMounted = false;


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

        public Vec2 WorldCockpitPosition // TODO: Take rotation into account when calculation position
        {
            get
            {
                float xDir = this.offDir;
                float yDir = 1; // this._directionAngle > 180f ? 1 : -1;
                Vec2 directionalMult = new Vec2(xDir, yDir);
                return this.position + this._cockpitPosition * directionalMult;
            }
        }

        public Vec2 DirectionVector
        {
            get
            {
                float rads = Maths.DegToRad(this._directionAngle);
                return new Vec2(Maths.FastCos(rads), Maths.FastSin(rads));
            }
        }

        public virtual void SetPilot(Duck d)
        {
            // TODO: Handle when a pilot takes another vehicle 
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
            this.OnMount();
        }

        public virtual void OnMount()
        {

        }

        public virtual void UnMount()
        {
            this._mounted = false;

            if (this._pilot == null || this._pilot.dead)
                return;

            this._pilot.moveLock = false;
            this._pilot.vSpeed += -1f;

            this.OnUnMount();
        }

        public virtual void OnUnMount()
        {
            this._accelerating = false;
        }

        public override void Update()
        {
            base.Update();
            UpdatePilot();
            UpdateInput();

            // TODO: Rotate the sprite to match direction

            if (this._directionAngle > 90f && this._directionAngle < 270f)
                this.graphic.flipV = true;
            else
                this.graphic.flipV = false;

            this.angleDegrees = this._directionAngle;
        }

        public virtual void UpdatePilot()
        {
            if (this._pilot == null || !this._pilot.isServerForObject)
                return;

            if (!this._mounted)
                return;

            this._pilot.position = this.WorldCockpitPosition;
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

            if (i.Down("SHOOT"))
                this.Fire();

            // Handle direction
            bool right = false;
            bool left = false;
            bool up = false;
            bool down = false;
            if (!this.lockH && ((double)i.leftStick.x > (double)0.4f || i.Down("RIGHT")))
            {
                right = true;
                this._pilot.offDir = (sbyte)1;
                // this.offDir = (sbyte)1;
            }
            if (!this.lockH && ((double)i.leftStick.x < -(double)0.4f || i.Down("LEFT")))
            {
                left = true;
                this._pilot.offDir = (sbyte)-1;
                // this.offDir = (sbyte)-1;
            }
            if (!this.lockV && ((double)i.leftStick.y > (double)0.4f || i.Down("UP")))
                up = true;
            if (!this.lockV && ((double)i.leftStick.y < -(double)0.4f || i.Down("DOWN")))
                down = true;

            // this._directionAngle = !flag4 ? (!flag5 ? 0.0f : (!(flag3 | flag2) ? 90f : 45f)) : (!(flag3 | flag2) ? -90f : -45f);

            if (up)
                this._directionAngle = 270f;

            if (down)
                this._directionAngle = 90f;

            if (right)
                this._directionAngle = 0f;

            if (left)
                this._directionAngle = 180f;

            if (right && down)
                this._directionAngle = 45f;

            if (right && up)
                this._directionAngle = 315f;

            if (left && down)
                this._directionAngle = 135f;

            if (left && up)
                this._directionAngle = 225f;


            // Accelerate if any direction key is pressed
            if (right || left || up || down)
            {
                this._accelerating = true;
                this.Accelerate();
            }
            else
            {
                this._accelerating = false;
                if (this._mounted || !this.keepDirectionUnMounted) this._directionAngle = this._pilot.offDir > 0 ? 0f : 180f;
            }

        }

        public virtual void Accelerate()
        {
            Vec2 speedAdd = this.DirectionVector * _speed * acceleration;
            this.hSpeed += speedAdd.x;
            this.vSpeed += speedAdd.y;
        }

        public virtual void Jump()
        {
            if (this.grounded)
            {
                this.vSpeed -= this._jumpPower;
            }
        }

        public virtual void Fire()
        {

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