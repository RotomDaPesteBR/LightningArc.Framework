## Release 1.0.0

| Rule ID | Category    | Severity | Notes                                                        |
|---------|-------------|----------|---------------------------------------------------------------|
| LARC001 | Usage       | Warning  | Unsafe access to Result.Value without IsSuccess check         |
| LARC002 | Usage       | Warning  | Unsafe access to Result.Error without IsFailure check         |
| LARC003 | Usage       | Info     | Result return value discarded                                 |
| LARC004 | Usage       | Warning  | Result.Success(null) for non-nullable reference types (renumbered from LARC013) |
| LARC005 | Usage       | Warning  | Result error shadowing detection (renumbered from LARC015)    |
| LARC006 | Usage       | Warning  | Redundant try-catch for Result mapping (renumbered from LARC032) |
| LARC007 | Usage       | Info     | ResultAggregator built with no Check/Ensure/When/CheckAll/WhenAll calls |
| LARC008 | Usage       | Info     | Error accumulated via '+=' inside a loop — suggests Error.Aggregate(...) or ResultAggregator |
| LARC020 | Usage       | Warning  | Implicit string → ValueObject conversion (renumbered from LARC010) |
| LARC021 | Usage       | Warning  | Null ValueObject → string conversion (renumbered from LARC011) |
| LARC022 | Usage       | Info     | ValueObject creation result discarded (renumbered from LARC012) |
| LARC023 | Design      | Warning  | ValueObject should be a record type (renumbered from LARC014) |
| LARC040 | Reliability | Warning  | Sync connection in async method usage (renumbered from LARC020) |
| LARC041 | Reliability | Info     | Missing transaction in repository call (renumbered from LARC021) |
| LARC042 | Reliability | Warning  | Direct DbConnection instantiation in repository (renumbered from LARC023) |
| LARC043 | Reliability | Warning  | ReleaseConnection called with null argument (renumbered from LARC030) |
| LARC044 | Reliability | Warning  | Missing ReleaseConnection after GetConnection/GetConnectionAsync |
| LARC060 | Usage       | Warning  | Missing .ToEndpointResult() in Minimal APIs (renumbered from LARC031) |
| LARC080 | Reliability | Info     | HostedService StartAsync performs no work (renumbered from LARC022) |
