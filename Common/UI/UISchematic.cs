using CalamitySchematicExporter.Utilities;
using Microsoft.Xna.Framework.Input;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.GameInput;
using Terraria.Localization;
using Terraria.UI;

namespace CalamitySchematicExporter.Common.UI;

/// <summary>
/// 
/// </summary>
public sealed class UISchematic : UIState
{
    /// <summary>
    ///     The width of the selection outline, in pixels.
    /// </summary>
    public const int OUTLINE_WIDTH = 2;

    /// <summary>
    /// 
    /// </summary>
    public static readonly Asset<Texture2D> PinTexture = ModContent.Request<Texture2D>($"{nameof(CalamitySchematicExporter)}/Assets/Textures/Pin");
    
    /// <summary>
    /// 
    /// </summary>
    public UIImage TopLeftPin { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public UIImage TopRightPin { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public UIImage BottomLeftPin { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public UIImage BottomRightPin { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsSelectingArea { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public bool HasSelectedArea { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsResizingTopLeft { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsResizingTopRight { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsResizingBottomLeft { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsResizingBottomRight { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsResizingAny => IsResizingTopLeft || IsResizingTopRight || IsResizingBottomRight || IsResizingBottomLeft;

    /// <summary>
    /// 
    /// </summary>
    public Vector2 Start;

    /// <summary>
    /// 
    /// </summary>
    public Vector2 End;

    private Player Player => Main.CurrentPlayer;

    public override void OnActivate()
    {
        base.OnActivate();
        
        Clear();
    }

    public override void OnDeactivate()
    {
        base.OnDeactivate();
        
        Clear();
    }

    public override void Recalculate()
    {
        base.Recalculate();
        
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
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        UpdateSelection();
        UpdateSize();
        
        Recalculate();
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);

        if (!HasSelectedArea || !IsSelectingArea)
        {
            return;
        }

        var area = GetDimensions().ToRectangle();

        // Top border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, 0, Main.screenWidth, area.Top), Color.Black * 0.75f);

        // Bottom border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, area.Bottom, Main.screenWidth, (int)MathF.Abs(Main.screenHeight - area.Bottom)), Color.Black * 0.75f);

        // Left border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(0, area.Top, area.Left, area.Height), Color.Black * 0.75f);

        // Right border.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.Right, area.Top, (int)MathF.Abs(Main.screenWidth - area.Left), area.Height), Color.Black * 0.75f);

        // Top outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.X, area.Y - OUTLINE_WIDTH, area.Width, OUTLINE_WIDTH), Color.White);

        // Bottom outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.X, area.Bottom, area.Width, OUTLINE_WIDTH), Color.White);

        // Left outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.X - OUTLINE_WIDTH, area.Y, OUTLINE_WIDTH, area.Height), Color.White);

        // Right outline.
        spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(area.Right, area.Y, OUTLINE_WIDTH, area.Height), Color.White);
    }

    private void Clear()
    {
        Start = Vector2.Zero;
        End = Vector2.Zero;

        IsSelectingArea = false;
        HasSelectedArea = false;
        
        IsResizingTopLeft = false;
        IsResizingTopRight = false;
        IsResizingBottomLeft = false;
        IsResizingBottomRight = false;
    }

    private void UpdateSelection()
    {
        if (HasSelectedArea)
        {
            return;
        }

        // We have to check for manual player inputs to ensure they dont conflict with other user interfaces or in-game mechanics that may manipulate it.
        var justLeftClicked = !Player.mouseInterface && PlayerInput.MouseInfo.LeftButton == ButtonState.Pressed && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Released;
        var justLeftReleased = !Player.mouseInterface && PlayerInput.MouseInfo.LeftButton == ButtonState.Released && PlayerInput.MouseInfoOld.LeftButton == ButtonState.Pressed;
    }
    
    private void UpdateSize() 
    {
        if (!IsResizingAny) 
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
}