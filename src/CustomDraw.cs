using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using DuckGame;

namespace DuckGame.Quahicle
{
    public class CustomDraw : IDrawable
    {
        public static CustomDraw instance = null;
        public CustomDraw()
        {
            instance = this;
        }

        public bool Visible
        {
            get
            {
                return true;
            }
        }

        public int DrawOrder
        {
            get
            {
                return 1;
            }
        }

        public event EventHandler<EventArgs> VisibleChanged;
        public event EventHandler<EventArgs> DrawOrderChanged;

        protected virtual void OnVisibleChanged(EventArgs e)
        {
            // Just to shut the warning
            VisibleChanged.ToString();
        }

        protected virtual void OnDrawOrderChanged(EventArgs e)
        {
            // Just to shut the warning
            DrawOrderChanged.ToString();
        }

        public static float CalculateScreenXCenter(string text)
        {
            float tWidth = Graphics.GetStringWidth(text);
            float xCenter = MonoMain.screenWidth / 2;
            return xCenter - tWidth / 2;
        }

        public void DrawHUD(VehicleBase vehicle)
        {
            IVehicleHUD HUD = vehicle.VehicleHUD;
            if (HUD == null) return;

            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Resolution.getTransformationMatrix());
            HUD.DrawPilotStatus();
            Graphics.screen.End();

            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Resolution.getTransformationMatrix());
            HUD.DrawControls();
            Graphics.screen.End();

            Graphics.screen.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Resolution.getTransformationMatrix());
            HUD.DrawVehicleStatus();
            Graphics.screen.End();
        }

        public void Draw(GameTime gameTime)
        {
            VehicleBase currentVehicle = Quahicle.Core.GetCurrentVehicle();
            if (currentVehicle == null)
                return;

            DrawHUD(currentVehicle);

        }

    }
}