using UnityEngine;

namespace Tools.Level.Layout;

[SelectionBase]
public class LayoutElement : MonoBehaviour
{
	public bool showInGame;

	public Category category;

	public LevelBuilder LevelBuilder { get; private set; }

	public SpriteRenderer SpriteRenderer { get; private set; }

	private void Awake()
	{
		SpriteRenderer = GetComponent<SpriteRenderer>();
	}

	private void Start()
	{
		LevelBuilder = GetComponentInParent<LevelBuilder>();
	}
}
