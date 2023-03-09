public class ParryAttackSpark : Effect
{
	public bool IsCuphead
	{
		set
		{
			base.animator.SetBool("IsCuphead", value);
		}
	}
}
