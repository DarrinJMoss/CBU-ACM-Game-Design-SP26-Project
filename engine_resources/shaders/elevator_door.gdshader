shader_type canvas_item;

uniform float offset = 0.0;

void fragment() {
	COLOR = texture(TEXTURE, UV + vec2(offset, 0.0f));
}
