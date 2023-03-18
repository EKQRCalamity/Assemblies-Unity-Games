using System;
using System.Collections;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Entity;

public class ColorFlash : MonoBehaviour
{
	public bool EnabledEffect = true;

	private bool _waitForFlash;

	[SerializeField]
	protected SpriteRenderer EntityRenderer;

	public Color FlashColor;

	private Material flashMat;

	private Material originalMat;

	public float FlashTimeAmount;

	private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

	private static readonly int Color = Shader.PropertyToID("_FlashColor");

	private void Start()
	{
		try
		{
			flashMat = new Material(Shader.Find("Sprites/DefaultColorFlash"));
			flashMat.SetColor(Color, FlashColor);
		}
		catch (Exception ex)
		{
			Debug.LogWarning(ex.Message);
		}
	}

	public void TriggerColorFlash()
	{
		if (!_waitForFlash && EnabledEffect)
		{
			_waitForFlash = true;
			StartCoroutine(ColorFlashCoroutine());
		}
	}

	private IEnumerator ColorFlashCoroutine()
	{
		originalMat = EntityRenderer.material;
		EntityRenderer.material = flashMat;
		EntityRenderer.material.SetFloat(FlashAmount, 1f);
		yield return new WaitForSeconds(FlashTimeAmount);
		EntityRenderer.material.SetFloat(FlashAmount, 0f);
		EntityRenderer.material = originalMat;
		_waitForFlash = false;
	}
}
