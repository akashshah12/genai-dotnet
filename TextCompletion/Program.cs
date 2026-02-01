using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using System;
using System.ClientModel;
using System.Text.Json.Serialization;

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

#region Sentiment Analysis

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

#region StructuredExtraction

string[] vehicleAds =
{
    "Explore this well-kept 2019 Toyota Camry featuring 40,000 miles, clean title, efficient fuel usage, roomy trunk, and safety tech like lane assist. Asking price starts at $18,000. Call Metro Auto at (555) 111-2222.",
    "Available for lease: 2021 Honda Civic with just 10,000 miles. Includes sunroof, upgraded audio, and rear camera. Ideal for city driving. Monthly lease from $250. Contact Uptown Motors at (555) 333-4444.",
    "For collectors: a 1968 Ford Mustang with V8 engine and manual transmission. Runs well but interior needs work. Approximately 80,000 miles. Priced at $25,000. Reach Retro Wheels at (555) 777-8888.",
    "Lease a brand-new 2023 Tesla Model 3 with zero mileage. Fully electric, autopilot enabled, modern minimalist design. Lease pricing begins at $450. Call EVolution Cars at (555) 999-0000.",
    "Offered for sale: 2015 Subaru Outback with 60,000 miles. AWD, heated seats, and generous cargo space. Starting at $14,000. Call Forrest Autos at (555) 222-1212."
};

foreach (string ad in vehicleAds)
{
var aiResponse = await client.GetResponseAsync<VehicleInfo>(
    $"""
        Extract structured vehicle data from the text below.
        The output must follow this C# model exactly:

        Condition: "New" or "Used"
        Make: manufacturer name
        Model: vehicle model
        Year: four digit year
        ListingType: "Sale" or "Lease"
        Price: whole number only
        Features: list of short feature descriptions
        TenWordSummary: summary with exactly ten words

        Listing text:
        {ad}
        """);

if (aiResponse.TryGetResult(out var vehicle))
{
var json = System.Text.Json.JsonSerializer.Serialize(
    vehicle,
    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

Console.WriteLine(json);
}
else
{
Console.WriteLine("Failed to parse vehicle listing.");
}
}

class VehicleInfo
{
    public required string Condition { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public ListingMode ListingType { get; set; }
    public int Price { get; set; }
    public required string[] Features { get; set; }
    public required string TenWordSummary { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
enum ListingMode
{
    Sale,
    Lease
}

#endregion

#region HikingChatExperience

// Initialize conversation with instructions for the 
/*List<ChatMessage> conversationLog = new()
{
    new ChatMessage(ChatRole.System, """
        You are an outgoing outdoor lover who enjoys guiding people toward great hiking experiences.
        When the conversation begins, briefly introduce yourself.

        Before giving trail recommendations, always ask the user:
        - Where they want to go hiking
        - How challenging they want the hike to be

        Once you have this information, suggest three nearby hiking trails
        with different distances. Include one interesting nature-related fact
        about the area in your recommendations. End every response by asking
        if the user needs additional help.
        """)
};

while (true)
{
    Console.WriteLine("Enter your message:");
    string? input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input))
        continue;

    conversationLog.Add(new ChatMessage(ChatRole.User, input));

    Console.WriteLine("Assistant:");
    var assistantReply = new StringBuilder();

    await foreach (var chunk in client.GetStreamingResponseAsync(conversationLog))
    {
        Console.Write(chunk.Text);
        assistantReply.Append(chunk.Text);
    }

    conversationLog.Add(
        new ChatMessage(ChatRole.Assistant, assistantReply.ToString())
    );

    Console.WriteLine();
}*/

#endregion