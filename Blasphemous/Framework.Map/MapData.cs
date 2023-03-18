using System.Collections.Generic;

namespace Framework.Map;

public class MapData
{
	public enum MarkType
	{
		Teleport,
		MeaCulpa,
		PrieDieu,
		Guilt,
		Cherub,
		Npc,
		Blue,
		Chest,
		Enemy,
		Green,
		Question,
		Red,
		Soledad,
		Nacimiento,
		Confessor,
		FuenteFlask,
		MiriamPortal
	}

	public static List<MarkType> MarkPrivate = new List<MarkType>
	{
		MarkType.Teleport,
		MarkType.MeaCulpa,
		MarkType.PrieDieu,
		MarkType.Guilt,
		MarkType.Nacimiento,
		MarkType.Soledad,
		MarkType.Confessor,
		MarkType.FuenteFlask,
		MarkType.MiriamPortal
	};

	public string Name;

	public List<CellData> Cells = new List<CellData>();

	public Dictionary<string, SecretData> Secrets = new Dictionary<string, SecretData>();

	public Dictionary<ZoneKey, List<CellData>> CellsByZone = new Dictionary<ZoneKey, List<CellData>>();

	public Dictionary<CellKey, CellData> CellsByCellKey = new Dictionary<CellKey, CellData>();

	public Dictionary<CellKey, MarkType> Marks = new Dictionary<CellKey, MarkType>();
}
