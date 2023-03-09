using UnityEngine;

public class DragonLevelPeashotFlash : AbstractPausableComponent
{
	public void Flash()
	{
		base.transform.SetScale(1f, MathUtils.PlusOrMinus(), 1f);
		base.animator.SetInteger("i", Random.Range(0, 4));
		base.animator.SetTrigger("OnChange");
	}
}
