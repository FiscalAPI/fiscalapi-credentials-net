
namespace Fiscalapi.Credentials.Common;

public static class CredentialSettings
{
    //Algoritmos ahora son parte de la instancia (thread-safe)

    /// <summary>
    /// Path of the CadenaOriginal.xslt file to do XML data transformation using an XSLT stylesheet.
    /// </summary>
    public static string? OriginalStringPath { get; set; }
}