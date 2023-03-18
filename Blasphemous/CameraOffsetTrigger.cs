using Com.LuisPedroFonseca.ProCamera2D;
using UnityEngine;

public class CameraOffsetTrigger : MonoBehaviour
{
	public Vector2 offset;

	public LayerMask triggerMask;

	private bool influenceOn;

	public Color gizmoColor = Color.cyan;

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (ProCamera2D.Instance != null && (triggerMask.value & (1 << collision.gameObject.layer)) > 0)
		{
			influenceOn = true;
		}
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if (!influenceOn && ProCamera2D.Instance != null && (triggerMask.value & (1 << collision.gameObject.layer)) > 0)
		{
			influenceOn = true;
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (ProCamera2D.Instance != null && (triggerMask.value & (1 << collision.gameObject.layer)) > 0)
		{
			influenceOn = false;
		}
	}

	private void Update()
	{
		if (influenceOn)
		{
			ProCamera2D.Instance.ApplyInfluence(offset);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = gizmoColor;
		Gizmos.DrawWireCube(size: new Vector3(20f, 11f, 0f), center: base.transform.position + (Vector3)offset);
		Gizmos.DrawIcon(base.transform.position + new Vector3(0f, 2f, 0f), "Blasphemous/TPO_singleImage.png", allowScaling: true);
	}
}
