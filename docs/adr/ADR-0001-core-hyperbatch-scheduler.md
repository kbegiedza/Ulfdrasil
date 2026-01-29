# Architectural Decision Record (ADR)

## Title
Hyperbatch scheduler with per-key micro-batching
*Date:* 2026-01-29
*Status:* Proposed
*Superseded by:* N/A
*Decision ID:* 0001-core-hyperbatch-scheduler
*Related ADRs:* N/A
*Approved by:* TBD

## Context

Ulfdrasil.Hyperbatch is intended to batch outgoing calls made by controllers and services. Batching must respect a
compatibility key (provider, operation, model, request options, tenant bucket) and avoid adding hidden latency to
incoming requests. The system must return per-item results by default and handle batch-level failures with retry or
bisect strategies.

## Decision

Introduce a Hyperbatch scheduler that multiplexes per-key micro-batch queues. Each queue flushes based on explicit
criteria (max batch size, max tokens, max wait time, backpressure, or caller deadline). Batch execution returns
per-item `Result<T>` values in request order. Batch-level failures are classified for retry or bisect behavior.

## Consequences

### Positive
- Clear batching boundaries controlled by the caller.
- Per-item results preserve error isolation and cancellation semantics.
- Compatibility-key queues enable safe batching across models/providers/tenants.
- Retry and bisect policies mitigate transient errors and bad inputs.

### Negative
- Additional complexity in scheduling and failure handling.
- Requires explicit configuration of batch limits and token counting.

## Implementation Steps

1. Implement scheduler, queue, and request metadata types in `Ulfdrasil.Hyperbatch`.
2. Add batch handler and scheduler abstractions for DI-friendly integration.
3. Add retry and bisect policies for batch-level failures.
4. Provide unit tests for size/time flush, bisect, and retry behavior.
5. Document glossary terms for Hyperbatch concepts.

## References

- docs/glossary.md

## Revision History

| Date       | Agent (Model)            | Description               | Approved by |
| ---------- | ------------------------ | ------------------------- | ----------- |
| 2026-01-29 | OpenAI (GPT-5.2)          | Initial draft of the ADR. | TBD         |
