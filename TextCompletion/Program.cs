using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System;
using System.ClientModel;

// get credentials from user secrets
IConfigurationRoot config = new  ConfigurationBuilder().AddUserSecrets<Program>().Build();

var credential = new ApiKeyCredential(config["GitHubModels:Token"] ?? throw new InvalidOperationException());
var options = new OpenAIClientOptions()
{
    Endpoint = new Uri("https://models.github.ai/inference")
};

//create a chat client
IChatClient client = new OpenAIClient(credential, options).GetChatClient("openai/gpt-4o-mini").AsIChatClient();

#region Basic Completion

//send prompt and get response
string prompt = "What is AI? explain max 20 words";
Console.WriteLine($"user >>> {prompt}");

ChatResponse response = await client.GetResponseAsync(prompt);

Console.WriteLine(response);
Console.WriteLine($"Tokens used: in={response.Usage?.InputTokenCount}, out={response.Usage?.OutputTokenCount}");

#endregion

#region Streamin

string streaminPrompt = "What is AI? explain max 200 words";
Console.WriteLine($"user >>> {streaminPrompt}");

var responseStream = client.GetStreamingResponseAsync(streaminPrompt);
await foreach (var message in responseStream)
{
    Console.Write(message.Text);
}

#endregion

#region Classification

var classificationPromp = """
    Please classify the following sentenses into categories:
    - 'complaint'
    - 'suggestion'
    - 'praise'
    - 'other'.

    1) "The interface looks fantastic"
    2) "It would be nice to have a dark mode."
    3) "Notifications are not working properly."
    4) "I really enjoy working with AI"
    """;

Console.WriteLine($"user >>> {classificationPromp}");

ChatResponse classificationResponse = await client.GetResponseAsync(classificationPromp);

Console.WriteLine(classificationResponse);

#endregion

#region Summarization

var summaryPromp = """
    Summarize the following blog in 1 concise sentences:

    "Microservice architecture is a modern software design approach that builds applications as a collection of small, independent, and loosely coupled services. Each service is 
    responsible for a specific business function and communicates with others through well-defined APIs. This structure allows teams to develop, deploy, and scale services 
    independently. It also enables the use of different technologies and programming languages within the same application. Compared to traditional monolithic architectures where 
    all components are tightly bundled together, microservices provide greater flexibility, scalability, and resilience."
    """;

Console.WriteLine($"user >>> {summaryPromp}");

ChatResponse summaryResponse = await client.GetResponseAsync(summaryPromp);

Console.WriteLine(summaryResponse);

#endregion

#region Sentiment Analysus

var analysisPromp = """
    You will analyze the sentiment of the following production reviews. 
    Each line is its own review. Output the sentiment of each review in a bulleted list and then provide a generate sentiment of of all reviews.
    
    The battery life is amazing and lasts all day.
    The product works fine, but the design feels outdated.
    It stopped working after a week, very disappointing.
    It stopped working after a week, very disappointing.
    The price is too high for the features offered.
    """;

Console.WriteLine($"user >>> {analysisPromp}");

ChatResponse responseAnalysis = await client.GetResponseAsync(analysisPromp);

Console.WriteLine(responseAnalysis);

#endregion