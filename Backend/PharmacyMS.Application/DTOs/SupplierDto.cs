namespace PharmacyMS.Application.DTOs;
public sealed record SupplierDto(int Id, string Code, string Name, string? ShortName, string? Email, string? Phone, string? Address, string? Notes);
public sealed record SupplierInput(string Code, string Name, string? ShortName, string? Email, string? Phone, string? Address, string? Notes);
