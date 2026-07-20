using Godot;
using System;

[GlobalClass]
public partial class TbbPopMenu : TextButtonBehavior
{
    public override void Hovered()
    {
        
        return;
    }
    public override void UnHovered()
    {
        return;
    }
    public override void Pressed()
    {
        UiManager.i.UiPop();
    }
}
