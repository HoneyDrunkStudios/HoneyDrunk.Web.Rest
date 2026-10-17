using HoneyDrunk.Web.Rest.Abstractions.Paging;
using Shouldly;

namespace HoneyDrunk.Web.Rest.Tests.Abstractions;

/// <summary>
/// Tests for <see cref="PageRequest"/> and <see cref="PageResult{T}"/>.
/// </summary>
public sealed class PagingTests
{
    /// <summary>
    /// Verifies that PageRequest has correct default values.
    /// </summary>
    [Fact]
    public void PageRequest_DefaultValues_AreCorrect()
    {
        PageRequest request = new();

        request.PageNumber.ShouldBe(PageRequest.DefaultPageNumber);
        request.PageSize.ShouldBe(PageRequest.DefaultPageSize);
        request.NormalizedPageNumber.ShouldBe(1);
        request.NormalizedPageSize.ShouldBe(20);
    }

    /// <summary>
    /// Verifies that PageRequest.Default returns correct default values.
    /// </summary>
    [Fact]
    public void PageRequest_Default_ReturnsCorrectValues()
    {
        PageRequest request = PageRequest.Default();

        request.PageNumber.ShouldBe(1);
        request.PageSize.ShouldBe(20);
    }

    /// <summary>
    /// Verifies that PageRequest.Create sets values correctly.
    /// </summary>
    [Fact]
    public void PageRequest_Create_SetsValuesCorrectly()
    {
        PageRequest request = PageRequest.Create(5, 50);

        request.PageNumber.ShouldBe(5);
        request.PageSize.ShouldBe(50);
    }

    /// <summary>
    /// Verifies that PageRequest.Skip calculates correctly for various inputs.
    /// </summary>
    [Fact]
    public void PageRequest_Skip_CalculatesCorrectly_Page1()
    {
        PageRequest request = PageRequest.Create(1, 20);
        request.Skip.ShouldBe(0);
    }

    /// <summary>
    /// Verifies that PageRequest.Skip calculates correctly for page 2.
    /// </summary>
    [Fact]
    public void PageRequest_Skip_CalculatesCorrectly_Page2()
    {
        PageRequest request = PageRequest.Create(2, 20);
        request.Skip.ShouldBe(20);
    }

    /// <summary>
    /// Verifies that PageRequest.Skip calculates correctly for page 3 with 10 size.
    /// </summary>
    [Fact]
    public void PageRequest_Skip_CalculatesCorrectly_Page3_Size10()
    {
        PageRequest request = PageRequest.Create(3, 10);
        request.Skip.ShouldBe(20);
    }

    /// <summary>
    /// Verifies that PageRequest.Skip calculates correctly for page 5 with 25 size.
    /// </summary>
    [Fact]
    public void PageRequest_Skip_CalculatesCorrectly_Page5_Size25()
    {
        PageRequest request = PageRequest.Create(5, 25);
        request.Skip.ShouldBe(100);
    }

