# Pi Node Monitor - 크로스 플랫폼 개발 명세서 (Cross-Platform Design Spec)

## 1. 개요 (Overview)
본 문서는 기존 Windows 전용(WinForms) Pi Node 모니터링 도구를 **Windows, macOS, Linux** 모든 환경에서 실행 가능한 애플리케이션으로 재개발하기 위한 기술적 요구사항 및 설계를 정의합니다.

---

## 2. 권장 기술 스택 (Tech Stack Recommendation)

가장 추천하는 스택은 **"Electron + React"** 입니다.

### 2.1. 프레임워크: [Electron](https://www.electronjs.org/)
*   **선정 이유**: 
    1.  Pi Node 공식 앱도 Electron 기반이므로 기술적 동질성이 높음.
    2.  OS 네이티브 기능(트레이 아이콘, 알림, 파일 시스템) 접근이 용이함.
    3.  Node.js 런타임을 내장하여 Docker 커맨드 제어가 쉬움.
*   **대안**: Flutter (UI 성능은 좋으나 Docker 프로세스 제어가 상대적으로 까다로움), .NET MAUI (Linux 지원 미흡).

### 2.2. UI 라이브러리: [React](https://react.dev/) + [TailwindCSS](https://tailwindcss.com/)
*   **선정 이유**: 현대적이고 아름다운 대시보드 UI를 빠르게 구축 가능. 컴포넌트 재사용 용이.

### 2.3. 상태 관리: [Zustand](https://github.com/pmndrs/zustand)
*   **선정 이유**: Redux보다 훨씬 가볍고 직관적인 전역 상태 관리 (노드 데이터 실시간 갱신용).

---

## 3. 아키텍처 설계 (Architecture Design)

Electron의 **메인 프로세스(Backend)**와 **렌더러 프로세스(Frontend)** 분리 구조를 따릅니다.

### 3.1. Main Process (Node.js 백엔드 역할)
*   **역할**: OS 시스템과 직접 통신하는 무거운 작업 담당.
*   **기능**:
    1.  **Docker 명령어 실행**: `child_process.exec('docker exec ...')`
    2.  **API 폴링**: `axios`로 `localhost:31401/metrics` 주기적 호출.
    3.  **데이터 가공**: 수집된 Raw 데이터를 JSON으로 정제.
    4.  **IPC 통신**: 정제된 데이터를 프론트엔드(Renderer)로 전송 (`mainWindow.webContents.send`).
    5.  **웹서버 내장**: Express.js를 이용해 모바일 연동용 경량 서버 구동 (선택 사항).

### 3.2. Renderer Process (React 프론트엔드 역할)
*   **역할**: 사용자에게 데이터를 시각적으로 보여줌.
*   **기능**:
    1.  **대시보드 UI**: Incoming/Outgoing, Block Height, Sync State 표시.
    2.  **설정 화면**: 알림 설정, Docker 컨테이너 이름 변경 등.
    3.  **IPC 수신**: 메인 프로세스가 보내주는 데이터를 `window.electron.onUpdate(...)`로 받아 화면 갱신.

---

## 4. 핵심 기능 명세 (Feature Specifications)

### 4.1. Docker 제어 (Cross-Platform 호환성)
Windows와 Unix 계열(Mac/Linux)의 Docker 경로 및 권한 문제를 해결해야 합니다.
*   **Windows**: Docker Desktop 백엔드 (WSL2) 통신.
*   **macOS/Linux**: `/var/run/docker.sock` 소켓 통신 또는 `docker` CLI 명령어 경로(`/usr/local/bin/docker`) 자동 탐지.

### 4.2. 모바일 연동 (Mobile Connect)
*   **서버**: Node.js의 `Fastify` 또는 `Express` 모듈을 사용하여 5000번 포트 웹서버 구동.
*   **네트워크**: `internal-ip` 패키지를 사용하여 현재 PC의 내부 IP 자동 감지 및 QR 코드 생성.

### 4.3. 알림 시스템 (Notifications)
*   **데스크톱 알림**: Electron의 `Notification` API 사용 (OS 네이티브 알림).
*   **메신저 알림**: Telegram Bot API 연동 (Node.js `node-telegram-bot-api` 패키지 사용).

---

## 5. 프로젝트 구조 예시 (Project Structure)

```
my-pi-monitor/
├── package.json
├── main/                 # [Main Process]
│   ├── index.ts          # 진입점 (앱 실행)
│   ├── docker.ts         # Docker 명령어 래퍼 함수
│   ├── apiScanner.ts     # 로컬 API 호출 로직
│   └── mobileServer.ts   # 모바일 연동 웹서버
├── renderer/             # [Renderer Process - React]
│   ├── src/
│   │   ├── components/   # Dashboard, Settings, GaugeChart
│   │   ├── hooks/        # useNodeData.ts
│   │   └── App.tsx
│   └── index.html
└── resources/            # 아이콘 및 이미지 에셋
```

---

## 6. 결론 (Conclusion)
Electron 기반으로 전환 시, **하나의 코드베이스**로 Windows, Mac, Linux 사용자 모두에게 동일한 경험을 제공할 수 있습니다. 
특히 JavaScript 생태계의 풍부한 라이브러리(차트, API 통신, 시스템 제어)를 활용할 수 있어 개발 속도와 UI 퀄리티가 대폭 향상될 것입니다.
