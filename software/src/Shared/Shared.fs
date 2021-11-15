namespace Shared

open System

module Comment =
    let isValid (comment: string) =
        String.IsNullOrWhiteSpace comment |> not

type ICardApi =
    { getCard: string -> Async<CardResponse>
      comment: string -> CommentRequest -> Async<CommentResponse> }
