using CalamitySchematicExporter.Utilities;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.UI;

namespace CalamitySchematicExporter.Common.UI;

public sealed class UISchematic : UIState
{
    /// <summary>
    ///     The size of each outline, in pixels.
    /// </summary>
    public const int OUTLINE_SIZE = 2;

    /// <summary>
    ///     The size of each square in the tile grid, in pixels.
    /// </summary>
    public const int TILE_GRID_SQUARE_SIZE = 16;

    /// <summary>
    /// </summary>
    public static readonly Asset<Texture2D> PinTexture = ModContent.Request<Texture2D>($"{nameof(CalamitySchematicExporter)}/Assets/Textures/Pin");

    /// <summary>
    /// </summary>
    public static readonly Asset<Effect> InvertEffect = ModContent.Request<Effect>($"{nameof(CalamitySchematicExporter)}/Assets/Effects/Invert", AssetRequestMode.ImmediateLoad);

    /// <summary>
    /// </summary>
    public static readonly SoundStyle HoverSound = new($"{nameof(CalamitySchematicExporter)}/Assets/Sounds/InventoryTick")
    {
        PitchVariance = 0.25f,
        SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
    };

    /// <summary>
    /// </summary>
    public Vector2 End;

    private float opacity;

    /// <summary>
    /// </summary>
    public Vector2 Start;

    /// <summary>
    /// </summary>
    public float Opacity
    {
        get => opacity;
        set => opacity = MathHelper.Clamp(value, 0f, 1f);
    }

    /// <summary>
    ///     Whether this state is active or not.
    /// </summary>
    public bool Active { get; private set; }

    /// <summary>
    ///     Whether the player is selecting an area or not.
    /// </summary>
    public bool IsSelectingArea { get; private set; }

    /// <summary>
    ///     Whether the player has selected an area or not.
    /// </summary>
    public bool HasSelectedArea { get; private set; }

    /// <summary>
    /// </summary>
    public bool IsResizingTopLeft { get; private set; }

    /// <summary>
    /// </summary>
    public bool IsResizingTopRight { get; private set; }

    /// <summary>
    /// </summary>
    public bool IsResizingBottomLeft { get; private set; }

    /// <summary>
    /// </summary>
    public bool IsResizingBottomRight { get; private set; }

    /// <summary>
    /// </summary>
    public bool IsResizingAny => IsResizingTopLeft || IsResizingTopRight || IsResizingBottomRight || IsResizingBottomLeft;

    /// <summary>
    /// </summary>
    public UIImage[] Pins { get; } =
    [
        new(PinTexture)
        {
            Top = StyleDimension.FromPixelsAndPercent(-PinTexture.Height() / 2f, 0f),
            Left = StyleDimension.FromPixelsAndPercent(-PinTexture.Width() / 2f, 0f)
        },
        new(PinTexture)
        {
            Top = StyleDimension.FromPixelsAndPercent(-PinTexture.Height() / 2f, 0f),
            Left = StyleDimension.FromPixelsAndPercent(-PinTexture.Width() / 2f, 1f)
        },
        new(PinTexture)
        {
            Top = StyleDimension.FromPixelsAndPercent(-PinTexture.Height() / 2f, 1f),
            Left = StyleDimension.FromPixelsAndPercent(-PinTexture.Width() / 2f, 0f)
        },
        new(PinTexture)
        {
            Top = StyleDimension.FromPixelsAndPercent(-PinTexture.Height() / 2f, 1f),
            Left = StyleDimension.FromPixelsAndPercent(-PinTexture.Width() / 2f, 1f)
        }
    ];

    /// <summary>
    /// </summary>
    public ref UIImage TopLeftPin => ref Pins[0];

    /// <summary>
    /// </summary>
    public ref UIImage TopRightPin => ref Pins[1];

    /// <summary>
    /// </summary>
    public ref UIImage BottomLeftPin => ref Pins[2];

    /// <summary>
    /// </summary>
    public ref UIImage BottomRightPin => ref Pins[3];

    private Player Player => Main.LocalPlayer;

    public UISchematic()
    {

    }
    
