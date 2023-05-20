using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace DuckGame.Quahicle
{

    public class BasicVehicleHUD : IVehicleHUD
    {
        private BitmapFont _font;
        private VehicleBase _vehicle;
        protected Dictionary<string, bool> Controls;
        protected float HUDScale = 1f;
        public BasicVehicleHUD(VehicleBase vehicle)
        {
            this._font = new BitmapFont("biosFont", 8);
            this._font.scale = new Vec2(1f * this.HUDScale, 1f * this.HUDScale);
            this._vehicle = vehicle;
            this.Controls = new Dictionary<string, bool>();

            this.Controls.Add("@RIGHT@ Move right", !this._vehicle.LockH);
            this.Controls.Add("@LEFT@ Move left", !this._vehicle.LockH);
            this.Controls.Add("@UP@ Move up", !this._vehicle.LockV);
            this.Controls.Add("@DOWN@ Move down", !this._vehicle.LockV);
            this.Controls.Add("@RAGDOLL@ Unmount", true);
            this.Controls.Add("@SHOOT@ Fire", this._vehicle.FireCooldown > 0f);
        }

        public void DrawControls()
        {
            if (!this._vehicle.Mounted) return;

            // Graphics.DrawRect(new Vec2(5f, MonoMain.screenHeight - 10),new Vec2(_font.GetWidth("@RIGHT@ Move right") + 15f, MonoMain.screenHeight - 140), Color.Gray);
            float offsetY = 30 * this.HUDScale;
            float incrementOffset = 20 * this.HUDScale;
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
            // Vehicle health

            // Vehicle fire cooldown
            float cd = this._vehicle.FireCooldownTimer;
            float cdMax = this._vehicle.FireCooldown;
            if (cdMax == 0) cdMax = 1;
            float perc = cd / cdMax;
            if (cd > 0f)
            {
                float centreX = MonoMain.screenWidth / 2;
                float centreY = MonoMain.screenHeight / 2;

                string cooldownTxt = "Recharging...";
                float txtWidth = _font.GetWidth(cooldownTxt);
                _font.Draw(cooldownTxt, new Vec2(centreX, centreY) + new Vec2((-txtWidth/2)  * this.HUDScale,-15f  * this.HUDScale), Color.White, input: Quahicle.Core.GetCurrentDuck().inputProfile);

                Vec2 offset1 = new Vec2(-txtWidth  * this.HUDScale, 5f  * this.HUDScale);
                Vec2 offset2 = new Vec2(txtWidth  * this.HUDScale, -5f  * this.HUDScale);
                // Cooldown bar outline 
                Graphics.DrawRect(new Vec2(centreX, centreY) + offset1, new Vec2(centreX, centreY) + offset2, Color.White, filled: false);
                // Cooldown bar filling ( goes from 100% width to 0% width of the bar )
                Graphics.DrawRect(new Vec2(centreX, centreY) + offset1, new Vec2(centreX + offset1.x, centreY) + offset2 * new Vec2(2*perc, 1), Color.White, filled: true);
            }

        }

        public void SetScale(float scale)
        {
            this.HUDScale = scale;
        }
    }
}