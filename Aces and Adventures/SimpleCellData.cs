using Google.Apis.Sheets.v4.Data;

public struct SimpleCellData
{
	public string value;

	public string note;

	public SimpleCellData(string value, string note)
	{
		this.value = value;
		this.note = note;
	}

	public SimpleCellData(string value)
	{
		this = default(SimpleCellData);
		this.value = value;
	}

	public SimpleCellData(CellData cellData)
		: this(cellData.Value(), cellData.Note)
	{
	}

	public CellData ToCellData()
	{
		return new CellData
		{
			UserEnteredValue = new ExtendedValue
			{
				StringValue = value
			},
			Note = note
		};
	}

	public override string ToString()
	{
		return "(" + value + "," + note + ")";
	}

	public static implicit operator SimpleCellData(CellData cellData)
	{
		return new SimpleCellData(cellData);
	}
}
