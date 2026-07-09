using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Serialization.SchemaMigrations;

namespace RESessionSheet.Domain.Serialization;

/// <summary>
/// System.Text.Json is used instead of Godot's own Resource/Json serialization so the Domain
/// layer stays engine-free and testable with plain dotnet test.
/// </summary>
public static class CharacterFileSerializer
{
	public static readonly JsonSerializerOptions Options = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
	};

	public static string Serialize(CharacterFile file) =>
		JsonSerializer.Serialize(file, Options);

	public static CharacterFile Deserialize(string json)
	{
		var node = JsonNode.Parse(json)
			?? throw new InvalidDataException("Character file JSON parsed to null.");

		int schemaVersion = node["schemaVersion"]?.GetValue<int>() ?? 0;
		var migrated = SchemaMigrator.Migrate(node, schemaVersion);

		return migrated.Deserialize<CharacterFile>(Options)
			?? throw new InvalidDataException("Character file JSON deserialized to null.");
	}
}
