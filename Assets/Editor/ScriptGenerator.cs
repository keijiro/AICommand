using UnityEngine;
using UnityEditor;
using System.Reflection;

public sealed class ScriptGeneratorWindow : EditorWindow
{
    [MenuItem("Window/Script Generator")]
    static void Init() => GetWindow<ScriptGeneratorWindow>();

    void OnGUI()
    {
        if (GUILayout.Button("Generate")) RunGenerator();
    }

    void RunGenerator()
    {
        var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(null, new object [] { "Assets/Test.cs", "using UnityEngine;" });
    }

    void OnEnable()
    {
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
    }

    void OnDisable()
    {
        AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
    }

    public void OnAfterAssemblyReload()
    {
        Debug.Log("After Assembly Reload");
    }
}
