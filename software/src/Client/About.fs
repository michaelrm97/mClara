module About

open Elmish
open Fable.React
open Fable.React.Props
open Feliz
open Fulma


let view: ReactElement =
    div [ ] [
        Nav.view

        div [
            Style [
                Padding "10px"
            ]
        ] [
            h1 [
                Style [
                    FontSize 32
                    Margin "10px"
                ]
            ] [
                str "About"
            ]
        ]
    ]
