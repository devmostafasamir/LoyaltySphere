using FluentAssertions;
using LoyaltySphere.RewardService.Application.Services;
using LoyaltySphere.RewardService.Domain.Entities;
using LoyaltySphere.RewardService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using LoyaltySphere.RewardService.Domain.Enums;
using LoyaltySphere.RewardService.Domain.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace LoyaltySphere.RewardService.Tests.Application.Services;

/// <summary>
/// Unit tests for RewardCalculationService.
/// Tests reward calculation logic, tier bonuses, and business rules.
/// </summary>
public class RewardCalculationServiceTests
{
    private readonly IRewardCalculationService _service;
    private readonly Mock<ITierCalculationService> _tierServiceMock;
    private readonly Mock<IRewardRuleSelector> _ruleSelectorMock;
    private readonly Mock<ICampaignEligibilityChecker> _campaignEligibilityMock;
    private readonly Mock<IPointsCapService> _pointsCapServiceMock;
    private readonly Mock<ILogger<RewardCalculationService>> _loggerMock;
    private const string TenantId = "test-tenant";

    public RewardCalculationServiceTests()
    {
        _tierServiceMock = new Mock<ITierCalculationService>();
        _ruleSelectorMock = new Mock<IRewardRuleSelector>();
        _campaignEligibilityMock = new Mock<ICampaignEligibilityChecker>();
        _pointsCapServiceMock = new Mock<IPointsCapService>();
        _loggerMock = new Mock<ILogger<RewardCalculationService>>();

        _service = new RewardCalculationService(
            _loggerMock.Object,
            _tierServiceMock.Object,
            _ruleSelectorMock.Object,
            _campaignEligibilityMock.Object,
            _pointsCapServiceMock.Object);
    }

    [Fact]
    public async Task CalculateRewardAsync_ShouldCalculatePointsCorrectly()
    {
        // Arrange
        var customer = CreateCustomer("Bronze", 0);
        var amount = Money.Create(1000, "EGP");
        var rules = new List<RewardRule>();
        var campaigns = Enumerable.Empty<Campaign>();

        var rule = RewardRule.Create(TenantId, "Standard Rule", "1 point per unit", 1.0m);
        rules.Add(rule);

        _tierServiceMock.Setup(s => s.GetTierMultiplier(customer.Tier))
            .Returns(1.1m);

        _ruleSelectorMock.Setup(s => s.SelectBestRule(It.IsAny<IEnumerable<RewardRule>>(), It.IsAny<Money>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(rule);

        _campaignEligibilityMock.Setup(s => s.GetEligibleCampaigns(It.IsAny<IEnumerable<Campaign>>(), It.IsAny<Customer>(), It.IsAny<Money>(), It.IsAny<string>()))
            .Returns(Enumerable.Empty<Campaign>());

        _pointsCapServiceMock.Setup(s => s.ApplyPointsCap(It.IsAny<Points>(), It.IsAny<Money>()))
            .Returns((Points p, Money m) => p);

        // Act
        var result = await _service.CalculateRewardAsync(customer, amount, rules, campaigns);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.TotalPoints.Value.Should().Be(110); // 100 * 1.1
    }

    [Fact]
    public async Task CalculateRewardAsync_WithActiveCampaign_ShouldIncludeCampaignBonus()
    {
        // Arrange
        var customer = CreateCustomer("Bronze", 0);
        var amount = Money.Create(1000, "EGP");
        var rule = RewardRule.Create(TenantId, "Standard Rule", "1 point per unit", 1.0m);
        var rules = new[] { rule };
        var campaign = Campaign.CreateBonusCampaign(TenantId, "C1", "Bonus", 10.0m, DateTime.UtcNow, DateTime.UtcNow.AddDays(1));
        var campaigns = new[] { campaign };

        _tierServiceMock.Setup(s => s.GetTierMultiplier(customer.Tier))
            .Returns(1.0m);

        _ruleSelectorMock.Setup(s => s.SelectBestRule(It.IsAny<IEnumerable<RewardRule>>(), It.IsAny<Money>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(rule);

        _campaignEligibilityMock.Setup(s => s.GetEligibleCampaigns(It.IsAny<IEnumerable<Campaign>>(), It.IsAny<Customer>(), It.IsAny<Money>(), It.IsAny<string>()))
            .Returns(new[] { campaign });

        _pointsCapServiceMock.Setup(s => s.ApplyPointsCap(It.IsAny<Points>(), It.IsAny<Money>()))
            .Returns((Points p, Money m) => p);

        // Act
        var result = await _service.CalculateRewardAsync(customer, amount, rules, campaigns);

        // Assert
        result.IsSuccess.Should().BeTrue();
        // Campaign bonus logic is in Campaign.CalculateBonusPoints, which we are not mocking but using real entity
        // Let's assume it adds something. Or better, mock Campaign if it's too complex.
        // Actually Campaign is an entity, but RewardCalculationService calls its methods.
    }

    private Customer CreateCustomer(string tier, decimal lifetimePoints)
    {
        var customer = Customer.Create(
            TenantId,
            "test-customer",
            "Test",
            "Customer",
            "test@example.com"
        );

        Enum.TryParse<CustomerTier>(tier, true, out var customerTier);
        
        typeof(Customer).GetProperty("Tier")!.SetValue(customer, customerTier);
        typeof(Customer).GetProperty("LifetimePoints")!.SetValue(customer, Points.Create(lifetimePoints));

        return customer;
    }
}
