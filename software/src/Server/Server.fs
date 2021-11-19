module Server

open FSharp.Control.Tasks
open System
open System.IO
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Configuration

open Giraffe
open Saturn

open Shared
open Store

// Get configuration settings
let configurationBuilder =
    (new ConfigurationBuilder())
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional = true, reloadOnChange = true)
        .AddEnvironmentVariables()

let config = configurationBuilder.Build()

// Get region
let region =
    match (String.IsNullOrEmpty config.["Region"]) with
    | true -> "Local"
    | false -> config.["Region"]

// Get connection string
// First try configuration, otherwise check KeyVault
let connectionString =
    match (String.IsNullOrEmpty config.["ConnectionString"]) with
    | true -> (Secrets.getConnectionStringFromKeyVault config.["KeyVaultUrl"] config.["KeyVaultItemName"]).Result
    | false -> config.["ConnectionString"]

// Create store
let store = new Store (connectionString)

let cardsApi: ICardApi = {
    getCard = fun (id: string) -> async {
        let! card = store.getCardResponse id |> Async.AwaitTask
        match card with
        | Some c ->
            // Log event
            store.addLog "READ" id DateTimeOffset.UtcNow region |> ignore
            return Ok c
        | None -> return Error (404, { ErrorCode = "Not Found"; Message = sprintf "Could not find card with id %s" id })
    }
    comment = fun (id: string) (body: CommentRequest) -> async {
        if String.IsNullOrWhiteSpace body.Comment || String.length body.Comment > 1024 then
            return Error (400, { ErrorCode = "Bad Request"; Message = "Invalid comment" })
            else
                let currentTime = DateTimeOffset.UtcNow
                match! (store.addComment id body.Comment currentTime |> Async.AwaitTask) with
                | true -> return Ok { CommentLastModified = currentTime }
                | false -> return Error (404, { ErrorCode = "Not Found"; Message = sprintf "Could not find card with id %s" id })
    }
}

let getCard: string -> HttpHandler = fun (id: string) (next: HttpFunc) (ctx: HttpContext) ->
    task {
        let! card = cardsApi.getCard id
        match card with
        | Ok c -> return! json c next ctx
        | Error (status, e) ->
            ctx.SetStatusCode status
            return! json e next ctx
    }

let comment: string -> HttpHandler = fun (id: string) (next: HttpFunc) (ctx: HttpContext) ->
    task {
        let! commentRequest = Controller.getJson<CommentRequest> ctx
        let! commentResponse = cardsApi.comment id commentRequest
        return! json commentResponse next ctx
    }

let health: HttpHandler = text "Healthy"

let cardApiRouter = router {
    getf "/%s" getCard
    postf "/%s/comment" comment
}

let webApp = router {
    forward "/api/cards" cardApiRouter
    get "health" health
}

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
