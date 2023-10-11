using System;

namespace AmplifyImpostors;

[Flags]
public enum DeferredBuffers
{
	AlbedoAlpha = 1,
	SpecularSmoothness = 2,
	NormalDepth = 4,
	EmissionOcclusion = 8
}
