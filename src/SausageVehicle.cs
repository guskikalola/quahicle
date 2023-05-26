using System.Collections.Generic;

namespace DuckGame.Quahicle
{
    [EditorGroup("Quahicle|Vehicles")]
    public class SausageVehicle : VehicleBase
    {
        private SpriteMap _spriteVehicle;
        private SpriteMap _spritePilot;
        private float _power;
        private AmmoType _ammoType;

        public SausageVehicle() : base()
        {
            this.VehicleName = "Sausage";
            _spriteVehicle = new SpriteMap(GetPath("sprites/sausage"), 64, 64);
            _spritePilot = new SpriteMap(GetPath("sprites/sausage_pilot"), 64, 64);

            this.graphic = _spriteVehicle;
            _spriteVehicle.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            _spritePilot.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);

            this.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            this.collisionOffset = new Vec2(-30f, -30f);
            this.collisionSize = new Vec2(62f, 62f);

            this.CockpitPosition = new Vec2(-8f, 0f);
            this.HidePilot = true;

            this.LockV = true;

            this.JumpPower = 7f;
            this.MaxHSpeed = 10f;
            this.MaxVSpeed = 10f;

            this.MountingDistance = 20f;

            this.FireCooldown = 15f;
            this.FireCooldownTimer = this.FireCooldown;

            this._power = 120f;
            this._ammoType = (AmmoType)new ATShrapnel();

            this.VehicleHUD = new BasicVehicleHUD(this);
        }

        public override void UpdateVehicleGraphic()
        {
        }

        public void FixRotation()
        {
            if (this.angleDegrees == 0) return; // Already in the correct rotation
            this.angleDegrees += 5 * this.Pilot.offDir;
        }

        public override void OnFire()
        {
            if (this.isServerForObject)
            {
                Bullet b = new Bullet(this.x, this.y, this._ammoType);
                ATMissile.DestroyRadius(this.position, this._power, b);
                SFX.Play("explode");
                Graphics.FlashScreen();
                RumbleManager.AddRumbleEvent(b.position, new RumbleEvent(RumbleIntensity.Heavy , RumbleDuration.Short, RumbleFalloff.Medium, RumbleType.Gameplay));
            }
        }

        public override void Update()
        {
            base.Update();

            if(!this.Mounted) this.angleDegrees = 0f;

            if (this.Pilot == null)
                return;

            if (System.Math.Abs(this.angleDegrees) >= 360) this.angleDegrees = 0;

            if (!this.grounded || !this.Accelerating)
            {
                this.FixRotation();
            }
            else
            {
                if (this.Pilot.offDir > 0)
                    this.angleDegrees += 5;
                else
                    this.angleDegrees -= 5;
            }

            this._spritePilot.angleDegrees = this.angleDegrees;
            this._spritePilot.color = this.Pilot.persona.colorUsable;
            _spritePilot.position = this.position;
        }

        public override void Draw()
        {
            base.Draw();
            if (this.Mounted) _spritePilot.Draw();
        }
    }
}