using MicroServicePanier.Model;
using StackExchange.Redis;
using System.Text.Json;


namespace MicroServicePanier.Service
{
    public class PanierService
    {
        private readonly IDatabase _redisDb;

        public PanierService(IDatabase redisDb)
        {
            _redisDb = redisDb;
        }
        // Génère la clé Redis pour un utilisateur
        private string Key(string userId) => $"cart:{userId}";

        // Récupère le panier d'un utilisateur
        public async Task<Cart?> GetCartAsync(string userId)
        {
            // Récupère le panier d'un utilisateur depuis Redis et le désérialise en objet Cart
            var data = await _redisDb.StringGetAsync(Key(userId));
            return data.IsNullOrEmpty ? null : JsonSerializer.Deserialize<Cart>(data!);
        }
        // Met à jour le panier d'un utilisateur
        public async Task<Cart> UpdateCartAsync(Cart cart)
        {
            var json = JsonSerializer.Serialize(cart);
            await _redisDb.StringSetAsync(Key(cart.UserId.ToString()), json);
            return cart;
        }
        // Supprime le panier d'un utilisateur
        public async Task DeleteCartAsync(string userId)
        {
            await _redisDb.KeyDeleteAsync(Key(userId));
        }

        // Ajoute l'item ou incrémente la quantité si présent
        public async Task<Cart> AddOrUpdateItemAsync(string userId, CartItem item)
        {
            // Récupérer le panier de l'utilisateur ou en créer un nouveau
            var cart = await GetCartAsync(userId) ?? new Cart { UserId = int.TryParse(userId, out var id) ? id : 0 };
            var existing = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existing != null)
            {
                existing.Quantity += item.Quantity;
            }
            else
            {
                cart.Items.Add(item);
            }

            await UpdateCartAsync(cart);
            return cart;
        }

        // Augmente la quantité d'un produit (par défaut +1)
        public async Task<CartItem> IncreaseQuantityAsync(string userId, int productId, int amount = 1)
        {
            var cart = await GetCartAsync(userId) ;
            if (cart == null)
            {
                return null!;
            }
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null)
            {
                return null!;
            }
            item.Quantity += amount;           

            //  persister et retourner l'item mis à jour
            await UpdateCartAsync(cart);
            return item;
        }

        // Diminue la quantité et supprime l'item si quantité <= 0
        // Retourne l'item concerné (avec la quantité mise à jour). Si supprimé, Quantity == 0.
        public async Task<CartItem?> DecreaseQuantityAsync(string userId, int productId, int amount = 1)
        {
            // Récupérer le panier de l'utilisateur 
            var cart = await GetCartAsync(userId);
            if (cart == null)
            {
                return null;
            }
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            
            if (item == null)
            {
                return null;
            }

            item.Quantity -= amount;
            if (item.Quantity <= 0)
            {
                // Retirer l'item du panier et persister
                cart.Items.Remove(item);
                await UpdateCartAsync(cart);

                // Retourner l'objet avec Quantity == 0 pour indiquer suppression
                item.Quantity = 0;
                return item;
            }

            // Quantité positive : persister et retourner l'item mis à jour
            await UpdateCartAsync(cart);
            return item;
        }

        // Supprime un produit du panier
        public async Task<Cart> RemoveItemAsync(string userId, int productId)
        {
            var cart = await GetCartAsync(userId);
            if (cart == null)
            {
                return null!;
            }
            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                cart.Items.Remove(item);
                await UpdateCartAsync(cart);
            }
            return cart;
        }
    }
}
