using System;
using System.Collections;
using System.Collections.Generic;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using DG.Tweening.Core.Surrogates;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Enemies.Framework.Damage;
using Gameplay.GameControllers.Enemies.Framework.IA;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Gizmos;
using Sirenix.OdinInspector;
using Tools.Level.Layout;
using UnityEngine;

namespace Tools.Level.Interactables;

public class Execution : Interactable, IDamageable
{
	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	private string activationSound;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	private float soundDelay;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	protected string ZoomInFx;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	[EventRef]
	protected string ZoomOutFx;

	[SerializeField]
	[BoxGroup("Audio Settings", true, false, 0)]
	public bool PlayOnUse;

	[SerializeField]
	[BoxGroup("Visual Effect", true, false, 0)]
	public GameObject ExecutionAwareness;

	[SerializeField]
	[BoxGroup("Allowed floor", true, false, 0)]
	public LayerMask AllowedFloor;

	public const float OverlappedInteractorDistanceOffset = 2.5f;

	private Vector2 _interactorStartPosition;

	private RootMotionDriver _rootMotion;

	public EnemyDamageArea DamageArea;

	private CameraPlayerOffset _cameraPlayerOffset;

	private Vector3 _currentCameraPosition;

	public float SafeTimeThreshold = 0.5f;

	private float _currentTimeThreshold;

	private ProCamera2DPanAndZoom _cameraZoom;

	private float _maxZoomInput = 1f;

	private float _zoomTimeLapse = 0.35f;

	private float _slowMotionTimeLapse = 0.5f;

	private float attackCoolDown;

	private const float AttackCooldown = 0.5f;

	public Core.SimpleEvent OnSlowMotion;

	public Core.SimpleEvent OnSlowMotionStart;

	public Core.SimpleEvent OnNormalTime;

	private LevelInitializer _currentLevel;

	[TutorialId]
	public string TutorialID;

	private const int TOTAL_NUMBER_OF_DIFFERENT_EXECUTIONS_FOR_AC27 = 5;

	private List<Interactable> overlappedInteractables = new List<Interactable>();

	public string ActivationSound => activationSound;

	public Enemy ExecutedEntity { get; set; }

	public Penitent Penitent { get; private set; }

	private bool PlayerIsOnSameFloor
	{
		get
		{
			RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, -Vector2.up, 3f, AllowedFloor);
			RaycastHit2D raycastHit2D2 = Physics2D.Raycast(Penitent.GetPosition(), -Vector2.up, 1f, AllowedFloor);
			if (raycastHit2D.collider == null || raycastHit2D2.collider == null)
			{
				return false;
			}
			return Mathf.Abs(raycastHit2D.collider.bounds.max.y - raycastHit2D2.collider.bounds.max.y) < 0.1f;
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		_rootMotion = GetComponentInChildren<RootMotionDriver>();
		DamageArea = GetComponent<EnemyDamageArea>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_cameraZoom = Core.Logic.CameraManager.PanAndZoom;
		_currentLevel = Core.Logic.CurrentLevelConfig;
		_cameraPlayerOffset = Core.Logic.CameraManager.CameraPlayerOffset;
		Penitent = Core.Logic.Penitent;
		DamageArea.SetOwner(ExecutedEntity);
		SetOverlappedInteractables();
		Invoke("TriggerAwareness", 0.2f);
		Invoke("ShowTutorial", 0.6f);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		ReleaseOverlappedInteractables();
	}

	public override bool AllwaysShowIcon()
	{
		return false;
	}

