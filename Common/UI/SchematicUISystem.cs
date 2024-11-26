using System.Collections.Generic;
using Terraria.UI;

namespace CalamitySchematicExporter.Common.UI;

[Autoload(Side = ModSide.Client)]
public sealed class SchematicUISystem : ModSystem
{
    private static GameTime lastGameTime;

    public override void UpdateUI(GameTime gameTime)
    {
        base.UpdateUI(gameTime);

        lastGameTime = gameTime;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        base.ModifyInterfaceLayers(layers);
        
        var index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");

        if (index == -1) {
            return;
        }
        
        layers.Insert(index + 1, new LegacyGameInterfaceLayer($"{nameof(CalamitySchematicExporter)}:{nameof(UISchematic)}", DrawState));
    }
    
    private static bool DrawState() {
        return true;
    }
}