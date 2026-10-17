using HoneyDrunk.Web.Rest.AspNetCore.Serialization;
using Shouldly;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HoneyDrunk.Web.Rest.Tests.AspNetCore;

/// <summary>
/// Tests for <see cref="JsonOptionsDefaults"/>.
/// </summary>
public sealed class JsonOptionsDefaultsTests
{
    /// <summary>
    /// Verifies that SerializerOptions uses camelCase naming.
    /// </summary>
    [Fact]
    public void SerializerOptions_UsesCamelCaseNaming()
    {
        TestPerson obj = new() { FirstName = "John", LastName = "Doe" };

        string json = JsonSerializer.Serialize(obj, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"firstName\"");
        json.ShouldContain("\"lastName\"");

        // Verify PascalCase properties are NOT in output (exact match)
        json.ShouldNotContain("\"FirstName\":", Case.Sensitive);
        json.ShouldNotContain("\"LastName\":", Case.Sensitive);
    }

    /// <summary>
    /// Verifies that SerializerOptions is case insensitive for deserialization.
    /// </summary>
    [Fact]
    public void SerializerOptions_IsCaseInsensitiveForDeserialization()
    {
        string jsonCamel = """{"firstName": "John", "lastName": "Doe"}""";
        string jsonPascal = """{"FirstName": "Jane", "LastName": "Smith"}""";

        TestPerson? resultCamel = JsonSerializer.Deserialize<TestPerson>(jsonCamel, JsonOptionsDefaults.SerializerOptions);
        TestPerson? resultPascal = JsonSerializer.Deserialize<TestPerson>(jsonPascal, JsonOptionsDefaults.SerializerOptions);

        resultCamel.ShouldNotBeNull();
        resultCamel.FirstName.ShouldBe("John");

        resultPascal.ShouldNotBeNull();
        resultPascal.FirstName.ShouldBe("Jane");
    }

    /// <summary>
    /// Verifies that SerializerOptions omits null properties.
    /// </summary>
    [Fact]
    public void SerializerOptions_OmitsNullProperties()
    {
        TestPerson obj = new() { FirstName = "John", LastName = null };

        string json = JsonSerializer.Serialize(obj, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"firstName\"");
        json.ShouldNotContain("\"lastName\"");
    }

    /// <summary>
    /// Verifies that SerializerOptions serializes enums as camelCase strings.
    /// </summary>
    [Fact]
    public void SerializerOptions_SerializesEnumsAsCamelCaseStrings()
    {
        TestOrder obj = new() { Status = OrderStatus.InProgress };

        string json = JsonSerializer.Serialize(obj, JsonOptionsDefaults.SerializerOptions);

        // Verify camelCase string format
        json.ShouldContain("\"status\":\"inProgress\"");

        // Verify not a number
        json.ShouldNotContain("\"status\":1", Case.Sensitive);

        // Verify exact case (PascalCase should not appear)
        json.ShouldNotContain("\"status\":\"InProgress\"", Case.Sensitive);
    }

    /// <summary>
    /// Verifies that SerializerOptions deserializes enums from strings.
    /// </summary>
    [Fact]
    public void SerializerOptions_DeserializesEnumsFromStrings()
    {
        string json = """{"status": "completed"}""";

        TestOrder? result = JsonSerializer.Deserialize<TestOrder>(json, JsonOptionsDefaults.SerializerOptions);

        result.ShouldNotBeNull();
        result.Status.ShouldBe(OrderStatus.Completed);
    }

    /// <summary>
    /// Verifies that SerializerOptions produces compact output (not indented).
    /// </summary>
    [Fact]
    public void SerializerOptions_ProducesCompactOutput()
    {
        TestPerson obj = new() { FirstName = "John", LastName = "Doe" };

        string json = JsonSerializer.Serialize(obj, JsonOptionsDefaults.SerializerOptions);

        json.ShouldNotContain("\n");
        json.ShouldNotContain("  ");
    }

    /// <summary>
    /// Verifies that Configure applies settings to provided options.
    /// </summary>
    [Fact]
    public void Configure_AppliesSettingsToProvidedOptions()
    {
        JsonSerializerOptions options = new();

        JsonOptionsDefaults.Configure(options);

        options.PropertyNamingPolicy.ShouldBe(JsonNamingPolicy.CamelCase);
        options.PropertyNameCaseInsensitive.ShouldBeTrue();
        options.DefaultIgnoreCondition.ShouldBe(JsonIgnoreCondition.WhenWritingNull);
        options.WriteIndented.ShouldBeFalse();
    }

    /// <summary>
    /// Verifies that Configure throws on null options.
    /// </summary>
    [Fact]
    public void Configure_ThrowsOnNullOptions()
    {
        Should.Throw<ArgumentNullException>(() => JsonOptionsDefaults.Configure(null!));
    }

    /// <summary>
    /// Verifies that SerializerOptions handles nested objects correctly.
    /// </summary>
    [Fact]
    public void SerializerOptions_HandlesNestedObjectsCorrectly()
    {
        NestedTestObject obj = new()
        {
            OuterName = "Outer",
            Inner = new TestPerson { FirstName = "Inner", LastName = "Object" },
        };

        string json = JsonSerializer.Serialize(obj, JsonOptionsDefaults.SerializerOptions);

        json.ShouldContain("\"outerName\":\"Outer\"");
        json.ShouldContain("\"inner\":{");
        json.ShouldContain("\"firstName\":\"Inner\"");
    }

    /// <summary>
    /// Verifies that SerializerOptions handles collections correctly.
    /// </summary>
    [Fact]
    public void SerializerOptions_HandlesCollectionsCorrectly()
    {
        List<TestPerson> list =
        [
            new() { FirstName = "John", LastName = "Doe" },
            new() { FirstName = "Jane", LastName = "Smith" },
        ];

        string json = JsonSerializer.Serialize(list, JsonOptionsDefaults.SerializerOptions);

        json.ShouldStartWith("[");
        json.ShouldEndWith("]");
        json.ShouldContain("\"firstName\":\"John\"");
        json.ShouldContain("\"firstName\":\"Jane\"");
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.OrderingRules", "SA1201:Elements should appear in the correct order", Justification = "Test file")]
    private enum OrderStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
    }

    private sealed class TestPerson
    {
        public string FirstName { get; set; } = string.Empty;

        public string? LastName { get; set; }
    }

    private sealed class TestOrder
    {
        public OrderStatus Status { get; set; }
    }

    private sealed class NestedTestObject
    {
        public string OuterName { get; set; } = string.Empty;

        public TestPerson? Inner { get; set; }
    }
}
