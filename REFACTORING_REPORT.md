# Refactoring Report: Wallet Service Extraction
# 리팩토링 보고서: 지갑 서비스 분리

**Date:** 2025-12-14  
**Author:** Pi Node Monitor AI Assistant  
**Target Component:** Wallet Integration Logic (지갑 연동 로직)

---

## 1. Summary (요약)
This refactoring separates the business logic related to the Pi Wallet from the UI layer (`Form1.cs`) into a dedicated service class (`WalletService.cs`). This adheres to the **Separation of Concerns (SoC)** principle, improving code readability and maintainability.

이번 리팩토링은 Pi 지갑과 관련된 비즈니스 로직을 UI 계층(`Form1.cs`)에서 분리하여 전용 서비스 클래스(`WalletService.cs`)로 이동시켰습니다. 이는 **관심사 분리(SoC)** 원칙을 준수하여 코드의 가독성과 유지보수성을 향상시킵니다.

---

## 2. Detailed Changes (상세 변경 사항)

### A. File I/O & Key Management (파일 입출력 및 키 관리)

*   **Before (이전)**:
    *   `Form1.cs` directly accessed `File.ReadAllText` and `File.WriteAllText` within button click events.
    *   `Form1.cs`가 버튼 클릭 이벤트 내에서 직접 파일을 읽고 썼습니다.
    ```csharp
    // Old Form1.cs
    btnSaveKey.Click += (s, e) => {
        _walletKey = txtPublicKey.Text.Trim();
        File.WriteAllText("wallet.dat", _walletKey); // Direct File Access
        // ...
    };
    ```

*   **After (이후)**:
    *   `WalletService` encapsulates all file operations. `Form1` simply calls `SaveKey`.
    *   `WalletService`가 모든 파일 작업을 캡슐화합니다. `Form1`은 단순히 `SaveKey`를 호출합니다.
    ```csharp
    // New Form1.cs
    btnSaveKey.Click += (s, e) => {
        _walletService.SaveKey(txtPublicKey.Text); // Clean Service Call
        // ...
    };
    ```

### B. Network API & Logic (네트워크 API 및 로직)

*   **Before (이전)**:
    *   `Form1.cs` contained raw `HttpClient` calls and complex `Regex` logic mixed with UI updates.
    *   `Form1.cs`에 `HttpClient` 호출과 복잡한 `Regex` 파싱 로직이 UI 업데이트 코드와 뒤섞여 있었습니다.
    ```csharp
    // Old UpdateWalletBalanceAsync
    using (var wClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) }) {
        var json = await wClient.GetStringAsync(url);
        var match = Regex.Match(json, "..."); // Complex Regex logic here
        if (match.Success) {
            lblBalance.Text = ... // UI update inside logic
        }
    }
    ```

*   **After (이후)**:
    *   `WalletService` returns a nullable decimal (`decimal?`). `Form1` only handles the result.
    *   `WalletService`는 `decimal?` 값을 반환하며, `Form1`은 결과 처리만 담당합니다.
    ```csharp
    // New UpdateWalletBalanceAsync
    var bal = await _walletService.GetBalanceAsync();
    if (bal.HasValue) {
        lblBalance.Text = $"Wallet: {bal.Value:N2} π";
    }
    ```

---

## 3. Benefits (이점)

1.  **Readability (가독성)**:
    *   `Form1.cs` size reduced. No more clutter with Regex patterns.
    *   `Form1.cs`의 크기가 줄어들고, 정규식 패턴 때문에 지저분했던 코드가 사라졌습니다.

2.  **Reusability (재사용성)**:
    *   `WalletService` can be used in other forms (e.g., Settings, Setup Wizard) without duplicating code.
    *   `WalletService`는 코드 중복 없이 다른 폼(설정, 설치 마법사 등)에서도 사용할 수 있습니다.

3.  **Maintainability (유지보수성)**:
    *   If Pi Network API changes, we only modify `WalletService.cs`. The UI remains untouched.
    *   Pi Network API가 변경되더라도 `WalletService.cs`만 수정하면 되며, UI 코드는 건드릴 필요가 없습니다.

---

## 4. Future Steps (향후 계획)
*   **Notification Service**: Extract the `NotifyIcon` and Sound logic into a `NotificationService` to further clean up `Form1`.
*   **알림 서비스**: `NotifyIcon`과 사운드 재생 로직을 `NotificationService`로 분리하여 `Form1`을 더욱 경량화할 예정입니다.
