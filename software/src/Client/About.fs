module About

open Fable.React
open Fable.React.Props
open Feliz

let view: ReactElement =
    div [
        Style [
            Padding "10px"
            MaxWidth "600px"
            Margin "auto"
        ]            
    ] [
        h1 [ ] [
            str "About"
        ]
        div [
            Style [
                MarginTop "10px"
            ]
        ] [
            str "C.L.A.R.A stands for \"Christmas Light Array Running on Azure\". It is also a reference to Clara from The Nutcracker"
            br [ ]
            br [ ]
            str "The original Project Clara was conceived in 2020 as an collection of LED strips attached onto a wall. These would be controlled by an Arduino mounted on a veroboard which would also play music to accompany the light display. The Arduino would also connect to the internet to query a web service that allowed the lighting configuration and music to be controlled by a website."
            br [ ]
            br [ ]
            img [
                Alt "LED strips attached to a wall in the shape of a Christmas tree"
                Src "clara2020.jpg"
                Style [
                    Width "80%"
                ]
            ]
            br [ ]
            br [ ]
            div [ ] [
                img [
                    Src "/GitHub-Mark-64px.png"
                    Alt "GitHub"
                    Style [
                        Height "16px"
                        MarginRight "5px"
                        VerticalAlign "baseline"
                    ]
                ]
                span [ ] [
                    str "See more about this project on "
                    a [
                        Href "https://github.com/michaelrm97/clara"
                    ] [
                        str "GitHub"
                    ]
                ]
            ]
            br [ ]
            str "In 2021, Project Clara was reimagined as a series of Christmas cards consisting of a printed circuit board placed between two laser cut pieces of plywood. The front contains LEDs arranged in the shape of a Christmas tree whilst the back contains a QR code that links to a message hosted on this website."
            br [ ]
            br [ ]
            img [
                Alt "Front of Christmas Card"
                Src "clara2021.jpg"
                Style [
                    Width "80%"
                ]
            ]
            br [ ]
            br [ ]
            div [ ] [
                img [
                    Src "/GitHub-Mark-64px.png"
                    Alt "GitHub"
                    Style [
                        Height "16px"
                        MarginRight "5px"
                        VerticalAlign "baseline"
                    ]
                ]
                span [ ] [
                    str "See more about this project on "
                    a [
                        Href "https://github.com/michaelrm97/mClara"
                    ] [
                        str "GitHub"
                    ]
                ]
            ]
            br [ ]
            div [ ] [
                str "Icons made by "
                a [
                    Href "https://www.flaticon.com/authors/freepik"
                    Title "Freepik" 
                ] [
                    str "Freepik"
                ]
                str " from "
                a [
                    Href "https://www.flaticon.com/"
                    Title "Flaticon"
                ] [
                    str "www.flaticon.com"
                ]
            ]
        ]
    ]
