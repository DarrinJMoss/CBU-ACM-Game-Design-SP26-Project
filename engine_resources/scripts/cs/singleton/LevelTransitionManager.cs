using Godot;
using System;
using System.Threading.Tasks;

public partial class LevelTransitionManager : CanvasLayer
{

    private ColorRect           ShaderRect                          = null;
    const   float               OPENCLOSE_SPEED                     = 15.0f;

    const   float               SHADER_CURVATURE_END                = 0.1f;
    const   float               SHADER_CURAVUTRE_STR                = 0.0f;

    const   float               SHADER_CURVATURE_SOFT_END           = 0.2f;
    const   float               SHADER_CURAVUTRE_SOFT_STR           = 0.0f;

    const   float               SHADER_VIGNETTE_END                 = 0.2f;
    const   float               SHADER_VIGNETTE_STR                 = 0.0f;

    const   float               SHADER_BRIGHTNESS_END               = 0.8f;
    const   float               SHADER_BRIGHTNESS_STR               = 1.0f;

    const   float               SHADER_CONTRAST_END                 = 1.5f;
    const   float               SHADER_CONTRAST_STR                 = 1.0f;

    const   float               SHADER_CHROMA_END                   = 3.0f;
    const   float               SHADER_CHROMA_STR                   = 0.0f;

    const   float               SHADER_SCANLINE_END                 = 0.95f;
    const   float               SHADER_SCANLINE_STR                 = 0.0f;

    private float               _shaderPercent                      = 0.0f;

    private bool                _resultsOpen                        = false;

    public class CcLevelTimestamp
    {
        public uint     minutes         = 0xFFFFFFFF;
        public uint     seconds         = 0xFFFFFFFF;
        public float    secondsFraction = -1.0f;
        public double   rawTime         = 0.0;

        public void Tick(double delta)
        {
            this.rawTime += delta;
        }

        public void ComputeMSF()
        {
            this.minutes = (uint)Mathf.Floor(this.rawTime / 60.0);
            this.seconds = (uint)Mathf.Floor(this.rawTime - (60.0 * this.minutes));
            this.secondsFraction = (float)(this.rawTime - ((double)minutes * 60.0) - (double)seconds);
        }

    }

    public static LevelTransitionManager i = null;

    public CcLevelTimestamp     lvTimestamp_1Complex         = null;
    public CcLevelTimestamp     lvTimestamp_2Ice             = null;
    public CcLevelTimestamp     lvTimestamp_3Slime           = null;
    public CcLevelTimestamp     lvTimestamp_4Bounce          = null;
    public CcLevelTimestamp     lvTimestamp_5Cannon          = null;

    private Global.Levels       _curLevel                   = Global.Levels.TESTING_LEVEL;
    private CcLevelTimestamp    _curTimestamp               = null;

    private RichTextLabel       _LB_Header                  = null;
    private RichTextLabel       _LB_Lhs                     = null;
    private RichTextLabel       _LB_RHS_TimeTaken           = null;
    private RichTextLabel       _LB_RHS_Rank                = null;
    private UiContainer         _LevelProceedMenu           = null;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        if (LevelTransitionManager.i == null)
        {
            Global.Log("LevelTransitionManager._Ready()", "engine_resources/scripts/cs/singleton/LevelTransitionManager.cs", "Created instance of LevelTransitionManager singleton.");
            LevelTransitionManager.i = this;
        }
        else
        {
            Global.LogWarning("LevelTransitionManager._Ready()", "engine_resources/scripts/cs/singleton/LevelTransitionManager.cs", "Instance of LevelTransitionManager singleton already exists.");
        }
    
        ShaderRect = GetNode<ColorRect>("Shader");
        if (ShaderRect.Material is ShaderMaterial shader) { shader.SetShaderParameter("alpha", 0.0f); }

        _LB_Header          = GetNode<RichTextLabel>("LevelCompleteTxt");
        _LB_Lhs             = GetNode<RichTextLabel>("MidLHS");
        _LB_RHS_Rank        = GetNode<RichTextLabel>("RHSRankTxt");
        _LB_RHS_TimeTaken   = GetNode<RichTextLabel>("RHSTimeTakenTxt");
    
        _LB_Header          .Hide();
        _LB_Lhs             .Hide();
        _LB_RHS_Rank        .Hide();
        _LB_RHS_TimeTaken   .Hide();

        _LevelProceedMenu   = GetNode<UiContainer>("LevelProceedOptions");

        _curLevel = Global.Levels.LV1_FACILITY;

        _curTimestamp = new CcLevelTimestamp
        {
            rawTime = 180.0
        };

        _ = CheckoutTimestamp(Global.Levels.LV2_ICE);

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (this._curTimestamp != null)
        {
            _curTimestamp.Tick(delta);
        }

        if (_resultsOpen)
        {   
            _shaderPercent = Mathf.Lerp(_shaderPercent, 1.0f, OPENCLOSE_SPEED * Global.i.GetClampedDelta_PR());
        }
        else
        {
            _shaderPercent = Mathf.Lerp(_shaderPercent, 0.0f, OPENCLOSE_SPEED * Global.i.GetClampedDelta_PR());
        }

