using HarmonyLib;
using System;
using System.Reflection;
using UnityModManagerNet;

namespace Honk
{
    public class Main
    {
        public static UnityModManager.ModEntry mod { get; private set; }

        internal static Settings settings { get; private set; }

        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            mod = modEntry;
            Harmony harmony = null;
            try
            {
                harmony = new Harmony(modEntry.Info.Id);
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                modEntry.Logger.LogException($"Failed to load {modEntry.Info.DisplayName}:", e);
                harmony?.UnpatchAll();
                return false;
            }

            settings = Settings.Load<Settings>(mod);
            modEntry.OnGUI += settings.Draw;
            modEntry.OnSaveGUI += settings.Save;
            modEntry.OnUpdate += Honk.OnUpdate;
            modEntry.OnToggle += (mod, enabled) => { if (!enabled) Honk.ResetAll(); return true; };

            return true; //Loaded successfully
        }
    }
}
