using FluentAssertions;
using Nebula.Application.DTOs;
using Nebula.Application.Services;

namespace Nebula.Tests.Unit.Dashboard;

public class OpportunityFlowNodeEmphasisCalculatorTests
{
    [Fact]
    public void Compute_AssignsBottleneckBlockedActiveAndNormal()
    {
        var nodes = new List<OpportunityFlowNodeDto>
        {
            new("Received", "Received", false, 1, "intake", 5, 0, 3, 2.1),
            new("Triaging", "Triaging", false, 2, "triage", 9, 5, 7, 3.5),
            new("UwReview", "UW Review", false, 3, "review", 4, 4, 2, 12.4),
            new("QuotePrep", "Quote Prep", false, 4, "decision", 1, 2, 1, 1.3),
            new("Bound", "Bound", true, 5, "decision", 18, 9, 0, 0.8),
        };

        var emphasis = OpportunityFlowNodeEmphasisCalculator.Compute(nodes);

        emphasis["Triaging"].Should().Be("bottleneck");
        emphasis["UwReview"].Should().Be("blocked");
        emphasis["QuotePrep"].Should().Be("active");
        emphasis["Received"].Should().Be("normal");
        emphasis.ContainsKey("Bound").Should().BeFalse();
    }

    [Fact]
    public void Compute_WhenBottleneckAndBlockedAreSameNode_PrioritizesBottleneck()
    {
        var nodes = new List<OpportunityFlowNodeDto>
        {
            new("A", "A", false, 1, "intake", 10, 0, 3, 15.0),
            new("B", "B", false, 2, "triage", 6, 4, 4, 5.0),
            new("C", "C", false, 3, "review", 2, 3, 2, 2.0),
        };

        var emphasis = OpportunityFlowNodeEmphasisCalculator.Compute(nodes);

        emphasis["A"].Should().Be("bottleneck");
        emphasis["B"].Should().Be("blocked");
        emphasis["C"].Should().Be("active");
    }

    [Fact]
    public void Compute_WhenNoActiveCounts_ReturnsNormalForNonTerminalNodes()
    {
        var nodes = new List<OpportunityFlowNodeDto>
        {
            new("A", "A", false, 1, "intake", 0, 0, 0, null),
            new("B", "B", false, 2, "triage", 0, 0, 0, null),
            new("C", "C", true, 3, "decision", 5, 3, 0, null),
        };

        var emphasis = OpportunityFlowNodeEmphasisCalculator.Compute(nodes);

        emphasis.Should().HaveCount(2);
        emphasis.Values.Should().OnlyContain(value => value == "normal");
    }
}
