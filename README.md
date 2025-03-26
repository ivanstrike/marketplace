# Marketplace Backend

It is the server-side component of an online marketplace platform. It enables users to register, list their products for sale, browse and purchase items from other users, and manage their transactions. The project is built using a **microservices architecture** to ensure scalability and maintainability, with services communicating via RabbitMQ, data stored in PostgreSQL, caching handled by Redis, and file storage managed by MinIO.

## Features
- **User Management**: Register and authenticate users with JWT-based authentication.
- **Product Listings**: Create, read, update, and delete (CRUD) product listings, including title, description, price, and images.
- **Order Management**: Add items to a cart.
- **File Storage**: Store product images using MinIO.
- **Scalability**: Microservices architecture with RabbitMQ for inter-service communication.
- **Performance**: Caching with Redis to improve response times for frequently accessed data.

## Installation
Follow these steps to set up the project locally.

1. Clone the repository:
   ```bash
   git clone https://github.com/ivanstrike/marketplace
   cd marketplace