    public override void OnActivate()
    {
        base.OnActivate();
        
        TopLeftPin.OnLeftMouseDown += StartResizing;
        TopLeftPin.OnLeftMouseUp += StopResizing;
        TopLeftPin.OnUpdate += UpdatePinEffects;
        
        TopRightPin.OnLeftMouseDown += StartResizing;
        TopRightPin.OnLeftMouseUp += StopResizing;
        TopRightPin.OnUpdate += UpdatePinEffects;
        
        BottomLeftPin.OnLeftMouseDown += StartResizing;
        BottomLeftPin.OnLeftMouseUp += StopResizing;
        BottomLeftPin.OnUpdate += UpdatePinEffects;
        
        BottomRightPin.OnLeftMouseDown += StartResizing;
        BottomRightPin.OnLeftMouseUp += StopResizing;
        BottomRightPin.OnUpdate += UpdatePinEffects;

        Clear();

        Active = true;
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        
        TopLeftPin.OnLeftMouseDown -= StartResizing;
        TopLeftPin.OnLeftMouseUp -= StopResizing;
        TopLeftPin.OnUpdate -= UpdatePinEffects;
        
        TopRightPin.OnLeftMouseDown -= StartResizing;
        TopRightPin.OnLeftMouseUp -= StopResizing;
        TopRightPin.OnUpdate -= UpdatePinEffects;
        
        BottomLeftPin.OnLeftMouseDown -= StartResizing;
        BottomLeftPin.OnLeftMouseUp -= StopResizing;
        BottomLeftPin.OnUpdate -= UpdatePinEffects;
        
        BottomRightPin.OnLeftMouseDown -= StartResizing;
        BottomRightPin.OnLeftMouseUp -= StopResizing;
        BottomRightPin.OnUpdate -= UpdatePinEffects;

        Clear();

        Active = false;
    }

    public override void Recalculate()
    {
        var screenStart = Start * 16f - Main.screenPosition;
        var screenEnd = End * 16f - Main.screenPosition;

        var newStart = (int)MathF.Min(screenStart.X, screenEnd.X);
        var newEnd = (int)MathF.Min(screenStart.Y, screenEnd.Y);

        Top.Set(newEnd, 0f);
        Left.Set(newStart, 0f);

        var newWidth = (int)MathF.Abs(screenStart.X - screenEnd.X);
        var newHeight = (int)MathF.Abs(screenStart.Y - screenEnd.Y);

        Width.Set(newWidth, 0f);
        Height.Set(newHeight, 0f);

        base.Recalculate();
    }

    public override void RecalculateChildren()
    {
        base.RecalculateChildren();

        Array.Sort(
            Pins,
            static (first, last) =>
            {
                var firstPosition = first.GetOuterDimensions().Position();
                var lastPosition = last.GetOuterDimensions().Position();

                return firstPosition.Y == lastPosition.Y ? firstPosition.X.CompareTo(lastPosition.X) : firstPosition.Y.CompareTo(lastPosition.Y);
            }
        );
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        Player.mouseInterface = false;

        if (Active)
        {
            UpdateResizing();
            UpdateText();
            UpdateEscaping();
            UpdateSelection();
        }

        Recalculate();

        Opacity = MathHelper.SmoothStep(Opacity, Active ? 1f : 0f, 0.2f);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        var gridColor = Color.LightGray * 0.5f * Opacity;

        var startX = (int)(Main.screenPosition.X / TILE_GRID_SQUARE_SIZE) * TILE_GRID_SQUARE_SIZE;
        var startY = (int)(Main.screenPosition.Y / TILE_GRID_SQUARE_SIZE) * TILE_GRID_SQUARE_SIZE;

        var endX = startX + Main.screenWidth + TILE_GRID_SQUARE_SIZE;
        var endY = startY + Main.screenHeight + TILE_GRID_SQUARE_SIZE;

        // Vertical grid lines.
        for (var x = startX; x <= endX; x += TILE_GRID_SQUARE_SIZE)
        {
            var verticalLineArea = new Rectangle((int)(x - Main.screenPosition.X), 0, OUTLINE_SIZE, Main.screenHeight);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, verticalLineArea, gridColor);
        }

