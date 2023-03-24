using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AICommand {

public sealed class AIGenerateScriptWindow : EditorWindow
{
    #region Temporary script file operations

    const string TempFilePath = "Assets/Scripts/";

    bool TempFileExists => System.IO.File.Exists(TempFilePath);

    void CreateScriptAsset(string code, string className)
    {
        // UnityEditor internal method: ProjectWindowUtil.CreateScriptAssetWithContent
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
        method.Invoke(null, new object[]{TempFilePath + className + ".cs", code});
        
    }

    #endregion

    #region Script generator

    static string WrapPrompt(string input)
      => "Write a full Unity Engine script.\n" +
         " - Include using UnityEngine;\n" +
         " - Do not use GameObject.FindGameObjectsWithTag.\n" +
         " - There is no selected object. Find game objects manually.\n" +
         " - I only need the script body. Don’t add any explanation.\n" +
         "The task is described as follows:\n" + input;

    void RunGenerator()
    {
        var code = OpenAIUtil.InvokeChat(WrapPrompt(_prompt));
        Debug.Log("AI command script:" + code);
        
        string pattern = @"class\s+(\w+)\s*:";
        string className = "";
        Match match = Regex.Match(code, pattern);
        if (match.Success) {
            className = match.Groups[1].Value;
            Debug.Log(className); // Output: AIGenerateScriptWindow
        }
        
        CreateScriptAsset(code, className);
    }

    #endregion

    #region Editor GUI

    string _prompt = "Create a script that enables a player to move a gameobject with physics based movement \n" +
                     "The player should be able to walk, run and jump \n" +
                     "The player should be have a camera gameobject following them that moves smoothly and can be rotated with the mouse \n" +
                     "The camera should be able to orbit the player and the player should move in the direction the camera is facing";

    const string ApiKeyErrorText =
      "API Key hasn't been set. Please check the project settings " +
      "(Edit > Project Settings > AI Command > API Key).";

    bool IsApiKeyOk
      => !string.IsNullOrEmpty(AICommandSettings.instance.apiKey);

    [MenuItem("Window/AI Command/Generate Script")]
    static void Init() => GetWindow<AIGenerateScriptWindow>(true, "AI Generate Script");

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
        //EditorApplication.ExecuteMenuItem("Edit/Do Task");
        //AssetDatabase.DeleteAsset(TempFilePath);
    }

    #endregion
}

} // namespace AICommand
