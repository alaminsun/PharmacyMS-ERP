namespace PharmacyMS.Application.DTOs;
public sealed record ShelfDto(int Id, string Name, string Number);
public sealed record ShelfInput(string Name, string Number);
