﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Antiforgery;
using PartsUnlimited.Models;
using PartsUnlimited.Telemetry;
using PartsUnlimited.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartsUnlimited.Controllers
{
    public class ShoppingCartController : Controller
    {
        [FromServices]
        public IPartsUnlimitedContext DbContext { get; set; }

        [FromServices]
        public ITelemetryProvider Telemetry { get; set; }

        [FromServices]
        public IAntiforgery Antiforgery { get; set; }

        //
        // GET: /ShoppingCart/

        public IActionResult Index()
        {
            var cart = ShoppingCart.GetCart(DbContext, Context);

            var items = cart.GetCartItems();
            var itemsCount = items.Sum(x => x.Count);
            var subTotal = items.Sum(x => x.Count * x.Product.Price);
            var shipping = itemsCount * (decimal)5.00;
            var tax = (subTotal + shipping) * (decimal)0.05;
            var total = subTotal + shipping + tax;

            var costSummary = new OrderCostSummary
            {
                CartSubTotal = subTotal.ToString("C"),
                CartShipping = shipping.ToString("C"),
                CartTax = tax.ToString("C"),
                CartTotal = total.ToString("C")
            };


            // Set up our ViewModel
            var viewModel = new ShoppingCartViewModel
            {
                CartItems = items,
                CartCount = itemsCount,
                OrderCostSummary = costSummary
            };

            // Track cart review event with measurements
            Telemetry.TrackTrace("Cart/Server/Index");

            // Return the view
            return View(viewModel);
        }

        //
        // GET: /ShoppingCart/AddToCart/5

        public async Task<IActionResult> AddToCart(int id)
        {
            // Retrieve the product from the database
            var addedProduct = DbContext.Products
                .Single(product => product.ProductId == id);

            // Start timer for save process telemetry
            var startTime = System.DateTime.Now;

            // Add it to the shopping cart
            var cart = ShoppingCart.GetCart(DbContext, Context);

            cart.AddToCart(addedProduct);

            await DbContext.SaveChangesAsync(Context.RequestAborted);

            // Trace add process
            var measurements = new Dictionary<string, double>()
            {
                {"ElapsedMilliseconds", System.DateTime.Now.Subtract(startTime).TotalMilliseconds }
            };
            Telemetry.TrackEvent("Cart/Server/Add", null, measurements);

            // Go back to the main store page for more shopping
            return RedirectToAction("Index");
        }

        //
        // AJAX: /ShoppingCart/RemoveFromCart/5
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var formParameters = await Context.Request.ReadFormAsync();
            var requestVerification = formParameters["RequestVerificationToken"];
            string cookieToken = null;
            string formToken = null;

            if (!string.IsNullOrWhiteSpace(requestVerification))
            {
                var tokens = requestVerification.Split(':');

                if (tokens != null && tokens.Length == 2)
                {
                    cookieToken = tokens[0];
                    formToken = tokens[1];
                }
            }

            Antiforgery.ValidateTokens(Context, new AntiforgeryTokenSet(formToken, cookieToken));

            // Start timer for save process telemetry
            var startTime = System.DateTime.Now;

            // Retrieve the current user's shopping cart
            var cart = ShoppingCart.GetCart(DbContext, Context);

            // Get the name of the product to display confirmation
            // TODO [EF] Turn into one query once query of related data is enabled
            int productId = DbContext.CartItems.Single(item => item.CartItemId == id).ProductId;
            string productName = DbContext.Products.Single(a => a.ProductId == productId).Title;

            // Remove from cart
            int itemCount = cart.RemoveFromCart(id);

            await DbContext.SaveChangesAsync(Context.RequestAborted);

            string removed = (itemCount > 0) ? " 1 copy of " : string.Empty;

            // Trace remove process
            var measurements = new Dictionary<string, double>()
            {
                {"ElapsedMilliseconds", System.DateTime.Now.Subtract(startTime).TotalMilliseconds }
            };
            Telemetry.TrackEvent("Cart/Server/Remove", null, measurements);

            // Display the confirmation message
            var items = cart.GetCartItems();
            var itemsCount = items.Sum(x => x.Count);
            var subTotal = items.Sum(x => x.Count * x.Product.Price);
            var shipping = itemsCount * (decimal)5.00;
            var tax = (subTotal + shipping) * (decimal)0.05;
            var total = subTotal + shipping + tax;

            var results = new ShoppingCartRemoveViewModel
            {
                Message = removed + productName +
                    " has been removed from your shopping cart.",
                CartSubTotal = subTotal.ToString("C"),
                CartShipping = shipping.ToString("C"),
                CartTax = tax.ToString("C"),
                CartTotal = total.ToString("C"),
                CartCount = itemsCount,
                ItemCount = itemCount,
                DeleteId = id
            };

            return Json(results);
        }
    }
}