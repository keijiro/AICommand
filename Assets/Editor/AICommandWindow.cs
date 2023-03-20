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
      => "Write a Unity Editor script with the following specifications:\n" +
         " - The script should provide its functionality as a menu item placed in the \"Edit\" menu, with the label \"Do Task\".\n" +
         " - The script should not create or utilize any editor windows. Instead, the designated task should be executed immediately when the menu item is invoked.\n" +
         " - Do not use GameObject.FindGameObjectsWithTag in the script.\n" +
         " - The script should not rely on a currently selected object. Instead, find the relevant game objects manually within the script.\n" +
         " - Provide only the body of the script without any additional explanations or comments.\n" +
         "The task for the script to perform is described as follows:\n" + input;

    void RunGenerator()
    {
        var code = OpenAIUtil.InvokeChat(WrapPrompt(_prompt));
        code = PostProcessGeneratedCode(code); // Add this line
        Debug.Log("AI command script:" + code);
        CreateScriptAsset(code);
    }

    #endregion

    private string PostProcessGeneratedCode(string code)
    {
        // Remove unexpected characters
        code = code.Replace("`", "");

        // Make sure the script starts with 'using' statements or a namespace declaration
        if (!code.TrimStart().StartsWith("using") && !code.TrimStart().StartsWith("namespace"))
        {
            code = "using UnityEngine;\nusing UnityEditor;\n\n" + code;
        }

        return code;
    }

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

    void OnDestroy()
    {
      AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
    }

    void OnAfterAssemblyReload()
    {
        if (!TempFileExists) return;
        EditorApplication.ExecuteMenuItem("Edit/Do Task");
        AssetDatabase.DeleteAsset(TempFilePath);
    }

    #endregion
}

} // namespace AICommand
