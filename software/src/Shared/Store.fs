namespace Shared

open Azure.Cosmos
open FSharp.Control.Tasks
open System.Threading.Tasks

type Store (connectionString: string) =
    let cosmosClient = new CosmosClient (connectionString)

    let dbName = "Clara"
    let cardsContainer = "Cards"
    let logsContainer = "Logs"
    let cardsContainerPartitionKeyPath = "/id"
    let logsContainerPartitionKeyPath = "/id"

    member x.createDB: Task<unit> = task {
        let! dbResponse = cosmosClient.CreateDatabaseIfNotExistsAsync dbName
        Task.WhenAll
            (dbResponse.Database.CreateContainerIfNotExistsAsync
                (new ContainerProperties (cardsContainer, cardsContainerPartitionKeyPath)),
            dbResponse.Database.CreateContainerIfNotExistsAsync
                (new ContainerProperties (logsContainer, logsContainerPartitionKeyPath)))
            |> ignore
    }

    member x.deleteDB: Task<unit> = task {
        let database = cosmosClient.GetDatabase dbName
        database.DeleteAsync () |> ignore
    }
