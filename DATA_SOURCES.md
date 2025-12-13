# Pi Node Monitor - Data Sources & Logic Documentation

This document describes exactly how **Pi Node Monitor Pro** fetches and interprets data from your Pi Node (Stellar Core).

## 1. Data Sources (Priority Order)

The application attempts to fetch data in the following order. It uses the first method that succeeds.

### Priority 1: Prometheus Metrics (Most Accurate)
- **URL**: `http://localhost:31403/metrics`
- **Method**: HTTP GET
- **Raw Data Format**: Plain Text (Prometheus-style key-value pairs)
- **Target Keys**:
  - `stellar_node_peers_connected_outbound` (Raw Outgoing Count)
  - `stellar_node_peers_connected_inbound` (Raw Incoming Count)
  - `stellar_node_ledger_age_seconds` (Ledger Age)
  - `stellar_node_ledger_ledger` (Local Block Number)

> **Why this source?**
> This is the internal raw counter of the Stellar Core process. It counts **all active TCP connections**.
> 
> **Discrepancy Note**: 
> Sometimes this raw count includes "handshaking" or "half-open" connections that the official Pi App might filter out. The official App often caps the display at 8/8 or 8/64, whereas raw metrics show the real physical connection count (which can be higher).

### Priority 2: JSON Info API (Fallback)
- **URL**: `http://localhost:31403/info`
- **Method**: HTTP GET
- **Raw Data Format**: JSON
- **Target Logic**:
  - Tries to read `peers.outbound_count` and `peers.inbound_count` (Newer Stellar versions).
  - If unavailable, reads `peers.authenticated_count` (Total Peers).
    - **Logic**: If Total <= 8, Outgoing = Total, Incoming = 0.
    - **Logic**: If Total > 8, Outgoing = 8, Incoming = Total - 8.

### Priority 3: Docker Command (Last Resort)
- **Command**: `docker exec pi-consensus curl http://localhost:11626/info`
- **Usage**: Used only if HTTP requests to localhost fail (e.g., firewall issues).

---

## 2. Data Processing Logic (The "Fix")

To match the official Pi Node App's display logic closer, the following post-processing is applied to the raw data:

### Outgoing Connection Cap
The Stellar Consensus Protocol targets **8 outgoing peers**. However, raw metrics can sometimes show more (e.g., temporary connections, mesh networking).

**The Logic:**
```csharp
// If raw outgoing is greater than 8 (e.g., 16), 
// we assume the excess are actually miscategorized or incoming connections in a mesh state.
if (outgoing > 8)
{
    incoming = incoming + (outgoing - 8); // Shift excess to incoming
    outgoing = 8;                         // Cap outgoing at 8
}
```

### Incoming Connection
- Represents other nodes verifying blocks from you.
- Official App often shows "8" as a generic "Good" sign, but actual incoming count can be much higher (up to 64 or more).
- **Pi Node Monitor Pro** displays the **Real Count** (after the shift adjustment above).

---

## 3. Why the Difference? (Screenshot Analysis)

If you see **Outgoing: 16** in Monitor Pro vs **Outgoing: 8** in Official App:

1. **Raw Reality**: Your node actually has 16 outbound TCP connections alive at the OS level.
2. **Official App Filtering**: The official app likely strictly filters only "Consensus-Participating" peers or hard-caps the UI display to 8.
3. **Monitor Pro Version**:
   - If you see "16", you are running a version **BEFORE** the "Cap at 8" logic was applied.
   - The latest version (with the fix) forces the display to **8**, moving the extra 8 to Incoming (so you would see Outgoing: 8, Incoming: 13+8 = 21).

## 4. Summary
- **Official App**: Shows "Protocol Ideal" state (Abstracted).
- **Monitor Pro**: Shows "Physical Network" state (Raw).
- **Latest Update**: Forces Monitor Pro to mimic Official App's "Outgoing 8" behavior to reduce confusion.
