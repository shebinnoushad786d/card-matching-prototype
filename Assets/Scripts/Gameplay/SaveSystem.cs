using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath =>
        Path.Combine(Application.persistentDataPath, "card_matching.json");

    // ✅ SAVE
    public static void Save(GameSaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Game Saved → " + SavePath);
    }

    // ✅ LOAD
    public static GameSaveData Load()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("No Save File Found");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        GameSaveData data = JsonUtility.FromJson<GameSaveData>(json);
        Debug.Log("Game Loaded");
        return data;
    }

    // ✅ CLEAR SAVE
    public static void Clear()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Save Cleared");
        }
    }
}
