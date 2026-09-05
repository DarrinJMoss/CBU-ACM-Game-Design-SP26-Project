using Godot;
using System;

[GlobalClass]
public partial class TbbChangeMenu : TextButtonBehavior
{
    
    [Export] NodePath targetMenuPath;

    public override void Hovered()
    {
        UiManager.i.targetedUi = GetNodeFromPath<UiContainer>(targetMenuPath);
        GetNodeFromPath<UiContainer>(targetMenuPath).OverrideVisibility(true);
        
    }
    public override void UnHovered()
    {
        UiManager.i.targetedUi = null;
        GetNodeFromPath<UiContainer>(targetMenuPath).OverrideVisibility(false);
    }
    public override void Pressed()
    {
        UiManager.i.targetedUi = null;
        UiManager.i.UiPush(GetNodeFromPath<UiContainer>(targetMenuPath));
    }

}
