# Glossary

- **Hyperbatch**: A batching mechanism that groups compatible requests into micro-batches to reduce downstream calls.
- **BatchKey**: A compatibility key that defines which requests can be batched together (e.g., provider, operation, model, tenant).
- **MicroBatchQueue**: An internal per-key queue that accumulates requests until flush criteria are met.
- **HyperbatchScheduler**: The component that multiplexes micro-batch queues and triggers batch execution.
- **HyperbatchBatchHandler**: A handler that executes a batch and returns per-item results.
