using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Button3DSoundPackHook : MonoBehaviour
{
	private static System.Random _Random;

	[SerializeField]
	protected Button3DSoundPack _soundPack;

	public bool playSoundIn3D;

	private static System.Random Random => _Random ?? (_Random = new System.Random());

	public Button3DSoundPack soundPack => this.CacheScriptObject(ref _soundPack);

	private void Start()
	{
		_RegisterEvents();
	}

	private void _RegisterEvents()
	{
		_RegisterPointerOverEvents(GetComponent<PointerOver3D>());
		if (!_RegisterPointerClickEvents(GetComponent<PointerClick3D>()))
		{
			_RegisterButtonEvents(GetComponent<Button>());
		}
	}

	private void _RegisterPointerOverEvents(PointerOver3D pointerOver)
	{
		if ((bool)pointerOver)
		{
			pointerOver.OnEnter.AddListener(delegate(PointerEventData e)
			{
				_PlaySound(e, soundPack.onEnter);
			});
			pointerOver.OnExit.AddListener(delegate(PointerEventData e)
			{
				_PlaySound(e, soundPack.onExit);
			});
		}
	}

	private bool _RegisterPointerClickEvents(PointerClick3D pointerClick)
	{
		if (!pointerClick)
		{
			return false;
		}
		pointerClick.OnDown.AddListener(delegate(PointerEventData e)
		{
			_PlaySound(e, soundPack.onDown);
		});
		pointerClick.OnUp.AddListener(delegate(PointerEventData e)
		{
			_PlaySound(e, soundPack.onUp);
		});
		pointerClick.OnClick.AddListener(delegate(PointerEventData e)
		{
			_PlaySound(e, soundPack.onClick);
		});
		return true;
	}

	private void _RegisterButtonEvents(Button button)
	{
		if ((bool)button)
		{
			button.onClick.AddListener(delegate
			{
				_PlaySound(null, soundPack.onClick);
			});
		}
	}

	private void _PlaySound(PointerEventData eventData, PooledAudioPack sounds)
	{
		if (playSoundIn3D)
		{
			sounds.PlaySafe(eventData.GetWorldPositionOfPress() ?? base.transform.position, Random, soundPack.mixerGroup);
		}
		else
		{
			sounds.PlaySafe(Random, soundPack.mixerGroup);
		}
	}
}
