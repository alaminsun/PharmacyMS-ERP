# PharmacyMS ERP v1.0.0 — Commercial Licensing Review

**Status:** Preliminary review; **NOT cleared for commercial source-code redistribution**.

**Scope:** Local PharmacyMS release candidate on `test/mediatr-12-5`, reviewed October 21, 2026. Package vulnerabilities and selected business workflows have been tested. Complete dependency licensing, third-party assets, source ownership, and distribution rights have not yet been fully audited. This document is not legal advice.

## 1. MediatR 12.5.0 - License and compatibility review

The final local release candidate references `MediatR` version `12.5.0` in `Backend/PharmacyMS.Application/PharmacyMS.Application.csproj`.

NuGet identifies MediatR 12.5.0 as licensed under Apache License 2.0. This is a permissive license that generally permits commercial use and redistribution, subject to the applicable conditions and preservation of required notices.

Official package:
https://www.nuget.org/packages/MediatR/12.5.0

License:
https://www.apache.org/licenses/LICENSE-2.0

### Compatibility verification

The following tests passed on the local `test/mediatr-12-5` Git branch:

- ASP.NET Core Release build
- xUnit tests: 4 passed, 0 failed
- NuGet vulnerability audit: no known vulnerabilities reported
- LocalDB migrations and API health checks
- Admin provisioning and login
- JWT authentication and Admin permissions retrieval
- Medicines query
- Supplier and medicine creation
- Purchase of 10 units and stock increase to 10
- Sale of 3 units and remaining stock verification at 7

These tests support compatibility for the tested workflows. They do not establish full test coverage of every feature.

### Remaining licensing work

- Preserve required Apache-2.0 license and attribution notices.
- Review all other direct and transitive dependencies.
- Verify rights to redistribute third-party assets, templates, fonts, icons, and any included code.
- Confirm source ownership and compatibility with the intended marketplace license.

The overall commercial redistribution review remains preliminary.

## 2. Direct packages spotted in repository

**.NET:** MediatR 12.5.0 (Apache-2.0), FluentValidation.DependencyInjectionExtensions 12.1.1, Microsoft.EntityFrameworkCore packages 10.0.11, ASP.NET Core Identity/JWT packages, Swashbuckle.AspNetCore 6.6.2, and other dependencies. Verify exact versions against the final restored package graph.

**Angular/npm:** Angular runtime packages updated to 21.2.25, Angular CLI/build updated to 21.2.27, and Vite pinned to 7.3.7 through overrides. Verify the complete final dependency graph against package-lock.json.

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

- [x] MediatR 12.5.0 Apache-2.0 package license verified; tested application workflows passed. Required redistribution notices remain under review.
- [ ] .NET and npm direct/transitive dependency list reviewed.
- [ ] Required third-party license texts, copyright notices, and attribution included.
- [ ] Icons, fonts, images, UI templates, and copied code are licensed for distribution.
- [ ] Confirm all application code is original or properly licensed for resale, including prior employer/client material.
- [ ] Review latest local code, not just older GitHub `main`.
- [ ] Do not include passwords, tokens, `.vs`, `node_modules`, `bin`, `obj`, `dist`, or production data in the buyer ZIP.
- [ ] Add a product license notice consistent with the marketplace license **without overriding third-party licenses**.

**Decision:** Finish the checks above before treating `PharmacyMS-ERP-v1.0.0.zip` as commercially ready.
