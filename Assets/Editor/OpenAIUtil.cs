using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

namespace AICommand {

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

    public static string InvokeChat(string prompt)
    {
        var settings = AICommandSettings.instance;

        // POST
        var jsonBody = CreateChatRequestBody(prompt);
        #if UNITY_2022_1_OR_NEWER
		using var post = UnityWebRequest.Post
          (OpenAI.Api.Url, jsonBody, "application/json");
        #else
        // Make a Web Request, the long way
        // The simpler .Post() does not seem to work, results in OpenAI API failure to recognize JSON
		//using var post = UnityWebRequest.Post(OpenAI.Api.Url, jsonBody);
		byte[] jsonRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
		using var post = new UnityWebRequest
            (OpenAI.Api.Url, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(jsonRaw));
		// Set Content-Type to json
        post.SetRequestHeader("Content-Type", "application/json");
        #endif

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
