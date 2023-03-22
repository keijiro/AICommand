using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;

namespace AICommand
{
    public sealed class AICommandWindowScripts : EditorWindow
    {
        #region Script file operations
        const string basePath = "Assets/Meus AI Assets/";
        const string ContextFilePath = "Assets/Meus AI Assets/context.txt";
        bool ContextFileExists => System.IO.File.Exists(ContextFilePath);
        string time = "";
        string objectName = "";

        async Task<string> CreateScriptAsset(string code, string name)
        {
            try
            {
                var flags = BindingFlags.Static | BindingFlags.NonPublic;
                var method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
                // Wait for one second
                method.Invoke(null, new object[] { basePath + name + ".cs", code });
                await Task.Delay(100);
            }
            catch (Exception e)
            {
                Debug.Log("Error: " + e);
            }

            return "ok";

        }

        #endregion

        #region Script generator

        static string WrapPrompt(string input, string name, bool useContext)
        {
            string previousContext = "";
            if (File.Exists(basePath + name + ".cs")) previousContext = System.IO.File.ReadAllText(basePath + name + ".cs");

            return
            "Write a Unity Engine script with the following rules: \n" +
            "- I only need the script body. Don’t add any explanation;  \n" +
            "- The first line must be using UnityEngine; \n" +
            "- The second line must be using System.Collections; \n" +
            "- The third line must be using System.Collections.Generic; \n" +
            "- Keep the previous script with same class name, just adjust it with the new prompts; \n" +
            "- Do not add anything to de prompt, just on the code; \n" +
            "- the class name must be named '" + name + "';\n\n" +
            "- use the following context: \n" +
            "- when you add new lines of code, put a comment at same line saying that is new" +
            previousContext + "\n\n" +
            "The task is described the following:\n" + input;
        }

        string FormatName(string name)
        {
            // Debug.Log("filepath: " + RuntimeFilePath);
            string defaultName = "Sem_Nome_" + time;
            name = name.ToLower();
            name = System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name.ToLower());
            name = System.Text.RegularExpressions.Regex.Replace(name, "[^a-zA-Z0-9_.]+", "", System.Text.RegularExpressions.RegexOptions.Compiled);
            name = name.Replace(":", "");
            name = name.Replace(".", "");
            name = name.Replace(" ", "");
            name = name.Replace("ex: ", "");
            if (name.Length == 0) name = defaultName;
            if (name.Length > 50) name = name.Substring(0, 50);
            if (name == "DigiteONomeDoAssetExEmpilharBlocos") name = "EmpilharBlocos";
            return name;
        }
        async void GenerateScript()
        {
            time = "" + System.DateTime.Now.ToString("yyyyMMddHHmmss");
            var name = FormatName(_assetName);
            var code = OpenAIUtil.InvokeChat(WrapPrompt(_prompt, name, true));
            string status = await CreateScriptAsset(code, name);


            //to automatically add the script to the object
            // var go = GameObject.Find(objectName);

            // if (go != null)
            // {
            //     System.Type CustomType = System.Type.GetType(name + ",Assembly-CSharp");
            //     GameObject source_gameObject = source as GameObject;
            //     source_gameObject.AddComponent(CustomType);
            // }

        }

        #endregion
        #region Editor GUI

        string _assetName = "ex: Player Movement";
        string _prompt = "Control player using arrowkeys to move and space to jump.";

        const string ApiKeyErrorText =
          "API Key hasn't been set. Please check the project settings " +
          "(AI Assets > Project Settings > AI Command > API Key).";

        bool IsApiKeyOk
      => !string.IsNullOrEmpty(AICommandSettings.instance.apiKey);


        [MenuItem("AITools/Script Generator")]
        // static void Init() => GetWindow<AICommandWindowScripts>(true, "Script Generator");
        static void Init()
        {
            var window = GetWindowWithRect<AICommandWindowScripts>(new Rect(0, 0, 165, 100));
            window.titleContent = new GUIContent("Script Generator");
        }

        public UnityEngine.Object source;
        private bool showComponents = false;
        private List<Component> componentsList = new List<Component>();
        public delegate void ComponentSelectedHandler(Component selectedComponent);
        public event ComponentSelectedHandler OnComponentSelected;
        private Component _selectedComponent;

