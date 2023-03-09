using UnityEngine;

public class MapSecretPathTrigger : AbstractMonoBehaviour
{
	[SerializeField]
	private bool enablePath;

	private Vector2 size;

	private void Start()
	{
		size = GetComponent<BoxCollider2D>().size;
	}

	private bool PointInBounds(Vector3 pos)
	{
		return pos.x > base.transform.position.x - size.x / 2f && pos.x < base.transform.position.x + size.x / 2f && pos.y > base.transform.position.y - size.y / 2f && pos.y < base.transform.position.y + size.y / 2f;
	}

	private void OnTriggerStay2D(Collider2D collider)
	{
		MapPlayerController component = collider.GetComponent<MapPlayerController>();
		if ((bool)component && PointInBounds(component.transform.position))
		{
			component.SecretPathEnter(enablePath);
		}
	}
}
