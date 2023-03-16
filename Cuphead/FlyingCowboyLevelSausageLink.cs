using System.Collections;
using UnityEngine;

public class FlyingCowboyLevelSausageLink : BasicProjectile
{
	private FlyingCowboyLevelMeat.SausageType sausageType;

	public void Initialize(FlyingCowboyLevelMeat.SausageType sausageType, Transform sausageLinkSqueezePoint, FlyingCowboyLevelSausageLink previousLink)
	{
		this.sausageType = sausageType;
		if (sausageType != FlyingCowboyLevelMeat.SausageType.U1 && sausageType != FlyingCowboyLevelMeat.SausageType.U2 && sausageType != FlyingCowboyLevelMeat.SausageType.U3)
		{
			StartCoroutine(squeeze_cr(sausageLinkSqueezePoint, previousLink));
		}
		if (sausageType == FlyingCowboyLevelMeat.SausageType.H1 || sausageType == FlyingCowboyLevelMeat.SausageType.H2 || sausageType == FlyingCowboyLevelMeat.SausageType.H3 || sausageType == FlyingCowboyLevelMeat.SausageType.H4 || sausageType == FlyingCowboyLevelMeat.SausageType.L5)
		{
			base.animator.SetFloat("Speed", Rand.PosOrNeg());
		}
	}

	public void Squeeze()
	{
		base.animator.Play("Squeeze" + sausageType);
	}

	private IEnumerator squeeze_cr(Transform sausageLinkSqueezePoint, FlyingCowboyLevelSausageLink previousLink)
	{
		while (base.transform.position.x > sausageLinkSqueezePoint.position.x)
		{
			yield return null;
		}
		Squeeze();
		if (previousLink != null)
		{
			previousLink.Squeeze();
		}
	}
}
