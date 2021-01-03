#### 1.5.3 (2021-01-03)

##### Chores

* **VersionUpdate:**
  *  1.5.3 ([31ff6fec](https://github.com/shrknt35/release/commit/31ff6fecf71ccc1dabac9b73d83a3f41d8562bfb))
  *  1.5.2-rc ([18e7fd59](https://github.com/shrknt35/release/commit/18e7fd5936a56b10b9cdecdc38ef2821072059df))
* **Release:**  1.5.1 ([66da8846](https://github.com/shrknt35/release/commit/66da8846576c2979712e086ba3252cf7f63c8aa4))

##### New Features

* **Flags:**  add flags for skipping tags and changelog ([603b1e2d](https://github.com/shrknt35/release/commit/603b1e2d1513c04df33053eb2a32de0afd1f2125))
* **Color:**  error and info are now in colors ([ef3db41c](https://github.com/shrknt35/release/commit/ef3db41cf2866cf04df67af47c046d598f67fb0a))

#### 1.5.2-rc (2021-01-03)

##### Chores

* **VersionUpdate:**  1.5.2-rc ([a1156d20](https://github.com/shrknt35/release/commit/a1156d2039a539b6055a65f67149bb4361e2058a))
* **Release:**  1.5.1 ([66da8846](https://github.com/shrknt35/release/commit/66da8846576c2979712e086ba3252cf7f63c8aa4))

##### New Features

* **Color:**  error and info are now in colors ([ef3db41c](https://github.com/shrknt35/release/commit/ef3db41cf2866cf04df67af47c046d598f67fb0a))

#### 1.5.1 (2021-01-02)

##### Chores

* **VersionUpdate:**
  *  1.5.0 ([eab4303e](https://github.com/shrknt35/release/commit/eab4303ead5a3c58e9f8555739a73ad4f95bf22e))
  *  1.4.0-rc ([11655eba](https://github.com/shrknt35/release/commit/11655eba3cf0a84b242f3be5db175bd25eb60ded))
* **VersionBump:**  updated version to 1.3 to track the first working prototype ([9a319e21](https://github.com/shrknt35/release/commit/9a319e215623fd9dca57a94717d5aac108de06dc))
* **Versioning:**  add versioning ([90215a3a](https://github.com/shrknt35/release/commit/90215a3ae5ea023e2afecb78c3946a84121e6755))
* **NugetPush:**  push to nuget ([bd475d4f](https://github.com/shrknt35/release/commit/bd475d4f8d2567966ec7135be3784c1a5485078f))
* **Actions:**
  *  updated .net core version. ([13000fe1](https://github.com/shrknt35/release/commit/13000fe16a0f3147178253518dac566981222de1))
  *  create github action. ([9fdaa907](https://github.com/shrknt35/release/commit/9fdaa907f560f139a8060df597b911d3703e5224))

##### New Features

* **Version:**  gitversion setup files ([2650f8c0](https://github.com/shrknt35/release/commit/2650f8c0d4828ee03c278a0bd7b928acbc248a13))
* **Prerelease:**  implemented process to pre-release tag and to specify release type ([acdebe58](https://github.com/shrknt35/release/commit/acdebe58e9922c4e7197cfcdd567198c4a2703b4))
* **ProcessUpdate:**  implemented processes updates ([1de65db6](https://github.com/shrknt35/release/commit/1de65db670a389de2b62e5fcb47cfad2278fde19))
* **Init:**  implemented init process ([43ae0bb4](https://github.com/shrknt35/release/commit/43ae0bb446977fb5fd1e6731deef43013dde9f07))
* **NoNBGV:**
  *  removing executing any of the nbgv command ([ae0fc60d](https://github.com/shrknt35/release/commit/ae0fc60d0080eb3bc0148c47190cd65acebe4ebd))
  *  removing executing any of the nbgv command ([54a5dcb0](https://github.com/shrknt35/release/commit/54a5dcb008c5118a13ed10f69d5074ed51055a08))
  *  removing executing any of the nbgv command ([e4fa5765](https://github.com/shrknt35/release/commit/e4fa5765ec400d3d9c267f6b0c66cec9c990b66b))
* **VersionizeUWP:**  add versioning for UWP app.: ([245ee038](https://github.com/shrknt35/release/commit/245ee038ca5947d487adf10c7b12ac067d676d0a))
* **Changelog:**  implemented changelog generation ([dbd72880](https://github.com/shrknt35/release/commit/dbd7288048ca8a257533d7d61ae6ae41153a3b03))
* **Rollback:**  implement rollback so that a fresh commit can be created ([e5224e80](https://github.com/shrknt35/release/commit/e5224e801da1c84873e1a714dba4f8445ab80564))
* **NugetKey:**  add nuget key to streamline the process ([632428be](https://github.com/shrknt35/release/commit/632428be1f8295661d5ce50e2fd9d24293747c1d))
* **CommandLine:**  ability to specify argument to arguments in cli ([2c16f85e](https://github.com/shrknt35/release/commit/2c16f85e3d93f6e9f53930954d5b69a50a89db6e))
* **Commit:**  commit the modified files after version change ([775b6dae](https://github.com/shrknt35/release/commit/775b6dae6ce7d9c1183b3d68b757d4e48eee5d1d))
* **Platform:**  ability to change plist and android manifest ([2fbb1894](https://github.com/shrknt35/release/commit/2fbb1894970f704e804d760b07c6c43dc235f2c3))
* **Platfomr:**  add platforms in the release process ([9294d811](https://github.com/shrknt35/release/commit/9294d811068ad02cfe6c63058182036fa6c34238))

##### Bug Fixes

* **Release:**  fix null version number incase of normal release ([38bad637](https://github.com/shrknt35/release/commit/38bad63729f5abfe2a043d1183afa5d85d87288d))
* **GetVersion:**  include assembly version in get-version command ([75bbab08](https://github.com/shrknt35/release/commit/75bbab08b68a061fb00c85597c1f6e6419d74a3e))
* **InitWorkflow:**  improved init workflow ([7bc12bf9](https://github.com/shrknt35/release/commit/7bc12bf9ab7669d9023cede06fc0972364093fdc))
* **Versioning:**  fix the releasing processes which was wasing dirty repo and no rollback after failure ([c5799373](https://github.com/shrknt35/release/commit/c5799373da415f845df1fb8469664ec14c3f34c4))
* **Restructuring:**  restructured the project to add a test project, now each project is in its own folder ([9235e643](https://github.com/shrknt35/release/commit/9235e643c1d229246b3cea723cf6f8c23b75e58a))

##### Other Changes

* //github.com/shrknt35/release into feat-no-nbgv ([6363b6ef](https://github.com/shrknt35/release/commit/6363b6ef3f8e1daae4f86e138592f498e2274b6b))

##### Tests

* **ReleaseManager:**  add test infra and base tests for release manager ([26a8a9ff](https://github.com/shrknt35/release/commit/26a8a9ffc1b9ecdfb862704f7c9cd8f54bfc939c))

