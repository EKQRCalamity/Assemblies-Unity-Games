using System;

public interface IAttacherParticleSettings
{
	void Apply(Random random, AttacherParticles attacherParticles, float emissionMultiplier);
}
