using Newtonsoft.Json.Linq;

namespace BD.WTTS;

public static class JTokenExtensions
{
    public static JToken ReplacePath(this JToken root, string path, JToken? newValue)
    {
        if (root == null || path == null || newValue == null)
            throw new ArgumentNullException();

        foreach (var value in root.SelectTokens(path).ToList())
        {
            if (value == root)
                root = newValue.DeepClone();
            else
                value.Replace(newValue.DeepClone());
        }

        return root;
    }

    /// <summary>
    /// Replaces a token's child or children with a new value. This version takes a string as input, and outputs as one.
    /// </summary>
    /// <param name="jsonString">JSON string to edit</param>
    /// <param name="path">Selector path of key to be edited</param>
    /// <param name="newValue">New value for key/s</param>
    /// <returns>Modified JSON string</returns>
    public static string ReplacePath(string jsonString, string path, JToken? newValue)
    {
        return JToken.Parse(jsonString).ReplacePath(path, newValue).ToString();
    }
}
