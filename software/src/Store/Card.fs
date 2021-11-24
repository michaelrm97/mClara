namespace Store

open System
open System.Text.Json.Serialization

// Card format in COSMOS table
type Card = {
    [<JsonPropertyName("id")>]
    Id: string
    Name: string
    DisplayName: string
    Content: string
    Comment: string
    Reply: string
    Created: DateTimeOffset
    ContentLastModified: DateTimeOffset
    CommentLastModified: Nullable<DateTimeOffset>
    ReplyLastModified: Nullable<DateTimeOffset>
}
