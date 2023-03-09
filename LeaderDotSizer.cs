using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LeaderDotSizer : MonoBehaviour
{
	private const string Dots = ". . . . . . . . . . . . . . . . . . . . . . .";

	private const float DotsPadding = 5f;

	[SerializeField]
	private TextMeshProUGUI descriptionText;

	[SerializeField]
	private Text leaderDotText;

	private void Start()
	{
		SetLeaderDots();
	}

	private void SetLeaderDots()
	{
		leaderDotText.text = ". . . . . . . . . . . . . . . . . . . . . . .";
		float num = leaderDotText.rectTransform.sizeDelta.x - descriptionText.preferredWidth;
		if (num < 0f)
		{
			leaderDotText.text = string.Empty;
			return;
		}
		int num2 = 100000;
		while (leaderDotText.text.Length > 2 && leaderDotText.preferredWidth > num && num2 > 0)
		{
			num2--;
			leaderDotText.text = leaderDotText.text.Substring(0, leaderDotText.text.Length - 2);
		}
	}
}