	protected override IEnumerator OnUse()
	{
		WaitForEndOfFrame lapse = new WaitForEndOfFrame();
		Penitent.PlatformCharacterController.PlatformCharacterPhysics.HSpeed = 0f;
		Penitent.PlatformCharacterController.InstantVelocity = Vector3.zero;
		Core.Input.SetBlocker("EXECUTION", blocking: true);
		Penitent interactor = Penitent;
		_interactorStartPosition = new Vector2(interactor.transform.position.x, interactor.transform.position.y);
		FlipAnimation(base.PlayerDirection);
		interactableAnimator.gameObject.SetActive(value: false);
		ShowPlayer(show: false);
		ExecutedEntity.KillEntity();
		if (PlayOnUse)
		{
			Core.Audio.PlaySfx(activationSound, soundDelay);
		}
		interactorAnimator.SetTrigger("USED");
		while (!base.Consumed)
		{
			yield return lapse;
		}
		AddProgressToAC27();
		Penitent.transform.position = GetRootMotionPosition(base.PlayerDirection);
		UnityEngine.Object.Destroy(ExecutedEntity.gameObject);
		UnityEngine.Object.Destroy(base.gameObject);
		ShowPlayer(show: true);
		ReleaseOverlappedInteractables();
		Core.Input.SetBlocker("EXECUTION", blocking: false);
	}

	private void AddProgressToAC27()
	{
		string text = ExecutedEntity.GetComponent<EnemyBehaviour>().GetType().ToString();
		string text2 = text.Split('.')[text.Split('.').Length - 1].Replace("Behaviour", string.Empty).Trim().ToUpper();
		string text3 = text2 + "_EXECUTED_FOR_AC27";
		Debug.Log("AddProgressToAC27: flagName: " + text3);
		Debug.Log("AddProgressToAC27: Core.Events.GetFlag(flagName): " + Core.Events.GetFlag(text3));
		if (!Core.Events.GetFlag(text3))
		{
			Core.Events.SetFlag(text3, b: true, forcePreserve: true);
			Core.AchievementsManager.Achievements["AC27"].AddProgress(20f);
		}
	}

	protected override void OnUpdate()
	{
		_currentTimeThreshold += Time.deltaTime;
		attackCoolDown -= Time.deltaTime;
		if (Penitent.PlatformCharacterInput.Attack)
		{
			attackCoolDown = 0.5f;
		}
		if (base.PlayerInRange && Penitent.Status.IsGrounded && !Penitent.Status.IsHurt && !base.Consumed && base.InteractionTriggered && attackCoolDown <= 0f && PlayerIsOnSameFloor)
		{
			Penitent.IsOnExecution = true;
			GhostTrailGenerator.AreGhostTrailsAllowed = false;
			Use();
		}
		if (base.BeingUsed)
		{
			PlayerReposition();
		}
	}

	private void ShowTutorial()
	{
		Core.Logic.Penitent.ShowTutorial(TutorialID);
	}

	protected override void PlayerReposition()
	{
		Penitent.transform.position = GetRootMotionPosition(base.PlayerDirection);
	}

	protected override void ShowPlayer(bool show)
	{
		base.ShowPlayer(show);
		Penitent.Physics.EnablePhysics(show);
	}

	protected override void InteractionEnd()
	{
		base.Consumed = true;
		AddFervourBonus();
		Core.Logic.Penitent.IsOnExecution = false;
		GhostTrailGenerator.AreGhostTrailsAllowed = true;
	}

	private void AddFervourBonus()
	{
		float num = 13.3f * Core.Logic.Penitent.Stats.FervourStrength.Final;
		Core.Logic.Penitent.Stats.Fervour.Current += num;
	}

	public Vector2 GetRootMotionPosition(EntityOrientation playerDirection)
	{
		Vector3 vector = base.transform.TransformPoint(_rootMotion.transform.localPosition);
		if (playerDirection == EntityOrientation.Left)
		{
			return new Vector2(vector.x, _interactorStartPosition.y);
		}
		float num = Math.Abs(base.transform.position.x - vector.x);
		return new Vector2(((Vector3)new Vector2(base.transform.position.x + num, _interactorStartPosition.y)).x, _interactorStartPosition.y);
	}

