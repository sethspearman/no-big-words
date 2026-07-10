using System.Text.Json.Serialization;
using NoBigWords.Infrastructure.Options;
using NoBigWords.Infrastructure.Providers;
using NoBigWords.Core.Abstractions;
using NoBigWords.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin();
    });
});

var dataRoot = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", "data"));
var dataFiles = new DataFilesOptions
{
    AllowedWordsPath = Path.Combine(dataRoot, "allowed-words-normalized.txt"),
    ReplacementsPath = Path.Combine(dataRoot, "replacements.generated.json"),
};

builder.Services.AddSingleton<IAllowedWordsProvider>(_ => new FileAllowedWordsProvider(dataFiles.AllowedWordsPath));
builder.Services.AddSingleton<IReplacementDictionary>(_ => new JsonReplacementDictionary(dataFiles.ReplacementsPath));
builder.Services.AddSingleton<IAiRewriteService, StubAiRewriteService>();
builder.Services.AddSingleton<IRewriteService, RewriteService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;
