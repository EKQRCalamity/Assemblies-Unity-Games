using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirplaneLevelBackgroundHandler : MonoBehaviour
{
	[Serializable]
	private struct bgObject
	{
		public string nameForSanity;

		public int startFrame;

		public int duration;

		public int spriteIndex;

		public int layerOffset;
	}

	private int CLOUD_DELAY_FRAMES_MIN = 5;

	private int CLOUD_DELAY_FRAMES_MAX = 10;

	[SerializeField]
	private float frameRate = 30f;

	[SerializeField]
	private Color[] bgColor;

	[SerializeField]
	private SpriteRenderer bgFillSprite;

	[SerializeField]
	private Sprite[] hillsSprites;

	[SerializeField]
	private SpriteRenderer hillsRenderer;

	[SerializeField]
	private bgObject[] objects;

	[SerializeField]
	private SpriteRenderer[] spriteRenderers;

	[SerializeField]
	private Sprite[] objectSprites;

	[SerializeField]
	private SpriteRenderer[] cloudRenderers;

	[SerializeField]
	private Animator[] cloudAnimators;

	[SerializeField]
	private SpriteRenderer[] distantHillsRenderers;

	private float distantHillsTimer;

	[SerializeField]
	private float distantHillsLoopTime = 40f;

	[SerializeField]
	private float distantHillsMaxScale = 3f;

	[SerializeField]
	private float distantHillsMinScale = 0.04f;

	[SerializeField]
	private float distantHillsFadeStartScale = 0.2f;

	private List<Coroutine> groundControllerCoroutine = new List<Coroutine>();

	private List<int> groundControllerCoroutineCurrentObject = new List<int>();

	private int hillsFrameIndex;

	private int prepopulateCounter;

	private float densityWavePosition;

	private float densityWaveRate = 0.5f;

	private void Start()
	{
		hillsFrameIndex = hillsSprites.Length - 1;
		StartCoroutine(main_loop_cr());
		StartCoroutine(cloud_loop_cr());
	}

	private IEnumerator cloud_loop_cr()
	{
		bool[] useAlternate = new bool[8];
		int[] lastCloud = new int[3] { -1, -1, -1 };
		for (int i = 0; i < 4; i++)
		{
			int num = UnityEngine.Random.Range(0, 8);
			if (num != 3 && num != lastCloud[0] && num != lastCloud[1] && num != lastCloud[2])
			{
				cloudRenderers[i].flipX = num >= 4;
				cloudAnimators[i].Play((num % 4 * 2 + ((!useAlternate[num]) ? 1 : 0)).ToString(), 0, UnityEngine.Random.Range(0f, 1f));
				useAlternate[num] = !useAlternate[num];
				lastCloud[2] = lastCloud[1];
				lastCloud[1] = lastCloud[0];
				lastCloud[0] = num;
			}
		}
		while (true)
		{
			int delay = UnityEngine.Random.Range(CLOUD_DELAY_FRAMES_MIN, CLOUD_DELAY_FRAMES_MAX);
			int t = 0;
			while (t < delay)
			{
				if (!CupheadTime.IsPaused())
				{
					t++;
				}
				yield return null;
			}
			int myRenderer = -1;
			for (int j = 0; j < cloudRenderers.Length; j++)
			{
				if (cloudRenderers[j].sprite == null)
				{
					myRenderer = j;
					break;
				}
			}
			if (myRenderer == -1)
			{
				yield return null;
				continue;
			}
			int num2 = UnityEngine.Random.Range(0, 8);
			if (num2 == 3 && (float)UnityEngine.Random.Range(0, 1) < 0.75f)
			{
				num2 += UnityEngine.Random.Range(1, 3) * ((!MathUtils.RandomBool()) ? 1 : (-1));
			}
			if (num2 != lastCloud[0] && num2 != lastCloud[1] && num2 != lastCloud[2])
			{
				cloudRenderers[myRenderer].flipX = num2 >= 4;
				if (num2 == 3)
				{
					cloudRenderers[myRenderer].flipX = MathUtils.RandomBool();
				}
				cloudAnimators[myRenderer].Play((num2 % 4 * 2 + ((!useAlternate[num2]) ? 1 : 0)).ToString(), 0, 0f);
				useAlternate[num2] = !useAlternate[num2];
				lastCloud[2] = lastCloud[1];
				lastCloud[1] = lastCloud[0];
				lastCloud[0] = num2;
			}
		}
	}

	private IEnumerator play_object_cr(int objectNum, int myIndex)
	{
		groundControllerCoroutineCurrentObject.Add(objectNum);
		while (true)
		{
			if (groundControllerCoroutineCurrentObject[myIndex] == -1)
			{
				yield return null;
				continue;
			}
			objectNum = groundControllerCoroutineCurrentObject[myIndex];
			while (hillsFrameIndex < objects[objectNum].startFrame)
			{
				yield return null;
			}
			int myRenderer = -1;
			for (int i = 0; i < spriteRenderers.Length; i++)
			{
				if (spriteRenderers[i].sprite == null)
				{
					myRenderer = i;
					break;
				}
			}
			if (myRenderer != -1)
			{
				int frameCounter = 0;
				int curHillsFrameIndex = hillsFrameIndex;
				for (; frameCounter < objects[objectNum].duration; frameCounter++)
				{
					spriteRenderers[myRenderer].sprite = objectSprites[objects[objectNum].spriteIndex + frameCounter];
					spriteRenderers[myRenderer].sortingOrder = -100 - frameCounter * 2 + objects[objectNum].layerOffset;
					while (curHillsFrameIndex == hillsFrameIndex)
					{
						yield return null;
					}
					curHillsFrameIndex = hillsFrameIndex;
				}
				spriteRenderers[myRenderer].sprite = null;
			}
			groundControllerCoroutineCurrentObject[myIndex] = -1;
		}
	}

	private IEnumerator main_loop_cr()
	{
		bool[] startObject = new bool[objects.Length];
		while (true)
		{
			hillsFrameIndex = (hillsFrameIndex + 1) % hillsSprites.Length;
			if (hillsFrameIndex == 0)
			{
				densityWavePosition += densityWaveRate;
				for (int i = 0; i < objects.Length; i++)
				{
					startObject[i] = UnityEngine.Random.Range(0f, 1f) < 0.4f + Mathf.Sin(densityWavePosition) * 0.2f;
				}
				if (startObject[3] && startObject[5])
				{
					startObject[(!MathUtils.RandomBool()) ? 5 : 3] = false;
				}
				for (int j = 0; j < objects.Length; j++)
				{
					if (!startObject[j])
					{
						continue;
					}
					int num = -1;
					for (int k = 0; k < groundControllerCoroutine.Count; k++)
					{
						if (groundControllerCoroutineCurrentObject[k] == -1)
						{
							num = k;
							break;
						}
					}
					if (num > -1)
					{
						groundControllerCoroutineCurrentObject[num] = j;
					}
					else
					{
						groundControllerCoroutine.Add(StartCoroutine(play_object_cr(j, groundControllerCoroutine.Count)));
					}
				}
			}
			if (prepopulateCounter >= 48)
			{
				yield return new WaitForEndOfFrame();
			}
			hillsRenderer.sprite = hillsSprites[hillsFrameIndex];
			bgFillSprite.color = bgColor[hillsFrameIndex];
			if (prepopulateCounter >= 48)
			{
				yield return CupheadTime.WaitForSeconds(this, 1f / frameRate);
			}
			else
			{
				yield return null;
			}
			prepopulateCounter++;
		}
	}

	private void Update()
	{
		distantHillsTimer += CupheadTime.Delta;
		float num = distantHillsTimer % (distantHillsLoopTime * (float)distantHillsRenderers.Length) / (distantHillsLoopTime * (float)distantHillsRenderers.Length);
		for (int i = 0; i < distantHillsRenderers.Length; i++)
		{
			float num2 = EaseUtils.EaseOutCubic(distantHillsMaxScale, distantHillsMinScale, (num + (float)i * (1f / (float)distantHillsRenderers.Length)) % 1f);
			distantHillsRenderers[i].transform.localScale = new Vector3(num2, num2);
			distantHillsRenderers[i].sortingOrder = -490 - (int)((num * (float)distantHillsRenderers.Length + (float)i) % (float)distantHillsRenderers.Length);
			distantHillsRenderers[i].color = Color.Lerp(Color.black, Color.white, Mathf.InverseLerp(distantHillsFadeStartScale, distantHillsMinScale, num2));
		}
	}
}
