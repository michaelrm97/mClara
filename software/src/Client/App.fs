module App

open Elmish
open Elmish.Navigation
open Elmish.React
open Elmish.UrlParser

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

type Page =
    | Home
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
    | Some About -> { CurrentRoute = optRoute.Value; ActivePage = Page.About }, Cmd.none
    | Some Home -> { CurrentRoute = optRoute.Value; ActivePage = Page.Home }, Cmd.none
    | None -> (model, Navigation.modifyUrl "/")

let init (location : Route option) =
    setRoute location
        { ActivePage = Page.Home
          CurrentRoute = Route.Home }

let update (msg : Msg) (model : Model) =
    match model.ActivePage, msg with
    | Page.Home, _ -> { model with ActivePage = Page.Home }, Cmd.none
    | Page.About, _ -> { model with ActivePage = Page.About }, Cmd.none
    | Page.Card cardModel, CardMsg cardMsg ->
        let (cardModel, cardCmd) = Card.update cardMsg cardModel
        { model with ActivePage = Page.Card cardModel }, Cmd.map CardMsg cardCmd

let view (model : Model) (dispatch : Dispatch<Msg>) =
    match model.ActivePage with
    | Page.Home -> Home.view
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
