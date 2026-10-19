
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var summaries = new[]
{
    "Ger", "Foarte frig", "Frig", "Răcoare", "Blând",
    "Călduț", "Plăcut", "Cald", "Canicular", "Arșiță"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast(
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/welcome", () =>
    new { Mensaje = "Salut Jorgesys, acesta este serviciul meu web pentru a afla vremea de azi!" });

// Nuevo endpoint: SearchNews | GET Request
//https://api.nytimes.com/svc/search/v2/articlesearch.json?q=Mexico&api-key=4H5XEUGjtufCj2oJvPwBjZKWDLbuLO5Hy1GX8zGq4APDE7sX
app.MapGet("/NYsearchNews", async (
    IHttpClientFactory factory,
    IConfiguration config,
    string? q) =>
{
    var apiKey = config["NYTimes:ApiKey"];

    if (string.IsNullOrWhiteSpace(apiKey))
        return Results.Problem("API key no configurada...");

    var query = string.IsNullOrWhiteSpace(q) ? "Mexico" : q;

    var url =
        "https://api.nytimes.com/svc/search/v2/articlesearch.json" +
        $"?q={Uri.EscapeDataString(query)}" +
        $"&api-key={Uri.EscapeDataString(apiKey)}";

    try
    {
        using var response = await factory.CreateClient().GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(
                $"NYTimes respondió con HTTP {(int)response.StatusCode}.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        return Results.Content(
            json, "application/json; charset=utf-8");
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            "Error de conexión con NYTimes.",
            statusCode: StatusCodes.Status502BadGateway);
    }
})
.WithName("SearchNews");


// Nuevo endpoint: SearchNews | GET Request
//https://api.nytimes.com/svc/topstories/v2/home.json?api-key=yourkey
app.MapGet("/NYTopStories", async (
    IHttpClientFactory factory,
    IConfiguration config,
    string? q) =>
{
    var apiKey = config["NYTimes:ApiKey"];

    if (string.IsNullOrWhiteSpace(apiKey))
        return Results.Problem("API key no configurada...");

    var url = "https://api.nytimes.com/svc/topstories/v2/home.json" +
     $"?api-key={Uri.EscapeDataString(apiKey)}";

    try
    {
        using var response = await factory.CreateClient().GetAsync(url);
        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(
                $"NYTimes respondió con HTTP {(int)response.StatusCode}.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        return Results.Content(
            json, "application/json; charset=utf-8");
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            "Error de conexión con NYTimes.",
            statusCode: StatusCodes.Status502BadGateway);
    }
})
.WithName("TopStories");

//* https://beeceptor.com/ POST
//https://app.beeceptor.com/console/mpb70b5173b650c2f020
app.MapPost("/SiteInfo", async (
    SiteInfoRequest request,
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration) =>
{
    var url = "https://mpb70b5173b650c2f020.free.beeceptor.com/API/site/";

    try
    {
        var client = httpClientFactory.CreateClient();
        //var body = new { site = "1" };
        var body = new
        {
            site = request.site ?? "1"
        };

         
        // Send POST with Body
        using var response = await client.PostAsJsonAsync(url, body);

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            return Results.Problem(
                $"Error del servicio remoto: HTTP {(int)response.StatusCode}.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        return Results.Content(
            json,
            "application/json; charset=utf-8");
    }
    catch (HttpRequestException)
    {
        return Results.Problem(
            "No fue posible conectar con el servicio remoto.",
            statusCode: StatusCodes.Status502BadGateway);
    }
})
.WithName("GetSiteInfo");
/* Use Postman with this configuration:
POST  http://localhost:PUERTO/SiteInfo
Body → raw → JSON
{
  "site": "1"
}*/


app.Run();


// Modelos
record SiteInfoRequest(string? site);

record WeatherForecast(
    DateOnly Date,
    int TemperatureC,
    string? Summary)
{
    public int TemperatureF =>
        32 + (int)(TemperatureC / 0.5556);
}
