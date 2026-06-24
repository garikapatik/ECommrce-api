# Grocery Ecommerce API (ASP.NET Core + SQLite)

This project is a backend API for a grocery ecommerce app using ASP.NET Core Web API and SQLite.

## Features

- Product search API
- User information API
- Order history API
- Create user API
- Update user API
- Create order API
- JWT authentication API
- SQLite database with initial seeded data

## Tech Stack

- ASP.NET Core 8 Web API
- Entity Framework Core 8
- SQLite
- Swagger (OpenAPI)

## Run the project

1. Open terminal in project folder.
2. Restore packages:

```bash
dotnet restore
```

3. Run API:

```bash
dotnet run
```

When app starts for the first time:
- `grocery.db` is created automatically
- Initial users, products, orders, and order items are inserted

## API Endpoints

### 1. Product Search

`GET /api/products/search?query=milk&category=Dairy&minPrice=1&maxPrice=5&page=1&pageSize=10&sortBy=price&sortDirection=asc`

Supported sorting values:
- `sortBy`: `name`, `price`, `category`
- `sortDirection`: `asc`, `desc`

Response now includes pagination metadata (`page`, `pageSize`, `totalCount`, `items`).

### 2. Auth Login (JWT)

`POST /api/auth/login`

Request body:

```json
{
	"email": "anita@example.com",
	"password": "Pass@123"
}
```

Use the returned token as Bearer token in Authorization header for protected APIs.

### 3. Create User

`POST /api/users`

### 4. User Information (Protected)

`GET /api/users/{id}`

Examples:
- `GET /api/users/1`
- `GET /api/users/2`

### 5. Update User (Protected)

`PUT /api/users/{id}`

### 6. Order History (Protected)

`GET /api/orders/history/{userId}`

Examples:
- `GET /api/orders/history/1`
- `GET /api/orders/history/2`

### 7. Create Order (Protected)

`POST /api/orders`

Request body:

```json
{
	"userId": 1,
	"items": [
		{ "productId": 1, "quantity": 2 },
		{ "productId": 3, "quantity": 1 }
	]
}

## Seeded sample data

- Users: Anita Sharma, Rahul Verma
- Products: milk, bread, banana, tomato, basmati rice
- Orders: sample delivered and shipped orders with items
- Default seeded login password for sample users: `Pass@123`
