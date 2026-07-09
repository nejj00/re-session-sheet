namespace RESessionSheet.Domain.Model;

public enum SkillCategory
{
	Talent,
	Skill,
	Knowledge
}

public enum SkillId
{
	// Talents
	Alertness, Athletics, Brawl, Dodge, Empathy, Expression, Intimidation, Leadership, Streetwise, Subterfuge,
	// Skills
	AnimalKen, Drive, Etiquette, Firearms, Melee, Performance, Repair, Security, Stealth, Survival,
	// Knowledges
	Bureaucracy, Computer, Enigmas, Investigation, Law, Linguistics, Medicine, Occult, Politics, Science
}

public static class SkillCatalog
{
	public static readonly IReadOnlyList<(SkillId Id, SkillCategory Category, string Label)> All = new[]
	{
		(SkillId.Alertness, SkillCategory.Talent, "Alertness"),
		(SkillId.Athletics, SkillCategory.Talent, "Athletics"),
		(SkillId.Brawl, SkillCategory.Talent, "Brawl"),
		(SkillId.Dodge, SkillCategory.Talent, "Dodge"),
		(SkillId.Empathy, SkillCategory.Talent, "Empathy"),
		(SkillId.Expression, SkillCategory.Talent, "Expression"),
		(SkillId.Intimidation, SkillCategory.Talent, "Intimidation"),
		(SkillId.Leadership, SkillCategory.Talent, "Leadership"),
		(SkillId.Streetwise, SkillCategory.Talent, "Streetwise"),
		(SkillId.Subterfuge, SkillCategory.Talent, "Subterfuge"),

		(SkillId.AnimalKen, SkillCategory.Skill, "Animal Ken"),
		(SkillId.Drive, SkillCategory.Skill, "Drive"),
		(SkillId.Etiquette, SkillCategory.Skill, "Etiquette"),
		(SkillId.Firearms, SkillCategory.Skill, "Firearms"),
		(SkillId.Melee, SkillCategory.Skill, "Melee"),
		(SkillId.Performance, SkillCategory.Skill, "Performance"),
		(SkillId.Repair, SkillCategory.Skill, "Repair"),
		(SkillId.Security, SkillCategory.Skill, "Security"),
		(SkillId.Stealth, SkillCategory.Skill, "Stealth"),
		(SkillId.Survival, SkillCategory.Skill, "Survival"),

		(SkillId.Bureaucracy, SkillCategory.Knowledge, "Bureaucracy"),
		(SkillId.Computer, SkillCategory.Knowledge, "Computer"),
		(SkillId.Enigmas, SkillCategory.Knowledge, "Enigmas"),
		(SkillId.Investigation, SkillCategory.Knowledge, "Investigation"),
		(SkillId.Law, SkillCategory.Knowledge, "Law"),
		(SkillId.Linguistics, SkillCategory.Knowledge, "Linguistics"),
		(SkillId.Medicine, SkillCategory.Knowledge, "Medicine"),
		(SkillId.Occult, SkillCategory.Knowledge, "Occult"),
		(SkillId.Politics, SkillCategory.Knowledge, "Politics"),
		(SkillId.Science, SkillCategory.Knowledge, "Science"),
	};
}

public class SkillEntry
{
	public int Rating { get; set; }
	public string Specialty { get; set; } = "";
}

public class Skills
{
	public Dictionary<SkillId, SkillEntry> Entries { get; set; } = CreateDefault();

	private static Dictionary<SkillId, SkillEntry> CreateDefault()
	{
		var entries = new Dictionary<SkillId, SkillEntry>();
		foreach (var (id, _, _) in SkillCatalog.All)
		{
			entries[id] = new SkillEntry();
		}
		return entries;
	}

	public SkillEntry Get(SkillId id) => Entries[id];
}
