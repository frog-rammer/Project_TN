# .NET Core 카드 게임 서버

.NET Core 기반의 카드 게임 서버입니다. MariaDB와 Redis를 사용하여 게임 데이터를 관리합니다.

## 요구 사항

- .NET 9.0 SDK
- MariaDB 서버
- Redis 서버

## 설치 및 실행 방법

1. MariaDB 서버와 Redis 서버가 실행 중인지 확인합니다.
2. `appsettings.json` 파일에서 데이터베이스와 Redis 연결 문자열을 수정합니다.
3. 데이터베이스 마이그레이션을 실행합니다:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. 서버를 실행합니다:

```bash
dotnet run
```

## API 엔드포인트

### 인증

- POST /api/auth/register - 회원 가입
- POST /api/auth/login - 로그인
- POST /api/auth/logout - 로그아웃

### 카드

- GET /api/card/list - 모든 카드 목록 조회
- GET /api/card/user - 사용자의 카드 목록 조회
- POST /api/card/deck/update - 사용자 덱 업데이트

### 전투

- POST /api/battle/start - 전투 시작
- POST /api/battle/end - 전투 종료

## 인증 헤더

인증이 필요한 API 요청에는 다음 헤더를 포함해야 합니다:

```
X-User-Id: 사용자 ID
X-Session-Id: 로그인 시 받은 세션 ID
``` 