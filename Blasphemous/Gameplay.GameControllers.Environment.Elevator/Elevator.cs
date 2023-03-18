using System;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.GameControllers.Environment.Elevator;

public class Elevator : MonoBehaviour
{
	[Serializable]
	public struct Floor
	{
		public int Order;

		public Transform Platform;

		public string OnDestination;
	}

	[SerializeField]
	public Floor[] Floors;

	[FoldoutGroup("Elevator Settings", 0)]
	[Range(1f, 100f)]
	public float MovingSpeed = 5f;

	[FoldoutGroup("Elevator Settings", 0)]
	public AnimationCurve MoveEase;

	[FoldoutGroup("Audio", 0)]
	[EventRef]
	public string MotionAudioFx;

	[FoldoutGroup("Audio", 0)]
	[EventRef]
	public string StartMotionAudioFx;

	[FoldoutGroup("Audio", 0)]
	[EventRef]
	public string StopMotionAudioFx;

	private EventInstance _elevatorMotionAudio;

	private Floor _currentFloor;

	private Floor _wishFloor;

	private Dictionary<int, Vector2> _floorPositions = new Dictionary<int, Vector2>();

	public bool IsRunning { get; private set; }

	private void Awake()
	{
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		SetPlatformsPositions();
		SetFlaggedPosition();
		IsRunning = false;
	}

	public void MoveToFloor(int order)
	{
		if (_currentFloor.Order != order && !IsRunning)
		{
			Floor wishFloor = GetWishFloor(order);
			_wishFloor = wishFloor;
			base.transform.DOMove(_floorPositions[_wishFloor.Order], GetDisplacementLapse(_wishFloor)).SetEase(MoveEase).OnStart(OnMoveStart)
				.OnComplete(OnMoveStop);
		}
	}

	private void OnMoveStart()
	{
		IsRunning = true;
		PlayMotionSound();
	}

	private void OnMoveStop()
	{
		IsRunning = false;
		_currentFloor = _wishFloor;
		SetWishFloorFlag(_wishFloor);
		StopMotionSound();
	}

	private Floor GetWishFloor(int order)
	{
		Floor result = _currentFloor;
		Floor[] floors = Floors;
		for (int i = 0; i < floors.Length; i++)
		{
			Floor floor = floors[i];
			if (floor.Order == order)
			{
				result = floor;
				break;
			}
		}
		return result;
	}

	private float GetDisplacementLapse(Floor wishFloor)
	{
		float num = Vector2.Distance(_currentFloor.Platform.position, wishFloor.Platform.position);
		return num / MovingSpeed;
	}

	private void SetPlatformsPositions()
	{
		Floor[] floors = Floors;
		for (int i = 0; i < floors.Length; i++)
		{
			Floor floor = floors[i];
			Vector2 value = new Vector2(floor.Platform.position.x, floor.Platform.position.y);
			_floorPositions.Add(floor.Order, value);
		}
	}

	private void SetWishFloorFlag(Floor floor)
	{
		Floor[] floors = Floors;
		for (int i = 0; i < floors.Length; i++)
		{
			Floor floor2 = floors[i];
			Core.Events.SetFlag(floor2.OnDestination, b: false);
		}
		Core.Events.SetFlag(floor.OnDestination, b: true);
	}

	private Floor GetFlaggedFloor()
	{
		Floor result = GetWishFloor(0);
		Floor[] floors = Floors;
		for (int i = 0; i < floors.Length; i++)
		{
			Floor floor = floors[i];
			if (Core.Events.GetFlag(floor.OnDestination))
			{
				result = floor;
			}
		}
		return result;
	}

	private void SetFlaggedPosition()
	{
		Floor flaggedFloor = GetFlaggedFloor();
		_currentFloor = flaggedFloor;
		base.transform.position = _floorPositions[_currentFloor.Order];
	}

	private void PlayMotionSound()
	{
		PlayElevatorActivation(StartMotionAudioFx);
		if (!_elevatorMotionAudio.isValid())
		{
			Core.Audio.PlayEventNoCatalog(ref _elevatorMotionAudio, MotionAudioFx);
		}
	}

	private void StopMotionSound()
	{
		PlayElevatorActivation(StopMotionAudioFx);
		if (_elevatorMotionAudio.isValid())
		{
			_elevatorMotionAudio.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
			_elevatorMotionAudio.release();
			_elevatorMotionAudio = default(EventInstance);
		}
	}

	private void PlayElevatorActivation(string activationEvent)
	{
		if (!string.IsNullOrEmpty(activationEvent) && (bool)Core.Logic.Penitent && Core.Logic.Penitent.IsVisible())
		{
			Core.Audio.PlaySfx(activationEvent);
		}
	}

	private void OnDestroy()
	{
		StopMotionSound();
		SpawnManager.OnPlayerSpawn -= OnPlayerSpawn;
	}
}
