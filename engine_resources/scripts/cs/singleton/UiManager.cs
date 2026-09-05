using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class UiManager : Node
{
    
    public static UiManager i;

    private Stack<UiContainer> uiStack = new Stack<UiContainer>();

    public UiContainer targetedUi = null;

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
        _SanitizeStack();
        if (uiStack.Count <= 0)
        {
            return null;
        }
        return uiStack.Pop();
    }
    public void UiPush(UiContainer ui)
    {
        _SanitizeStack();
        uiStack.Push(ui);
    }
    public UiContainer UiPeek()
    {
        _SanitizeStack();
        if (uiStack.Count <= 0)
        {
            return null;
        }
        UiContainer p = uiStack.Peek();
        return p;
    }
    public bool IsInStack(UiContainer ui)
    {
        _SanitizeStack();
        return uiStack.Contains(ui);
    }
    public void UiWipeStack()
    {
        uiStack.Clear();
    }
    public int GetStackSize()
    {
        _SanitizeStack();
        return uiStack.Count;
    }

    /// <summary>
    /// Checks the topmost element of the UI stack and makes sure its valid.
    /// Removes it if it is not.
    /// </summary>
    private void _SanitizeStack()
    {
        if (uiStack.Count <= 0) { return; }
        UiContainer p = uiStack.Pop();
        if (GodotObject.IsInstanceValid(p))
        {
            uiStack.Push(p);
        }
    }
    public List<string> GetStackNames()
    {
        _SanitizeStack();
        List<string> returnList = new List<string>();
        foreach (UiContainer ui in this.uiStack)
        {
            returnList.Add(ui.Name);
        }
        returnList.Reverse();
        return returnList;
    }

    public bool KeyboardModeEnabled()
    {
        return isKeyboardMode;
    }


}
