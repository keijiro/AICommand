using UnityEngine;
using UnityEditor;

namespace AICommand {

[FilePath("UserSettings/AICommandSettings.asset",
          FilePathAttribute.Location.ProjectFolder)]
public sealed class AICommandSettings : ScriptableSingleton<AICommandSettings>
{
    public string apiKey = null;
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
        EditorGUI.BeginChangeCheck();
        key = EditorGUILayout.TextField("API Key", key);
        if (EditorGUI.EndChangeCheck())
        {
            settings.apiKey = key;
            settings.Save();
        }
    }

    [SettingsProvider]
    public static SettingsProvider CreateCustomSettingsProvider()
      => new AICommandSettingsProvider();
}

} // namespace AICommand
