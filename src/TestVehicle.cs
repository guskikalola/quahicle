namespace DuckGame.Quahicle
{
    [EditorGroup("Quahicle|Vehicles")]
    public class TestVehicle : VehicleBase
    {
        private SpriteMap _sprite;
        public TestVehicle()
        {
            this._vehicleName = "Test Vehicle";

            this._sprite = new SpriteMap("pistol", 18, 10);
            this.graphic = _sprite;
            this.center = new Vec2(10f, 3f);
            this.collisionOffset = new Vec2(-8f, -3f);
            this.collisionSize = new Vec2(16f, 9f);
            this.lockV = true;

            this._jumpPower = 20f;
        }
    }
}