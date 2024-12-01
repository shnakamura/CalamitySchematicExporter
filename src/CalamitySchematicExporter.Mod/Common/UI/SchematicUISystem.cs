using System.Collections.Generic;
using CalamitySchematicExporter.Common.Input;
using Terraria.UI;

namespace CalamitySchematicExporter.Common.UI;

[Autoload(Side = ModSide.Client)]
public sealed class SchematicUISystem : ModSystem
{
    // The game doesn't provide any 'GameTime' instance during rendering, so we have to keep track of it ourselves.
    private static GameTime lastGameTime;
    
    /// <summary>
    /// 
    /// </summary>
    public static UISchematic State { get; private set; }
    
    /// <summary>
    /// 
    /// </summary>
    public static UserInterface UserInterface { get; private set; }
    
    public override void Load()
    {
        base.Load();

        State = new UISchematic();

        UserInterface = new UserInterface();
        UserInterface.SetState(State);
        
        Deactivate();
    }

    public override void Unload()
    {
        base.Unload();
        
        Deactivate();
        
        State = null;
        UserInterface = null;
    }

    public override void PostUpdateInput()
    {
        base.PostUpdateInput();

        if (!SchematicKeybindSystem.CaptureKeybind.JustPressed)
        {
            return;
        }
     
        Toggle();
    }

    public override void UpdateUI(GameTime gameTime)
    {
        base.UpdateUI(gameTime);
        
        UserInterface.Update(gameTime);

        lastGameTime = gameTime;
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        base.ModifyInterfaceLayers(layers);
        
        var index = layers.FindIndex(layer => layer.Name == "Vanilla: Mouse Text");

        if (index == -1) {
            return;
        }
        
        layers.Insert(index + 1, new LegacyGameInterfaceLayer($"{nameof(CalamitySchematicExporter)}:{nameof(UISchematic)}", DrawState, InterfaceScaleType.Game));
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Activate()
    {
        State.Activate();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Deactivate()
    {
        State.Deactivate();
    }

    /// <summary>
    /// 
    /// </summary>
    public static void Toggle()
    {
        if (State.Active)
        {
            Deactivate();
        }
        else
        {
            Activate();
        }
    }
    
    private static bool DrawState() {
        UserInterface.Draw(Main.spriteBatch, lastGameTime);
        
        return true;
    }
}