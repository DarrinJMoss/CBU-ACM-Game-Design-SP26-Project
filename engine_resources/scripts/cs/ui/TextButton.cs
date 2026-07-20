using Godot;
using System;

public partial class TextButton : RichTextLabel
{

    [Export] public UiContainer Manager;    
    [Export] public String buttonText;
    [Export] public TextButtonBehavior behavior = null;
    [Export] public bool showCursorOnHover = true;


    [Signal] public delegate void TbPressedEventHandler(String caller, String[] args);


    private bool isHovered = false;


    public override void _Ready()
    {
        if (behavior == null)
        {
            GD.PrintErr("ERR in TextButton.cs | TextButton::_Ready() | No behavior given to button. Deleting button");
            this.QueueFree();
            return;
        }

        // Tell the behavior class that we exist
        behavior.Register(this);

        this.Text = buttonText;

        this.MouseEntered += _TextButtonHovered;
        this.MouseExited  += _TextButtonUnHovered;
    }


    public override void _Input(InputEvent @event)
    {
        if (!Manager.IsInteractable())
        {
            if (isHovered)
            {
                _TextButtonUnHovered();
            }
            return;
        }
        if (isHovered && Input.IsActionJustReleased("MouseDown"))
        {
            behavior.Pressed();
        }
    }



    private void _TextButtonHovered()
    {
        isHovered = true;
        this.Text = "[color=yellow]" + buttonText + "[/color]";
        behavior.Hovered();
        this.GrabFocus();
    }
    private void _TextButtonUnHovered()
    {
        isHovered = false;
        this.Text = buttonText;
        behavior.UnHovered();
        this.ReleaseFocus();
    }


}
