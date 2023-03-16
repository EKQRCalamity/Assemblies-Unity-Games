using UnityEngine;

public class DLCPreLastBossCutscene : DLCGenericCutscene
{
	[SerializeField]
	private TrappedChar trappedChar;

	[SerializeField]
	private GameObject[] trappedChalice;

	[SerializeField]
	private GameObject[] trappedMugman;

	[SerializeField]
	private GameObject[] trappedCuphead;

	[SerializeField]
	private GameObject altText;

	[SerializeField]
	private GameObject altTextTrappedCharCuphead;

	[SerializeField]
	private GameObject altTextTrappedCharMugman;

	protected override void Start()
	{
		base.Start();
		if (trappedChar == TrappedChar.None)
		{
			trappedChar = DetectCharacter();
		}
		switch (trappedChar)
		{
		case TrappedChar.Chalice:
			trappedMugman[0].SetActive(value: false);
			trappedMugman[1].SetActive(value: false);
			trappedCuphead[0].SetActive(value: false);
			trappedCuphead[1].SetActive(value: false);
			break;
		case TrappedChar.Mugman:
			trappedChalice[0].SetActive(value: false);
			trappedChalice[1].SetActive(value: false);
			trappedCuphead[0].SetActive(value: false);
			trappedCuphead[1].SetActive(value: false);
			text[5] = altText;
			text[6] = altTextTrappedCharMugman;
			break;
		case TrappedChar.Cuphead:
			trappedChalice[0].SetActive(value: false);
			trappedChalice[1].SetActive(value: false);
			trappedMugman[0].SetActive(value: false);
			trappedMugman[1].SetActive(value: false);
			text[5] = altText;
			text[6] = altTextTrappedCharCuphead;
			break;
		}
	}

	protected override void OnCutsceneOver()
	{
		SceneLoader.LoadLevel(Levels.Saltbaker, SceneLoader.Transition.Iris);
	}
}
