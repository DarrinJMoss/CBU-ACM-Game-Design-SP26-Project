using Godot;
using System;

[GlobalClass]
public partial class TbbChangeMenu : TextButtonBehavior
{
    
    [Export] NodePath targetMenuPath;

    public override void Hovered()
    {
        GetNodeFromPath<UiContainer>(targetMenuPath).OverrideVisibility(true);
    }
    public override void UnHovered()
    {
        GetNodeFromPath<UiContainer>(targetMenuPath).OverrideVisibility(false);
    }
    public override void Pressed()
    {
        UiManager.i.UiPush(GetNodeFromPath<UiContainer>(targetMenuPath));
    }

}
