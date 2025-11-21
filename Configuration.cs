using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Massiv
{
    public static class Configuration
    {
        public static IConfiguration AppSettings { get; }

        static Configuration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            AppSettings = builder.Build();
        }

        public static string GetConnectionString(string name = "DB")
        {
            return AppSettings.GetConnectionString(name);
        }
    }
}
