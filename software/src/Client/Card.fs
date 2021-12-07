module Card

open Elmish
open Fable.Core
open System
open Thoth.Fetch
open Thoth.Json

open PendingOption
open Shared

type Model = { Id: string; Card: CardResponse PendingOption; Input: string Option }

type Msg =
    | GotCard of Result<CardResponse, int * ErrorResponse>
    | SetInput of string
    | EditComment
    | DeleteComment
    | Reset
    | Comment
    | Commented of Result<CommentResponse, int * ErrorResponse> * string

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
    let model = { Id = id; Card = Pending; Input = Option.Some "" }

    let cmd =
        Cmd.OfAsync.perform cardsApi.getCard model.Id GotCard

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | GotCard response ->
        (match response with
        | Ok c ->
            { model with Card = Some c
                         Input =
                            if String.IsNullOrEmpty c.Comment then
                                Option.Some ""
                            else Option.None }
        | Error (status, e) ->
            printfn "Received error response: %d %s" status e.Message
            { model with Card = None }), Cmd.none
    | SetInput value ->
        { model with Input = Option.Some value }, Cmd.none
    | EditComment ->
        { model with Input =
                        match model.Card with
                        | Some c -> if String.IsNullOrEmpty c.Comment then Option.Some "" else Option.Some c.Comment
                        | _ -> Option.None}, Cmd.none
    | DeleteComment ->
        let req = { Comment = null }
        let cmd =
            Cmd.OfAsync.perform (
                fun r -> async {
                    let! result = cardsApi.comment model.Id r
                    return (result, null)
                }) req Commented

        model, cmd
    | Reset ->
        { model with Input =
                        match model.Card with
                        | Some c ->
                            printfn "%s" c.Comment
                            if String.IsNullOrEmpty c.Comment then
                                Option.Some ""
                            else Option.None
                        | _ -> Option.None }, Cmd.none
    | Comment ->
        match model.Input with
        | Option.Some i ->
            let req = { Comment = i }
            let cmd =
                Cmd.OfAsync.perform (
                    fun r -> async {
                        let! result = cardsApi.comment model.Id r
                        return (result, i)
                    }) req Commented

            model, cmd
        | Option.None -> model, Cmd.none
    | Commented (response, comment) ->
        (match response with
        | Ok c ->
            { model with Card =
                            (match model.Card with
                            | Some m -> Some {
                                m with CommentLastModified = Nullable c.CommentLastModified
                                       Comment = comment }
                            | _ -> model.Card)
                         Input = if String.IsNullOrEmpty comment then Option.Some "" else Option.None
            }
        | Error (status, e) ->
            printfn "Received error response: %d %s" status e.Message
            model), Cmd.none

open Fable.DateFunctions
open Fable.React
open Fable.React.Props
open Fulma

let commentSection (model: Model) (dispatch: Msg -> unit) =
    div [
        Style [
            PaddingTop "10px"
        ]
    ] [
        match model.Card with
        | Some c ->
            match model.Input with
            | Option.Some i ->
                Textarea.textarea [
                    Textarea.Option.Placeholder "Write a response..."
                    Textarea.Option.Value i
                    Textarea.Option.Props [
                        Style [
                            VerticalAlign "text-bottom"
                            MarginRight "10px"
                            Height "90px"
                        ]
                    ]
                    Textarea.Option.OnChange (fun x -> SetInput x.Value |> dispatch )
                ] [ ]
                Button.button [
                    Button.Color IsPrimary
                    Button.Props [
                        Style [
                            Float FloatOptions.Right
                            MarginTop "10px"
                        ]
                    ]
                    Button.OnClick (fun _ -> Comment |> dispatch)
                    Button.Disabled (String.IsNullOrEmpty i || i = c.Comment)
                ] [
                    str "Send"
                ]
                Button.button [
                    Button.Color IsDanger
                    Button.Props [
                        Style [
                            Float FloatOptions.Right
                            MarginTop "10px"
                            MarginRight "10px"
                        ]
                    ]
                    Button.OnClick (fun _ -> Reset |> dispatch)
                ] [
                    str "Reset"
                ]
            | Option.None ->
                div [
                    Style [
                        Padding "10px"
                        BorderStyle "solid"
                        BorderWidth "1px"
                    ]
                ] [
                    div [ ] [
                        i [
                            Style [
                                Float FloatOptions.Left
                            ]
                        ] [
                            sprintf "%s wrote on %s" c.DisplayName
                                (if c.CommentLastModified.HasValue then
                                    ExternalDateFns.formatWithStr (c.CommentLastModified.Value.ToLocalTime()) "yyyy-MM-dd hh:mm"
                                else "")
                            |> str
                        ]
                        i [
                            Style [
                                Float FloatOptions.Right
                                MarginLeft "10px"
                                Cursor "pointer"
                            ]
                            Class "fas fa-trash"
                            OnClick (fun _ -> DeleteComment |> dispatch)
                        ] [ ]
                        i [
                            Style [
                                Float FloatOptions.Right
                                Cursor "pointer"
                            ]
                            Class "fas fa-edit"
                            OnClick (fun _ -> EditComment |> dispatch)
                        ] [ ]
                    ]
                    br [ ]
                    str c.Comment
                ]
        | _ -> div [ ] [ ]
    ]

let view (model: Model) (dispatch: Msg -> unit) =
    div [
        Style [
            Padding "10px"
            MaxWidth "600px"
            Margin "auto"
        ]
    ] [
        match model.Card with
        | None ->
            h1 [ ] [
                str "Card not found"
            ]
        | Pending ->
            h1 [ ] [
                str "Loading..."
            ]
        | Some c ->
            div [ ] [
                h1 [ ] [
                    str c.Name
                ]
                str c.Content
                commentSection model dispatch
            ]
    ]
