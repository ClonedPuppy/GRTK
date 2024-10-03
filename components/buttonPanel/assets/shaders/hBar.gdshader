shader_type spatial;

uniform float fill_amount : hint_range(0, 1) = 0;

void fragment() {
    vec2 uv = UV;
    
    // Green color for the filled part
    vec3 fillColor = vec3(0.0, 1.0, 0.0);
    
    // Background color (e.g., gray)
    vec3 bgColor = vec3(0.5, 0.5, 0.5);
    
    // Determine color based on fill amount
    vec3 finalColor = (uv.y < fill_amount) ? fillColor : bgColor;
    
    ALBEDO =  finalColor;
}