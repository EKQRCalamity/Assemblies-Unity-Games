using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObject/Preferences/GameColors")]
public class GameColors : ScriptableObject
{
	public static ResourceBlueprint<GameColors> Default = "GameState/GameColors";

	[Header("Glow Colors")]
	[ColorUsage(true, true)]
	public Color activate = new Color(0f, 2f, 0f, 1f);

	[ColorUsage(true, true)]
	public Color target = new Color(0f, 1f, 2f, 1f);

	[ColorUsage(true, true)]
	public Color targetSelected = new Color(0f, 1f, 2f, 1f);

	[ColorUsage(true, true)]
	public Color attack = new Color(2f, 0.5f, 0.25f, 1f);

	[ColorUsage(true, true)]
	public Color attackConfirm = new Color(2f, 0f, 0f, 1f);

	[ColorUsage(true, true)]
	public Color used = new Color(3f, 2f, 0f, 1f);

	[ColorUsage(true, true)]
	public Color canBeUsed = new Color(2f, 1.25f, 0f, 1f);

	[ColorUsage(true, true)]
	public Color failure = new Color(2f, 0f, 0f, 1f);

	[ColorUsage(true, true)]
	public Color tie = new Color(0f, 0f, 2f, 1f);

	[ColorUsage(true, true)]
	public Color success = new Color(0f, 2f, 0f, 1f);

	[ColorUsage(true, true)]
	public Color endTurnCaution = new Color(2f, 1f, 0.25f);

	[ColorUsage(true, true)]
	public Color endTurn = new Color(0f, 2f, 0f);

	[ColorUsage(true, true)]
	public Color selected = new Color(0f, 1f, 2f, 1f);

	[ColorUsage(true, true)]
	public Color normalRank = new Color(1f, 1f, 1f, 1f);

	[ColorUsage(true, true)]
	public Color eliteRank = new Color(0f, 0f, 2f, 1f);

	[ColorUsage(true, true)]
	public Color legendRank = new Color(2f, 2f, 0f, 1f);
}
