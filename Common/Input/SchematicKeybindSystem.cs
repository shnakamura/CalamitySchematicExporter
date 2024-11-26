using Microsoft.Xna.Framework.Input;

namespace CalamitySchematicExporter.Common.Input;

[Autoload(Side = ModSide.Client)]
public sealed class SchematicKeybindSystem : ModSystem
{
    public static ModKeybind CaptureKeybind { get; private set; }

    public override void Load()
    {
        base.Load();

        CaptureKeybind = KeybindLoader.RegisterKeybind(Mod, nameof(CaptureKeybind), Keys.P);
    }

    public override void Unload()
    {
        base.Unload();

        CaptureKeybind = null;
    }
}