using UnityEngine;

namespace Gameplay.GameControllers.Enemies.WaxCrawler.Animator;

public class WaxCrawlerAnimatorBridge : MonoBehaviour
{
	public WaxCrawler WaxCrawler;

	private void Start()
	{
		if (WaxCrawler == null)
		{
			Debug.LogError("You need a waxcrawler entity");
		}
	}

	public void PlayWalk()
	{
		if (!(WaxCrawler == null))
		{
			WaxCrawler.Audio.Walk();
		}
	}

	public void PlayAppear()
	{
		if (!(WaxCrawler == null))
		{
			WaxCrawler.Audio.Appear();
		}
	}

	public void PlayDisappeaar()
	{
		if (!(WaxCrawler == null))
		{
			WaxCrawler.Audio.Disappear();
		}
	}
}
