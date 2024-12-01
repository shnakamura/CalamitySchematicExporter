namespace CalamitySchematicExporter.Content.Projectiles;

public class SchematicReticle : ModProjectile
{
    private static Vector2 HeldItemOffset => new(22f, -25f);

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
    }

    public override void AI()
    {
        var player = Main.player[Projectile.owner];

        // If you're dumb enough to use this in multiplayer, nobody else run the code.
        if (Main.myPlayer != Projectile.owner)
        {
            return;
        }

        var csp = player.GetModPlayer<CalamitySchematicPlayer>();

        // If the player is no longer actively channeling the item (or gets cursed or CCed...), try to write a schematic of their selected area.
        if (!player.channel || player.noItems || player.CCed)
        {
            csp.AttemptExportSchematic();
            Projectile.Kill();
            return;
        }

        // Get the cursor's tile position and apply it to one or both corners.
        var cursorTilePos = Main.MouseWorld.ToTileCoordinates();

        // On frame 1, where the first corner won't exist yet, set that corner.
        if (!csp.CornerOne.HasValue)
        {
            csp.CornerOne = cursorTilePos;
        }

        // Set the second corner on every frame so you can drag it around.
        csp.CornerTwo = cursorTilePos;

        // Cool looking visuals that become stronger as you select more area
        var selection = csp.SchematicArea.GetValueOrDefault();
        var area = selection.Width * selection.Height;
        var sqrtArea = Math.Sqrt(area);
        var lightScale = Math.Min(0.3 + 0.0325 * sqrtArea, 1.6);
        Lighting.AddLight(Projectile.Center, 0f, 0.7f * (float)lightScale, (float)lightScale);
        var dustRadius = (float)Math.Min(5D * sqrtArea, 150D);
        SpawnEnergyVacuumDust(area, dustRadius);

        // Set the projectile's position to be (roughly) the center of where the player is holding up the item.
        var offset = HeldItemOffset;
        if (player.direction == -1)
        {
            offset.X = -offset.X - 7f;
        }

        Projectile.Center = player.Center + offset;

        // Keep the player channeling this item as a holdout projectile while it is functioning.
        player.itemTime = 2;
        player.itemAnimation = 2;
    }

    private void SpawnEnergyVacuumDust(double area, float spawnRadius)
    {
        var dustCount = (int)(0.5 * Math.Pow(area, 2D / 3D));
        var dustID = 56;
        var minScale = 0.4f;
        var maxScale = minScale + spawnRadius * 0.003f;
        var dustColor = new Color(0.85f, 1f, 0.85f);
        for (var i = 0; i < dustCount; ++i)
        {
            var posOffset = Main.rand.NextVector2Circular(spawnRadius, spawnRadius);
            var dustPos = Projectile.Center + posOffset;
            var dustVel = posOffset * -0.08f;
            var dustScale = Main.rand.NextFloat(minScale, maxScale);
            var d = Dust.NewDustDirect(dustPos, 0, 0, dustID, 0.08f, 0.08f, newColor: dustColor);
            d.velocity = dustVel;
            d.noGravity = true;
            d.scale = dustScale;
        }
    }

    // The Schematic Reticle cannot damage players or NPCs, or cut grass or break other tiles.
    public override bool? CanDamage()
    {
        return false;
    }

    // Destroy the owner's corner data when the projectile expires for any reason.
    public override void Kill(int timeLeft)
    {
        var player = Main.player[Projectile.owner];
        if (Main.myPlayer != Projectile.owner)
        {
            return;
        }

        var csp = player.GetModPlayer<CalamitySchematicPlayer>();
        csp.CornerOne = csp.CornerTwo = null;
    }
}