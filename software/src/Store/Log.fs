namespace Store

open System
open System.Text.Json.Serialization

// Log format in COSMOS table
type Log = {
    [<JsonPropertyName("id")>]
    Id: string;
    Operation: string;
    CardId: string;
    AccessTime: DateTimeOffset;
    Region: string;
}
