# MicroServicePanier

Service ASP.NET Core (.NET 8) pour la gestion d'un panier utilisateur, stocké dans Redis.  
Expose une API REST (contrôleur `PanierController`) et utilise `PanierService` pour persister les paniers dans Redis.

## Caractéristiques
- Stockage des paniers dans Redis (`StackExchange.Redis`)
- Opérations : récupérer, ajouter/incrémenter produit, augmenter/diminuer quantité, supprimer produit
- JSON simple pour les objets `Cart` / `CartItem`
- Déploiement via Docker + `docker-compose`

## Prérequis
- .NET 8 SDK (pour exécution locale)
- Docker & Docker Compose (pour containerisation)
- Redis (local ou via Docker)

## Structure importante
- `Controllers/PanierController.cs` — endpoints REST
- `Service/PanierService.cs` — logique métier + accès Redis
- `Model/Cart.cs`, `Model/CartItem.cs` — modèles
- `Dockerfile`, `docker-compose.yml`
## URL de production
Base URL :  
`https://microservice-asp-net-core-production-53f9.up.railway.app`

Toutes les routes listées ci‑dessous sont relatives à cette base.

## Endpoints (routes)
Toutes les routes sont préfixées par `/api/panier`

- GET `/api/panier/{userId}`  
  Récupérer le panier de l'utilisateur.  
  -> 200 OK + `Cart` ou 200 + `null` si aucun panier.

- POST `/api/panier/{userId}`  
  Ajouter un produit ou incrémenter sa quantité. Body: `CartItem`.  
  -> 200 OK + `Cart`

- DELETE `/api/panier/{userId}`  
  Supprimer le panier complet.  
  -> 204 No Content

- PATCH `/api/panier/{userId}/items/{productId}/increase?amount={n}`  
  Augmenter la quantité (défaut `amount=1`).  
  -> 200 OK + `CartItem`

- PATCH `/api/panier/{userId}/items/{productId}/decrease?amount={n}`  
  Diminuer la quantité (suppr. si <= 0). Retourne uniquement le `CartItem` concerné (si supprimé, `Quantity == 0`).  
  -> 200 OK + `CartItem` ou 404 Not Found si produit absent

- DELETE `/api/panier/{userId}/items/{productId}`  
  Supprimer un produit du panier.  
  -> 200 OK + `Cart`

## Exemple JSON `Cartitem`
Exemple minimal à envoyer en POST (ou comme référence) :
{ 
    "ProductId": 42,
     "ProductName": "Tasse en céramique", 
     "UnitPrice": 12.5, 
     "Quantity": 2,
      "ProductImage": "https://exemple.com/images/tasse.jpg" 
}
`Cart` Panier d'un utilisateur
{ 
    "UserId": 1, 
    "Items": [ 
        { 
            "ProductId": 42, 
            "ProductName": "Tasse en céramique", 
            "UnitPrice": 12.5, 
            "Quantity": 2, 
            "ProductImage": "https://exemple.com/images/tasse.pg" 
        } ] 
}


## Exemples d'appels (curl)
- Récupérer :  
  `curl -sS https://microservice-asp-net-core-production-53f9.up.railway.app/api/panier/1`

- Ajouter / incrémenter :  
  `curl -sS -X POST "https://microservice-asp-net-core-production-53f9.up.railway.app/api/panier/1" -H "Content-Type: application/json" -d '<CartItem JSON>'`

- Augmenter quantité :  
  `curl -sS -X PATCH "https://microservice-asp-net-core-production-53f9.up.railway.app/api/panier/1/items/42/increase?amount=1"`

- Diminuer quantité :  
  `curl -sS -X PATCH "https://microservice-asp-net-core-production-53f9.up.railway.app/api/panier/1/items/42/decrease?amount=1"`

- Supprimer produit :  
  `curl -sS -X DELETE "https://microservice-asp-net-core-production-53f9.up.railway.app/api/panier/1/items/42"`

## Exécution locale (développement)
1. Démarrer Redis local :  
   `docker run --name redis -p 6379:6379 -d redis`

2. Configurer la chaîne Redis dans `Program.cs` ou via variable d'environnement (recommandé) :
   - variable proposée : `REDIS_CONNECTION` (ex. `localhost:6379`)
   - adapter `ConnectionMultiplexer.Connect(...)` pour lire la variable.

3. Lancer l'API :  
   `dotnet run --project MicroServicePanier.csproj`

Pour exécuter avec Docker Compose :  
`docker compose up --build` (le `docker-compose.yml` du projet démarre l'API et Redis).

## Docker / Déploiement
- `Dockerfile` utilise un multi-stage build (SDK pour la compilation, image runtime pour l'exécution) afin de réduire la taille de l'image finale.
- En container, Kestrel n’expose généralement que HTTP ; TLS est normalement géré par un reverse‑proxy (Railway gère le TLS pour l'URL fournie).

## Configuration recommandée
- Utiliser une variable d'environnement pour la connexion Redis au lieu d'une valeur en dur.
- Préfixer les clés Redis par environnement (ex. `dev:cart:{userId}`) pour éviter les collisions.
- Activer Swagger uniquement en __Development__ (par défaut dans le code actuel).

## Remarques techniques
- Clé Redis construite via la méthode `Key(userId)` : format `cart:{userId}`.
- Les opérations : ajout/incrément, augmentation/diminution de quantité, suppression d'item et suppression de panier sont persistées dans Redis sous forme de JSON sérialisé.
