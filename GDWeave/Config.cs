using System.Text.Json.Serialization;

namespace GDWeave;

public class Config {
    [JsonInclude] public bool ControllerSupport;
    [JsonInclude] public bool ControllerVibration = true;
    [JsonInclude] public double ControllerVibrationStrength = 1.0;

    [JsonInclude] public bool MenuTweaks = true;
    [JsonInclude] public bool SortInventory;
}
