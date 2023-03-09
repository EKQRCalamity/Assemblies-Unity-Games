using System.Collections.Generic;
using UnityEngine;

public class EnemyDatabase : ScriptableObject
{
	private static EnemyProperties defaultProperties = new EnemyProperties();

	public const string PATH = "EnemyDatabase/data_enemies";

	private static EnemyDatabase _instance;

	public List<EnemyProperties> enemyProperties;

	public int index;

	public static EnemyDatabase Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Resources.Load<EnemyDatabase>("EnemyDatabase/data_enemies");
			}
			return _instance;
		}
	}

	public static EnemyProperties GetProperties(EnemyID id)
	{
		return id switch
		{
			EnemyID.Undefined => null, 
			EnemyID.blue_goblin => Instance.enemyProperties[0], 
			EnemyID.pink_goblin => Instance.enemyProperties[1], 
			EnemyID.wind => Instance.enemyProperties[2], 
			EnemyID.blob_runner => Instance.enemyProperties[3], 
			EnemyID.lobber => Instance.enemyProperties[4], 
			EnemyID.flower_grunt => Instance.enemyProperties[5], 
			EnemyID.mushroom => Instance.enemyProperties[6], 
			EnemyID.chomper => Instance.enemyProperties[7], 
			EnemyID.acorn => Instance.enemyProperties[8], 
			EnemyID.acornmaker => Instance.enemyProperties[9], 
			EnemyID.spiker => Instance.enemyProperties[10], 
			EnemyID.ladybug => Instance.enemyProperties[11], 
			EnemyID.dragonfly => Instance.enemyProperties[12], 
			EnemyID.dragonflyshot => Instance.enemyProperties[13], 
			EnemyID.woodpecker => Instance.enemyProperties[14], 
			EnemyID.beetle => Instance.enemyProperties[15], 
			EnemyID.lobster => Instance.enemyProperties[16], 
			EnemyID.barnacle => Instance.enemyProperties[17], 
			EnemyID.urchin => Instance.enemyProperties[18], 
			EnemyID.crab => Instance.enemyProperties[19], 
			EnemyID.krill => Instance.enemyProperties[20], 
			EnemyID.clam => Instance.enemyProperties[21], 
			EnemyID.starfish => Instance.enemyProperties[22], 
			EnemyID.flyingfish => Instance.enemyProperties[23], 
			EnemyID.satyr => Instance.enemyProperties[24], 
			EnemyID.mudman => Instance.enemyProperties[25], 
			EnemyID.smallmudman => Instance.enemyProperties[26], 
			EnemyID.dragon => Instance.enemyProperties[27], 
			EnemyID.miner => Instance.enemyProperties[28], 
			EnemyID.fan => Instance.enemyProperties[29], 
			EnemyID.flamer => Instance.enemyProperties[30], 
			EnemyID.wall => Instance.enemyProperties[31], 
			EnemyID.funhousewall => Instance.enemyProperties[32], 
			EnemyID.funwall2 => Instance.enemyProperties[33], 
			EnemyID.rocket => Instance.enemyProperties[34], 
			EnemyID.jack => Instance.enemyProperties[35], 
			EnemyID.duck => Instance.enemyProperties[36], 
			EnemyID.miniduck => Instance.enemyProperties[37], 
			EnemyID.jackinbox => Instance.enemyProperties[38], 
			EnemyID.tuba => Instance.enemyProperties[39], 
			EnemyID.starcannon => Instance.enemyProperties[40], 
			EnemyID.balloon => Instance.enemyProperties[41], 
			EnemyID.pretzel => Instance.enemyProperties[42], 
			EnemyID.arcade => Instance.enemyProperties[43], 
			EnemyID.ballrunner => Instance.enemyProperties[44], 
			EnemyID.magician => Instance.enemyProperties[45], 
			EnemyID.polebot => Instance.enemyProperties[46], 
			EnemyID.log => Instance.enemyProperties[47], 
			EnemyID.hotdog => Instance.enemyProperties[48], 
			_ => defaultProperties, 
		};
	}
}
