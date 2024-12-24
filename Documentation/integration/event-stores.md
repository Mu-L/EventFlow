---
layout: default
title: Event stores
parent: Integration
nav_order: 2
---

# Event stores

By default, EventFlow uses an in-memory event store. However, EventFlow provides support for several alternatives.

- [In-memory](#in-memory) (for testing)
- [Microsoft SQL Server](#mssql-event-store)
- [MongoDB](#mongo-db)
- [Redis](#redis)
- [Files](#files) (for testing)

## In-memory

!!! attention
    The in-memory event store should not be used for production environments, only for testing purposes.

Using the in-memory event store is easy as it's enabled by default, so there is no need to do anything.

## MSSQL event store

See [MSSQL setup](mssql.md) for details on how to get started using Microsoft SQL Server in EventFlow.

To configure EventFlow to use MSSQL as the event store, simply add the `UseMssqlEventStore()` method as shown here.

```csharp
IRootResolver rootResolver = EventFlowOptions.New
  ...
  .UseMssqlEventStore()
  ...
  .CreateResolver();
```

### Create and migrate required MSSQL databases

Before you can use the MSSQL event store, the required database and tables must be created. The database specified in your MSSQL connection will *not* be automatically created; you have to do this yourself.

To make EventFlow create the required tables, execute the following code.

```csharp
var msSqlDatabaseMigrator = rootResolver.Resolve<IMsSqlDatabaseMigrator>();
EventFlowEventStoresMsSql.MigrateDatabase(msSqlDatabaseMigrator);
```

You should do this either on application start or preferably upon application install or update, e.g., when the website is installed.

!!! attention
    If you utilize user permissions in your application, you need to grant the event writer access to the user-defined table type `eventdatamodel_list_type`. EventFlow uses this type to pass entire batches of events to the database.

## PostgreSQL event store

The setup for PostgreSQL is similar to that of MSSQL. See [PostgreSQL setup](postgresql.md) for setup documentation.

## MongoDB

See [MongoDB setup](mongodb.md) for details on how to get started using MongoDB in EventFlow.

To configure EventFlow to use MongoDB as the event store, simply add the `UseMongoDbEventStore()` method as shown here.

```csharp
IRootResolver rootResolver = EventFlowOptions.New
  ...
  .UseMongoDbEventStore()
  ...
  .CreateResolver();
```

## Redis

See [Redis setup](redis.md) for details on how to get started using Redis and EventFlow. Ensure that your database is persistent.

To configure EventFlow to use Redis as the event store, simply add the `UseRedisEventStore()` method as shown in the example below.

```csharp
IRootResolver rootResolver = EventFlowOptions.New
  ...
  .UseRedisEventStore()
  ...
  .CreateResolver();
```

## Files

!!! attention
    The Files event store should not be used for production environments, only for testing purposes.

The file-based event store is useful if you have a set of events that represent a certain scenario and would like to create a test that verifies that the domain handles it correctly.

To use the file-based event store, simply invoke `.UseFilesEventStore("...")` with the path containing the files.

```csharp
var storePath = @"c:\eventstore";
var rootResolver = EventFlowOptions.New
    ...
    .UseFilesEventStore(FilesEventStoreConfiguration.Create(storePath))
    ...
    .CreateResolver();
```
