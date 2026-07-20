using Godot;
using System;

[GlobalClass]
public partial class TbbEmitSignal : TextButtonBehavior
{

    [Export] String[] optionalArgs;

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
        this.ParentButtonRef.EmitSignal("TbPressed", ParentButtonRef.Name, optionalArgs);
    }



}
