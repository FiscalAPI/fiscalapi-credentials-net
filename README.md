# Fiscalapi Credentials

[![Nuget](https://img.shields.io/nuget/v/Fiscalapi.Credentials)](https://www.nuget.org/packages/Fiscalapi.Credentials)
[![License](https://img.shields.io/github/license/FiscalAPI/fiscalapi-credentials-net)](https://github.com/FiscalAPI/fiscalapi-credentials-net/blob/main/LICENSE)

Biblioteca para trabajar con archivos **CSD** y **FIEL** del SAT de manera sencilla en .NET. **`Credentials`** simplifica la firma (sellado), la verificación de firmas, el cálculo de hashes (por ejemplo, para servicios de descarga masiva de XML y metadatos), así como la obtención de información relevante de los certificados y llaves públicas del SAT.  

La firma digital es un proceso criptográfico que garantiza la autenticidad, integridad y no repudio de un documento o mensaje. En México, el SAT requiere que los contribuyentes utilicen un **Certificado de Sello Digital (CSD)** para firmar (sellar) las facturas, mientras que una **Firma Electrónica Avanzada (FIEL)** se utiliza para firmar documentos de cualquier otro tipo (contratos, acuerdos, cotizaciones, correos, etc) de manera legalmente válida.

## Tabla de Contenido

1. [Acerca de la Librería](#Características)  
2. [Instalación](#Instalación)  
3. [Uso Básico](#uso-básico)  
   - [Certificado (`Certificate`)](#uso-del-certificado)  
   - [Clave Privada (`PrivateKey`)](#uso-de-la-clave-privada)  
   - [Credencial (`Credential`)](#uso-del-objeto-credential)  
4. [Acerca de los Archivos CSD y FIEL](#acerca-de-los-archivos-de-certificado-y-llave-privada)  
5. [Compatibilidad](#compatibilidad)  
6. [Roadmap](#roadmap)  
7. [Contribuciones](#contribuciones)  
8. [🤝 Contribuir](#-contribuir)  
9. [🐛 Reportar Problemas](#-reportar-problemas)  
10. [📄 Licencia](#-licencia)  
11. [🔗 Enlaces Útiles](#-enlaces-útiles)  


## 🚀 Características

- **Firmar (sellar) documentos**: Utilizar CSD o FIEL para generar firmas digitales que cumplen con los lineamientos del SAT.  
- **Verificar firmas**: Validar que la firma fue generada correctamente con la llave privada asociada.  
- **Calcular hashes**: Útil para servicios de descarga masiva de XML del SAT, comparaciones de integridad, etc.  
- **Obtener datos del certificado**: Número de serie, fecha de vigencia, RFC, razón social, entre otros.  
- **Generar archivos PFX (PKCS#12)** a partir de los archivos proporcionados por el SAT sin necesidad de `openssl`.  

### Clases Principales

1. **`Certificate`**  
   - Maneja todo lo relacionado al `.cer` (X.509 DER).  
   - Obtiene número de certificado, versión, periodo de vigencia, etc.  
   - Convierte de X.509 DER a X.509 PEM.  

2. **`PrivateKey`**  
   - Maneja todo lo relacionado al `.key` (PKCS#8 DER).  
   - Convierte la clave de PKCS#8 DER a PKCS#8 PEM.  
   - Requiere la contraseña de la llave privada para operar.  

3. **`Credential`**  
   - Une `Certificate` y `PrivateKey`.  
   - Permite firmar, validar firmas, crear archivos PFX, etc.  
   - Identifica si es CSD o FIEL y verifica su vigencia.  

## 📦Instalación

**NuGet Package Manager**:

```bash
NuGet\Install-Package Fiscalapi.Credentials
```

**.NET CLI**:

```bash
dotnet add package Fiscalapi.Credentials
```

## Ejemplos de uso

### Uso del Certificado

```csharp
// Cargar el archivo .cer
var cerPath = @"C:\Users\Usuario\Desktop\cer.cer";
var cerBytes = File.ReadAllBytes(cerPath);
var cerBase64 = Convert.ToBase64String(cerBytes);

// Crear instancia de Certificate
var certificate = new Certificate(cerBase64); 
// (Por ejemplo, cerBase64 puede guardarse en BD y luego recuperarse)

// Mostrar información básica del certificado
Console.WriteLine($"PlainBase64: {certificate.PlainBase64}");
Console.WriteLine($"RFC: {certificate.Rfc}");
Console.WriteLine($"Razón Social: {certificate.Organization}");
Console.WriteLine($"Serial Number: {certificate.SerialNumber}");
Console.WriteLine($"Certificate Number: {certificate.CertificateNumber}");
Console.WriteLine($"Válido desde: {certificate.ValidFrom}");
Console.WriteLine($"Válido hasta: {certificate.ValidTo}");
Console.WriteLine($"¿Es FIEL?: {certificate.IsFiel()}");
Console.WriteLine($"¿Está vigente?: {certificate.IsValid()}"); // ValidTo > DateTime.Now

// Convertir X.509 DER base64 a X.509 PEM
var pemCertificate = certificate.GetPemRepresentation();
File.WriteAllText("MyPemCertificate.pem", pemCertificate);
```

### Uso de la Clave Privada

```csharp
// Cargar el archivo .key
var keyPath = @"C:\Users\Usuario\Desktop\key.key";
var keyBytes = File.ReadAllBytes(keyPath);
var keyBase64 = Convert.ToBase64String(keyBytes);

// Crear instancia de PrivateKey con la contraseña
var privateKey = new PrivateKey(keyBase64, "TuPasswordDeLaLlave");

// Convertir PKCS#8 DER a PKCS#8 PEM
var PemPrivateKey = privateKey.GetPemRepresentation();
File.WriteAllText("MyPemPrivateKey.pem", PemPrivateKey);
```

### Uso del Objeto Credential

```csharp
// Crear instancia de Credential a partir de certificate y privateKey
var cred = new Credential(certificate, privateKey);

var dataToSign = "Hola Mundo"; // Reemplazar con cadena original u otro contenido

// Firmar datos
var signedBytes = cred.SignData(dataToSign);

// Verificar firma
var originalDataBytes = Encoding.UTF8.GetBytes(dataToSign);
var isValidSignature = cred.VerifyData(originalDataBytes, signedBytes);
Console.WriteLine($"¿Firma Válida?: {isValidSignature}");

// Crear archivo PFX (PKCS#12)
var pfxBytes = cred.CreatePFX();
File.WriteAllBytes("MyPFX.pfx", pfxBytes);

// Calcular y verificar hash (por ejemplo, para descarga masiva XML)
var dataToHash = "XML canonical representation";
var hashBase64 = cred.CreateHash(dataToHash);
var isHashValid = cred.VerifyHash(dataToHash, hashBase64);
Console.WriteLine($"¿Hash Válido?: {isHashValid}");

// Información adicional
Console.WriteLine($"Tipo de Credencial: {cred.CredentialType}");  // Enum: Fiel || Csd
Console.WriteLine($"¿Es FIEL válida?: {cred.IsValidFiel()}");
```


## Acerca de los Archivos de Certificado y Llave Privada

Los certificados provistos por el SAT suelen estar en formato **X.509 DER** (`.cer`), mientras que las llaves privadas están en **PKCS#8 DER** (`.key`). Estos formatos **no** se pueden usar directamente en la mayoría de las bibliotecas de C#, pero **`Credentials`** resuelve este problema convirtiéndolos internamente a **PEM** (`.pem`) sin requerir `openssl`.

Esta conversión consiste básicamente en:

1. Codificar en **Base64** el contenido DER.
2. Separar en líneas de 64 caracteres.
3. Agregar las cabeceras y pies específicos para certificados y llaves privadas.

Por lo tanto, no necesitas realizar la conversión manual ni depender de utilerías externas para utilizar tus archivos **CSD** o **FIEL**.


## Compatibilidad

- Compatible con **.NET 6**, **.NET 8** y **.NET 9**  WinForms, WPF, Console, ASP.NET, Blazor, MVC, WebApi.   
- Mantenemos la compatibilidad con al menos la versión LTS más reciente de .NET.  
- Se sigue el [**Versionado Semántico 2.0.0**]([docs/SEMVER.md](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning?tabs=semver20sort)), por lo que puedes confiar en que las versiones nuevas no romperán tu aplicación de forma inesperada.
## Roadmap

- [x] Conversión de **X.509 DER** a **X.509 PEM** (SAT .cer).  
- [x] Conversión de **PKCS#8 DER** a **PKCS#8 PEM** (SAT .key).  
- [x] Creación de archivo .PFX (PKCS#12) a partir de los archivos X.509 PEM y PKCS#8 PEM.  
- [x] Firma de datos con `SHA256withRSA`.  
- [x] Verificación de datos firmados.  
- [x] Cálculo y verificación de hash para servicios SAT de descarga masiva de XML.  
- [ ] Persistencia de CSD y FIEL utilizando Entity Framework Core y bases de datos relacionales.  


## 🤝 Contribuir

1. Haz un fork del repositorio.  
2. Crea una rama para tu feature: `git checkout -b feature/AmazingFeature`.  
3. Realiza commits de tus cambios: `git commit -m 'Add some AmazingFeature'`.  
4. Sube tu rama: `git push origin feature/AmazingFeature`.  
5. Abre un Pull Request en GitHub.


## 🐛 Reportar Problemas

1. Asegúrate de usar la última versión del SDK.  
2. Verifica si el problema ya fue reportado.  
3. Proporciona un ejemplo mínimo reproducible.  
4. Incluye los mensajes de error completos.


## 📄 Licencia

Este proyecto está licenciado bajo la Licencia **MPL**. Consulta el archivo [LICENSE](LICENSE.txt) para más detalles.


## 🔗 Enlaces Útiles

- [Documentación Oficial](https://docs.fiscalapi.com)  
- [Portal de FiscalAPI](https://fiscalapi.com)  
- [Facturar en WinForms/Console](https://github.com/FiscalAPI/fiscalapi-samples-net-winforms)  
- [Facturar en ASP.NET](https://github.com/FiscalAPI/fiscalapi-samples-net-aspnet)

---

Desarrollado con ❤️ por [Fiscalapi](https://www.fiscalapi.com)
