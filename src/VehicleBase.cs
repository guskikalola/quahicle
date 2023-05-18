namespace DuckGame.Quahicle
{
    public abstract class VehicleBase : PhysicsObject
    {
        public float Health { get; protected set; } = 1.0f;
        public string VehicleName { get; protected set; } = "Unnamed Vehicle";
        public float Speed { get; protected set; } = 1.0f;
        public Duck Pilot { get; protected set; } = null;
        public Vec2 CockpitPosition { get; protected set; } = new Vec2(0, 0);
        public bool Mounted { get; protected set; } = false;
        public StateBinding MountedBinding = new StateBinding("_mounted");
        public bool Accelerating { get; protected set; } = false;
        public StateBinding AcceleratingBinding = new StateBinding("_accelerating");
        public float DirectionAngle { get; protected set; } = 0f;
        public StateBinding DirectionAngleBinding = new StateBinding("_directionAngle", -1, false, false);
        public float JumpPower { get; protected set; } = 0f;
        public bool LockH { get; protected set; } = false;
        public bool LockV { get; protected set; } = false;
        public float MaxHSpeed { get; protected set; } = 5.0f;
        public float MaxVSpeed { get; protected set; } = 5.0f;
        public float AccelerationMul { get; protected set; } = 1.0f;
        public bool KeepDirectionUnMounted { get; protected set; } = false;
        public float FireCooldown { get; protected set; } = 30f;
        public float FireCooldownTimer { get; protected set; } = 0f;
        public StateBinding boostCooldownTimerBinding = new StateBinding("FireCooldownTimer");
        public IVehicleHUD VehicleHUD;

        protected BitmapFont TargetFont;

        protected VehicleBase(float xval, float yval) : base(xval, yval)
        {
            this.layer = Layer.Blocks;
            this.isStatic = true;
            this.dontCrush = true;
            this.alpha = 0.99f;
            this.VehicleHUD = new BasicVehicleHUD(this);

            this.TargetFont = new BitmapFont("smallFont", 4, 3);
        }

        protected VehicleBase() : this(0, 0)
        {
        }

        public Vec2 WorldCockpitPosition // TODO: Take rotation into account when calculating position
        {
            get
            {
                float xDir = this.offDir;
                float yDir = 1; // this._directionAngle > 180f ? 1 : -1;
                Vec2 directionalMult = new Vec2(xDir, yDir);
                return this.position + this.CockpitPosition * directionalMult;
            }
        }

        public Vec2 DirectionVector
        {
            get
            {
                float rads = Maths.DegToRad(this.DirectionAngle);
                return new Vec2(Maths.FastCos(rads), Maths.FastSin(rads));
            }
        }

        public virtual void SetPilot(Duck d)
        {
            // Do not allow to override the current pilot, first it has to be set to null 
            if (d != null && this.Pilot != null)
                return;

            if (d == null) { this.Pilot = d; return; }; // Stop recursivity by doing this

            // Remove pilot from previous vehicle
            // Because every peer will call this, must check vehicle pilot is equal to d. this avoid unmounting other ducks
            // Only allow to take ownership over other vehicle if unmounted from previous one
            VehicleBase prevVehicle = Quahicle.Core.GetCurrentVehicle();
            if (prevVehicle != null && prevVehicle.Pilot.Equals(d) && !prevVehicle.Mounted)
            {
                prevVehicle.SetPilot(null);
                this.Pilot = d;
                d.Fondle(this);
            }
            else if (prevVehicle == null)
            {
                this.Pilot = d;
                d.Fondle(this);
            }


        }

        public virtual void Mount()
        {
            if (this.Pilot == null || this.Pilot.dead)
                return;

            this.Mounted = true;

            this.Pilot.moveLock = true;
            this.Pilot.CancelFlapping();
            this.OnMount();
        }

        public virtual void OnMount()
        {

        }

        public virtual void UnMount()
        {
            this.Mounted = false;

            if (this.Pilot == null || this.Pilot.dead)
                return;

            this.Pilot.moveLock = false;
            this.Pilot.vSpeed += -1f;

            this.OnUnMount();
        }

        public virtual void OnUnMount()
        {
            this.Accelerating = false;
        }

        public Duck GetCandidateDuck()
        {
            return Level.CheckCircle<Duck>(this.position, 15f);
        }

        public override void Update()
        {
            base.Update();

            // Search for pilot if none
            if (this.Pilot == null)
            {
                Duck candidate = this.GetCandidateDuck();
                if (candidate != null && candidate.inputProfile.Down("QUACK"))
                    this.SetPilot(candidate);
            }

            UpdatePilot();
            UpdateInput();

            // Flip graphic to match direction and prevent rendering upside down
            if (this.DirectionAngle > 90f && this.DirectionAngle < 270f)
                this.graphic.flipV = true;
            else
                this.graphic.flipV = false;

            // Update current vehicle's angle with direction angle
            this.angleDegrees = this.DirectionAngle;

            // Handle fire cooldown timer
            if (this.FireCooldownTimer > 0f)
                this.FireCooldownTimer -= 0.03f;

            if (this.FireCooldownTimer <= 0f) this.FireCooldownTimer = 0f;

        }

        public virtual void UpdatePilot()
        {
            if (this.Pilot == null || !this.Pilot.isServerForObject)
                return;

            if (!this.Mounted)
                return;

            this.Pilot.position = this.WorldCockpitPosition;
        }

        public virtual bool CanMount()
        {
            if (this.Pilot == null || this.Mounted) return false;
            else return this.Pilot.Distance(this) < 15.0f;
        }


        public virtual void UpdateInput()
        {
            if (this.Pilot == null || !isServerForObject)
                return;

            InputProfile i = this.Pilot.inputProfile;
            if (i == null)
                return;

            if (i.Down("QUACK") && this.CanMount())
                this.Mount();

            if (!this.Mounted) return;

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
            if (!this.LockH && ((double)i.leftStick.x > (double)0.4f || i.Down("RIGHT")))
            {
                right = true;
                this.Pilot.offDir = (sbyte)1;
            }
            if (!this.LockH && ((double)i.leftStick.x < -(double)0.4f || i.Down("LEFT")))
            {
                left = true;
                this.Pilot.offDir = (sbyte)-1;
            }
            if (!this.LockV && ((double)i.leftStick.y > (double)0.4f || i.Down("UP")))
                up = true;
            if (!this.LockV && ((double)i.leftStick.y < -(double)0.4f || i.Down("DOWN")))
                down = true;

            if (up)
                this.DirectionAngle = 270f;

            if (down)
                this.DirectionAngle = 90f;

            if (right)
                this.DirectionAngle = 0f;

            if (left)
                this.DirectionAngle = 180f;

            if (right && down)
                this.DirectionAngle = 45f;

            if (right && up)
                this.DirectionAngle = 315f;

            if (left && down)
                this.DirectionAngle = 135f;

            if (left && up)
                this.DirectionAngle = 225f;


            // Accelerate if any direction key is pressed
            if (right || left || up || down)
            {
                this.Accelerating = true;
                this.Accelerate();
            }
            else
            {
                this.Accelerating = false;
                if (this.Mounted || !this.KeepDirectionUnMounted) this.DirectionAngle = this.Pilot.offDir > 0 ? 0f : 180f;
            }

        }

        public virtual void Accelerate()
        {
            Vec2 speedAdd = this.DirectionVector * Speed * AccelerationMul;
            this.hSpeed += speedAdd.x;
            this.vSpeed += speedAdd.y;
        }

        public virtual void Jump()
        {
            if (this.grounded)
            {
                this.vSpeed -= this.JumpPower;
            }
        }

        public virtual void Fire()
        {
            if (this.FireCooldownTimer <= 0f)
            {
                this.FireCooldownTimer = this.FireCooldown;
                this.OnFire();
            }
        }

        public virtual void OnFire()
        {

        }

        public override void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
        {
            if (this.Mounted && with == this.Pilot) return;

            base.Impact(with, from, solidImpact);
        }

        public override void UpdatePhysics()
        {
            base.UpdatePhysics();
            this.hSpeed = MathHelper.Clamp(this.hSpeed, -this.MaxHSpeed, this.MaxHSpeed);

            // TODO: Search for an alternative to deal with jumpPower. If jumpPower exceeds maxVSpeed then the extra power is rendered useless.
            // by adding the jumpPower to the maxVSpeed this extra power is taken into account but can result in unexpected behaviour.
            this.vSpeed = MathHelper.Clamp(this.vSpeed, -(this.JumpPower + this.MaxVSpeed), this.MaxVSpeed);
        }

        public override void Draw()
        {
            base.Draw();
            Graphics.DrawCircle(this.position, 15f, Color.White);
            if (!isLocal)
                return;
            // Draw hint to mount the vehicle if unmounted and near it
            Duck d = this.Pilot == null ? this.GetCandidateDuck() : this.Pilot; // If no pilot, then default to closest duck
            VehicleBase curVehicle = Quahicle.Core.GetVehicleOf(d);
            bool mountedInOtherVehicle = false;

            if(curVehicle != null && curVehicle.Mounted) mountedInOtherVehicle = true; 

            if (!mountedInOtherVehicle && d != null && (this.Pilot == null || this.CanMount()))
                this.TargetFont.Draw("@QUACK@", this.position - new Vec2(6f, this.height), Color.White, input: d.inputProfile);
        }
    }
}