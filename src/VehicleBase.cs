namespace DuckGame.Quahicle
{
    public abstract class VehicleBase : PhysicsObject
    {
        public float Health { get; protected set; }
        public string VehicleName { get; protected set; }
        public float Speed { get; protected set; }
        public Duck Pilot { get; protected set; }
        public StateBinding PilotBinding = new StateBinding("Pilot");
        public Vec2 CockpitPosition { get; protected set; }
        public bool Mounted { get; protected set; }
        public StateBinding MountedBinding = new StateBinding("Mounted");
        public bool Accelerating { get; protected set; }
        public StateBinding AcceleratingBinding = new StateBinding("Accelerating");
        public float DirectionAngle { get; protected set; }
        public StateBinding DirectionAngleBinding = new StateBinding("DirectionAngle", -1, false, false);
        public float JumpPower { get; protected set; }
        public bool LockH { get; protected set; }
        public bool LockV { get; protected set; }
        public float MaxHSpeed { get; protected set; }
        public float MaxVSpeed { get; protected set; }
        public float AccelerationMul { get; protected set; }
        public bool KeepDirectionUnMounted { get; protected set; }
        public float FireCooldown { get; protected set; }
        public float FireCooldownTimer { get; protected set; }
        public bool DeathControl { get; protected set; }
        public float MountingDistance { get; protected set; }
        public StateBinding boostCooldownTimerBinding = new StateBinding("FireCooldownTimer");
        public IVehicleHUD VehicleHUD;
        protected BitmapFont TargetFont;

        protected VehicleBase(float xval, float yval) : base(xval, yval)
        {

            this.Health = 1f;
            this.VehicleName = "Vehicle Base";
            this.Speed = 1f;
            this.Pilot = null;
            this.CockpitPosition = new Vec2(0, 0);
            this.Mounted = false;
            this.Accelerating = false;
            this.DirectionAngle = 0f;
            this.JumpPower = 0f;
            this.LockH = false;
            this.LockV = false;
            this.MaxHSpeed = 5f;
            this.MaxVSpeed = 5f;
            this.AccelerationMul = 0.5f;
            this.KeepDirectionUnMounted = false;
            this.FireCooldown = 0f;
            this.FireCooldownTimer = 0f;
            this.DeathControl = false;
            this.MountingDistance = 15.0f;

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

        public virtual void RemovePilot()
        {
            this.UnMount();
            this.Pilot = null;
        }

        public virtual void SetPilot(Duck d)
        {
            // Do not allow to override the current pilot, first it has to be removed
            if (d == null || this.Pilot != null)
                return;


            if (this.CanMount(d))
            {
                Quahicle.Core.RemoveFromEveryVehicle(d);
                this.Pilot = d;
                Thing.Fondle(this, d.connection);
            }

        }

        public virtual void Mount()
        {
            if (this.Pilot == null || this.Pilot.dead)
                return;


            Thing.Fondle(this, this.Pilot.connection);

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
            return Level.CheckCircle<Duck>(this.position, this.MountingDistance);
        }

        public override void Update()
        {
            base.Update();

            this.VehicleHUD.SetScale(1);

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
            if (this.Pilot == null || !this.isServerForObject)
                return;

            if (!this.Mounted)
                return;

            this.Pilot.vSpeed = 0f;
            this.Pilot.hSpeed = 0f;

            this.Pilot.position = this.WorldCockpitPosition;
        }

        public virtual bool CanMount(Duck d)
        {
            VehicleBase currVehicle = Quahicle.Core.GetVehicleOf(d);
            if (currVehicle != null && currVehicle.Mounted) return false;
            else if (this.Mounted) return false;
            else if (this.Pilot == null) return true;
            else if (this.Pilot.Equals(d)) return d.Distance(this) < this.MountingDistance;
            else return false;
        }

        public virtual void UpdateInput()
        {
            if (this.Pilot == null)
                return;

            if (this.Pilot.dead && !this.DeathControl)
                return;

            InputProfile i = this.Pilot.inputProfile;
            Duck target = Profiles.DefaultPlayer1.duck;
            if (DuckNetwork.localProfile != null)
                target = DuckNetwork.localProfile.duck;

            if (i == null || !this.Pilot.Equals(target))
                return;

            if (i.Down("QUACK") && this.CanMount(this.Pilot))
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

            // Draw hint to mount the vehicle if unmounted and near it
            Duck d = this.Pilot == null ? this.GetCandidateDuck() : this.Pilot; // If no pilot, then default to closest duck
            if (d == null || !d.isLocal) return;

            if (this.CanMount(d))
                this.TargetFont.Draw("@QUACK@", this.position - new Vec2(6f, this.height / 2), Color.White, input: d.inputProfile);
        }
    }
}