using System.Collections.Generic;
using Framework.FrameworkCore;
using Framework.Managers;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.Player.Dust;

public class WallClimbDust : Trait
{
	public Animator[] WallClimbDustAnimators;

	public SpriteRenderer[] SpriteRenderers;

	public GameObject WallClimbDustPrefab;

	private readonly int _wallClimbDustAnim = Animator.StringToHash("WallClimbEffect");

	private bool _enableAnimators;

	public Core.SimpleEvent OnDustStore;

	private List<GameObject> _climbDustList = new List<GameObject>();

	public void TriggerDust()
	{
		if (!(base.EntityOwner == null))
		{
			FlipSpriteRenderer();
			for (int i = 0; i < WallClimbDustAnimators.Length; i++)
			{
				WallClimbDustAnimators[i].Play(_wallClimbDustAnim, 0, 0f);
			}
			InstantiateClimbDust();
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		EnableAnimators(!base.EntityOwner.Status.IsGrounded);
	}

	public void FlipSpriteRenderer()
	{
		if (base.EntityOwner == null)
		{
			return;
		}
		for (int i = 0; i < SpriteRenderers.Length; i++)
		{
			if (base.EntityOwner.Status.Orientation == EntityOrientation.Left && !SpriteRenderers[i].flipX)
			{
				SpriteRenderers[i].flipX = true;
			}
			else if (base.EntityOwner.Status.Orientation == EntityOrientation.Right && SpriteRenderers[i].flipX)
			{
				SpriteRenderers[i].flipX = false;
			}
		}
	}

	private void EnableAnimators(bool e = true)
	{
		if (_enableAnimators != e)
		{
			for (int i = 0; i < WallClimbDustAnimators.Length; i++)
			{
				WallClimbDustAnimators[i].enabled = e;
			}
			_enableAnimators = e;
		}
	}

	public void InstantiateClimbDust()
	{
		if (!(WallClimbDustPrefab == null))
		{
			GameObject gameObject = null;
			if (_climbDustList.Count > 0)
			{
				gameObject = _climbDustList[_climbDustList.Count - 1];
				_climbDustList.Remove(gameObject);
				gameObject.SetActive(value: true);
				gameObject.transform.position = base.EntityOwner.transform.position;
			}
			else
			{
				gameObject = Object.Instantiate(WallClimbDustPrefab, base.EntityOwner.transform.position, Quaternion.identity);
			}
			if (gameObject != null)
			{
				gameObject.GetComponent<SpriteRenderer>().flipX = base.EntityOwner.Status.Orientation == EntityOrientation.Left;
			}
		}
	}

	public void StoreClimbDust(GameObject cd)
	{
		if (!_climbDustList.Contains(cd))
		{
			_climbDustList.Add(cd);
			cd.SetActive(value: false);
			if (OnDustStore != null)
			{
				OnDustStore();
			}
		}
	}
}
