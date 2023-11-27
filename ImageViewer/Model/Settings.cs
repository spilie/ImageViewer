using Newtonsoft.Json;
using System.IO;

namespace ImageViewer.Model;

/// <summary>
/// 設定檔格式
/// </summary>
public class Settings
{
    /// <summary>
    /// 實體
    /// </summary>
    [JsonIgnore]
    private static Settings? INSTANCE { get; set; }

    /// <summary>
    /// 取得實體
    /// </summary>
    /// <returns>Settings</returns>
    public static Settings GetInstance()
    {
        if (INSTANCE == null)
        {
            if (File.Exists("appsettings.json"))
            {
                string json = File.ReadAllText("appsettings.json");
                INSTANCE = JsonConvert.DeserializeObject<Settings>(json);
            }

            INSTANCE ??= GetDefault();
        }

        return INSTANCE;
    }

    /// <summary>
    /// 取得預設值
    /// </summary>
    /// <param name="writeJson">是否寫入Json檔案</param>
    /// <returns>Settings</returns>
    public static Settings GetDefault(bool writeJson = false)
    {
        Settings settings = new();

        if (writeJson)
        {
            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText("appsettings.json", json);
        }

        return settings;
    }

    /// <summary>
    /// 是否自動撥放
    /// </summary>
    public bool IsAutoPlay { get; set; } = true;

    /// <summary>
    /// 循環播放
    /// </summary>
    public bool IsCyclePlay { get; set; } = false;

    /// <summary>
    /// 自動撥放間隔
    /// </summary>
    public int AutoPlaySec { get; set; } = 3;

    /// <summary>
    /// 存檔
    /// </summary>
    /// <param name="isAutoPlay">是否自動撥放</param>
    /// <param name="isCyclePlay">是否循環撥放</param>
    /// <param name="autoPlaySec">自動撥放間隔秒數</param>
    public static void Save(bool isAutoPlay, bool isCyclePlay, int autoPlaySec)
    {
        Settings settings = new()
        {
            IsAutoPlay = isAutoPlay,
            AutoPlaySec = autoPlaySec,
            IsCyclePlay = isCyclePlay
        };

        string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
        File.WriteAllText("appsettings.json", json);
    }
}

