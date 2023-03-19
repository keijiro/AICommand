using UnityEngine;
using UnityEditor;

namespace AICommand {

[FilePath("UserSettings/AICommandSettings.asset",
          FilePathAttribute.Location.ProjectFolder)]
public sealed class AICommandSettings : ScriptableSingleton<AICommandSettings>
{
    public string apiKey = null;
    public int timeout = 0;
    public void Save() => Save(true);
    void OnDisable() => Save();
}

sealed class AICommandSettingsProvider : SettingsProvider
{
    public AICommandSettingsProvider()
      : base("Project/AI Command", SettingsScope.Project) {}

    public override void OnGUI(string search)
    {
        var settings = AICommandSettings.instance;

        var key = settings.apiKey;
        var timeout = settings.timeout;

        EditorGUI.BeginChangeCheck();

        key = EditorGUILayout.TextField("API Key", key);
        timeout = EditorGUILayout.IntField("Timeout", timeout);

        if (EditorGUI.EndChangeCheck())
        {
            settings.apiKey = key;
            settings.timeout = timeout;
            settings.Save();
        }
    }

    [SettingsProvider]
    public static SettingsProvider CreateCustomSettingsProvider()
      => new AICommandSettingsProvider();
}

} // namespace AICommand
