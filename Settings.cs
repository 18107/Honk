using UnityModManagerNet;

namespace Honk
{
    public class Settings : UnityModManager.ModSettings, IDrawable
    {
        [Draw("Horn mode", DrawType.ToggleGroup, Tooltip = "When activating the horn, also activate: Nothing, Locomotives connected by MU, Locomotives in this train, All locomotives on the map")]
        public Option hornControlType = Option.MU;

        public enum Option
        {
            Single,
            MU,
            Train,
            All
        }

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }

        public void OnChange()
        {
            Honk.ResetAll();
        }
    }
}
