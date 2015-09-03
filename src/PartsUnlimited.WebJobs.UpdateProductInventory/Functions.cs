﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.WebJobs;
using Microsoft.Data.Entity;
using Microsoft.Framework.Configuration;
using PartsUnlimited.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PartsUnlimited.WebJobs.UpdateProductInventory
{
    public class Functions
    {
        public async static Task UpdateProductProcessTaskAsync([QueueTrigger("product")] ProductMessage message)
        {
            var builder = new ConfigurationBuilder(Environment.CurrentDirectory)
                    .AddJsonFile("config.json");

            var config = builder.Build();
            var connectionString = config["Data:DefaultConnection:ConnectionString"];

            using (var context = new PartsUnlimitedContext(connectionString))
            {
                var dbProductList = await context.Products.ToListAsync();
                foreach (var queueProduct in message.ProductList)
                {
                    var dbProduct = dbProductList.SingleOrDefault(x => x.SkuNumber == queueProduct.SkuNumber);

                    if (dbProduct != null)
                    {
                        dbProduct.Inventory = queueProduct.Inventory;
                        dbProduct.LeadTime = queueProduct.LeadTime;
                    }
                }
                await context.SaveChangesAsync(CancellationToken.None);
            }
        }
    }
}
