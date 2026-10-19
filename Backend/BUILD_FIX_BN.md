# PharmacyMS: CS0246 SignInManager<> এবং CS0006 build fix

## কারণ
`PharmacyMS.Infrastructure` একটি `Microsoft.NET.Sdk` class library। `Microsoft.AspNetCore.Identity.EntityFrameworkCore` package থাকলেও ASP.NET Core shared framework-এ থাকা `SignInManager<TUser>` resolve করতে framework reference দরকার। `IdentityService.cs`-এ `using Microsoft.AspNetCore.Identity;` আগে থেকেই আছে।

## Fix
`PharmacyMS.Infrastructure/PharmacyMS.Infrastructure.csproj`-এ এটি যোগ করা হয়েছে:

```xml
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

## Windows PowerShell-এ পরীক্ষা
Solution root-এ:

```powershell
dotnet restore .\PharmacyMS.slnx
dotnet clean .\PharmacyMS.slnx
dotnet build .\PharmacyMS.Infrastructure\PharmacyMS.Infrastructure.csproj
dotnet build .\PharmacyMS.slnx
```

`CS0006` error সাধারণত Infrastructure compile failure-এর ফলে আসে। আগে Infrastructure project build নিশ্চিত করুন।

যদি error থাকে, `dotnet build .\PharmacyMS.Infrastructure\PharmacyMS.Infrastructure.csproj`-এর প্রথম error-এর পূর্ণ text দিন।

এই fix শুধু compile reference পরিবর্তন করে; database/schema/data বদলায় না। এই runtime-এ .NET SDK নেই, তাই project-এর actual compile চালানো হয়নি।
