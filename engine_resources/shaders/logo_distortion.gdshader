shader_type canvas_item;

uniform float chromaticAbStrength   = 0.0;
uniform float alpha                 = 1.0;

uniform float sineStren             = 1.0;
uniform float time                  = 0.0;

uniform float ECHO_CNT              = 2.5;

void fragment() {
    vec2 uvMod = vec2(0.0, 0.0);
    
    uvMod.x = sin(time + UV.y * ECHO_CNT) * sineStren;
    
	float r = texture(TEXTURE, UV + uvMod + vec2(SCREEN_PIXEL_SIZE.x * chromaticAbStrength, 0.0), 0.0).r;
    float g = texture(TEXTURE, UV + uvMod, 0.0).g;
    float a = texture(TEXTURE, UV + uvMod, 0.0).a * alpha;
    float b = texture(TEXTURE, UV + uvMod + vec2(SCREEN_PIXEL_SIZE.x * -chromaticAbStrength, 0.0), 0.0).b;
    
    COLOR = vec4(r, g, b, a);
}