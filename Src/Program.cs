using HelloWorldCopilot.Plugins;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

var collection = new ServiceCollection();

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var deploymentName = configuration["GitHub:DeploymentName"];
var apiKey = configuration["GitHub:Token"];
var endpoint = configuration["GitHub:Endpoint"];

collection.AddLogging(services => services.AddConsole()
    .AddFilter("Microsoft.SemanticKernel.Connectors", LogLevel.Debug)
    .SetMinimumLevel(LogLevel.Warning));

collection.AddSingleton<IConfiguration>(configuration);
collection.AddHttpClient();
collection.AddTransient<CommonPlugins>();

collection.AddKernel()
    .AddAzureOpenAIChatCompletion(deploymentName!, endpoint!, apiKey!);

collection.AddTransient(serviceProvider =>
{
    var plugins = new KernelPluginCollection();
    plugins.AddFromObject(serviceProvider.GetRequiredService<CommonPlugins>());
    return plugins;
});

var serviceProvider = collection.BuildServiceProvider();
var chatCompletionService = serviceProvider.GetRequiredService<IChatCompletionService>();

var kernel = serviceProvider.GetRequiredService<Kernel>();
var executionSettings = new PromptExecutionSettings
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var copilotMessage = "Hello, I am your personal Copilot. How can I help you today? Type /bye to exit.";
var chatHistory = new ChatHistory();
chatHistory.AddSystemMessage("You are an adaptive personal copilot that acts as a " +
    "knowledgeable developer coach, productivity guru, fitness motivator, and creative thinker. " +
    "Provide concise, actionable, and user-focused responses.");

chatHistory.AddAssistantMessage(copilotMessage);
Console.WriteLine($"Copilot > {copilotMessage}");
Console.WriteLine();
Console.Write("You > ");
var input = Console.ReadLine();
while (!string.IsNullOrEmpty(input) &&
    !input.Trim().Equals("/bye", StringComparison.OrdinalIgnoreCase))
{
    chatHistory.AddUserMessage(input);
    var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);
    chatHistory.AddAssistantMessage(response.Content!);
    Console.WriteLine($"Copilot > {response.Content!}");
    Console.WriteLine();
    Console.Write("You > ");
    input = Console.ReadLine();
}