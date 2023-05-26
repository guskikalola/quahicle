using System.Collections.Generic;

namespace DuckGame.Quahicle
{
    [EditorGroup("Quahicle|Vehicles")]
    public class BathVehicle : VehicleBase
    {
        public BathVehicle() : base()
        {
            SpriteMap _sprite;
            this.VehicleName = "Turbo Bathtub";
            _sprite = new SpriteMap(GetPath("sprites/bath"), 64, 64);
            _sprite.AddAnimation("default", 0.3f, true, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11);
            _sprite.SetAnimation("default");
            this.graphic = _sprite;
            this.graphic.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            this.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            this.collisionOffset = new Vec2(-30f, -30f);
            this.collisionSize = new Vec2(62f, 62f);

            this.CockpitPosition = new Vec2(-8f, 0f);

            this.LockV = true;

            this.JumpPower = 7f;
            this.MaxHSpeed = 10f;
            this.MaxVSpeed = 10f;

            this.MountingDistance = 20f;

            this.VehicleHUD = new BasicVehicleHUD(this);

        }

        private void DuckGoYEET(Duck duck)
        {
            if (duck == null)
                return;

            Vec2 vec = Maths.AngleToVec(Maths.DegToRad(-this.DirectionAngle));
            if (duck.ragdoll == null)
            {
                IPlatform p = Level.CheckRay<IPlatform>(this.position, this.position + new Vec2((this.DirectionVector.x >= 0.5 ? 1 : -1) * 50f, 0));
                if (p != null) duck.Kill(new DTImpale(this));
                else
                {
                    duck.ApplyForce(vec * 100f);
                    duck.GoRagdoll();
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if(!isServerForObject) return;
            foreach (Ragdoll r in Level.CheckCircleAll<Ragdoll>(this.position, 45f))
            {
                if (!r._duck.dead) r.Unragdoll();
            }

            if (this.Accelerating && this.grounded)
            {
                Vec2 pos = this.position + new Vec2(0, (this.height / 2));
                Spark s = Spark.New(pos.x, pos.y, this.DirectionVector, 0.001f);
                Level.Add(s);
            }
        }

        public override void Impact(MaterialThing with, ImpactedFrom from, bool solidImpact)
        {
            if (with == this.Pilot) return;

            Duck d = with as Duck;
            if (d != null && this.Mounted)
            {
                this.DuckGoYEET(d);
            }

            base.Impact(with, from, solidImpact);
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}