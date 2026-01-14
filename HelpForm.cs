using System;
using System.Windows.Forms;

namespace PiNodeMonitorWinForm
{
    public class HelpForm : Form
    {
        public HelpForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Quick Guide & Help";
            this.Size = new System.Drawing.Size(500, 500);
            this.StartPosition = FormStartPosition.CenterParent;
            this.MinimumSize = new System.Drawing.Size(400, 400);

            TextBox txtHelp = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new System.Drawing.Font("Segoe UI", 10F),
                Padding = new Padding(10),
                Text = GetHelpText()
            };

            this.Controls.Add(txtHelp);
        }

        private string GetHelpText()
        {
            return @"[파이 노드 모니터 프로 퀵 가이드]

1. 시작하기
- 프로그램 실행 시 자동으로 노드를 감시합니다.
- 관리자 권한으로 실행하면 도커 제어가 더 확실합니다.

2. 보너스 기록
- 1분마다 자동으로 보너스 점수가 기록됩니다.
- [History] 버튼을 눌러 과거 기록을 확인할 수 있습니다.
- 데이터는 Data\Bonus_History.csv 파일에 저장됩니다.

3. 외부(모바일) 접속
- 하단의 'Public IP'와 'PIN'을 기억하세요.
- 포트 5000번이 공유기에서 포워딩되어야 밖에서 접속 가능합니다.
- 접속 주소: http://[공인IP]:5000

4. 텔레그램 봇 (추천)
- @BotFather를 통해 생성한 토큰을 설정에 입력하세요.
- 밖에서도 /status 명령어로 노드 보너스를 확인할 수 있습니다.

5. 자주 묻는 질문
- 보너스가 0인 경우: 설치 초기이거나 서버 평가 기간입니다.
- 가용성(Availability): 노드가 동기화된 시간의 비율입니다.

제작: Antigravity Architect";
        }
    }
}
