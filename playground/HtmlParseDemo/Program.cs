// Copyright (c) Quinntyne Brown. All Rights Reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using Endpoint;
using Endpoint.Artifacts.Abstractions;
using Endpoint.Engineering.AI.Services;
using Endpoint.Engineering.Cli.Commands;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

// Setup dependency injection
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Add MediatR and scan the assembly containing HtmlParseRequest
services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<HtmlParseRequest>());

// Add core services from Endpoint assembly
services.AddCoreServices(typeof(IArtifactGenerator).Assembly);

// Add DotNet services
services.AddDotNetServices();

// Add Engineering services (includes IHtmlParserService)
services.AddEngineeringServices();

var serviceProvider = services.BuildServiceProvider();

var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
var mediator = serviceProvider.GetRequiredService<IMediator>();
var htmlParserService = serviceProvider.GetRequiredService<IHtmlParserService>();

try
{
    logger.LogInformation("=== HTML Parse Demo ===");
    logger.LogInformation("This demo shows how to parse HTML and extract token-efficient content for LLM consumption.");
    logger.LogInformation("");

    // Demo 1: Parse HTML from a file using the service directly
    logger.LogInformation("--- Demo 1: Parse HTML from file using service directly ---");
    logger.LogInformation("");

    var sampleHtmlPath = Path.Combine(AppContext.BaseDirectory, "sample.html");

    if (File.Exists(sampleHtmlPath))
    {
        logger.LogInformation("Parsing sample.html...");
        var parsedContent = await htmlParserService.ParseHtmlFromFileAsync(sampleHtmlPath);

        logger.LogInformation("Original HTML size: {OriginalSize} characters", (await File.ReadAllTextAsync(sampleHtmlPath)).Length);
        logger.LogInformation("Parsed content size: {ParsedSize} characters", parsedContent.Length);
        logger.LogInformation("");
        logger.LogInformation("Parsed content:");
        logger.LogInformation("----------------");
        Console.WriteLine(parsedContent);
        logger.LogInformation("----------------");
    }
    else
    {
        logger.LogWarning("sample.html not found at {Path}", sampleHtmlPath);
    }

    logger.LogInformation("");

    // Demo 2: Parse inline HTML string using the service
    logger.LogInformation("--- Demo 2: Parse inline HTML string ---");
    logger.LogInformation("");

    var inlineHtml = """
        <!DOCTYPE html>
        <html>
        <head>
            <title>Product Page</title>
            <link rel="stylesheet" href="styles.css">
            <script src="app.js"></script>
            <style>
                .product { color: blue; font-size: 14px; }
            </style>
        </head>
        <body>
            <header>
                <nav>
                    <a href="/">Home</a>
                    <a href="/products">Products</a>
                    <a href="/about">About Us</a>
                </nav>
            </header>

            <main>
                <h1>Featured Product</h1>
                <p>Discover our amazing product that will change your life.</p>

                <section>
                    <h2>Features</h2>
                    <ul>
                        <li>Easy to use</li>
                        <li>Fast performance</li>
                        <li>Great value</li>
                    </ul>
                </section>

                <section>
                    <h2>Pricing</h2>
                    <table>
                        <tr><th>Plan</th><th>Price</th></tr>
                        <tr><td>Basic</td><td>$9.99/mo</td></tr>
                        <tr><td>Pro</td><td>$19.99/mo</td></tr>
                    </table>
                </section>

                <form>
                    <label for="email">Email Address</label>
                    <input type="email" id="email" placeholder="Enter your email">
                    <button type="submit">Subscribe</button>
                </form>
            </main>

            <footer>
                <p>&copy; 2024 Our Company. All rights reserved.</p>
            </footer>

            <script>
                // Analytics tracking code
                console.log('Page loaded');
            </script>
        </body>
        </html>
        """;

    var inlineParsed = htmlParserService.ParseHtml(inlineHtml);

    logger.LogInformation("Original HTML size: {OriginalSize} characters", inlineHtml.Length);
    logger.LogInformation("Parsed content size: {ParsedSize} characters", inlineParsed.Length);
    logger.LogInformation("Token reduction: ~{Reduction}%", Math.Round((1 - (double)inlineParsed.Length / inlineHtml.Length) * 100));
    logger.LogInformation("");
    logger.LogInformation("Parsed content:");
    logger.LogInformation("----------------");
    Console.WriteLine(inlineParsed);
    logger.LogInformation("----------------");
    logger.LogInformation("");

    // Demo 3: Use the CLI command via MediatR
    logger.LogInformation("--- Demo 3: Using CLI command via MediatR ---");
    logger.LogInformation("");

    if (File.Exists(sampleHtmlPath))
    {
        var request = new HtmlParseRequest
        {
            Path = sampleHtmlPath
        };

        logger.LogInformation("Sending HtmlParseRequest via MediatR...");
        await mediator.Send(request);
    }
    else
    {
        logger.LogInformation("Skipping MediatR demo (sample.html not found)");
    }

    logger.LogInformation("");

    // Demo 4: Fetch and parse HTML from a URL
    logger.LogInformation("--- Demo 4: Fetch and parse HTML from URL ---");
    logger.LogInformation("");

    var testUrl = "https://example.com";
    logger.LogInformation("Fetching HTML from: {Url}", testUrl);

    try
    {
        var urlParsed = await htmlParserService.ParseHtmlFromUrlAsync(testUrl);

        logger.LogInformation("Parsed content size: {ParsedSize} characters", urlParsed.Length);
        logger.LogInformation("");
        logger.LogInformation("Parsed content from {Url}:", testUrl);
        logger.LogInformation("----------------");
        Console.WriteLine(urlParsed);
        logger.LogInformation("----------------");
    }
    catch (Exception urlEx)
    {
        logger.LogWarning(urlEx, "Could not fetch URL (this is expected in offline environments)");
    }

    logger.LogInformation("");

    // Demo 5: Use the CLI command with URL via MediatR
    logger.LogInformation("--- Demo 5: Using CLI command with URL via MediatR ---");
    logger.LogInformation("");

    try
    {
        var urlRequest = new HtmlParseRequest
        {
            Url = testUrl
        };

        logger.LogInformation("Sending HtmlParseRequest with URL via MediatR...");
        await mediator.Send(urlRequest);
    }
    catch (Exception urlEx)
    {
        logger.LogWarning(urlEx, "Could not fetch URL via command (this is expected in offline environments)");
    }

    logger.LogInformation("");
    logger.LogInformation("=== Demo Completed Successfully! ===");
}
catch (Exception ex)
{
    logger.LogError(ex, "Error during HTML parse demo");
}
