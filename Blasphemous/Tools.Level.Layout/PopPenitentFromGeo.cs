using Framework.Managers;
using UnityEngine;

namespace Tools.Level.Layout;

public class PopPenitentFromGeo : MonoBehaviour
{
	private const float MIN_SECONDS_BEFORE_POPUP = 5f;

	private const float V_THRESHOLD_FOR_CHECKING = 0.5f;

	private const float Y_OFFSET_TO_POP_UP = 0.05f;

	private const float X_OFFSET_TO_POP_SIDEWAYS = 0.5f;

	private BoxCollider2D boxCollider;

	private bool checkToPopUpPenitent;

	private float currentSecondsOnScene;

	private RaycastHit2D[] raycastHits;

	private void Awake()
	{
		boxCollider = GetComponent<BoxCollider2D>();
	}

	private void Start()
	{
		checkToPopUpPenitent = ShouldCheckToPopUpPenitent();
		raycastHits = new RaycastHit2D[1];
	}

	private void OnEnable()
	{
		currentSecondsOnScene = 0f;
	}

	private bool ShouldCheckToPopUpPenitent()
	{
		return (bool)boxCollider && (LayerMask.LayerToName(base.gameObject.layer).Equals("Floor") || LayerMask.LayerToName(base.gameObject.layer).Equals("Wall"));
	}

	private void FixedUpdate()
	{
		if (!Core.ready || !Core.Logic.Penitent || !checkToPopUpPenitent)
		{
			return;
		}
		if (currentSecondsOnScene < 5f)
		{
			currentSecondsOnScene += Time.deltaTime;
		}
		else
		{
			if (!boxCollider.OverlapPoint(Core.Logic.Penitent.GetPosition() + Vector3.up * 0.5f))
			{
				return;
			}
			if (boxCollider.size.x >= boxCollider.size.y)
			{
				Vector2 popUpPoint = GetPopUpPoint();
				if (RaycastToCheckForGeo(popUpPoint, Vector2.up))
				{
					PopPenitentSideways();
				}
				else
				{
					Core.Logic.Penitent.Teleport(popUpPoint);
				}
				return;
			}
			Vector2 popUpSidewaysPoint = GetPopUpSidewaysPoint();
			Vector2 vector = Core.Logic.Penitent.GetPosition();
			Vector2 dir = popUpSidewaysPoint - vector;
			if (RaycastToCheckForGeo(popUpSidewaysPoint, dir))
			{
				PopPenitentUp();
			}
			else
			{
				Core.Logic.Penitent.Teleport(popUpSidewaysPoint);
			}
		}
	}

	private Vector2 GetPopUpPoint()
	{
		float num = boxCollider.transform.position.y + boxCollider.offset.y;
		float y = num + boxCollider.size.y / 2f + 0.05f;
		return new Vector2(Core.Logic.Penitent.transform.position.x, y);
	}

	private Vector2 GetPopUpSidewaysPoint()
	{
		float num = boxCollider.transform.position.x + boxCollider.offset.x;
		float num2 = num;
		num2 = ((!(Core.Logic.Penitent.transform.position.x < num)) ? (num2 + (boxCollider.size.x / 2f + 0.5f)) : (num2 - (boxCollider.size.x / 2f + 0.5f)));
		return new Vector2(num2, Core.Logic.Penitent.transform.position.y);
	}

	private bool RaycastToCheckForGeo(Vector2 point, Vector2 dir)
	{
		LayerMask layerMask = LayerMask.GetMask("Wall", "Floor");
		if (Physics2D.RaycastNonAlloc(point, dir, raycastHits, 0.1f, layerMask) > 0)
		{
			Debug.DrawLine(point, raycastHits[0].point, Color.cyan, 5f);
			return true;
		}
		return false;
	}

	private void PopPenitentUp()
	{
		Vector2 popUpPoint = GetPopUpPoint();
		Core.Logic.Penitent.Teleport(popUpPoint);
	}

	private void PopPenitentSideways()
	{
		Vector2 popUpSidewaysPoint = GetPopUpSidewaysPoint();
		Core.Logic.Penitent.Teleport(popUpSidewaysPoint);
	}
}
