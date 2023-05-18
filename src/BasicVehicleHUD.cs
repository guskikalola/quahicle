using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DuckGame.Quahicle
{

    public class BasicVehicleHUD : IVehicleHUD
    {
        private BitmapFont _font;
        private VehicleBase _vehicle;
        protected Dictionary<string, bool> Controls;
        public BasicVehicleHUD(VehicleBase vehicle)
        {
            this._font = new BitmapFont("biosFont", 8);
            this._font.scale = new Vec2(1f, 1f);
            this._vehicle = vehicle;
            this.Controls = new Dictionary<string, bool>();

            this.Controls.Add("@RIGHT@ Move right", !this._vehicle.LockH);
            this.Controls.Add("@LEFT@ Move left", !this._vehicle.LockH);
            this.Controls.Add("@UP@ Move up", !this._vehicle.LockV);
            this.Controls.Add("@DOWN@ Move down", !this._vehicle.LockV);
            this.Controls.Add("@RAGDOLL@ Unmount", true);
            this.Controls.Add("@SHOOT@ Fire", true);
        }

        public void DrawControls()
        {
            if (!this._vehicle.Mounted) return;

            // Graphics.DrawRect(new Vec2(5f, MonoMain.screenHeight - 10),new Vec2(_font.GetWidth("@RIGHT@ Move right") + 15f, MonoMain.screenHeight - 140), Color.Gray);
            int offsetY = 30;
            int incrementOffset = 20;
            foreach (string txt in this.Controls.Keys)
            {
                bool shouldDraw = this.Controls[txt];
                if (shouldDraw)
                {
                    _font.Draw(txt, new Vec2(10f, MonoMain.screenHeight - offsetY), Color.White, input: Quahicle.Core.GetCurrentDuck().inputProfile);
                    offsetY += incrementOffset;
                }
            }
        }

        public void DrawPilotStatus()
        {
        }

        public void DrawVehicleStatus()
        {
            if (!this._vehicle.Mounted) return;

        }
    }
}