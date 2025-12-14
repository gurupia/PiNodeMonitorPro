# 📡 Pi Node Monitor Pro: Multi-Node Control Center Development Log

## 1. Project Overview
**Objective**: To enable centralized monitoring and remote management of multiple Pi Nodes from a single "Control Center" PC without relying on external remote desktop tools (e.g., TeamViewer).

**Key Features**:
- **Multi-Node Dashboard**: Real-time status (Block, Sync State, In/Out) of 10+ nodes in a single grid view.
- **Smart Polling**: Asynchronous HTTP polling to minimize network load.
- **Remote Screen View (Eye)**: Real-time screen mirroring (1 FPS) for visual troubleshooting.
- **Remote Interaction (Hand)**: Click-to-Control functionality to perform recovery actions (e.g., restarting Docker) remotely.

---

## 2. System Architecture

The system follows a **P2P (Peer-to-Peer) Agent Architecture**. Each Pi Node Monitor instance acts as both a monitoring client and a server agent.

### **A. Server Side (Agent Node)**
- **Component**: `MobileServer.cs` (Embedded Kestrel Server)
- **Role**: Exposes internal node status and OS control capabilities via REST API.
- **Port**: 5000 (Default)
- **Security**: PIN-based authentication.

### **B. Client Side (Control Center)**
- **Component**: `MultiMonitorForm.cs` & `RemoteViewForm.cs`
- **Role**: Aggregates data from multiple agents and provides a UI for visualization and control.

---

## 3. Core Implementation Details

### 3.1. REST API Expansion (`MobileServer.cs`)
We extended the existing MobileServer to support remote screen capture and input simulation using Win32 APIs.

| Endpoint | Method | Params | Description |
|:--- | :--- | :--- | :--- |
| `/api/status` | GET | `pin` | Returns node metrics (Block, Sync Status, etc.) JSON. |
| `/api/screen` | GET | `pin` | Captures the primary screen and returns a JPEG image. |
| `/api/click` | GET | `pin`, `x`, `y` | Simulates a mouse left-click at the specified (x, y) coordinates. |

**Key Technologies**:
- **System.Drawing.Common**: Used for `Graphics.CopyFromScreen` to capture the desktop.
- **user32.dll (Win32 API)**: `SetCursorPos` and `mouse_event` used to simulate clicks.

### 3.2. Dashboard UI (`MultiMonitorForm.cs`)
The main dashboard displays a list of nodes.

- **Data Grid**: `DataGridView` tailored with specific columns (Alias, IP, State, Height).
- **Concurrency**: Uses `Task.WhenAll` to poll multiple nodes in parallel, preventing UI freeze.
- **UI Logic**:
    - **Dynamic Coloring**: Rows turn Green (Synced) or Pink/Gray (Issues) based on status.
    - **Layout Fix**: Resolved Z-Order overlapping issues between the Top Panel and the Grid using a container panel strategy (`pnlGridContainer`).
    - **Error Handling**: Implemented `IsDisposed` checks to prevent `InvalidOperationException` during async UI updates.

### 3.3. Remote View & Control (`RemoteViewForm.cs`)
A lightweight remote desktop implementation.

- **Streaming Strategy**: Polls `/api/screen` every 1000ms (1 FPS). This is sufficient for troubleshooting static error messages (e.g., "Docker stopped").
- **Coordinate Transformation**:
    - The `PictureBox` uses `SizeMode.Zoom`.
    - We implemented a coordinate mapping logic to translate the **User's Click (UI Coordinates)** on the resized image to the **Actual Resolution (Server Coordinates)**.
    - The translated coordinates are sent to `/api/click`.

---

## 4. Operational Workflow (User Scenario)

1.  **Deployment**: Execute `PiNodeMonitorWinForm.exe` on all Node PCs.
2.  **Registration**: On the Main PC, open "Multi-View" and add Node IPs (e.g., `192.168.1.5:5000`) and PINs.
3.  **Monitoring**: Watch the dashboard.
    - **Green Light**: Everything is fine.
    - **Gray/Pink Light**: Node halted or Docker issue.
4.  **Action (Troubleshooting)**:
    - Click **"📺 View"** button for the problematic node.
    - **Verify**: See the visual error message (e.g., "Check port 31400").
    - **Fix**: Click directly on the remote screen image (e.g., Click "Check Now" button or "Start Docker").
    - **Confirm**: Close the view and watch the status turn Green on the dashboard.

---

## 5. Technical Challenges & Solutions

### **Issue 1: UI Overlap (Grid hiding behind Header)**
- **Problem**: The DataGridView with `Dock=Fill` was rendering *behind* the Top Panel due to Z-Order precedence in WinForms.
- **Solution**: Encapsulated the Grid in a separate container Panel (`pnlGridContainer`) and explicitly set `pnlTop.SendToBack()` to ensure correct docking space reservation.

### **Issue 2: Async UI Thread Crashes**
- **Problem**: Closing the dashboard while a polling task was running caused `InvalidOperationException` (Invoke on disposed handle).
- **Solution**: Added strict `if (this.IsDisposed || !this.IsHandleCreated) return;` checks before any `this.Invoke` calls and stopped the Timer on `OnFormClosing`.

### **Issue 3: Remote Click Accuracy**
- **Problem**: Clicking on a zoomed-out image in the viewer sent wrong coordinates to the full-HD server.
- **Solution**: Implemented a ratio-based coordinate transformation logic that accounts for `PictureBox` pillar-boxing/letter-boxing.

---

## 6. Future Roadmap
- **Macros**: Add programmable buttons (e.g., "Restart Docker") that perform a sequence of clicks automatically.
- **Notifications**: Integrate Telegram alerts directly into the Dashboard (e.g., receive screenshot on Telegram on error).
- **Security**: Implement SSL/TLS for API communication if exposing over the internet.
