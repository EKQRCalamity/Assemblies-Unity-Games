using System;
using System.Collections;
using Gameplay.UI;
using Sirenix.OdinInspector;
using Tools.Level.Layout;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Entity;

public class MasterShaderEffects : MonoBehaviour
{
	[SerializeField]
	protected SpriteRenderer EntityRenderer;

	[FoldoutGroup("Color flash", 0)]
	public Color FlashColor;

	[FoldoutGroup("Color flash", 0)]
	public float FlashTimeAmount;

	[FoldoutGroup("Damage effect", 0)]
	public Material damageEffectTestMaterial;

	public bool applyLevelEffects;

	private const string MATERIAL_TO_IGNORE = "ArcadeDamage";

	private bool _waitForFlash;

	private Coroutine lerpInOut;

	private void Awake()
	{
		if (EntityRenderer == null)
		{
			EntityRenderer = GetComponent<SpriteRenderer>();
		}
	}

	private void Start()
	{
		if (applyLevelEffects)
		{
			LevelInitializer levelInitializer = UnityEngine.Object.FindObjectOfType<LevelInitializer>();
			if (levelInitializer != null)
			{
				levelInitializer.ApplyLevelColorEffects(this);
			}
		}
	}

	[FoldoutGroup("Color flash", 0)]
	[Button("ColorFlash", ButtonSizes.Small)]
	public void TriggerColorFlash()
	{
		if (!_waitForFlash)
		{
			_waitForFlash = true;
			StartCoroutine(ColorFlashCoroutine());
		}
	}

	private IEnumerator ColorFlashCoroutine()
	{
		EntityRenderer.material.EnableKeyword("COLOR_LERP_ON");
		EntityRenderer.material.SetFloat("_LerpAmount", 1f);
		yield return new WaitForSecondsRealtime(FlashTimeAmount);
		EntityRenderer.material.SetFloat("_LerpAmount", 0f);
		EntityRenderer.material.DisableKeyword("COLOR_LERP_ON");
		_waitForFlash = false;
	}

	public void SetColorizeStrength(float v)
	{
		EntityRenderer.material.SetFloat("_ColorizeStrength", v);
	}

	public void SetColorTintStrength(float v)
	{
		EntityRenderer.material.SetFloat("_LerpAmount", v);
	}

	public void SetColorTint(Color color, float strength, bool enabled)
	{
		SetColorTintStrength(1f);
		EntityRenderer.material.SetColor("_LerpColor", color);
		if (enabled)
		{
			EntityRenderer.material.EnableKeyword("COLOR_LERP_ON");
		}
		else
		{
			EntityRenderer.material.DisableKeyword("COLOR_LERP_ON");
		}
	}

	public void TriggerColorizeLerp(float origin, float end, float seconds, Action callback = null)
	{
		StartCoroutine(ColorizeLerp(origin, end, seconds, callback));
	}

	public void TriggerColorizeLerpInOut(float secondsIn, float secondsOut)
	{
		if (lerpInOut != null)
		{
			StopCoroutine(lerpInOut);
		}
		lerpInOut = StartCoroutine(ColorizeLerpInOut(secondsIn, secondsOut));
	}

	public void TriggerColorTintLerp(float origin, float end, float seconds, Action callback = null)
	{
		StartCoroutine(ColorTintLerp(origin, end, seconds, callback));
	}

	public void StartColorizeLerp(float origin, float end, float seconds, Action callback)
	{
		StartCoroutine(ColorizeLerp(origin, end, seconds, callback));
	}

	private IEnumerator ColorizeLerp(float origin, float end, float seconds, Action callback)
	{
		EntityRenderer.material.EnableKeyword("COLORIZE_ON");
		float counter = 0f;
		while (counter < seconds)
		{
			float v = Mathf.Lerp(origin, end, counter / seconds);
			counter += Time.deltaTime;
			SetColorizeStrength(v);
			yield return null;
		}
		EntityRenderer.material.SetFloat("_ColorizeStrength", end);
		callback?.Invoke();
	}

	private IEnumerator ColorizeLerpInOut(float secondsIn, float secondsOut)
	{
		yield return StartCoroutine(ColorizeLerp(0f, 1f, secondsIn, null));
		yield return StartCoroutine(ColorizeLerp(1f, 0f, secondsOut, null));
	}

	private IEnumerator ColorTintLerp(float origin, float end, float seconds, Action callback)
	{
		EntityRenderer.material.EnableKeyword("COLOR_LERP_ON");
		float counter = 0f;
		float v2 = 0f;
		while (counter < seconds)
		{
			v2 = Mathf.Lerp(origin, end, counter / seconds);
			counter += Time.deltaTime;
			SetColorTintStrength(v2);
			yield return null;
		}
		EntityRenderer.material.SetFloat("_LerpAmount", end);
		callback?.Invoke();
	}

	public void TestDamageEffect()
	{
		EntityRenderer.material.EnableKeyword("DAMAGE_EFFECT_ON");
		SetDamageEffectFromMaterial(damageEffectTestMaterial);
	}

	public void DamageEffectBlink(float waitSeconds, float blinkSeconds, Material effectMaterial = null)
	{
		if (effectMaterial == null)
		{
			effectMaterial = damageEffectTestMaterial;
		}
		SetDamageEffectFromMaterial(effectMaterial);
		UIController.instance.StartCoroutine(DamageEffectCoroutine(waitSeconds, blinkSeconds));
	}

	private IEnumerator DamageEffectCoroutine(float waitseconds, float blinkseconds)
	{
		if (EntityRenderer.material.name.StartsWith("ArcadeDamage"))
		{
			yield return null;
			yield break;
		}
		if (EntityRenderer != null)
		{
			EntityRenderer.material.EnableKeyword("DAMAGE_EFFECT_ON");
		}
		if (EntityRenderer != null)
		{
			yield return new WaitForSecondsRealtime(blinkseconds);
		}
		if (EntityRenderer != null)
		{
			yield return new WaitUntil(() => !EntityRenderer.material.name.StartsWith("ArcadeDamage"));
		}
		if (EntityRenderer != null)
		{
			EntityRenderer.material.DisableKeyword("DAMAGE_EFFECT_ON");
		}
	}

	public void ColorizeWave(float frec)
	{
		float num = Mathf.Sin(Time.time * frec);
		num = (1f + num) / 2f;
		EntityRenderer.material.SetFloat("_ColorizeStrength", num);
	}

	public void SetDamageEffectFromMaterial(Material m)
	{
		EntityRenderer.material.SetColor("_DamageEffectColor", m.GetColor("_Color"));
		EntityRenderer.material.SetColor("_DamageEffectHighlight", m.GetColor("_Highlight"));
		EntityRenderer.material.SetFloat("_DamageEffectTimescale", m.GetFloat("_TimeScale"));
	}

	public void SetColorizeData(Color colorizeColor, Color colorizeMultColor, float colorizeAmount)
	{
		EntityRenderer.material.EnableKeyword("COLORIZE_ON");
		EntityRenderer.material.SetColor("_ColorizeColor", colorizeColor);
		EntityRenderer.material.SetColor("_ColorizeMultColor", colorizeMultColor);
		EntityRenderer.material.SetFloat("_ColorizeStrength", colorizeAmount);
	}

	public void DeactivateColorize()
	{
		EntityRenderer.sharedMaterial.DisableKeyword("COLORIZE_ON");
	}
}
