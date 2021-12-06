module Card

open Elmish
open Fable.Core
open Fable.React
open System
open Thoth.Fetch
open Thoth.Json

open Shared
open Nav

type Model = { Id: string; Card: CardResponse Option; Input: string }

type Msg =
    | GotCard of Result<CardResponse, int * ErrorResponse>
    | SetInput of string
    | Comment
    | Commented of Result<CommentResponse, int * ErrorResponse>

let private asString (o: JsonValue): string = unbox o

let private nullableStringDecoder: Decoder<string> =
    fun (path: string) (value: JsonValue) ->
        if value :? string then Ok (asString value)
        else if isNull value then Ok null
        else Error (path, BadPrimitive("a string", value))

let private nullableDateTimeOffsetDecoder: Decoder<Nullable<DateTimeOffset>> =
    fun (path: string) (value: JsonValue) ->
        if value :? string then match DateTimeOffset.TryParse(asString value) with
                                | true, x -> Ok (Nullable x)
                                | _ -> Error (path, BadPrimitive("a datetimeoffset", value))
        else if isNull value then Ok (Nullable ())
        else Error (path, BadPrimitive("a datetimeoffset", value))

let private cardResponseDecoder: Decoder<CardResponse> = Decode.object (fun get -> {
    Name = get.Required.Field "name" Decode.string
    DisplayName = get.Required.Field "displayName" nullableStringDecoder
    Content = get.Required.Field "content" Decode.string
    Comment = get.Required.Field "comment" nullableStringDecoder
    Reply = get.Required.Field "reply" nullableStringDecoder
    CommentLastModified = get.Required.Field "commentLastModified" nullableDateTimeOffsetDecoder
    ReplyLastModified = get.Required.Field "replyLastModified" nullableDateTimeOffsetDecoder
})

let private nullableDateTimeOffsetEncoder: Encoder<Nullable<DateTimeOffset>> =
    fun (value: Nullable<DateTimeOffset>) ->
        let t = value.Value.ToString()
        if value.HasValue then (Encode.string t) else null

let private extraCoders = Extra.empty |> Extra.withCustom nullableDateTimeOffsetEncoder nullableDateTimeOffsetDecoder

let private handleResult (result: Result<'T, FetchError>): Async<Result<'T, int * ErrorResponse>> = async {
    match result with
    | Ok c -> return Ok c
    | Error e ->
        match e with
        | FetchFailed response ->
            let! bodyText = response.text () |> Async.AwaitPromise
            match Decode.fromString (Decode.Auto.generateDecoder<ErrorResponse> CaseStrategy.CamelCase) bodyText with
            | Ok e -> return Error (response.Status, e)
            | Error s -> return Error (response.Status, { ErrorCode = "Deserialization Error"; Message = s })
        | PreparingRequestFailed exn -> return Error(0, { ErrorCode = "Preparing Request Failed"; Message = exn.Message })
        | NetworkError exn -> return Error(0, { ErrorCode = "Network Error"; Message = exn.Message })
        | DecodingFailed s -> return Error (0, { ErrorCode = "Decoding Failed"; Message = s })
}

let private cardsApi: ICardApi = {
    getCard = fun (id: string) -> async {
        let! result = Fetch.tryGet (sprintf "/api/cards/%s" id, extra = extraCoders, decoder = cardResponseDecoder) |> Async.AwaitPromise
        return! handleResult result
    }

    comment = fun (id: string) (commentRequest: CommentRequest) -> async {
        let! result = Fetch.tryPost (sprintf "/api/cards/%s/comment" id, commentRequest, caseStrategy = CaseStrategy.CamelCase) |> Async.AwaitPromise
        return! handleResult result
    }
}    

let init (id: string) : Model * Cmd<Msg> =
    let model = { Id = id; Card = None; Input = "" }

    let cmd =
        Cmd.OfAsync.perform cardsApi.getCard model.Id GotCard

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GotCard response ->
        (match response with
        | Ok c -> { model with Card = Some c }
        | Error (status, e) ->
            printfn "Received error response: %d %s" status e.Message
            { model with Card = None }), Cmd.none
    | SetInput value ->
        { model with Input = value }, Cmd.none
    | Comment ->
        printfn "You are here"
        let req = { Comment = model.Input }
        let cmd =
            Cmd.OfAsync.perform (cardsApi.comment model.Id) req Commented

        model, cmd
    | Commented response ->
        (match response with
        | Ok c ->
            { model with Card =
                            (match model.Card with
                            | None -> None
                            | Some m -> Some {
                                m with CommentLastModified = Nullable c.CommentLastModified; Comment = model.Input }) }
        | Error (status, e) ->
            printfn "Received error response: %d %s" status e.Message
            model), Cmd.none

open Feliz
open Feliz.Bulma
open Fulma

let private name (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> m.Name

let private content (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> m.Content

let private comment (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> m.Comment

let private reply (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> m.Reply
    
let private commentLastModified (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> if m.CommentLastModified.HasValue then m.CommentLastModified.Value.ToLocalTime().ToString()
                else ""
    
let private replyLastModified (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> if m.ReplyLastModified.HasValue then m.ReplyLastModified.Value.ToLocalTime().ToString()
                else ""

let private containerBox (model: Model) (dispatch: Msg -> unit) =
    Bulma.box [
        Bulma.content [
            prop.text (sprintf "Name: %s" (name model))
        ]
        Bulma.content [
            prop.text (sprintf "Content: %s" (content model))
        ]
        Bulma.content [
            prop.text (sprintf "Comment: %s" (comment model))
        ]
        Bulma.content [
            prop.text (sprintf "Reply: %s" (reply model))
        ]
        Bulma.content [
            prop.text (sprintf "Comment last modified: %s" (commentLastModified model))
        ]
        Bulma.content [
            prop.text (sprintf "Reply last modified: %s" (replyLastModified model))
        ]
        Bulma.field.div [
            field.isGrouped
            prop.children [
                Bulma.control.p [
                    control.isExpanded
                    prop.children [
                        Bulma.input.text [
                            prop.value model.Input
                            prop.placeholder "Add a comment"
                            prop.onChange (fun x -> SetInput x |> dispatch)
                        ]
                    ]
                ]
                Bulma.control.p [
                    Bulma.button.a [
                        color.isPrimary
                        prop.disabled (Comment.isValid model.Input |> not)
                        prop.onClick (fun _ -> dispatch Comment)
                        prop.text "Add"
                    ]
                ]
            ]
        ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    div [ ] [
        Nav.view
        Bulma.heroBody [
            Bulma.container [
                Bulma.column [
                    column.is6
                    column.isOffset3
                    prop.children [
                        Bulma.title [
                            text.hasTextCentered
                            prop.text "Project Clara"
                        ]
                        containerBox model dispatch
                    ]
                ]
            ]
        ]
    ]
