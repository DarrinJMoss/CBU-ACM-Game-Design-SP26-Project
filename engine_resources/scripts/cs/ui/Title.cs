using Godot;
using Godot.NativeInterop;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public partial class Title : CanvasLayer
{

    private UiContainer     _MainMenu_Root      = null;
    private UiContainer     _Settings_Root      = null;

    private AnimationPlayer _Anim               = null;

    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        _MainMenu_Root   = GetNode<UiContainer>("MenuRoots/Menu");
        _Settings_Root   = GetNode<UiContainer>("MenuRoots/Settings");

        _Anim            = GetNode<AnimationPlayer>("Anim");

        // Link every button to this script
        foreach (Node button in GetTree().GetNodesInGroup("Buttons"))
        {
            if (button is TextButton tb)
            {
                tb.TbPressed += _ButtonPressed;
            }
        }

        _Anim.Play("Intro");

        await ToSignal(_Anim, AnimationPlayer.SignalName.AnimationFinished);

        _Anim.Play("FlickOn");

        UiManager.i.UiPush(_MainMenu_Root);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {

    }

    private void _ButtonPressed(string caller, string[] args)
    {
        
        switch (caller)
        {
            // Menu
            case "Main_Credits":
                _ShowCredits();
                break;

            // Level Select
            case "Main_NewGame":
            case "Levels_1":
                GetTree().ChangeSceneToFile("res://engine_resources/scenes/levels/test_level.tscn");
                break;
            case "Levels_2":
                _BeginGame(Global.Levels.LV2_ICE);
                break;
            case "Levels_3":
                _BeginGame(Global.Levels.LV3_SLIME);
                break;
            case "Levels_4":
                _BeginGame(Global.Levels.LV4_BOUNCE);
                break;
            case "Levels_5":
                _BeginGame(Global.Levels.LV5_CANNONS);
                break;

            // Quit AYS
            case "Ays_Yes":
                GetTree().Quit();
                break;
            
            default:
                break;
        }

    }


    private void _BeginGame(Global.Levels lvIdx)
    {
        UiManager.i.UiWipeStack();
    }

    private void _ShowCredits()
    {
        UiManager.i.UiWipeStack();
    }

}
