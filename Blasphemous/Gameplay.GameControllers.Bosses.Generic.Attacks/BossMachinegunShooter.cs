using System.Collections;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.CommonAttacks;
using Gameplay.GameControllers.Enemies.Projectiles;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.Generic.Attacks;

public class BossMachinegunShooter : MonoBehaviour
{
	public float maxAnimSpeed = 3f;

	public float minAnimSpeed = 1f;

	public float maxDuration = 7f;

	public AnimationCurve fireRateChangeCurve;

	public BossStraightProjectileAttack projectileAttack;

	public GameObject muzzleFlashEffect;

	public Animator machinegunAnimator;

	private float _currentDisp;

	private Transform _currentTarget;

	public bool useBounceBackData;

	public Transform bouncebackOwner;

	public Vector2 bouncebackOffset;

	public float dispersionAtMaxSpeed = 1f;

	public bool useVerticalDispersion;

	private void Start()
	{
		PoolManager.Instance.CreatePool(muzzleFlashEffect, projectileAttack.poolSize);
	}

	public void StartAttack(Transform target)
	{
		_currentTarget = target;
		StartCoroutine(MachinegunCoroutine());
	}

	private IEnumerator MachinegunCoroutine()
	{
		float durationCounter = 0f;
		machinegunAnimator.SetBool("FIRE", value: true);
		while (durationCounter < maxDuration)
		{
			float nValue = fireRateChangeCurve.Evaluate(durationCounter / maxDuration);
			_currentDisp = dispersionAtMaxSpeed * nValue;
			machinegunAnimator.speed = Mathf.Lerp(minAnimSpeed, maxAnimSpeed, nValue);
			durationCounter += Time.deltaTime;
			yield return null;
		}
		machinegunAnimator.SetBool("FIRE", value: false);
	}

	public void StopMachinegun()
	{
		machinegunAnimator.SetBool("FIRE", value: false);
		StopAllCoroutines();
	}

	public void OnMachinegunRevolution()
	{
		if (machinegunAnimator.GetBool("FIRE"))
		{
			Shoot(_currentDisp, useVerticalDispersion);
		}
	}

	private void Shoot(float dispersion, bool verticalDispersion = false)
	{
		Vector2 vector = (Vector2)_currentTarget.transform.position + Vector2.up * 1.4f;
		if (verticalDispersion)
		{
			vector += Vector2.up * Random.Range(-1f, 1f) * dispersion;
		}
		else
		{
			vector += new Vector2(Random.Range(-1f, 1f), 0f) * dispersion;
		}
		Vector2 dir = vector - (Vector2)base.transform.position;
		StraightProjectile straightProjectile = projectileAttack.Shoot(dir, (Vector3)dir.normalized);
		AcceleratedProjectile component = straightProjectile.GetComponent<AcceleratedProjectile>();
		component.SetAcceleration(dir.normalized * 10f);
		if (useBounceBackData && bouncebackOwner != null)
		{
			component.SetBouncebackData(bouncebackOwner, bouncebackOffset);
		}
		PoolManager.Instance.ReuseObject(muzzleFlashEffect, base.transform.position, Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * 57.29578f));
	}
}
