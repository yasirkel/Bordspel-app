using System;
using BordspelApp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BordspelApp.Tests.TestHelpers;

public sealed class TestWebApplicationFactory : WebApplicationFactory<BordspelApp.Api.Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            var domainDatabaseName = $"BordspelAppDomainTests-{Guid.NewGuid()}";
            var identityDatabaseName = $"BordspelAppIdentityTests-{Guid.NewGuid()}";

            services.RemoveAll<DbContextOptions<BordspelAppDbContext>>();
            services.RemoveAll<IDbContextFactory<BordspelAppDbContext>>();
            services.RemoveAll<BordspelAppDbContext>();
            services.AddDbContextFactory<BordspelAppDbContext>(options =>
                options.UseInMemoryDatabase(domainDatabaseName)
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));

            services.RemoveAll<DbContextOptions<BordspelAppIdentityDbContext>>();
            services.RemoveAll<BordspelAppIdentityDbContext>();
            services.AddDbContext<BordspelAppIdentityDbContext>(options =>
                options.UseInMemoryDatabase(identityDatabaseName)
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning)));
        });
    }
}
