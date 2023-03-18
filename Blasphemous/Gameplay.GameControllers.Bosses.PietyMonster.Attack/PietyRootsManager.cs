using System.Collections;
using System.Collections.Generic;
using Framework.FrameworkCore;
using Gameplay.GameControllers.Entities;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.PietyMonster.Attack;

public class PietyRootsManager : MonoBehaviour
{
	private List<GameObject> _pietyRoots = new List<GameObject>();

	public LayerMask TargetLayer;

	public GameObject PietyRootPrefab;

	public float MinSpawnRootDistance = 3f;

	public float MinDistanceBetweenFollowingRoots = 2f;

	private List<Vector2> _lastRootPositions = new List<Vector2>();

	[Range(0f, 2f)]
	public float DominoRootsOffset = 1.5f;

	public PietyMonster PietyMonster { get; set; }

	public BoxCollider2D Collider { get; set; }

	public GameObject Target { get; set; }

	public float RootDamage { get; set; }

	private void Start()
	{
		Collider = GetComponent<BoxCollider2D>();
		if (PietyRootPrefab == null)
		{
			Debug.LogError("A Piety Monster's root prefab is needed");
		}
	}

	private void Update()
	{
		if (PietyMonster == null)
		{
			PietyMonster = Object.FindObjectOfType<PietyMonster>();
		}
	}

	public void EnableNearestRoots()
	{
		float missingRatio = PietyMonster.Stats.Life.MissingRatio;
		int rootsAmount = ((!((double)missingRatio >= 0.5)) ? 3 : 2);
		StartCoroutine(SpawnFollowingRoots(rootsAmount));
	}

	public void EnableDominoRoots()
	{
		float missingRatio = PietyMonster.Stats.Life.MissingRatio;
		int rootsAmount = ((!(missingRatio >= 0.5f)) ? 5 : 3);
		StartCoroutine(SpawnLeftDominoRoots(rootsAmount));
		StartCoroutine(SpawnRighDominoRoots(rootsAmount));
	}

	private IEnumerator SpawnFollowingRoots(int rootsAmount)
	{
		float yPos = Collider.bounds.min.y;
		for (int i = 0; i < rootsAmount; i++)
		{
			float xPos = GetTargetXPosition();
			Vector2 rootPosition = new Vector2(xPos, yPos);
			if (!RootIsTooCloseToLastFollowingRoot(rootPosition))
			{
				if (!RootIsTooCloseToBoss(rootPosition))
				{
					GetRoot(rootPosition);
					_lastRootPositions.Add(rootPosition);
				}
			}
			else
			{
				Vector3 nearestRootToLastRoot = GetNearestRootToLastRoot(rootPosition);
				if (!RootIsTooCloseToBoss(nearestRootToLastRoot))
				{
					GetRoot(nearestRootToLastRoot);
					_lastRootPositions.Add(nearestRootToLastRoot);
				}
			}
			yield return new WaitForSeconds(0.5f);
		}
		_lastRootPositions.Clear();
	}

	public float GetTargetXPosition()
	{
		if (Target == null)
		{
			return base.transform.position.x;
		}
		float num = Mathf.Floor(Target.transform.position.x * 32f) / 32f;
		if (num >= Collider.bounds.max.x)
		{
			num = Collider.bounds.max.x - 0.5f;
		}
		else if (num <= Collider.bounds.min.x)
		{
			num = Collider.bounds.min.x + 0.5f;
		}
		return num;
	}

	private IEnumerator SpawnRighDominoRoots(int rootsAmount)
	{
		float startXPos = GetDominoRightStartPos();
		float yPos = Collider.bounds.min.y;
		float rightXBoundary = Collider.bounds.max.x;
		for (int i = 0; i < rootsAmount && !(startXPos + (float)i > rightXBoundary); i++)
		{
			Vector2 rootPos = new Vector2(startXPos + (float)i * DominoRootsOffset, yPos);
			GetRoot(rootPos);
			yield return new WaitForSeconds(0.1f);
		}
	}

