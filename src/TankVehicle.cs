using System.Collections.Generic;

namespace DuckGame.Quahicle
{
    [EditorGroup("Quahicle|Vehicles")]
    public class TankVehicle : VehicleBase
    {
        private SpriteMap _spriteVehicle;
        private float _power;
        private AmmoType _ammoType;
        private Vec2 _gunPosition;

        public TankVehicle() : base()
        {
            this.VehicleName = "Tank";
            _spriteVehicle = new SpriteMap(GetPath("sprites/tank"), 64, 33);

            this.graphic = _spriteVehicle;
            _spriteVehicle.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);

            this.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            this.collisionOffset = new Vec2(-30f, -15f);
            this.collisionSize = new Vec2(62f, 32f);

            this.CockpitPosition = new Vec2(-8f, 0f);
            this.HidePilot = true;

            this.LockV = true;

            this.JumpPower = 7f;
            this.MaxHSpeed = 2f;
            this.MaxVSpeed = 10f;

            this.MountingDistance = 20f;

            this.FireCooldown = 4f;
            this.FireCooldownTimer = this.FireCooldown;

            this._ammoType = (AmmoType)new ATMissile();

            this.VehicleHUD = new BasicVehicleHUD(this);

            this._gunPosition = new Vec2(30f, 0);

        }

        // This returns the base position, without rotation
        private Vec2 GetWorldGunPosition()
        {
            return this.position + this._gunPosition;
        }

        private Vec2 WorldGunPositionRotated { get; set; }

        private void CalculateWorldGunPositionRotated()
        {
            float degs = Maths.Clamp(this.DirectionAngle, 180, 360);
            if (this.DirectionAngle == 0) degs = 0;

            this.WorldGunPositionRotated = this.GetWorldGunPosition().Rotate(Maths.DegToRad(degs), this.position);
        }

        private void SpawnParticles()
        {
            int num = 0;
            for (int i = 0; i < 5; ++i)
            {
                MusketSmoke musketSmoke =
                    new MusketSmoke(this.WorldGunPositionRotated.x - 16f + Rando.Float(32f) + this.offDir * 10f, this.WorldGunPositionRotated.y - 16f + Rando.Float(32f));
                musketSmoke.depth = (Depth)((float)(.9f + (float)i * (1f / 1000f)));
                if (num < 3)
                    musketSmoke.move.x -= (float)this.offDir * Rando.Float(0.1f);
                if (num > 3 && num < 5)
                    musketSmoke.fly.x += (float)this.offDir * (2f + Rando.Float(7.8f));
                Level.Add((Thing)musketSmoke);
                ++num;
            }
        }

        private void FireMissile()
        {
            Vec2 bulletPos = this.WorldGunPositionRotated;
            Bullet b = new Bullet(bulletPos.x, bulletPos.y, this._ammoType, -this.DirectionAngle);
            Level.Add(b);
        }

        public override void OnFire()
        {
            SFX.Play("explode");
            this.SpawnParticles();
            RumbleManager.AddRumbleEvent(this.position, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium, RumbleType.Gameplay));
            if (!(this.isServerForObject)) return;
            this.FireMissile();
        }

        public override void Update()
        {
            base.Update();
            this.CalculateWorldGunPositionRotated();
        }

        public override void Draw()
        {
            base.Draw();
            Graphics.DrawLine(this.position, this.WorldGunPositionRotated, Color.ForestGreen, 3f, -1f);
        }
    }
}