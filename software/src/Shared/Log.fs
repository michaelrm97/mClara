namespace Shared

open System

type Operation =
    | READ
    | COMMENT

// Log format in COSMOS table
type Log = {
    Operation: Operation;
    CardId: string;
    AccessTime: DateTimeOffset;
    ClientTimeZoneOffset: int;
}
