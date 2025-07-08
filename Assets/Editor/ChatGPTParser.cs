using UnityEngine;
using System.Text.RegularExpressions;

public static class ChatGPTParser
{
    public static string ExtractCodeFromJson(string json)
    {
        try
        {
            
            var match = Regex.Match(json, "\"content\":\\s*\"((?:[^\"\\\\]|\\\\.)*)\"", RegexOptions.Singleline);


            if (!match.Success)
                return "// Failed to extract code.";

            string content = match.Groups[1].Value;

           
            content = content.Replace("\\n", "\n").Replace("\\\"", "\"");

            
            content = Regex.Replace(content, "```(csharp|cs)?", "");
            content = content.Replace("```", "");

            return content.Trim();
        }
        catch
        {
            return "// Exception during code extraction.";
        }
    }
}