using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Serialization;

namespace RESessionSheet.App.Services;

/// <summary>
/// Directory-scan-based character persistence, mirroring Flare's glob-scan save-file discovery
/// (self-healing, no separate roster index file to desync). Pure C#/System.IO so it's testable
/// without booting the Godot engine — the caller (GameState) resolves the actual user:// path via
/// Godot APIs and passes it in here.
/// </summary>
public class SaveService
{
	public string CharactersDirectory { get; }

	public SaveService(string charactersDirectory)
	{
		CharactersDirectory = charactersDirectory;
		Directory.CreateDirectory(CharactersDirectory);
	}

	public IReadOnlyList<CharacterFile> ListRoster()
	{
		var results = new List<CharacterFile>();
		foreach (var path in Directory.EnumerateFiles(CharactersDirectory, "*.json"))
		{
			try
			{
				results.Add(CharacterFileSerializer.Deserialize(File.ReadAllText(path)));
			}
			catch
			{
				// Skip corrupt/unreadable files rather than failing the whole roster listing.
			}
		}

		return results;
	}

	public CharacterFile Load(Guid id)
	{
		string path = PathFor(id);
		if (!File.Exists(path))
		{
			throw new FileNotFoundException($"Character file not found: {path}", path);
		}

		return CharacterFileSerializer.Deserialize(File.ReadAllText(path));
	}

	public void Save(CharacterFile characterFile)
	{
		characterFile.LastModifiedUtc = DateTime.UtcNow;
		File.WriteAllText(PathFor(characterFile.Id), CharacterFileSerializer.Serialize(characterFile));
	}

	public void Delete(Guid id)
	{
		string path = PathFor(id);
		if (File.Exists(path))
		{
			File.Delete(path);
		}
	}

	public CharacterFile CreateNew(string name)
	{
		var file = new CharacterFile();
		file.Sheet.Name = name;
		Save(file);
		return file;
	}

	private string PathFor(Guid id) => Path.Combine(CharactersDirectory, $"{id}.json");
}
