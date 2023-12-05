using UnityEngine;
using UnityEditor;

namespace AICommand
{
    // Custom attribute to specify the file path for the settings asset
    [FilePath("UserSettings/AICommandSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class AICommandSettings : ScriptableSingleton<AICommandSettings>
    {
        // Public settings variables
        public string apiKey = null;
        public int timeout = 0;

        // Save settings method
        public void Save() => Save(true);

        // Ensure settings are saved when the scriptable object is disabled
        void OnDisable() => Save();
    }

    // Custom settings provider for the AICommandSettings
    sealed class AICommandSettingsProvider : SettingsProvider
    {
        // Constructor
        public AICommandSettingsProvider() : base("Project/AI Command", SettingsScope.Project) { }

        // GUI for displaying and editing settings in the Unity Editor
        public override void OnGUI(string search)
        {
            // Get the instance of AICommandSettings
            var settings = AICommandSettings.instance;

            // Extract settings values
            var key = settings.apiKey;
            var timeout = settings.timeout;

            // Begin checking for changes in the GUI
            EditorGUI.BeginChangeCheck();

            // Display and edit API Key field
            key = EditorGUILayout.TextField("API Key", key);

            // Display and edit Timeout field
            timeout = EditorGUILayout.IntField("Timeout", timeout);

            // If there are changes, update the settings and save
            if (EditorGUI.EndChangeCheck())
            {
                settings.apiKey = key;
                settings.timeout = timeout;
                settings.Save();
            }
        }

        // Custom method to create the settings provider
        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider() => new AICommandSettingsProvider();
    }
}
