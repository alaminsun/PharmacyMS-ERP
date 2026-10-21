# PharmacyMS ERP v1.0.0 — Commercial Licensing Review

**Status:** Preliminary review; **NOT cleared for commercial source-code redistribution**.

**Scope:** GitHub `alaminsun/PharmacyMS-ERP`, `main` branch checked October 21, 2026. The locally modified working tree, exact restored transitive package graph, artwork/fonts, and included third-party source have **not** been fully examined. This document is not legal advice.

## 1. High-priority issue — MediatR 14.2.0

`Backend/PharmacyMS.Application/PharmacyMS.Application.csproj` references `MediatR` version `14.2.0`. The publisher says versions **13 and later** offer an open-source route under **RPL 1.5** and a separate commercial license. RPL has reciprocal obligations; commercial licensing terms must be checked specifically for redistributing source-code products to multiple buyers. A license key or a log-warning suppression is **not** proof that redistribution is authorized.

Before listing, choose one of the following only after checking its legal consequences:

1. Obtain written clarification/appropriate commercial rights from Lucky Penny Software, including redistribution of the source-code product and rights/requirements for each buyer; or
2. Review the RPL 1.5 obligations with qualified advice and determine whether they are compatible with Vibe96's Standard License and the intended business model; or
3. Evaluate replacement with a differently licensed mediator implementation and retest the entire application.

Do not include or publish a third-party license key in the product ZIP. Do not claim that all dependencies are commercially cleared yet.

Publisher information:
- https://luckypennysoftware.com/faq
- https://mediatr.io/
- https://github.com/LuckyPennySoftware/MediatR/blob/main/LICENSE.md

## 2. Direct packages spotted in repository

**.NET:** MediatR 14.2.0, FluentValidation.DependencyInjectionExtensions 12.1.1, Microsoft.EntityFrameworkCore packages 10.0.11, Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.11, Microsoft.AspNetCore.Authentication.JwtBearer 10.0.11, Swashbuckle.AspNetCore 6.6.2, System.IdentityModel.Tokens.Jwt 8.23.0, System.Security.Cryptography.Pkcs 10.0.13.

**Angular/npm:** @angular packages 21.2.13, RxJS ~7.8.2, tslib ^2.8.1, zone.js ~0.15.1, TypeScript ~5.9.3, Angular CLI/build, Playwright test tools.

These are **inventories, not license approvals**. License details for direct and transitive dependencies and any bundled assets must be checked and their required notices retained.

No root-level `LICENSE`, `LICENSE.md`, `COPYING`, or `LICENSE-NOTICE.md` was found in the inspected GitHub branch; confirm whether your local working copy contains a license. Source ownership and right to distribute cannot be established by repository inspection alone.

## 3. Run locally in PowerShell

From `D:\Sun\PharmacyMS-Updated`:

```powershell
cd "D:\Sun\PharmacyMS-Updated\Backend"
dotnet list PharmacyMS.slnx package --include-transitive
# If supported by your installed SDK, also inspect known vulnerabilities:
dotnet list PharmacyMS.slnx package --vulnerable --include-transitive

cd "D:\Sun\PharmacyMS-Updated\AngularClient"
npm ls --all
npm audit --omit=dev
npm run build
```

NuGet package license information can be checked by opening each package's official NuGet page; npm package license data can be inspected with `npm view <package-name> license`, including resolved dependency versions. Do not infer that a dependency is redistributable solely because `npm install` or `dotnet restore` succeeds.

## 4. Marketplace requirements

Vibe96 Seller Agreement (section 5) asks sellers to have rights to distribute all included code/assets and follow third-party license terms; its Standard License covers the **seller's product rights** but does not override upstream component licenses. Verify that the proposed distribution model is compatible with the actual licenses of all dependencies.

- https://www.vibe96.com/seller-agreement
- https://www.vibe96.com/license

## 5. Source-code and asset clearance checklist

- [ ] MediatR 14.x distribution rights and buyer licensing path resolved in writing.
- [ ] .NET and npm direct/transitive dependency list reviewed.
- [ ] Required third-party license texts, copyright notices, and attribution included.
- [ ] Icons, fonts, images, UI templates, and copied code are licensed for distribution.
- [ ] Confirm all application code is original or properly licensed for resale, including prior employer/client material.
- [ ] Review latest local code, not just older GitHub `main`.
- [ ] Do not include passwords, tokens, `.vs`, `node_modules`, `bin`, `obj`, `dist`, or production data in the buyer ZIP.
- [ ] Add a product license notice consistent with the marketplace license **without overriding third-party licenses**.

**Decision:** Finish the checks above before treating `PharmacyMS-ERP-v1.0.0.zip` as commercially ready.
