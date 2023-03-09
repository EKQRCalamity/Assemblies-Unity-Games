using System.Collections;
using UnityEngine;

public class FlyingBlimpLevelFadeBackground : ScrollingSprite
{
	public bool fadeOriginal;

	[SerializeField]
	private FlyingBlimpLevelMoonLady moonLady;

	[SerializeField]
	private Transform replacementSprite;

	private SpriteRenderer[] replacementClones;

	private SpriteRenderer current;

	private SpriteRenderer[] currentClones;

	private float fadeTime;

	private bool startedChange;

	protected override void Start()
	{
		base.Start();
		FrameDelayedCallback(DisableSprites, 1);
	}

	private void DisableSprites()
	{
		fadeTime = 10f;
		current = GetComponent<SpriteRenderer>();
		replacementClones = replacementSprite.gameObject.transform.GetComponentsInChildren<SpriteRenderer>();
		currentClones = current.gameObject.transform.GetComponentsInChildren<SpriteRenderer>();
		replacementSprite.transform.position = new Vector2(base.transform.position.x, replacementSprite.transform.position.y);
		replacementSprite.gameObject.GetComponent<SpriteRenderer>().enabled = false;
		for (int i = 0; i < replacementClones.Length; i++)
		{
			replacementClones[i].enabled = false;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (moonLady.state == FlyingBlimpLevelMoonLady.State.Morph && !startedChange)
		{
			startedChange = true;
			StartChange();
		}
	}

	private void StartChange()
	{
		StartCoroutine(change_cr());
	}

	private IEnumerator change_cr()
	{
		float t = 0f;
		float alphaValue = 1f;
		float startSpeed = speed;
		float endSpeed = speed + speed * 0.3f;
		while (t < fadeTime)
		{
			for (int j = 0; j < replacementClones.Length; j++)
			{
				if (replacementClones[j].transform != null)
				{
					replacementClones[j].enabled = true;
					replacementClones[j].color = new Color(1f, 1f, 1f, t / fadeTime);
				}
			}
			if (fadeOriginal)
			{
				for (int i = 0; i < currentClones.Length; i++)
				{
					if (currentClones[i].transform != null)
					{
						currentClones[i].color = new Color(1f, 1f, 1f, alphaValue - t / fadeTime);
						if (alphaValue <= 0f)
						{
							currentClones[i].color = new Color(1f, 1f, 1f, 0f);
							yield return null;
						}
					}
				}
			}
			speed = Mathf.Lerp(startSpeed, endSpeed, t / fadeTime);
			t += (float)CupheadTime.Delta;
			yield return null;
		}
		for (int k = 0; k < replacementClones.Length; k++)
		{
			replacementClones[k].color = new Color(1f, 1f, 1f, 1f);
		}
		if (fadeOriginal)
		{
			for (int l = 0; l < currentClones.Length; l++)
			{
				currentClones[l].enabled = false;
			}
		}
		yield return null;
	}
}
