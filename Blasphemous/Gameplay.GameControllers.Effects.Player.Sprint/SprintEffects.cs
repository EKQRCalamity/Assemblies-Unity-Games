using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Sprint;

public class SprintEffects : MonoBehaviour
{
	public ParticleSystem feetParticle;

	public ParticleSystem startParticles;

	public ParticleSystem constantParticles;

	public GameObject sprintStartPrefab;

	public GameObject sprintFootstepPrefab;

	private void Awake()
	{
		PoolManager.Instance.CreatePool(sprintStartPrefab, 4);
		PoolManager.Instance.CreatePool(sprintFootstepPrefab, 10);
	}

	public void EmitFeet(Vector2 position)
	{
		PoolManager.Instance.ReuseObject(sprintFootstepPrefab, position, Quaternion.identity);
	}

	private void SetScaleFromOrientation()
	{
		Vector3 one = Vector3.one;
		Vector3 vector = new Vector3(-1f, 1f, 1f);
		feetParticle.transform.localScale = ((Core.Logic.Penitent.Status.Orientation != 0) ? vector : one);
	}

	public void EmitOnStart()
	{
		PoolManager.Instance.ReuseObject(sprintStartPrefab, startParticles.transform.position, Quaternion.identity);
	}
}
