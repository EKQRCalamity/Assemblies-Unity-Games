using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WinScreenGradeDisplay : AbstractMonoBehaviour
{
	private static readonly int[] NormalCurveValues = new int[12]
	{
		0, 38, 28, 65, 25, 36, 8, 28, 26, 40,
		5, 5
	};

	private static readonly int[] NormalCurveOffsets = new int[12]
	{
		0, 21, 16, 37, 17, 23, 6, 17, 15, 22,
		6, 6
	};

	private static readonly int[] GollyCurveValues = new int[12]
	{
		0, 53, 47, 51, 54, 54, 20, 51, 49, 49,
		51, 26
	};

	private static readonly int[] GollyCurveOffsets = new int[12]
	{
		0, 30, 28, 31, 31, 31, 16, 29, 29, 29,
		29, 16
	};

	[SerializeField]
	private Text text;

	[SerializeField]
	private TextMeshProUGUI topGradeLabel;

	[SerializeField]
	private Text topGradeValue;

	[SerializeField]
	private string[] grades;

	[SerializeField]
	private Animator circle;

	[SerializeField]
	private Animator recordBanner;

	[SerializeField]
	private GameObject[] recordEnglish;

	[SerializeField]
	private GameObject[] recordOther;

	[SerializeField]
	private Image recordBannerEnglish;

	[SerializeField]
	private Image recordBannerOther;

	[SerializeField]
	private Animator gollyBanner;

	[SerializeField]
	private GameObject[] gollyEnglish;

	[SerializeField]
	private GameObject[] gollyOther;

	[SerializeField]
	private Image gollyBannerEnglish;

	[SerializeField]
	private Image gollyBannerOther;

	[SerializeField]
	private SpriteRenderer tryRegular;

	[SerializeField]
	private SpriteRenderer tryExpert;

	[SerializeField]
	private GameObject[] normalBannerTexts;

	[SerializeField]
	private GameObject[] topScoreBannerTexts;

	private const float COUNTER_TIME = 0.02f;

	private const float BANNER_FLASH_Y_OFFSET = 2f;

	private CupheadInput.AnyPlayerInput input;

	public LevelScoringData.Grade Grade { get; set; }

	public Level.Mode Difficulty { get; set; }

	public bool Celebration { get; private set; }

	public bool FinishedGrading { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		input = new CupheadInput.AnyPlayerInput();
	}

	private void Start()
	{
		Celebration = false;
		if (Level.PreviouslyWon)
		{
			topGradeLabel.fontStyle = ((Localization.language == Localization.Languages.Korean) ? FontStyles.Bold : topGradeLabel.fontStyle);
			topGradeValue.text = " " + grades[(int)Level.PreviousGrade];
		}
	}

	public void Show()
	{
		StartCoroutine(grade_tally_up_cr());
	}

	private IEnumerator grade_tally_up_cr()
	{
		bool isTallying = true;
		float t = 0f;
		int counter = 0;
		text.text = grades[grades.Length - 1].Substring(0, 1) + " ";
		while (counter <= (int)Grade && isTallying && counter < (int)Grade)
		{
			AudioManager.Play("win_score_tick");
			counter++;
			text.text = grades[counter].Substring(0, 1) + " ";
			while (t < 0.02f)
			{
				if (input.GetButtonDown(CupheadButton.Accept))
				{
					isTallying = false;
					break;
				}
				t += (float)CupheadTime.Delta;
				yield return null;
			}
			t = 0f;
		}
		AudioManager.Play("win_grade_chalk");
		circle.SetTrigger("Circle");
		text.GetComponent<Animator>().SetTrigger("MakeBig");
		text.text = grades[(int)Grade];
		if (counter == grades.Length - 1)
		{
			text.color = ColorUtils.HexToColor("FCC93D");
		}
		LevelScoringData.Grade PerfectGrade = ((Difficulty != Level.Mode.Hard) ? LevelScoringData.Grade.APlus : LevelScoringData.Grade.S);
		bool english = Localization.language == Localization.Languages.English;
		if (!english)
		{
			AlignBannerText();
		}
		if (!Level.IsTowerOfPower)
		{
			if (Grade == PerfectGrade)
			{
				StartCoroutine(fade_text_cr());
				yield return CupheadTime.WaitForSeconds(this, 0.16f);
				gollyBanner.SetTrigger("OnBanner");
				Celebration = true;
				LanguageUpdate(english);
				gollyBannerEnglish.enabled = english;
				gollyBannerOther.enabled = !english;
				yield return gollyBanner.WaitForAnimationToEnd(this, "Golly");
			}
			else if (Grade > Level.PreviousGrade || !Level.PreviouslyWon)
			{
				StartCoroutine(fade_text_cr());
				yield return CupheadTime.WaitForSeconds(this, 0.16f);
				recordBanner.SetTrigger("OnBanner");
				Celebration = true;
				LanguageUpdate(english);
				recordBannerEnglish.enabled = english;
				recordBannerOther.enabled = !english;
				yield return recordBanner.WaitForAnimationToEnd(this, "Record");
			}
		}
		if (Level.IsTowerOfPower && (int)Grade >= TowerOfPowerLevelGameInfo.MIN_RANK_NEED_TO_GET_TOKEN)
		{
			TowerOfPowerLevelGameInfo.AddToken();
		}
		FinishedGrading = true;
		yield return null;
	}

	private void AlignBannerText()
	{
		for (int i = 0; i < normalBannerTexts.Length; i++)
		{
			normalBannerTexts[i].GetComponent<TextMeshCurveAndJitter>().CurveScale = NormalCurveValues[(int)Localization.language];
			Vector3 localPosition = normalBannerTexts[i].transform.localPosition;
			localPosition.y = -NormalCurveOffsets[(int)Localization.language];
			if (i == normalBannerTexts.Length - 1)
			{
				localPosition.y += 2f;
			}
			normalBannerTexts[i].transform.localPosition = localPosition;
		}
		for (int j = 0; j < topScoreBannerTexts.Length; j++)
		{
			topScoreBannerTexts[j].GetComponent<TextMeshCurveAndJitter>().CurveScale = GollyCurveValues[(int)Localization.language];
			Vector3 localPosition2 = topScoreBannerTexts[j].transform.localPosition;
			localPosition2.y = -GollyCurveOffsets[(int)Localization.language];
			if (j == topScoreBannerTexts.Length - 1)
			{
				localPosition2.y -= 2f;
			}
			topScoreBannerTexts[j].transform.localPosition = localPosition2;
		}
	}

	private void LanguageUpdate(bool english)
	{
		for (int i = 0; i < recordEnglish.Length; i++)
		{
			recordEnglish[i].SetActive(english);
		}
		for (int j = 0; j < gollyEnglish.Length; j++)
		{
			gollyEnglish[j].SetActive(english);
		}
		for (int k = 0; k < recordOther.Length; k++)
		{
			recordOther[k].SetActive(!english);
		}
		for (int l = 0; l < gollyOther.Length; l++)
		{
			gollyOther[l].SetActive(!english);
		}
	}

	private IEnumerator fade_text_cr()
	{
		float t = 0f;
		float fadeTime = 0.29f;
		Color topGradeLabelColor = topGradeLabel.color;
		Color topGradeValColor = topGradeValue.color;
		while (t < fadeTime)
		{
			t += (float)CupheadTime.Delta;
			topGradeLabel.color = new Color(topGradeLabelColor.r, topGradeLabelColor.g, topGradeLabelColor.b, 1f - t / fadeTime);
			topGradeValue.color = new Color(topGradeValColor.r, topGradeValColor.g, topGradeValColor.b, 1f - t / fadeTime);
			if (tryExpert.gameObject.activeSelf)
			{
				SpriteRenderer[] componentsInChildren = tryExpert.GetComponentsInChildren<SpriteRenderer>();
				foreach (SpriteRenderer spriteRenderer in componentsInChildren)
				{
					spriteRenderer.color = new Color(1f, 1f, 1f, 1f - t / fadeTime);
				}
				RawImage[] componentsInChildren2 = tryExpert.GetComponentsInChildren<RawImage>();
				foreach (RawImage rawImage in componentsInChildren2)
				{
					rawImage.color = new Color(1f, 1f, 1f, 1f - t / fadeTime);
				}
				TextMeshCurveAndJitter[] componentsInChildren3 = tryExpert.GetComponentsInChildren<TextMeshCurveAndJitter>();
				foreach (TextMeshCurveAndJitter textMeshCurveAndJitter in componentsInChildren3)
				{
					float value = Mathf.Clamp(255f - t / fadeTime * 255f, 0f, 255f);
					textMeshCurveAndJitter.AlphaValue = Convert.ToByte(value);
				}
			}
			if (tryRegular.gameObject.activeSelf)
			{
				SpriteRenderer[] componentsInChildren4 = tryRegular.GetComponentsInChildren<SpriteRenderer>();
				foreach (SpriteRenderer spriteRenderer2 in componentsInChildren4)
				{
					spriteRenderer2.color = new Color(1f, 1f, 1f, 1f - t / fadeTime);
				}
				RawImage[] componentsInChildren5 = tryRegular.GetComponentsInChildren<RawImage>();
				foreach (RawImage rawImage2 in componentsInChildren5)
				{
					rawImage2.color = new Color(1f, 1f, 1f, 1f - t / fadeTime);
				}
				TextMeshCurveAndJitter[] componentsInChildren6 = tryRegular.GetComponentsInChildren<TextMeshCurveAndJitter>();
				foreach (TextMeshCurveAndJitter textMeshCurveAndJitter2 in componentsInChildren6)
				{
					float value2 = Mathf.Clamp(255f - t / fadeTime * 255f, 0f, 255f);
					textMeshCurveAndJitter2.AlphaValue = Convert.ToByte(value2);
				}
			}
			yield return null;
		}
		yield return null;
	}
}
