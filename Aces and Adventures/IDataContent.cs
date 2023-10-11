using System.Collections.Generic;

public interface IDataContent
{
	string tags { get; set; }

	string GetTitle();

	string GetAutomatedDescription();

	List<string> GetAutomatedTags();

	void PrepareDataForSave();

	string GetSaveErrorMessage();

	void OnLoadValidation();
}
