namespace QuartileChallenge.Core.DTOs;

public record ProductDto(
    Guid Id,
    string Name,
    string Description,
    decimal Price,
    Guid StoreId,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    bool IsActive
);

public record CreateProductDto(
    string Name,
    string Description,
    decimal Price,
    Guid StoreId
);

public record UpdateProductDto(
    string Name,
    string Description,
    decimal Price
);