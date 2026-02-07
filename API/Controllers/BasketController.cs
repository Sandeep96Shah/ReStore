using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class BasketController(StoreContext context) : BaseApiController
    {
        [HttpGet]
        public async Task<ActionResult<BasketDto>> GetBasket()
        {
            var basket = await RetrieveBasket();
            if (basket == null) return NotFound();
            return basket.ToDto();
        }

        [HttpPost]
        public async Task<ActionResult<BasketDto>> AddItemToBasket(int productId, int quantity)
        {
            // get Basket
            var basket = await RetrieveBasket();

            // Create Basket
            basket ??= CreateBasket();

            // get product
            var product = await context.Products.FindAsync(productId);
            if (product == null) return BadRequest("Problem adding item to basket as item is not found in DB");

            // add item to basket
            basket.AddItem(product, quantity);

            // save changes
            var result = await context.SaveChangesAsync() > 0;

            if(result) return CreatedAtAction(nameof(GetBasket), basket.ToDto());

            return BadRequest("Problem adding item to basket");
        }

        public async Task<ActionResult> RemoveBasketItem(int productId, int quantity)
        {
            // Get basket
            var basket = await RetrieveBasket();
            if (basket == null) return BadRequest();

            // reduce or remove item from basket.
            basket.RemoveItem(productId, quantity);

            // save changes.
            var result = await context.SaveChangesAsync() > 0;

            if (result) return Ok();

            return BadRequest("Problem removing item from basket");
        }
        private Basket CreateBasket()
        {
            var basketId = Guid.NewGuid().ToString();
            var cookiesOptions = new CookieOptions
            {
                IsEssential = true,
                Expires = DateTime.Now.AddDays(30)
            };
            Response.Cookies.Append("basketId", basketId, cookiesOptions);
            var basket = new Basket { BasketId = basketId };
            context.Baskets.Add(basket);
            return basket;
        }

        private async Task<Basket?> RetrieveBasket()
        {
            return await context.Baskets
                .Include(x => x.Items)
                .ThenInclude(x => x.Product)
                .SingleOrDefaultAsync(x => x.BasketId == Request.Cookies["basketId"]);
        }
    }
}
