module Index

open Elmish
open Fable.Core
open Fetch
open Shared
open System

open Thoth.Fetch
open Thoth.Json

type Model = { Id: string; Card: CardResponse Option; Input: string }

type Msg =
    | GotCard of CardResponse
    | SetInput of string
    | Comment
    | Commented of CommentResponse

let asString (o: JsonValue): string = unbox o

let nullableStringDecoder: Decoder<string> =
    fun (path: string) (value: JsonValue) ->
        if value :? string then Ok (asString value)
        else if isNull value then Ok null
        else Error (path, BadPrimitive("a string", value))

let nullableDateTimeOffsetDecoder: Decoder<Nullable<DateTimeOffset>> =
    fun (path: string) (value: JsonValue) ->
        if value :? string then match DateTimeOffset.TryParse(asString value) with
                                | true, x -> Ok (Nullable x)
                                | _ -> Error (path, BadPrimitive("a datetimeoffset", value))
        else if isNull value then Ok (Nullable ())
        else Error (path, BadPrimitive("a datetimeoffset", value))

let cardResponseDecoder: Decoder<CardResponse> = Decode.object (fun get -> {
    Name = get.Required.Field "name" Decode.string
    Content = get.Required.Field "content" Decode.string
    Comment = get.Required.Field "comment" Decode.string
    Reply = get.Required.Field "reply" nullableStringDecoder
    CommentLastModified = get.Required.Field "commentLastModified" nullableDateTimeOffsetDecoder
    ReplyLastModified = get.Required.Field "replyLastModified" nullableDateTimeOffsetDecoder
})

let nullableDateTimeOffsetEncoder: Encoder<Nullable<DateTimeOffset>> =
    fun (value: Nullable<DateTimeOffset>) ->
        let t = value.Value.ToString()
        if value.HasValue then (Encode.string t) else null

let extraCoders = Extra.empty |> Extra.withCustom nullableDateTimeOffsetEncoder nullableDateTimeOffsetDecoder

let cardsApi: ICardApi = {
    getCard = fun (id: string) ->
        Fetch.get (sprintf "api/cards/%s" id, extra = extraCoders, decoder = cardResponseDecoder)
        |> Async.AwaitPromise
    comment = fun (id: string) (commentRequest: CommentRequest) ->
        Fetch.post (sprintf "api/cards/%s/comment" id, commentRequest, caseStrategy = CaseStrategy.CamelCase)
        |> Async.AwaitPromise
}    

let init () : Model * Cmd<Msg> =
    let model = { Id = "abcd"; Card = None; Input = "" }

    let cmd =
        Cmd.OfAsync.perform cardsApi.getCard model.Id GotCard

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GotCard response -> { model with Card = Some response }, Cmd.none
    | SetInput value -> { model with Input = value }, Cmd.none
    | Comment ->
        let req = { Comment = model.Input }
        let cmd =
            Cmd.OfAsync.perform (cardsApi.comment model.Id) req Commented

        model, cmd
    | Commented response ->
        { model with Card =
                        (match model.Card with
                        | None -> None
                        | Some m -> Some { m with CommentLastModified = Nullable response.CommentLastModified }) }, Cmd.none

open Feliz
open Feliz.Bulma

let name (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> m.Name

let content (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> m.Content

let comment (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> m.Comment

let reply (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> m.Reply
    
let commentLastModified (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> if m.CommentLastModified.HasValue then m.CommentLastModified.Value.ToLocalTime().ToString()
                else ""
    
let replyLastModified (model: Model) =
    match model.Card with
    | None -> ""
    | Some m -> if m.ReplyLastModified.HasValue then m.ReplyLastModified.Value.ToLocalTime().ToString()
                else ""

let navBrand =
    Bulma.navbarBrand.div [
        Bulma.navbarItem.a [
            prop.href "https://safe-stack.github.io/"
            navbarItem.isActive
            prop.children [
                Html.img [
                    prop.src "/favicon.png"
                    prop.alt "Logo"
                ]
            ]
        ]
    ]

let containerBox (model: Model) (dispatch: Msg -> unit) =
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
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.style [
            style.backgroundSize "cover"
            style.backgroundImageUrl "https://unsplash.it/1200/900?random"
            style.backgroundPosition "no-repeat center center fixed"
        ]
        prop.children [
            Bulma.heroHead [
                Bulma.navbar [
                    Bulma.container [ navBrand ]
                ]
            ]
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                prop.text "software"
                            ]
                            containerBox model dispatch
                        ]
                    ]
                ]
            ]
        ]
    ]
