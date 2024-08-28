# 1. 프로젝트 정보

### 소개
트럼프 카드 게임 프로젝트<br>
블랙잭, 원카드(PVE), 원카드(PVP)<br>

클라이언트: Unity, C#을 사용하여 개발<br>
서버: JAVA, C#을 사용하여 개발(socket programming)<br>

개발 기간: 2023/10 ~ 2023/12<br>

### 개발 동기
간단한 카드게임 구현<br>

소켓 프로그래밍을 게임에 적용해보기 위함<br>

# 2. 프로젝트 구조
### 1. 구조도
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/5057156d-d11a-440a-b792-d19949219466" width="800" height="366"/>

### 2. 클래스 다이어그램
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/767a2a71-4a89-48e7-a625-ec0ecbc5fa5c"/>

# 3. 프로젝트 설명
### 1. 로비 화면
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/dbafd650-5b29-4fcd-8261-67006925b66f" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/a079b31d-2d76-4e39-9326-50c8a48f548b" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/7b739f38-648c-4572-b57a-ad071110dede" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/225879dc-99cd-4625-a68d-aacef1aa51b6" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/79a24c19-4371-40fa-ba75-b2bb0280fa30" width="320" height="180"/>

서버 열기: Open Server 버튼 클릭<br>
포트번호를 입력하여 서버를 열 수 있음(Super Peer)<br>

서버 접속: Connect To Server 버튼 클릭<br>
Super Peer의 IP주소, 포트번호를 통해 Super Peer에 접속할 수 있음(P2P)<br>
서버의 IP주소, 포트번호를 통해 서버에 접속할 수 있음(Client-Server)<br>

게임 선택: Start 버튼 클릭<br>
    
### 2. 게임 선택 화면
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/4b366ae8-3a5f-4c24-a5b1-48c826551384" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/6fec7842-be9a-43c8-bd20-392c48eb78d6" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/c64a5836-de09-47b5-a6db-82c0c8240f34" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/282017d9-cb1d-4f4f-92ec-6d0d76bce611" width="320" height="180"/>


블랙잭: BlackJack 버튼 클릭<br>

원카드(PVE): OneCard(PVE) 버튼 클릭<br>

원카드(PVP): OneCard(PVP) 버튼 클릭<br>
닉네임을 입력 후 체크 버튼을 누르면 서버 게임 방에 등록됨<br>
입력한 닉네임이 이미 사용 중일 경우 사용 불가<br>

닫기 버튼을 누르면 게임 방에서 나가짐<br>
    
4명이 모일 경우 게임 시작<br>

### 3. 블랙잭 게임 화면
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/ef139f02-9f84-4c92-9d20-27e57a30d96f" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/538433fa-ab6e-48c0-a27b-6cda28e821c6" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/6fa7ce17-6a91-4bf3-9798-967158157f69" width="320" height="180"/>
    
컴퓨터는 3장의 카드, 플레이어는 2장 또는 3장의 카드로 진행<br>
배팅 후 2장의 카드를 받고, 한장을 더 받을지 말지 선택 가능<br>
결과에 따라 칩을 얻거나 잃음<br>

### 4. 원카드 게임 화면
### PVE
https://github.com/user-attachments/assets/e3d5afe4-735d-4dfc-b9b9-0e7b39bd377f

<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/77bf5e6e-dbe2-41c6-a615-18a3424a8c91" width="320" height="180"/><br>

### PVP
https://github.com/user-attachments/assets/5a3727ad-0a38-4ae8-9be5-69b440a6615c


<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/8c22e107-0a7e-4d5a-bf8e-d9d69df664dd" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/c4e8f2e5-bf3b-4f02-a48c-0dd147f7b19b" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/28fe601f-9a95-4b97-920d-59b793f25c09" width="320" height="180"/>
<img src="https://github.com/slllldka/Trump_Card_Game/assets/121309640/db842826-6f9b-42da-869d-2721d694d24b" width="320" height="180"/>

원카드의 일반적인 규칙을 적용<br>
공격카드(A, 2, Joker), 수비카드(3), 특수카드(K, Q, J, 7) 로직 구현<br>

PVE의 경우 플레이어의 모든 행동은 클라이언트에서 처리됨<br>
PVP의 경우 플레이어의 모든 행동은 서버 또는 Super Peer를 통해 처리됨<br>

# 4. 문제해결
#### 서버와 클라이언트의 통신이 많이 발생할 때 프리징 현상이 나타나는 문제
처음에 서버를 구현할 때 하나의 클라이언트와의 상호작용을 하나의 스레드만 사용해서 구현하였는데,<br>
서버와 클라이언트의 통신이 매우 많이 일어나는 순간(게임이 시작될 때 모든 유저에게 7장의 카드를, 총 28장의 카드를 나누어 줄 때) 
프리징 현상이 발생함<br>

→ 서버와 하나의 클라이언트와의 상호작용을 정보 전송을 위한 스레드, 정보 수신을 위한 스레드, 정보 처리를 위한 스레드<br>
총 3개의 스레드를 사용하여 구현하는 것으로 변경하였고, 개선할 수 있었음<br>

# 5. 좋았던 점
처음에 서버를 JAVA를 사용해서 클라이언트와 분리하여 구현하였는데,<br>
이를 C#을 사용해서 클라이언트 내부로 옮기고,<br>
유저가 직접 로컬 서버를 열 수 있도록 구현해보았음<br>
