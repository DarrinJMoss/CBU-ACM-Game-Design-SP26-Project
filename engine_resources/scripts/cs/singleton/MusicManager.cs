using Godot;
using System;

public partial class MusicManager : Node
{
    const string TRACKPATH_TITLE                = "res://streamed_assets/audio/music/title.ogg";
    const string TRACKPATH_STAGE_1              = "res://streamed_assets/audio/music/stage_1.ogg";
    const string TRACKPATH_STAGE_1_MENU         = "res://streamed_assets/audio/music/stage_1_menu.ogg";
    const string TRACKPATH_STAGE_ICE            = "res://streamed_assets/audio/music/stage_ice.ogg";
    const string TRACKPATH_STAGE_ICE_MENU       = "res://streamed_assets/audio/music/stage_ice_menu.ogg";

    private class MusicTrack
    {
        AudioStream     nonMenuTrack    = null;
        AudioStream     menuTrack       = null;
        bool            menuTrackEn     = false;

        public MusicTrack(AudioStream track)
        {
            this.nonMenuTrack   = track;
            this.menuTrack      = null;
            this.menuTrackEn    = false;
        }
        public MusicTrack(AudioStream nmTrack, AudioStream mTrack)
        {
            this.nonMenuTrack   = nmTrack;
            this.menuTrack      = mTrack;
            this.menuTrackEn    = true;
        }

        public bool MenuTrackEnabled()
        {
            return this.menuTrackEn;
        }
        public AudioStream GetNonMenuTrack()
        {
            return this.nonMenuTrack;
        }
        public AudioStream GetMenuTrack()
        {
            return this.menuTrack;
        }
    }

    public enum Tracks
    {
        TITLE       = 0,
        STAGE_1,
        STAGE_ICE,
        TRACK_COUNT,
        SILENT      = 99
    }
    private MusicTrack[]        tracklist                   = new MusicTrack[(int)Tracks.TRACK_COUNT];
    private Tracks              currentTrack                = Tracks.SILENT;

    private AudioStreamPlayer   track1_reg                  = null;
    private AudioStreamPlayer   track1_menu                 = null;
    private AudioStreamPlayer   track2_reg                  = null;
    private AudioStreamPlayer   track2_menu                 = null;

    private const float         DB_ENABLED                  =  0.0f;
    private const float         DB_DISABLED                 = -30.0f;
    private const float         DB_CHANGE_WEIGHT_PLUS       = 20.0f;
    private const float         DB_CHANGE_WEIGHT_MINUS      = 5.0f;
    private const float         TRACK_DISABLE_THRESHHOLD    = -7.0f;
    private const float         TRACK_ENABLE_THRESHOLD      = -7.0f;
    private bool                curTrackIs1                 = false;
    private float               track1_targetDB             = DB_ENABLED;
    private float               track1_curDB                = DB_ENABLED;
    private bool                track1_enabled              = false;
    private float               track2_targetDB             = DB_DISABLED;
    private float               track2_curDB                = DB_DISABLED;
    private bool                track2_enabled              = false;

    public static MusicManager  i                           = null;



