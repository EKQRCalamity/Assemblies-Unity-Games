using System;
using System.Collections;
using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;
using Gameplay.GameControllers.Bosses.BejeweledSaint.Audio;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint;

public class BejeweledSaintBoss : MonoBehaviour
{
	public Core.SimpleEvent OnRaised;

	[SerializeField]
	protected GameObject LeftSweepAttackLimit;

	[SerializeField]
	protected GameObject RightSweepAttackLimit;

	[MinMaxSlider(0f, 5f, false)]
	public Vector2 BossHeightPosition;

	public float MaxTimeCollapsed;

	private Coroutine currentDownfallCoroutine;

	public BejeweledSmashHandManager HandsManager { get; private set; }

	public BsHolderManager HoldersManager { get; private set; }

	public BsDivineBeamManager BeamManager { get; private set; }

	public BejeweledSaintHead Head { get; private set; }

	public BejeweledSaintArmAttack AttackArm { get; private set; }

	public BejeweledSaintCastArm CastArm { get; private set; }

	public BejeweledSaintAudio Audio { get; private set; }

	public bool IsRaised { get; private set; }

	public Vector3 LeftSweepAttackLimitPosition => LeftSweepAttackLimit.transform.position;

	public Vector3 RightSweepAttackLimitPosition => RightSweepAttackLimit.transform.position;

	private void Awake()
	{
		Head = GetComponentInChildren<BejeweledSaintHead>();
		AttackArm = GetComponentInChildren<BejeweledSaintArmAttack>();
		CastArm = GetComponentInChildren<BejeweledSaintCastArm>();
		HandsManager = UnityEngine.Object.FindObjectOfType<BejeweledSmashHandManager>();
		BeamManager = UnityEngine.Object.FindObjectOfType<BsDivineBeamManager>();
		HoldersManager = GetComponentInChildren<BsHolderManager>();
		Audio = GetComponentInChildren<BejeweledSaintAudio>();
	}

	private void Start()
	{
		BsHolderManager holdersManager = HoldersManager;
		holdersManager.OnBossCollapse = (Core.SimpleEvent)Delegate.Combine(holdersManager.OnBossCollapse, new Core.SimpleEvent(OnBossCollapse));
		Head.EnableDamageArea(enableDamageArea: false);
		Head.OnDeath += OnBossDeath;
		IsRaised = true;
	}

	private void OnBossDeath()
	{
		DOTween.Kill(base.transform);
		if (currentDownfallCoroutine != null)
		{
			StopCoroutine(currentDownfallCoroutine);
			currentDownfallCoroutine = null;
		}
		SetSmoothYPos(-20f, 5f, null);
	}

	private void OnBossCollapse()
	{
		IsRaised = false;
		DOTween.Kill(base.transform);
		Head.EnableDamageArea(enableDamageArea: true);
		Audio.PlaySaintFall();
		SetSmoothYPos(BossHeightPosition.x, 1f, DownfallLapse);
	}

	private void OnBossRaised()
	{
		if (OnRaised != null)
		{
			OnRaised();
		}
		IsRaised = true;
		Head.EnableDamageArea(enableDamageArea: false);
		HoldersManager.EnableHoldersDamageArea(enableDamageArea: true);
	}

	private void SetSmoothYPos(float yPos, float time, TweenCallback endCallback)
	{
		base.transform.DOLocalMoveY(yPos, time).SetEase(Ease.Linear).OnComplete(endCallback)
			.SetId("VerticalMotion");
	}

	public void IntroRaise()
	{
		HoldersManager.SetDefaultLocalPositions();
		HoldersManager.HealHolders();
		HoldersManager.SortRealHolder();
		Audio.PlaySaintRise();
		SetSmoothYPos(BossHeightPosition.y, 2.5f, OnBossRaisedIntro);
	}

	public void SetIntroPosition()
	{
		base.transform.localPosition = new Vector3(base.transform.localPosition.x, BossHeightPosition.x, base.transform.localPosition.z);
	}

	private void OnBossRaisedIntro()
	{
		IsRaised = true;
		Head.EnableDamageArea(enableDamageArea: false);
		HoldersManager.EnableHoldersDamageArea(enableDamageArea: true);
	}

	public void DownfallLapse()
	{
		currentDownfallCoroutine = StartCoroutine(DownfallLapseCoroutine());
	}

	private IEnumerator DownfallLapseCoroutine()
	{
		yield return new WaitForSeconds(MaxTimeCollapsed);
		if (!Head.Status.Dead)
		{
			HoldersManager.SetDefaultLocalPositions();
			HoldersManager.HealHolders();
			HoldersManager.SortRealHolder();
			Audio.PlaySaintRise();
			SetSmoothYPos(BossHeightPosition.y, 1f, OnBossRaised);
		}
	}

	private void OnDestroy()
	{
		Head.OnDeath -= OnBossDeath;
	}
}
