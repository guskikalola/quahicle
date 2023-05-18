using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

// The title of your mod, as displayed in menus
[assembly: AssemblyTitle("Quahicle")]

// The author of the mod
[assembly: AssemblyCompany("guskikalola")]

// The description of the mod
[assembly: AssemblyDescription("Quack + Vehicle: Vehicles in Duckgame!")]

// The mod's version
[assembly: AssemblyVersion("1.0.0.0")]

namespace DuckGame.Quahicle
{
    public class Quahicle : Mod
    {
        // The mod's priority; this property controls the load order of the mod.
        public override Priority priority
        {
            get { return base.priority; }
        }

        // This function is run before all mods are finished loading.
        protected override void OnPreInitialize()
        {
            base.OnPreInitialize();
        }

        // This function is run after all mods are loaded.
        protected override void OnPostInitialize()
        {
            base.OnPostInitialize();
        }

        public static QuahicleCore Core { get => QuahicleCore.Instance; }

        protected override void OnStart()
        {
            base.OnStart();
			QuahicleCore core = QuahicleCore.Instance;
			if (core != null)
				DevConsole.Log("[Quahicle] QuahicleCore instance created!");
        }
    }
}
