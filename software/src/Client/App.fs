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

type Model =
    { ActivePage : Page
      CurrentRoute : Router.Route }

type Msg =
    | CardMsg of Card.Msg

let private setRoute (optRoute: Router.Route option) (model: Model) =
    match optRoute with
    | Some (Router.Card id) ->
        let (cardModel, cardCmd) = Card.init id
        { CurrentRoute = optRoute.Value; ActivePage = Page.Card cardModel }, Cmd.map CardMsg cardCmd
    | Some Router.About -> { CurrentRoute = optRoute.Value; ActivePage = Page.About }, Cmd.none
    | Some Router.Home -> { CurrentRoute = optRoute.Value; ActivePage = Page.Home }, Cmd.none
    | None -> (model, Navigation.modifyUrl "/")

let init (location : Router.Route option) =
    setRoute location
        { ActivePage = Page.Home
          CurrentRoute = Router.Home }

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
|> Program.toNavigable (parseHash Router.routeParser) setRoute
#if DEBUG
|> Program.withConsoleTrace
#endif
|> Program.withReactSynchronous "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
