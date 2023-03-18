using Sirenix.OdinInspector;
using UnityEngine;

namespace Gameplay.UI.Others.UIGameLogic;

public class PlayerGuiltPanel : SerializedMonoBehaviour
{
	private Animator animator;

	private const int MAX_GUILT = 7;

	private const string GUILT_LEVEL_KEY = "GUILT_LEVEL";

	private void Awake()
	{
		animator = GetComponent<Animator>();
	}

	public void SetGuiltLevel(int value = 0, bool instantly = false)
	{
		if (value < 0 || value > 7)
		{
			Debug.LogError("Invalid guilt amount");
		}
		else if (instantly)
		{
			animator.Play($"LEVEL{value}");
		}
		else
		{
			animator.SetInteger("GUILT_LEVEL", value);
		}
	}
}
