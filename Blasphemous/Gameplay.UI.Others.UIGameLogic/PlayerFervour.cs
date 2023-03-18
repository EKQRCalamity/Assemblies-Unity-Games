using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Framework.FrameworkCore;
using Framework.Inventory;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using Gameplay.GameControllers.Penitent.Abilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Gameplay.UI.Others.UIGameLogic;

public class PlayerFervour : MonoBehaviour
{
	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private Image fillAnimable;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private Image fillExact;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private Image fillExactFull;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private Image fillNotEnough;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private AnimationCurve LossAnimationCurve;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private AnimationCurve AddAnimationCurve;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private AnimationCurve NotEnoughAlphaAnimationCurve;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private RectTransform background;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private RectTransform backgroundMid;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private float backgroundStartSize = 60f;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private float endFillSize = 10f;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	[Range(0f, 10f)]
	private float speed;

	[BoxGroup("Bar", true, false, 0)]
	[SerializeField]
	private Transform marksParent;

	[BoxGroup("Prayer Info", true, false, 0)]
	[SerializeField]
	private GameObject normalPrayerInUse;

	[BoxGroup("Prayer Info", true, false, 0)]
	[SerializeField]
	private GameObject pe02PrayerInUse;

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	private GameObject fervourSpark;

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	private string barAnimChildName = "Anim";

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	private string barMaskChildName = "Mask";

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	private string barBarChildName = "Bar";

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	private float epsilonToShowLastBar = 5f;

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	private int barAnimEndPosition = 7;

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	private float barAnimUpdatedElapsed = 0.4f;

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	private float barAnimMovementPerElapsed = 4f;

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	private float barNotEnoughDuration = 0.5f;

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	[EventRef]
	private string notEnoughSound = string.Empty;

	[BoxGroup("Bar Anim", true, false, 0)]
	[SerializeField]
	[EventRef]
	private string notEnoughSoundNotEquipped = string.Empty;

	[BoxGroup("Prayer Info", true, false, 0)]
	[SerializeField]
	private Image prayerTimer;

	[BoxGroup("Guilt", true, false, 0)]
	[SerializeField]
	private GameObject guiltRoot;

	[BoxGroup("Guilt", true, false, 0)]
	[SerializeField]
	private RectTransform guiltBar;

	[BoxGroup("Guilt", true, false, 0)]
	[SerializeField]
	private int positionOcupedByBack = 5;

	[BoxGroup("Guilt", true, false, 0)]
	[SerializeField]
	private Image guiltEnd;

	[BoxGroup("Guilt", true, false, 0)]
	[SerializeField]
	private Image guiltStart;

	private RectTransform fillAnimableTransform;

	private RectTransform fillExactTransform;

	private RectTransform fillExactFullTransform;

	private RectTransform fillNotEnoughTransform;

	private float _timeElapsed;

	private float lastBarWidth = -1f;

	private float lastValue = -1f;

	private float lastMaxFervour = -1f;

	private int currentMarks = -1;

	private int currentMarksSeparation = -1;

	private bool fillsIncrease;

	private List<RectTransform> anims;

	private float currentAnimPosition;

	private float currentAnimElapsed;

	private bool isNotEnough;

	private float currentNotEnough;

	private float currentSegmentsFilled = -1f;

	public const float EXECUTION_FERVOUR_BONUS = 13.3f;

	public static PlayerFervour Instance { get; private set; }

	private float BarTarget
	{
		get
		{
			Penitent penitent = Core.Logic.Penitent;
			float current = penitent.Stats.Fervour.Current;
			float currentMaxWithoutFactor = penitent.Stats.Fervour.CurrentMaxWithoutFactor;
			return current / currentMaxWithoutFactor;
		}
	}

