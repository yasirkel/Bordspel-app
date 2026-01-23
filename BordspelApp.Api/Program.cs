using BordspelApp.Application.Interfaces;
using BordspelApp.Infrastructure.Data;
using BordspelApp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using BordspelApp.Api.GraphQL;
using HotChocolate;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add GraphQL (HotChocolate v13)
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    // Register DTO types explicitly so HotChocolate uses these runtime types
    // instead of traversing EF entities (prevents EF entities like Person
    // from being treated as input types).
    .AddType<BordspelApp.Api.GraphQL.Models.GameNightDto>()
    .AddType<BordspelApp.Api.GraphQL.Models.BoardGameDto>()
    .AddType<BordspelApp.Api.GraphQL.Models.RecommendationDto>();

// Add DbContext
builder.Services.AddDbContext<BordspelAppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Repositories
builder.Services.AddScoped<IPersonRepository, PersonRepository>();
builder.Services.AddScoped<IBoardGameRepository, BoardGameRepository>();
builder.Services.AddScoped<IGameNightRepository, GameNightRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Map GraphQL endpoint for Banana Cake Pop and GraphiQL
app.MapGraphQL("/graphql");

app.Run();
