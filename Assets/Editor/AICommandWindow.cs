using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace AICommand {

public sealed class AICommandWindow : EditorWindow
{
    #region Temporary script file operations

    const string TempFilePath = "Assets/AICommandTemp.cs";

    bool TempFileExists => System.IO.File.Exists(TempFilePath);

    void CreateScriptAsset(string code)
    {
        // UnityEditor internal method: ProjectWindowUtil.CreateScriptAssetWithContent
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
        method.Invoke(null, new object[]{TempFilePath, code});
    }

    #endregion

    #region Script generator

    static string WrapPrompt(string input)
      => "Write a Unity Editor script.\n" +
         " - It provides its functionality as a menu item placed \"Edit\" > \"Do Task\".\n" +
         " - It doesn’t provide any editor window. It immediately does the task when the menu item is invoked.\n" +
         " - Don’t use GameObject.FindGameObjectsWithTag.\n" +
         " - There is no selected object. Find game objects manually.\n" +
         " - I only need the script body. Don’t add any explanation.\n" +
         "The task is described as follows:\n" + input;

    void RunGenerator()
    {
        var code = OpenAIUtil.InvokeChat(WrapPrompt(_prompt));
        Debug.Log("AI command script:" + code);
        CreateScriptAsset(code);
    }

    #endregion

    #region Editor GUI

    string _prompt = "Create 100 cubes at random points.";

    const string ApiKeyErrorText =
      "API Key hasn't been set. Please check the project settings " +
      "(Edit > Project Settings > AI Command > API Key).";

    bool IsApiKeyOk
      => !string.IsNullOrEmpty(AICommandSettings.instance.apiKey);

    [MenuItem("Window/AI Command")]
    static void Init() => GetWindow<AICommandWindow>(true, "AI Command");

    void OnGUI()
    {
        if (IsApiKeyOk)
        {
            _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.ExpandHeight(true));
            if (GUILayout.Button("Run")) RunGenerator();
        }
        else
        {
            EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
        }
    }

    #endregion

    #region Script lifecycle

    void OnEnable()
      => AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

    void OnDisable()
      => AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

    void OnAfterAssemblyReload()
    {
        if (!TempFileExists) return;
        EditorApplication.ExecuteMenuItem("Edit/Do Task");
        AssetDatabase.DeleteAsset(TempFilePath);
    }

    #endregion
}

} // namespace AICommand
