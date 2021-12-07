module Nav

open Fable.React.Props
open Fable.React
open Feliz
open Fulma

let view: ReactElement =
    div [ ] [
        Navbar.navbar [
            Navbar.Color IsPrimary
        ] [
            Navbar.Brand.div [ ] [
                Navbar.Item.a [
                    Navbar.Item.Props [ Href "/" ]
                ] [
                    img [
                        Style [ PaddingRight "10px" ]
                        Src "favicon.png"
                    ]
                    div [
                        Style [
                            FontSize "24px"
                        ]
                    ] [
                        str "Project Clara"
                    ]
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
