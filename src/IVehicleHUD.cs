namespace DuckGame.Quahicle
{
    public interface IVehicleHUD
    {
        void DrawPilotStatus();
        void DrawControls();
        void DrawVehicleStatus();
        void SetScale(float scale);
    }
}