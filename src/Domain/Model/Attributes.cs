namespace RESessionSheet.Domain.Model;

public enum AttributeCategory
{
	Physical,
	Social,
	Mental
}

public enum AttributeId
{
	Strength,
	Dexterity,
	Stamina,
	Charisma,
	Manipulation,
	Appearance,
	Perception,
	Intelligence,
	Wits
}

public static class AttributeCatalog
{
	public static readonly IReadOnlyList<(AttributeId Id, AttributeCategory Category, string Label)> All = new[]
	{
		(AttributeId.Strength, AttributeCategory.Physical, "Strength"),
		(AttributeId.Dexterity, AttributeCategory.Physical, "Dexterity"),
		(AttributeId.Stamina, AttributeCategory.Physical, "Stamina"),
		(AttributeId.Charisma, AttributeCategory.Social, "Charisma"),
		(AttributeId.Manipulation, AttributeCategory.Social, "Manipulation"),
		(AttributeId.Appearance, AttributeCategory.Social, "Appearance"),
		(AttributeId.Perception, AttributeCategory.Mental, "Perception"),
		(AttributeId.Intelligence, AttributeCategory.Mental, "Intelligence"),
		(AttributeId.Wits, AttributeCategory.Mental, "Wits"),
	};
}

public class Attributes
{
	public int Strength { get; set; } = 0;
	public int Dexterity { get; set; } = 0;
	public int Stamina { get; set; } = 0;
	public int Charisma { get; set; } = 0;
	public int Manipulation { get; set; } = 0;
	public int Appearance { get; set; } = 0;
	public int Perception { get; set; } = 0;
	public int Intelligence { get; set; } = 0;
	public int Wits { get; set; } = 0;

	public int Get(AttributeId id) => id switch
	{
		AttributeId.Strength => Strength,
		AttributeId.Dexterity => Dexterity,
		AttributeId.Stamina => Stamina,
		AttributeId.Charisma => Charisma,
		AttributeId.Manipulation => Manipulation,
		AttributeId.Appearance => Appearance,
		AttributeId.Perception => Perception,
		AttributeId.Intelligence => Intelligence,
		AttributeId.Wits => Wits,
		_ => throw new ArgumentOutOfRangeException(nameof(id), id, null)
	};

	public void Set(AttributeId id, int value)
	{
		switch (id)
		{
			case AttributeId.Strength: Strength = value; break;
			case AttributeId.Dexterity: Dexterity = value; break;
			case AttributeId.Stamina: Stamina = value; break;
			case AttributeId.Charisma: Charisma = value; break;
			case AttributeId.Manipulation: Manipulation = value; break;
			case AttributeId.Appearance: Appearance = value; break;
			case AttributeId.Perception: Perception = value; break;
			case AttributeId.Intelligence: Intelligence = value; break;
			case AttributeId.Wits: Wits = value; break;
			default: throw new ArgumentOutOfRangeException(nameof(id), id, null);
		}
	}
}
