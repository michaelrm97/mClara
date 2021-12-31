module Hedgehog

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
            str "The Hedgehog"
        ]
        div [
            Style [
                MarginTop "10px"
            ]
        ] [
            str "The hedgehog steps out into the sun"
            br [ ]
            str "His only source of warmth"
            br [ ]
            str "The piercing rays of the summer heat"
            br [ ]
            str "Still less sharp than the quills of his peers"
            br [ ]
            br [ ]
            
            str "Step out of the spotlight they say"
            br [ ]
            str "If you cannot stand getting burnt"
            br [ ]
            str "But only knives await"
            br [ ]
            str "In the cold bosom of the cave"
            br [ ]
            br [ ]
            
            str "A single spine in a sea of isolation"
            br [ ]
            str "A millions spine in a sea of loneliness"
            br [ ]
            str "At an arms length away"
            br [ ]
            str "Only radiation reaches one another"
            br [ ]
            br [ ]
            
            str "Don't rely on others for your own happiness"
        ]
    ]
