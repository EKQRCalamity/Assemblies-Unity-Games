using UnityEngine;

public class LevelScoringData
{
	public enum Grade
	{
		DMinus,
		D,
		DPlus,
		CMinus,
		C,
		CPlus,
		BMinus,
		B,
		BPlus,
		AMinus,
		A,
		APlus,
		S,
		P
	}

	public float time;

	public float goalTime;

	public int finalHP;

	public int numTimesHit;

	public int numParries;

	public int superMeterUsed;

	public int coinsCollected;

	public Level.Mode difficulty;

	public bool pacifistRun;

	public bool useCoinsInsteadOfSuperMeter;

	public bool usedDjimmi;

	public bool player1IsChalice;

	public bool player2IsChalice;

	public Grade CalculateGrade()
	{
		if (pacifistRun && !usedDjimmi)
		{
			return Grade.P;
		}
		ScoringEditorData scoringProperties = Cuphead.Current.ScoringProperties;
		float num = Mathf.Clamp(100f - 100f * (time - goalTime) / (goalTime * (scoringProperties.bestTimeMultiplierForNoScore - 1f)), 0f, 100f);
		float num2 = Mathf.Clamp(100f - 100f * ((scoringProperties.hitsForNoScore - (float)finalHP) / scoringProperties.hitsForNoScore), 0f, 100f);
		float num3 = 100f * Mathf.Min(numParries, scoringProperties.parriesForHighestGrade + scoringProperties.bonusParries) / scoringProperties.parriesForHighestGrade;
		float num4 = 100f * Mathf.Min(superMeterUsed, scoringProperties.superMeterUsageForHighestGrade + scoringProperties.bonusSuperMeterUsage) / scoringProperties.superMeterUsageForHighestGrade;
		if (useCoinsInsteadOfSuperMeter)
		{
			num4 = 100f * ((float)coinsCollected / 5f);
		}
		float num5 = num * scoringProperties.timeWeight + num2 * scoringProperties.hitsWeight + num3 * scoringProperties.parriesWeight + num4 * scoringProperties.superMeterUsageWeight;
		ScoringEditorData.GradingCurveEntry[] array = ((difficulty == Level.Mode.Easy) ? scoringProperties.easyGradingCurve : ((difficulty != Level.Mode.Normal) ? scoringProperties.hardGradingCurve : scoringProperties.mediumGradingCurve));
		Grade grade = Grade.DMinus;
		ScoringEditorData.GradingCurveEntry[] array2 = array;
		foreach (ScoringEditorData.GradingCurveEntry gradingCurveEntry in array2)
		{
			grade = gradingCurveEntry.grade;
			if (num5 < gradingCurveEntry.upperLimit)
			{
				break;
			}
		}
		if (usedDjimmi && grade > Grade.BPlus)
		{
			grade = Grade.BPlus;
		}
		return grade;
	}
}
