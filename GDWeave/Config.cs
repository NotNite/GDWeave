using System.Text.Json.Serialization;

namespace GDWeave;

public class Config {
    [JsonInclude] public bool ControllerSupport = false;
}
