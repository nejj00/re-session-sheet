using RESessionSheet.Domain.Model;

namespace RESessionSheet.Domain.Rules;

/// <summary>
/// Applies dot-rating changes against an XpLedger using a cumulative cost table.
/// Raising a rating charges the sum of intervening level costs from Available XP.
/// Lowering a rating refunds that same amount back to Available (confirmed project decision,
/// a deliberate change from the old sheet's no-refund behavior).
/// </summary>
public static class XpEngine
{
	/// <summary>
	/// Cost (positive) or refund (negative) to move from currentLevel to targetLevel.
	/// </summary>
	public static int CostDelta(int currentLevel, int targetLevel, int[] cumulativeTable)
	{
		ValidateLevel(currentLevel, cumulativeTable);
		ValidateLevel(targetLevel, cumulativeTable);
		return cumulativeTable[targetLevel] - cumulativeTable[currentLevel];
	}

	/// <summary>
	/// Cost to raise from currentLevel to targetLevel. Zero if targetLevel is not higher than currentLevel.
	/// </summary>
	public static int CostToRaise(int currentLevel, int targetLevel, int[] cumulativeTable)
	{
		int delta = CostDelta(currentLevel, targetLevel, cumulativeTable);
		return delta > 0 ? delta : 0;
	}

	/// <summary>
	/// Attempts to set an attribute (or virtue) to targetLevel. Raising charges Available XP and fails
	/// (no mutation) if Available is insufficient; lowering refunds the difference back to Available.
	/// </summary>
	public static bool TrySetAttribute(XpLedger ledger, ref int attributeValue, int targetLevel, int[]? costTable = null)
	{
		var table = costTable ?? XpCostTables.AttributeCumulative;
		int delta = CostDelta(attributeValue, targetLevel, table);
		if (delta > 0 && delta > ledger.Available)
		{
			return false;
		}

		ledger.Available -= delta;
		ledger.Spent += delta;
		attributeValue = targetLevel;
		return true;
	}

	/// <summary>
	/// Attempts to set a skill's rating to targetLevel. Same charge/refund semantics as TrySetAttribute.
	/// </summary>
	public static bool TrySetSkill(XpLedger ledger, SkillEntry skill, int targetLevel)
	{
		var table = XpCostTables.SkillCumulative;
		int delta = CostDelta(skill.Rating, targetLevel, table);
		if (delta > 0 && delta > ledger.Available)
		{
			return false;
		}

		ledger.Available -= delta;
		ledger.Spent += delta;
		skill.Rating = targetLevel;
		return true;
	}

	private static void ValidateLevel(int level, int[] table)
	{
		if (level < 0 || level >= table.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(level), level, $"Level must be between 0 and {table.Length - 1}.");
		}
	}
}
