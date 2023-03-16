using UnityEngine;

public class LevelPlayerChaliceIntroAnimation : Effect
{
	[SerializeField]
	private GameObject cuphead;

	[SerializeField]
	private GameObject mugman;

	public LevelPlayerChaliceIntroAnimation Create(Vector3 position, bool isMugman, bool isScared)
	{
		LevelPlayerChaliceIntroAnimation levelPlayerChaliceIntroAnimation = base.Create(position) as LevelPlayerChaliceIntroAnimation;
		levelPlayerChaliceIntroAnimation.SetSprites(isMugman);
		string stateName = "Intro_CH_MM" + ((!isScared) ? "_Hold" : "_Scared");
		levelPlayerChaliceIntroAnimation.animator.Play(stateName);
		return levelPlayerChaliceIntroAnimation;
	}

	public void EndHold()
	{
		base.animator.SetTrigger("Continue");
	}

	private void SetSprites(bool isMugman)
	{
		if (Level.Current.CurrentLevel == Levels.Saltbaker)
		{
			cuphead.SetActive(value: false);
			mugman.SetActive(value: false);
		}
		else
		{
			cuphead.SetActive(!isMugman);
			mugman.SetActive(isMugman);
		}
	}
}
