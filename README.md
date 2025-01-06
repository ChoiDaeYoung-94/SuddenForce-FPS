# SuddenForce-FPS

A 3D FPS unity(2022.3.52f1 LTS) game with multiplayer features, developed using Photon Fusion.


### Trailer

TODO


## Getting Started

1. Clone
~~~
git clone https://github.com/ChoiDaeYoung-94/SuddenForce-FPS.git
~~~
2. Open Project in Unity


## Download

- TODO > GOOGLE PLAY
- [APK](https://drive.google.com/file/d/1SQojyQafq9IdmNvTONF80G8ri3QPqeXk/view?usp=sharing)


## Technologies and Techniques (링크 클릭 시 해당 내용 코드로 이동합니다.)

- [google v2, PlayFab login code](https://github.com/ChoiDaeYoung-94/SuddenForce-FPS/blob/main/Assets/03.Objects/LoginCanvas/Login.cs)
- [dynamic scrollview code](https://github.com/ChoiDaeYoung-94/SuddenForce-FPS/blob/main/Assets/03.Objects/Dynamic%20ScrollView/RoomManage.cs)


## Project Goals

- [x] 멀티 플레이 게임 (Photon Fusion 사용)
- skill system, equipment system 구현
- [x] 구글 플레이 v2(https://developer.android.com/games/pgs/android/android-signin?hl=ko) 사용
- prefab 아름답게 사용


## Self Feedback

- 


## SDK, Package ...

- [Photon Fusion](https://doc.photonengine.com/fusion/current/getting-started/sdk-download)
- [play games plugin](https://github.com/playgameservices/play-games-plugin-for-unity/releases)
- [PlayFabEditorExtensions, PlayFabSDK](https://docs.microsoft.com/ko-kr/gaming/playfab/sdks/unity3d/installing-unity3d-sdk)
- [Google Mobile Ads SDK](https://developers.google.com/admob/android/quick-start?hl=ko)
- [DOTween](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)
- [MiniJSON](https://github.com/Unity-Technologies/UnityCsReference/blob/master/External/JsonParsers/MiniJson/MiniJSON.cs)
- [Keystore Helper](https://assetstore.unity.com/packages/tools/utilities/keystore-helper-58627)
- [In-game Debug Console](https://assetstore.unity.com/packages/tools/gui/in-game-debug-console-68068)
- [Safe Area Helper](https://assetstore.unity.com/packages/tools/gui/safe-area-helper-130488)



## Build

| platform  | output   |
| --------- | -------- |
| AOS       | apk, aab |

build 추출물은 Project root/Build/AOS에 위치한다.


### Unity Scenario

시작 전 게임 프로젝트의 root 경로에 Build 폴더를 만든 뒤 진행한다.

- apk
  - Unity Menu - Build - AOS - APK
- aab
  - Unity Menu - Build - AOS - AAB


### CLI Scenario

https://github.com/ChoiDaeYoung-94/unity-cicd 레포의 build.py를 사용하여 빌드한다.

build.py를 통해 build 시 aab, apk 모두 빌드된다.

terminal > python build.py > 매개변수 입력 > build


### Github Actions Scenario

main branch에 push 할 경우 Github Action이 작동하고 BuildPC에서 빌드를 진행한다.

마지막 commit message에 ci skip 이 포함되어 있을 경우 Github Actions을 skip 한다.

빌드 추출물(aab)은 Appcenter에 upload 되며 Appcenter에서 다운로드 시 apk로 다운로드 하기때문에 apk는 추출하지 않는다.

정상적으로 upload 되었다면 Appcenter에 등록되어 있는 group 사용자에게 알림(e-mail)을 보낸다.