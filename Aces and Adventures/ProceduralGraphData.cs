using System.Collections.Generic;
using System.Linq;
using ProtoBuf;

[ProtoContract]
[UIField]
public class ProceduralGraphData : IDataContent
{
	[ProtoContract(EnumPassthru = true)]
	public enum Skin
	{
		Light,
		Dark
	}

	private const string CAT_MAIN = "Main";

	private const string CAT_DATA = "Data";

	[ProtoMember(1)]
	[UIField]
	[UICategory("Main")]
	private string _name;

	[ProtoMember(2)]
	private ProceduralGraph _graph;

	[ProtoMember(3, OverwriteList = true)]
	[UIField(maxCount = 0, collapse = UICollapseType.Open, tooltip = "Instructions that will run whenever the map is set active.")]
	[UIFieldCollectionItem]
	[UICategory("Data")]
	private List<AdventureCard.SelectInstruction> _onMapActivatedInstructions;

	[ProtoMember(4)]
	[UIField(onValueChangedMethod = "_OnMapMaterialChange")]
	[UICategory("Main")]
	[UIHorizontalLayout("V")]
	private MapMaterialType _mapMaterial;

	[ProtoMember(5)]
	[UIField(onValueChangedMethod = "_OnSkinChange")]
	[UICategory("Main")]
	[UIHorizontalLayout("V")]
	private Skin _skin;

	public ProceduralGraph graph => _graph ?? (_graph = new ProceduralGraph());

	public IEnumerable<AdventureCard.SelectInstruction> onMapActivatedInstructions
	{
		get
		{
			IEnumerable<AdventureCard.SelectInstruction> enumerable = _onMapActivatedInstructions;
			return enumerable ?? Enumerable.Empty<AdventureCard.SelectInstruction>();
		}
	}

	public MapMaterialType mapMaterial => _mapMaterial;

	public Skin skin => _skin;

	[ProtoMember(128)]
	public string tags { get; set; }

	public string GetTitle()
	{
		return _name ?? "";
	}

	public string GetAutomatedDescription()
	{
		return "";
	}

	public List<string> GetAutomatedTags()
	{
		return null;
	}

	public void PrepareDataForSave()
	{
		_graph?.PrepareDataForSave();
	}

	public string GetSaveErrorMessage()
	{
		if (_name.HasVisibleCharacter())
		{
			return null;
		}
		return "Please enter a name before saving.";
	}

	public void OnLoadValidation()
	{
	}

	private void _OnMapMaterialChange()
	{
		ProceduralMapView.Instance?.UpdateMaterial(_mapMaterial);
	}

	private void _OnSkinChange()
	{
		ProceduralMapView.Instance?.UpdateSkin(_skin);
	}
}
