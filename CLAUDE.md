# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

`Fiscalapi.Credentials` is a single-project .NET library (NuGet: `Fiscalapi.Credentials`) that wraps Mexican SAT cryptographic credentials — **CSD** (Certificado de Sello Digital) and **FIEL / e.firma** — so callers can sign/verify data, hash for the SAT XML mass-download service, and produce PFX (PKCS#12) bundles without shelling out to `openssl`.

Target frameworks: `net8.0;net9.0`. Root namespace: `Fiscalapi.Credentials`. See `fiscalapi-credentials.csproj` — `GeneratePackageOnBuild=True`, so every `Release` build produces a `.nupkg`. The `<Version>` element there is the single source of truth (bump it to release a new version; the CI workflow extracts it from the csproj).

## Commands

```bash
dotnet restore
dotnet build --configuration Release           # also emits the .nupkg (GeneratePackageOnBuild)
dotnet pack  --configuration Release --output ./nupkg
```

Publishing is handled by `.github/workflows/CICD.yml` (manual `workflow_dispatch`), which reads the version from the csproj and pushes to nuget.org using the `NUGET_KEY` secret.

There is **no test project** in the solution — do not claim tests pass; there is nothing to run. If you add tests, add a new test project to `fiscalapi-credentials.sln`.

## Architecture

The public API is three cooperating classes in `Core/`, backed by small helpers in `Common/`.

- **`Certificate`** (`Core/Certificate.cs`) — wraps a `.cer` (X.509 DER) supplied as a base64 string. Internally builds an `X509Certificate2` on construction and exposes SAT-relevant metadata (`Rfc`, `Organization`, `SerialNumber`, `CertificateNumber`, `ValidFrom`/`ValidTo`, `IsFiel()`, `IsValid()`). Also converts DER → PEM via `GetPemRepresentation()`.
- **`PrivateKey`** (`Core/PrivateKey.cs`) — wraps a `.key` (PKCS#8 DER) plus its password. Handles DER → PEM conversion and low-level `SignData` / `VerifyData` using a caller-provided `HashAlgorithmName` and `RSASignaturePadding`.
- **`Credential`** (`Core/Credential.cs`) — composes `ICertificate` + `IPrivateKey`. At construction it calls `X509Certificate2.CreateFromPem(PemCertificate, PemPrivateKey)` once and reuses that instance for `CreatePFX()`. Signing/verifying is delegated to the `PrivateKey`. The `CredentialType` (Csd vs Fiel) is derived from the certificate, not stored.

Always program against the interfaces (`ICertificate`, `IPrivateKey`, `ICredential`) — the README and public surface are built around them.

### Signature algorithm is instance state (thread-safety invariant)

`Credential.SignatureAlgorithm` and `Credential.SignaturePadding` are **per-instance** properties, defaulting to `SHA256` + `Pkcs1`. Commit `fb11729` deliberately moved these off of a static `CredentialSettings` to avoid cross-thread mutation. Do **not** reintroduce static signature state. Use the fluent helpers to switch modes:

- `ConfigureAlgorithmForInvoicing()` → `SHA256` (CFDI sealing).
- `ConfigureAlgorithmForXmlDownloader()` → `SHA1` (SAT mass-download service).

Note a quirk: `CreateHash` / `VerifyHash` are hard-coded to `SHA1` regardless of `SignatureAlgorithm`. If you need a SHA256 hash, don't assume `ConfigureAlgorithmForInvoicing()` changes hashing behavior.

### `CredentialSettings.OriginalStringPath` (the one remaining static)

`GetOriginalStringByXmlString` transforms a CFDI XML into its "cadena original" via `XslCompiledTransform`. The XSLT file path is read from the static `CredentialSettings.OriginalStringPath` — callers must set this once at startup (e.g. to the SAT-provided `cadenaoriginal_4_0.xslt`). It is intentionally static because the XSLT is a deployment artifact, not per-request state. The transformer enables `EnableDocumentFunction` and `EnableScript` because the official SAT XSLT uses both.

### Input format convention

Throughout the API, `.cer` and `.key` bytes are passed as **base64 strings**, not byte arrays or file paths. `Certificate.PlainBase64` and `PrivateKey.PlainBase64` preserve the original base64 so credentials can be round-tripped through a database without re-reading the files.

## Conventions

- Code and comments are a mix of Spanish (README, user-facing docs) and English (XML doc comments, identifiers). Match the surrounding file — don't translate one into the other mid-file.
- `Common/HelpersToVerify.cs` is entirely commented-out legacy Chilkat code kept for historical reference; don't "clean it up" unless the user asks.
- The README is packed into the NuGet (`<PackageReadmeFile>README.md</PackageReadmeFile>`), so README edits ship to NuGet consumers on the next release.
