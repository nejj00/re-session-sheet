using RESessionSheet.Domain.Rules;

namespace RESessionSheet.Tests;

public class HerbRecipesTests
{
	[Fact]
	public void GreenGreen_Heals5Hp()
	{
		var effect = HerbRecipes.Resolve(new[] { HerbColor.Green, HerbColor.Green });
		Assert.NotNull(effect);
		Assert.Equal(5, effect!.HpHealed);
		Assert.Equal(0, effect.StrainHealed);
		Assert.False(effect.GrantsSoak);
		Assert.False(effect.GrantsPoisonImmunity);
	}

	[Fact]
	public void GreenGreenGreen_Heals6Hp()
	{
		var effect = HerbRecipes.Resolve(new[] { HerbColor.Green, HerbColor.Green, HerbColor.Green });
		Assert.NotNull(effect);
		Assert.Equal(6, effect!.HpHealed);
	}

	[Fact]
	public void GreenGreenRed_Heals5Hp_AndGrantsSoak()
	{
		var effect = HerbRecipes.Resolve(new[] { HerbColor.Green, HerbColor.Green, HerbColor.Red });
		Assert.NotNull(effect);
		Assert.Equal(5, effect!.HpHealed);
		Assert.True(effect.GrantsSoak);
		Assert.False(effect.GrantsPoisonImmunity);
	}

	[Fact]
	public void GreenBlue_Heals4Hp_AndGrantsPoisonImmunity()
	{
		var effect = HerbRecipes.Resolve(new[] { HerbColor.Green, HerbColor.Blue });
		Assert.NotNull(effect);
		Assert.Equal(4, effect!.HpHealed);
		Assert.True(effect.GrantsPoisonImmunity);
		Assert.False(effect.GrantsSoak);
	}

	[Fact]
	public void GreenGreenBlue_Heals5Hp_AndGrantsPoisonImmunity()
	{
		var effect = HerbRecipes.Resolve(new[] { HerbColor.Green, HerbColor.Green, HerbColor.Blue });
		Assert.NotNull(effect);
		Assert.Equal(5, effect!.HpHealed);
		Assert.True(effect.GrantsPoisonImmunity);
	}

	[Fact]
	public void RedBlue_Heals4Strain_AndGrantsPoisonImmunity_NoHp()
	{
		var effect = HerbRecipes.Resolve(new[] { HerbColor.Red, HerbColor.Blue });
		Assert.NotNull(effect);
		Assert.Equal(0, effect!.HpHealed);
		Assert.Equal(4, effect.StrainHealed);
		Assert.True(effect.GrantsPoisonImmunity);
	}

	[Fact]
	public void RedGreenBlue_Heals4HpAnd4Strain_GrantsSoakAndPoisonImmunity()
	{
		var effect = HerbRecipes.Resolve(new[] { HerbColor.Red, HerbColor.Green, HerbColor.Blue });
		Assert.NotNull(effect);
		Assert.Equal(4, effect!.HpHealed);
		Assert.Equal(4, effect.StrainHealed);
		Assert.True(effect.GrantsSoak);
		Assert.True(effect.GrantsPoisonImmunity);
	}

	[Fact]
	public void ComboOrderDoesNotMatter()
	{
		var a = HerbRecipes.Resolve(new[] { HerbColor.Blue, HerbColor.Green, HerbColor.Red });
		var b = HerbRecipes.Resolve(new[] { HerbColor.Red, HerbColor.Green, HerbColor.Blue });
		var c = HerbRecipes.Resolve(new[] { HerbColor.Green, HerbColor.Red, HerbColor.Blue });

		Assert.Equal(a, b);
		Assert.Equal(b, c);
	}

	[Fact]
	public void UnknownCombo_ReturnsNull()
	{
		Assert.Null(HerbRecipes.Resolve(new[] { HerbColor.Red, HerbColor.Red }));
		Assert.Null(HerbRecipes.Resolve(new[] { HerbColor.Blue }));
		Assert.Null(HerbRecipes.Resolve(new[] { HerbColor.Green, HerbColor.Green, HerbColor.Green, HerbColor.Green }));
	}

	[Fact]
	public void EmptyCombo_ReturnsNull()
	{
		Assert.Null(HerbRecipes.Resolve(Array.Empty<HerbColor>()));
	}

	[Fact]
	public void FirstAidSpray_IsFullReset()
	{
		Assert.True(HerbRecipes.FirstAidSpray.FullReset);
		Assert.Equal(0, HerbRecipes.FirstAidSpray.HpHealed);
	}
}
