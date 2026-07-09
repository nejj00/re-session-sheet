namespace RESessionSheet.Domain.Model;

/// <summary>
/// Root save envelope. SchemaVersion gates the migration pipeline in
/// Domain/Serialization/SchemaMigrations so future format changes can upgrade old saves in place.
/// </summary>
public class CharacterFile
{
	public const int CurrentSchemaVersion = 1;

	public int SchemaVersion { get; set; } = CurrentSchemaVersion;
	public Guid Id { get; set; } = Guid.NewGuid();
	public CharacterSheet Sheet { get; set; } = new();
	public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
	public DateTime LastModifiedUtc { get; set; } = DateTime.UtcNow;
}
