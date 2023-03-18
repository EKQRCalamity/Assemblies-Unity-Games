using Sirenix.OdinInspector;
using UnityEngine;

public class LaudesPlatformController : MonoBehaviour
{
	public Animator[] animators;

	private void Awake()
	{
	}

	[Button(ButtonSizes.Small)]
	public void GetChildrenAnimators()
	{
		animators = GetComponentsInChildren<Animator>();
	}

	[Button(ButtonSizes.Small)]
	public void ShowAllPlatforms()
	{
		if (animators != null)
		{
			Animator[] array = animators;
			foreach (Animator animator in array)
			{
				animator.SetBool("HIDDEN", value: false);
			}
		}
	}

	[Button(ButtonSizes.Small)]
	public void HideAllPlatforms()
	{
		if (animators != null)
		{
			Animator[] array = animators;
			foreach (Animator animator in array)
			{
				animator.SetBool("HIDDEN", value: true);
			}
		}
	}
}
