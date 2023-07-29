using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Formatting = Newtonsoft.Json.Formatting;

namespace BD.WTTS;

public static class JTokenExtensions
{
    public static JToken ReplacePath<T>(this JToken root, string path, T newValue)
    {
        if (root == null || path == null || newValue == null)
            throw new ArgumentNullException();

        foreach (var value in root.SelectTokens(path).ToList())
        {
            if (value == root)
                root = JToken.FromObject(newValue);
            else
                value.Replace(JToken.FromObject(newValue));
        }

        return root;
    }

    /// <summary>
    /// Replaces a token's child or children with a new value. This version takes a string as input, and outputs as one.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="jsonString">JSON string to edit</param>
    /// <param name="path">Selector path of key to be edited</param>
    /// <param name="newValue">New value for key/s</param>
    /// <returns>Modified JSON string</returns>
    public static string ReplacePath<T>(string jsonString, string path, T newValue)
    {
        return JToken.Parse(jsonString).ReplacePath(path, newValue).ToString();
    }
}