        void OnGUI()
        {
            if (IsApiKeyOk)
            {
                source = EditorGUILayout.ObjectField("Source", source, typeof(GameObject), true);

                if (source != null)
                {
                    objectName = source.name;
                }
                else
                {
                    objectName = "";
                }

                if (source != null)
                {
                    // Component[] components = FormatComponents(((GameObject)source).GetComponents<Component>());

                    Component[] components = ((GameObject)source).GetComponents<Component>();

                    List<Component> filteredComponents = FilterComponents(components, componentNamesToExclude);

                    //if the filteredComponents is empty, then the source object has no components that we want to show
                    if (filteredComponents.Count == 0)
                    {
                        EditorGUILayout.HelpBox("The selected object source as no custom scripts yet, you can create one below", MessageType.Info);
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Components:");
                    }
                    foreach (Component component in filteredComponents)
                    {
                        bool isSelected = _selectedComponent == component;
                        bool newIsSelected = EditorGUILayout.Toggle(component.GetType().ToString(), isSelected);

                        if (newIsSelected != isSelected)
                        {
                            _selectedComponent = newIsSelected ? component : null;
                            OnComponentSelected?.Invoke(_selectedComponent);
                        }
                    }
                }

                _assetName = EditorGUILayout.TextField("Script Name", _assetName);
                _prompt = EditorGUILayout.TextArea(_prompt, GUILayout.ExpandHeight(true));

                if (GUILayout.Button("Generate Script")) GenerateScript();
            }
            else
            {
                EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
            }
        }

        private void OnEnable()
        {
            OnComponentSelected += GetScriptName;
        }

        private void OnDisable()
        {
            OnComponentSelected -= GetScriptName;
        }

        private void GetScriptName(Component selectedComponent)
        {
            // Execute your custom code here
            if (selectedComponent == null)
            {
                // Debug.Log("Selected component: null");
                _assetName = "ex: Player Movement.";
                return;
            }
            // Debug.Log("Selected component: " + selectedComponent.GetType().ToString());
            _assetName = selectedComponent.GetType().ToString().Replace("UnityEngine.", "");

        }

        public List<Component> FilterComponents(Component[] components, string[] excludedTypes)
        {
            List<Component> filteredComponents = new List<Component>();

            foreach (Component component in components)
            {
                bool shouldExclude = false;

                foreach (string excludedType in excludedTypes)
                {
                    if (component.GetType().ToString().Contains("UnityEngine." + excludedType))
                    {
                        shouldExclude = true;
                        break;
                    }
                }

                if (!shouldExclude)
                {
                    filteredComponents.Add(component);
                }
            }

            return filteredComponents;
        }

        string[] componentNamesToExclude = {
            "Transform",
            "Camera",
            "GUILayer",
            "Light",
            "Animation",
            "ParticleSystem",
            "Rigidbody",
            "Collider",
            "BoxCollider",
            "SphereCollider",
            "CapsuleCollider",
            "MeshCollider",
            "WheelCollider",
            "CharacterController",
            "AudioListener",
            "AudioSource",
            "AudioReverbZone",
            "AudioLowPassFilter",
            "AudioHighPassFilter",
            "AudioEchoFilter",
            "AudioDistortionFilter",
            "AudioReverbFilter",
            "AnimationFilter",
            "ConstantForce",
            "AreaEffector2D",
            "BuoyancyEffector2D",
            "PointEffector2D",
            "PlatformEffector2D",
            "SurfaceEffector2D",
            "RelativeJoint2D",
            "FixedJoint2D",
            "FrictionJoint2D",
            "HingeJoint2D",
            "SliderJoint2D",
            "SpringJoint2D",
            "WheelJoint2D",
            "PhysicsMaterial2D",
            "BuoyancyEffector",
            "PointEffector",
            "AreaEffector",
            "GravityEffector",
            "SurfaceEffector",
            "Joint",
            "FixedJoint",
            "HingeJoint",
            "SpringJoint",
            "CharacterJoint",
            "ConfigurableJoint",
            "RelativeJoint",
            "GenericJoint",
            "ConstantForce",
            "Character",
            "NetworkView",
            "GUIText",
            "GUITexture",
            "TextMesh",
            "Renderer",
            "MeshRenderer",
            "MeshFilter",
            "SkinnedMeshRenderer",
            "LineRenderer",
            "TrailRenderer",
            "ParticleSystemRenderer",
            "SpriteRenderer",
            "LightProbeGroup",
            "ReflectionProbe",
            "ReflectionProbeGroup",
            "OcclusionArea",
            "OcclusionPortal",
            "Terrain",
            "TerrainCollider",
            "NavMeshAgent",
            "NavMeshObstacle",
            "OffMeshLink",
            "Projector",
            "Skybox",
            "FlareLayer",
            "Canvas",
            "CanvasRenderer",
            "RectTransform",
            "CanvasGroup",
            "LayoutElement",
            "ContentSizeFitter",
            "AspectRatioFitter",
            "Mask",
            "RectMask2D",
            "PhysicsUpdateBehaviour2D",
            "ConstantForce2D",
            "Effector2D",
            "CompositeCollider2D",
            "CircleCollider2D",
            "BoxCollider2D",
            "EdgeCollider2D",
            "PolygonCollider2D",
            "TilemapCollider2D",
            "TilemapRenderer",
            "SpriteMask",
            "Grid",
            "Tilemap",
            "TilemapCollider3D"
        };

        #endregion

    }

} // namespace AICommand

