#version 430 core

out vec4 FragColor;

uniform sampler2D color;

#define SAMPLE(x,y) outer = max(outer, texelFetch(color, coords + ivec2(x, y), 0).a);
#define SAMPLEF(f,x,y) outer = max(outer, f* texelFetch(color, coords + ivec2(x, y), 0).a);

#define SAMPLE1(x) SAMPLE(x, x); SAMPLE(-x, x); SAMPLE(x, -x); SAMPLE(-x, -x);
#define SAMPLE1F(f, x) SAMPLEF(f, x, x); SAMPLEF(f, -x, x); SAMPLEF(f, x, -x); SAMPLEF(f, -x, -x);

#define SAMPLE2(x,y) \
	SAMPLE(x, y); SAMPLE(-x, y); SAMPLE(x, -y); SAMPLE(-x, -y);\
	SAMPLE(y, x); SAMPLE(-y, x); SAMPLE(y, -x); SAMPLE(-y, -x);

#define SAMPLE2F(f, x,y) \
	SAMPLEF(f, x, y); SAMPLEF(f, -x, y); SAMPLEF(f, x, -y); SAMPLEF(f, -x, -y); \
	SAMPLEF(f, y, x); SAMPLEF(f, -y, x); SAMPLEF(f, y, -x); SAMPLEF(f, -y, -x);

void main() 
{
	ivec2 coords = ivec2(gl_FragCoord.xy);
	float inner = texelFetch(color, coords, 0).a;

	if(inner > 0)
	{
		FragColor = mix(vec4(0,0,0,1), vec4(1), inner);
		return;
	}
	float outer = 0;

	SAMPLE(0, -1); SAMPLE(0, -2); SAMPLE(0, -3); SAMPLE(0, -4);
	SAMPLE(0, 1);  SAMPLE(0, 2);  SAMPLE(0, 3);  SAMPLE(0, 4);

	SAMPLE(-1, 0); SAMPLE(-2, 0); SAMPLE(-3, 0); SAMPLE(-4, 0);
	SAMPLE(1, 0); SAMPLE(2, 0); SAMPLE(3, 0); SAMPLE(4, 0);

	SAMPLE1(1);    SAMPLE2(1, 2); SAMPLE2(1, 3);
	SAMPLE2(2, 1); SAMPLE1(2);    SAMPLE2(2, 3);
	SAMPLE2(3, 1); SAMPLE2(3, 2); SAMPLE1F(0.75f, 3);

	SAMPLE2(1, 4);
	SAMPLE2F(0.87f, 2, 4);
	SAMPLE2F(0.5f, 3, 4);


	if(outer == 0)
		discard;

	FragColor = mix(vec4(0.0f), vec4(0.0f, 0.0f, 0.0f, 1.0f), outer);
}