using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class UsedKey
{
    public string Key { get; set; }
    public int Done { get; set; }
}

public static class KeyHelper
{
    private static string usedKeysFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "usedKeys.json");

    public static List<UsedKey> ReadUsedKeys()
    {
        if (!File.Exists(usedKeysFilePath)) return new List<UsedKey>();

        var json = File.ReadAllText(usedKeysFilePath);
        return JsonConvert.DeserializeObject<List<UsedKey>>(json);
    }

    public static void WriteUsedKeys(List<UsedKey> usedKeys)
    {
        var json = JsonConvert.SerializeObject(usedKeys, Formatting.Indented);
        File.WriteAllText(usedKeysFilePath, json);
    }

    public static bool IsKeyUsed(string key)
    {
        var usedKeys = ReadUsedKeys();
        return usedKeys.Exists(k => k.Key == key && k.Done == 0); // Done = 0 means not yet submitted
    }

    public static void MarkKeyAsUsed(string key)
    {
        var usedKeys = ReadUsedKeys();
        var keyToMark = usedKeys.Find(k => k.Key == key);
        if (keyToMark != null)
        {
            keyToMark.Done = 1; // Mark as done
        }
        WriteUsedKeys(usedKeys);
    }

    public static void AddUsedKey(string key)
    {
        var usedKeys = ReadUsedKeys();
        usedKeys.Add(new UsedKey { Key = key, Done = 0 });
        WriteUsedKeys(usedKeys);
    }
}
