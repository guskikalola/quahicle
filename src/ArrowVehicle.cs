namespace DuckGame.Quahicle
{
    [EditorGroup("Quahicle|Vehicles")]
    public class ArrowVehicle : VehicleBase
    {
        private SpriteMap _sprite;
        private float _boostDuration = 5f;
        private float _boostTimer = 0f;
        public StateBinding boostTimerBinding = new StateBinding("_boostTimer");

        private readonly float baseSpeed = 2f;
        private readonly float boostSpeed = 6f;
        public ArrowVehicle() : base()
        {
            this.VehicleName = "Arrow Vehicle";
            this._sprite = new SpriteMap(GetPath("sprites/arrow"), 32, 8, false);
            this.graphic = (Sprite)this._sprite;
            this.graphic.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            this.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            this.collisionOffset = new Vec2(-16f, -3f);
            this.collisionSize = new Vec2(32f, 8f);

            this.CockpitPosition = new Vec2(0, -17f);

            this.JumpPower = 0f;
            this.MaxHSpeed = this.baseSpeed;
            this.MaxVSpeed = this.baseSpeed;

            this.gravMultiplier = 0f;

            this.FireCooldown = 10f;

            this.FireCooldownTimer = this.FireCooldown;

            this.VehicleHUD = new BasicVehicleHUD(this);
        }

        public override void Update()
        {
            if (!this.Accelerating || !this.Mounted)
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

            if (this._boostTimer <= 0f)
            {
                this._boostTimer = 0f;
                this.MaxHSpeed = this.baseSpeed;
                this.MaxVSpeed = this.baseSpeed;
            }
            else
            {
                this.MaxHSpeed = this.boostSpeed;
                this.MaxVSpeed = this.boostSpeed;
            }


            base.Update();

            if (this.Pilot != null && this._boostTimer <= 0f && this.IsInsideBlock())
                this.Pilot.Kill(new DTCrush(this));
        }

        public override void OnFire()
        {
            this._boostTimer = this._boostDuration;
        }

        private bool IsInsideBlock()
        {
            IPlatform p1 = Level.CheckPoint<IPlatform>(this.Pilot.position);
            IPlatform p2 = Level.CheckPoint<IPlatform>(this.position);
            return p1 != null && p2 != null;
        }

        public override void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
        {
            if (with == this.Pilot) return;

            if (this._boostTimer > 0f)
            {
                if (with is IPlatform) return;

                Duck d = with as Duck;
                if (d != null)
                    d.Kill(new DTImpale(this));
            }

            base.Impact(with, from, solidImpact);
        }
    }
}