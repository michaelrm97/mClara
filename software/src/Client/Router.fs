module Router

open Elmish.UrlParser

//let inline (</>) a b = a + "/" + string b

type Route = Home | About | Card of string

let toHash (route: Route) =
    match route with
    | Home -> "home"
    | About -> "about"
    | Card id -> sprintf "cards/%s" id

let routeParser: Parser<Route -> Route, Route> =
    oneOf [ map Card (s "cards" </> str)
            map About (s "about")
            map Home (s "home") ]
