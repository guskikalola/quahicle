namespace DuckGame.Quahicle
{
    [EditorGroup("Quahicle|Vehicles")]
    public class TestVehicle : VehicleBase
    {
        private SpriteMap _sprite;
        public TestVehicle()
        {
            this._vehicleName = "Test Vehicle";
            this._sprite = new SpriteMap(GetPath("sprites/ducktor"), 82, 46, false);
            this.graphic = (Sprite)this._sprite;
            this.graphic.scale = new Vec2(0.5f, 0.5f);
            this.graphic.center = new Vec2(this.graphic.width / 2, this.graphic.height / 2);
            this.center = new Vec2(41f * this.graphic.scale.x, 26f * this.graphic.scale.y);
            this.collisionOffset = new Vec2(-41f * this.graphic.scale.x, -26f * this.graphic.scale.y);
            this.collisionSize = new Vec2(82f * this.graphic.scale.x, 46f * this.graphic.scale.y);
            this.lockV = false;

            this._cockpitPosition = new Vec2(20f * this.graphic.scale.x, 0f * this.graphic.scale.y);

            this._jumpPower = 10f;
        }
    }
}