module Nav

open Fable.React.Props
open Fable.React
open Feliz
open Fulma

let view: ReactElement =
    div [
        Style [
            FontSize 20
        ]
    ] [
        Navbar.navbar [ Navbar.Color IsPrimary ] [
            Navbar.Brand.div [ ] [
                Navbar.Item.a [
                    Navbar.Item.Props [ Href "/" ]
                ] [
                    img [
                        Style [ PaddingRight "10px" ]
                        Src "favicon.png"
                    ]
                    str "Project Clara"
                ]
            ]
            Navbar.End.div [ ] [
                Navbar.Item.a [
                    Navbar.Item.Props [ Href "/about" ]
                ] [
                    str "About"
                ]
            ]
        ]
    ]
