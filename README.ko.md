# GravitaMaze (중력 미로)

[![en](https://img.shields.io/badge/lang-en-red.svg)](https://github.com/salt26/gravita-maze/blob/main/README.md)
[![ko](https://img.shields.io/badge/lang-ko-blue.svg)](https://github.com/salt26/gravita-maze/blob/main/README.ko.md)
[![Version badge](https://img.shields.io/badge/Version-1.4.1-purple.svg)](https://github.com/salt26/gravita-maze/releases/tag/v1.4.1)  
[![Licence](https://img.shields.io/badge/License-MIT-green.svg)](./LICENSE)  
![Unity](https://img.shields.io/badge/unity-%23000000.svg?style=for-the-badge&logo=unity&logoColor=white)  
![GitHub Actions](https://img.shields.io/badge/github%20actions-%232671E5.svg?style=for-the-badge&logo=githubactions&logoColor=white)

## 게임 소개

* 퍼즐 게임
  * 중력을 조작해서 공을 미로에서 탈출시켜 보세요!
* 영어 및 한국어 지원!

### 다운로드

#### [v1.4.1 for Android, Windows, and macOS](https://github.com/salt26/gravita-maze/releases/tag/v1.4.1)

<details>
<summary>여기를 클릭하면 과거 업데이트 내용을 표시하거나 숨길 수 있습니다.</summary>

### 업데이트 (v1.3.1 -> v1.4.0)

#### 사운드

* 다양한 효과음 추가
* Editor scene에 배경음 추가

#### 첫 플레이 유저를 위한 개선 사항

* 앱을 설치한 후 튜토리얼을 바로 플레이할 수 있도록 첫 플레이 유저 scene을 추가
  * 튜토리얼 스킵도 가능함.
* 자세한 튜토리얼 툴팁 제공.
* 튜토리얼 맵 수정.

#### 신규 모드 추가

* Custom 모드 추가
  * 플레이하고 싶은 맵을 선택하여 플레이 가능
  * 커스텀 맵을 플레이하기 위해서 Editor 모드에 진입할 필요가 없어졌습니다.
  * 각 맵에 대해서, 해당 맵을 클리어하기까지의 시도 횟수를 기록하고 반영구적으로 저장함.
* Training 모드 추가
  * 기믹의 종류별로 분류된 맵을 연습할 수 있습니다.
  * 각 맵에 대해서, 해당 맵을 클리어하기까지의 시도 횟수를 기록하고 반영구적으로 저장함.
* 크레딧 scene 추가

#### 성취감 및 동기 부여

* Tutorial과 Adventure 모드에서 표시되는 결과 UI에 애니메이션과 SFX를 추가하고 재구성함.
* 시스템 추가
  * 튜토리얼을 클리어하면 별 3개를 받습니다.
  * Adventure 레벨을 클리어하면, 남은 목숨의 수에 따라 별을 받습니다.
  * 모드 선택 및 Adventure 레벨 선택 scene에서, 각 레벨에서 획득한 별의 개수를 확인할 수 있습니다. 앱을 재시작하거나 업데이트하더라도 유지됩니다.
* Adventure 모드에 다양한 맵 추가
  * 이전보다 더욱 다양한 맵을 경험할 수 있습니다.
  * 반복 플레이 주기를 더욱 늘려줍니다.
* Custom 모드에 'God' 난이도 추가
  * 난이도 기준: 숙련된 플레이어가 클리어하기 위해 20-50번 시도해야 하는 맵
* 새로운 맵 추가 및 맵 난이도 밸런스 조정

#### 편의 기능 및 사용자 경험 개선

* 중력 조작 버튼 4개의 크기 확대.
* 쇳덩이 안의 폰트를 밝은 색으로 변경.
* 모드 선택 및 Adventure 레벨 선택 scene에서, 맵의 위치를 재배치해서 재시도 버튼을 누를 필요가 없도록 변경함.
* Tutorial, Adventure, Custom, Traning 모드에서 게임을 일시정지할 때 표시되는 메뉴 UI를 재배치함.
  * 일시정지 메뉴에서 배경음 및 효과음 음량을 조절 가능.
  * 일시정지 메뉴에서 맵을 스킵(남은 시간을 0으로 변경)할 수 있음.
* Custom 모드에서 난이도별로 폴더가 오름차순으로 정렬되도록 폴더명 변경.
* 포털 애니메이션 추가 및 이미지 변경.

### 업데이트 (v1.3.0 -> v1.3.1)

#### 공통

* Game Play 모드
  * 공이 죽거나 재도전 버튼을 누르면 시간이 멈춤. 중력 조작 버튼을 누르면 다시 시간이 흐름.
  * 시간이 멈추면 타이머 UI가 분홍색으로 변함. 시간이 다시 흐르면 보라색으로 변함.
  * 공이 미로를 탈출하면, 중력에 의해 천천히 움직이는 애니메이션을 표시.
* 게임 플레이 scene (Tutorial, Adventure, Editor 내 테스트 단계)에 신규 BGM 추가
* Adventure 모드
  * 대규모 맵 밸런스 패치 적용
    * Easy: 맵 5개 업데이트, 각 맵의 시간 제한 변경.
    * Normal: 맵 7개 업데이트.
    * Hard: 맵 5개 업데이트.
    * Insane: 맵 7개 업데이트.
  * Adventure 모드에서 셔터가 포함된 맵도 등장합니다.
* Editor 내에서, Adventure 모드에서는 등장하지 않는 신규 맵(셔터를 포함하는 맵)을 플레이 가능함
  * Android 유저 분들은, `GravitaMaze.zip`을 다운로드한 후 "내 파일" 앱을 사용해서 루트 경로(`Internal storage`)에 압축 해제해주세요.
  * macOS 유저 분들은, `GravitaMaze.zip`을 다운로드한 후, 압축 해제해서 `Maps` 폴더를 `GravitaMaze.app`의 루트 경로로 옮겨주세요.

#### Android

* CI에 Android target API level을 28로 설정함. (Android 9.0 'Pie')
  * Target API level이 29 이상일 경우, 저장소 읽기/쓰기 권한과 관련한 문제가 발생함.

### 업데이트 (v.1.2.1 -> v1.3.0)

#### 공통

* 셔터가 추가됐습니다!
  * 공이 통과하기 전까지는, 셔터는 아무 일도 하지 않습니다.
  * 공이 통과하고 나면, 셔터는 벽으로 바뀝니다.
* Main scene에 BGM 추가
* Tutorial 모드
  * 셔터를 포함하는 맵 2개 추가
* Editor 모드
  * 맵에 셔터 추가 가능
  * 폴더가 비어있을 경우, "Empty!" 텍스트 표시
  * 긴 파일명과 관련한 버그 수정
  * Open 및 Save UI에서 스크롤바와 관련한 버그 수정
* Editor 내에서, Adventure 모드에서는 등장하지 않는 신규 맵(셔터를 포함하는 맵)을 플레이 가능함
  * Android 유저 분들은, `GravitaMaze.zip`을 다운로드한 후 "내 파일" 앱을 사용해서 루트 경로(`Internal storage`)에 압축 해제해주세요.
  * macOS 유저 분들은, `GravitaMaze.zip`을 다운로드한 후, 압축 해제해서 `Maps` 폴더를 `GravitaMaze.app`의 루트 경로로 옮겨주세요.
* 지원 해상도 추가.
  * 9:22 화면비 지원. (세로 화면)
* Android, Windows, macOS에 대해 자동으로 빌드를 진행하도록 CI 추가.

#### Android

* CI에 Android target API level을 28로 설정함. (Android 9.0 'Pie')
  * Target API level이 29 이상일 경우, 저장소 읽기/쓰기 권한과 관련한 문제가 발생함.

### 업데이트 (v.1.1.0 -> v.1.2.1)

#### 공통

* Tutorial 모드
  * 진행 사항이 표시됩니다.
  * 일시 정지 및 계속 플레이 기능이 지원됩니다.
  * 게임을 나가거나 클리어하면 결과창이 표시됩니다.
* Adventure 모드
  * 대규모 맵 밸런스 패치 적용
    * Easy: 목숨 5개, 맵 10개, 난이도가 더 쉬워졌습니다!
    * Normal: 목숨 5개, 맵 10개, 난이도가 약간 더 쉬워졌습니다.
    * Hard: 목숨 7개, 맵 10개
    * Insane: 목숨 10개, 맵 10개, 난이도가 더 어려워졌습니다!
  * 남은 목숨과 진행 사항이 표시됩니다.
  * 일시 정지 및 계속 플레이 기능이 지원됩니다.
  * 게임을 나가거나 클리어하면 결과창이 표시됩니다.
* Editor 내에서, Adventure 모드에서는 등장하지 않는 신규 맵을 플레이 가능함
  * Android 유저 분들은, `GravitaMaze.zip`을 다운로드한 후 "내 파일" 앱을 사용해서 루트 경로(`Internal storage`)에 압축 해제해주세요.
* 다양한 해상도 지원.
  * 9:16, 9:18, 9:18.5, 9:19, 9:19.5, 9:20, 9:20.5, 9:21 화면비 지원. (세로 화면)
  * 3:4 화면비 미지원.

#### Android

* Tutorial 및 Adventure 모드에서 뒤로가기 버튼을 누르면 일시정지 버튼이 적용됩니다.

#### Windows

* Tutorial 및 Adventure 모드에서 Enter 키를 누르면 다음 버튼이 적용됩니다.
* Tutorial 및 Adventure 모드에서 Esc 키를 누르면 일시정지 버튼이 적용됩니다.

#### macOS

* Tutorial 및 Adventure 모드에서 Enter 키를 누르면 다음 버튼이 적용됩니다.
* Tutorial 및 Adventure 모드에서 Esc 키를 누르면 일시정지 버튼이 적용됩니다.

### 업데이트 (v.1.0.2 -> v.1.1.0)

#### 공통

* Adventure 모드를 플레이할 수 있습니다!
  * Easy, Normal, Hard, Insane 레벨 제공.
  * adventure 모드에서, 맵이 랜덤으로 회전하거나 반전됩니다.
  * 5개의 목숨이 주어지지만, 아직 UI에서는 표시되지 않습니다.
* 맵 파일(`.txt`)을 직접 수정해서 시간 제한을 30초 이상으로 늘리더라도, 시간 제한은 최대 30초로 고정됩니다.

### 업데이트 (v.1.0.1 -> v.1.0.2)

#### 공통

* 시간 제한의 기본값이 10초에서 30초로 증가했습니다.
* 여러 맵을 추가했습니다.

#### Android

* 맵의 저장 장소를 앱 내부 데이터에서 디바이스 내부 저장소로 변경했습니다.
  * 나만의 맵을 공유하거나 다른 사람의 맵을 다운로드할 수 있습니다!
  * 맵 파일은 `GravitaMaze/Maps`에 저장됩니다.

</details>

---

## 플레이 방법

### Android

* [여기를 클릭!](https://github.com/salt26/gravita-maze/releases/tag/v1.4.1)
* `GravitaMaze.v1.4.1.a.zip`을 Android 스마트폰에 다운로드해서 압축 해제한 후, `GravitaMaze.apk`를 실행해서 설치를 진행하세요.
  * `출처를 알 수 없는 앱 설치` 창이 나오면, `무시하고 설치`를 선택하세요.
  * Google Play Protect에서 `안전하지 않은 앱 차단됨` 창이 나오는 경우 ***확인 버튼을 누르지 말고*** `세부정보 더 보기 -> 무시하고 설치하기`를 선택하세요.
  * `Google에 알 수 없는 앱 전송`과 관련한 메시지가 뜨는 경우가 있을 수도 있어요. 이 경우 무슨 옵션을 선택해도 상관 없어요.
  * 브라우저에서 apk 파일이 다운로드되지 않는 경우, `설정 - 애플리케이션 - ... 아이콘 - 특별한 접근 - 출처를 알 수 없는 앱 설치`에서 해당 브라우저에 권한을 허용해주세요.
  * 앱을 처음으로 실행하는 경우, 외부/내부 저장소 쓰기 권한을 허용해주셔야 해요.
    * 해당 권한을 거부하는 경우, 앱에서 맵을 저장하거나 불러올 수 없어요.
    * `다시 묻지 않음` 옵션을 선택한 경우, 애플리케이션 권한 설정에 들어가서 `GravitaMaze`에 해당 권한을 직접 허용해주셔야 해요.
* `GravitaMaze.zip`을 다운로드한 후 "내 파일" 앱에서 루트 경로(`Internal storage`)에 압축 해제하시면 커스텀 맵을 즐길 수 있어요.
  * 해당 맵은 Custom 모드에서 플레이 가능하며, Adventure 모드에서는 플레이할 수 없어요.
* 4개의 "화살표" 버튼을 클릭해서 중력을 조작할 수 있어요.
* 공이 죽는 경우, "재도전" 버튼을 터치해서 해당 맵에 재도전할 수 있어요.
  * 공이 죽으면 시간이 멈춰요.
  * 중력을 조작하면 시간이 다시 흘러요.
* 제한 시간 안에 공을 미로에서 탈출시켜 보세요.
  * 시간이 초과되면 목숨이 1개 깎여요.

### Windows

* [여기를 클릭!](https://github.com/salt26/gravita-maze/releases/tag/v1.4.1)
* `GravitaMaze.v1.4.1.w.zip`을 다운로드한 후 압축 해제하세요.
* `GravitaMaze.exe`를 실행하세요.
* `GravitaMaze.zip`을 다운로드한 후 압축 해제하고, `Maps` 폴더를 `GravitaMaze.exe`의 루트 경로로 옮기면 커스텀 맵을 즐길 수 있어요.
  * 해당 맵은 Custom 모드에서 플레이 가능하며, Adventure 모드에서는 플레이할 수 없어요.
* 4개의 "화살표" 버튼을 클릭해서 중력을 조작할 수 있어요.
* 공이 죽는 경우, "재도전" 버튼(또는 Space 키)을 눌러서 해당 맵에 재도전할 수 있어요.
  * 공이 죽으면 시간이 멈춰요.
  * 중력을 조작하면 시간이 다시 흘러요.
* 제한 시간 안에 공을 미로에서 탈출시켜 보세요.
  * 시간이 초과되면 목숨이 1개 깎여요.

### macOS

* [여기를 클릭!](https://github.com/salt26/gravita-maze/releases/tag/v1.4.1)
* `GravitaMaze.v1.4.1.m.zip`을 다운로드한 후 압축 해제하세요.
* `GravitaMaze.app`을 실행하세요.
* `GravitaMaze.zip`을 다운로드한 후 압축 해제하고, `Maps` 폴더를 `GravitaMaze.app`의 루트 경로로 옮기면 커스텀 맵을 즐길 수 있어요.
  * 해당 맵은 Custom 모드에서 플레이 가능하며, Adventure 모드에서는 플레이할 수 없어요.
* 4개의 "화살표" 버튼을 클릭해서 중력을 조작할 수 있어요.
* 공이 죽는 경우, "재도전" 버튼(또는 Space 키)을 눌러서 해당 맵에 재도전할 수 있어요.
  * 공이 죽으면 시간이 멈춰요.
  * 중력을 조작하면 시간이 다시 흘러요.
* 제한 시간 안에 공을 미로에서 탈출시켜 보세요.
  * 시간이 초과되면 목숨이 1개 깎여요.

## 맵 에디터

* 맵 에디터로 나만의 맵을 만들 수 있어요!

## 플레이 모드

* Tutorial 모드, 4개의 Adventure 모드, Custom 모드, Training 모드가 있어요!

![Screenshot1](./Figures/Screenshot1.v1.3.1.png)

![Screenshot2](./Figures/Screenshot2.v1.3.1.png)
