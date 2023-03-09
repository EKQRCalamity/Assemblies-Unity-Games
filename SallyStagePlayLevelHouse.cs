using System.Collections;
using UnityEngine;

public class SallyStagePlayLevelHouse : AbstractPausableComponent
{
	[SerializeField]
	private Transform[] windowRoots;

	[SerializeField]
	private SallyStagePlayLevelWindow windowPrefab;

	private SallyStagePlayLevelWindow[] windows;

	private LevelProperties.SallyStagePlay properties;

	private SallyStagePlayLevel parent;

	private const int WINDOW_NUM = 9;

	public void StartPhase2(SallyStagePlayLevel parent, LevelProperties.SallyStagePlay properties)
	{
		SetUp(parent, properties);
	}

	public void StartAttacks()
	{
		if (!SallyStagePlayLevelBackgroundHandler.HUSBAND_GONE)
		{
			StartCoroutine(family_cr());
		}
		else
		{
			StartCoroutine(nuns_cr());
		}
	}

	private void SetUp(SallyStagePlayLevel parent, LevelProperties.SallyStagePlay properties)
	{
		this.parent = parent;
		this.properties = properties;
		parent.OnPhase3 += OnPhase3;
		StartCoroutine(setup_windows_cr());
	}

	private IEnumerator setup_windows_cr()
	{
		Vector3 pos = Vector3.zero;
		int num = 1;
		windows = new SallyStagePlayLevelWindow[9];
		for (int i = 0; i < 9; i++)
		{
			windows[i] = Object.Instantiate(windowPrefab);
			windows[i].transform.position = windowRoots[i].position;
			windows[i].Init(windowRoots[i].position, parent);
			windows[i].transform.parent = base.transform;
			windows[i].windowNum = num + i;
		}
		yield return null;
	}

	private IEnumerator nuns_cr()
	{
		LevelProperties.SallyStagePlay.Nun p = properties.CurrentState.nun;
		string[] windowPattern = p.appearPosition.GetRandom().Split(',');
		int windowPos = 0;
		int pinkStringMainIndex = Random.Range(0, p.pinkString.Length);
		string[] pinkString = p.pinkString[pinkStringMainIndex].Split(',');
		int pinkStringIndex = Random.Range(0, pinkString.Length);
		while (true)
		{
			for (int i = 0; i < windowPattern.Length; i++)
			{
				Parser.IntTryParse(windowPattern[i], out windowPos);
				SallyStagePlayLevelWindow[] array = windows;
				foreach (SallyStagePlayLevelWindow window in array)
				{
					if (window.windowNum == windowPos)
					{
						window.WindowOpenNun(properties, pinkString[pinkStringIndex][0] == 'P');
						if (pinkStringIndex < pinkString.Length - 1)
						{
							pinkStringIndex++;
						}
						else
						{
							pinkStringMainIndex = (pinkStringMainIndex + 1) % p.pinkString.Length;
							pinkStringIndex = 0;
						}
						yield return CupheadTime.WaitForSeconds(this, p.reappearDelayRange.RandomFloat());
					}
				}
			}
			yield return null;
		}
	}

	private IEnumerator family_cr()
	{
		LevelProperties.SallyStagePlay.Baby p = properties.CurrentState.baby;
		string[] windowPattern = p.appearPosition.GetRandom().Split(',');
		int windowPos = 0;
		while (true)
		{
			for (int i = 0; i < windowPattern.Length; i++)
			{
				Parser.IntTryParse(windowPattern[i], out windowPos);
				SallyStagePlayLevelWindow[] array = windows;
				foreach (SallyStagePlayLevelWindow window in array)
				{
					if (window.windowNum == windowPos)
					{
						window.WindowOpenBaby(properties);
						yield return CupheadTime.WaitForSeconds(this, p.hesitate);
						yield return CupheadTime.WaitForSeconds(this, p.reappearDelayRange.RandomFloat());
					}
				}
			}
			yield return null;
		}
	}

	private void OnPhase3()
	{
		StopAllCoroutines();
		Object.Destroy(base.gameObject, 1f);
		parent.OnPhase3 -= OnPhase3;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		windowPrefab = null;
	}
}
