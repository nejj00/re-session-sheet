using System.Text.Json;
using System.Text.Json.Serialization;
using RESessionSheet.Domain.Model;

namespace RESessionSheet.Domain.Serialization;

/// <summary>
/// Pure C# JSON (de)serialization for the item catalog, mirroring CharacterFileSerializer's
/// options so both use the same conventions (camelCase, enums as strings). Unlike character
/// saves, the catalog has no schema-version/migration concern - it's static shipped content.
/// </summary>
public static class ItemCatalogSerializer
{
	public static readonly JsonSerializerOptions Options = new()
	{
		WriteIndented = true,
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
	};

	public static string Serialize(ItemCatalog catalog) =>
		JsonSerializer.Serialize(catalog, Options);

	public static ItemCatalog Deserialize(string json) =>
		JsonSerializer.Deserialize<ItemCatalog>(json, Options)
			?? throw new InvalidDataException("Item catalog JSON deserialized to null.");
}
