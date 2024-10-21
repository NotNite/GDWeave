using System.Text.Json;
using System.Text.Json.Serialization;

namespace GDWeave;

public enum PatternType {
    LoadByteCode,
    SetCode
}

internal class PatternOverride {
    public required Dictionary<PatternType, string[]> PatternOverrides { get; set; } = new();
}
