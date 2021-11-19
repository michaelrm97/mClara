namespace Shared

open System

// Error response
type ErrorResponse =
    { ErrorCode: string
      Message: string }

// Response sent to client on GET Request
type CardResponse = {
    Name: string
    Content: string
    Comment: string
    Reply: string
    CommentLastModified: Nullable<DateTimeOffset>
    ReplyLastModified: Nullable<DateTimeOffset>
}

// Request sent from client on comment
type CommentRequest = {
    Comment: string
}

// Response sent to client on comment
type CommentResponse = {
    CommentLastModified: DateTimeOffset
}

module Comment =
    let isValid (comment: string) =
        String.IsNullOrWhiteSpace comment |> not

type ICardApi =
    { getCard: string -> Async<Result<CardResponse, int * ErrorResponse>>
      comment: string -> CommentRequest -> Async<Result<CommentResponse, int * ErrorResponse>> }
