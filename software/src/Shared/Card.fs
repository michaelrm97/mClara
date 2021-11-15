namespace Shared

open System

// Card format in COSMOS table
type Card = {
    Id: string;
    Name: string;
    Content: string;
    Comment: string;
    Reply: string;
    Created: DateTimeOffset;
    ContentLastModified: Nullable<DateTimeOffset>;
    CommentLastModified: Nullable<DateTimeOffset>;
    ReplyLastModified: Nullable<DateTimeOffset>;
}

// Response sent to client on GET Request
type CardResponse = {
    Name: string;
    Content: string;
    Comment: string;
    Reply: string;
    CommentLastModified: Nullable<DateTimeOffset>;
    ReplyLastModified: Nullable<DateTimeOffset>;
}

// Request sent from client on comment
type CommentRequest = {
    Comment: string;
}

// Response sent to client on comment
type CommentResponse = {
    CommentLastModified: DateTimeOffset;
}
