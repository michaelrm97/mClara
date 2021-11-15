module Server

open System
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks

open Giraffe
open Saturn

open Shared

let cardsApi: ICardApi = {
    getCard = fun (_id: string) -> async {
        printf "I am here"
        return {
            Name = "Name"
            Content = "Content"
            Comment = "Thanks"
            Reply = null
            CommentLastModified = Nullable DateTimeOffset.Now
            ReplyLastModified = Nullable ()
        }
    }
    comment = fun (_id: string) (_body: CommentRequest) -> async {
        return {
            CommentLastModified = DateTimeOffset.Now
        }
    }
}

let getCard: string -> HttpHandler = fun (id: string) (next: HttpFunc) (ctx: HttpContext) ->
    task {
        let! card = cardsApi.getCard id
        return! json card next ctx
    }

let comment: string -> HttpHandler = fun (id: string) (next: HttpFunc) (ctx: HttpContext) ->
    task {
        let! commentRequest = Controller.getJson<CommentRequest> ctx
        let! commentResponse = cardsApi.comment id commentRequest
        return! json commentResponse next ctx
    }

let cardApiRouter = router {
    getf "/%s" getCard
    postf "/%s/comment" comment
}

let webApp = router {
    forward "/api/cards" cardApiRouter
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
