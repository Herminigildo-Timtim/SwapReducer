using Argus.Sync.Data.Models;
using Argus.Sync.Extensions;
using swap_reducer.Models;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCardanoIndexer<SwapDbContext>(builder.Configuration);
builder.Services.AddReducers<SwapDbContext, IReducerModel>(builder.Configuration);

WebApplication app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
