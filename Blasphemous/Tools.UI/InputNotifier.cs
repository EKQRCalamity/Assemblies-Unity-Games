using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Entities;
using Rewired;
using RewiredConsts;
using Sirenix.OdinInspector;
using Tools.Level;
using UnityEngine;

namespace Tools.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class InputNotifier : MonoBehaviour
{
	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[ActionIdProperty(typeof(Action))]
	private int action;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool requiresInteractable = true;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	[ShowIf("requiresInteractable", true)]
	private Interactable connectToInteractable;

	[SerializeField]
	[BoxGroup("Design Settings", true, false, 0)]
	private bool showWhenInputBlocked;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private InputIcon inputIconPrefab;

	[SerializeField]
	[BoxGroup("Attached References", true, false, 0)]
	private CollisionSensor[] sensor;

	private float fadeTime = 0.15f;

	private SpriteRenderer spriteRenderer;

	private InputIcon inputIcon;

	private bool AllwaysShow;

	private bool Showing;

	private bool Ready => sensor != null;

	private bool InteractableCanBeConsumed => connectToInteractable != null && connectToInteractable.CanBeConsumed();

	private bool InteractableIsBeingUsed => connectToInteractable != null && connectToInteractable.BeingUsed;

	private bool InteractableLocked => connectToInteractable != null && connectToInteractable.Locked;

	private bool InteractablePlayerInside => connectToInteractable != null && connectToInteractable.PlayerInRange;

	private void Start()
	{
		if (!Ready)
		{
			return;
		}
		spriteRenderer = GetComponent<SpriteRenderer>();
		CreateInputIcon();
		if (connectToInteractable != null && connectToInteractable.AllwaysShowIcon())
		{
			AllwaysShow = true;
			Showing = true;
			inputIcon.Fade(1f, fadeTime);
			inputIcon.RefreshIcon();
			spriteRenderer.DOFade(1f, fadeTime);
			return;
		}
		for (int i = 0; i < sensor.Length; i++)
		{
			if (!(sensor[i] == null))
			{
				sensor[i].OnPenitentEnter += ShowNotification;
				sensor[i].OnPenitentExit += HideNotification;
			}
		}
		Core.Input.OnInputLocked += HideNotification;
		Core.Input.OnInputUnlocked += ShowNotification;
	}

	private void OnDestroy()
	{
		if (AllwaysShow)
		{
			return;
		}
		for (int i = 0; i < sensor.Length; i++)
		{
			if (!(sensor[i] == null))
			{
				sensor[i].OnPenitentEnter -= ShowNotification;
				sensor[i].OnPenitentExit -= HideNotification;
			}
		}
		Core.Input.OnInputLocked -= HideNotification;
		Core.Input.OnInputUnlocked -= ShowNotification;
	}

	private void Update()
	{
		if (AllwaysShow && Showing && (!InteractableCanBeConsumed || InteractableIsBeingUsed))
		{
			Showing = false;
			HideNotification();
		}
	}

	private void CreateInputIcon()
	{
		inputIcon = Object.Instantiate(inputIconPrefab);
		SpriteRenderer componentInChildren = inputIcon.GetComponentInChildren<SpriteRenderer>();
		TextMesh componentInChildren2 = inputIcon.GetComponentInChildren<TextMesh>();
		Color color = componentInChildren.color;
		color.a = 0f;
		componentInChildren.color = color;
		color = componentInChildren2.color;
		color.a = 0f;
		componentInChildren2.color = color;
		inputIcon.transform.parent = base.transform;
		inputIcon.transform.localPosition = Vector3.zero;
		inputIcon.action = action;
	}

	private void ShowNotification()
	{
		if (!inputIcon)
		{
			CreateInputIcon();
		}
		Invoke("DelayerShowNotification", 0.1f);
	}

	private void DelayerShowNotification()
	{
		if (Ready && (!requiresInteractable || (InteractableCanBeConsumed && InteractablePlayerInside && !InteractableLocked)) && (!Core.Input.InputBlocked || showWhenInputBlocked))
		{
			inputIcon.Fade(1f, fadeTime);
			inputIcon.RefreshIcon();
			spriteRenderer.DOFade(1f, fadeTime);
		}
	}

	private void HideNotification()
	{
		if (Ready)
		{
			inputIcon.Fade(0f, fadeTime);
		}
	}
}
