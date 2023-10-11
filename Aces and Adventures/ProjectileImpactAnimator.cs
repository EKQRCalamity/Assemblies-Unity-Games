using System;
using UnityEngine;

[ScriptOrder(-1)]
public class ProjectileImpactAnimator : MonoBehaviour
{
	private static GameObject _Blueprint;

	[SerializeField]
	protected Transform _aroundTarget;

	[SerializeField]
	protected Transform _projectileParent;

	private float _timeOfCreation;

	private ProjectileFlightSFX _projectile;

	private Vector4[] _randomRangeSamples = new Vector4[7];

	private Vector4 _angularVelocity;

	private float _delay;

	private Quaternion _startingLocalRotation;

	public static GameObject Blueprint
	{
		get
		{
			if (!_Blueprint)
			{
				return _Blueprint = Resources.Load<GameObject>("GameState/Ability/Projectiles/ProjectileImpactAnimator");
			}
			return _Blueprint;
		}
	}

	private void LateUpdate()
	{
		if (!_projectile.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		_projectile.afterImpactParameters.attachment.Update(_projectile, base.transform, _projectile.target, ref _projectile.velocity, ref _angularVelocity);
		if (!_projectile.afterImpactParameters.animation.enabled)
		{
			return;
		}
		ProjectileMediaData.ProjectileAfterImpactParameters.Animation animation = _projectile.afterImpactParameters.animation;
		float normalizedTime = Mathf.Clamp01((Time.time - _timeOfCreation) / Mathf.Max(MathUtil.BigEpsilon, _projectile.lifetimeAfterImpact - _delay));
		if (animation.translation.localTranslation.enabled)
		{
			_projectile.transform.localPosition = animation.translation.localTranslation.GetTranslation(normalizedTime, _randomRangeSamples[0]);
		}
		if (animation.translation.translationAroundTarget.enabled)
		{
			_aroundTarget.localPosition = animation.translation.translationAroundTarget.GetTranslation(normalizedTime, _randomRangeSamples[1]);
		}
		if (animation.translation.worldTranslation.enabled)
		{
			if (!animation.translation.translationAroundTarget.enabled)
			{
				_aroundTarget.localPosition = Vector3.zero;
			}
			_aroundTarget.position += animation.translation.worldTranslation.GetTranslation(normalizedTime, _randomRangeSamples[2]);
		}
		if (animation.rotation.localRotation.enabled)
		{
			_projectile.innerTransform.localRotation = Quaternion.Euler(animation.rotation.localRotation.GetEulerAngles(normalizedTime, _randomRangeSamples[3])) * _startingLocalRotation;
		}
		if (animation.rotation.rotationAroundTarget.enabled)
		{
			_aroundTarget.localRotation = Quaternion.Euler(animation.rotation.rotationAroundTarget.GetEulerAngles(normalizedTime, _randomRangeSamples[4]));
		}
		if (animation.scale.localScale.enabled)
		{
			_projectile.innerTransform.localScale = animation.scale.localScale.GetScale(normalizedTime, _randomRangeSamples[5]);
		}
		if (animation.scale.scaleAroundTarget.enabled)
		{
			_aroundTarget.localScale = animation.scale.scaleAroundTarget.GetScale(normalizedTime, _randomRangeSamples[6]);
		}
	}

	private void OnDisable()
	{
		_projectile = null;
	}

	public ProjectileImpactAnimator SetData(System.Random random, ProjectileFlightSFX projectile, Transform target, float lifetimeOverage)
	{
		_angularVelocity = Vector4.zero;
		_delay = random.Range(projectile.afterImpactParameters.animation.delay);
		_timeOfCreation = Time.time + _delay - lifetimeOverage;
		base.transform.CopyFrom(target, copyPosition: true, copyRotation: true, copyScale: false);
		_aroundTarget.SetLocalToIdentity();
		_projectile = projectile;
		_projectileParent.SetLocalToIdentity();
		_projectileParent.CopyFrom(projectile.transform, copyPosition: true, copyRotation: true, copyScale: false);
		_projectile.transform.SetParent(_projectileParent, worldPositionStays: true);
		for (int i = 0; i < _randomRangeSamples.Length; i++)
		{
			_randomRangeSamples[i] = random.Value4();
		}
		_startingLocalRotation = _projectile.innerTransform.localRotation;
		return this;
	}
}
