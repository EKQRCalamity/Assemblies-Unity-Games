using Extras.EOS.Data;
using UnityEngine;

namespace Extras.EOS;

public class EOSSDKComponent : MonoBehaviour
{
	[SerializeField]
	private EOSConnectionInfoData _eosConnectionInfo;

	private void Awake()
	{
		Object.Destroy(base.gameObject);
	}
}
