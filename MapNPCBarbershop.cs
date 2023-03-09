using UnityEngine;

public class MapNPCBarbershop : AbstractMonoBehaviour
{
	[SerializeField]
	private RuntimeAnimatorController fourAnimatorController;

	[SerializeField]
	private Vector3 fourPosition;

	[SerializeField]
	protected MapNPCLostBarbershop mapNPCDistanceAnimator;

	public MapDialogueInteraction mapDialogueInteraction;

	[SerializeField]
	private int dialoguerVariableID = 10;

	private void Start()
	{
		if (Dialoguer.GetGlobalFloat(dialoguerVariableID) > 0f)
		{
			NowFour();
			CleanUp();
		}
	}

	protected override void OnDrawGizmosSelected()
	{
		base.OnDrawGizmosSelected();
		Gizmos.DrawWireSphere(fourPosition + base.transform.parent.position, 1f);
	}

	public void NowFour()
	{
		base.animator.runtimeAnimatorController = fourAnimatorController;
		base.transform.localPosition = fourPosition;
	}

	public void CleanUp()
	{
		if ((bool)mapDialogueInteraction)
		{
			Object.Destroy(mapDialogueInteraction);
		}
		if ((bool)mapNPCDistanceAnimator)
		{
			Object.Destroy(mapNPCDistanceAnimator);
		}
	}

	private void SongLooped()
	{
	}

	private void Show()
	{
		base.animator.SetTrigger("show");
	}
}
