using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OldManLevelPlatformManager : LevelProperties.OldMan.Entity
{
	private const float PLATFORM_EXIT_SPEED = 10f;

	[SerializeField]
	private OldManLevelPlatform[] allPlatforms;

	[SerializeField]
	private Animator[] beardSettles;

	[SerializeField]
	private SpriteRenderer mainBeardTufts;

	[SerializeField]
	private float wobbleBeforeRemoveTime = 1f;

	private bool inPhaseOne = true;

	private float lastPos;

	private List<int> removeOrderList = new List<int>();

	private List<float> removeThresholdList = new List<float>();

	public Vector3[] GetPlatformPositions()
	{
		Vector3[] array = new Vector3[allPlatforms.Length];
		for (int i = 0; i < allPlatforms.Length; i++)
		{
			ref Vector3 reference = ref array[i];
			reference = allPlatforms[i].platform.position;
		}
		return array;
	}

	public Transform GetPlatform(int i)
	{
		if (allPlatforms[i] == null)
		{
			return null;
		}
		return allPlatforms[i].platform.transform;
	}

	public bool PlatformRemoved(int which)
	{
		return allPlatforms[which].removed;
	}

	public override void LevelInit(LevelProperties.OldMan properties)
	{
		base.LevelInit(properties);
		for (int i = 0; i < allPlatforms.Length; i++)
		{
			allPlatforms[i].platform.SetPosition(null, properties.CurrentState.platforms.minHeight);
		}
		StartCoroutine(handle_platforms_cr());
		StartCoroutine(handle_remove_platforms_cr());
	}

	public void EndPhase()
	{
		inPhaseOne = false;
		mainBeardTufts.enabled = false;
		beardSettles[0].transform.parent.gameObject.SetActive(value: true);
	}

	private IEnumerator handle_remove_platforms_cr()
	{
		float bossHealthMax = base.properties.CurrentHealth;
		float bossHealthMin = bossHealthMax * base.properties.GetNextStateHealthTrigger();
		int currentCount = 0;
		string[] removeOrder = base.properties.CurrentState.platforms.removeOrder[UnityEngine.Random.Range(0, base.properties.CurrentState.platforms.removeOrder.Length)].Split(',');
		string[] removeThreshold = base.properties.CurrentState.platforms.removeThreshold.Split(',');
		if (removeOrder.Length != removeThreshold.Length)
		{
			Debug.Break();
		}
		if (removeOrder.Length == 0)
		{
			yield break;
		}
		for (int i = 0; i < removeOrder.Length; i++)
		{
			int result = 0;
			Parser.IntTryParse(removeOrder[i], out result);
			float result2 = 0f;
			Parser.FloatTryParse(removeThreshold[i], out result2);
			removeOrderList.Add(result);
			removeThresholdList.Add(result2);
		}
		while (currentCount < removeThresholdList.Count)
		{
			float t = Mathf.InverseLerp(bossHealthMax, bossHealthMin, base.properties.CurrentHealth);
			if (t > removeThresholdList[currentCount])
			{
				RemovePlatform(removeOrderList[currentCount]);
				currentCount++;
			}
			yield return null;
		}
	}

	private IEnumerator handle_platforms_cr()
	{
		yield return CupheadTime.WaitForSeconds(this, 3f);
		LevelProperties.OldMan.Platforms p = base.properties.CurrentState.platforms;
		int orderMainIndex = UnityEngine.Random.Range(0, p.moveOrder.Length);
		string[] orderString = p.moveOrder[orderMainIndex].Split(',');
		int orderIndex = UnityEngine.Random.Range(0, orderString.Length);
		bool skipPlatform2 = false;
		bool stoppedMoving = false;
		while (!stoppedMoving)
		{
			if (!inPhaseOne)
			{
				stoppedMoving = true;
				yield return null;
			}
			skipPlatform2 = false;
			orderString = p.moveOrder[orderMainIndex].Split(',');
			string[] spawnOrder = orderString[orderIndex].Split('-');
			string[] array = spawnOrder;
			foreach (string s in array)
			{
				int result = 0;
				Parser.IntTryParse(s, out result);
				if (allPlatforms[result].isMoving)
				{
					skipPlatform2 = true;
				}
				else
				{
					StartCoroutine(move_platform_cr(allPlatforms[result]));
				}
			}
			if (!skipPlatform2)
			{
				yield return CupheadTime.WaitForSeconds(this, p.delayRange.RandomFloat());
			}
			if (orderIndex < orderString.Length - 1)
			{
				orderIndex++;
			}
			else
			{
				orderMainIndex = (orderMainIndex + 1) % p.moveOrder.Length;
				orderIndex = 0;
			}
			yield return null;
		}
		StartCoroutine(end_phase_cr());
	}

	private IEnumerator end_phase_cr()
	{
		List<int> order = new List<int>(5);
		for (int j = 0; j < 5; j++)
		{
			if (!allPlatforms[j].removed)
			{
				order.Add(j);
			}
		}
		for (int k = 0; k < order.Count; k++)
		{
			int value = order[k];
			int index = UnityEngine.Random.Range(0, order.Count);
			order[k] = order[index];
			order[index] = value;
		}
		for (int i = 0; i < order.Count; i++)
		{
			StartCoroutine(slide_out_cr(allPlatforms[order[i]]));
			yield return CupheadTime.WaitForSeconds(this, 0.1f);
		}
	}

	public void RemovePlatform(int which)
	{
		allPlatforms[which].removed = true;
		mainBeardTufts.enabled = false;
		beardSettles[0].transform.parent.gameObject.SetActive(value: true);
		StartCoroutine(slide_out_cr(allPlatforms[which]));
	}

	public void AttachGnome(int which, OldManLevelGnomeClimber c)
	{
		allPlatforms[which].activeClimber = c;
	}

	private IEnumerator move_platform_cr(OldManLevelPlatform movingPlatform)
	{
		LevelProperties.OldMan.Platforms p = base.properties.CurrentState.platforms;
		float t = 0f;
		float time = p.moveTime / 2f;
		movingPlatform.isMoving = true;
		while (t < time && inPhaseOne && !movingPlatform.removed)
		{
			t += (float)CupheadTime.Delta;
			float val = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, 0f, 1f, t / time);
			float lastPos = movingPlatform.platform.transform.position.y;
			movingPlatform.platform.SetPosition(null, Mathf.Lerp(p.minHeight, p.maxHeight, val));
			movingPlatform.effectiveVel = movingPlatform.platform.transform.position.y - lastPos;
			yield return null;
		}
		if (inPhaseOne && !movingPlatform.removed)
		{
			t = 0f;
			movingPlatform.platform.SetPosition(null, p.maxHeight);
		}
		while (t < time && inPhaseOne && !movingPlatform.removed)
		{
			t += (float)CupheadTime.Delta;
			float val2 = EaseUtils.Ease(EaseUtils.EaseType.easeInOutSine, 0f, 1f, t / time);
			float lastPos2 = movingPlatform.platform.transform.position.y;
			movingPlatform.platform.SetPosition(null, Mathf.Lerp(p.maxHeight, p.minHeight, val2));
			movingPlatform.effectiveVel = movingPlatform.platform.transform.position.y - lastPos2;
			yield return null;
		}
		if (inPhaseOne && !movingPlatform.removed)
		{
			movingPlatform.platform.SetPosition(null, p.minHeight);
			movingPlatform.isMoving = false;
		}
		yield return null;
	}

	private IEnumerator slide_out_cr(OldManLevelPlatform movingPlatform)
	{
		LevelProperties.OldMan.Platforms p = base.properties.CurrentState.platforms;
		int id = 4 - Array.IndexOf(allPlatforms, movingPlatform);
		YieldInstruction wait = new WaitForFixedUpdate();
		float moveHeight = p.minHeight * 2f - 50f;
		if (movingPlatform.effectiveVel > 0f)
		{
			movingPlatform.effectiveVel *= 0.5f;
		}
		float t = wobbleBeforeRemoveTime;
		while (t > 0f || (bool)movingPlatform.activeClimber)
		{
			t -= (float)CupheadTime.Delta;
			movingPlatform.platform.transform.GetChild(0).transform.GetChild(0).localPosition = new Vector3(Mathf.Sin(t * 100f) * 2.5f, 0f);
			yield return null;
		}
		movingPlatform.platform.transform.GetChild(0).transform.GetChild(0).localPosition = Vector3.zero;
		while (movingPlatform.platform.transform.position.y > moveHeight)
		{
			if (!CupheadTime.IsPaused())
			{
				movingPlatform.platform.SetPosition(null, Mathf.Clamp(movingPlatform.platform.transform.position.y + movingPlatform.effectiveVel, -1000f, 117f));
			}
			movingPlatform.effectiveVel -= 10f * CupheadTime.FixedDelta;
			if (movingPlatform.platform.transform.position.y < -384f)
			{
				beardSettles[id].Play("Settle");
			}
			yield return wait;
		}
		if ((bool)movingPlatform.platform.GetComponentInChildren<LevelPlayerController>())
		{
			LevelPlayerController[] componentsInChildren = movingPlatform.platform.GetComponentsInChildren<LevelPlayerController>();
			LevelPlayerController[] array = componentsInChildren;
			foreach (LevelPlayerController levelPlayerController in array)
			{
				levelPlayerController.transform.parent = null;
			}
		}
		UnityEngine.Object.Destroy(movingPlatform.platform.gameObject);
		yield return null;
	}
}
