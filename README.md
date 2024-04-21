# MassTransit.BusOutboxDeliveryPerf
This repo is for tinkering with the MassTransit transactional bus outbox delivery performance with SQL Server.

## How to run
1. Ensure SQLLocalDB/SQL Server is reachable
2. Run the Publisher project to insert messages to the bus outbox
3. Run the Sweeper project to deliver messages from the outbox to the (in-memory) broker

## CustomBusOutboxDeliveryService
If wanting to test a custom implementation of `BusOutboxDeliveryService`, `CustomBusOutboxDeliveryService`, make these changes in the Sweeper project:  
```csharp
builder.Services.RemoveHostedService<BusOutboxDeliveryService<MessageDbContext>>();

builder.Services.AddHostedService<CustomBusOutboxDeliveryService<MessageDbContext>>();
```
