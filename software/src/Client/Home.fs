module Home

open Feliz
open Feliz.Bulma

let view: ReactElement =
    Bulma.hero [
        hero.isFullHeight
        color.isPrimary
        prop.children [
            Bulma.heroBody [
                Bulma.container [
                    Bulma.column [
                        column.is6
                        column.isOffset3
                        prop.children [
                            Bulma.title [
                                text.hasTextCentered
                                prop.text "Home"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
