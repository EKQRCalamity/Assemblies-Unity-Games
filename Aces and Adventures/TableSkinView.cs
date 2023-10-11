using UnityEngine;

public class TableSkinView : MonoBehaviour
{
	public MaterialEvent onTableMaterialChange;

	public MaterialEvent onTableClothMaterialChange;

	private void _OnTableChange(TableSkinType skinType)
	{
		TableSkin component = EnumUtil.GetResourceBlueprint(skinType).GetComponent<TableSkin>();
		onTableMaterialChange?.Invoke(component.tableMaterial);
		onTableClothMaterialChange?.Invoke(component.tableClothMaterial);
	}

	private void Start()
	{
		_OnTableChange(ProfileManager.options.cosmetic.table);
		ProfileOptions.CosmeticOptions.OnTableChange += _OnTableChange;
	}

	private void OnDestroy()
	{
		ProfileOptions.CosmeticOptions.OnTableChange -= _OnTableChange;
	}
}
