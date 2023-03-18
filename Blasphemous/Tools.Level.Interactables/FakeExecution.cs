using System;
using System.Collections;
using Com.LuisPedroFonseca.ProCamera2D;
using DG.Tweening;
using DG.Tweening.Core.Surrogates;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Gameplay.GameControllers.Camera;
using Gameplay.GameControllers.Effects.Player.GhostTrail;
using Gameplay.GameControllers.Entities;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Gizmos;
using Sirenix.OdinInspector;
using Tools.Level.Layout;
using UnityEngine;

namespace Tools.Level.Interactables;

public class FakeExecution : Interactable
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
	[BoxGroup("Special Case", true, false, 0)]
	public bool IsCrisanta = true;

	[SerializeField]
	[BoxGroup("Special Case", true, false, 0)]
	public bool InstantiatesSomethingAfterExecution = true;

	[SerializeField]
	[BoxGroup("Special Case", true, false, 0)]
	[ShowIf("InstantiatesSomethingAfterExecution", true)]
	public GameObject PrefabToInstantiateAfterExecution;

	[HideInInspector]
	public EntityOrientation InstanceOrientation;

	private Vector2 _interactorStartPosition;

	private RootMotionDriver _rootMotion;

	private CameraPlayerOffset _cameraPlayerOffset;

	private Vector3 _currentCameraPosition;

	public float SafeTimeThreshold = 0.5f;

	private float _currentTimeThreshold;

	private ProCamera2DPanAndZoom _cameraZoom;

	private float _maxZoomInput = 1f;

	private float _zoomTimeLapse = 0.35f;

	private float _slowMotionTimeLapse = 0.5f;

	public Core.SimpleEvent OnSlowMotion;

	public Core.SimpleEvent OnSlowMotionStart;

	public Core.SimpleEvent OnNormalTime;

	private LevelInitializer _currentLevel;

	public string ActivationSound => activationSound;

	public Penitent Penitent { get; private set; }

	protected override void OnAwake()
	{
		base.OnAwake();
		_rootMotion = GetComponentInChildren<RootMotionDriver>();
	}

	protected override void OnStart()
	{
		base.OnStart();
		_cameraZoom = Core.Logic.CameraManager.PanAndZoom;
		_currentLevel = Core.Logic.CurrentLevelConfig;
		_cameraPlayerOffset = Core.Logic.CameraManager.CameraPlayerOffset;
		Penitent = Core.Logic.Penitent;
		Invoke("TriggerAwareness", 0.2f);
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
		if (PlayOnUse)
		{
			Core.Audio.PlaySfx(activationSound, soundDelay);
		}
		interactorAnimator.SetTrigger("USED");
		if (IsCrisanta && Core.InventoryManager.IsTrueSwordHeartEquiped())
		{
			PlayMakerFSM.BroadcastEvent("TRUE ENDING EXECUTION START");
		}
		while (!base.Consumed)
		{
			yield return lapse;
		}
		Penitent.transform.position = GetRootMotionPosition(base.PlayerDirection);
		UnityEngine.Object.Destroy(base.gameObject);
		ShowPlayer(show: true);
		Core.Input.SetBlocker("EXECUTION", blocking: false);
	}

	protected override void OnUpdate()
	{
		if (Penitent == null)
		{
			Penitent = Core.Logic.Penitent;
			return;
		}
		_currentTimeThreshold += Time.deltaTime;
		if (base.PlayerInRange && Penitent.Status.IsGrounded && !Penitent.Status.IsHurt && !base.Consumed && base.InteractionTriggered)
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
		Core.Logic.Penitent.IsOnExecution = false;
		GhostTrailGenerator.AreGhostTrailsAllowed = true;
		Hit hit = default(Hit);
		hit.DamageType = DamageArea.DamageType.Critical;
		Hit hit2 = hit;
		Penitent.IncrementFervour(hit2);
		if (IsCrisanta && Core.InventoryManager.IsTrueSwordHeartEquiped())
		{
			PlayMakerFSM.BroadcastEvent("TRUE ENDING EXECUTION DONE");
		}
		if (InstantiatesSomethingAfterExecution && PrefabToInstantiateAfterExecution != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(PrefabToInstantiateAfterExecution, base.transform.position, base.transform.rotation);
			gameObject.transform.localScale = ((InstanceOrientation != EntityOrientation.Left) ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, 1f, 1f));
		}
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

	private void TriggerAwareness()
	{
		if (!(ExecutionAwareness != null))
		{
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
