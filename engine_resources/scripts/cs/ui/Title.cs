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
        GD.Print($"Caller {caller}");
        switch (caller)
        {
            // Menu
            case "Main_Credits":
                _ShowCredits();
                break;

            // Level Select
            case "Main_NewGame":
                _BeginGame(Global.Levels.TESTING_LEVEL);
                break;
            case "Levels_1":
                _BeginGame(Global.Levels.LV1_FACILITY);
                break;
            case "Levels_2":
                _BeginGame(Global.Levels.LV2_ICE);
                break;
            case "Levels_3":
                /*
                    S <  1:30
                    A <  2:00
                    B <  2:30
                    C <  3:00
                    D <  3:30
                    F >= 3:30
                */
                _BeginGame(Global.Levels.LV3_SLIME);
                break;
            case "Levels_4":
                /*
                    S <  1:00
                    A <  1:30
                    B <  2:00
                    C <  2:30
                    D <  3:00
                    F >= 3:30
                */
                _BeginGame(Global.Levels.LV4_BOUNCE);
                break;
            case "Levels_5":
                /*
                    S <  0:45
                    A <  1:15
                    B <  1:45
                    C <  2:15
                    D <  2:45
                    F >= 2:45
                */
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
        GD.Print($"lvIdx : {lvIdx.ToString()}");
        UiManager.i.UiWipeStack();
        switch (lvIdx)
        {
            case Global.Levels.LV1_FACILITY:
                GetTree().ChangeSceneToFile("res://engine_resources/scenes/levels/1_complex.tscn");
                break;
            case Global.Levels.LV2_ICE:
                GetTree().ChangeSceneToFile("res://engine_resources/scenes/levels/test_level.tscn");
                break;
            case Global.Levels.LV3_SLIME:
                GetTree().ChangeSceneToFile("res://engine_resources/scenes/levels/3_slime.tscn");
                break;
            case Global.Levels.LV4_BOUNCE:
                GetTree().ChangeSceneToFile("res://engine_resources/scenes/levels/4_bounce.tscn");
                break;
            case Global.Levels.LV5_CANNONS:
                GetTree().ChangeSceneToFile("res://engine_resources/scenes/levels/5_cannon.tscn");
                break;
            default:
                GetTree().ChangeSceneToFile("res://engine_resources/scenes/levels/test_level.tscn");
                break;
        }
    }

    private void _ShowCredits()
    {
        UiManager.i.UiWipeStack();
    }

}
