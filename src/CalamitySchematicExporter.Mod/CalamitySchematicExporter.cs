using CalamityMod.Schematics;
using CalamitySchematicExporter.Content.Tiles;
using CalamitySchematicExporter.Content.Walls;

namespace CalamitySchematicExporter;

public sealed class CalamitySchematicExporter : Mod
{
    public override void Load()
    {
        CalamitySchematicIO.PreserveTileID = (ushort)ModContent.TileType<PreserverTile>();
        CalamitySchematicIO.PreserveWallID = (ushort)ModContent.WallType<PreserverWall>();
    }

    public override void Unload()
    {
        CalamitySchematicIO.PreserveTileID = 0;
        CalamitySchematicIO.PreserveWallID = 0;
    }
}