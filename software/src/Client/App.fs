module App

open Elmish
open Elmish.Navigation
open Elmish.React
open Elmish.UrlParser

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

[<RequireQualifiedAccess>]
module Cmd =
    open Browser.Dom

    let setTitle (value: string) : Cmd<'Msg> = 
        [ fun _ -> document.title <- value ]

type Page =
    | Home of Home.Model
    | About
    | Card of Card.Model

type Route =
    | Home
    | About
    | Card of string

type Model =
    { ActivePage : Page
      CurrentRoute: Route }

type Msg =
    | CardMsg of Card.Msg
    | HomeMsg of Home.Msg

let parser =
    oneOf [ map About (s "about")
            map Home (s "home")
            map Home top
            map Card str ]
    |> parsePath

let private setRoute (optRoute: Route option) (model: Model) =
    match optRoute with
    | Some (Card id) ->
        let (cardModel, cardCmd) = Card.init id
        { CurrentRoute = optRoute.Value; ActivePage = Page.Card cardModel }, Cmd.map CardMsg cardCmd
    | Some About -> { CurrentRoute = optRoute.Value; ActivePage = Page.About }, Cmd.setTitle "Project Clara | About"
    | Some Home ->
        let (homeModel, homeCmd) = Home.init
        { CurrentRoute = optRoute.Value; ActivePage = Page.Home homeModel }, Cmd.batch [
            Cmd.map HomeMsg homeCmd
            Cmd.setTitle "Project Clara | Home"
        ]
    | None -> model, Navigation.modifyUrl "/"

let init (location : Route option) =
    let (homeModel, _homeCmd) = Home.init
    setRoute location
        { ActivePage = Page.Home homeModel
          CurrentRoute = Route.Home }

let update (msg : Msg) (model : Model) =
    match model.ActivePage, msg with
    | Page.Home homeModel, HomeMsg homeMsg ->
        match homeMsg with
        | Home.Msg.GotoCard ->
            let (cardModel, cardCmd) = Card.init homeModel.Input
            { model with ActivePage = Page.Card cardModel }, Cmd.batch [
                Navigation.modifyUrl (sprintf "/%s" (homeModel.Input.ToUpper()))
                Cmd.map CardMsg cardCmd
                sprintf "Project Clara | %s" cardModel.Id |> Cmd.setTitle
            ]
        | _ ->
            let (homeModel, homeCmd) = Home.update homeMsg homeModel
            { model with ActivePage = Page.Home homeModel }, Cmd.map HomeMsg homeCmd
    | Page.About, _ ->
        { model with ActivePage = Page.About }, Cmd.none
    | Page.Card cardModel, CardMsg cardMsg ->
        let (cardModel, cardCmd) = Card.update cardMsg cardModel
        let title =
            match cardModel.Card with
            | None -> sprintf "Project Clara | %s" cardModel.Id
            | Some c -> sprintf "Project Clara | %s" c.Name
        { model with ActivePage = Page.Card cardModel }, Cmd.batch [
            Cmd.map CardMsg cardCmd
            Cmd.setTitle title
        ]
    | _ -> // Invalid message, just ignore
        model, Cmd.none

let view (model : Model) (dispatch : Dispatch<Msg>) =
    match model.ActivePage with
    | Page.Home homeModel -> Home.view homeModel (HomeMsg >> dispatch)
    | Page.About -> About.view
    | Page.Card cardModel -> Card.view cardModel (CardMsg >> dispatch)

// App
Program.mkProgram init update view
|> Program.toNavigable parser setRoute
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
