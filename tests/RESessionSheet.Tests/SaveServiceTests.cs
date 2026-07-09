using RESessionSheet.App.Services;
using RESessionSheet.Domain.Model;

namespace RESessionSheet.Tests;

public class SaveServiceTests : IDisposable
{
	private readonly string _tempDir;
	private readonly SaveService _service;

	public SaveServiceTests()
	{
		_tempDir = Path.Combine(Path.GetTempPath(), "re-session-sheet-tests-" + Guid.NewGuid());
		_service = new SaveService(_tempDir);
	}

	public void Dispose()
	{
		if (Directory.Exists(_tempDir))
		{
			Directory.Delete(_tempDir, recursive: true);
		}
	}

	[Fact]
	public void Constructor_CreatesCharactersDirectoryIfMissing()
	{
		Assert.True(Directory.Exists(_tempDir));
	}

	[Fact]
	public void CreateNew_SavesAndReturnsCharacterWithGivenName()
	{
		var file = _service.CreateNew("Aslan Zırhbaş");

		Assert.Equal("Aslan Zırhbaş", file.Sheet.Name);
		Assert.True(File.Exists(Path.Combine(_tempDir, $"{file.Id}.json")));
	}

	[Fact]
	public void Load_RoundTripsSavedCharacter()
	{
		var original = _service.CreateNew("Test Character");
		original.Sheet.Attributes.Strength = 4;
		_service.Save(original);

		var loaded = _service.Load(original.Id);

		Assert.Equal(original.Id, loaded.Id);
		Assert.Equal("Test Character", loaded.Sheet.Name);
		Assert.Equal(4, loaded.Sheet.Attributes.Strength);
	}

	[Fact]
	public void Load_MissingCharacter_Throws()
	{
		Assert.Throws<FileNotFoundException>(() => _service.Load(Guid.NewGuid()));
	}

	[Fact]
	public void ListRoster_ReturnsAllSavedCharacters()
	{
		_service.CreateNew("Alice");
		_service.CreateNew("Bob");
		_service.CreateNew("Carol");

		var roster = _service.ListRoster();

		Assert.Equal(3, roster.Count);
		Assert.Contains(roster, c => c.Sheet.Name == "Alice");
		Assert.Contains(roster, c => c.Sheet.Name == "Bob");
		Assert.Contains(roster, c => c.Sheet.Name == "Carol");
	}

	[Fact]
	public void ListRoster_SkipsCorruptFilesRatherThanThrowing()
	{
		_service.CreateNew("Good Character");
		File.WriteAllText(Path.Combine(_tempDir, "corrupt.json"), "{ this is not valid json");

		var roster = _service.ListRoster();

		Assert.Single(roster);
		Assert.Equal("Good Character", roster[0].Sheet.Name);
	}

	[Fact]
	public void Delete_RemovesCharacterFile()
	{
		var file = _service.CreateNew("To Delete");
		Assert.Single(_service.ListRoster());

		_service.Delete(file.Id);

		Assert.Empty(_service.ListRoster());
	}

	[Fact]
	public void Delete_NonexistentCharacter_DoesNotThrow()
	{
		_service.Delete(Guid.NewGuid());
	}

	[Fact]
	public void Save_UpdatesLastModifiedUtc()
	{
		var file = _service.CreateNew("Timestamp Test");
		var originalTimestamp = file.LastModifiedUtc;

		Thread.Sleep(10);
		_service.Save(file);

		Assert.True(file.LastModifiedUtc > originalTimestamp);
	}
}
