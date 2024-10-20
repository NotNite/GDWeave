using System.Text.Json.Serialization;

namespace WebfishingPlus;

public class Config {
    [JsonInclude] public bool ControllerSupport;
    [JsonInclude] public bool ControllerVibration = true;
    [JsonInclude] public double ControllerVibrationStrength = 1.0;

    [JsonInclude] public bool MenuTweaks = true;
    [JsonInclude] public bool SortInventory;
    [JsonInclude] public bool FixHotbar = true;
}
