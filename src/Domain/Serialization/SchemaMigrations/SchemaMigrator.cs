using System.Text.Json.Nodes;
using RESessionSheet.Domain.Model;

namespace RESessionSheet.Domain.Serialization.SchemaMigrations;

/// <summary>
/// Runs sequential MigrateVxToVx+1 steps over the raw JSON to bring an older save up to the
/// current schema before typed deserialization. Schema v1 is the only version that exists so far,
/// so this is a no-op passthrough — scaffolding for when a v2 migration is actually needed.
/// </summary>
public static class SchemaMigrator
{
	public static JsonNode Migrate(JsonNode root, int fromVersion)
	{
		if (fromVersion > CharacterFile.CurrentSchemaVersion)
		{
			throw new InvalidOperationException(
				$"Character file schema version {fromVersion} is newer than this app supports ({CharacterFile.CurrentSchemaVersion}).");
		}

		// No migrations registered yet — schema v1 is the only version.
		return root;
	}
}
