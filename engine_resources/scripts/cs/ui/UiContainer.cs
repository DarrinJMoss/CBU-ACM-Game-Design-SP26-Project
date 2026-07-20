using Godot;
using System;

public partial class UiContainer : Control
{

    private const float NOT_TOP_LUM = 0.5f;

    private bool overrideVisible = false;
    public  bool titleScreenHack = false;

    public override void _Process(double delta)
    {
        if (titleScreenHack) 
        {
            this.Show();
            this.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else if (overrideVisible || UiManager.i.IsInStack(this))
        {
            this.Show();
            if (UiManager.i.UiPeek() != this)
            {
                this.Modulate = new Color(NOT_TOP_LUM, NOT_TOP_LUM, NOT_TOP_LUM, 1.0f);
            }
            else
            {
                this.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }
        }
        else
        {
            this.Hide();
        }
    }

    public bool IsInteractable()
    {
        return UiManager.i.UiPeek() == this;
    }

    public void OverrideVisibility(bool v)
    {
        overrideVisible = v;
    }

}