	private void FlipAnimation(EntityOrientation direction)
	{
		if (direction == EntityOrientation.Left)
		{
			interactorAnimator.transform.position = sensors[0].transform.position;
			SpriteRenderer[] componentsInChildren = interactorAnimator.GetComponentsInChildren<SpriteRenderer>();
			SpriteRenderer[] array = componentsInChildren;
			foreach (SpriteRenderer spriteRenderer in array)
			{
				spriteRenderer.flipX = false;
			}
		}
		else
		{
			interactorAnimator.transform.position = sensors[1].transform.position;
			SpriteRenderer[] componentsInChildren2 = interactorAnimator.GetComponentsInChildren<SpriteRenderer>();
			SpriteRenderer[] array2 = componentsInChildren2;
			foreach (SpriteRenderer spriteRenderer2 in array2)
			{
				spriteRenderer2.flipX = true;
			}
		}
	}

	public void Damage(Hit hit)
	{
		if (!(_currentTimeThreshold < SafeTimeThreshold) && !Penitent.IsOnExecution)
		{
			_currentTimeThreshold = 0f;
			ExecutedEntity.Animator.gameObject.SetActive(value: true);
			ExecutedEntity.Animator.enabled = true;
			ExecutedEntity.SpriteRenderer.enabled = true;
			ExecutedEntity.Animator.Play("Death");
			Core.Audio.EventOneShotPanned(hit.HitSoundId, base.transform.position);
			ExecutedEntity.KillEntity();
			ReleaseOverlappedInteractables();
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public Vector3 GetPosition()
	{
		return base.transform.position;
	}

	private void ReleaseOverlappedInteractables()
	{
		foreach (Interactable overlappedInteractable in overlappedInteractables)
		{
			overlappedInteractable.OverlappedInteractor = false;
		}
		overlappedInteractables.Clear();
	}

	private void SetOverlappedInteractables()
	{
		float num = Vector2.Distance(base.transform.position, Core.Logic.Penitent.GetPosition());
		Interactable[] array = UnityEngine.Object.FindObjectsOfType<Interactable>();
		foreach (Interactable interactable in array)
		{
			if (interactable.Equals(this) || !(Vector2.Distance(interactable.transform.position, base.transform.position) < 2.5f))
			{
				continue;
			}
			if (interactable as Execution != null)
			{
				float num2 = Vector2.Distance(interactable.transform.position, Core.Logic.Penitent.GetPosition());
				if (!(num2 < num))
				{
					interactable.OverlappedInteractor = true;
					overlappedInteractables.Add(interactable);
				}
			}
			else
			{
				interactable.OverlappedInteractor = true;
				overlappedInteractables.Add(interactable);
			}
		}
	}

	private void TriggerAwareness()
	{
		if (ExecutionAwareness != null)
		{
			Vector2 vector = new Vector2(ExecutedEntity.transform.position.x, Core.Logic.Penitent.transform.position.y);
			UnityEngine.Object.Instantiate(ExecutionAwareness, vector, Quaternion.identity);
		}
	}

	public void DoSlowmotion()
	{
		DOTween.To(() => _currentLevel.TimeScale, delegate(float x)
		{
			_currentLevel.TimeScale = x;
		}, 0.25f, _slowMotionTimeLapse).SetUpdate(isIndependentUpdate: true).OnComplete(OnCameraZoomIn)
			.SetEase(Ease.OutSine)
			.OnStart(SlowMotionStart)
			.OnComplete(SlowMotionCallBack);
	}

	public void StopSlowMotion()
	{
		DOTween.To(() => _currentLevel.TimeScale, delegate(float x)
		{
			_currentLevel.TimeScale = x;
		}, 1f, _slowMotionTimeLapse).SetUpdate(isIndependentUpdate: true).OnComplete(OnCameraZoomIn)
			.SetEase(Ease.OutSine)
			.OnComplete(NormalTimeCallback);
	}

	private void SlowMotionStart()
	{
		if (OnSlowMotionStart != null)
		{
			OnSlowMotionStart();
		}
	}

	private void SlowMotionCallBack()
	{
		if (OnSlowMotion != null)
		{
			OnSlowMotion();
		}
	}

	private void NormalTimeCallback()
	{
		if (OnNormalTime != null)
		{
			OnNormalTime();
		}
	}

	public void CameraZoomIn()
	{
		if (!(_cameraZoom == null))
		{
			Core.Logic.CameraManager.ZoomActive = true;
			if (!_cameraZoom.AllowZoom)
			{
				_cameraZoom.AllowZoom = true;
			}
			CenterCamera();
			DOTween.To(() => _cameraZoom.ZoomInput, delegate(float x)
			{
				_cameraZoom.ZoomInput = x;
			}, 0f - _maxZoomInput, _zoomTimeLapse).OnStart(PlayZoomIn).SetUpdate(isIndependentUpdate: true)
				.OnComplete(OnCameraZoomIn)
				.SetEase(Ease.OutSine);
		}
	}

	private void OnCameraZoomIn()
	{
		Core.Logic.CameraManager.ProCamera2D.OverallOffset.x = 0f;
		_cameraPlayerOffset.RestoredXOffset = false;
	}

	public void CameraZoomOut()
	{
		if (!(_cameraZoom == null))
		{
			DOTween.To(() => _cameraZoom.ZoomInput, delegate(float x)
			{
				_cameraZoom.ZoomInput = x;
			}, _maxZoomInput, _zoomTimeLapse).SetUpdate(isIndependentUpdate: true).OnComplete(OnCameraZoomOut)
				.SetEase(Ease.OutSine);
		}
	}

	private void OnCameraZoomOut()
	{
		Core.Logic.CameraManager.ZoomActive = false;
		_cameraZoom.CancelZoom();
		CameraFollowTarget(follow: true);
	}

	public void CenterCamera()
	{
		if (!(_cameraPlayerOffset == null))
		{
			ProCamera2D proCamera = Core.Logic.CameraManager.ProCamera2D;
			_currentCameraPosition = new Vector3(proCamera.transform.position.x, proCamera.transform.position.y, proCamera.transform.position.z);
			Vector3 endValue = new Vector3(Core.Logic.Penitent.transform.position.x, _currentCameraPosition.y, _currentCameraPosition.z);
			DOTween.To(() => proCamera.transform.position, delegate(Vector3Wrapper x)
			{
				proCamera.transform.position = x;
			}, endValue, _zoomTimeLapse).SetUpdate(isIndependentUpdate: true).OnStart(delegate
			{
				CameraFollowTarget(follow: false);
			})
				.SetEase(Ease.InSine);
		}
	}

	private void CameraFollowTarget(bool follow)
	{
		Core.Logic.CameraManager.ProCamera2D.FollowHorizontal = follow;
		Core.Logic.CameraManager.ProCamera2D.FollowVertical = follow;
		if (follow)
		{
			Core.Logic.CameraManager.ProCamera2D.HorizontalFollowSmoothness = 0.1f;
			Core.Logic.CameraManager.ProCamera2D.VerticalFollowSmoothness = 0.1f;
		}
	}

	public void RestoreCameraOffset()
	{
		if (!(_cameraPlayerOffset == null))
		{
			ProCamera2D proCamera = Core.Logic.CameraManager.ProCamera2D;
			DOTween.To(() => proCamera.transform.position, delegate(Vector3Wrapper x)
			{
				proCamera.transform.position = x;
			}, _currentCameraPosition, 1f).SetUpdate(isIndependentUpdate: true).SetEase(Ease.InSine);
		}
	}

	public void PlayZoomIn()
	{
		Core.Audio.PlaySfx(ZoomInFx);
	}

	public void PlayZoomOut()
	{
		Core.Audio.PlaySfx(ZoomOutFx);
	}

	public bool BleedOnImpact()
	{
		return false;
	}

	public bool SparkOnImpact()
	{
		return false;
	}
}
