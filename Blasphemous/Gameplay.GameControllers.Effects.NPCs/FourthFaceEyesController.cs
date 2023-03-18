using DG.Tweening;
using Framework.Managers;
using Gameplay.GameControllers.Penitent;
using UnityEngine;

namespace Gameplay.GameControllers.Effects.NPCs;

public class FourthFaceEyesController : MonoBehaviour
{
	public Transform target;

	public float maxDistanceToFollow;

	public float maxHorizontalDisplacement;

	public float maxVerticalDisplacement;

	public float verticalOffset;

	private Vector2 startingPos;

	private Tween horLookTween;

	private Tween verLookTween;

	private void Start()
	{
		startingPos = base.transform.position;
		SpawnManager.OnPlayerSpawn += OnPlayerSpawn;
	}

	private void OnPlayerSpawn(Gameplay.GameControllers.Penitent.Penitent penitent)
	{
		target = penitent.transform;
	}

	private void Update()
	{
		if ((bool)target)
		{
			if (Vector2.Distance(target.position, base.transform.position) > maxDistanceToFollow)
			{
				ReturnToStartingPosition();
			}
			else
			{
				LookAtTarget();
			}
		}
	}

	private void LookAtTarget()
	{
		if (horLookTween == null && verLookTween == null)
		{
			LookHorizontally();
			LookVertically();
		}
	}

	private void LookHorizontally()
	{
		float num = 0f;
		if (target.position.x > startingPos.x)
		{
			float num2 = target.position.x - startingPos.x;
			num = Mathf.Lerp(startingPos.x, startingPos.x + maxHorizontalDisplacement, num2 / maxDistanceToFollow);
		}
		else
		{
			float num3 = startingPos.x - target.position.x;
			num = Mathf.Lerp(startingPos.x, startingPos.x - maxHorizontalDisplacement, num3 / maxDistanceToFollow);
		}
		horLookTween = base.transform.DOMoveX(num, 0.1f).SetEase(Ease.InQuad).OnComplete(delegate
		{
			horLookTween = null;
		});
	}

	private void LookVertically()
	{
		float num = target.position.y + verticalOffset;
		float num2 = 0f;
		if (num > startingPos.y)
		{
			float num3 = num - startingPos.y;
			num2 = Mathf.Lerp(startingPos.y, startingPos.y + maxVerticalDisplacement, num3 / maxDistanceToFollow);
		}
		else
		{
			float num4 = startingPos.y - num;
			num2 = Mathf.Lerp(startingPos.y, startingPos.y - maxVerticalDisplacement, num4 / maxDistanceToFollow);
		}
		verLookTween = base.transform.DOMoveY(num2, 0.1f).SetEase(Ease.InQuad).OnComplete(delegate
		{
			verLookTween = null;
		});
	}

	private void ReturnToStartingPosition()
	{
		if (horLookTween == null && verLookTween == null)
		{
			horLookTween = base.transform.DOMoveX(startingPos.x, 0.5f).SetEase(Ease.InQuad).OnComplete(delegate
			{
				horLookTween = null;
			});
			verLookTween = base.transform.DOMoveY(startingPos.y, 0.5f).SetEase(Ease.InQuad).OnComplete(delegate
			{
				verLookTween = null;
			});
		}
	}
}
