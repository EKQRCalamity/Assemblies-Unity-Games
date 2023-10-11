using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(ParticleSystem))]
public class RigidBodyParticleForce : MonoBehaviour
{
	private Rigidbody _body;

	private ParticleSystem _particleSystem;

	private ParticleSystem.Particle[] _particles;

	public Rigidbody body
	{
		get
		{
			if (!_body)
			{
				return _body = GetComponent<Rigidbody>();
			}
			return _body;
		}
	}

	public ParticleSystem system
	{
		get
		{
			if (!_particleSystem)
			{
				return _particleSystem = GetComponent<ParticleSystem>();
			}
			return _particleSystem;
		}
	}

	private void Awake()
	{
		ParticleSystem.MainModule main = system.main;
		ParticleSystem.EmissionModule emission = system.emission;
		ParticleSystem.ExternalForcesModule externalForces = system.externalForces;
		main.startLifetime = float.PositiveInfinity;
		main.startSpeed = 0f;
		main.simulationSpace = ParticleSystemSimulationSpace.World;
		main.maxParticles = 1;
		emission.rateOverTime = 0f;
		emission.rateOverDistance = 0f;
		externalForces.enabled = true;
		GetComponent<ParticleSystemRenderer>().enabled = false;
		system.Emit(1);
		_particles = new ParticleSystem.Particle[1];
		system.GetParticles(_particles);
		_particles[0].position = body.position;
		system.SetParticles(_particles, 1);
	}

	private void FixedUpdate()
	{
		system.GetParticles(_particles);
		body.velocity += _particles[0].velocity;
		_particles[0].position = body.position;
		_particles[0].velocity = Vector3.zero;
		system.SetParticles(_particles, 1);
	}
}