	private void Awake()
	{
		Instance = this;
		anims = new List<RectTransform>();
		fillExactTransform = fillExact.GetComponent<RectTransform>();
		fillExactFullTransform = fillExactFull.GetComponent<RectTransform>();
		fillAnimableTransform = fillAnimable.GetComponent<RectTransform>();
		fillNotEnoughTransform = fillNotEnough.GetComponent<RectTransform>();
		ChangeImageAlpha(fillNotEnough, 0f);
		LevelManager.OnLevelLoaded += OnLevelLoaded;
		LevelManager.OnBeforeLevelLoad += OnBeforeLevelLoad;
		normalPrayerInUse.SetActive(value: false);
		pe02PrayerInUse.SetActive(value: false);
		prayerTimer.fillAmount = 0f;
		base.enabled = false;
		fervourSpark.SetActive(value: false);
	}

	private void OnDestroy()
	{
		LevelManager.OnLevelLoaded -= OnLevelLoaded;
		LevelManager.OnBeforeLevelLoad -= OnBeforeLevelLoad;
	}

	private void OnBeforeLevelLoad(Level oldLevel, Level newLevel)
	{
		base.enabled = false;
	}

	private void OnLevelLoaded(Level oldLevel, Level newLevel)
	{
		base.enabled = true;
	}

	private void Update()
	{
		PrayerUse prayerCast = Core.Logic.Penitent.PrayerCast;
		normalPrayerInUse.SetActive(prayerCast.IsUsingAbility && !Core.PenitenceManager.UseStocksOfHealth);
		pe02PrayerInUse.SetActive(prayerCast.IsUsingAbility && Core.PenitenceManager.UseStocksOfHealth);
		float fillAmount = 0f;
		if (prayerCast.IsUsingAbility)
		{
			fillAmount = 1f - prayerCast.GetPercentTimeCasting();
		}
		prayerTimer.fillAmount = fillAmount;
		float barTarget = BarTarget;
		if (lastValue != barTarget)
		{
			fillsIncrease = lastValue < barTarget;
			lastValue = barTarget;
			_timeElapsed = 0f;
		}
		CalculateBarSize();
		CalculateFillsBars();
		CalculateMarks();
		CalculateNotEnough();
		Penitent penitent = Core.Logic.Penitent;
		float currentMaxWithoutFactor = penitent.Stats.Fervour.CurrentMaxWithoutFactor;
		if (currentMaxWithoutFactor != lastMaxFervour)
		{
			lastMaxFervour = currentMaxWithoutFactor;
			CalculateBarPentalty();
		}
	}

