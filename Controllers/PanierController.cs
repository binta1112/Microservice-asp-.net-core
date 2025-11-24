using MicroServicePanier.Model;
using Microsoft.AspNetCore.Mvc;
using MicroServicePanier.Service;

namespace MicroServicePanier.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PanierController : ControllerBase // Inherits from ControllerBase for API controllers
    {
        private readonly PanierService _cartService;

        public PanierController(PanierService service)
        {
            _cartService = service;
        }
        // Récupérer le panier d'un utilisateur
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetCart(string userId)
            => Ok(await _cartService.GetCartAsync(userId));

        // Supprimer le panier d'un utilisateur
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteCart(string userId)
        {
            await _cartService.DeleteCartAsync(userId);
            return NoContent();
        }

        // Ajouter ou incrémenter un produit dans le panier
        [HttpPost("{userId}")]
        public async Task<IActionResult> AddOrUpdateItem(string userId, [FromBody] CartItem item)
        {
            if (item == null) return BadRequest();
            var cart = await _cartService.AddOrUpdateItemAsync(userId, item);
            return Ok(cart);
        }

        // Augmenter la quantité (par défaut +1)
        [HttpPatch("{userId}/items/{productId}/increase")]
        public async Task<IActionResult> IncreaseQuantity(string userId, int productId, [FromQuery] int amount = 1)
        {
            var item = await _cartService.IncreaseQuantityAsync(userId, productId, amount);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // Diminuer la quantité (par défaut -1). Retourne uniquement l'item concerné.
        [HttpPatch("{userId}/items/{productId}/decrease")]
        public async Task<IActionResult> DecreaseQuantity(string userId, int productId, [FromQuery] int amount = 1)
        {
            var item = await _cartService.DecreaseQuantityAsync(userId, productId, amount);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // Supprimer un produit du panier
        [HttpDelete("{userId}/items/{productId}")]
        public async Task<IActionResult> RemoveItem(string userId, int productId)
        {
            var cart = await _cartService.RemoveItemAsync(userId, productId);
            return Ok(cart);
        }
    }
}
