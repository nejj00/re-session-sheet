using RESessionSheet.Domain.Rules;

namespace RESessionSheet.Tests;

public class ItemShapeTests
{
	[Fact]
	public void Parse_SingleCell_ProducesOneByOneTrueGrid()
	{
		var grid = ItemShape.Parse(new[] { "X" });

		Assert.Equal(1, ItemShape.Width(grid));
		Assert.Equal(1, ItemShape.Height(grid));
		Assert.True(grid[0, 0]);
	}

	[Fact]
	public void Parse_PistolLShape_MatchesExpectedOccupancy()
	{
		// "3 boxes straight and 1 box coming down from a side" - a 2-wide, 3-tall L.
		var grid = ItemShape.Parse(new[] { "X.", "X.", "XX" });

		Assert.Equal(2, ItemShape.Width(grid));
		Assert.Equal(3, ItemShape.Height(grid));
		Assert.Equal(4, ItemShape.CountOccupiedCells(grid));

		Assert.True(grid[0, 0]);
		Assert.False(grid[0, 1]);
		Assert.True(grid[1, 0]);
		Assert.False(grid[1, 1]);
		Assert.True(grid[2, 0]);
		Assert.True(grid[2, 1]);
	}

	[Fact]
	public void Parse_MismatchedRowLengths_Throws()
	{
		Assert.Throws<ArgumentException>(() => ItemShape.Parse(new[] { "XX", "X" }));
	}

	[Fact]
	public void Parse_EmptyRows_Throws()
	{
		Assert.Throws<ArgumentException>(() => ItemShape.Parse(Array.Empty<string>()));
	}

	[Fact]
	public void Rotate_Zero_ReturnsSameShape()
	{
		var grid = ItemShape.Parse(new[] { "X.", "X.", "XX" });
		var rotated = ItemShape.Rotate(grid, 0);

		Assert.Equal(ItemShape.Width(grid), ItemShape.Width(rotated));
		Assert.Equal(ItemShape.Height(grid), ItemShape.Height(rotated));
		for (int r = 0; r < ItemShape.Height(grid); r++)
		{
			for (int c = 0; c < ItemShape.Width(grid); c++)
			{
				Assert.Equal(grid[r, c], rotated[r, c]);
			}
		}
	}

	[Fact]
	public void Rotate_90_SwapsDimensionsAndPreservesCellCount()
	{
		var grid = ItemShape.Parse(new[] { "X.", "X.", "XX" }); // 2 wide, 3 tall
		var rotated = ItemShape.Rotate(grid, 90);

		Assert.Equal(3, ItemShape.Width(rotated));
		Assert.Equal(2, ItemShape.Height(rotated));
		Assert.Equal(ItemShape.CountOccupiedCells(grid), ItemShape.CountOccupiedCells(rotated));
	}

	[Fact]
	public void Rotate_90_ProducesExpectedOccupancy()
	{
		// L-shape:      Rotated 90 CW should become:
		// X.                XXX
		// X.                X..
		// XX
		var grid = ItemShape.Parse(new[] { "X.", "X.", "XX" });
		var rotated = ItemShape.Rotate(grid, 90);

		Assert.True(rotated[0, 0]);
		Assert.True(rotated[0, 1]);
		Assert.True(rotated[0, 2]);
		Assert.True(rotated[1, 0]);
		Assert.False(rotated[1, 1]);
		Assert.False(rotated[1, 2]);
	}

	[Fact]
	public void Rotate_360_ReturnsToOriginal()
	{
		var grid = ItemShape.Parse(new[] { "X.", "XX", ".X" });
		var rotated = ItemShape.Rotate(grid, 360);

		Assert.Equal(ItemShape.Width(grid), ItemShape.Width(rotated));
		Assert.Equal(ItemShape.Height(grid), ItemShape.Height(rotated));
		for (int r = 0; r < ItemShape.Height(grid); r++)
		{
			for (int c = 0; c < ItemShape.Width(grid); c++)
			{
				Assert.Equal(grid[r, c], rotated[r, c]);
			}
		}
	}

	[Fact]
	public void Rotate_NegativeDegrees_NormalizesCorrectly()
	{
		var grid = ItemShape.Parse(new[] { "X.", "X.", "XX" });
		var rotatedNeg90 = ItemShape.Rotate(grid, -90);
		var rotatedPos270 = ItemShape.Rotate(grid, 270);

		Assert.Equal(ItemShape.Width(rotatedPos270), ItemShape.Width(rotatedNeg90));
		Assert.Equal(ItemShape.Height(rotatedPos270), ItemShape.Height(rotatedNeg90));
		for (int r = 0; r < ItemShape.Height(rotatedNeg90); r++)
		{
			for (int c = 0; c < ItemShape.Width(rotatedNeg90); c++)
			{
				Assert.Equal(rotatedPos270[r, c], rotatedNeg90[r, c]);
			}
		}
	}

	[Fact]
	public void CountOccupiedCells_RectangleShape_CountsAllCells()
	{
		var grid = ItemShape.Parse(new[] { "XX", "XX" });
		Assert.Equal(4, ItemShape.CountOccupiedCells(grid));
	}
}
