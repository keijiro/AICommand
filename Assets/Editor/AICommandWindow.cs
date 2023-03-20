using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;

namespace AICommand
{

    public sealed class AICommandWindow : EditorWindow
    {
        #region Temporary script file operations
        //generate random file name

        // const string AssetFilePath = "Assets/AI_.cs";
        const string AssetFilePath = "Assets/Meus AI Assets/";
        const string RuntimeFilePath = "Assets/Meus AI Assets/ai_runtime_spawner.cs";
        const string ContextFilePath = "Assets/Meus AI Assets/context.txt";
        bool executeAfterGenerate = false;



        bool TempFileExists => System.IO.File.Exists(AssetFilePath);
        bool RuntimeFileExists => System.IO.File.Exists(RuntimeFilePath);
        bool ContextFileExists => System.IO.File.Exists(ContextFilePath);

        string time = "";

        void CreateScriptAsset(string code, string name, string type)
        {
            // UnityEditor internal method: ProjectWindowUtil.CreateScriptAssetWithContent
            var flags = BindingFlags.Static | BindingFlags.NonPublic;
            var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
            method.Invoke(null, new object[] { AssetFilePath + name + ".cs", code });
            // method.Invoke(null, new object[] { RuntimeFilePath, "" });
            if (!ContextFileExists)
            {
                method.Invoke(null, new object[] { ContextFilePath, "" });
                Debug.Log("Context file created");
            }
        }

        #endregion

        #region Script generator


        static string WrapPrompt(string input, string name, bool useContext)
        {
            string previousContext = System.IO.File.ReadAllText(ContextFilePath);
            return
                "- " + previousContext + "\n\n" + "- the class name must be named '" + name + "';\n" +

                "The task is described the following:\n" + input;
        }

        string FormatName(string name)
        {
            Debug.Log("filepath: " + RuntimeFilePath);
            string defaultName = "Sem_Nome_" + time;
            //remove spacial characters
            name = name.ToLower();
            name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            name = System.Text.RegularExpressions.Regex.Replace(name, "[^a-zA-Z0-9_.]+", "", System.Text.RegularExpressions.RegexOptions.Compiled);
            name = name.Replace(":", "");
            name = name.Replace(".", "");
            name = name.Replace(" ", "");
            if (name.Length == 0) name = defaultName;
            if (name.Length > 50) name = name.Substring(0, 50);
            if (name == "DigiteONomeDoAssetExEmpilharBlocos") name = "EmpilharBlocos";
            return name;
        }
        void GenerateScript()
        {
            time = "" + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            var name = FormatName(_assetName);
            var code = OpenAIUtil.InvokeChat(WrapPrompt(_prompt, name, true));
            StreamWriter writer = new StreamWriter(ContextFilePath, true);
            // writer.WriteLine(WrapPrompt(_prompt, name, true) + "\n" + code + "\n");
            writer.WriteLine(_prompt + "\n" + code + "\n");
            writer.Close();
            CreateScriptAsset(code, name, "script");
        }

        #endregion

        #region Editor GUI

        string _assetName = "Digite o nome do asset ex: Empilhar Blocos.";
        string _prompt = "Crie uma pilha de blocos coloridos";

        const string ApiKeyErrorText =
          "API Key hasn't been set. Please check the project settings " +
          "(AI Assets > Project Settings > AI Command > API Key).";

        bool IsApiKeyOk
      => !string.IsNullOrEmpty(AICommandSettings.instance.apiKey);

        [MenuItem("AITools/Gerador de Assets")]
        static void Init() => GetWindow<AICommandWindow>(true, "Gerador de Assets");

        void OnGUI()
        {
            if (IsApiKeyOk)
            {
                _assetName = EditorGUILayout.TextArea(_assetName, GUILayout.ExpandHeight(false));
                _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.ExpandHeight(true));
                if (GUILayout.Button("Gerar Script")) { GenerateScript(); }
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
            if (!RuntimeFileExists) return;
            if (executeAfterGenerate) EditorApplication.ExecuteMenuItem("AITools/AI Assets/" + FormatName(_assetName));
            AssetDatabase.DeleteAsset(RuntimeFilePath);
            // EditorApplication.ExecuteMenuItem("AI Assets/Do Task");
        }

        #endregion
    }

} // namespace AICommand
