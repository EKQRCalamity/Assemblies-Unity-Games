using System.Collections.Generic;
using UnityEngine;

namespace Effects;

public class BloodParticleFixer : MonoBehaviour
{
	public ParticleSystem decalParticleSystem;

	public ParticleSystem dripParticleSystem;

	private ParticleSystem ps;

	public List<ParticleCollisionEvent> collisionEvents;

	private void Awake()
	{
		ps = GetComponent<ParticleSystem>();
		collisionEvents = new List<ParticleCollisionEvent>();
	}

	public void OnParticleCollision(GameObject other)
	{
		int num = ps.GetCollisionEvents(other, collisionEvents);
	}
}
