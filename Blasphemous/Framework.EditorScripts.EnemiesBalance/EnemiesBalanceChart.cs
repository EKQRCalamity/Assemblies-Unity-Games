using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Framework.EditorScripts.EnemiesBalance;

[CreateAssetMenu(fileName = "EnemiesBalanceChart", menuName = "Blasphemous/Enemies Balance Chart")]
public class EnemiesBalanceChart : ScriptableObject
{
	[SerializeField]
	public TextAsset BalanceChart;

	[SerializeField]
	public List<EnemyBalanceItem> EnemiesBalanceItems;

	[Button(ButtonSizes.Small)]
	private void LoadBalanceChart()
	{
		ClearChart();
		if (!(BalanceChart == null))
		{
			List<Dictionary<string, object>> balanceChartData = CSVReader.Read(BalanceChart);
			ApplyStatsBalance(balanceChartData);
		}
	}

	private void ApplyStatsBalance(List<Dictionary<string, object>> balanceChartData, float defaultIncrementFactor = 0f)
	{
		try
		{
			for (int i = 0; i < balanceChartData.Count; i++)
			{
				EnemyBalanceItem enemyBalanceItem = new EnemyBalanceItem();
				enemyBalanceItem.Id = (string)balanceChartData[i]["Id"];
				enemyBalanceItem.Name = (string)balanceChartData[i]["Name"];
				enemyBalanceItem.LifeBase = float.Parse(balanceChartData[i]["Life Base"].ToString());
				enemyBalanceItem.Strength = float.Parse(balanceChartData[i]["Strength"].ToString());
				enemyBalanceItem.ContactDamage = float.Parse(balanceChartData[i]["Contact Damage"].ToString());
				enemyBalanceItem.PurgePoints = int.Parse(balanceChartData[i]["Purge Points"].ToString());
				EnemyBalanceItem item = enemyBalanceItem;
				EnemiesBalanceItems.Add(item);
			}
		}
		catch (KeyNotFoundException)
		{
			Debug.LogError("La clave solicitada no existe en el archivo CSV.");
		}
	}

	[Button(ButtonSizes.Small)]
	private void ClearChart()
	{
		if (EnemiesBalanceItems != null && EnemiesBalanceItems.Count > 0)
		{
			EnemiesBalanceItems.Clear();
		}
	}
}
