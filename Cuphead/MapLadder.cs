using System;
using UnityEngine;

public class MapLadder : AbstractMonoBehaviour
{
	public enum Location
	{
		Top,
		Bottom
	}

	[Serializable]
	public class PointProperties
	{
		public Vector2 interactionPoint = Vector2.zero;

		public float interactionDistance = 0.2f;

		public Vector2 dialogueOffset = DIALOGUE_OFFSET;

		public Vector2 exit = Vector2.zero;

		public Location location { get; private set; }

		public static PointProperties TopDefault()
		{
			PointProperties pointProperties = new PointProperties();
			pointProperties.interactionPoint = INTERACTION_POINT_TOP;
			pointProperties.exit = EXIT_TOP;
			pointProperties.location = Location.Top;
			return pointProperties;
		}

		public static PointProperties BottomDefault()
		{
			PointProperties pointProperties = new PointProperties();
			pointProperties.interactionPoint = INTERACTION_POINT_BOTTOM;
			pointProperties.exit = EXIT_BOTTOM;
			pointProperties.location = Location.Bottom;
			return pointProperties;
		}
	}

	public static readonly Vector2 DIALOGUE_OFFSET = new Vector2(0f, 0.5f);

	public static readonly Vector2 INTERACTION_POINT_TOP = new Vector2(0f, 0.1f);

	public static readonly Vector2 INTERACTION_POINT_BOTTOM = new Vector2(0f, -0.1f);

	public const float INTERACTION_DISTANCE = 0.2f;

	public static readonly AbstractUIInteractionDialogue.Properties DIALOGUE_ENTER = new AbstractUIInteractionDialogue.Properties("CLIMB");

	public static readonly AbstractUIInteractionDialogue.Properties DIALOGUE_EXIT = new AbstractUIInteractionDialogue.Properties("EXIT");

	public static readonly Vector2 EXIT_TOP = new Vector2(0f, 0.2f);

	public static readonly Vector2 EXIT_BOTTOM = new Vector2(0f, -0.2f);

	public float height = 1f;

	[SerializeField]
	private PointProperties top = PointProperties.TopDefault();

	[SerializeField]
	private PointProperties bottom = PointProperties.BottomDefault();

	protected override void OnDrawGizmos()
	{
		float num = 0.1f;
		base.OnDrawGizmos();
		Vector3 position = base.baseTransform.position;
		Vector3 vector = position + new Vector3(0f, height, 0f);
		DrawPointGizmos(position, bottom);
		DrawPointGizmos(vector, top);
		Gizmos.color = Color.black;
		Gizmos.DrawLine(position, vector);
		Gizmos.DrawLine(new Vector2(position.x - num, position.y), new Vector2(position.x + num, position.y));
	}

	private void DrawPointGizmos(Vector2 point, PointProperties properties)
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(point + properties.dialogueOffset, 0.05f);
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(point + properties.dialogueOffset, 0.07f);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(point + properties.interactionPoint, 0.05f);
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(point + properties.interactionPoint, 0.07f);
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(point + properties.interactionPoint, properties.interactionDistance);
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(point + properties.interactionPoint, properties.interactionDistance + 0.02f);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(point + properties.exit, Vector3.one * 0.05f);
		Gizmos.color = Color.white;
		Gizmos.DrawWireCube(point + properties.exit, Vector3.one * 0.07f);
	}

	private void SetLayer(SpriteRenderer renderer)
	{
		if (!(renderer == null))
		{
			renderer.sortingLayerName = "Background";
			renderer.sortingOrder = 100;
		}
	}
}
