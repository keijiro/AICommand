using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace AICommand {

public sealed class AICommandWindow : EditorWindow
{
    [MenuItem("Window/AI Command")]
    static void Init() => GetWindow<AICommandWindow>();

    const string TempFilePath = "Assets/Test.cs";

    bool TempFileExists
      => System.IO.File.Exists(TempFilePath);

    string _prompt = "Create 100 cubes at random points.";

    static string WrapPrompt(string input)
      => "Write a Unity Editor script.\n" +
         " - It provides its functionality as a menu item placed \"Edit\" > \"Do Task\".\n" +
         " - It doesn’t provide any editor window. It immediately does the task when the menu item is invoked.\n" +
         " - I only need the script body. Don’t add any explanation.\n" +
         "The task is described the following:\n" + input;

    void CreateScriptAssetWithContent(string code)
    {
        Debug.Log(code);
        var flags = BindingFlags.Static | BindingFlags.NonPublic;
        var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
        method.Invoke(null, new object[]{TempFilePath, code});
    }

    void OnGUI()
    {
        _prompt = EditorGUILayout.TextArea(_prompt);
        if (GUILayout.Button("Generate")) RunGenerator();
    }

    void RunGenerator()
      => CreateScriptAssetWithContent(OpenAIUtil.InvokeChat(WrapPrompt(_prompt)));

    void OnEnable()
      => AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

    void OnDisable()
      => AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

    public void OnAfterAssemblyReload()
    {
        if (!TempFileExists) return;
        EditorApplication.ExecuteMenuItem("Edit/Do Task");
        AssetDatabase.DeleteAsset(TempFilePath);
    }
}

} // namespace AICommand
