namespace PharmacyMS.Application.DTOs;
public sealed record CategoryDto(int Id, string Name, string? Description);
public sealed record CategoryInput(string Name, string? Description);
