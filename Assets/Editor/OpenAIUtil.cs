using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace AICommand
{
    public enum ModelType
    {
        gpt_4,
        gpt_3_5_turbo
    };

    //Dictionary for the ModelType to the string used by the API
    static class ModelTypeDict
    {
        public static string GetModelTypeString(ModelType modelType)
        {
            switch (modelType)
            {
                case ModelType.gpt_4:
                    return "gpt-4";
                default:
                    return "gpt-3.5-turbo";
            }
        }
    }

    static class OpenAIUtil
    {

        static string CreateChatRequestBody(string prompt, ModelType modelType)
        {
            var msg = new OpenAI.RequestMessage();
            msg.role = "user";
            msg.content = prompt;

            var req = new OpenAI.Request();
            req.model = ModelTypeDict.GetModelTypeString(modelType);
            req.messages = new[] { msg };

            return JsonUtility.ToJson(req);
        }

        public static string InvokeChat(string prompt, ModelType _modelType)
        {
            var settings = AICommandSettings.instance;

            // POST
            using var post = UnityWebRequest.Post
                (OpenAI.Api.Url, CreateChatRequestBody(prompt, _modelType), "application/json");

            // Request timeout setting
            post.timeout = settings.timeout;

            // API key authorization
            post.SetRequestHeader("Authorization", "Bearer " + settings.apiKey);

            // Request start
            var req = post.SendWebRequest();

            // Progress bar (Totally fake! Don't try this at home.)
            for (var progress = 0.0f; !req.isDone; progress += 0.01f)
            {
                EditorUtility.DisplayProgressBar
                    ("AI Command", "Generating...", progress);
                System.Threading.Thread.Sleep(100);
                progress += 0.01f;
            }

            EditorUtility.ClearProgressBar();

            // Response extraction
            var json = post.downloadHandler.text;
            var data = JsonUtility.FromJson<OpenAI.Response>(json);
            return data.choices[0].message.content;
        }
    }
}
// namespace AICommand
