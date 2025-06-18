namespace QuartileChallenge.Core.DTOs;

public record StoreDto(
    Guid Id,
    Guid CompanyId,
    string Name,
    string Location,
    DateTime CreatedAt,
    DateTime? ModifiedAt,
    bool IsActive
);

public record CreateStoreDto(
    Guid CompanyId,
    string Name,
    string Location
);

public record UpdateStoreDto(
    string Name,
    string Location
);