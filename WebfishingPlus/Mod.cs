using System.Reflection;
using GDWeave;
using WebfishingPlus.Mods;

namespace WebfishingPlus;

public class Mod : IMod {
    public static readonly string Version = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
    public static Config Config = null!;

    public Mod(IModInterface modInterface) {
        Config = modInterface.ReadConfig<Config>();

        if (Config.ControllerSupport) {
            modInterface.RegisterScriptMod(new ControllerInput.InputRegister());
            modInterface.RegisterScriptMod(new ControllerInput.PlayerModifier());
            modInterface.RegisterScriptMod(new ControllerInput.Fishing3Modifier());
            modInterface.RegisterScriptMod(new ControllerInput.InputRemapButtonModifier());
        }

        if (Config.MenuTweaks) {
            modInterface.RegisterScriptMod(new MenuTweaks.MainMenuModifier());
            modInterface.RegisterScriptMod(new MenuTweaks.EscMenuModifier());
        }

        if (Config.FixHotbar) {
            modInterface.RegisterScriptMod(new FixHotbar());
        }

        if (Config.SortInventory) {
            modInterface.RegisterScriptMod(new InventorySorter());
        }
    }

    public void Dispose() { }
}