        // Horizontal grid lines.
        for (var y = startY; y <= endY; y += TILE_GRID_SQUARE_SIZE)
        {
            var horizontalLineArea = new Rectangle(0, (int)(y - Main.screenPosition.Y), Main.screenWidth, OUTLINE_SIZE);

            spriteBatch.Draw(TextureAssets.MagicPixel.Value, horizontalLineArea, gridColor);
        }

        // Overlay.
        var overlayArea = new Rectangle(0, 0, Main.screenWidth, Main.screenHeight);

        spriteBatch.Draw(TextureAssets.MagicPixel.Value, overlayArea, Color.Black * (Active ? 1f - Opacity * Opacity * Opacity : 0f));

        var borderColor = Color.Black * 0.8f * Opacity;

        if (!HasSelectedArea && !IsSelectingArea)
        {
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, Main.screenHeight), borderColor);
            return;
        }

        var screenStart = Start * 16f - Main.screenPosition;
        var screenEnd = End * 16f - Main.screenPosition;

        var selectionWidth = (int)MathF.Abs(screenEnd.X - screenStart.X);
        var selectionHeight = (int)MathF.Abs(screenEnd.Y - screenStart.Y);

        var selectionX = (int)MathF.Min(screenStart.X, screenEnd.X);
        var selectionY = (int)MathF.Min(screenStart.Y, screenEnd.Y);

        var selectionColor = Color.White * (0.05f + 0.05f * MathF.Sin(Main.GameUpdateCount * 0.05f)) * Opacity;

        // Selection highlight.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(selectionX, selectionY, selectionWidth, selectionHeight), selectionColor);

        var area = GetDimensions().ToRectangle();

        // Top border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, area.Top), borderColor);

        // Bottom border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, area.Bottom, Main.screenWidth, (int)MathF.Abs(Main.screenHeight - area.Bottom)), borderColor);

        // Left border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, area.Top, area.Left, area.Height), borderColor);

        // Right border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.Right, area.Top, (int)MathF.Abs(Main.screenWidth - area.Left), area.Height), borderColor);

        var outlineColor = Color.White * Opacity;

        // Top outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.X, area.Y, area.Width, OUTLINE_SIZE), outlineColor);

        // Bottom outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.X, area.Bottom, area.Width + OUTLINE_SIZE, OUTLINE_SIZE), outlineColor);

        // Left outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.X, area.Y, OUTLINE_SIZE, area.Height), outlineColor);

        // Right outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.Right, area.Y, OUTLINE_SIZE, area.Height), outlineColor);
        
        base.Draw(spriteBatch);
    }

    private void Clear()
    {
        RemoveAllChildren();
        
        Start = Vector2.Zero;
        End = Vector2.Zero;

        IsSelectingArea = false;
        HasSelectedArea = false;

        IsResizingTopLeft = false;
        IsResizingTopRight = false;
        IsResizingBottomLeft = false;
        IsResizingBottomRight = false;
    }

    private void UpdateResizing()
    {
        if (!Active || !IsResizingAny)
        {
            return;
        }

        var position = Main.MouseWorld.SnapToTileCoordinates();

        if (IsResizingTopLeft)
        {
            Start = position;
        }
        else if (IsResizingTopRight)
        {
            Start.Y = position.Y;
            End.X = position.X;
        }
        else if (IsResizingBottomLeft)
        {
            Start.X = position.X;
            End.Y = position.Y;
        }
        else if (IsResizingBottomRight)
        {
            End = position;
        }
    }

    private void StartResizing(UIMouseEvent evt, UIElement element)
    {
        if (!Active)
        {
            return;
        }
        
        if (element == TopLeftPin)
        {
            IsResizingTopLeft = true;
            IsResizingTopRight = false;
            IsResizingBottomLeft = false;
            IsResizingBottomRight = false;
        }
        else if (element == TopRightPin)
        {
            IsResizingTopLeft = false;
            IsResizingTopRight = true;
            IsResizingBottomLeft = false;
            IsResizingBottomRight = false;
        }
        else if (element == BottomLeftPin)
        {
            IsResizingTopLeft = false;
            IsResizingTopRight = false;
            IsResizingBottomLeft = true;
            IsResizingBottomRight = false;
        }
        else if (element == BottomRightPin)
        {
            IsResizingTopLeft = false;
            IsResizingTopRight = false;
            IsResizingBottomLeft = false;
            IsResizingBottomRight = true;
        }

        SoundEngine.PlaySound(in HoverSound);
    }

    private void StopResizing(UIMouseEvent evt, UIElement element)
    {
        if (!Active)
        {
            return;
        }
        
        Start = (TopLeftPin.GetDimensions().Center() + Main.screenPosition).SnapToTileCoordinates();
        End = (BottomRightPin.GetDimensions().Center() + Main.screenPosition).SnapToTileCoordinates();

        IsResizingTopLeft = false;
        IsResizingTopRight = false;
        IsResizingBottomLeft = false;
        IsResizingBottomRight = false;
        
        SoundEngine.PlaySound(HoverSound with
        {
            Pitch = -0.25f
        });
    }

    private void AppendPins()
    {
        if (!Active)
        {
            return;
        }
        
        if (!HasChild(TopLeftPin))
        {
            Append(TopLeftPin);
        }

        if (!HasChild(TopRightPin))
        {
            Append(TopRightPin);
        }

        if (!HasChild(BottomLeftPin))
        {
            Append(BottomLeftPin);
        }

        if (!HasChild(BottomRightPin))
        {
            Append(BottomRightPin);
        }
    }

    private void UpdatePinEffects(UIElement element)
    {
        if (element is not UIImage image)
        {
            return;
        }

        image.NormalizedOrigin = new Vector2(0.5f);

        if (element == TopLeftPin)
        {
            var target = IsResizingTopLeft ? image.Rotation + MathHelper.ToRadians(10f) : 0f;
            
            image.Rotation = image.Rotation.AngleLerp(target, 0.2f);
        }
        else if (element == TopRightPin)
        {
            var target = IsResizingTopRight ? image.Rotation + MathHelper.ToRadians(10f) : 0f;
            
            image.Rotation = image.Rotation.AngleLerp(target, 0.2f);
        }
        else if (element == BottomLeftPin)
        {
            var target = IsResizingBottomLeft ? image.Rotation + MathHelper.ToRadians(10f) : 0f;
            
            image.Rotation = image.Rotation.AngleLerp(target, 0.2f);
        }
        else if (element == BottomRightPin)
        {
            var target = IsResizingBottomRight ? image.Rotation + MathHelper.ToRadians(10f) : 0f;
            
            image.Rotation = image.Rotation.AngleLerp(target, 0.2f);
        }

        image.ImageScale = MathHelper.SmoothStep(image.ImageScale, image.IsMouseHovering ? 1.2f : 1f, 0.5f);
    }

    private void UpdateText()
    {
        if (!Active || HasSelectedArea || IsSelectingArea)
        {
            return;
        }

        Main.instance.MouseText("Select Area");
    }

    private void UpdateEscaping()
    {
        if (!Active)
        {
            return;
        }

        var justRightClicked = !Player.mouseInterface && PlayerInput.MouseInfo.RightButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.RightButton == ButtonState.Released;
        var justEscaped = Main.keyState.IsKeyDown(Keys.Escape) && !Main.oldKeyState.IsKeyDown(Keys.Escape);

        if (!justRightClicked && !justEscaped)
        {
            return;
        }

        Player.releaseInventory = false;
        Player.mouseInterface = true;

        SchematicUISystem.Toggle();
    }

    private void UpdateSelection()
    {
        if (!Active || HasSelectedArea)
        {
            return;
        }

        var justLeftClicked = !Player.mouseInterface && PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released;
        var justLeftReleased = !Player.mouseInterface && PlayerInput.MouseInfo.LeftButton == ButtonState.Released && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Pressed;

        if (justLeftClicked)
        {
            Start = Main.MouseWorld.SnapToTileCoordinates();
            IsSelectingArea = true;
        }

        if (justLeftReleased)
        {
            IsSelectingArea = false;
            HasSelectedArea = true;
            
            AppendPins();
        }

        if (!IsSelectingArea)
        {
            return;
        }

        End = Main.MouseWorld.SnapToTileCoordinates();
    }
}