// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using Microsoft.AspNet.Builder;
using Microsoft.Azure.WebJobs;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Dnx.Runtime;

namespace PartsUnlimited.WebJobs.ProcessOrder
{
    public class Program
    {
        public IConfiguration Configuration { get; private set; }

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

            var jobHostConfig = new JobHostConfiguration(Configuration["Data:AzureWebJobsStorage:ConnectionString"]);
            var host = new JobHost(jobHostConfig);
            var methodInfo = typeof(Functions).GetMethods().First();

            host.Call(methodInfo);
            return 0;
        }
    }
}
