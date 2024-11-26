using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.UI;

namespace CalamitySchematicExporter;

public class CalamitySchematicUI : ModSystem
{
    private const float Epsilon = 5E-6f;
    private const float OutOfSelectionDimFactor = 0.06f;
    private static readonly Color BaseGridColor = new(0.24f, 0.8f, 0.9f, 0.5f);
    private static readonly Rectangle TexUpperHalfRect = new(0, 0, 18, 18);

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        layers.Insert(0, new LegacyGameInterfaceLayer("Schematic Selection Grid", RenderSchematicSelectionGrid));
    }

    private static bool RenderSchematicSelectionGrid()
    {
        var gridSquareTex = TextureAssets.Extra[68].Value;
        var rectNull = Main.LocalPlayer.GetModPlayer<CalamitySchematicPlayer>().SchematicArea;
        
        if (!rectNull.HasValue)
        {
            return true;
        }

        var selection = rectNull.Value;

        var topLeftScreenTile = (Main.screenPosition / 16f).Floor();
       
        for (var i = 0; i <= Main.screenWidth; i += 16)
        {
            for (var j = 0; j <= Main.screenHeight; j += 16)
            {
                var offset = new Vector2(i >> 4, j >> 4);
                var gridTilePos = topLeftScreenTile + offset;
                var gridTilePoint = new Point((int)(gridTilePos.X + Epsilon), (int)(gridTilePos.Y + Epsilon));
                var inSelection = selection.Contains(gridTilePoint);
                var gridColor = BaseGridColor * (inSelection ? 1f : OutOfSelectionDimFactor);
               
                Main.spriteBatch.Draw(gridSquareTex, gridTilePos * 16f - Main.screenPosition, TexUpperHalfRect, gridColor, 0f, Vector2.Zero, 1f, 0, 0f);
            }
        }

        return true;
    }
}