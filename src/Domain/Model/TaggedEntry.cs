namespace RESessionSheet.Domain.Model;

/// <summary>
/// A freeform Merit/Flaw/Trait/Background entry for V1. CatalogId is reserved for a later
/// phase where these link back to a structured catalog (data/catalogs/merits_flaws.json) with
/// auto-applied mechanical effects instead of being purely descriptive text.
/// </summary>
public class TaggedEntry
{
	public string Name { get; set; } = "";
	public string Description { get; set; } = "";
	public string? CatalogId { get; set; }
}
