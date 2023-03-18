using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Framework.Managers;
using Framework.Pooling;
using Gameplay.GameControllers.Bosses.PontiffHusk;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.HighWills.Attack;

public class RangedMineShooter : MonoBehaviour
{
	[Serializable]
	public struct MineData
	{
		public GameObject MinePrefab;

		public float MovementPortionToSpawn;
	}

	[HideInInspector]
	public PontiffHuskBoss PontiffHuskBoss;

	public GameObject CrisantaSimpleVFX;

	public SpriteRenderer SpriteRenderer;

	public List<MineData> MinesData;

	public AnimationCurve VerticalMovementWhileShooting;

	public float MovementTime;

	public float EndPointRelativeHeight;

	public float TimeToFadeAfterFinalMine = 0.25f;

	public float FadeTime = 0.1f;

	public float ParadinhaTimeScale = 0.05f;

	public float ParadinhaDuration = 0.4f;

	private Vector3 startPos;

	private bool shootingMines;

	private float timePassed;

	private int lastMineIndex;

	private List<RangedMine> mines = new List<RangedMine>();

	private bool crisantaAttacked;

	private void Start()
	{
		SpriteRenderer = GetComponent<SpriteRenderer>();
		SpriteRenderer.DOFade(0f, 0f);
	}

	private void Update()
	{
		if (CheckIfAllMinesGotDestroyed())
		{
			LaunchCrisantaAttack();
		}
		if (shootingMines)
		{
			timePassed += Time.deltaTime;
			float portion = timePassed / MovementTime;
			ShootMineIfNeeded(portion);
			UpdatePosition(portion);
		}
	}

	private bool CheckIfAllMinesGotDestroyed()
	{
		return mines.Count == MinesData.Count && !crisantaAttacked && !mines.Exists((RangedMine x) => !x.GotDestroyed);
	}

	private void LaunchCrisantaAttack()
	{
		Core.Logic.ScreenFreeze.Freeze(ParadinhaTimeScale, ParadinhaDuration);
		mines.Clear();
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(CrisantaSimpleVFX, PontiffHuskBoss.gameObject.transform.position, Quaternion.identity, createPoolIfNeeded: true);
		PoolObject componentInChildren = objectInstance.GameObject.GetComponentInChildren<PoolObject>(includeInactive: true);
		componentInChildren.gameObject.SetActive(value: true);
		componentInChildren.OnObjectReuse();
		crisantaAttacked = true;
		StartCoroutine(WaitAndDamageHW());
	}

	private IEnumerator WaitAndDamageHW()
	{
		yield return new WaitForSeconds(0.5f);
	}

	private void ShootMineIfNeeded(float portion)
	{
		int num = lastMineIndex + 1;
		if (!(MinesData[num].MovementPortionToSpawn < portion))
		{
			return;
		}
		lastMineIndex++;
		PoolManager.ObjectInstance objectInstance = PoolManager.Instance.ReuseObject(MinesData[num].MinePrefab, base.transform.position, Quaternion.Euler(0f, 0f, -90f), createPoolIfNeeded: true, MinesData.Count * 3);
		RangedMine component = objectInstance.GameObject.GetComponent<RangedMine>();
		mines.Add(component);
		RangedMine priorMine = null;
		foreach (RangedMine mine in mines)
		{
			mine.SetPriorMine(priorMine);
			priorMine = mine;
		}
		if (num == MinesData.Count - 1)
		{
			shootingMines = false;
			StartCoroutine(WaitAndFade());
		}
	}

	private IEnumerator WaitAndFade()
	{
		yield return new WaitForSeconds(TimeToFadeAfterFinalMine);
		SpriteRenderer.DOFade(0f, FadeTime);
	}

	private void UpdatePosition(float portion)
	{
		float num = VerticalMovementWhileShooting.Evaluate(portion);
		float num2 = num * EndPointRelativeHeight;
		float num3 = timePassed * 3f;
		base.transform.position = startPos + Vector3.up * num2 + Vector3.right * num3;
	}

	public void StartShootingMines(Vector3 startPos)
	{
		this.startPos = startPos;
		base.transform.position = startPos;
		SpriteRenderer.DOFade(1f, 0.1f).OnComplete(ShootMines);
	}

	private void ShootMines()
	{
		shootingMines = true;
		crisantaAttacked = false;
		lastMineIndex = -1;
		timePassed = 0f;
		mines.Clear();
	}
}
