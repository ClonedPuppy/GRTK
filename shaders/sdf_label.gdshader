shader_type spatial;

uniform sampler2D sdf_atlas: repeat_disable;
uniform int string_length;
uniform float char_advances[15];
uniform vec2 atlas_uvs[30];
uniform vec2 plane_uvs[30];
uniform float max_string_width;

void fragment() {
    float total_advance = 0.0;
    float alpha = 0.0;
    float y_offset = 0.5;
    int current_char = 0;
    float sdf_value = 0.0;
    vec2 atlas_glyph_uv = vec2(0, 0);

    // Calculate the total width of the string in UV space
    for (int i = 0; i < string_length; i++) {
        total_advance += char_advances[i] * 0.1666;
    }

    float center_offset_x = (1.0 - ((total_advance) / 1.0)) * 0.5;

    float current_advance = 0.0; // Tracks the current position along the X axis


    // Iterate over each character in the string to find and fill its corresponding area.
    for (int i = 0; i < string_length; i++) {
        // Advance scale factor for current character
        float advance_scale = atlas_uvs[current_char + 1].x / char_advances[i];

        // Calculate the position of the text label
        float position_uv_x_start = center_offset_x + current_advance;
        float position_uv_x_end   = position_uv_x_start + char_advances[i] * advance_scale;
        float position_uv_y_start = y_offset - atlas_uvs[current_char + 1].y/2.0;
        float position_uv_y_end   = y_offset + atlas_uvs[current_char + 1].y / 2.0;

        // Calculate the glyphs position on the atlas texture
        float atlas_UV_x = atlas_uvs[current_char].x;
        float atlas_UV_y = atlas_uvs[current_char].y;

        if (UV.x >= position_uv_x_start && UV.x <= position_uv_x_end) {
            if (UV.y >= position_uv_y_start && UV.y <= position_uv_y_end) {

                atlas_glyph_uv = vec2(UV.x + atlas_UV_x - position_uv_x_start, 
                                      UV.y + atlas_UV_y - position_uv_y_start);

                } 
        }

        sdf_value = texture(sdf_atlas, atlas_glyph_uv).r;
        alpha = max(alpha, smoothstep(0.45, 0.52, sdf_value));

        // Update indices and total advance for the next character.
        current_advance += char_advances[i] * advance_scale;
        current_char += 2;
    }

    // Draw the cross after processing all characters
    float crossThickness = 0.002; // Adjust for desired thickness of the cross lines
    float centerLine = 0.5; // Position of the cross' center line in UV space

    // Determine if the current fragment is within the vertical line of the cross
    bool isVerticalLine = abs(UV.x - centerLine) < crossThickness * 0.5;
    // Determine if the current fragment is within the horizontal line of the cross
    bool isHorizontalLine = abs(UV.y - centerLine) < crossThickness * 0.5;

    // If the fragment is within either line, set alpha to 1.0 to draw the line
    if (isVerticalLine || isHorizontalLine) {
        alpha = 1.0;
    }

    // Set the fragment color to white and apply the calculated alpha.
    ALBEDO = vec3(1.0); // Set to white color.
    ALPHA = alpha; // Apply alpha for transparency control.
}
