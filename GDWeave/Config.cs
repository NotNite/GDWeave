using System.Text.Json.Serialization;

namespace GDWeave;

public class Config {
    [JsonInclude] public bool ControllerSupport = false;
    [JsonInclude] public bool ControllerVibration = true;
    [JsonInclude] public double ControllerVibrationStrength = 1.0;
}
