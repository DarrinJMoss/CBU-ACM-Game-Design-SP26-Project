using Godot;
using System.Collections.Generic;

public partial class UiManager : Node
{
    
    public static UiManager i;

    private Stack<UiContainer> uiStack = new Stack<UiContainer>();

    public bool isKeyboardMode = false;


    public override void _Ready()
    {
        if (i == null)
        {
            i = this;
        }
        else
        {
            this.QueueFree();
        }
        
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouse)
        {
            isKeyboardMode = false;
            return;
        }
        if (@event is InputEventKey k)
        {
            if (k.Keycode == Godot.Key.Left || k.Keycode == Godot.Key.Right || k.Keycode == Godot.Key.Up || k.Keycode == Godot.Key.Down)
            {
                isKeyboardMode = true;
            }
        }
    }




    public UiContainer UiPop()
    {
        if (uiStack.Count <= 0)
        {
            return null;
        }
        return uiStack.Pop();
    }
    public void UiPush(UiContainer ui)
    {
        uiStack.Push(ui);
    }
    public UiContainer UiPeek()
    {
        if (uiStack.Count <= 0)
        {
            return null;
        }
        return uiStack.Peek();
    }
    public bool IsInStack(UiContainer ui)
    {
        return uiStack.Contains(ui);
    }
    public void UiWipeStack()
    {
        uiStack.Clear();
    }
    public int GetStackSize()
    {
        return uiStack.Count;
    }

    public bool KeyboardModeEnabled()
    {
        return isKeyboardMode;
    }


}
