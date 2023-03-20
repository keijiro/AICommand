using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace AICommand
{
    static class OpenAIUtil
    {
        static string CreateChatRequestBody(string prompt)
        {
            var msg = new OpenAI.RequestMessage();
            msg.role = "user";
            msg.content = prompt;

            var req = new OpenAI.Request();
            req.model = "gpt-3.5-turbo";
            req.messages = new [] { msg };

            return JsonUtility.ToJson(req);
        }

        private static UnityWebRequest CreateApiRequest(string url, object body)
        {

#if UNITY_2022_2_OR_NEWER

         var request = UnityWebRequest.Post(OpenAI.Api.Url, CreateChatRequestBody(prompt), "application/json");
         return request;
#else

            string bodyString = null;
            if (body is string)
            {
                bodyString = (string)body;
            }
            else if (body != null)
            {
                bodyString = JsonUtility.ToJson(body);
            }

            var request = new UnityWebRequest();
            request.url = url;
            request.method = "POST";
            request.downloadHandler = new DownloadHandlerBuffer();
            request.uploadHandler = new UploadHandlerRaw(string.IsNullOrEmpty(bodyString) ? null : Encoding.UTF8.GetBytes(bodyString));
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 60;
            return request;
#endif
        }

        public static string InvokeChat(string prompt)
        {
            var settings = AICommandSettings.instance;

            // POST
            using var post = CreateApiRequest(OpenAI.Api.Url, CreateChatRequestBody(prompt));

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
} // namespace AICommand
