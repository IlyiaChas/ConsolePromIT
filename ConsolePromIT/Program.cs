using ConsolePromIT.Data;
using ConsolePromIT.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;

namespace ConsolePromIT
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
             .ConfigureServices((context, services) =>
             {
                 services.AddScoped<ParsingService>();
                 var connection = context.Configuration.GetConnectionString("DefaultConnection");
                 services.AddDbContext<DataContext>(options => options.UseSqlServer(connection));
             })
             .Build();


             host.Services.GetRequiredService<ParsingService>()?.ParsingFile();
         



        }
    }
}
