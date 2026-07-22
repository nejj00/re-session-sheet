namespace RESessionSheet.Domain.Rules;

/// <summary>
/// Parses and rotates the ASCII-grid shape strings used by ItemDefinition.Shape ('X' = occupied,
/// '.' = empty). Only one canonical (unrotated) orientation is stored per catalog item; rotated
/// variants (matching InventoryItem.Rotation: 0/90/180/270) are computed here on demand, so the
/// future grid inventory screen never needs four pre-authored shapes per item.
/// </summary>
public static class ItemShape
{
	public static bool[,] Parse(IReadOnlyList<string> rows)
	{
		if (rows.Count == 0)
		{
			throw new ArgumentException("Shape must have at least one row.", nameof(rows));
		}

		int height = rows.Count;
		int width = rows[0].Length;
		var grid = new bool[height, width];

		for (int r = 0; r < height; r++)
		{
			if (rows[r].Length != width)
			{
				throw new ArgumentException("All shape rows must be the same length.", nameof(rows));
			}

			for (int c = 0; c < width; c++)
			{
				grid[r, c] = rows[r][c] == 'X';
			}
		}

		return grid;
	}

	/// <summary>Rotates a shape grid clockwise by the given degrees (normalized to 0/90/180/270).</summary>
	public static bool[,] Rotate(bool[,] shape, int degrees)
	{
		int steps = ((degrees / 90) % 4 + 4) % 4;
		var current = shape;
		for (int i = 0; i < steps; i++)
		{
			current = RotateOnce(current);
		}

		return current;
	}

	private static bool[,] RotateOnce(bool[,] shape)
	{
		int rows = shape.GetLength(0);
		int cols = shape.GetLength(1);
		var result = new bool[cols, rows];

		for (int r = 0; r < rows; r++)
		{
			for (int c = 0; c < cols; c++)
			{
				result[c, rows - 1 - r] = shape[r, c];
			}
		}

		return result;
	}

	public static int CountOccupiedCells(bool[,] shape)
	{
		int count = 0;
		foreach (bool cell in shape)
		{
			if (cell)
			{
				count++;
			}
		}

		return count;
	}

	public static int Width(bool[,] shape) => shape.GetLength(1);

	public static int Height(bool[,] shape) => shape.GetLength(0);
}
