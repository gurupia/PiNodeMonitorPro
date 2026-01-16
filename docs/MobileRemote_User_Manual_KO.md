# Pi Node Mobile Remote Control - 사용자 매뉴얼

## 개요
스마트폰 브라우저에서 Pi Node를 원격으로 모니터링하고 제어할 수 있는 기능입니다.

---

## 1. 시작하기

### 1.1 서버 상태 확인
앱 실행 시 모바일 서버가 자동으로 시작됩니다.
- 메뉴 바에서 **Dashboard > Mobile Connection** 클릭
- QR 코드와 접속 주소가 표시됩니다

### 1.2 모바일 접속 (로컬/외부 공통)

#### A. 같은 WiFi 네트워크 (로컬)
1. PC와 스마트폰을 같은 WiFi에 연결
2. QR 코드를 스캔하거나 `192.168.x.x` 주소로 접속

#### B. 보안 터널 접속 (외부망/LTE) - [PRO 기능]
1. QR 접속 화면에서 **[보안터널] https://xxx.trycloudflare.com** 옵션 선택
2. 별도의 설정 없이 외부 어디서나 접속 가능
3. 자동으로 PIN 인증 및 대시보드 표시

---

## 2. 주요 기능

### 2.1 Dashboard 탭
| 항목 | 설명 |
|------|------|
| **State** | 노드 동기화 상태 (Synced = 정상) |
| **Age** | 마지막 블록 이후 경과 시간 |
| **Incoming** | 들어오는 연결 수 |
| **Outgoing** | 나가는 연결 수 |

### 2.2 Quick Actions
- 🔄 **Restart Pi Container**: Docker 컨테이너 재시작
- 🐋 **Restart Docker Desktop**: Docker 앱 재시작
- 🥧 **Restart Pi App**: Pi Network 앱 재시작

### 2.3 Remote 탭
- 실시간 화면 스트리밍
- 터치로 마우스 클릭/드래그 가능

---

## 3. 문제 해결

| 증상 | 원인 | 해결책 |
|------|------|--------|
| 접속 안됨 | 앱이 실행 중이지 않음 | 앱 실행 확인 |
| 접속 안됨 | 다른 네트워크 | 같은 WiFi 연결 확인 |
| 접속 안됨 | 방화벽 차단 | Windows 방화벽에서 포트 5000 허용 |
| PIN 오류 | PIN 변경됨 | QR 코드 다시 스캔 |
| 화면 안 나옴 | 스트리밍 지연 | 잠시 대기 또는 새로고침 |

---

## 4. 보안 참고사항

- **PIN 인증**: 모든 API 요청에 4자리 PIN 필요
- **LAN 전용**: 기본 설정은 로컬 네트워크 내에서만 접속 가능
- **외부 접속**: Cloudflare Tunnel을 활용하면 포트포워딩 없이도 안전하게 외부 접속이 가능합니다. (권장)

---

## 5. 시스템 요구사항

- **PC**: Windows 10/11, .NET Framework 4.8
- **모바일**: Chrome, Safari, Edge 등 최신 브라우저
- **네트워크**: 같은 WiFi/LAN 환경

---

*마지막 업데이트: 2026-01-16 (PRO Optimization 적용)*
