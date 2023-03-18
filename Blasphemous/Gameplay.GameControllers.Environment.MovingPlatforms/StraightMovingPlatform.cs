using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using Tools.Gameplay;
using Tools.Level;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.MovingPlatforms;

[RequireComponent(typeof(Collider2D))]
public class StraightMovingPlatform : PersistentObject, IActionable
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	protected bool IsSafePosition;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string MovingPlatformLoop;

	[SerializeField]
	[FoldoutGroup("Audio", 0)]
	[EventRef]
	protected string MovingPlatformStop;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool persistState;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private Transform Destination;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[PropertyRange(0.0, 60.0)]
	private float RunningTimeout;

	private float currentRunningTimeoutLapse;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	public float Speed = 2f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private float ChasingElongation = 0.5f;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool StartRunning;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool DeactivatesGameobjectsWhenRunning;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("DeactivatesGameobjectsWhenRunning", true)]
	private List<GameObject> GameobjectsToDeactivate;

	[SerializeField]
	[FoldoutGroup("Motion Settings", 0)]
	private bool OneWay;

	[SerializeField]
	[BoxGroup("Flag Settings", true, false, 0)]
	[OnValueChanged("DestinyFlagName", false)]
	protected string OnDestination;

	private Vector3 _origin;

	private Vector3 _destination;

	protected Vector2 OriginPosition;

	protected Vector2 DestinationPosition;

	private float _accumulatedTime;

	private Vector3 _velocity = Vector3.zero;

	private Collider2D _platformCollider;

	private bool _running;

	private SpriteRenderer _spriteRenderer;

	private const float VerticalOffset = 0.01f;

	private bool _gosDeactivated;

	private int _journeyCount;

	private Tweener tweener;

	private EventInstance _movementLoopFx;

	public bool Enabled { get; private set; }

	public bool IsRunning => _running;

	private bool TimeoutConsumed => currentRunningTimeoutLapse <= 0f;

	public bool Locked { get; set; }

	private void Start()
	{
		currentRunningTimeoutLapse = RunningTimeout;
		MovingPlatformDestination componentInChildren = GetComponentInChildren<MovingPlatformDestination>();
		Vector3 position = componentInChildren.transform.position;
		_spriteRenderer = GetComponent<SpriteRenderer>();
		_destination = (DestinationPosition = new Vector2(position.x, position.y));
		_origin = (OriginPosition = new Vector2(base.transform.position.x, base.transform.position.y));
		_platformCollider = GetComponent<BoxCollider2D>();
		if (StartRunning)
		{
			_running = true;
		}
		if (Core.Events.GetFlag(OnDestination))
		{
			base.transform.position = DestinationPosition;
			SwapTravelPoints(ref _origin, ref _destination);
		}
		if (!IsSafePosition && base.gameObject.GetComponent<NoSafePosition>() == null)
		{
			base.gameObject.AddComponent<NoSafePosition>();
		}
	}

	public Vector3 GetVelocity()
	{
		return _velocity;
	}

	private void Update()
	{
		if (!_platformCollider.enabled || !_running)
		{
			return;
		}
		float num = Vector2.Distance(base.transform.position, _destination);
		currentRunningTimeoutLapse -= Time.deltaTime;
		if (num > 0f && !DOTween.IsTweening(base.transform) && TimeoutConsumed)
		{
			tweener = base.transform.DOMove(_destination, num / Speed).SetEase(Ease.InOutSine).SetUpdate(UpdateType.Normal, isIndependentUpdate: false);
			PlayMovementLoopFx(ref _movementLoopFx);
		}
		if (num <= 0.01f)
		{
			_journeyCount++;
			currentRunningTimeoutLapse = RunningTimeout;
			base.transform.position = _destination;
			_destination = _origin;
			_origin = base.transform.position;
			StopMovementLoopFx(ref _movementLoopFx);
			PlayMovementStopFx();
			tweener.Kill();
			tweener = null;
		}
		SetMotionConstraints();
		if (DeactivatesGameobjectsWhenRunning && !_gosDeactivated)
		{
			_gosDeactivated = true;
			GameobjectsToDeactivate.ForEach(delegate(GameObject x)
			{
				x.SetActive(value: false);
			});
		}
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Penitent"))
		{
			Core.Logic.Penitent.FloorChecker.OnMovingPlatform = true;
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.gameObject.CompareTag("Penitent"))
		{
			Core.Logic.Penitent.FloorChecker.OnMovingPlatform = false;
		}
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying && Destination != null)
		{
			Debug.DrawLine(base.transform.position, Destination.position);
		}
	}

	public void DestinyFlagName(string flagName)
	{
		OnDestination = flagName.Replace(' ', '_').ToUpper();
	}

	private void SetMotionConstraints()
	{
		if (OneWay && _journeyCount > 0)
		{
			if ((Vector2)base.transform.position == DestinationPosition)
			{
				_running = false;
			}
			if ((Vector2)base.transform.position == OriginPosition)
			{
				_running = false;
			}
			SetMotionFlags();
		}
	}

	private void SetMotionFlags()
	{
		bool b = (Vector2)base.transform.position == DestinationPosition;
		Core.Events.SetFlag(OnDestination, b);
	}

	public void ResetJourneyCounter()
	{
		if (_journeyCount > 0)
		{
			_journeyCount = 0;
		}
	}

	private void SwapTravelPoints(ref Vector3 origin, ref Vector3 destination)
	{
		Vector3 vector = origin;
		origin = destination;
		destination = vector;
	}

	public void Use()
	{
		ResetJourneyCounter();
		_running = !_running;
		if (DOTween.IsTweening(base.transform))
		{
			StopMovementLoopFx(ref _movementLoopFx);
			DOTween.Kill(base.transform);
		}
	}

	public void Reset()
	{
		ResetJourneyCounter();
		_running = false;
		if (DOTween.IsTweening(base.transform))
		{
			StopMovementLoopFx(ref _movementLoopFx);
			DOTween.Kill(base.transform);
		}
		currentRunningTimeoutLapse = RunningTimeout;
		MovingPlatformDestination componentInChildren = GetComponentInChildren<MovingPlatformDestination>(includeInactive: true);
		Vector3 position = componentInChildren.transform.position;
		_destination = DestinationPosition;
		_origin = OriginPosition;
		if (Core.Events.GetFlag(OnDestination))
		{
			base.transform.position = DestinationPosition;
			SwapTravelPoints(ref _origin, ref _destination);
		}
		else
		{
			base.transform.position = OriginPosition;
		}
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		if (persistState)
		{
			BasicPersistence basicPersistence = (BasicPersistence)data;
			_running = basicPersistence.triggered;
			if (Core.Events.GetFlag(OnDestination))
			{
				_running = false;
			}
		}
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		if (!persistState)
		{
			return null;
		}
		BasicPersistence basicPersistence = CreatePersistentData<BasicPersistence>();
		basicPersistence.triggered = _running;
		return basicPersistence;
	}

	private void OnDestroy()
	{
		if (_running)
		{
			Core.Events.SetFlag(OnDestination, b: true);
		}
		StopMovementLoopFx(ref _movementLoopFx);
	}

	private void PlayMovementStopFx()
	{
		if (!MovingPlatformStop.IsNullOrWhitespace() && _spriteRenderer.isVisible)
		{
			Core.Audio.PlaySfx(MovingPlatformStop);
		}
	}

	private void PlayMovementLoopFx(ref EventInstance audioInstance)
	{
		if (!MovingPlatformLoop.IsNullOrWhitespace())
		{
			if (_movementLoopFx.isValid())
			{
				_movementLoopFx.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
				_movementLoopFx.release();
				_movementLoopFx = default(EventInstance);
			}
			_movementLoopFx = Core.Audio.CreateEvent(MovingPlatformLoop, base.transform.position);
			_movementLoopFx.start();
		}
	}

	private void StopMovementLoopFx(ref EventInstance audioInstance)
	{
		if (_movementLoopFx.isValid())
		{
			PlayMovementStopFx();
			_movementLoopFx.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
			_movementLoopFx.release();
			_movementLoopFx = default(EventInstance);
		}
	}
}
