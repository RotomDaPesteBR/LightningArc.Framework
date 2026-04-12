## Release 1.0.0

| Rule ID | Category | Severity | Notes                                    |
|---------|----------|--------|------------------------------------------|
| LARC001 | Usage    | Warning  | Result value unsafe access detection     |
| LARC002 | Usage    | Warning  | Result error unsafe access detection     |
| LARC003 | Usage    | Warning  | Result discarded detection               |
| LARC010 | Usage    | Warning  | Implicit string to ValueObject conversion|
| LARC011 | Usage    | Warning  | Null ValueObject ToString conversion     |
| LARC012 | Usage    | Warning  | ValueObject creation discarded detection |
| LARC013 | Usage    | Warning  | Prevent Result.Success(null) for non-nullable types |
| LARC014 | Design   | Warning  | Ensure ValueObjects are record types     |
| LARC015 | Usage    | Warning  | Result error shadowing detection         |
| LARC020 | Reliability | Warning | Sync connection in async method usage    |
| LARC021 | Reliability | Warning | Missing transaction in repository calls  |
| LARC022 | Reliability | Info    | HostedService StartAsync performs no work |
| LARC023 | Reliability | Warning | Direct DbConnection instantiation detection |
| LARC030 | Reliability | Warning | ReleaseConnection null argument usage  |
| LARC031 | Usage    | Warning  | Missing .ToEndpointResult() in Minimal APIs |
| LARC032 | Usage    | Warning  | Redundant try-catch for Result mapping   |
