using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.EditorScripts.BossesBalance;

[CreateAssetMenu(fileName = "BossesBalanceChart", menuName = "Blasphemous/Bosses Balance Chart")]
public class BossesBalanceChart : ScriptableObject
{
	[SerializeField]
	public TextAsset BalanceChart;

	[SerializeField]
	private List<Dictionary<string, object>> bossesBalance;

	[SerializeField]
	private List<string> LoadedBosses;

	public List<Dictionary<string, object>> BossesBalance => CSVReader.Read(BalanceChart);

	[Button(ButtonSizes.Small)]
	private void LoadBalanceChart()
	{
		ClearChart();
		if (!(BalanceChart == null))
		{
			bossesBalance = CSVReader.Read(BalanceChart);
			LoadedBossesIds(bossesBalance);
		}
	}

	private void LoadedBossesIds(List<Dictionary<string, object>> bossesBalance)
	{
		if (LoadedBosses == null)
		{
			LoadedBosses = new List<string>();
		}
		foreach (Dictionary<string, object> item in bossesBalance)
		{
			LoadedBosses.Add(item["Name"].ToString());
		}
	}

	[Button(ButtonSizes.Small)]
	private void ClearChart()
	{
		if (LoadedBosses != null && LoadedBosses.Count > 0)
		{
			LoadedBosses.Clear();
		}
		if (bossesBalance != null && bossesBalance.Count > 0)
		{
			bossesBalance.Clear();
		}
	}
}