    /// <summary>
    /// Verifies that NormalizedPageNumber returns 1 for zero.
    /// </summary>
    [Fact]
    public void PageRequest_NormalizedPageNumber_ReturnsMinimumOf1_ForZero()
    {
        PageRequest request = new() { PageNumber = 0 };
        request.NormalizedPageNumber.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that NormalizedPageNumber returns 1 for negative.
    /// </summary>
    [Fact]
    public void PageRequest_NormalizedPageNumber_ReturnsMinimumOf1_ForNegative()
    {
        PageRequest request = new() { PageNumber = -1 };
        request.NormalizedPageNumber.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that NormalizedPageNumber returns same value for positive.
    /// </summary>
    [Fact]
    public void PageRequest_NormalizedPageNumber_ReturnsSameValue_ForPositive()
    {
        PageRequest request = new() { PageNumber = 5 };
        request.NormalizedPageNumber.ShouldBe(5);
    }

    /// <summary>
    /// Verifies that NormalizedPageSize returns 1 for zero.
    /// </summary>
    [Fact]
    public void PageRequest_NormalizedPageSize_Returns1_ForZero()
    {
        PageRequest request = new() { PageSize = 0 };
        request.NormalizedPageSize.ShouldBe(1);
    }

    /// <summary>
    /// Verifies that NormalizedPageSize clamps to max of 100.
    /// </summary>
    [Fact]
    public void PageRequest_NormalizedPageSize_ClampsToMax100()
    {
        PageRequest request = new() { PageSize = 150 };
        request.NormalizedPageSize.ShouldBe(100);
    }

    /// <summary>
    /// Verifies that NormalizedPageSize returns same for value in range.
    /// </summary>
    [Fact]
    public void PageRequest_NormalizedPageSize_ReturnsSame_ForValueInRange()
    {
        PageRequest request = new() { PageSize = 50 };
        request.NormalizedPageSize.ShouldBe(50);
    }

    /// <summary>
    /// Verifies that PageResult.Empty creates correct empty result.
    /// </summary>
    [Fact]
    public void PageResult_Empty_CreatesCorrectEmptyResult()
    {
        PageResult<string> result = PageResult<string>.Empty();

        result.Items.ShouldBeEmpty();
        result.PageNumber.ShouldBe(1);
        result.PageSize.ShouldBe(PageRequest.DefaultPageSize);
        result.TotalCount.ShouldBe(0);
        result.TotalPages.ShouldBe(0);
        result.HasPreviousPage.ShouldBeFalse();
        result.HasNextPage.ShouldBeFalse();
    }

    /// <summary>
    /// Verifies that PageResult.Empty with custom values works correctly.
    /// </summary>
    [Fact]
    public void PageResult_Empty_WithCustomValues_WorksCorrectly()
    {
        PageResult<string> result = PageResult<string>.Empty(3, 10);

        result.Items.ShouldBeEmpty();
        result.PageNumber.ShouldBe(3);
        result.PageSize.ShouldBe(10);
        result.TotalCount.ShouldBe(0);
    }

    /// <summary>
    /// Verifies that PageResult.Create sets all properties correctly.
    /// </summary>
    [Fact]
    public void PageResult_Create_SetsAllPropertiesCorrectly()
    {
        List<string> items = ["item1", "item2", "item3"];

        PageResult<string> result = PageResult<string>.Create(items, 2, 10, 35);

        result.Items.ShouldBe(items);
        result.PageNumber.ShouldBe(2);
        result.PageSize.ShouldBe(10);
        result.TotalCount.ShouldBe(35);
    }

    /// <summary>
    /// Verifies that TotalPages calculates correctly for exact division.
    /// </summary>
    [Fact]
    public void PageResult_TotalPages_CalculatesCorrectly_ExactDivision()
    {
        PageResult<string> result = PageResult<string>.Create([], 1, 10, 100);
        result.TotalPages.ShouldBe(10);
    }

    /// <summary>
    /// Verifies that TotalPages rounds up for partial page.
    /// </summary>
    [Fact]
    public void PageResult_TotalPages_RoundsUp_ForPartialPage()
    {
        PageResult<string> result = PageResult<string>.Create([], 1, 10, 101);
        result.TotalPages.ShouldBe(11);
    }

    /// <summary>
    /// Verifies that TotalPages returns 0 when TotalCount is 0.
    /// </summary>
    [Fact]
    public void PageResult_TotalPages_ReturnsZero_WhenTotalCountIsZero()
    {
        PageResult<string> result = PageResult<string>.Create([], 1, 10, 0);
        result.TotalPages.ShouldBe(0);
    }

    /// <summary>
    /// Verifies that TotalPages returns 0 when PageSize is 0.
    /// </summary>
    [Fact]
    public void PageResult_TotalPages_ReturnsZero_WhenPageSizeIsZero()
    {
        PageResult<string> result = new()
        {
            Items = [],
            PageNumber = 1,
            PageSize = 0,
            TotalCount = 100,
        };

        result.TotalPages.ShouldBe(0);
    }

    /// <summary>
    /// Verifies that HasPreviousPage is false for page 1.
    /// </summary>
    [Fact]
    public void PageResult_HasPreviousPage_IsFalse_ForPage1()
    {
        PageResult<string> result = PageResult<string>.Create([], 1, 10, 100);
        result.HasPreviousPage.ShouldBeFalse();
    }

    /// <summary>
    /// Verifies that HasPreviousPage is true for page 2.
    /// </summary>
    [Fact]
    public void PageResult_HasPreviousPage_IsTrue_ForPage2()
    {
        PageResult<string> result = PageResult<string>.Create([], 2, 10, 100);
        result.HasPreviousPage.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies that HasNextPage is true when more pages exist.
    /// </summary>
    [Fact]
    public void PageResult_HasNextPage_IsTrue_WhenMorePagesExist()
    {
        PageResult<string> result = PageResult<string>.Create([], 1, 10, 100);
        result.HasNextPage.ShouldBeTrue();
    }

    /// <summary>
    /// Verifies that HasNextPage is false on last page.
    /// </summary>
    [Fact]
    public void PageResult_HasNextPage_IsFalse_OnLastPage()
    {
        PageResult<string> result = PageResult<string>.Create([], 10, 10, 100);
        result.HasNextPage.ShouldBeFalse();
    }
}
