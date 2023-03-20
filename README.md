# AICommand

![out](https://user-images.githubusercontent.com/343936/226172223-acfba006-8621-425f-a697-be745a94503f.gif)

This is a proof-of-concept integration of ChatGPT into Unity Editor. You can
control the Editor using natural language prompts.

## How to try it

You have to generate an API key to use the ChatGPT API. Please generate it on
your [account page](https://platform.openai.com/account/api-keys) and set it on
the Project Settings page (Edit > Project Settings > AI Command > API Key).

**CAUTION** - The API key is stored in `UserSettings/AICommandSettings.asset`.
You must exclude the directory when sharing your project with others.

You can open the AI Command window from Window > AI Command.

## FAQ

### Is it practical?

**No.** I created this proof-of-concept and proved that it doesn't work yet. It
works nicely in some cases and fails very poorly in others. I got several ideas
from those successes and failures, which is this project's main aim.

### Can I install this to my project?

This is just a proof-of-concept project, so there is no standard way to install
it in other projects. If you want to try it with your project anyway, you can
simply copy the `Assets/Editor` directory to your project.

### AICommandWindow throws NullReferenceException

Probably your OpenAI trial period has expired. Please check your billing status.
See [AIShader issue #1] for details.

[AIShader Issue #1]: https://github.com/keijiro/AIShader/issues/1

### I pressed the "Run" button, resulting in compilation errors.

In many cases, ChatGPT fails to implement your command correctly. You might get
the correct one by repeatedly pressing the "Run" button.

(That's why I don't think this PoC is practical...)
