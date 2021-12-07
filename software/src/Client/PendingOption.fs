module PendingOption

type PendingOption<'T> =
| Some of 'T
| None
| Pending