using System.Collections;
using DG.Tweening;
using FMOD.Studio;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Tools.Level.Actionables;

public class DestroyableBridge : PersistentObject, IActionable
{
	private class DestroyableBridgePersistenceData : PersistentManager.PersistentData
	{
		public bool used;

		public DestroyableBridgePersistenceData(string id)
			: base(id)
		{
		}
	}

	public Transform Destination;

	public float interpolationSeconds = 0.5f;

	public AnimationCurve translationCurve;

	public AnimationCurve rotationCurve;

	public bool alreadyUsed;

	public Transform childBridge;

	private Vector2 _targetDestination;

	private Quaternion _targetRotation;

	[SerializeField]
	[BoxGroup("Audio", true, false, 0)]
	[EventRef]
	private string bridgeFallingSound;

	private EventInstance _audioInstance;

	public bool Enabled { get; private set; }

	public bool Locked { get; set; }

	public void Use()
	{
		PlaySound();
		StartCoroutine(MoveToDestinationCoroutine());
	}

	private void Start()
	{
	}

	private void GetFinalPositionAndRotation()
	{
		_targetDestination = Destination.position;
		_targetRotation = Destination.rotation;
	}

	private void SetActivatedTransform()
	{
		childBridge.position = _targetDestination;
		childBridge.rotation = _targetRotation;
	}

	private IEnumerator MoveToDestinationCoroutine()
	{
		Vector2 originPos = childBridge.position;
		Vector2 targetPosition = Destination.position;
		float angle = Destination.localEulerAngles.z;
		childBridge.DOMove(targetPosition, interpolationSeconds).SetEase(translationCurve);
		childBridge.DOLocalRotate(new Vector3(0f, 0f, angle), interpolationSeconds).SetEase(rotationCurve);
		childBridge.GetComponentInChildren<Collider2D>().enabled = true;
		alreadyUsed = true;
		yield return null;
	}

	private void OnDrawGizmos()
	{
		if (!Application.isPlaying)
		{
			Debug.DrawLine(childBridge.position, Destination.position);
		}
	}

	public override PersistentManager.PersistentData GetCurrentPersistentState(string dataPath, bool fullSave)
	{
		DestroyableBridgePersistenceData destroyableBridgePersistenceData = CreatePersistentData<DestroyableBridgePersistenceData>();
		destroyableBridgePersistenceData.used = alreadyUsed;
		return destroyableBridgePersistenceData;
	}

	public override void SetCurrentPersistentState(PersistentManager.PersistentData data, bool isloading, string dataPath)
	{
		DestroyableBridgePersistenceData destroyableBridgePersistenceData = (DestroyableBridgePersistenceData)data;
		alreadyUsed = destroyableBridgePersistenceData.used;
		if (alreadyUsed)
		{
			GetFinalPositionAndRotation();
			SetActivatedTransform();
		}
	}

	public void PlaySound()
	{
		if (_audioInstance.isValid())
		{
			_audioInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		}
		_audioInstance = Core.Audio.CreateEvent(bridgeFallingSound);
		_audioInstance.start();
	}
}
