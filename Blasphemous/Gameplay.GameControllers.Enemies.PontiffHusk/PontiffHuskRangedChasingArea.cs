using UnityEngine;

namespace Gameplay.GameControllers.Enemies.PontiffHusk;

[RequireComponent(typeof(Collider2D))]
public class PontiffHuskRangedChasingArea : MonoBehaviour
{
	public float alertTime = 2f;

	private float deltaAlertTime;

	private bool playerEnterInChasingArea;

	public LayerMask playerLayer;

	public bool ChasingPlayer { get; private set; }

	private void Awake()
	{
	}

	private void Start()
	{
		ChasingPlayer = false;
	}

	private void Update()
	{
		if (playerEnterInChasingArea)
		{
			deltaAlertTime += Time.deltaTime;
			if (deltaAlertTime >= alertTime && !ChasingPlayer)
			{
				ChasingPlayer = true;
			}
		}
		else
		{
			deltaAlertTime = 0f;
			ChasingPlayer = false;
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if ((playerLayer.value & (1 << collision.gameObject.layer)) > 0 && !playerEnterInChasingArea)
		{
			playerEnterInChasingArea = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if ((playerLayer.value & (1 << collision.gameObject.layer)) > 0 && playerEnterInChasingArea)
		{
			playerEnterInChasingArea = !playerEnterInChasingArea;
		}
	}
}
