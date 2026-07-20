using Godot;
using System;

public partial class OptionCursor : Control
{

    public static OptionCursor instance;

    private const  float   XOFFSET_INIT = -16.0f;

    public Control currentFocus = null;

    public override void _Ready()
    {
        GetViewport().GuiFocusChanged += _FocusChanged;
        instance = this;
        HideCursor();
        GD.Print("Hello!");
    }


    public override void _Input(InputEvent @event)
    {
        if (GetViewport().GuiGetFocusOwner() == null)
        {
            currentFocus = null;
            HideCursor();
        }
    }


    public override void _Process(double delta)
    {
        this.GlobalPosition = this.GlobalPosition.Lerp(currentFocus.GlobalPosition, 1.0f * Global.i.GetClampedDelta_PR() * 15.0f);
    }



    private void _FocusChanged(Control newFocus)
    {

        currentFocus = newFocus;

        if (currentFocus != null)
        {
            if (currentFocus is TextButton tB)
            {
                if (!tB.showCursorOnHover)
                {
                    HideCursor();
                    return;
                }
            }
            _ShowCursor();
            return;
        }
        HideCursor();
    }


    public void HideCursor()
    {
        this.SetProcess(false);
        this.SetProcessInput(false);
        this.Hide();
    }
    private void _ShowCursor()
    {
        this.SetProcess(true);
        this.SetProcessInput(true);
        this.Show();
        this.GlobalPosition = currentFocus.GlobalPosition + Vector2.Right * XOFFSET_INIT;
    }
    
}
