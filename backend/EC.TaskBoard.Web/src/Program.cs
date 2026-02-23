using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using StackExchange.Redis;
using EC.TaskBoard.Web.Shared.Configs;
using EC.TaskBoard.Web.Shared.Middleware;
using EC.TaskBoard.Web.Common.Framework.Hooks;
using EC.TaskBoard.Web.Common.Framework.Cache.Redis;
using EC.TaskBoard.Web.Common.Framework.Cache.Common;
using EC.TaskBoard.Web.Common.Framework.Persistence.Common;
using EC.TaskBoard.Web.Common.Framework.Persistence.Postgres;
using EC.TaskBoard.Web.Common.Foundation.Orchestration.Handlers;
using EC.TaskBoard.Web.Features.Board.Domain.Events;
using EC.TaskBoard.Web.Features.Board.Presentation.API.REST.Handlers;
using EC.TaskBoard.Web.Features.Board.Orchestration.Handlers.Events;
using EC.TaskBoard.Web.Features.Board.Orchestration.Mappings;
using EC.TaskBoard.Web.Features.Board.Orchestration.Services;
using EC.TaskBoard.Web.Features.Board.Orchestration.Interfaces;
using EC.TaskBoard.Web.Features.Board.Infrastructure.Persistence;
using EC.TaskBoard.Web.Features.Board.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Path.Combine(Directory.GetCurrentDirectory(), "etc")
});

builder.Services.Configure<CardConfig>(builder.Configuration.GetSection("Card"));
builder.Services.Configure<ListConfig>(builder.Configuration.GetSection("List"));
builder.Services.Configure<IdempotencyConfig>(builder.Configuration.GetSection("Idempotency"));

builder.Services.AddExceptionHandler<DomainExceptionHandler>();
builder.Services.AddExceptionHandler<EntryNotFoundExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly)
);

builder.Services.AddAutoMapper(cfg => cfg.AddProfile(new BoardProfile()));

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var (key, value) in context.ModelState)
        {
            if (value.Errors.Count > 0)
                errors[key] = value.Errors.Select(e => e.ErrorMessage).ToArray();
        }

        return new UnprocessableEntityObjectResult(new
        {
            message = "Validation failed",
            errors
        });
    };
});

builder.Services.AddScoped<ICardService, CardService>();
builder.Services.AddScoped<IListService, ListService>();
builder.Services.AddScoped<IActivityLogService, ActivityLogService>();

builder.Services.AddScoped<ICardRepository, CardRepository>();
builder.Services.AddScoped<IListRepository, ListRepository>();
builder.Services.AddScoped<IActivityLogRepository, ActivityLogRepository>();

builder.Services.AddScoped<IDomainEventHandler<CardAttributeChangedEvent>, CardAttributeChangedEventHandler>();

builder.Services.AddScoped<DomainEventPropagationInterceptor>();
builder.Services.AddScoped<EntityTimestampPopulationInterceptor>();

builder.Services.AddScoped<IDatabaseExceptionTranslator, PostgresTranslator>();

builder.Services.AddDbContext<BoardDbContext>((provider, options) =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DatabaseConnection")
            ?? throw new InvalidOperationException("DatabaseConnection is not configured.")
    );

    options.AddInterceptors(
        provider.GetRequiredService<DomainEventPropagationInterceptor>(),
        provider.GetRequiredService<EntityTimestampPopulationInterceptor>()
    );
});

var redisConnectionString = builder.Configuration.GetConnectionString("CacheIdempotencyConnection")
    ?? throw new InvalidOperationException("CacheIdempotencyConnection is not configured.");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConnectionString;
});

builder.Services.AddSingleton<IConnectionMultiplexer>(
    _ => ConnectionMultiplexer.Connect(redisConnectionString)
);

builder.Services.AddSingleton<ICacheClient, RedisCacheClient>();

builder.Services.AddScoped<IdempotencyMiddleware>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseRouting();

app.UseMiddleware<IdempotencyMiddleware>();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BoardDbContext>();
    await db.Database.MigrateAsync();
}

app.Run();
