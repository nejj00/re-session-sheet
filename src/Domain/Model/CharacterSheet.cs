namespace RESessionSheet.Domain.Model;

public class CharacterSheet
{
	// Bio
	public string Name { get; set; } = "";
	public string Age { get; set; } = "";
	public string Gender { get; set; } = "";
	public string Height { get; set; } = "";
	public string Weight { get; set; } = "";
	public string EyeColor { get; set; } = "";
	public string HairColor { get; set; } = "";
	public string SkinTone { get; set; } = "";
	public string Demeanor { get; set; } = "";
	public string Rank { get; set; } = "";
	public string FirstTeam { get; set; } = "";

	public string Backstory { get; set; } = "";

	// Archetype tags — first-class, distinct from the mechanical Merits/Flaws list below.
	public string Role { get; set; } = "";
	public string Talent { get; set; } = "";
	public string Specialist { get; set; } = "";
	public List<string> ArchetypeMerits { get; set; } = new();
	public List<string> ArchetypeFlaws { get; set; } = new();

	public Attributes Attributes { get; set; } = new();
	public Skills Skills { get; set; } = new();
	public Virtues Virtues { get; set; } = new();
	public XpLedger Xp { get; set; } = new();
	public HealthTrack Health { get; set; } = new();
	public Strain Strain { get; set; } = new();
	public ActionPoints Ap { get; set; } = new();
	public Inventory Inventory { get; set; } = new();
	public List<SessionLogEntry> SessionLog { get; set; } = new();

	// V1: freeform tagged lists (later phase: structured catalog references via TaggedEntry.CatalogId)
	public List<TaggedEntry> Merits { get; set; } = new();
	public List<TaggedEntry> Flaws { get; set; } = new();
	public List<TaggedEntry> Traits { get; set; } = new();
	public List<TaggedEntry> Backgrounds { get; set; } = new();

	public List<string> WoundNotes { get; set; } = new();
}
