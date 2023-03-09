using UnityEngine;

public class DamageReceiverChild : AbstractMonoBehaviour
{
	[SerializeField]
	private DamageReceiver receiver;

	public DamageReceiver Receiver => receiver;

	private void Start()
	{
		base.tag = receiver.tag;
	}
}
