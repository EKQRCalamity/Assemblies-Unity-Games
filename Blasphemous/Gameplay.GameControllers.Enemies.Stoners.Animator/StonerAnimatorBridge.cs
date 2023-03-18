using UnityEngine;

namespace Gameplay.GameControllers.Enemies.Stoners.Animator;

public class StonerAnimatorBridge : MonoBehaviour
{
	private Stoners _stoners;

	public GameObject StonersGrave;

	private void Awake()
	{
		_stoners = GetComponentInParent<Stoners>();
	}

	public void SetBullsEyePos()
	{
		_stoners.Attack.SetBullsEyeWhenThrow();
	}

	public void ThrowRock()
	{
		_stoners.Attack.ThrowRock();
	}

	public void PlayStonersRaise()
	{
		if (_stoners.Status.IsVisibleOnCamera)
		{
			_stoners.Audio.Raise();
		}
	}

	public void PlayThrowRock()
	{
		_stoners.Audio.ThrowRock();
	}

	public void PlayPassRock()
	{
		_stoners.Audio.PassRock();
	}

	public void PlayDamage()
	{
		_stoners.Audio.Damage();
	}

	public void PlayDeath()
	{
		_stoners.Audio.Death();
	}

	public void InstantiateStonersGrave()
	{
		if (!(StonersGrave == null))
		{
			Object.Instantiate(StonersGrave, _stoners.Animator.transform.position, Quaternion.identity);
			Object.Destroy(_stoners.gameObject);
		}
	}
}
