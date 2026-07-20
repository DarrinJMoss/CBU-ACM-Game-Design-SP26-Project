using Godot;
using System;

public partial class PauseMenu : CanvasLayer
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

    private float               _pauseMenu_openPercent              = 0.0f;


    private RichTextLabel       PauseHeaderTxt                      = null;
    const   float               PAUSE_HEADER_TXT_LERP_WEIGHT        = 10.0f;
    const   float               PAUSE_HEADER_TXT_SHIFT_AMOUNT       = -96.0f;
    private Vector2             _pauseHeaderTxt_posBase             = new Vector2(72.0f, 129.0f);
    private Vector2             _pauseHeaderTxt_posOffset           = Vector2.Zero;
    private Vector2             _pauseHeaderTxt_posOffset_Target    = Vector2.Zero;


    private Control             PauseRoot                           = null;
    const float                 PAUSE_LERP_WEIGHT                   = 15.0f;
    const float                 PAUSE_ROOT_INIT_OFFSET_X            = -64.0f;
    private Vector2             _pauseRoot_posOffset                = Vector2.Zero;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        ShaderRect      = GetNode<ColorRect>("%CrtShaderRect");
        PauseRoot       = GetNode<Control>("%PauseRoot");
        PauseHeaderTxt  = GetNode<RichTextLabel>("%PauseHeaderTxt");
        PauseRoot.Hide();
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.IsActionJustPressed("Pause")) 
        {
            if (GetTree().Paused)
            {
                // CLOSE MENU
                GetTree().Paused = false;
                PauseRoot.Hide();
            }
            else
            {
                // OPEN MENU
                GetTree().Paused = true;
                PauseRoot.Show();
                _pauseHeaderTxt_posOffset = new Vector2(PAUSE_ROOT_INIT_OFFSET_X, 0.0f);
                _pauseHeaderTxt_posOffset_Target = Vector2.Zero;
            }
        }
    }


	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        #region Shader Effect Manager
        if (GetTree().Paused)
        {   
            _pauseMenu_openPercent = Mathf.Lerp(_pauseMenu_openPercent, 1.0f, OPENCLOSE_SPEED * Global.i.GetClampedDelta_PR());
        }
        else
        {
            _pauseMenu_openPercent = Mathf.Lerp(_pauseMenu_openPercent, 0.0f, OPENCLOSE_SPEED * Global.i.GetClampedDelta_PR());
        }

        if (ShaderRect.Material is ShaderMaterial shader)
        {
            shader.SetShaderParameter("alpha",              Mathf.Lerp(0.0f,                        1.0f,                       _pauseMenu_openPercent * 8.5f));
            ShaderRect.Show();

            shader.SetShaderParameter("curvature",          Mathf.Lerp(SHADER_CURAVUTRE_STR,        SHADER_CURVATURE_END,       _pauseMenu_openPercent));
            shader.SetShaderParameter("corner_soften",      Mathf.Lerp(SHADER_CURAVUTRE_SOFT_STR,   SHADER_CURVATURE_SOFT_END,  _pauseMenu_openPercent));
            shader.SetShaderParameter("vignette",           Mathf.Lerp(SHADER_VIGNETTE_STR,         SHADER_VIGNETTE_END,        _pauseMenu_openPercent));
            shader.SetShaderParameter("brightness",         Mathf.Lerp(SHADER_BRIGHTNESS_STR,       SHADER_BRIGHTNESS_END,      _pauseMenu_openPercent));
            shader.SetShaderParameter("contrast",           Mathf.Lerp(SHADER_CONTRAST_STR,         SHADER_CONTRAST_END,        _pauseMenu_openPercent));
            shader.SetShaderParameter("chroma_offset_px",   Mathf.Lerp(SHADER_CHROMA_STR,           SHADER_CHROMA_END,          _pauseMenu_openPercent));
            shader.SetShaderParameter("scanline_strength",  Mathf.Lerp(SHADER_SCANLINE_STR,         SHADER_SCANLINE_END,        _pauseMenu_openPercent));
        }
        #endregion


        #region Pause Menu Header Manager

        PauseHeaderTxt.Position = _pauseHeaderTxt_posBase + _pauseHeaderTxt_posOffset;

        _pauseHeaderTxt_posOffset_Target = Vector2.Left * PAUSE_HEADER_TXT_SHIFT_AMOUNT * Mathf.Max(UiManager.i.GetStackSize() - 1, 0);

        _pauseHeaderTxt_posOffset = _pauseHeaderTxt_posOffset.Lerp(_pauseHeaderTxt_posOffset_Target, PAUSE_HEADER_TXT_LERP_WEIGHT * Global.i.GetClampedDelta_PR());

        #endregion
    }   
}
