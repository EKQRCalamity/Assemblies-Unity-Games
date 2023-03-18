using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemsController : MonoBehaviour
{
	private ParticleSystem rootSystem;

	private List<ParticleSystem> childrenSystems;

	public void CopyParentScaleToChildren()
	{
		if (childrenSystems == null || childrenSystems.Count == 0)
		{
			childrenSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
		}
		foreach (ParticleSystem childrenSystem in childrenSystems)
		{
			childrenSystem.transform.localScale = base.transform.localScale;
		}
	}

	public void PlayParticles()
	{
		if (rootSystem == null)
		{
			rootSystem = GetComponent<ParticleSystem>();
		}
		rootSystem.Play();
	}

	public void PlayParticlesInChildren()
	{
		if (childrenSystems == null || childrenSystems.Count == 0)
		{
			childrenSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
		}
		foreach (ParticleSystem childrenSystem in childrenSystems)
		{
			childrenSystem.Play();
		}
	}

	public void StopParticles()
	{
		if (rootSystem == null)
		{
			rootSystem = GetComponent<ParticleSystem>();
		}
		rootSystem.Stop();
	}

	public void StopParticlesInChildren()
	{
		if (childrenSystems == null || childrenSystems.Count == 0)
		{
			childrenSystems = new List<ParticleSystem>(GetComponentsInChildren<ParticleSystem>());
		}
		foreach (ParticleSystem childrenSystem in childrenSystems)
		{
			childrenSystem.Stop();
		}
	}
}
