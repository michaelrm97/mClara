module Home

open Elmish
open Fable.React.Props
open Fable.React
open Feliz
open Fulma

type Model = { Input: string }

type Msg =
    | SetInput of string
    | GotoCard

let init: Model * Cmd<Msg> =
    let model = { Input = "" }

    let cmd = Cmd.none

    model, cmd

let update (msg: Msg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | SetInput value ->
        { model with Input = value }, Cmd.none
    | GotoCard -> // Should have been handled above, just ignore
        model, Cmd.none

let view (model: Model) (dispatch: Msg -> unit) : ReactElement =
    div [
        Style [
            Padding "10px"
            FontSize 16
            MaxWidth "600px"
            Margin "auto"
        ]
    ] [
        h1 [
            Style [ ]
        ] [
            str "Welcome to Project Clara"
        ]
        section [
            Style [
                FontSize "16px"
            ]
        ] [
            str "Scan the QR code at the back of your card or enter the 5 character code below:"
            form [
                Style [
                    MarginTop "10px"
                ]
            ] [
                Input.text [
                    Input.Placeholder "ABCDE"
                    Input.Props [
                        MaxLength 5.0
                        Style [
                            Width "100px"
                            MarginRight "10px" 
                        ]
                    ]
                    Input.OnChange (fun x -> SetInput x.Value |> dispatch )
                ]
                Button.button [
                    Button.Color IsPrimary
                    Button.Disabled (model.Input.Length = 5 |> not)
                    Button.OnClick (fun _ -> GotoCard |> dispatch)
                ] [
                    str "Go"
                ]
            ]
        ]
    ]
