// Inspired and heavyily copied code from this excellent post:
// https://bgolus.medium.com/the-best-darn-grid-shader-yet-727f9278b9d8

shader_type spatial;

render_mode blend_mix;

uniform vec2 camera_uv;

void vertex() {
	// Called for every vertex the material is visible on.
}

float pristineGrid(vec2 uv, vec2 lineWidth) {
    vec2 ddx = dFdx(uv);
    vec2 ddy = dFdy(uv);
    vec2 uvDeriv = vec2(length(vec2(ddx.x, ddy.x)), length(vec2(ddx.y, ddy.y)));
    bvec2 invertLine = bvec2(lineWidth.x > 0.5, lineWidth.y > 0.5);
    vec2 targetWidth = vec2(
        invertLine.x ? .0 - lineWidth.x : lineWidth.x,
        invertLine.y ? 1.0 - lineWidth.y : lineWidth.y
    );
    vec2 drawWidth = clamp(targetWidth, uvDeriv, vec2(0.5));
    vec2 lineAA = uvDeriv * 1.5;
    vec2 gridUV = abs(fract(uv) * 2.0 - 1.0);
    gridUV.x = invertLine.x ? gridUV.x : 1.0 - gridUV.x;
    gridUV.y = invertLine.y ? gridUV.y : 1.0 - gridUV.y;
    vec2 grid2 = smoothstep(drawWidth + lineAA, drawWidth - lineAA, gridUV);

    grid2 *= clamp(targetWidth / drawWidth, 0.0, 1.0);
    grid2 = mix(grid2, targetWidth, clamp(uvDeriv * 2.0 - 1.0, 0.0, 1.0));
    grid2.x = invertLine.x ? 1.0 - grid2.x : grid2.x;
    grid2.y = invertLine.y ? 1.0 - grid2.y : grid2.y;
    return mix(grid2.x, 1.0, grid2.y);
}

void fragment() {
	// Adjust this to control the number of grid lines
	float scaleFactor = 500.0; 
    vec2 scaledUV = UV * scaleFactor;

    // Adjust line width as needed
    vec2 lineWidth = vec2(0.025, 0.025); 

    // Generate the grid pattern
    float gridValue = pristineGrid(scaledUV, lineWidth);

    // Calculate distance falloff
	float centerDistance = length(UV - camera_uv);
    float falloffStart = 0.0; // Start of the falloff
    float falloffEnd = 0.5; // End of the falloff, after which the grid is completely faded out
    float falloff = 0.25 - smoothstep(falloffStart, falloffEnd, centerDistance);
    falloff = clamp(falloff, 0.0, 1.0); // Ensure falloff doesn't go below 0

    // Apply the distance falloff to the grid value
    float finalGridValue = gridValue * falloff;

    ALBEDO = vec3(finalGridValue);
    ALPHA = gridValue * falloff;
}