	private IEnumerator SpawnLeftDominoRoots(int rootsAmount)
	{
		float startXPos = GetDominoLeftStartPos();
		float yPos = Collider.bounds.min.y;
		float leftXBoundary = Collider.bounds.min.x;
		WaitForSeconds delay = new WaitForSeconds(0.1f);
		for (int i = 0; i < rootsAmount && !(startXPos - (float)i < leftXBoundary); i++)
		{
			Vector2 rootPos = new Vector2(startXPos - (float)i * DominoRootsOffset, yPos);
			GetRoot(rootPos);
			yield return delay;
		}
	}

	private bool RootIsTooCloseToBoss(Vector2 rootPos)
	{
		bool result = false;
		Vector3 position = PietyMonster.transform.position;
		if (PietyMonster.Status.Orientation == EntityOrientation.Left)
		{
			if (rootPos.x < position.x && Vector2.Distance(position, rootPos) < MinSpawnRootDistance)
			{
				result = true;
			}
		}
		else if (rootPos.x > position.x && Vector2.Distance(position, rootPos) < MinSpawnRootDistance)
		{
			result = true;
		}
		return result;
	}

	private bool RootIsTooCloseToLastFollowingRoot(Vector2 rootPos)
	{
		bool result = false;
		if (_lastRootPositions.Count > 0)
		{
			Vector2 b = _lastRootPositions[_lastRootPositions.Count - 1];
			float num = Vector2.Distance(rootPos, b);
			result = num < MinDistanceBetweenFollowingRoots;
		}
		return result;
	}

	private Vector3 GetNearestRootToLastRoot(Vector2 targetPos)
	{
		Vector2 vector = targetPos;
		Vector2 vector2 = _lastRootPositions[_lastRootPositions.Count - 1];
		if (vector2.x >= targetPos.x)
		{
			float x = ((!(vector2.x - MinDistanceBetweenFollowingRoots > Collider.bounds.min.x)) ? (vector2.x + MinDistanceBetweenFollowingRoots) : (vector2.x - MinDistanceBetweenFollowingRoots));
			vector = new Vector2(x, vector2.y);
		}
		else
		{
			float x2 = ((!(vector2.x + MinDistanceBetweenFollowingRoots > Collider.bounds.max.x)) ? (vector2.x + MinDistanceBetweenFollowingRoots) : (vector2.x - MinDistanceBetweenFollowingRoots));
			vector = new Vector2(x2, vector2.y);
		}
		return vector;
	}

	public float GetDominoRightStartPos()
	{
		return PietyMonster.PietyBehaviour.SmashAttack.PietySmash.AttackAreas[0].WeaponCollider.bounds.max.x;
	}

	public float GetDominoLeftStartPos()
	{
		return PietyMonster.PietyBehaviour.SmashAttack.PietySmash.AttackAreas[0].WeaponCollider.bounds.min.x;
	}

	public void GetRoot(Vector2 rootPosition)
	{
		if (_pietyRoots.Count > 0)
		{
			GameObject gameObject = _pietyRoots[_pietyRoots.Count - 1];
			_pietyRoots.Remove(gameObject);
			gameObject.SetActive(value: true);
			gameObject.transform.position = rootPosition;
		}
		else
		{
			GameObject gameObject2 = Object.Instantiate(PietyRootPrefab, rootPosition, Quaternion.identity);
			PietyRoot component = gameObject2.GetComponent<PietyRoot>();
			AttackArea component2 = gameObject2.GetComponent<AttackArea>();
			component2.Entity = PietyMonster;
			component.Manager = this;
		}
	}

	public void StoreRoot(GameObject pietyRoot)
	{
		if (!_pietyRoots.Contains(pietyRoot))
		{
			_pietyRoots.Add(pietyRoot);
		}
	}

	private void OnTriggerEnter2D(Collider2D target)
	{
		if ((TargetLayer.value & (1 << target.gameObject.layer)) > 0)
		{
			Target = target.gameObject;
		}
	}

	public void EnablePietyRoot(PietyRoot pietyRoot)
	{
		if (!pietyRoot.gameObject.activeSelf)
		{
			pietyRoot.gameObject.SetActive(value: true);
		}
	}

	public void DisablePietyRoot(PietyRoot pietyRoot)
	{
		if (pietyRoot.gameObject.activeSelf)
		{
			pietyRoot.gameObject.SetActive(value: false);
		}
	}
}
