using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Managers;
using Gameplay.GameControllers.Penitent.Gizmos;
using UnityEngine;

namespace Gameplay.GameControllers.Bosses.BejeweledSaint.Attack;

public class BejeweledSmashHandManager : MonoBehaviour
{
	public BejeweledSmashHand[] SmashHands;

	public RootMotionDriver[] HandsSpawnPoint;

	private readonly List<int> _pickedUpPositions = new List<int>();

	public bool HandsUp { get; set; }

	public bool IsBusy { get; set; }

	private void Start()
	{
		BejeweledSmashHand.OnHandDown = (Core.SimpleEvent)Delegate.Combine(BejeweledSmashHand.OnHandDown, new Core.SimpleEvent(OnHandDown));
	}

	private void OnDestroy()
	{
		BejeweledSmashHand.OnHandDown = (Core.SimpleEvent)Delegate.Remove(BejeweledSmashHand.OnHandDown, new Core.SimpleEvent(OnHandDown));
	}

	private void OnHandDown()
	{
		int num = SmashHands.Count((BejeweledSmashHand x) => x.IsRaised = false);
		if (SmashHands.Length >= num)
		{
			HandsUp = false;
		}
	}

	public Vector3 GetRandomSpawnPoint()
	{
		if (_pickedUpPositions.Count >= HandsSpawnPoint.Length)
		{
			_pickedUpPositions.Clear();
		}
		int num;
		do
		{
			num = UnityEngine.Random.Range(0, HandsSpawnPoint.Length);
		}
		while (_pickedUpPositions.Contains(num));
		_pickedUpPositions.Add(num);
		return HandsSpawnPoint[num].transform.position;
	}

	public void LineAttack(Vector2 origin, Vector2 dir)
	{
		StartCoroutine(LineAttackCoroutine(origin, dir));
	}

	private IEnumerator LineAttackCoroutine(Vector2 origin, Vector2 dir)
	{
		IsBusy = true;
		HandsUp = true;
		float offset = 2.5f;
		float delay = 0.4f;
		for (int i = 0; i < SmashHands.Length; i++)
		{
			BejeweledSmashHand smashHand = SmashHands[i];
			smashHand.transform.position = new Vector2(origin.x, smashHand.transform.position.y) + dir.normalized * offset * i;
			smashHand.AttackAppearing();
			smashHand.IsRaised = true;
			yield return new WaitForSeconds(delay);
		}
		IsBusy = false;
		_pickedUpPositions.Clear();
	}

	public void SmashAttack()
	{
		StartCoroutine(SmashAttackCoroutine());
	}

	private IEnumerator SmashAttackCoroutine()
	{
		HandsUp = true;
		IsBusy = true;
		BejeweledSmashHand[] smashHands = SmashHands;
		foreach (BejeweledSmashHand smashHand in smashHands)
		{
			smashHand.transform.position = new Vector2(GetRandomSpawnPoint().x, smashHand.transform.position.y);
			smashHand.AttackAppearing();
			smashHand.IsRaised = true;
			yield return new WaitForSeconds(1f);
		}
		IsBusy = false;
		_pickedUpPositions.Clear();
	}
}