    public override void _Ready()
    {
        tracklist[(int)Tracks.TITLE]        = new MusicTrack(GD.Load<AudioStream>(TRACKPATH_TITLE));
        tracklist[(int)Tracks.STAGE_1]      = new MusicTrack(GD.Load<AudioStream>(TRACKPATH_STAGE_1),       GD.Load<AudioStream>(TRACKPATH_STAGE_1_MENU));
        tracklist[(int)Tracks.STAGE_ICE]    = new MusicTrack(GD.Load<AudioStream>(TRACKPATH_STAGE_ICE),     GD.Load<AudioStream>(TRACKPATH_STAGE_ICE_MENU));

        track1_reg  = GetNode<AudioStreamPlayer>("%T1Reg");
        track1_menu = GetNode<AudioStreamPlayer>("%T1Menu");
        track2_reg  = GetNode<AudioStreamPlayer>("%T2Reg");
        track2_menu = GetNode<AudioStreamPlayer>("%T2Menu");

        if (MusicManager.i == null)
        {
            Global.Log("MusicManager._Ready()", "engine_resources/scripts/cs/singleton/MusicManager.cs", "Created instance of MusicManager singleton.");
            MusicManager.i = this; 
        }
        else
        {
            Global.LogWarning("MusicManager._Ready()", "engine_resources/scripts/cs/singleton/MusicManager.cs", "Instance of MusicManager singleton already exists.");
            this.QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        // Track one master control
        bool pt1_en = track1_enabled;
        track1_curDB = track1_targetDB;
        if (track1_curDB >= TRACK_ENABLE_THRESHOLD)
        {
            track1_enabled      = true;
        }
        else if (track1_curDB <= TRACK_DISABLE_THRESHHOLD)
        {
            track1_enabled      = false;
        }

        if (pt1_en != track1_enabled)
        {
            if (track1_enabled)
            {
                StartTrack1();
            }
            else
            {
                StopTrack1();
            }
        }


        // Track two master control
        bool pt2_en = track2_enabled;
        track2_curDB = track2_targetDB;
        if (track2_curDB >= TRACK_ENABLE_THRESHOLD)
        {
            track2_enabled      = true;
        }
        else if (track2_curDB <= TRACK_DISABLE_THRESHHOLD)
        {
            track2_enabled      = false;
        }

        if (pt2_en != track2_enabled)
        {
            if (track2_enabled)
            {
                StartTrack2();
            }
            else
            {
                StopTrack2();
            }
        }


        // Individual channel control
        if (Global.i.IsInMenu())
        {
            track1_menu.VolumeDb    = Mathf.Lerp(track1_menu.VolumeDb,  track1_curDB,   DB_CHANGE_WEIGHT_PLUS   * Global.i.GetClampedDelta_PR());
            track1_reg.VolumeDb     = Mathf.Lerp(track1_reg.VolumeDb,   DB_DISABLED,    DB_CHANGE_WEIGHT_MINUS  * Global.i.GetClampedDelta_PR());

            track2_menu.VolumeDb    = Mathf.Lerp(track2_menu.VolumeDb,  track2_curDB,   DB_CHANGE_WEIGHT_PLUS   * Global.i.GetClampedDelta_PR());
            track2_reg.VolumeDb     = Mathf.Lerp(track2_reg.VolumeDb,   DB_DISABLED,    DB_CHANGE_WEIGHT_MINUS  * Global.i.GetClampedDelta_PR());
        }
        else
        {
            track1_menu.VolumeDb    = Mathf.Lerp(track1_menu.VolumeDb,  DB_DISABLED,    DB_CHANGE_WEIGHT_MINUS  * Global.i.GetClampedDelta_PR());
            track1_reg.VolumeDb     = Mathf.Lerp(track1_reg.VolumeDb,   track1_curDB,   DB_CHANGE_WEIGHT_PLUS   * Global.i.GetClampedDelta_PR());

            track2_menu.VolumeDb    = Mathf.Lerp(track2_menu.VolumeDb,  DB_DISABLED,    DB_CHANGE_WEIGHT_MINUS  * Global.i.GetClampedDelta_PR());
            track2_reg.VolumeDb     = Mathf.Lerp(track2_reg.VolumeDb,   track2_curDB,   DB_CHANGE_WEIGHT_PLUS   * Global.i.GetClampedDelta_PR());
        }
    }


    public void ChangeTrack(Tracks nTrack)
    {
        // Don't update the track if it's already being played
        if (nTrack == currentTrack)
        {
            return;
        }
        currentTrack = nTrack;
        Global.Log("MusicManager.ChangeTrack()", "engine_resources/scripts/cs/singleton/MusicManager.cs", $"Changing Track to ID: {nTrack}");

        if (nTrack == Tracks.SILENT)
        {
            track2_targetDB = DB_DISABLED;
            track1_targetDB = DB_DISABLED;
            return;
        }

        MusicTrack targetTrack = this.tracklist[(int)nTrack];

        curTrackIs1 = !curTrackIs1;

        if (curTrackIs1)
        {
            Global.Log("MusicManager.ChangeTrack()", "engine_resources/scripts/cs/singleton/MusicManager.cs", "Current track is 1");
            track1_reg.Stream = targetTrack.GetNonMenuTrack();
            if (targetTrack.MenuTrackEnabled())
            {
                track1_menu.Stream = targetTrack.GetMenuTrack();
            }
            else
            {
                track1_menu.Stream = targetTrack.GetNonMenuTrack();
            }
            track1_targetDB = DB_ENABLED;
            track2_targetDB = DB_DISABLED;
            GD.Print(track1_targetDB);
        }
        else
        {
            Global.Log("MusicManager.ChangeTrack()", "engine_resources/scripts/cs/singleton/MusicManager.cs", "Current track is 2");
            track2_reg.Stream = targetTrack.GetNonMenuTrack();
            if (targetTrack.MenuTrackEnabled())
            {
                track2_menu.Stream = targetTrack.GetMenuTrack();
            }
            else
            {
                track2_menu.Stream = targetTrack.GetNonMenuTrack();
            }
            track1_targetDB = DB_DISABLED;
            track2_targetDB = DB_ENABLED;
        }


    }

    private void StartTrack1()
    {
        track1_reg.Play();
        track1_menu.Play();
    }
    private void StopTrack1()
    {
        track1_reg.Stop();
        track1_menu.Stop();
    }

    private void StartTrack2()
    {
        track2_reg.Play();
        track2_menu.Play();
    }
    private void StopTrack2()
    {
        track2_reg.Stop();
        track2_menu.Stop();
    }

}