	public void Show()
	{
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	public void RefreshGuilt(bool whenDead)
	{
		if (whenDead)
		{
			StartCoroutine(RefreshGuiltWhenDead());
			return;
		}
		Penitent penitent = Core.Logic.Penitent;
		float maxFactor = penitent.Stats.Fervour.MaxFactor;
		Image component = guiltBar.gameObject.GetComponent<Image>();
		if (maxFactor >= 1f)
		{
			component.DOFade(0f, 1f);
			guiltEnd.DOFade(0f, 1f);
			guiltStart.DOFade(0f, 1f);
		}
		CalculateBarPentalty();
	}

	public void NotEnoughFervour()
	{
		if (isNotEnough)
		{
			return;
		}
		isNotEnough = true;
		currentNotEnough = 0f;
		Prayer prayerInSlot = Core.InventoryManager.GetPrayerInSlot(0);
		if (prayerInSlot == null)
		{
			if (!string.IsNullOrEmpty(notEnoughSoundNotEquipped))
			{
				Core.Audio.PlayOneShot(notEnoughSoundNotEquipped);
			}
		}
		else if (!string.IsNullOrEmpty(notEnoughSound))
		{
			Core.Audio.PlayOneShot(notEnoughSound);
		}
	}

	private IEnumerator RefreshGuiltWhenDead()
	{
		Penitent penitent = Core.Logic.Penitent;
		float factor = penitent.Stats.Fervour.MaxFactor;
		Image bar = guiltBar.gameObject.GetComponent<Image>();
		bar.color = new Color(1f, 1f, 1f, 0f);
		guiltEnd.color = new Color(1f, 1f, 1f, 0f);
		guiltStart.color = new Color(1f, 1f, 1f, 0f);
		guiltBar.sizeDelta = new Vector2(0f, guiltBar.sizeDelta.y);
		yield return new WaitForSeconds(2f);
		if (factor < 1f)
		{
			bar.DOFade(1f, 1f);
			guiltEnd.DOFade(1f, 1f);
			guiltStart.DOFade(1f, 1f);
		}
		CalculateBarPentalty();
	}

	private void CalculateBarSize()
	{
		Penitent penitent = Core.Logic.Penitent;
		float currentMaxWithoutFactor = penitent.Stats.Fervour.CurrentMaxWithoutFactor;
		if (currentMaxWithoutFactor != lastBarWidth)
		{
			lastBarWidth = currentMaxWithoutFactor;
			float num = currentMaxWithoutFactor - backgroundStartSize - endFillSize;
			num = ((!(num > 0f)) ? 0f : num);
			backgroundMid.sizeDelta = new Vector2(num, backgroundMid.sizeDelta.y);
			fillExactTransform.sizeDelta = new Vector2(currentMaxWithoutFactor, fillExactTransform.sizeDelta.y);
			fillExactFullTransform.sizeDelta = new Vector2(currentMaxWithoutFactor, fillExactFullTransform.sizeDelta.y);
			fillAnimableTransform.sizeDelta = new Vector2(currentMaxWithoutFactor, fillAnimableTransform.sizeDelta.y);
			background.sizeDelta = new Vector2(currentMaxWithoutFactor, background.sizeDelta.y);
			fillNotEnoughTransform.sizeDelta = new Vector2(currentMaxWithoutFactor, fillNotEnoughTransform.sizeDelta.y);
		}
	}

	private void CalculateBarPentalty()
	{
		Penitent penitent = Core.Logic.Penitent;
		float currentMaxWithoutFactor = penitent.Stats.Fervour.CurrentMaxWithoutFactor;
		float maxFactor = penitent.Stats.Fervour.MaxFactor;
		guiltRoot.SetActive(maxFactor < 1f);
		if (maxFactor < 1f)
		{
			float x = (1f - maxFactor) * currentMaxWithoutFactor - (float)positionOcupedByBack;
			guiltBar.DOSizeDelta(new Vector2(x, guiltBar.sizeDelta.y), 2f);
		}
	}

	private void CalculateFillsBars()
	{
		_timeElapsed += Time.deltaTime;
		if (fillsIncrease)
		{
			if (Mathf.Approximately(fillExact.fillAmount, BarTarget))
			{
				fillExact.fillAmount = BarTarget;
				_timeElapsed = 0f;
			}
			else
			{
				fillExact.fillAmount = Mathf.Lerp(fillExact.fillAmount, BarTarget, AddAnimationCurve.Evaluate(_timeElapsed));
				float x = (float)(int)Core.Logic.Penitent.Stats.Fervour.CurrentMaxWithoutFactor * fillExact.fillAmount - 1f;
				fervourSpark.transform.localPosition = new Vector3(x, fervourSpark.transform.localPosition.y);
			}
			fillAnimable.fillAmount = fillExact.fillAmount;
		}
		else
		{
			fillExact.fillAmount = BarTarget;
			if (Mathf.Approximately(fillAnimable.fillAmount, BarTarget))
			{
				fillAnimable.fillAmount = BarTarget;
				_timeElapsed = 0f;
			}
			else
			{
				fillAnimable.fillAmount = Mathf.Lerp(fillAnimable.fillAmount, BarTarget, LossAnimationCurve.Evaluate(_timeElapsed));
			}
		}
		fillNotEnough.fillAmount = fillExact.fillAmount;
	}

	private void CalculateMarks()
	{
		int num = 0;
		int num2 = 0;
		Prayer prayerInSlot = Core.InventoryManager.GetPrayerInSlot(0);
		int num3 = (prayerInSlot ? (prayerInSlot.fervourNeeded + (int)Core.Logic.Penitent.Stats.PrayerCostAddition.Final) : 0);
		if (num3 > 0)
		{
			num = (int)Core.Logic.Penitent.Stats.Fervour.CurrentMax / num3;
			num2 = (int)Core.Logic.Penitent.Stats.Fervour.Current / num3;
			fillExactFull.fillAmount = (float)(num2 * num3) / Core.Logic.Penitent.Stats.Fervour.CurrentMaxWithoutFactor;
		}
		else
		{
			fillExactFull.fillAmount = 0f;
		}
		bool flag = Core.Logic.Penitent.Stats.Fervour.CurrentMax - (float)(num3 * num) > epsilonToShowLastBar;
		bool flag2 = false;
		float num4 = (float)(-num3) + 1f;
		if (num != currentMarks || num3 != currentMarksSeparation || (float)num2 != currentSegmentsFilled)
		{
			if (num == 0)
			{
				currentAnimPosition = num4;
				currentAnimElapsed = 0f;
				flag2 = true;
			}
			anims.Clear();
			if (currentAnimPosition > (float)barAnimEndPosition)
			{
				currentAnimPosition = num4;
			}
			currentMarks = num;
			currentMarksSeparation = num3;
			currentSegmentsFilled = num2;
			float num5 = 0f;
			for (int i = 0; i < marksParent.childCount; i++)
			{
				RectTransform rectTransform = (RectTransform)marksParent.GetChild(i);
				bool flag3 = i < currentMarks;
				rectTransform.gameObject.SetActive(flag3);
				if (flag3)
				{
					rectTransform.sizeDelta = new Vector2(num3, rectTransform.sizeDelta.y);
					rectTransform.localPosition = new Vector3(num5, 0f, 0f);
					num5 += (float)currentMarksSeparation;
					RectTransform rectTransform2 = (RectTransform)rectTransform.Find(barMaskChildName);
					rectTransform2.sizeDelta = new Vector2((float)num3 - 1f, rectTransform2.sizeDelta.y);
					RectTransform rectTransform3 = (RectTransform)rectTransform.Find(barBarChildName);
					rectTransform3.gameObject.SetActive(flag || i != currentMarks - 1);
					bool flag4 = (float)i < currentSegmentsFilled;
					RectTransform rectTransform4 = (RectTransform)rectTransform2.Find(barAnimChildName);
					rectTransform4.gameObject.SetActive(flag4);
					if (flag4)
					{
						SetBarPosition(rectTransform4);
						anims.Add(rectTransform4);
					}
				}
			}
		}
		if (flag2 || num <= 0)
		{
			return;
		}
		currentAnimElapsed += Time.deltaTime;
		if (currentAnimElapsed >= barAnimUpdatedElapsed)
		{
			currentAnimElapsed = 0f;
			currentAnimPosition += barAnimMovementPerElapsed;
			if (currentAnimPosition > (float)barAnimEndPosition)
			{
				currentAnimPosition = num4;
			}
			anims.ForEach(delegate(RectTransform anim)
			{
				SetBarPosition(anim);
			});
		}
	}

	private void CalculateNotEnough()
	{
		if (isNotEnough)
		{
			float alpha = 0f;
			currentNotEnough += Time.deltaTime;
			if (currentNotEnough >= barNotEnoughDuration)
			{
				isNotEnough = false;
				currentNotEnough = 0f;
			}
			else
			{
				alpha = NotEnoughAlphaAnimationCurve.Evaluate(currentNotEnough);
			}
			ChangeImageAlpha(fillNotEnough, alpha);
		}
	}

	private void SetBarPosition(RectTransform bar)
	{
		bar.localPosition = new Vector3(currentAnimPosition, bar.localPosition.y);
	}

	private void ChangeImageAlpha(Image img, float alpha)
	{
		Color color = img.color;
		color.a = alpha;
		img.color = color;
	}

	public void ShowSpark()
	{
		StartCoroutine(ShowSparkCoroutine());
	}

	private IEnumerator ShowSparkCoroutine()
	{
		yield return null;
		Instance.fervourSpark.SetActive(value: true);
		float securityTimeLeft = 2f;
		while (!Mathf.Approximately(fillExact.fillAmount, BarTarget) && securityTimeLeft > 0f)
		{
			securityTimeLeft -= Time.deltaTime;
			float posx = (float)(int)Core.Logic.Penitent.Stats.Fervour.CurrentMaxWithoutFactor * fillExact.fillAmount - 1f;
			fervourSpark.transform.localPosition = new Vector3(posx, fervourSpark.transform.localPosition.y);
			yield return null;
		}
		fervourSpark.SetActive(value: false);
	}
}
