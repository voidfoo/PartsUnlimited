// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;
using System;

namespace PartsUnlimited.WebJobs.UpdateProductInventory
{
    public class Program
    {
        public IConfiguration Configuration { get; set; }

        public Program(IApplicationEnvironment env)
        {
            var builder = new ConfigurationBuilder(env.ApplicationBasePath)
                    .AddJsonFile("config.json");

            Configuration = builder.Build();
        }

        public int Main(string[] args)
        {
            var webjobsConnectionString = Configuration["Data:AzureWebJobsStorage:ConnectionString"];
            var dbConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];

            if (string.IsNullOrWhiteSpace(webjobsConnectionString))
            {
                Console.WriteLine("The configuration value for Azure Web Jobs Connection String is missing.");
                return 10;
            }

            if (string.IsNullOrWhiteSpace(dbConnectionString))
            {
                Console.WriteLine("The configuration value for Database Connection String is missing.");
                return 10;
            }

            var jobHostConfig = new JobHostConfiguration(webjobsConnectionString);
            var host = new JobHost(jobHostConfig);

            host.RunAndBlock();
            return 0;
        }
    }
}
