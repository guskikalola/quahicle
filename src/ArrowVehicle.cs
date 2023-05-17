namespace DuckGame.Quahicle
{
    [EditorGroup("Quahicle|Vehicles")]
    public class ArrowVehicle : VehicleBase
    {
        private SpriteMap _sprite;
        private float _boostCooldown = 30f;
        private float _boostDuration = 5f;
        private float _boostCooldownTimer = 0f;
        private float _boostTimer = 0f;
        public StateBinding boostCooldownTimerBinding = new StateBinding("_boostCooldownTimer");
        public StateBinding boostTimerBinding = new StateBinding("_boostTimer");

        private readonly float baseSpeed = 2f;
        private readonly float boostSpeed = 6f;
        public ArrowVehicle()
        {
            this._vehicleName = "Arrow Vehicle";
            this._sprite = new SpriteMap(GetPath("sprites/arrow"), 32, 8, false);
            this.graphic = (Sprite)this._sprite;
            this.graphic.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            this.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            this.collisionOffset = new Vec2(-16f, -3f);
            this.collisionSize = new Vec2(32f, 8f);

            this._cockpitPosition = new Vec2(0, -17f);

            this._jumpPower = 0f;
            this.maxHSpeed = this.baseSpeed;
            this.maxVSpeed = this.baseSpeed;

            this.gravMultiplier = 0f;
        }

        public override void Update()
        {
            if (!this._accelerating || !this._mounted)
            {
                this.hSpeed = 0f;
                this.vSpeed = 0f;
            }

            if (this._boostTimer > 0f)
            {
                this.graphic.color = Color.OrangeRed;
                this._boostTimer -= 0.03f;
            }
            else this.graphic.color = Color.White;

            if (this._boostCooldownTimer > 0f)
                this._boostCooldownTimer -= 0.03f;

            if (this._boostTimer <= 0f)
            {
                this._boostTimer = 0f;
                this.maxHSpeed = this.baseSpeed;
                this.maxVSpeed = this.baseSpeed;
            }
            else
            {
                this.maxHSpeed = this.boostSpeed;
                this.maxVSpeed = this.boostSpeed;
            }

            if (this._boostCooldownTimer <= 0f) this._boostCooldownTimer = 0f;

            base.Update();

            if (this._boostTimer <= 0f && this.IsInsideBlock())
                this._pilot.Kill(new DTCrush(this));


        }

        public override void Fire()
        {
            if (this._boostCooldownTimer == 0)
            {
                this._boostTimer = this._boostDuration;
                this._boostCooldownTimer = this._boostCooldown;
            }
        }

        private bool IsInsideBlock()
        {
            IPlatform p1 = Level.CheckPoint<IPlatform>(this._pilot.position);
            IPlatform p2 = Level.CheckPoint<IPlatform>(this.position);
            return p1 != null && p2 != null;
        }

        public override void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
        {
            if (with == this._pilot) return;

            if (this._boostTimer > 0f)
            {
                if (with is IPlatform) return;

                Duck d = with as Duck;
                if (d != null)
                {
                    d.Kill(new DTImpale(this));
                }
            }

            base.Impact(with, from, solidImpact);
        }
    }
}