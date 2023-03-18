using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Managers;
using Framework.Pooling;
using Framework.Util;
using Gameplay.GameControllers.AlliedCherub;
using Gameplay.GameControllers.Bosses.Amanecidas;
using Gameplay.GameControllers.Bosses.Isidora;
using Gameplay.GameControllers.Bosses.Snake;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Protection;

public class AlliedCherubSystem : PoolObject
{
	[Serializable]
	public struct AlliedCherubSlot
	{
		public Vector2 Offset;

		public GameObject AlliedCherub;
	}

	public AlliedCherubSlot[] AlliedCherubs;

	public List<Gameplay.GameControllers.AlliedCherub.AlliedCherub> AvailableCherubs = new List<Gameplay.GameControllers.AlliedCherub.AlliedCherub>();

	public List<Enemy> LevelEnemies = new List<Enemy>();

	public List<Enemy> LevelEnemiesChecked = new List<Enemy>();

	private int _storedCherubOnUse;

	public float MinDistanceAttack = 7f;

	public float CheckEnemyLapse = 0.75f;

	private float currentCheckEnemyLapse;

	private List<GameObject> cherubList = new List<GameObject>();

	public bool IsCherubDeployed { get; set; }

	public bool IsEnable { get; set; }

	public event Action<AlliedCherubSystem> OnCherubsDepleted;

	private void Update()
	{
		currentCheckEnemyLapse += Time.deltaTime;
		if (!(currentCheckEnemyLapse > CheckEnemyLapse))
		{
			return;
		}
		currentCheckEnemyLapse = 0f;
		Enemy closerEnemy = GetCloserEnemy();
		if (closerEnemy == null)
		{
			return;
		}
		GameplayUtils.DrawDebugCross(closerEnemy.transform.position, Color.cyan, 3f);
		Debug.Log("LOOKING FOR TARGET");
		bool flag = false;
		while (ExistMoreEnemiesToCheck())
		{
			if (CanAttackEnemy(closerEnemy) && LaunchCherubToEnemy(closerEnemy))
			{
				Debug.Log("FOUND TARGET");
				LevelEnemiesChecked.Clear();
				flag = true;
				break;
			}
			Debug.Log("LOOKING FOR ANOTHER TARGET");
			LevelEnemiesChecked.Add(closerEnemy);
			closerEnemy = GetCloserEnemy();
			if (closerEnemy == null)
			{
				break;
			}
		}
		if (!flag)
		{
			Debug.Log("DIDNT FIND ANY TARGET");
			currentCheckEnemyLapse = CheckEnemyLapse * 0.9f;
			LevelEnemies.Clear();
			LevelEnemiesChecked.Clear();
			FindLevelEnemies();
		}
	}

	private bool ExistMoreEnemiesToCheck()
	{
		return LevelEnemiesChecked.Count < LevelEnemies.Count;
	}

	private bool CanAttackEnemy(Enemy e)
	{
		if (e is Snake)
		{
			Snake snake = e as Snake;
			return snake.IsCurrentlyDamageable();
		}
		return DistanceToEnemy(e) < MinDistanceAttack && e.SpriteRenderer != null && e.SpriteRenderer.isVisible && CanBeDamaged(e);
	}

	private bool CanBeDamaged(Enemy e)
	{
		bool result = !e.IsGuarding;
		if (e is Amanecidas)
		{
			return true;
		}
		return result;
	}

	public override void OnObjectReuse()
	{
		base.OnObjectReuse();
		LevelEnemies.Clear();
		LevelEnemiesChecked.Clear();
		FindLevelEnemies();
	}

	public void DeployCherubs()
	{
		IsCherubDeployed = false;
		IsEnable = true;
		_storedCherubOnUse = 0;
		StartCoroutine(InstantiateCherubCoroutine());
	}

	public bool LaunchCherubToEnemy(Enemy enemy)
	{
		if (AvailableCherubs.Count > 0)
		{
			Gameplay.GameControllers.AlliedCherub.AlliedCherub alliedCherub = AvailableCherubs[AvailableCherubs.Count - 1];
			if (alliedCherub.Behaviour.CanSeeEnemy(enemy))
			{
				AvailableCherubs.Remove(alliedCherub);
				alliedCherub.Behaviour.Attack(enemy);
				Debug.Log("LAUNCHING CHERUB!");
				if (AvailableCherubs.Count == 0)
				{
					OnLastCherubLaunched();
				}
				return true;
			}
			Debug.Log("THEY CAN'T SEE THE ENEMY");
			return false;
		}
		Debug.Log("NO AVAILABLE CHERUBS!");
		return false;
	}

	private void OnLastCherubLaunched()
	{
		if (this.OnCherubsDepleted != null)
		{
			this.OnCherubsDepleted(this);
		}
	}

	private IEnumerator InstantiateCherubCoroutine()
	{
		for (byte i = 0; i < AlliedCherubs.Length; i = (byte)(i + 1))
		{
			GetCherub(AlliedCherubs[i]);
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.75f);
		IsCherubDeployed = true;
	}

	private void FindLevelEnemies()
	{
		Enemy[] array = UnityEngine.Object.FindObjectsOfType<Enemy>();
		foreach (Enemy enemy in array)
		{
			if (!enemy.Status.Dead && !(enemy.tag == "CherubCaptor") && !(enemy is HomingBonfire))
			{
				LevelEnemies.Add(enemy);
			}
		}
	}

	public float DistanceToEnemy(Enemy enemy)
	{
		return Vector2.Distance(Core.Logic.Penitent.transform.position, enemy.transform.position);
	}

	private Enemy GetCloserEnemy()
	{
		float num = float.MaxValue;
		Enemy result = null;
		Vector3 position = Core.Logic.Penitent.transform.position;
		foreach (Enemy levelEnemy in LevelEnemies)
		{
			if (!(levelEnemy == null) && !LevelEnemiesChecked.Contains(levelEnemy))
			{
				float num2 = DistanceToEnemy(levelEnemy);
				if (num2 < num)
				{
					result = levelEnemy;
					num = num2;
				}
			}
		}
		return result;
	}

	public void DisposeSystem()
	{
		foreach (Gameplay.GameControllers.AlliedCherub.AlliedCherub availableCherub in AvailableCherubs)
		{
			availableCherub.Store();
		}
		AvailableCherubs.Clear();
	}

	public void SetCherubsPosition()
	{
	}

	private void GetCherub(AlliedCherubSlot cherubSlot)
	{
		Vector3 position = Core.Logic.Penitent.transform.position;
		GameObject gameObject;
		if (cherubList.Count > AlliedCherubs.Length)
		{
			gameObject = cherubList[cherubList.Count - 1];
			cherubList.Remove(gameObject);
			gameObject.SetActive(value: true);
			gameObject.transform.position = position;
		}
		else
		{
			gameObject = UnityEngine.Object.Instantiate(cherubSlot.AlliedCherub, position, Quaternion.identity);
		}
		Gameplay.GameControllers.AlliedCherub.AlliedCherub componentInChildren = gameObject.GetComponentInChildren<Gameplay.GameControllers.AlliedCherub.AlliedCherub>();
		if (!(componentInChildren == null))
		{
			componentInChildren.Deploy(cherubSlot, this);
			AvailableCherubs.Add(componentInChildren);
		}
	}

	public void StoreCherub(GameObject cherub)
	{
		cherubList.Add(cherub);
		_storedCherubOnUse++;
		cherub.SetActive(value: false);
		if (_storedCherubOnUse >= AlliedCherubs.Length)
		{
			IsEnable = false;
			Destroy();
		}
	}
}