        if (ShaderRect.Material is ShaderMaterial shader)
        {
            shader.SetShaderParameter("alpha",              Mathf.Lerp(0.0f,                        1.0f,                       _shaderPercent * 8.5f));
            ShaderRect.Show();

            shader.SetShaderParameter("curvature",          Mathf.Lerp(SHADER_CURAVUTRE_STR,        SHADER_CURVATURE_END,       _shaderPercent));
            shader.SetShaderParameter("corner_soften",      Mathf.Lerp(SHADER_CURAVUTRE_SOFT_STR,   SHADER_CURVATURE_SOFT_END,  _shaderPercent));
            shader.SetShaderParameter("vignette",           Mathf.Lerp(SHADER_VIGNETTE_STR,         SHADER_VIGNETTE_END,        _shaderPercent));
            shader.SetShaderParameter("brightness",         Mathf.Lerp(SHADER_BRIGHTNESS_STR,       SHADER_BRIGHTNESS_END,      _shaderPercent));
            shader.SetShaderParameter("contrast",           Mathf.Lerp(SHADER_CONTRAST_STR,         SHADER_CONTRAST_END,        _shaderPercent));
            shader.SetShaderParameter("chroma_offset_px",   Mathf.Lerp(SHADER_CHROMA_STR,           SHADER_CHROMA_END,          _shaderPercent));
            shader.SetShaderParameter("scanline_strength",  Mathf.Lerp(SHADER_SCANLINE_STR,         SHADER_SCANLINE_END,        _shaderPercent));
        }
    }

    public void OpenTimestamp(Global.Levels lvID)
    {
        this._curTimestamp = new CcLevelTimestamp();
        this._curLevel = lvID;
    }

    public async Task CheckoutTimestamp(Global.Levels nextLvID)
    {
        CcLevelTimestamp displayTimestamp = new CcLevelTimestamp();
        _curTimestamp.ComputeMSF();

        switch (this._curLevel)
        {
            case Global.Levels.LV1_FACILITY:
                lvTimestamp_1Complex    = _curTimestamp;   break;

            case Global.Levels.LV2_ICE:
                lvTimestamp_2Ice        = _curTimestamp;   break;

            case Global.Levels.LV3_SLIME:
                lvTimestamp_3Slime      = _curTimestamp;   break;

            case Global.Levels.LV4_BOUNCE:
                lvTimestamp_4Bounce     = _curTimestamp;   break;

            case Global.Levels.LV5_CANNONS:
                lvTimestamp_5Cannon     = _curTimestamp;   break;

            default:
                Global.LogError("LevelTransitionManager.CheckoutTimestamp()", "engine_resources/scripts/cs/singleton/LevelTransitionManager.cs", $"Invalid game level encountered when checking out. ID = {_curLevel}. Was LevelTransistionManager.i.StartTimestamp() called?");
                break;
        }

        await ToSignal(GetTree().CreateTimer(5.0f), Timer.SignalName.Timeout);

        _resultsOpen = true;

        await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);

        _LB_Header.Show();

        await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);

        _LB_Lhs.Text = "TIME TAKEN:\n "; _LB_Lhs.Show();

        await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);

        do
        {
            displayTimestamp.ComputeMSF();

            string lp_minText;
            if (displayTimestamp.minutes < 10) {
                lp_minText = $"0{displayTimestamp.minutes}";
            }
            else
            {
                lp_minText = displayTimestamp.minutes.ToString();
            }

            string lp_secText;
            if (displayTimestamp.seconds < 10) {
                lp_secText = $"0{displayTimestamp.seconds}";
            }
            else
            {
                lp_secText = displayTimestamp.seconds.ToString();
            }

            string lp_frcText = Mathf.Round(displayTimestamp.secondsFraction * 100.0f).ToString();
            if (lp_frcText.Length == 1)
            {
                lp_frcText += "0";
            }

            _LB_RHS_TimeTaken.Text = $"{lp_minText}:{lp_secText}.{lp_frcText}"; _LB_RHS_TimeTaken.Show();

            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

            displayTimestamp.rawTime += GetProcessDeltaTime() * 60.0 * Mathf.Max(1.0f, (float)displayTimestamp.rawTime / 120.0f);

        } while (_curTimestamp.rawTime > displayTimestamp.rawTime);

        string minText;
        if (displayTimestamp.minutes < 10) {
            minText = $"0{displayTimestamp.minutes}";
        }
        else
        {
            minText = displayTimestamp.minutes.ToString();
        }

        string secText;
        if (displayTimestamp.seconds < 10) {
            secText = $"0{displayTimestamp.seconds}";
        }
        else
        {
            secText = displayTimestamp.seconds.ToString();
        }

        string frcText = Mathf.Round(displayTimestamp.secondsFraction * 100.0f).ToString();
        if (frcText.Length == 1)
        {
            frcText += "0";
        }

        _LB_RHS_TimeTaken.Text = $"[color=yellow][shake]{minText}:{secText}.{frcText}"; _LB_RHS_TimeTaken.Show();

        await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);

        _LB_Lhs.Text = "TIME TAKEN:\nRANK:";

        await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);

        _LB_RHS_Rank.Text = "[color=yellow][shake]Beans"; _LB_RHS_Rank.Show();

        await ToSignal(GetTree().CreateTimer(0.5f), Timer.SignalName.Timeout);

        UiManager.i.UiWipeStack(); UiManager.i.UiPush(_LevelProceedMenu);

    }
}
