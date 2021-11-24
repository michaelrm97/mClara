namespace Store

open Azure.Cosmos
open FSharp.Control.Tasks
open System
open System.Threading.Tasks
open System.Collections.Generic

open Shared

type Store (connectionString: string) =
    let cosmosClient = new CosmosClient (connectionString)

    let dbName = "Clara"
    let cardsContainerName = "Cards"
    let logsContainerName = "Logs"
    let cardsContainerPartitionKeyPath = "/id"
    let logsContainerPartitionKeyPath = "/id"

    let claraDB = cosmosClient.GetDatabase dbName
    let cardsContainer = cosmosClient.GetContainer (dbName, cardsContainerName)
    let logsContainer = cosmosClient.GetContainer (dbName, logsContainerName)

    let byIdQueryText (id: string) = sprintf "SELECT * FROM c WHERE c.id = '%s'" id
    let byNameQueryText (name: string) = sprintf "SELECT * FROM c WHERE c.Name = '%s'" name

    // Used by PowerShell
    member x._getItemsHelper<'T> (enumerator: IAsyncEnumerator<'T>): Task<'T list> = task {
        let! hasNext = enumerator.MoveNextAsync ()
        match hasNext with
        | true ->
            let item = enumerator.Current
            let! others = x._getItemsHelper enumerator
            return item :: others
        | false -> return []
    }

    member x._getItems<'T> (iterator: Azure.AsyncPageable<'T>): Task<'T list> = task {
        let enumerator = iterator.GetAsyncEnumerator ()
        let! items = x._getItemsHelper enumerator
        let! _ = enumerator.DisposeAsync ()
        return items
    }

    member x.createDB unit: Task<unit> = task {
        let! dbResponse = cosmosClient.CreateDatabaseIfNotExistsAsync dbName
        Task.WhenAll
            (dbResponse.Database.CreateContainerIfNotExistsAsync
                (new ContainerProperties (cardsContainerName, cardsContainerPartitionKeyPath)),
            dbResponse.Database.CreateContainerIfNotExistsAsync
                (new ContainerProperties (logsContainerName, logsContainerPartitionKeyPath)))
            |> ignore
    }

    member x.deleteDB unit: Task<unit> = task {
        claraDB.DeleteAsync () |> ignore
    }

    member x.clearCardsContainer unit: Task<unit> = task {
        cardsContainer.DeleteContainerAsync().Result |> ignore
        (claraDB.CreateContainerAsync
            (new ContainerProperties (cardsContainerName, cardsContainerPartitionKeyPath))).Result
            |> ignore
    }

    member x.clearLogsContainer unit: Task<unit> = task {
        logsContainer.DeleteContainerAsync().Result |> ignore
        (claraDB.CreateContainerAsync
            (new ContainerProperties (logsContainerName, logsContainerPartitionKeyPath))).Result
            |> ignore
    }

    member x.listCards unit: Task<Card list> = task {
        let sqlQueryText = "SELECT * FROM c";
        let queryIterator = cardsContainer.GetItemQueryIterator<Card> (sqlQueryText)
        return! x._getItems queryIterator
    }

    member x.getCard (id: string): Task<Card> = task {
        let sqlQueryText = byIdQueryText id
        let queryIterator = cardsContainer.GetItemQueryIterator<Card> (sqlQueryText)
        let! items = x._getItems queryIterator
        return List.exactlyOne items
    }

    member x.getCardByName (name: string): Task<Card> = task {
        let sqlQueryText = byNameQueryText name
        let queryIterator = cardsContainer.GetItemQueryIterator<Card> (sqlQueryText)
        let! items = x._getItems queryIterator
        return List.exactlyOne items
    }

    member x.newCard (id: string) (name: string) (displayName: string) (content: string) (overwrite: bool): Task<bool> = task {
        let currentTime = DateTimeOffset.UtcNow
        let card: Card = {
            Id = id
            Name = name
            DisplayName = displayName
            Content = content
            Comment = null
            Reply = null
            Created = currentTime
            ContentLastModified = currentTime
            CommentLastModified = Nullable ()
            ReplyLastModified = Nullable ()
        }

        return! x.addRawCard card overwrite
    }

    member x.editCardHelper
            (items: Card list)
            (id: string)
            (newName: string)
            (newDisplayName: string)
            (newContent: string)
            (newReply: string)
            (forceReply: bool)
            (create: bool): Task<bool> = task {
        let currentTime = DateTimeOffset.UtcNow

        if List.length items = 1 then
            let item = List.exactlyOne items
            if (String.IsNullOrEmpty item.Comment) && ((not (String.IsNullOrEmpty newReply)) && (not forceReply)) then
                return false
            else
                let card: Card = {
                    Id = item.Id
                    Name = if String.IsNullOrEmpty newName then item.Name else newName
                    DisplayName = if String.IsNullOrEmpty newDisplayName then item.DisplayName else newDisplayName
                    Content = if String.IsNullOrEmpty newContent then item.Content else newContent
                    Comment = item.Comment
                    Reply = if String.IsNullOrEmpty newReply then item.Reply else newReply
                    Created = item.Created
                    ContentLastModified = if String.IsNullOrEmpty newContent then item.ContentLastModified else currentTime
                    CommentLastModified = item.CommentLastModified
                    ReplyLastModified = if String.IsNullOrEmpty newReply then item.ReplyLastModified else Nullable currentTime
                }
                let! _ = cardsContainer.ReplaceItemAsync (card, item.Id, new PartitionKey(item.Id))
                return true
        else
            if (not create) || String.IsNullOrEmpty newName || ((not (String.IsNullOrEmpty newReply)) && (not forceReply)) then
                return false
            else
                let card: Card = {
                    Id = id
                    Name = newName
                    DisplayName = newDisplayName
                    Content = newContent
                    Comment = null
                    Reply = newReply
                    Created = currentTime
                    ContentLastModified = currentTime
                    CommentLastModified = Nullable ()
                    ReplyLastModified = if String.IsNullOrEmpty newReply then Nullable() else Nullable currentTime
                }
                try
                    let! _ = cardsContainer.CreateItemAsync (card, new PartitionKey (card.Id))
                    return true
                with
                | :? CosmosException as cex when cex.ErrorCode = "Conflict" -> return false
    }

    member x.editCard
            (id: string)
            (newName: string)
            (newDisplayName : string)
            (newContent: string)
            (newReply: string)
            (forceReply: bool)
            (create: bool): Task<bool> = task {
        let sqlQueryText = byIdQueryText id
        let queryIterator = cardsContainer.GetItemQueryIterator<Card> (sqlQueryText)
        let! items = x._getItems queryIterator
        return! x.editCardHelper items id newName newDisplayName newContent newReply forceReply create
    }

    member x.editCardByName
            (name: string)
            (backupId: string)
            (newName: string)
            (newDisplayName : string)
            (newContent: string)
            (newReply: string)
            (forceReply: bool)
            (create: bool): Task<bool> = task {
        let sqlQueryText = byNameQueryText name
        let queryIterator = cardsContainer.GetItemQueryIterator<Card> (sqlQueryText)
        let! items = x._getItems queryIterator
        return! x.editCardHelper items backupId newName newDisplayName  newContent newReply forceReply create
    }

    member x.deleteCard (id: string): Task<bool> = task {
        let! _itemResponse = cardsContainer.DeleteItemAsync<Card> (id, new PartitionKey(id))
        return true
    }

    member x.deleteCardByName (name: string): Task<bool> = task {
        let sqlQueryText = sprintf "SELECT * FROM c WHERE c.Name = '%s'" name
        let queryIterator = cardsContainer.GetItemQueryIterator<Card> (sqlQueryText)
        let! items = x._getItems queryIterator

        if List.length items = 1 then
            let item = List.exactlyOne items
            return! x.deleteCard item.Id
        else return false // Could not find card with name
    }

    member x.getLogs (operation: string) (cardId: string) (region: string): Task<Log list> = task {
        let sqlQueryText =
            sprintf "SELECT * FROM c"

        let whereClauses = [
            if String.IsNullOrEmpty operation then "" else sprintf "c.Operation = '%s'" operation
            if String.IsNullOrEmpty cardId then "" else sprintf "c.CardId = '%s'" cardId
            if String.IsNullOrEmpty region then "" else sprintf "c.Region = '%s'" region
        ]

        let whereClause = String.Join (" AND ", List.filter (String.IsNullOrEmpty >> not) whereClauses)

        let fullQuery =
            if String.IsNullOrEmpty whereClause then
                sqlQueryText else sprintf "%s WHERE %s" sqlQueryText whereClause

        let queryIterator = logsContainer.GetItemQueryIterator<Log> (fullQuery)
        return! x._getItems queryIterator
    }

    member x.deleteLog (id: string): Task<unit> = task {
        logsContainer.DeleteItemAsync(id, new PartitionKey (id)) |> ignore
    }

    member x.addRawLog (log: Log): Task<unit> = task {
        logsContainer.UpsertItemAsync(log, new PartitionKey(log.Id)) |> ignore
    }

    member x.addRawCard (card: Card) (overwrite: bool): Task<bool> = task {
        if overwrite then
            cardsContainer.UpsertItemAsync (card, new PartitionKey (card.Id)) |> ignore
            return true
        else
            try
                let! _ = cardsContainer.CreateItemAsync (card, new PartitionKey (card.Id))
                return true
            with
            | :? CosmosException as cex when cex.ErrorCode = "Conflict" -> return false
    }

    // Used by server
    member x.getCardResponse (id: string): Task<CardResponse option> = task {
        let sqlQueryText =
            sprintf "SELECT c.id, c.Name, c.Content, c.Comment, c.Reply, c.CommentLastModified, c.ReplyLastModified FROM c WHERE c.id = '%s'" id
        let queryIterator = cardsContainer.GetItemQueryIterator<CardResponse> (sqlQueryText)
        let! items = x._getItems queryIterator
        return match items with
                | item :: [] -> Some item
                | _ -> None
    }

    member x.addLog (operation: string) (cardId: string) (accessTime: DateTimeOffset) (region: string): Task<unit> = task {
        let log = {
            Id = Guid.NewGuid().ToString()
            Operation = operation
            CardId = cardId
            AccessTime = accessTime
            Region = region
        }

        logsContainer.CreateItemAsync(log, new PartitionKey(log.Id)) |> ignore
    }

    member x.addComment (id: string) (comment: string) (accessTime: DateTimeOffset): Task<bool> = task {
        let sqlQueryText = byIdQueryText id
        let queryIterator = cardsContainer.GetItemQueryIterator<Card> (sqlQueryText)
        let! items = x._getItems queryIterator
        match items with
        | item :: [] ->
            let newItem = {
                Id = item.Id
                Name = item.Name
                DisplayName = item.DisplayName
                Content = item.Content
                Comment = comment
                Reply = item.Reply
                Created = item.Created
                ContentLastModified = item.ContentLastModified
                CommentLastModified = Nullable accessTime
                ReplyLastModified = item.ReplyLastModified
            }
            let! _ = cardsContainer.ReplaceItemAsync (newItem, item.Id, new PartitionKey(item.Id))
            return true
        | _ -> return false
    }
