## [1.5.9-rc](https://github.com/shrknt35/release/compare/v1.5.9...v1.5.9-rc) (2021-01-10)



## [1.5.9](https://github.com/shrknt35/release/compare/v1.5.5-rc...v1.5.9) (2021-01-10)


### Bug Fixes

* **BuildNumber:** build bumber should be part of main version string ([b19c8ba](https://github.com/shrknt35/release/commit/b19c8ba225f4a24bef3e22874c4bb2aa54c9fcaa))
* **FindFlag:** add ability to find any folder up the change that has the config file ([e46ec53](https://github.com/shrknt35/release/commit/e46ec5310388d41e7b54f34c4c4f8dca326f113f))
* **Native:** fix where native tag was not being accepted ([8b77e52](https://github.com/shrknt35/release/commit/8b77e52d1c1ada94e3a9e3537d3e6827129df962))
* **PreRelease:** fix pre-release-tag name variable name ([dd40c6a](https://github.com/shrknt35/release/commit/dd40c6a13e31e7b7e121f34ee86c8f9466f7eb14))
* **RelativePath:** add ability to add relative root path ([e94cd32](https://github.com/shrknt35/release/commit/e94cd3222b3cd1c80cc495183632d6dba55948bc))


### Features

* **BuildNumber:** add ability to increment build number ([21891d5](https://github.com/shrknt35/release/commit/21891d504cae6fa69694749316f6074c28c340a9))
* **BuildNumber:** include build number in the version string ([ba79208](https://github.com/shrknt35/release/commit/ba7920837b373a7d26097fee197285b9bff51f7b))



## [1.5.5-rc](https://github.com/shrknt35/release/compare/v1.5.1...v1.5.5-rc) (2021-01-04)


### Features

* **Color:** error and info are now in colors ([ef3db41](https://github.com/shrknt35/release/commit/ef3db41cf2866cf04df67af47c046d598f67fb0a))
* **Flags:** add flags for skipping tags and changelog ([603b1e2](https://github.com/shrknt35/release/commit/603b1e2d1513c04df33053eb2a32de0afd1f2125))



## [1.5.1](https://github.com/shrknt35/release/compare/9235e643c1d229246b3cea723cf6f8c23b75e58a...v1.5.1) (2021-01-02)


### Bug Fixes

* **GetVersion:** include assembly version in get-version command ([75bbab0](https://github.com/shrknt35/release/commit/75bbab08b68a061fb00c85597c1f6e6419d74a3e))
* **InitWorkflow:** improved init workflow ([7bc12bf](https://github.com/shrknt35/release/commit/7bc12bf9ab7669d9023cede06fc0972364093fdc))
* **Release:** fix null version number incase of normal release ([38bad63](https://github.com/shrknt35/release/commit/38bad63729f5abfe2a043d1183afa5d85d87288d))
* **Restructuring:** restructured the project to add a test project, now each project is in its own folder ([9235e64](https://github.com/shrknt35/release/commit/9235e643c1d229246b3cea723cf6f8c23b75e58a))
* **Versioning:** fix the releasing processes which was wasing dirty repo and no rollback after failure ([c579937](https://github.com/shrknt35/release/commit/c5799373da415f845df1fb8469664ec14c3f34c4))


### Features

* **Changelog:** implemented changelog generation ([dbd7288](https://github.com/shrknt35/release/commit/dbd7288048ca8a257533d7d61ae6ae41153a3b03))
* **CommandLine:** ability to specify argument to arguments in cli ([2c16f85](https://github.com/shrknt35/release/commit/2c16f85e3d93f6e9f53930954d5b69a50a89db6e))
* **Commit:** commit the modified files after version change ([775b6da](https://github.com/shrknt35/release/commit/775b6dae6ce7d9c1183b3d68b757d4e48eee5d1d))
* **Init:** implemented init process ([43ae0bb](https://github.com/shrknt35/release/commit/43ae0bb446977fb5fd1e6731deef43013dde9f07))
* **NoNBGV:** removing executing any of the nbgv command ([ae0fc60](https://github.com/shrknt35/release/commit/ae0fc60d0080eb3bc0148c47190cd65acebe4ebd))
* **NoNBGV:** removing executing any of the nbgv command ([54a5dcb](https://github.com/shrknt35/release/commit/54a5dcb008c5118a13ed10f69d5074ed51055a08))
* **NoNBGV:** removing executing any of the nbgv command ([e4fa576](https://github.com/shrknt35/release/commit/e4fa5765ec400d3d9c267f6b0c66cec9c990b66b))
* **NugetKey:** add nuget key to streamline the process ([632428b](https://github.com/shrknt35/release/commit/632428be1f8295661d5ce50e2fd9d24293747c1d))
* **Platfomr:** add platforms in the release process ([9294d81](https://github.com/shrknt35/release/commit/9294d811068ad02cfe6c63058182036fa6c34238))
* **Platform:** ability to change plist and android manifest ([2fbb189](https://github.com/shrknt35/release/commit/2fbb1894970f704e804d760b07c6c43dc235f2c3))
* **Prerelease:** implemented process to pre-release tag and to specify release type ([acdebe5](https://github.com/shrknt35/release/commit/acdebe58e9922c4e7197cfcdd567198c4a2703b4))
* **ProcessUpdate:** implemented processes updates ([1de65db](https://github.com/shrknt35/release/commit/1de65db670a389de2b62e5fcb47cfad2278fde19))
* **Rollback:** implement rollback so that a fresh commit can be created ([e5224e8](https://github.com/shrknt35/release/commit/e5224e801da1c84873e1a714dba4f8445ab80564))
* **Version:** gitversion setup files ([2650f8c](https://github.com/shrknt35/release/commit/2650f8c0d4828ee03c278a0bd7b928acbc248a13))
* **VersionizeUWP:** add versioning for UWP app.: ([245ee03](https://github.com/shrknt35/release/commit/245ee038ca5947d487adf10c7b12ac067d676d0a))



