#### 1.17.4-beta.87 (2021-02-04)

##### Chores

* **VersionUpdate:**  1.17.4-beta.87 ([51df281d](https://github.com/shrknt35/release/commit/51df281da47cff77dc5ce1633392ecdd1221ca9e))

##### New Features

* **CommandExecutor:**  add ability to execute commands in mac and linux ([7663153e](https://github.com/shrknt35/release/commit/7663153e4ecdcbed36d99dc9964f72e810afc6d4))
* **Changelog:**  add config to exclude types in changelog generation ([d7d56771](https://github.com/shrknt35/release/commit/d7d56771092a6b7fabb1342b9cc4ed637e7ec58d))

#### 1.17.1-rc.81 (2021-01-26)

#### 1.17.0.80 (2021-01-25)

#### 1.17.0.79 (2021-01-25)

#### 1.6.14-rc.75 (2021-01-25)

##### Chores

* **Version:**
  *  updated version ([53e44b56](https://github.com/shrknt35/release/commit/53e44b569cad415b4af21470865395ccd2b40438))
  *  uploaded build number ([beeb42b6](https://github.com/shrknt35/release/commit/beeb42b6628349fe5870c7cac9d4bd8dc3ec0486))
  *  updated the version ([3d0f2bad](https://github.com/shrknt35/release/commit/3d0f2bad2e9983c58cc0a33364fcaab32b330d42))
* **Release:**  1.5.11-rc.68 ([a3435a4b](https://github.com/shrknt35/release/commit/a3435a4b24ed57702506ff58f677b3cbd2a66903))
* **CICD:**  update pipeline to update build number ([4d8743c4](https://github.com/shrknt35/release/commit/4d8743c4c0c165a4678a08660356a87b30eae95c))

##### New Features

* **Changelog:**
  *  ability to specify changelog filename and to generate changelog between last two tags ([fba92595](https://github.com/shrknt35/release/commit/fba92595bb9ac14884670fabfdccec362ad93233))
  *  word limit on changelog file ([9f14f3dc](https://github.com/shrknt35/release/commit/9f14f3dc241515a960ea83ca94fcdbc2bdc7baaf))

##### Bug Fixes

* **CommandHandler:**  error evaluaction corrected ([19418b81](https://github.com/shrknt35/release/commit/19418b81d0becad05bc5c2688d0f78a6672d29ca))
* **CommandExecuter:**  logging all errors and standard outs ([8c8d34c1](https://github.com/shrknt35/release/commit/8c8d34c15eb00ead072db160b9b0976db3e98b1b))
* **BuildNumber:**  the build numer should be updated every time, no mater the release type ([12bc156a](https://github.com/shrknt35/release/commit/12bc156a41de98eb4de0ec7e78b3997877f8312f))
* **VersionParse:**
  *  fix while increment the patch was combined with pre-release tag which was failing the int parse ([24e7585b](https://github.com/shrknt35/release/commit/24e7585ba8afb526ebc5c3f131b444c06e6528e5))
  *  adjusted the parsing to acomodate build number ([0fe02ba7](https://github.com/shrknt35/release/commit/0fe02ba76d15ea6dbc96d6c6ccb0dfb85ee14718))

##### Other Changes

* **CustomVersion:**  refactored the code around the custom version selection ([8a81b0bd](https://github.com/shrknt35/release/commit/8a81b0bd89d6cbdc61c9fa602ea90368a0893dd2))

