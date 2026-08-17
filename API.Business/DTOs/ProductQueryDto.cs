using API.Business.Enums;
using System.ComponentModel.DataAnnotations;

namespace API.Business.DTOs;

public class ProductQueryDto : IValidatableObject
{
    public string? Name { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, int.MaxValue)]
    public int PageSize { get; set; } = 10;

    public ProductSortField SortBy { get; set; } = ProductSortField.Id;

    public SortDirection SortDirection { get; set; } = SortDirection.Asc;

    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (MinPrice.HasValue && MinPrice.Value < 0)
        {
            yield return new ValidationResult(
                "MinPrice cannot be negative.",
                [nameof(MinPrice)]);
        }

        if (MaxPrice.HasValue && MaxPrice.Value < 0)
        {
            yield return new ValidationResult(
                "MaxPrice cannot be negative.",
                [nameof(MaxPrice)]);
        }

        if (MinPrice.HasValue &&
            MaxPrice.HasValue &&
            MinPrice.Value > MaxPrice.Value)
        {
            yield return new ValidationResult(
                "MinPrice cannot be greater than MaxPrice.",
                [nameof(MinPrice), nameof(MaxPrice)]);
        }
    }
}