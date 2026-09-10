using Godot;
using System;

public partial class MusicManager : Node
{
    const string TRACKPATH_TITLE                = "res://streamed_assets/audio/music/title.ogg";
    const string TRACKPATH_STAGE_1              = "res://streamed_assets/audio/music/stage_1.ogg";
    const string TRACKPATH_STAGE_1_MENU         = "res://streamed_assets/audio/music/stage_1_menu.ogg";
    const string TRACKPATH_STAGE_ICE            = "res://streamed_assets/audio/music/stage_ice.ogg";
    const string TRACKPATH_STAGE_ICE_MENU       = "res://streamed_assets/audio/music/stage_ice_menu.ogg";
    const string TRACKPATH_STAGE_SLIME          = "res://streamed_assets/audio/music/stage_slime.ogg";
    const string TRACKPATH_STAGE_SLIME_MENU     = "res://streamed_assets/audio/music/stage_slime_menu.ogg";

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
        STAGE_SLIME,
        TRACK_COUNT,
        SILENT      = 99
    }
    private MusicTrack[]        tracklist                   = new MusicTrack[(int)Tracks.TRACK_COUNT];
    private Tracks              currentTrack                = Tracks.SILENT;

    private AudioStreamPlayer   mPlayer_main                = null;
    private AudioStreamPlayer   mPlayer_menu                = null;

    private const float         DB_ENABLED                  =  0.0f;
    private const float         DB_DISABLED                 = -30.0f;
    private const float         DB_CHANGE_WEIGHT_PLUS       = 20.0f;
    private const float         DB_CHANGE_WEIGHT_MINUS      = 5.0f;
    private bool                curTrackIs1                 = false;
    private float               track1_targetDB             = DB_ENABLED;
    private float               track1_curDB                = DB_ENABLED;

    public static MusicManager  i                           = null;



    public override void _Ready()
    {
        tracklist[(int)Tracks.TITLE]        = new MusicTrack(GD.Load<AudioStream>(TRACKPATH_TITLE));
        tracklist[(int)Tracks.STAGE_1]      = new MusicTrack(GD.Load<AudioStream>(TRACKPATH_STAGE_1),       GD.Load<AudioStream>(TRACKPATH_STAGE_1_MENU));
        tracklist[(int)Tracks.STAGE_ICE]    = new MusicTrack(GD.Load<AudioStream>(TRACKPATH_STAGE_ICE),     GD.Load<AudioStream>(TRACKPATH_STAGE_ICE_MENU));
        tracklist[(int)Tracks.STAGE_SLIME]  = new MusicTrack(GD.Load<AudioStream>(TRACKPATH_STAGE_SLIME),   GD.Load<AudioStream>(TRACKPATH_STAGE_SLIME_MENU));


        mPlayer_main = GetNode<AudioStreamPlayer>("%MPlayerMain"); mPlayer_main.Playing = true;
        mPlayer_menu = GetNode<AudioStreamPlayer>("%MPlayerMenu"); mPlayer_menu.Playing = true;

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
        // Individual channel control
        if (Global.i.IsInMenu())
        {
            mPlayer_menu.VolumeDb = Mathf.Lerp(mPlayer_menu.VolumeDb,  track1_curDB,   DB_CHANGE_WEIGHT_PLUS   * Global.i.GetClampedDelta_PR());
            mPlayer_main.VolumeDb = Mathf.Lerp(mPlayer_main.VolumeDb,  DB_DISABLED,    DB_CHANGE_WEIGHT_MINUS  * Global.i.GetClampedDelta_PR());
        }
        else
        {
            mPlayer_menu.VolumeDb = Mathf.Lerp(mPlayer_menu.VolumeDb,  DB_DISABLED,    DB_CHANGE_WEIGHT_MINUS  * Global.i.GetClampedDelta_PR());
            mPlayer_main.VolumeDb = Mathf.Lerp(mPlayer_main.VolumeDb,  track1_curDB,   DB_CHANGE_WEIGHT_PLUS   * Global.i.GetClampedDelta_PR());
        }
    }


    public void ChangeTrack(Tracks nTrack)
    {
        if (nTrack == Tracks.SILENT)
        {
            mPlayer_main.Stop();
            mPlayer_menu.Stop();
            return;
        }
        // Don't update the track if it's already being played
        if (nTrack == currentTrack)
        {
            return;
        }
        currentTrack = nTrack;
        Global.Log("MusicManager.ChangeTrack()", "engine_resources/scripts/cs/singleton/MusicManager.cs", $"Changing Track to ID: {nTrack}");

        MusicTrack targetTrack = this.tracklist[(int)nTrack];

        mPlayer_main.Stream = targetTrack.GetNonMenuTrack();
        if (targetTrack.MenuTrackEnabled())
        {
            mPlayer_menu.Stream = targetTrack.GetMenuTrack();
        }
        else
        {
            mPlayer_menu.Stream = targetTrack.GetNonMenuTrack();
        }
        track1_targetDB = DB_ENABLED;

        mPlayer_main.Play();
        mPlayer_menu.Play();

    }

}
