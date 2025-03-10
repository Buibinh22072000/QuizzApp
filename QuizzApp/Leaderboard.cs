using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.IO;

public static class Leaderboard
{
    private static readonly string filePath = "scores.json";

    public static void SaveScores(ObservableCollection<UserScore> scores)
    {
        var json = JsonConvert.SerializeObject(scores, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public static ObservableCollection<UserScore> LoadScores()
    {
        if (!File.Exists(filePath))
        {
            return new ObservableCollection<UserScore>();
        }

        var json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<ObservableCollection<UserScore>>(json);
    }

    public static void ClearScores()
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}