using System.Collections.Generic;
using System.Linq;
using CalamityMod.Schematics;
using CalamitySchematicExporter.Content.Projectiles;
using Terraria.Audio;
using Terraria.DataStructures;

namespace CalamitySchematicExporter.Content.Items;

public class SchematicExporterItem : ModItem
{
    private const string BaseCompressionTooltip = "Right click to toggle compressed output files";
    private static string CompressionStatusString => CalamitySchematicIO.UseCompression ? "enabled" : "disabled";

    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 26;

        Item.useTime = Item.useAnimation = 40;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.channel = true;

        Item.shoot = ModContent.ProjectileType<SchematicReticle>();
        Item.shootSpeed = 0f;

        Item.rare = ItemRarityID.Red;
        Item.value = 0;

        Item.UseSound = SoundID.Item64;
    }

    public override bool AltFunctionUse(Player player)
    {
        return true;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var rightClick = player.altFunctionUse == ItemAlternativeFunctionID.ActivatedAndUsed;
        if (player.whoAmI == Main.myPlayer && rightClick)
        {
            CalamitySchematicIO.UseCompression = !CalamitySchematicIO.UseCompression;
            SoundEngine.PlaySound(SoundID.Item65);
            Main.NewText($"Schematic compression {CompressionStatusString}.");
        }

        return !rightClick;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        var compressionTooltip = tooltips.First(line => line.Name == "Tooltip2");
        compressionTooltip.Text = $"{BaseCompressionTooltip} (Compression currently {CompressionStatusString})";
    }
}