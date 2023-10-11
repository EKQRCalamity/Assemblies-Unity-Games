using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
	public int subEmmiterId;

	private ParticleSystem particleSystem;

	private List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

	private ParticleSystem.Particle[] particles;

	private float lifetime;

	public float particleDistanceCheck = 0.5f;

	public Vector3 offset = new Vector3(0f, 0.3f, 0f);

	public bool debug;

	private void Start()
	{
		particleSystem = GetComponent<ParticleSystem>();
		lifetime = particleSystem.main.startLifetime.constant;
		particles = new ParticleSystem.Particle[particleSystem.main.maxParticles];
	}

	private void OnParticleCollision(GameObject other)
	{
		_ = Random.value;
		particleSystem.GetCollisionEvents(other, collisionEvents);
		foreach (ParticleCollisionEvent collisionEvent in collisionEvents)
		{
			if (!(collisionEvent.intersection != Vector3.zero))
			{
				continue;
			}
			int num = particleSystem.GetParticles(particles);
			for (int i = 0; i < num; i++)
			{
				if (particles[i].startLifetime == lifetime && Vector3.Magnitude(base.transform.TransformPoint(particles[i].position) - collisionEvent.intersection) < particleDistanceCheck)
				{
					if (debug)
					{
						Debug.Log("collision particle " + i + " " + particles[i].startLifetime);
					}
					particles[i].startLifetime = particles[i].startLifetime - 0.01f;
					particleSystem.SetParticles(particles);
					particles[i].position += offset;
					particleSystem.TriggerSubEmitter(subEmmiterId, ref particles[i]);
					break;
				}
			}
		}
	}
}
