using UnityEngine;

public class LevelUIInteractionDialogue : AbstractUIInteractionDialogue
{
	public enum TailPosition
	{
		Right,
		Left,
		Bottom
	}

	private const float TAIL_WIDTH = 14f;

	private const float OFFSET_GLYPH = 27f;

	private const float OFFSET_GLYPH_ONLY = 5.3f;

	private float glyphOffsetAddition;

	private TailPosition tailPosition;

	[SerializeField]
	private GameObject bottomTail;

	[SerializeField]
	private GameObject leftTail;

	[SerializeField]
	private GameObject rightTail;

	private static GameObject defaultTarget;

	protected override float PreferredWidth
	{
		get
		{
			if (tmpText.text.Length == 0)
			{
				return tmpText.preferredWidth + glyph.preferredWidth + 5.3f + glyphOffsetAddition;
			}
			return tmpText.preferredWidth + glyph.preferredWidth + 27f + glyphOffsetAddition;
		}
	}

	public static LevelUIInteractionDialogue Create(Properties properties, PlayerInput player, Vector2 offset, float glyphOffsetAddition = 0f, TailPosition tailPosition = TailPosition.Bottom, bool playerTarget = true)
	{
		LevelUIInteractionDialogue levelUIInteractionDialogue = Object.Instantiate(Level.Current.LevelResources.levelUIInteractionDialogue);
		levelUIInteractionDialogue.glyphOffsetAddition = glyphOffsetAddition;
		levelUIInteractionDialogue.tailPosition = tailPosition;
		levelUIInteractionDialogue.Init(properties, player, offset);
		switch (tailPosition)
		{
		case TailPosition.Right:
			levelUIInteractionDialogue.dialogueOffset = new Vector2(offset.x - levelUIInteractionDialogue.back.sizeDelta.x * 0.5f - 14f, offset.y);
			break;
		case TailPosition.Left:
			levelUIInteractionDialogue.dialogueOffset = new Vector2(offset.x + levelUIInteractionDialogue.back.sizeDelta.x * 0.5f + 14f, offset.y);
			break;
		}
		if (!playerTarget && defaultTarget == null)
		{
			defaultTarget = GameObject.CreatePrimitive(PrimitiveType.Cube);
			defaultTarget.transform.position = Vector3.zero;
			defaultTarget.transform.localScale = Vector3.zero;
			levelUIInteractionDialogue.target = defaultTarget.transform;
		}
		else if (!playerTarget)
		{
			levelUIInteractionDialogue.target = defaultTarget.transform;
		}
		return levelUIInteractionDialogue;
	}

	protected override void Awake()
	{
		base.Awake();
		base.transform.SetParent(LevelHUD.Current.Canvas.transform, worldPositionStays: false);
	}

	protected override void Init(Properties properties, PlayerInput player, Vector2 offset)
	{
		base.Init(properties, player, offset);
		UpdatePos();
	}

	private void Update()
	{
		UpdatePos();
		UpdateTailPosition();
	}

	protected virtual void UpdatePos()
	{
		if (target != null)
		{
			base.transform.position = (Vector2)target.position + dialogueOffset;
		}
	}

	private void UpdateTailPosition()
	{
		switch (tailPosition)
		{
		case TailPosition.Bottom:
			bottomTail.SetActive(value: true);
			break;
		case TailPosition.Right:
			rightTail.SetActive(value: true);
			break;
		case TailPosition.Left:
			leftTail.SetActive(value: true);
			break;
		}
	}
}
