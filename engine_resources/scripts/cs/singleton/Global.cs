using Godot;
using System;
using System.Collections.Generic;

public partial class Global : Node
{
    public static Global i = null;

    public const float  CONTROLLER_DEADZONE     = 0.2f;

    private const bool  ENABLE_REGULAR_LOGS     = true;

    // Reference to the current player node if any script needs it.
    //public Player PlayerRef;

    public override void _Ready()
    {
        if (Global.i == null)
        {
            Global.Log("Global._Ready()", "engine_resources/scripts/cs/singleton/Global.cs", "Created instance of Global singleton.");
            Global.i = this; 
        }
        else
        {
            Global.LogWarning("Global._Ready()", "engine_resources/scripts/cs/singleton/Global.cs", "Instance of Global singleton already exists.");
            this.QueueFree();
        }
    }

    public static void Log(string method, string file, string txt)
    {
        if (!ENABLE_REGULAR_LOGS)
        {
            return;
        }
        GD.Print($"In {file} | {method} -> {txt}");
    }
    public static void LogWarning(string method, string file, string txt)
    {
        GD.PushWarning($"WARNING in {file} | {method} -> {txt}");
    }
    public static void LogError(string method, string file, string txt)
    {
        GD.PushError($"WARNING in {file} | {method} -> {txt}");
    }

    public float GetClampedDelta_PH()
    {
        return Mathf.Clamp((float)GetPhysicsProcessDeltaTime(), 0.0f, 1.0f);
    }
    public double dGetClampedDelta_PH()
    {
        return Mathf.Clamp(GetPhysicsProcessDeltaTime(), 0.0, 1.0);
    }
    public float GetClampedDelta_PR()
    {
        return Mathf.Clamp((float)GetProcessDeltaTime(), 0.0f, 1.0f);
    }
    public double dGetClampedDelta_PR()
    {
        return Mathf.Clamp(GetProcessDeltaTime(), 0.0, 1.0);
    }
    public bool IsInMenu()
    {
        return UiManager.i.GetStackSize() != 0;
    }


}
