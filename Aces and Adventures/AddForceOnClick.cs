using System;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class AddForceOnClick : MonoBehaviour, IPointerDownHandler, IEventSystemHandler, IPointerUpHandler, IPointerClickHandler
{
	private static System.Random _random;

	[EnumFlags]
	public PointerInputButtonFlags validButtons = PointerInputButtonFlags.Left;

	public Vector2 forceRange = new Vector2(100f, 200f);

	public ForceMode forceMode = ForceMode.Impulse;

	public SingleSoundPack clickSound;

	private Rigidbody _body;

	private static System.Random random => _random ?? (_random = new System.Random());

	public Rigidbody body
	{
		get
		{
			if (!_body)
			{
				return _body = GetComponentInParent<Rigidbody>();
			}
			return _body;
		}
	}

	public AddForceOnClick SetData(Vector2 forceRange, ForceMode forceMode)
	{
		this.forceRange = forceRange;
		this.forceMode = forceMode;
		return this;
	}

	public AddForceOnClick SetData(AddForceOnClick copyFrom)
	{
		return SetData(copyFrom.forceRange, copyFrom.forceMode);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
	}

	public void OnPointerUp(PointerEventData eventData)
	{
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!validButtons.IsValid(eventData.button))
		{
			return;
		}
		RaycastResult pointerCurrentRaycast = eventData.pointerCurrentRaycast;
		if (pointerCurrentRaycast.isValid)
		{
			float num = random.Range(forceRange) * Mathf.Sqrt(Mathf.Clamp01(eventData.ClickHeldTime() / InputManager.I.ClickThreshold));
			body.AddForceAtPosition(pointerCurrentRaycast.Direction() * num, pointerCurrentRaycast.worldPosition, forceMode);
			if ((bool)clickSound)
			{
				clickSound.sounds.PlaySafe(eventData.pointerCurrentRaycast.worldPosition, random, clickSound.mixerGroup, loop: false, 0f, 1f, 0f, 50f, 128, 0f, Mathf.Abs(num / forceRange.AbsMax().InsureNonZero()));
			}
		}
	}
}
