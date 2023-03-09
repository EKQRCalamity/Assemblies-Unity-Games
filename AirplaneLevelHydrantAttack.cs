using UnityEngine;

public class AirplaneLevelHydrantAttack : MonoBehaviour
{
	private const float SPEED_MODIFIER = 1.5f;

	[SerializeField]
	private Transform spawnPos;

	private float speed = 800f;

	private void AniEvent_SpawnHydrant(AnimationEvent ev)
	{
		((GameObject)ev.objectReferenceParameter).GetComponent<BasicProjectile>().Create(spawnPos.position, ev.floatParameter, (float)ev.intParameter * 1.5f);
	}

	private void SFX_DOGFIGHT_Leader_CopterBG()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_leader_copterbackground");
	}

	private void SFX_DOGFIGHT_Leader_CopterBGCannonFire()
	{
		AudioManager.Play("sfx_dlc_dogfight_p1_leader_copterbackground_canon");
	}
}
