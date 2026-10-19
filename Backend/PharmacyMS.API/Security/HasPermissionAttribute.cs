using Microsoft.AspNetCore.Authorization;
namespace PharmacyMS.API.Security;
[AttributeUsage(AttributeTargets.Class|AttributeTargets.Method,AllowMultiple=true)]
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission) => Policy=permission;
}
