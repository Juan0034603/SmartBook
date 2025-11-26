using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace SmartBook.Application.Extensions;

internal static class StringExtensions
{

    /// <summary>
    /// Sanitiza un string removiendo etiquetas HTML y caracteres peligrosos.
    /// </summary>
    /// <param name="input">Texto de entrada</param>
    /// <returns>Texto limpio y seguro</returns>
    public static string Sanitize(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var decoded = HttpUtility.HtmlDecode(input);
        string clean = Regex.Replace(decoded, "<.*?>", string.Empty);

        clean = Regex.Replace(clean, "(?i)script|onerror|onload|alert|eval|javascript", string.Empty);

        return clean.Trim();
    }

    public static string RemoveAccents(this string texto)
    {
        var acentos = new Dictionary<char, char>
        {
            { 'á', 'a' }, { 'é', 'e' }, { 'í', 'i' }, { 'ó', 'o' }, { 'ú', 'u' },{ 'Á', 'A' }, { 'É', 'E' }, { 'Í', 'I' }, { 'Ó', 'O' }, { 'Ú', 'U' }
        };

        var sb = new StringBuilder();
        foreach (char c in texto)
        {

            if (acentos.ContainsKey(c))
            {
                sb.Append(acentos[c]);
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }




}
