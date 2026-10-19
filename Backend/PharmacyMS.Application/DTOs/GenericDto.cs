namespace PharmacyMS.Application.DTOs;
public sealed record GenericDto(int Id, string Code, string Name);
public sealed record GenericInput(string Code, string Name);
