namespace FinanzasApp.Models;

public record CardDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string Bank { get; init; } = "";
    public string Type { get; init; } = "";     // "VISA" | "MASTERCARD"
    public string CreatedAt { get; init; } = "";
}

public record CardUpsert
{
    public string Name { get; init; } = "";
    public string Bank { get; init; } = "";
    public string Type { get; init; } = "";
}

public record CardCategoryDto
{
    public int Id { get; init; }
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public string? Color { get; init; }
    public string? LogoUrl { get; init; }
}

public record CardCategoryUpsert
{
    public string Name { get; init; } = "";
    public string? Description { get; init; }
    public string? Color { get; init; }
    public string? LogoUrl { get; init; }
}

public record CardCategoryRuleDto
{
    public int Id { get; init; }
    public string Keyword { get; init; } = "";
    public int CategoryId { get; init; }
    public string CategoryName { get; init; } = "";
    public int Priority { get; init; }
}

public record CardCategoryRuleUpsert
{
    public string Keyword { get; init; } = "";
    public int CategoryId { get; init; }
    public int Priority { get; init; } = 1;
}

public record LogoRuleDto
{
    public int Id { get; init; }
    public string Keyword { get; init; } = "";
    public string LogoUrl { get; init; } = "";
    public int Priority { get; init; }
}

public record LogoRuleUpsert
{
    public string Keyword { get; init; } = "";
    public string LogoUrl { get; init; } = "";
    public int Priority { get; init; } = 1;
}
