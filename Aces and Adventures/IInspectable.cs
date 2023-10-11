public interface IInspectable
{
	string inspectedName { get; set; }

	object inspectedValue { get; }

	UICategorySet[] uiCategorySets { get; }
}
