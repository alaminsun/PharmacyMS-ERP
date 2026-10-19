namespace PharmacyMS.Application.DTOs;
public sealed record CustomerDto(int Id, string Code, string Name, string? Email, string? Phone, string? Address, string? Notes);
public sealed record CustomerInput(string Code, string Name, string? Email, string? Phone, string? Address, string? Notes);
