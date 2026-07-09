using RESessionSheet.Domain.Model;
using RESessionSheet.Domain.Rules;

namespace RESessionSheet.Tests;

public class XpEngineTests
{
	[Theory]
	[InlineData(0, 1)]
	[InlineData(0, 6)]
	[InlineData(1, 2)]
	[InlineData(2, 4)]
	public void CostToRaise_Attribute_MatchesCumulativeTableDifference(int from, int to)
	{
		var expected = XpCostTables.AttributeCumulative[to] - XpCostTables.AttributeCumulative[from];
		Assert.Equal(expected, XpEngine.CostToRaise(from, to, XpCostTables.AttributeCumulative));
	}

	[Fact]
	public void CostToRaise_Attribute_Level0To1_Costs5()
	{
		Assert.Equal(5, XpEngine.CostToRaise(0, 1, XpCostTables.AttributeCumulative));
	}

	[Fact]
	public void CostToRaise_Attribute_Level0To6_Costs30()
	{
		Assert.Equal(30, XpEngine.CostToRaise(0, 6, XpCostTables.AttributeCumulative));
	}

	[Fact]
	public void CostToRaise_Skill_Level0To6_Costs18()
	{
		Assert.Equal(18, XpEngine.CostToRaise(0, 6, XpCostTables.SkillCumulative));
	}

	[Fact]
	public void CostToRaise_WhenTargetNotHigher_ReturnsZero()
	{
		Assert.Equal(0, XpEngine.CostToRaise(3, 3, XpCostTables.AttributeCumulative));
		Assert.Equal(0, XpEngine.CostToRaise(3, 1, XpCostTables.AttributeCumulative));
	}

	[Fact]
	public void TrySetAttribute_RaisingWithEnoughXp_ChargesAvailableAndIncreasesSpent()
	{
		var ledger = new XpLedger { Available = 100, Spent = 0 };
		int attribute = 1;

		bool result = XpEngine.TrySetAttribute(ledger, ref attribute, 3);

		Assert.True(result);
		Assert.Equal(3, attribute);
		// cost from level 1 to 3 = table[3]-table[1] = 15-5 = 10
		Assert.Equal(90, ledger.Available);
		Assert.Equal(10, ledger.Spent);
	}

	[Fact]
	public void TrySetAttribute_RaisingWithInsufficientXp_FailsAndDoesNotMutate()
	{
		var ledger = new XpLedger { Available = 5, Spent = 0 };
		int attribute = 1;

		bool result = XpEngine.TrySetAttribute(ledger, ref attribute, 6); // would cost 25

		Assert.False(result);
		Assert.Equal(1, attribute);
		Assert.Equal(5, ledger.Available);
		Assert.Equal(0, ledger.Spent);
	}

	[Fact]
	public void TrySetAttribute_Lowering_RefundsXpToAvailable()
	{
		var ledger = new XpLedger { Available = 90, Spent = 10 };
		int attribute = 3;

		bool result = XpEngine.TrySetAttribute(ledger, ref attribute, 1);

		Assert.True(result);
		Assert.Equal(1, attribute);
		Assert.Equal(100, ledger.Available);
		Assert.Equal(0, ledger.Spent);
	}

	[Fact]
	public void TrySetAttribute_LoweringThenRaisingAgain_RoundTripsCorrectly()
	{
		var ledger = new XpLedger { Available = 100, Spent = 0 };
		int attribute = 1;

		XpEngine.TrySetAttribute(ledger, ref attribute, 5);
		XpEngine.TrySetAttribute(ledger, ref attribute, 2);
		XpEngine.TrySetAttribute(ledger, ref attribute, 5);

		// Net cost should equal going directly from the starting level (1) to the final level (5),
		// regardless of the detour through level 2 in between.
		int netCost = XpCostTables.AttributeCumulative[5] - XpCostTables.AttributeCumulative[1];
		Assert.Equal(5, attribute);
		Assert.Equal(100 - netCost, ledger.Available);
		Assert.Equal(netCost, ledger.Spent);
	}

	[Fact]
	public void TrySetSkill_RaisingWithEnoughXp_ChargesAvailableAndIncreasesSpent()
	{
		var ledger = new XpLedger { Available = 50, Spent = 0 };
		var skill = new SkillEntry { Rating = 0 };

		bool result = XpEngine.TrySetSkill(ledger, skill, 3);

		Assert.True(result);
		Assert.Equal(3, skill.Rating);
		Assert.Equal(50 - 9, ledger.Available); // table[3] = 9
		Assert.Equal(9, ledger.Spent);
	}

	[Fact]
	public void TrySetSkill_Lowering_RefundsXpToAvailable()
	{
		var ledger = new XpLedger { Available = 41, Spent = 9 };
		var skill = new SkillEntry { Rating = 3 };

		bool result = XpEngine.TrySetSkill(ledger, skill, 1);

		Assert.True(result);
		Assert.Equal(1, skill.Rating);
		Assert.Equal(47, ledger.Available); // refunded 6 (9 - 3)
		Assert.Equal(3, ledger.Spent);
	}

	[Fact]
	public void TrySetSkill_RaisingWithInsufficientXp_FailsAndDoesNotMutate()
	{
		var ledger = new XpLedger { Available = 2, Spent = 0 };
		var skill = new SkillEntry { Rating = 0 };

		bool result = XpEngine.TrySetSkill(ledger, skill, 1); // costs 3

		Assert.False(result);
		Assert.Equal(0, skill.Rating);
		Assert.Equal(2, ledger.Available);
	}

	[Fact]
	public void TrySetAttribute_OutOfRangeLevel_Throws()
	{
		var ledger = new XpLedger { Available = 100 };
		int attribute = 1;
		Assert.Throws<ArgumentOutOfRangeException>(() => XpEngine.TrySetAttribute(ledger, ref attribute, 7));
		Assert.Throws<ArgumentOutOfRangeException>(() => XpEngine.TrySetAttribute(ledger, ref attribute, -1));
	}

	[Fact]
	public void XpLedger_Total_IsSpentPlusAvailable()
	{
		var ledger = new XpLedger { Available = 30, Spent = 305 };
		Assert.Equal(335, ledger.Total);
	}
}
