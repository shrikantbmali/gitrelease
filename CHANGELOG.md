#### 1.5.11-rc.60 (2021-01-11)

##### Chores

* **CICD:**  update pipeline to update build number ([4d8743c4](https://github.com/shrknt35/release/commit/4d8743c4c0c165a4678a08660356a87b30eae95c))
* **Version:**  updated the version ([3d0f2bad](https://github.com/shrknt35/release/commit/3d0f2bad2e9983c58cc0a33364fcaab32b330d42))

##### New Features

* **Changelog:**  word limit on changelog file ([9f14f3dc](https://github.com/shrknt35/release/commit/9f14f3dc241515a960ea83ca94fcdbc2bdc7baaf))

##### Bug Fixes

* **BuildNumber:**  the build numer should be updated every time, no mater the release type ([12bc156a](https://github.com/shrknt35/release/commit/12bc156a41de98eb4de0ec7e78b3997877f8312f))
* **VersionParse:**
  *  fix while increment the patch was combined with pre-release tag which was failing the int parse ([24e7585b](https://github.com/shrknt35/release/commit/24e7585ba8afb526ebc5c3f131b444c06e6528e5))
  *  adjusted the parsing to acomodate build number ([0fe02ba7](https://github.com/shrknt35/release/commit/0fe02ba76d15ea6dbc96d6c6ccb0dfb85ee14718))

