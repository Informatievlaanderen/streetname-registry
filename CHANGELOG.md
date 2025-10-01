# [5.3.0](https://github.com/informatievlaanderen/streetname-registry/compare/v5.2.0...v5.3.0) (2025-10-01)


### Bug Fixes

* add missing connectionstrings to default appsettings ([b501d9c](https://github.com/informatievlaanderen/streetname-registry/commit/b501d9ccd5d019c0ce2ea7368d4bf0ed70990530))


### Features

* **api:** use elasticsearch for oslo list ([5c70882](https://github.com/informatievlaanderen/streetname-registry/commit/5c7088215998ac152e6809bf33532a7e201e59ca))

# [5.2.0](https://github.com/informatievlaanderen/streetname-registry/compare/v5.1.2...v5.2.0) (2025-09-30)


### Features

* add streetnames list to Elastic (projections) ([c9c1cf4](https://github.com/informatievlaanderen/streetname-registry/commit/c9c1cf4783e4bd176a7acce4538f03052a11aafb))

## [5.1.2](https://github.com/informatievlaanderen/streetname-registry/compare/v5.1.1...v5.1.2) (2025-08-27)


### Bug Fixes

* **oslo:** straatnamen fetch list ([f800eae](https://github.com/informatievlaanderen/streetname-registry/commit/f800eae4a164931e56c5f1e7d9575b4c4c8859e4))

## [5.1.1](https://github.com/informatievlaanderen/streetname-registry/compare/v5.1.0...v5.1.1) (2025-08-27)


### Performance Improvements

* **oslo:** add indexes + select only needed fields for list GAWR-7040 ([4fb01ef](https://github.com/informatievlaanderen/streetname-registry/commit/4fb01efeea1489f4e1b7e867550e54f8804ef885))

# [5.1.0](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.11...v5.1.0) (2025-08-19)


### Features

* add remove municipality GAWR-4168 ([#467](https://github.com/informatievlaanderen/streetname-registry/issues/467)) ([2be1549](https://github.com/informatievlaanderen/streetname-registry/commit/2be1549e8b3276ec748610aac82461f77523e5b4))

## [5.0.11](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.10...v5.0.11) (2025-06-05)


### Bug Fixes

* **oslo-api:** correct timestamp for list ([fd49f9c](https://github.com/informatievlaanderen/streetname-registry/commit/fd49f9c174d2e1acfa526a4d41c7b39ab0198025))

## [5.0.10](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.9...v5.0.10) (2025-05-28)


### Performance Improvements

* streetname list query GAWR-6931 ([451b869](https://github.com/informatievlaanderen/streetname-registry/commit/451b86936dd5356804b128f1a9068dd52b91b770))

## [5.0.9](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.8...v5.0.9) (2025-05-27)


### Bug Fixes

* **ci:** separate last-changed-list ([f7170cc](https://github.com/informatievlaanderen/streetname-registry/commit/f7170cc7ca73807e1e66e1ea798eb2b6c224892c))

## [5.0.8](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.7...v5.0.8) (2025-05-07)


### Bug Fixes

* **projector:** add healthcheck ([55c4450](https://github.com/informatievlaanderen/streetname-registry/commit/55c445017dddd433bd4f737a298c05e2e620d348))

## [5.0.7](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.6...v5.0.7) (2025-05-05)


### Bug Fixes

* **consumer:** bump message-handling to 6.0.3 ([0b73b49](https://github.com/informatievlaanderen/streetname-registry/commit/0b73b49bd794befc1a32af2ca0d947975672eda5))

## [5.0.6](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.5...v5.0.6) (2025-05-02)


### Bug Fixes

* **abstractions:** fix package ([c05bac2](https://github.com/informatievlaanderen/streetname-registry/commit/c05bac2a7da1a186a17639889e41645157808d7b))

## [5.0.5](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.4...v5.0.5) (2025-05-02)


### Bug Fixes

* **oslo:** fix package ([6730c35](https://github.com/informatievlaanderen/streetname-registry/commit/6730c35493cc834536083fb8c16f8ea59a3ad0b0))

## [5.0.4](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.3...v5.0.4) (2025-04-23)


### Bug Fixes

* **ci:** get correct datadog tracer ([95de95e](https://github.com/informatievlaanderen/streetname-registry/commit/95de95edf6a7611b945a34bd5fdf482dbcd285e1))

## [5.0.3](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.2...v5.0.3) (2025-04-23)


### Bug Fixes

* **ci:** build lambda on arm64 ([2d40248](https://github.com/informatievlaanderen/streetname-registry/commit/2d40248f08b147df9f39881d2e10a39103448be0))

## [5.0.2](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.1...v5.0.2) (2025-04-22)


### Bug Fixes

* **ci:** fix push image ([fce8b7e](https://github.com/informatievlaanderen/streetname-registry/commit/fce8b7e4820150f26167fd809a839d9668aff919))

## [5.0.1](https://github.com/informatievlaanderen/streetname-registry/compare/v5.0.0...v5.0.1) (2025-04-22)


### Bug Fixes

* **ci:** fix push image ([4cb5315](https://github.com/informatievlaanderen/streetname-registry/commit/4cb5315ce447740edfde16718495b3fc3614d065))

# [5.0.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.19.3...v5.0.0) (2025-04-22)


### Code Refactoring

* use renovate and nuget + update pipeline ([c6ae4d2](https://github.com/informatievlaanderen/streetname-registry/commit/c6ae4d2bc7d56b0cc456a24a44dcd72a29be7026))


### BREAKING CHANGES

* update to dotnet 9

## [4.19.3](https://github.com/informatievlaanderen/streetname-registry/compare/v4.19.2...v4.19.3) (2025-03-24)


### Bug Fixes

* **ldes:** produce! :) ([a9c4e12](https://github.com/informatievlaanderen/streetname-registry/commit/a9c4e12526097cace211017f1ee8e3f65fe78d24))

## [4.19.2](https://github.com/informatievlaanderen/streetname-registry/compare/v4.19.1...v4.19.2) (2025-03-24)


### Bug Fixes

* **ldes:** correct index ([ee758a2](https://github.com/informatievlaanderen/streetname-registry/commit/ee758a2b850be321d220a93e4ed2d05bc571b79f))

## [4.19.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.19.0...v4.19.1) (2025-03-24)


### Bug Fixes

* **ci:** push producer-ldes image to trigger bump ([3ffa21f](https://github.com/informatievlaanderen/streetname-registry/commit/3ffa21f8450aa93622ad73c6c65a141ccd8c3e8d))

# [4.19.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.18.3...v4.19.0) (2025-03-24)


### Features

* GAWR-6786 add Ldes Producer ([a816acd](https://github.com/informatievlaanderen/streetname-registry/commit/a816acd37c1b3f56d89cfc909b9188c6d99f571a))

## [4.18.3](https://github.com/informatievlaanderen/streetname-registry/compare/v4.18.2...v4.18.3) (2025-02-03)


### Bug Fixes

* **ci:** update deployv4 lambda to trigger build ([45be5a0](https://github.com/informatievlaanderen/streetname-registry/commit/45be5a08706836ebe0b0dfc03113df8c28a916e6))

## [4.18.2](https://github.com/informatievlaanderen/streetname-registry/compare/v4.18.1...v4.18.2) (2025-01-16)


### Bug Fixes

* **producer:** correct snapshot producer ([c297594](https://github.com/informatievlaanderen/streetname-registry/commit/c29759478494895207545ec43b2ef8bbac1ef2e9))

## [4.18.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.18.0...v4.18.1) (2025-01-16)

# [4.18.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.17.1...v4.18.0) (2025-01-07)


### Features

* **api:** add docs for feeds GAWR-5321 ([37965e5](https://github.com/informatievlaanderen/streetname-registry/commit/37965e54388bb641821be936ad0b1b87b28bbc15))

## [4.17.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.17.0...v4.17.1) (2025-01-02)


### Bug Fixes

* **backoffice:** add distributed cache for introspection cache ([82b84f2](https://github.com/informatievlaanderen/streetname-registry/commit/82b84f20b8111860cb9b4e3366eb45f6daa54d60))

# [4.17.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.16.2...v4.17.0) (2025-01-02)


### Features

* add cache introspection ([6e2a6c9](https://github.com/informatievlaanderen/streetname-registry/commit/6e2a6c9852e58dbf8640a67f0575c28926e8b6e7))

## [4.16.2](https://github.com/informatievlaanderen/streetname-registry/compare/v4.16.1...v4.16.2) (2024-12-24)


### Bug Fixes

* don't approve streetname for merger when it wasnt merged ([514c55c](https://github.com/informatievlaanderen/streetname-registry/commit/514c55c00c313cf8e33103f626fac39ab93e2c09))

## [4.16.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.16.0...v4.16.1) (2024-12-10)


### Bug Fixes

* don't include removed streetname in StreetNameWasRetiredBecauseOfMunicipalityMerger.MergedPersistentLocalIds ([2a976bb](https://github.com/informatievlaanderen/streetname-registry/commit/2a976bb0f973355a302c666f88fce10a84ba87d5))

# [4.16.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.15.0...v4.16.0) (2024-12-05)


### Features

* configure consumer offset without deploy ([e0b3c9f](https://github.com/informatievlaanderen/streetname-registry/commit/e0b3c9fcbe096b41daec040ad62fead0a2466dde))

# [4.15.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.14.0...v4.15.0) (2024-12-02)


### Features

* allow correction of empty name ([388c8fa](https://github.com/informatievlaanderen/streetname-registry/commit/388c8fa8a2f233a03e97208588cad121fa553053))

# [4.14.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.13.0...v4.14.0) (2024-11-19)


### Bug Fixes

* **docs:** hide migrated event legacy aggregate ([2f3c89d](https://github.com/informatievlaanderen/streetname-registry/commit/2f3c89db446274449e0bd168fd8aa1f302677f50))


### Features

* add events + test to check if projections / producer are missing events ([e746932](https://github.com/informatievlaanderen/streetname-registry/commit/e7469322bc4c9b499a1e8c608bae8077d832aaab))

# [4.13.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.12.0...v4.13.0) (2024-11-18)


### Bug Fixes

* **backoffice:** finetune multiple split/merge error ([81fc479](https://github.com/informatievlaanderen/streetname-registry/commit/81fc4798c20162b20dcb9e98dc675371f7e3ee3f))


### Features

* hide events ([e7f922d](https://github.com/informatievlaanderen/streetname-registry/commit/e7f922de4c5c3f5c57fa967bb9019dacc4e54eab))

# [4.12.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.11.2...v4.12.0) (2024-11-14)


### Features

* **backoffice:** add dry run to merger + add error for combining and splitting same streetname ([3789e18](https://github.com/informatievlaanderen/streetname-registry/commit/3789e18ce767f6113e6caaf5489c18ba18d35e3b))

## [4.11.2](https://github.com/informatievlaanderen/streetname-registry/compare/v4.11.1...v4.11.2) (2024-10-25)


### Bug Fixes

* correct streetname when language is not present ([3ab64d1](https://github.com/informatievlaanderen/streetname-registry/commit/3ab64d12eda21d149cf7a8eaaab6b2615ef6e58a))

## [4.11.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.11.0...v4.11.1) (2024-10-24)

# [4.11.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.10.2...v4.11.0) (2024-10-23)


### Features

* **wfs:** add geolocation view ([34334b2](https://github.com/informatievlaanderen/streetname-registry/commit/34334b25a07b6de02b3b3d23616990c5a17700a1))

## [4.10.2](https://github.com/informatievlaanderen/streetname-registry/compare/v4.10.1...v4.10.2) (2024-10-21)


### Bug Fixes

* check duplicate merger streetnames in api ([9a34ee7](https://github.com/informatievlaanderen/streetname-registry/commit/9a34ee738df7e43e2b3bfb40b88ea50f4f36a9d2))

## [4.10.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.10.0...v4.10.1) (2024-10-14)


### Bug Fixes

* **backoffice:** add default blacklistedovocodes section ([3270f61](https://github.com/informatievlaanderen/streetname-registry/commit/3270f61bfea919b042481a1aac56ba6bc7e16f01))

# [4.10.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.9.7...v4.10.0) (2024-10-07)


### Bug Fixes

* municipality merger in syndication ([db0b3eb](https://github.com/informatievlaanderen/streetname-registry/commit/db0b3ebe0c1a9322174128ad0a10624e5fb60d41))


### Features

* **backoffice:** restrict rename & retire ([e9db544](https://github.com/informatievlaanderen/streetname-registry/commit/e9db54463c5e40c083e248dc6003a77d4d3fb912))

## [4.9.7](https://github.com/informatievlaanderen/streetname-registry/compare/v4.9.6...v4.9.7) (2024-10-03)


### Bug Fixes

* temporarily disable correct renamed streetname ([d62ff86](https://github.com/informatievlaanderen/streetname-registry/commit/d62ff86454abfbbc31bfcf02750e1f87f9fa7b9b))

## [4.9.6](https://github.com/informatievlaanderen/streetname-registry/compare/v4.9.5...v4.9.6) (2024-09-24)


### Bug Fixes

* return multiple error messages at once for propose for municipality merger ([5cf5823](https://github.com/informatievlaanderen/streetname-registry/commit/5cf5823ec43d96db6680d1aa279f1e1de3a5d529))

## [4.9.5](https://github.com/informatievlaanderen/streetname-registry/compare/v4.9.4...v4.9.5) (2024-09-24)


### Bug Fixes

* municipality status for MunicipalityWasImported should be Proposed ([94244b0](https://github.com/informatievlaanderen/streetname-registry/commit/94244b019b8ac3f866b5b07b3150c5a7916b36f6))

## [4.9.4](https://github.com/informatievlaanderen/streetname-registry/compare/v4.9.3...v4.9.4) (2024-09-24)


### Bug Fixes

* ci deploy lambda ([49d398f](https://github.com/informatievlaanderen/streetname-registry/commit/49d398f0475d15eacd2afd7601bf38be06291eec))

## [4.9.3](https://github.com/informatievlaanderen/streetname-registry/compare/v4.9.2...v4.9.3) (2024-09-17)


### Bug Fixes

* use persistentlocalids for propose streetnames for municipality merger ([84040ec](https://github.com/informatievlaanderen/streetname-registry/commit/84040ec88a29c979ed209b80a50407d1bd17fbb1))

## [4.9.2](https://github.com/informatievlaanderen/streetname-registry/compare/v4.9.1...v4.9.2) (2024-09-04)


### Bug Fixes

* use lifetimescope when dispatching multiple commands for municipality proposal ([965a6e6](https://github.com/informatievlaanderen/streetname-registry/commit/965a6e6772615b99cc5196108d46bf4c3e76072b))

## [4.9.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.9.0...v4.9.1) (2024-09-04)


### Bug Fixes

* check for empty HomonymAddition when proposing merger ([26a29b8](https://github.com/informatievlaanderen/streetname-registry/commit/26a29b8ec0f82482f4e64a0386f53b6d79f77343))

# [4.9.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.8.3...v4.9.0) (2024-09-03)


### Features

* all stream create oslo snapshots ([d752316](https://github.com/informatievlaanderen/streetname-registry/commit/d752316833d7891d65bb4f607fa8466ab1e5347e))

## [4.8.3](https://github.com/informatievlaanderen/streetname-registry/compare/v4.8.2...v4.8.3) (2024-09-03)


### Bug Fixes

* rename streetname to same streetname ([8d756aa](https://github.com/informatievlaanderen/streetname-registry/commit/8d756aaf12264295fb27281456e9e9df69c9bf80))

## [4.8.2](https://github.com/informatievlaanderen/streetname-registry/compare/v4.8.1...v4.8.2) (2024-09-03)


### Bug Fixes

* sqs handler name to match request ([da579ce](https://github.com/informatievlaanderen/streetname-registry/commit/da579ce7bf26651cddf6e5daaca4c67e92b92846))

## [4.8.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.8.0...v4.8.1) (2024-08-30)


### Bug Fixes

* add missing sqs->lambda request mapping for ProposeStreetNamesForMunicipalityMerger ([195767c](https://github.com/informatievlaanderen/streetname-registry/commit/195767ce725e1d4cb3db5f80f402f2f73868cc91))

# [4.8.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.7.0...v4.8.0) (2024-08-22)


### Features

* clean up legacy api ([82df484](https://github.com/informatievlaanderen/streetname-registry/commit/82df484354fce8a0f4355e7cdd16c8f9656f0001))
* **consumer:** add offset as projection state to read consumer ([aaea937](https://github.com/informatievlaanderen/streetname-registry/commit/aaea937b2e82dab51a3f0df4849d5a948e3a7435))

# [4.7.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.6.0...v4.7.0) (2024-07-29)


### Bug Fixes

* add fusie reason ([fbc2bd7](https://github.com/informatievlaanderen/streetname-registry/commit/fbc2bd739a45d1a9705efbfcd494531da90f98ca))
* add streetnamewasproposedformunicipalitymerger to backoffice projection ([4f6d24f](https://github.com/informatievlaanderen/streetname-registry/commit/4f6d24f446f14f2b0ecceb2eb6c8746e47f6695d))


### Features

* add integration projection for merger ([f2a02f2](https://github.com/informatievlaanderen/streetname-registry/commit/f2a02f2f422d1e16f9cd686be11b60423c9a2e54))
* add snapshot status endpoint ([c87475b](https://github.com/informatievlaanderen/streetname-registry/commit/c87475b91882b0921a7cf5b3bd8826b02e3c7be6))
* **consumer:** add postal relink municipality ([9ae2c22](https://github.com/informatievlaanderen/streetname-registry/commit/9ae2c221a58f806eeaa0a592534ca78d15989281))
* enable blacklisted ovocodes ([316329b](https://github.com/informatievlaanderen/streetname-registry/commit/316329bc18712b7b2c320089369fd57bc241bce9))
* proposed streetnames can be included in municipality merger ([b84a270](https://github.com/informatievlaanderen/streetname-registry/commit/b84a27041a137547546b1ec91465a4a7e5e7ce07))

# [4.6.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.5.2...v4.6.0) (2024-07-15)


### Features

* add commands ApproveStreetNamesForMunicipalityMerger and RetireMunicipalityForMunicipalityMerger ([aefb429](https://github.com/informatievlaanderen/streetname-registry/commit/aefb429c74fb9f9a484fbe2a885367da461937d6))
* add domain propose streetname from merger ([76f2f6d](https://github.com/informatievlaanderen/streetname-registry/commit/76f2f6da3cae398df23a7e092c06e823616e4905))
* add MunicipalityWasMerged handler to consumer ([e6e6333](https://github.com/informatievlaanderen/streetname-registry/commit/e6e63335864187b1910d55266462e5cb36086673))
* add propose for muni merger api + lambda ([4dfd328](https://github.com/informatievlaanderen/streetname-registry/commit/4dfd3289bc21d381aad7c75c2051425b51c2910d))

## [4.5.2](https://github.com/informatievlaanderen/streetname-registry/compare/v4.5.1...v4.5.2) (2024-07-02)


### Bug Fixes

* streetname sync controller ([a0a8fc5](https://github.com/informatievlaanderen/streetname-registry/commit/a0a8fc5bc38fb23b6456c5599dd0877cbabb435a))

## [4.5.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.5.0...v4.5.1) (2024-07-02)


### Bug Fixes

* correct docs oslo sync ([fdea69e](https://github.com/informatievlaanderen/streetname-registry/commit/fdea69e32715c4639ec974ee1a8475b75564038e))

# [4.5.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.4.1...v4.5.0) (2024-06-28)


### Features

* remove unneeded producer ([fd04566](https://github.com/informatievlaanderen/streetname-registry/commit/fd04566e50d40b627b3d245bbf943cc588969051))

## [4.4.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.4.0...v4.4.1) (2024-06-18)


### Bug Fixes

* **ci:** use new lambda deploy test+stg ([e59da98](https://github.com/informatievlaanderen/streetname-registry/commit/e59da980cf5a005e09874fc69e71c27984747a56))

# [4.4.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.3.0...v4.4.0) (2024-06-18)


### Features

* remove postal consumer legacy ([fb5be96](https://github.com/informatievlaanderen/streetname-registry/commit/fb5be96e78e6ee39c446e40921b2fc5ede029343))

# [4.3.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.2.1...v4.3.0) (2024-06-17)


### Bug Fixes

* add delay to backoffice projections ([f64447f](https://github.com/informatievlaanderen/streetname-registry/commit/f64447f7ce6a652d958e02c8b2d38546522fe428))
* use consumed event provenance timestamp for command ([963b09c](https://github.com/informatievlaanderen/streetname-registry/commit/963b09cf87f57c40626938bd947a63aec3aaf819))


### Features

* add syndication to oslo api ([148d03f](https://github.com/informatievlaanderen/streetname-registry/commit/148d03f78740f9143534da421ebc21026413412b))

## [4.2.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.2.0...v4.2.1) (2024-05-15)


### Bug Fixes

* **ci:** add newstg pipeline + add version to prd ([48e41bc](https://github.com/informatievlaanderen/streetname-registry/commit/48e41bcea4b417c2707f8eba942516b428257fbb))

# [4.2.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.1.1...v4.2.0) (2024-04-29)


### Features

* add combined index isremoved and status on integration projections ([3bda9f9](https://github.com/informatievlaanderen/streetname-registry/commit/3bda9f98e8949f70bb8ab36d5275743398e852ea))

## [4.1.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.1.0...v4.1.1) (2024-04-25)


### Bug Fixes

* restore snapshot streetname isrenamed ([aac4dcc](https://github.com/informatievlaanderen/streetname-registry/commit/aac4dcc027604833cd4b272cddd42656efb4b8a6))

# [4.1.0](https://github.com/informatievlaanderen/streetname-registry/compare/v4.0.7...v4.1.0) (2024-04-22)


### Features

* prevent correcting retirement when streetname was renamed ([8f717d8](https://github.com/informatievlaanderen/streetname-registry/commit/8f717d89adad6746c16708b6c507bb098829e870))

## [4.0.7](https://github.com/informatievlaanderen/streetname-registry/compare/v4.0.6...v4.0.7) (2024-04-08)


### Bug Fixes

* enable authorization ([5ff1bd3](https://github.com/informatievlaanderen/streetname-registry/commit/5ff1bd32f0caadabfd834b4ed6157d4cf96b2a3a))

## [4.0.6](https://github.com/informatievlaanderen/streetname-registry/compare/v4.0.5...v4.0.6) (2024-04-05)

## [4.0.5](https://github.com/informatievlaanderen/streetname-registry/compare/v4.0.4...v4.0.5) (2024-04-02)

## [4.0.4](https://github.com/informatievlaanderen/streetname-registry/compare/v4.0.3...v4.0.4) (2024-03-25)


### Bug Fixes

* style to bump ([f73caa1](https://github.com/informatievlaanderen/streetname-registry/commit/f73caa1e78f850ae10e7d762a6b073f1b7805b4d))

## [4.0.3](https://github.com/informatievlaanderen/streetname-registry/compare/v4.0.2...v4.0.3) (2024-03-25)


### Bug Fixes

* style to trigger build ([85dde5d](https://github.com/informatievlaanderen/streetname-registry/commit/85dde5d88baf07326dd0985f9527efea5919d3cf))

## [4.0.2](https://github.com/informatievlaanderen/streetname-registry/compare/v4.0.1...v4.0.2) (2024-03-25)


### Bug Fixes

* style to trigger build ([fe3988d](https://github.com/informatievlaanderen/streetname-registry/commit/fe3988df7db1378a38c6d879d0182562847d8871))

## [4.0.1](https://github.com/informatievlaanderen/streetname-registry/compare/v4.0.0...v4.0.1) (2024-03-20)


### Bug Fixes

* disable automatic fluentvalidation ([ae78384](https://github.com/informatievlaanderen/streetname-registry/commit/ae78384560fe50eb3172bbe204daefa2108f6ab9))

# [4.0.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.45.0...v4.0.0) (2024-03-19)


### Features

* move to dotnet 8.0.2 ([073c741](https://github.com/informatievlaanderen/streetname-registry/commit/073c741cfe3be7a5dfa466c1032ed9ed2bb876fa))


### BREAKING CHANGES

* move to dotnet 8.0.2

# [3.45.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.44.0...v3.45.0) (2024-03-01)


### Features

* remove bosa functionality GAWR-5457 ([d9bde12](https://github.com/informatievlaanderen/streetname-registry/commit/d9bde12534a8c0bfe8249d7f9de2bb23df07130e))

# [3.44.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.43.0...v3.44.0) (2024-02-22)


### Features

* add type to latest versions integration ([73e8496](https://github.com/informatievlaanderen/streetname-registry/commit/73e8496e1976ff6b2043cc6d4c544303b071da2f))

# [3.43.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.42.2...v3.43.0) (2024-02-20)


### Features

* shutdown backoffice projections service when projection is stopped ([715cc68](https://github.com/informatievlaanderen/streetname-registry/commit/715cc68ff2e8e624a6d23173e5ebb0d543f622cd))

## [3.42.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.42.1...v3.42.2) (2024-02-19)


### Bug Fixes

* add offset setting to consumer ([1f3c8a3](https://github.com/informatievlaanderen/streetname-registry/commit/1f3c8a33adaa5d79451098dbf61aab8fc863a1b2))

## [3.42.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.42.0...v3.42.1) (2024-02-15)


### Bug Fixes

* **ci:** new lambda pipeline test ([09f7d6c](https://github.com/informatievlaanderen/streetname-registry/commit/09f7d6c20c611ff458290cdf8f6336fd76d0d984))

# [3.42.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.41.2...v3.42.0) (2024-02-14)


### Features

* add lastchangedlist console ([c98b052](https://github.com/informatievlaanderen/streetname-registry/commit/c98b052b5a92c76a762fa64c1ad5ccf40ae61441))

## [3.41.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.41.1...v3.41.2) (2024-02-13)


### Bug Fixes

* **bump:** ci ([1941f38](https://github.com/informatievlaanderen/streetname-registry/commit/1941f38c2c874bd09aba0c246a6147f0dbf7baeb))

## [3.41.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.41.0...v3.41.1) (2024-02-13)


### Bug Fixes

* **bump:** change cd test pipeline ([cc1f3f2](https://github.com/informatievlaanderen/streetname-registry/commit/cc1f3f229e29948bd45932c776f09cbb879c96f0))

# [3.41.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.40.1...v3.41.0) (2024-02-08)


### Bug Fixes

* push images to new ECR ([14fce7e](https://github.com/informatievlaanderen/streetname-registry/commit/14fce7e1269bfacea8b01414ae3e519d9e885cbe))


### Features

* add backoffice projections status ([4854a84](https://github.com/informatievlaanderen/streetname-registry/commit/4854a84db8d2f55171a35941bd1662823f4aec11))

## [3.40.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.40.0...v3.40.1) (2024-02-06)


### Bug Fixes

* bump projection handling ([f66a8d6](https://github.com/informatievlaanderen/streetname-registry/commit/f66a8d6f83b09e92ced3e94be760f15f6b268841))
* correct streetnamenames capitalize ([24af8b2](https://github.com/informatievlaanderen/streetname-registry/commit/24af8b294b324c42db8de9ddfc8df925ebd6c34c))
* integration projections ([f30afeb](https://github.com/informatievlaanderen/streetname-registry/commit/f30afeb71f34dae52cbec0153fc7e1e0ce3923a9))

# [3.40.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.39.0...v3.40.0) (2024-02-02)


### Features

* add cachevalidator lastchangedlist GAWR-5407 ([139ca2b](https://github.com/informatievlaanderen/streetname-registry/commit/139ca2b80b9f98b0c657c3ff4fb676cde935d069))

# [3.39.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.38.1...v3.39.0) (2024-01-29)


### Bug Fixes

* add objectid to idempotencekey snapshot oslo ([94e3109](https://github.com/informatievlaanderen/streetname-registry/commit/94e3109a1130b2dc820c6a3c9f1030a8a5e0eb57))


### Features

* add flemish region filter for list api GAWR-5205 ([8e6edb0](https://github.com/informatievlaanderen/streetname-registry/commit/8e6edb005f12b2cf3ce78dceeb80c184bb697260))

## [3.38.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.38.0...v3.38.1) (2024-01-17)


### Bug Fixes

* dont do rename municipality check on nulls ([77711a1](https://github.com/informatievlaanderen/streetname-registry/commit/77711a13578200cae119696c9811414cff7ed443))
* mapping integration projections ([59aa7e3](https://github.com/informatievlaanderen/streetname-registry/commit/59aa7e328b11a28fa6cbbebfe95b9c924cbb8685))

# [3.38.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.37.0...v3.38.0) (2024-01-16)


### Features

* add integration projections GAWR-4534 ([0fcfc06](https://github.com/informatievlaanderen/streetname-registry/commit/0fcfc06f6e52a7e0ff9a8d44531feb9e7faad8e6))

# [3.37.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.36.1...v3.37.0) (2024-01-16)


### Bug Fixes

* correct errorcode renamestreename GAWR-5382 ([5adad7c](https://github.com/informatievlaanderen/streetname-registry/commit/5adad7c9c567a279bc30f474802399c31be8e2e6))
* StreetNameWasRenamed docs ([2c9b5da](https://github.com/informatievlaanderen/streetname-registry/commit/2c9b5da42039fdc33c527ab9a7bc2640c22e19b0))


### Features

* add rename validation different municipalities GAWR-5383 ([4cac530](https://github.com/informatievlaanderen/streetname-registry/commit/4cac5300baf9e469c929d3eedfac2b1e74c82310))

## [3.36.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.36.0...v3.36.1) (2024-01-09)


### Bug Fixes

* command provenance should be set after idempotency check ([3f0003e](https://github.com/informatievlaanderen/streetname-registry/commit/3f0003ef02f4c69a8af26addf4057003c350af4d))

# [3.36.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.35.0...v3.36.0) (2024-01-09)


### Bug Fixes

* event timestamp ([1fec37c](https://github.com/informatievlaanderen/streetname-registry/commit/1fec37cd4e96b88cf29c71492d5ddcf5109aeda4))


### Features

* add idempotencykey ksql ([02aea13](https://github.com/informatievlaanderen/streetname-registry/commit/02aea13a85b3f6c2652af6b5a2ba4ef85c57c39c))
* add integrationdb ksql scripts ([23c9a7d](https://github.com/informatievlaanderen/streetname-registry/commit/23c9a7d7adf1e858103a6c245e75cffc32f4dc23))

# [3.35.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.34.0...v3.35.0) (2023-11-29)


### Features

* add rename streetname command ([19444af](https://github.com/informatievlaanderen/streetname-registry/commit/19444af4b8a6dad76293470a79f445d543327052))
* add rename streetname producers ([7da4376](https://github.com/informatievlaanderen/streetname-registry/commit/7da4376506f2159409560fb3fbc9f4e11fc63e72))
* add StreetNameWasRenamed event ([9415b78](https://github.com/informatievlaanderen/streetname-registry/commit/9415b78c794009595456f33018e245be521d767a))
* add StreetNameWasRenamed event ([7a1b2ba](https://github.com/informatievlaanderen/streetname-registry/commit/7a1b2bae1c1f10ee2f8389c99abf2fa0b5b5b2d9))
* rename streetname backoffice api ([d9e3425](https://github.com/informatievlaanderen/streetname-registry/commit/d9e3425bd13c5ba43e10518189e00d67f45790c8))
* rename streetname domain ([1a97b20](https://github.com/informatievlaanderen/streetname-registry/commit/1a97b2080ad502070516d6947bbaa757238b256f))
* rename streetname lambda ([1c8d9d4](https://github.com/informatievlaanderen/streetname-registry/commit/1c8d9d4e34847da6b61da431f317727a241ab6ee))
* rename streetname projections ([3b0a27e](https://github.com/informatievlaanderen/streetname-registry/commit/3b0a27e96bb0168ce9165d50545ba25200bbf96b))

# [3.34.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.33.1...v3.34.0) (2023-11-01)


### Bug Fixes

* mark removed migrated streetname instead of false ([17a6bb3](https://github.com/informatievlaanderen/streetname-registry/commit/17a6bb3a39c9d6ccd874d494010340a8aa40a665))
* migrations postal consumer ([8cad687](https://github.com/informatievlaanderen/streetname-registry/commit/8cad6876740748719090fe0a5c47c59c7fc2f1be))
* remove consumer read postal from deploy ([0508f83](https://github.com/informatievlaanderen/streetname-registry/commit/0508f834127d425143fd33391375a10332e7c56e))
* return empty list when no niscode found for postal code on streetname list ([29a766a](https://github.com/informatievlaanderen/streetname-registry/commit/29a766a001f8383cab8ec65c810e5d96fd9231d2))


### Features

* add postal read consumer ([eb8051c](https://github.com/informatievlaanderen/streetname-registry/commit/eb8051c05e1e42687a815dd37c3da7ac43dcafd4))
* implement postalcode filter on list streetname ([18cbbc8](https://github.com/informatievlaanderen/streetname-registry/commit/18cbbc8515536a52f07f273d829d94255307d47a))

## [3.33.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.33.0...v3.33.1) (2023-10-03)


### Bug Fixes

* add snapshot-verifier to release pipeline ([1939af1](https://github.com/informatievlaanderen/streetname-registry/commit/1939af1bc58e3f808d375998ea639873b4d2fc84))
* change snapshot verifier folder name ([6123584](https://github.com/informatievlaanderen/streetname-registry/commit/612358455108049a2358a2ec29e67955ad01115a))

# [3.33.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.32.0...v3.33.0) (2023-09-26)


### Features

* don't cache legacy GAWR-5195 ([13ebfe6](https://github.com/informatievlaanderen/streetname-registry/commit/13ebfe6858ce0a45c230e4226b9257de1996b3eb))

# [3.32.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.10...v3.32.0) (2023-09-21)


### Features

* add snapshot verifier ([d705da7](https://github.com/informatievlaanderen/streetname-registry/commit/d705da7507271a360b116050b31f75cc6d01d1e9))

## [3.31.10](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.9...v3.31.10) (2023-09-04)


### Bug Fixes

* bump lambda package ([29e7095](https://github.com/informatievlaanderen/streetname-registry/commit/29e709515d985cc9625f291a21618313b2071a2f))

## [3.31.9](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.8...v3.31.9) (2023-09-04)


### Bug Fixes

* bump lambda ([9e6ada9](https://github.com/informatievlaanderen/streetname-registry/commit/9e6ada942abadf6a8cc13b1b99ab01b858bf660d))

## [3.31.8](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.7...v3.31.8) (2023-09-01)


### Bug Fixes

* bump lambda package ([81e23c2](https://github.com/informatievlaanderen/streetname-registry/commit/81e23c29536c9bff4b2d53b12260f18b4d5c0719))

## [3.31.7](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.6...v3.31.7) (2023-09-01)


### Bug Fixes

* bump lambda ([a91f2b1](https://github.com/informatievlaanderen/streetname-registry/commit/a91f2b110ea36227f01331f34fb1a0e7a48b8f89))


### Performance Improvements

* recreate wms index to add include niscode ([1b881f9](https://github.com/informatievlaanderen/streetname-registry/commit/1b881f95d05d3adc7d5181b0b2218f57e8a7d1d0))

## [3.31.6](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.5...v3.31.6) (2023-08-21)


### Bug Fixes

* call cancel on lambda cancellationToken after 5 minutes ([16cefe0](https://github.com/informatievlaanderen/streetname-registry/commit/16cefe005f57564ef1fc8a4e14318fbc53269dd8))

## [3.31.5](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.4...v3.31.5) (2023-08-14)


### Bug Fixes

* style to trigger build ([544e135](https://github.com/informatievlaanderen/streetname-registry/commit/544e1355bcb9cf77773fa38fe71d2ef06c6023a9))

## [3.31.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.3...v3.31.4) (2023-08-11)


### Bug Fixes

* style to trigger build ([b305986](https://github.com/informatievlaanderen/streetname-registry/commit/b30598674bec697cca35e31e069ccad8e66f2dcb))

## [3.31.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.2...v3.31.3) (2023-08-08)


### Bug Fixes

* remove no longer needed cp lambda.zip parameters from release ([e00fbc9](https://github.com/informatievlaanderen/streetname-registry/commit/e00fbc954a35725bdad68df0daabe149ba33addd))

## [3.31.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.1...v3.31.2) (2023-08-08)


### Bug Fixes

* add ~/ before lambda.zip in push to s3 test step ([d1bdb03](https://github.com/informatievlaanderen/streetname-registry/commit/d1bdb0317d37946ad7667906d524c284914894d6))

## [3.31.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.31.0...v3.31.1) (2023-08-08)


### Bug Fixes

* trigger release ([a638998](https://github.com/informatievlaanderen/streetname-registry/commit/a638998854e4e1d28d957ec64f7d8dce1d8d8667))

# [3.31.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.30.3...v3.31.0) (2023-08-07)


### Bug Fixes

* remove elastic apm ([eb50358](https://github.com/informatievlaanderen/streetname-registry/commit/eb50358e0028a45e5e1a31fc1bd64a3542e24a12))


### Features

* add levenshtein distance calculation when correcting streetname ([44b8e6c](https://github.com/informatievlaanderen/streetname-registry/commit/44b8e6c8a2c4da379b31b929d6753d2a697c2b72))

## [3.30.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.30.2...v3.30.3) (2023-06-21)


### Bug Fixes

* naming producers migration ([af79840](https://github.com/informatievlaanderen/streetname-registry/commit/af79840b4b12c006cb0f5c67cd82d5a2b8e725f4))

## [3.30.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.30.1...v3.30.2) (2023-06-14)


### Bug Fixes

* producer naming ([9b54464](https://github.com/informatievlaanderen/streetname-registry/commit/9b54464d6e007b5208b2f3da1ca87c2cff1eee83))

## [3.30.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.30.0...v3.30.1) (2023-06-13)


### Bug Fixes

* add idempotent municipality-streetname relation ([172e7c0](https://github.com/informatievlaanderen/streetname-registry/commit/172e7c036f791888e3c938932c90cae24d7147df))

# [3.30.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.29.1...v3.30.0) (2023-06-12)


### Bug Fixes

* returning only date on status controller consumer ([adf637c](https://github.com/informatievlaanderen/streetname-registry/commit/adf637c64d630ece175ac0d371def41f9b8bcc0e))


### Features

* add consumer status endpoint in projector GAWR-4879 ([e60a907](https://github.com/informatievlaanderen/streetname-registry/commit/e60a907fbd31afe24dae9a39fe7236ceeba062bd))

## [3.29.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.29.0...v3.29.1) (2023-06-01)


### Bug Fixes

* to trigger build update CI newprd lambda ([b5dc250](https://github.com/informatievlaanderen/streetname-registry/commit/b5dc2500cfcc69e5ec945691f34ad3dc6ee2f2f3))

# [3.29.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.28.4...v3.29.0) (2023-05-30)


### Bug Fixes

* flaky test ([834d6ec](https://github.com/informatievlaanderen/streetname-registry/commit/834d6ec01936f975b1fe8765c5528ca11ae81b24))


### Features

* add migrate producer projections ([606caa4](https://github.com/informatievlaanderen/streetname-registry/commit/606caa457fdc3d2a6bde25449d1f1a2094dec990))

## [3.28.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.28.3...v3.28.4) (2023-05-28)


### Bug Fixes

* add concurrentdictionary in migrator ([12b2fc1](https://github.com/informatievlaanderen/streetname-registry/commit/12b2fc18064f2cd386422869ea29c1a04ff0ce30))

## [3.28.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.28.2...v3.28.3) (2023-05-28)


### Performance Improvements

* use dbcontextfactory in migrator ([3fd9faa](https://github.com/informatievlaanderen/streetname-registry/commit/3fd9faa90be0d3c02dfb9882a11a52756bbec976))

## [3.28.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.28.1...v3.28.2) (2023-05-27)


### Bug Fixes

* consumer group suffix ([a727ff2](https://github.com/informatievlaanderen/streetname-registry/commit/a727ff2d727873554b3451cdb47ffc731ee2417f))

## [3.28.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.28.0...v3.28.1) (2023-05-27)


### Performance Improvements

* use parallelization for migrator ([758c294](https://github.com/informatievlaanderen/streetname-registry/commit/758c294dbfd7ae9e022ff7f0aeab4356d124a429))

# [3.28.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.27.6...v3.28.0) (2023-05-26)


### Features

* add elasticapm in lambda ([42976d5](https://github.com/informatievlaanderen/streetname-registry/commit/42976d54cefbc5191107dad1cb863f0012a1705b))


### Performance Improvements

* improve municipality streetnames lookups ([6ebea98](https://github.com/informatievlaanderen/streetname-registry/commit/6ebea98f36190ea1b6ca79f939ac086eac551998))

## [3.27.6](https://github.com/informatievlaanderen/streetname-registry/compare/v3.27.5...v3.27.6) (2023-05-23)


### Bug Fixes

* update update niscode migration to -events database ([3832fe0](https://github.com/informatievlaanderen/streetname-registry/commit/3832fe0b4de7371c1cf4de31381c87fba4e23dc8))

## [3.27.5](https://github.com/informatievlaanderen/streetname-registry/compare/v3.27.4...v3.27.5) (2023-05-23)


### Bug Fixes

* add CI/CD pipeline new production ([bea389e](https://github.com/informatievlaanderen/streetname-registry/commit/bea389e1996dc0c4f0eae59a9b9581a5101dcbb2))

## [3.27.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.27.3...v3.27.4) (2023-05-10)


### Bug Fixes

* produce oslo snapshot based upon timestamp and etag ([6d31561](https://github.com/informatievlaanderen/streetname-registry/commit/6d31561d7ba63126c42784b3aff5993c126c5e7e))

## [3.27.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.27.2...v3.27.3) (2023-05-02)


### Bug Fixes

* change validation errorcodes so they are unique GAWR-4817 ([5ac96fc](https://github.com/informatievlaanderen/streetname-registry/commit/5ac96fc25f8498cac81ff6394d5f971b4f203fac))

## [3.27.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.27.1...v3.27.2) (2023-04-20)


### Bug Fixes

* extractv2 lookup projections ([a6ca17d](https://github.com/informatievlaanderen/streetname-registry/commit/a6ca17dce58199886026b2e81646d91ef5c73cbf))

## [3.27.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.27.0...v3.27.1) (2023-04-20)


### Bug Fixes

* find and update extractv2 ([ed0da3e](https://github.com/informatievlaanderen/streetname-registry/commit/ed0da3ea7ee9756e89a49b1cc4979c37f4d8aeb7))

# [3.27.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.26.1...v3.27.0) (2023-04-20)


### Features

* add creatieid to extract v2 ([159f0e0](https://github.com/informatievlaanderen/streetname-registry/commit/159f0e091fbbb501f4322563beecfb5f4c5bdf8a))
* add elastic apm ([1b0402a](https://github.com/informatievlaanderen/streetname-registry/commit/1b0402a17904fdd65b3be8afd9ffe6b4c2562abe))

## [3.26.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.26.0...v3.26.1) (2023-04-18)


### Bug Fixes

* niscode migration ([fdf755b](https://github.com/informatievlaanderen/streetname-registry/commit/fdf755b55097eb85c7a1108e77e81c0b87048ceb))

# [3.26.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.25.5...v3.26.0) (2023-04-18)


### Features

* add NisCode to BackOfficeContext ([8697c6b](https://github.com/informatievlaanderen/streetname-registry/commit/8697c6b121245e6f81d6c3e8912f5c6ce85e9c30))

## [3.25.5](https://github.com/informatievlaanderen/streetname-registry/compare/v3.25.4...v3.25.5) (2023-04-14)


### Bug Fixes

* valueobject Names to filter out empty values ([9bcf96e](https://github.com/informatievlaanderen/streetname-registry/commit/9bcf96ef6887f859aaa9c80e85014c0c45517d1f))

## [3.25.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.25.3...v3.25.4) (2023-04-13)


### Bug Fixes

* case insensitive streetname name and homonym addition comparison ([c193ce0](https://github.com/informatievlaanderen/streetname-registry/commit/c193ce0cd50269fe065e7ef5063be060c538d08b))
* don't put migrated removed streetname in extract ([4f72c6c](https://github.com/informatievlaanderen/streetname-registry/commit/4f72c6c791b6128d7a2e3cf19d373cac59897caa))

## [3.25.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.25.2...v3.25.3) (2023-04-11)


### Bug Fixes

* don't include in extract when removed ([fccaf7a](https://github.com/informatievlaanderen/streetname-registry/commit/fccaf7a19956608747a26be5c0aadbe0eaee2e51))

## [3.25.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.25.1...v3.25.2) (2023-04-04)


### Bug Fixes

* remove special characters from error message ([c7685d0](https://github.com/informatievlaanderen/streetname-registry/commit/c7685d09cccebe758e73def38b591c27c76351ab))

## [3.25.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.25.0...v3.25.1) (2023-03-16)


### Bug Fixes

* allow null on language for correct homonym addition request ([036925f](https://github.com/informatievlaanderen/streetname-registry/commit/036925fc98616790f71ba12fe4e8492f1c0a2c57))
* prevents null on correct streetnamename request ([0b94c83](https://github.com/informatievlaanderen/streetname-registry/commit/0b94c83e6df621f65a525e436994390b828cf71c))

# [3.25.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.24.0...v3.25.0) (2023-03-14)


### Features

* add example request for change streetname ([1b62060](https://github.com/informatievlaanderen/streetname-registry/commit/1b62060c9ef655cfccfb8a8dab384b1e92cad612))

# [3.24.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.23.2...v3.24.0) (2023-03-14)


### Bug Fixes

* comments pr ([9a0941a](https://github.com/informatievlaanderen/streetname-registry/commit/9a0941afb74c20e0742e4b0ff015f08abb59b22e))


### Features

* add change streetname domain ([9d9311c](https://github.com/informatievlaanderen/streetname-registry/commit/9d9311c3f777b541491f66cf37eef6719390aa9f))
* add integration tests for change streetnameÃ ([0449dc0](https://github.com/informatievlaanderen/streetname-registry/commit/0449dc0ece1b09504d438b397ab2e77eba319d9f))
* add StreetNameNamesWereChanged projections ([026ea9e](https://github.com/informatievlaanderen/streetname-registry/commit/026ea9ed88f43eaec7aad92ed88918740b513e7d))
* StreetNameNamesWereChanged backoffice ([f3abb9d](https://github.com/informatievlaanderen/streetname-registry/commit/f3abb9da8d17d805df77059f0f6d3eaf6ac68c71))

## [3.23.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.23.1...v3.23.2) (2023-03-13)


### Bug Fixes

* error code and message of homonymaddition Validations ([8b9e38c](https://github.com/informatievlaanderen/streetname-registry/commit/8b9e38c2a23d59ba08fc21e7cd185e94718657c8))

## [3.23.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.23.0...v3.23.1) (2023-03-07)


### Bug Fixes

* make producer reliable ([08c6287](https://github.com/informatievlaanderen/streetname-registry/commit/08c6287d82e662e7d929a1d97a51abc439488097))

# [3.23.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.22.1...v3.23.0) (2023-03-06)


### Features

* add CorrectStreetNameHomonymAdditionsRequest examples ([e8689a3](https://github.com/informatievlaanderen/streetname-registry/commit/e8689a3513d9b34191daef72343893d43e209fe8))
* remove streetname v2 ([974bed0](https://github.com/informatievlaanderen/streetname-registry/commit/974bed0cab79128f8e3ee97e562be92a744e0e29))

## [3.22.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.22.0...v3.22.1) (2023-03-01)


### Bug Fixes

* response examples ([b220b6f](https://github.com/informatievlaanderen/streetname-registry/commit/b220b6f62e6f825421ad14dbd5b42bc6d2f646e5))

# [3.22.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.21.1...v3.22.0) (2023-03-01)


### Bug Fixes

* no merge group for ksql ([839db65](https://github.com/informatievlaanderen/streetname-registry/commit/839db653199f69c348bed55b8f5d4c01ef0700d0))


### Features

* add v2 examples ([e1c487a](https://github.com/informatievlaanderen/streetname-registry/commit/e1c487ad545ee72a9882690af1a8445746f46a7d))

## [3.21.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.21.0...v3.21.1) (2023-02-27)


### Bug Fixes

* bump basisregisters-sqs ([647a906](https://github.com/informatievlaanderen/streetname-registry/commit/647a906e2706e4d84d4cad746d72e1346efd441e))
* bump grar common to 18.1.1 ([a57da16](https://github.com/informatievlaanderen/streetname-registry/commit/a57da16720f22f7d8eb56f69adead0e25a7d2bb5))
* bump MediatR ([eb41590](https://github.com/informatievlaanderen/streetname-registry/commit/eb41590d26bacaa7e71911e83d1577830319145f))
* remove ServiceFactory delegate ([5a1a226](https://github.com/informatievlaanderen/streetname-registry/commit/5a1a22622cadb002a98069097a3eec9e1b832145))

# [3.21.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.20.0...v3.21.0) (2023-02-24)


### Features

* correct homonymadditons api & lambda ([e451655](https://github.com/informatievlaanderen/streetname-registry/commit/e451655a9ff4c7f97704b1e43ede007a708ea8ca))

# [3.20.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.19.3...v3.20.0) (2023-02-24)


### Bug Fixes

* consumer should commit if message is already processed ([7690d91](https://github.com/informatievlaanderen/streetname-registry/commit/7690d91f38578db1d5789fcd6ada696d68bc80fe))
* remove valueobject from events ([a8257e5](https://github.com/informatievlaanderen/streetname-registry/commit/a8257e5ae7a606b16363593ff0c7163be7470923))


### Features

* correct streetname homonymadditions ([ab04c35](https://github.com/informatievlaanderen/streetname-registry/commit/ab04c35bbf228d307069f4ebc1538c465a9974c3))

## [3.19.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.19.2...v3.19.3) (2023-02-17)


### Bug Fixes

* registration sequence module ([eca9afb](https://github.com/informatievlaanderen/streetname-registry/commit/eca9afbd08851954a8752dddccc3fa3c0b97a85f))

## [3.19.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.19.1...v3.19.2) (2023-02-17)


### Bug Fixes

* consumer group name ([88bfc75](https://github.com/informatievlaanderen/streetname-registry/commit/88bfc75a40dceacafd45e3362ba0cefd916ac4b1))
* number ksql ([91e62f9](https://github.com/informatievlaanderen/streetname-registry/commit/91e62f953c3c6d1f344d56918b6a3372c74720e7))
* propose streetname lambda handler idempotency ([b36b858](https://github.com/informatievlaanderen/streetname-registry/commit/b36b85897f09969adf31ae5dad519a8efc2e37b2))
* use merge queue ([725dbc1](https://github.com/informatievlaanderen/streetname-registry/commit/725dbc11da190802bb78fa7866d959169bc395e6))
* yield persistentlocalid on propose streetname (idempotency) ([b94c722](https://github.com/informatievlaanderen/streetname-registry/commit/b94c72268a048897a8c5e9ee7dddba6cd5b6cd93))

## [3.19.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.19.0...v3.19.1) (2023-02-13)


### Bug Fixes

* catch aggregatenotfoundexception in backoffice controllers ([1293b83](https://github.com/informatievlaanderen/streetname-registry/commit/1293b83a5f6cf2437cd6c9bf86791675eac640ee))
* catch idempotencyexception on propose streetname ([7d0e9d8](https://github.com/informatievlaanderen/streetname-registry/commit/7d0e9d8a3b38bd295e4ffd4f2496159690fd0aeb))
* event jsondata should contain provenance ([70a84ba](https://github.com/informatievlaanderen/streetname-registry/commit/70a84ba103e04f54f775fd49b3f683c8fbf8283f))
* fix sonar security warning ([62489ed](https://github.com/informatievlaanderen/streetname-registry/commit/62489ed143eefedbadd680eb64fb553d1a078498))
* homonym addition duplicate streetname name validation ([#200](https://github.com/informatievlaanderen/streetname-registry/issues/200)) ([ccf03dc](https://github.com/informatievlaanderen/streetname-registry/commit/ccf03dc446739c03b02a31da72d6075bed3af0e4))
* remove persistentlocalid from proposestreetname identityfields ([ae16e49](https://github.com/informatievlaanderen/streetname-registry/commit/ae16e490744a5567e597de2511894657e6b821c4))
* use correct date in provenance for consumer command handling ([22d973c](https://github.com/informatievlaanderen/streetname-registry/commit/22d973c11722bae839688e1de6041b7fadc65e23))

# [3.19.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.18.3...v3.19.0) (2023-01-27)


### Bug Fixes

* review ioc registrations ([9854a48](https://github.com/informatievlaanderen/streetname-registry/commit/9854a4836ac9e2c6d8fa7a570becbc189d60f783))


### Features

* refactor error codes ([#185](https://github.com/informatievlaanderen/streetname-registry/issues/185)) ([71fcfbe](https://github.com/informatievlaanderen/streetname-registry/commit/71fcfbecd5fa2cabd83c568ff9be2366f58e31c7))

## [3.18.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.18.2...v3.18.3) (2023-01-20)


### Bug Fixes

* fix integration tests ([74d146d](https://github.com/informatievlaanderen/streetname-registry/commit/74d146d6287cffa6b0bd0bb58b51fd20d505a691))

## [3.18.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.18.1...v3.18.2) (2023-01-19)


### Bug Fixes

* bump to build ([0fcf76b](https://github.com/informatievlaanderen/streetname-registry/commit/0fcf76b5bd29abc22ca62b2e170ba448f23ea064))

## [3.18.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.18.0...v3.18.1) (2023-01-18)


### Bug Fixes

* don't test IntegrationTests ([f626ad9](https://github.com/informatievlaanderen/streetname-registry/commit/f626ad99bc0799006ac0f880ded5ac3d1ff40787))
* refine integration tests to be run once every day at 1:00 AM (which is 0:00 UTC) ([70c87d6](https://github.com/informatievlaanderen/streetname-registry/commit/70c87d638014033cd83dfea8e6dff15960f15b76))

# [3.18.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.17.1...v3.18.0) (2023-01-18)


### Bug Fixes

* 2 Authorize attributes ([208eea8](https://github.com/informatievlaanderen/streetname-registry/commit/208eea86cf7dcbb173e2e76a939b4bb778ed0aca))
* add environment variables to integration tests configuration & change build.yml ([4c4af4f](https://github.com/informatievlaanderen/streetname-registry/commit/4c4af4f265817afd0030c96a43c857107405530a))
* bump basisregisters-acmidm & remove unneeded dependencies ([66c5c10](https://github.com/informatievlaanderen/streetname-registry/commit/66c5c10cf15cd77ef547ad9aa225335fa658401b))
* remove acm-idm from Migrator.StreetName ([e028af2](https://github.com/informatievlaanderen/streetname-registry/commit/e028af2a325b1cc4c1d52623d128ee8a6482f7c7))
* remove JwtBearer from test references ([c826b3b](https://github.com/informatievlaanderen/streetname-registry/commit/c826b3baabe1c4efab7af9ae4aa91c7fdd6bf3a8))
* run integration tests during build & release; clean up integration tests -> test fixture ([f2bfdb2](https://github.com/informatievlaanderen/streetname-registry/commit/f2bfdb2bbafb1dbbf7a8f8906510ed6fbb1986f8))


### Features

* add acm idm integration test project ([73880d6](https://github.com/informatievlaanderen/streetname-registry/commit/73880d6eb459f366169925a51991c7741f5fb3c5))
* add acm/idm ([d0e3c5c](https://github.com/informatievlaanderen/streetname-registry/commit/d0e3c5caef3ee224fade75f038e8d2ee3aa86ebb))

## [3.17.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.17.0...v3.17.1) (2023-01-13)


### Bug Fixes

* test setassemblyversions ([3a4d3cb](https://github.com/informatievlaanderen/streetname-registry/commit/3a4d3cbbc7903fb8997e45e52cf12ad80256dac2))

# [3.17.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.16...v3.17.0) (2023-01-12)


### Features

* update Be.Vlaanderen.Basisregisters.Api to 19.0.1 ([41211f2](https://github.com/informatievlaanderen/streetname-registry/commit/41211f28f97a5de56ee9d72b8937f033c478664e))

## [3.16.16](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.15...v3.16.16) (2023-01-12)


### Bug Fixes

* use build-pipeline workflows ([dc01b07](https://github.com/informatievlaanderen/streetname-registry/commit/dc01b07c6986f3b7dd35cc268c1e1cbf724cc5a7))

## [3.16.15](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.14...v3.16.15) (2023-01-12)


### Bug Fixes

* test workflow in other repo ([eb2045f](https://github.com/informatievlaanderen/streetname-registry/commit/eb2045f78c153da23248e3a76e4dd6a655441519))

## [3.16.14](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.13...v3.16.14) (2023-01-11)


### Bug Fixes

* add deploy to release 2 ([37913ec](https://github.com/informatievlaanderen/streetname-registry/commit/37913ecd0d05968a055041d6442c2265ec21bd4c))

## [3.16.13](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.12...v3.16.13) (2023-01-11)


### Bug Fixes

* upload lambda/push images ([448f912](https://github.com/informatievlaanderen/streetname-registry/commit/448f9124bec762eaa913ed85346c3adc89573bba))

## [3.16.12](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.11...v3.16.12) (2023-01-11)


### Bug Fixes

* ci push images/lambda/nuget/atlassian ([94bb69f](https://github.com/informatievlaanderen/streetname-registry/commit/94bb69fa633a8668c57329dfebf90bb0d6261c20))

## [3.16.11](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.10...v3.16.11) (2023-01-11)


### Bug Fixes

* first try new release ([70100b4](https://github.com/informatievlaanderen/streetname-registry/commit/70100b41a6b9f4b50613985bf553d27848750931))
* same as previous ([3b8fd7a](https://github.com/informatievlaanderen/streetname-registry/commit/3b8fd7af2272c92e8668e87cf31bdbef9b43af80))

## [3.16.10](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.9...v3.16.10) (2023-01-11)


### Bug Fixes

* improve ci/cd performance ([f428207](https://github.com/informatievlaanderen/streetname-registry/commit/f42820763d95a350740832438f9c64936f795e8a))

## [3.16.9](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.8...v3.16.9) (2023-01-10)


### Bug Fixes

* finishing up arne release test ([1d217e7](https://github.com/informatievlaanderen/streetname-registry/commit/1d217e7b4e3575d2437eed20670aa3746bfbe9c7))

## [3.16.8](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.7...v3.16.8) (2023-01-10)


### Bug Fixes

* use caching instead of artifacts ([82a65bf](https://github.com/informatievlaanderen/streetname-registry/commit/82a65bfa3d7a0fd17ad35492ed61fdaf7974e43a))

## [3.16.7](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.6...v3.16.7) (2023-01-10)


### Bug Fixes

* ci try using cache ([6279956](https://github.com/informatievlaanderen/streetname-registry/commit/62799561ecef7c68194d42497e7f32c656add55d))

## [3.16.6](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.5...v3.16.6) (2023-01-09)


### Bug Fixes

* add checkout to ci upload artifact ([9f01a7e](https://github.com/informatievlaanderen/streetname-registry/commit/9f01a7e45eb9697da6fbbd9903ac2abb6f44fc61))

## [3.16.5](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.4...v3.16.5) (2023-01-09)


### Bug Fixes

* parallel publish / pack in build.fsx ([75af203](https://github.com/informatievlaanderen/streetname-registry/commit/75af2030abbeab8944560c059246d9e9d325e755))

## [3.16.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.3...v3.16.4) (2023-01-09)


### Bug Fixes

* ci build dependency step ([6691227](https://github.com/informatievlaanderen/streetname-registry/commit/669122777d09de0085acc4c97abed0d00fd053d2))

## [3.16.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.2...v3.16.3) (2023-01-09)


### Bug Fixes

* try ci improvement ([babe123](https://github.com/informatievlaanderen/streetname-registry/commit/babe12366a84b1b36a94e65740e5888be3aebd07))

## [3.16.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.1...v3.16.2) (2023-01-09)


### Bug Fixes

* ci parallel download / push artifact ([1c48d75](https://github.com/informatievlaanderen/streetname-registry/commit/1c48d7517014749e23b53c11adad98d5da0997c2))

## [3.16.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.16.0...v3.16.1) (2023-01-09)


### Bug Fixes

* bump build ([00d0bbb](https://github.com/informatievlaanderen/streetname-registry/commit/00d0bbb799bdab1869000ba6ccbb4f6b03b6a7b1))

# [3.16.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.15.1...v3.16.0) (2022-12-23)


### Features

* add backoffice projection ([426f787](https://github.com/informatievlaanderen/streetname-registry/commit/426f787d8320903a4381dc9444a2f2f59af21dd2))

## [3.15.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.15.0...v3.15.1) (2022-12-12)


### Bug Fixes

* hide ignore properties for docs ([0026ed0](https://github.com/informatievlaanderen/streetname-registry/commit/0026ed0f46dcecc9035e03fea4ad49d58d4ae9c8))
* registration mediatr ([ee5ce96](https://github.com/informatievlaanderen/streetname-registry/commit/ee5ce964e73e2eaffadad26fc4c8b5e815df22a4))

# [3.15.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.14.2...v3.15.0) (2022-12-09)


### Bug Fixes

* include production in release.yml ([81ed7ad](https://github.com/informatievlaanderen/streetname-registry/commit/81ed7adb5f760ee21ac9c1c90f32a61fcfb99222))
* reduce cognitive complexity ([6b7e509](https://github.com/informatievlaanderen/streetname-registry/commit/6b7e509ab6de0747dc196864d74e97c67a95b1e3))
* remove unused workflows ([1b30296](https://github.com/informatievlaanderen/streetname-registry/commit/1b3029603145cc54fda04f9e5fc8d929f9b7db59))
* use featuretoggle for oslo handlers ([26fa91c](https://github.com/informatievlaanderen/streetname-registry/commit/26fa91cdc596e2918cce5105e8469fce34519ada))
* use Oslo handlers ([afd0d31](https://github.com/informatievlaanderen/streetname-registry/commit/afd0d31b84eb618f66c8577b61ebb2502033393a))


### Features

* add mediatr in api.legacy ([6e75a60](https://github.com/informatievlaanderen/streetname-registry/commit/6e75a607cb905533491dc2222d9ad9a5654e386c))

## [3.14.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.14.1...v3.14.2) (2022-11-03)


### Bug Fixes

* update ci & test branch protection ([73ab3f7](https://github.com/informatievlaanderen/streetname-registry/commit/73ab3f799a8f5673a9e90d32955df5be7347efe9))
* use VBR_SONAR_TOKEN ([3fd83ab](https://github.com/informatievlaanderen/streetname-registry/commit/3fd83ab983716af6d49ba0ceceba5d8e774c3f85))

## [3.14.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.14.0...v3.14.1) (2022-11-02)


### Bug Fixes

* add coverage to build.yml ([ecad46d](https://github.com/informatievlaanderen/streetname-registry/commit/ecad46d6683be25cf85db408e64e5f787c9a8838))
* enable pr's & coverage ([fc2927b](https://github.com/informatievlaanderen/streetname-registry/commit/fc2927bdd577af6bac94b384b11f9300ec52f6a9))
* push images to production ([5ef767a](https://github.com/informatievlaanderen/streetname-registry/commit/5ef767ae166cd695f464e0830086b3f7fa2aa253))

# [3.14.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.9...v3.14.0) (2022-10-28)


### Bug Fixes

* revert set-output change ([7b2ea9d](https://github.com/informatievlaanderen/streetname-registry/commit/7b2ea9d2c741fbc59dac886d9de04d6ffe8dfe60))


### Features

* implement SqsLambdaHandlerBase ([80c44d4](https://github.com/informatievlaanderen/streetname-registry/commit/80c44d480b5c1283fe7fbf61b47d6573020795c2))

## [3.13.9](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.8...v3.13.9) (2022-10-25)


### Bug Fixes

* reduce cognitive complexity ([b63ccb1](https://github.com/informatievlaanderen/streetname-registry/commit/b63ccb118f4f0f0829aa9b868f35431f64233ccc))
* replace set-output (deprecated) ([d3c737e](https://github.com/informatievlaanderen/streetname-registry/commit/d3c737e76cbca79c994ba830a6cd2a947b9b92b8))

## [3.13.8](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.7...v3.13.8) (2022-10-25)


### Bug Fixes

* add ossf scorecards ([295ce4f](https://github.com/informatievlaanderen/streetname-registry/commit/295ce4fbdc4ff6a72db2cbf6eb8d8a3a82ebe0f4))
* fix Sonar bug ([d6229de](https://github.com/informatievlaanderen/streetname-registry/commit/d6229de520065ea4e98554e8120537ec888ace90))
* migrate to node16 (to avoid Github warnings) ([1e4a4fc](https://github.com/informatievlaanderen/streetname-registry/commit/1e4a4fc581a55b6c71747a4f9601e78b5d817e71))
* really final slack fix ([ee39135](https://github.com/informatievlaanderen/streetname-registry/commit/ee39135331d2f1333fc626a359ba5d2348356c97))
* remove unneeded registration ([af8641b](https://github.com/informatievlaanderen/streetname-registry/commit/af8641ba263ba55db5e1fed5187cc6b324bd2022))
* switch to manual scorecards run ([33f3550](https://github.com/informatievlaanderen/streetname-registry/commit/33f35505aa1f64eb7537ac258c4b5e1589afa7b0))
* update README.md ([fe648a6](https://github.com/informatievlaanderen/streetname-registry/commit/fe648a66d69bcba39e2f899207760440020775eb))

## [3.13.7](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.6...v3.13.7) (2022-10-23)


### Bug Fixes

* fix slack notification ([a140187](https://github.com/informatievlaanderen/streetname-registry/commit/a1401871e528ebc979aadc1cde7c36f6a9602da1))

## [3.13.6](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.5...v3.13.6) (2022-10-23)


### Bug Fixes

* fix slack ([b83b7aa](https://github.com/informatievlaanderen/streetname-registry/commit/b83b7aab71fdb740e0bc6205ae6b9c73667d875a))

## [3.13.5](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.4...v3.13.5) (2022-10-23)


### Bug Fixes

* fix loop ([b67f320](https://github.com/informatievlaanderen/streetname-registry/commit/b67f32064ed8a9720ea878e41bc9f0dfa9cbe64a))

## [3.13.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.3...v3.13.4) (2022-10-22)

## [3.13.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.2...v3.13.3) (2022-10-21)


### Bug Fixes

* fix slack notification & use secret for channel name ([0ef1236](https://github.com/informatievlaanderen/streetname-registry/commit/0ef12368cf47348a0a94eda18680d6488473236e))

## [3.13.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.1...v3.13.2) (2022-10-21)


### Bug Fixes

* add slack to release.yml ([f9281e6](https://github.com/informatievlaanderen/streetname-registry/commit/f9281e6d05b098647b753b5e23c02cf4dd9081e6))
* change channel id ([f03da0c](https://github.com/informatievlaanderen/streetname-registry/commit/f03da0c4a671ae44288fca627bdca79ea980fafe))
* fix channel id ([6026b9b](https://github.com/informatievlaanderen/streetname-registry/commit/6026b9b7a8017e0ad4645526cd38822e65689bc5))
* include token ([4c1b6f4](https://github.com/informatievlaanderen/streetname-registry/commit/4c1b6f4c672671e7746bb1205646d39f241b0206))
* use repository name ([71f37eb](https://github.com/informatievlaanderen/streetname-registry/commit/71f37ebdf8eac0f2879043123a65ad138b434fed))

## [3.13.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.13.0...v3.13.1) (2022-10-20)


### Bug Fixes

* interpolation of error ([a574aec](https://github.com/informatievlaanderen/streetname-registry/commit/a574aeca7d8e3e8f2b220e7bc38ebcf1d81817f9))
* remove unused assignments ([0664e85](https://github.com/informatievlaanderen/streetname-registry/commit/0664e85a70c37b3c7d495ebc6a9f7d745a7bbe3e))
* simplify loops ([edf5c1c](https://github.com/informatievlaanderen/streetname-registry/commit/edf5c1c6374a3781064f5ead7aa7831c48fd8cef))

# [3.13.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.12.6...v3.13.0) (2022-10-19)


### Features

* add ldes ([a7ac60f](https://github.com/informatievlaanderen/streetname-registry/commit/a7ac60fde1ceb871b2799ee8b7fd3150e41b8db3))

## [3.12.6](https://github.com/informatievlaanderen/streetname-registry/compare/v3.12.5...v3.12.6) (2022-10-18)


### Bug Fixes

* use retrypolicy from package ([5d5da47](https://github.com/informatievlaanderen/streetname-registry/commit/5d5da47c9322952e1cdacd3224ce425b0a60ebfd))

## [3.12.5](https://github.com/informatievlaanderen/streetname-registry/compare/v3.12.4...v3.12.5) (2022-10-18)


### Bug Fixes

* override InnerMapDomainException ([c020923](https://github.com/informatievlaanderen/streetname-registry/commit/c020923d4a6341df27b2d8e01ddc61c3c2fcdf07))
* remove lambda environments ([0f5f5bb](https://github.com/informatievlaanderen/streetname-registry/commit/0f5f5bb58b3507979d0a13b2fb0e10c637f4ec86))
* use BasisRegisters.Sqs package ([ef28bad](https://github.com/informatievlaanderen/streetname-registry/commit/ef28bade80c6d0a6018fc2393cc2bd12c5b28dc3))

## [3.12.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.12.3...v3.12.4) (2022-10-17)


### Bug Fixes

* don't need approval for lambda ([552790b](https://github.com/informatievlaanderen/streetname-registry/commit/552790be36fa6f84acfd06babdbb369a7e5243fc))

## [3.12.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.12.2...v3.12.3) (2022-10-14)


### Bug Fixes

* duplicate streetname correct retirement ([ce8f545](https://github.com/informatievlaanderen/streetname-registry/commit/ce8f54531a5447a70b2c9ed5d119461c7170a129))

## [3.12.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.12.1...v3.12.2) (2022-10-14)


### Bug Fixes

* handle SqsStreetNameCorrectRetirementRequest in lambda ([af4f4fd](https://github.com/informatievlaanderen/streetname-registry/commit/af4f4fd0baedf8b2c765064ac0ef06b2ed62b50f))

## [3.12.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.12.0...v3.12.1) (2022-10-14)


### Bug Fixes

* change error propose invalid municipality status ([0a40144](https://github.com/informatievlaanderen/streetname-registry/commit/0a4014453ea7a4bfe7f361b64b9936e8349ff6b2))

# [3.12.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.11.0...v3.12.0) (2022-10-12)


### Bug Fixes

* correct address approval missing validator ([4333f11](https://github.com/informatievlaanderen/streetname-registry/commit/4333f11154b58294cbe9422d498620a57c4789b3))


### Features

* correct retirement ([86d32f1](https://github.com/informatievlaanderen/streetname-registry/commit/86d32f16389545d912c884f5e6b24a5c97ec1940))
* correct streetname rejection ([23765f6](https://github.com/informatievlaanderen/streetname-registry/commit/23765f64ec6464dfd3ddbdccbef244344614e647))
* StreetNameWasCorrectedFromRetiredToCurrent backoffice ([61dcba2](https://github.com/informatievlaanderen/streetname-registry/commit/61dcba2d43661b25b87e8b93a938ef980c66d511))
* StreetNameWasCorrectedFromRetiredToCurrent projections ([015815f](https://github.com/informatievlaanderen/streetname-registry/commit/015815f000dca085e91aba6f9ff9c18b366511cb))

# [3.11.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.51...v3.11.0) (2022-10-11)


### Features

* correct streetname approval ([c7171b4](https://github.com/informatievlaanderen/streetname-registry/commit/c7171b4892d4fed8abaaf56eedb785f2a341ac2f))

## [3.10.51](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.50...v3.10.51) (2022-10-05)


### Bug Fixes

* clean up unneeded execution ([d3567f5](https://github.com/informatievlaanderen/streetname-registry/commit/d3567f5798f469cee670df659dffca01393e7176))
* comment production deploy (because S3 doesn't exist yet) ([db91ba1](https://github.com/informatievlaanderen/streetname-registry/commit/db91ba1d1e2827bce81fc7f5202ced9cf1b90bd9))

## [3.10.50](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.49...v3.10.50) (2022-10-05)


### Bug Fixes

* add production release to workflow ([f189d81](https://github.com/informatievlaanderen/streetname-registry/commit/f189d81a6799f01d7c18868f306a71347ac577c0))

## [3.10.49](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.48...v3.10.49) (2022-10-05)


### Bug Fixes

* Sonar issues ([c041116](https://github.com/informatievlaanderen/streetname-registry/commit/c041116336926db0828efa922136baa1d3fe387a))

## [3.10.48](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.47...v3.10.48) (2022-10-05)


### Bug Fixes

* make ExtractMetadataKeys static ([3f2eca9](https://github.com/informatievlaanderen/streetname-registry/commit/3f2eca94ca93960518e8fe4796f9dd07ac8f5d02))

## [3.10.47](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.46...v3.10.47) (2022-10-05)


### Bug Fixes

* fix serializable ([f3e50b3](https://github.com/informatievlaanderen/streetname-registry/commit/f3e50b33efcbe0a720a8386ddf9e59eb87e5be7c))
* fix test login ([df84c31](https://github.com/informatievlaanderen/streetname-registry/commit/df84c31c6fbf0942dd0dbfcec77c5fe1a87cd84b))

## [3.10.46](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.45...v3.10.46) (2022-10-04)


### Bug Fixes

* sqs handler action correctstreetnamenames GAWR-3771 ([09d9891](https://github.com/informatievlaanderen/streetname-registry/commit/09d98916fbe5e93a3a54f12908e3f44b4f2b9a51))

## [3.10.45](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.44...v3.10.45) (2022-10-04)


### Bug Fixes

* fix lambda job ([1950433](https://github.com/informatievlaanderen/streetname-registry/commit/1950433caad6db7916c525a5e5345bee4ddfa0c0))
* lambda to test ([b74aebb](https://github.com/informatievlaanderen/streetname-registry/commit/b74aebb40c4f71f776f844ee9b9d75c03a3f27aa))

## [3.10.44](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.43...v3.10.44) (2022-09-28)


### Bug Fixes

* AggregateIdIsNotFoundException should result in 404 ([931c317](https://github.com/informatievlaanderen/streetname-registry/commit/931c31735ddf9f7b183ab315c6c02ccaa4da0f5c))

## [3.10.43](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.42...v3.10.43) (2022-09-26)


### Bug Fixes

* remove datacontract ([c30947e](https://github.com/informatievlaanderen/streetname-registry/commit/c30947e8149aded6a52ed1360f518dabdcb15ec9))

## [3.10.42](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.41...v3.10.42) (2022-09-26)


### Bug Fixes

* streetNameRejectRequest serialization on sqs queue ([43cb46e](https://github.com/informatievlaanderen/streetname-registry/commit/43cb46eec276aad25ae2318293b8602d9fbbd882))

## [3.10.41](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.40...v3.10.41) (2022-09-26)


### Bug Fixes

* bump packages ([e2c1220](https://github.com/informatievlaanderen/streetname-registry/commit/e2c1220f65dc38a02914f653d2dd22e8b5144c96))
* public fields ([4b23e7c](https://github.com/informatievlaanderen/streetname-registry/commit/4b23e7cd9de266b2d09a726e2e7301a2abc5c974))

## [3.10.40](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.39...v3.10.40) (2022-09-26)


### Bug Fixes

* public field ([2410e78](https://github.com/informatievlaanderen/streetname-registry/commit/2410e78e8f0395f9320897f55db7984d9efbdff7))

## [3.10.39](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.38...v3.10.39) (2022-09-26)


### Bug Fixes

* quote github ([2a0350c](https://github.com/informatievlaanderen/streetname-registry/commit/2a0350c4189c15733f046ca7f3cd30b7e04dcc68))

## [3.10.38](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.37...v3.10.38) (2022-09-26)


### Bug Fixes

* dummy commit ([8922fb6](https://github.com/informatievlaanderen/streetname-registry/commit/8922fb6e33ad108f34268892e5165f41201709ae))

## [3.10.37](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.36...v3.10.37) (2022-09-26)


### Bug Fixes

* bump ticketing packages ([9c42614](https://github.com/informatievlaanderen/streetname-registry/commit/9c42614bacfe26232df10af94c5b49dbb05c1ebb))

## [3.10.36](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.35...v3.10.36) (2022-09-25)


### Bug Fixes

* use github token for github nupkg publish ([bc0d31f](https://github.com/informatievlaanderen/streetname-registry/commit/bc0d31fd63f1eff948b18fbba078cd9d828067d3))

## [3.10.35](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.34...v3.10.35) (2022-09-25)


### Bug Fixes

* separate github & nuget ([8096e6d](https://github.com/informatievlaanderen/streetname-registry/commit/8096e6d8989131a300a1c48fc2b5564fef7a6013))

## [3.10.34](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.33...v3.10.34) (2022-09-25)


### Bug Fixes

* nupkg to github ([2226333](https://github.com/informatievlaanderen/streetname-registry/commit/2226333c278a16127f83c46ca5eb1e93c2811103))

## [3.10.33](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.32...v3.10.33) (2022-09-25)


### Bug Fixes

* fix lambda ([314a74b](https://github.com/informatievlaanderen/streetname-registry/commit/314a74b52d879c4b3480154d0283d6d3a6d9b945))
* lambda deployment ([42da34a](https://github.com/informatievlaanderen/streetname-registry/commit/42da34a0fc14c40ac0b732a629028d464b7c6b9f))
* Nuget deploy to Github ([5dbd4e0](https://github.com/informatievlaanderen/streetname-registry/commit/5dbd4e00805e565dfccd3b7914da1f0572281b3c))

## [3.10.32](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.31...v3.10.32) (2022-09-25)


### Bug Fixes

* use dotnet nuget push ([9d9dae0](https://github.com/informatievlaanderen/streetname-registry/commit/9d9dae07f02d1130d3f295838a3f4b8ca735925a))

## [3.10.31](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.30...v3.10.31) (2022-09-25)


### Bug Fixes

* deploy to staging with protected environment ([e68361b](https://github.com/informatievlaanderen/streetname-registry/commit/e68361b9cef7befaea1f2d82861918322b28fa98))
* fix packaging settings ([12ac44e](https://github.com/informatievlaanderen/streetname-registry/commit/12ac44e0910c20b250086d1ffe2cb904f91da8db))

## [3.10.30](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.29...v3.10.30) (2022-09-25)


### Bug Fixes

* fix jira release & deploy to test ([48fd080](https://github.com/informatievlaanderen/streetname-registry/commit/48fd080f029419bade87b6bd1d2279a2448dfca8))

## [3.10.29](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.28...v3.10.29) (2022-09-23)


### Bug Fixes

* auto-deploy to test ([df7dabe](https://github.com/informatievlaanderen/streetname-registry/commit/df7dabe9aa06230af37bb130bee055b6528ea507))
* bump aws lambda to fix mapping problem ([a4a3563](https://github.com/informatievlaanderen/streetname-registry/commit/a4a356328471c5b9845ef61bbc34a226439d2d9f))
* bump version ([898f6d8](https://github.com/informatievlaanderen/streetname-registry/commit/898f6d8888a371599d97f2ab5a65d3c676c98082))
* bump version ([bdfc241](https://github.com/informatievlaanderen/streetname-registry/commit/bdfc241439e2b32d20a912b4a372fd7a382e79c4))
* configure services in lambda ([1efd342](https://github.com/informatievlaanderen/streetname-registry/commit/1efd3424fb1adb0b4db2c67e5e4583526a5e0dbc))

# [2.0.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.0.0...v2.0.0) (2022-09-23)


### Bug Fixes

* [#8](https://github.com/informatievlaanderen/streetname-registry/issues/8) + Volgende is now not emitted if null ([fe6eb46](https://github.com/informatievlaanderen/streetname-registry/commit/fe6eb4641a7799314114fdead6cce33c4e9f1317))
* add --no-restore & --no-build ([d21ca1f](https://github.com/informatievlaanderen/streetname-registry/commit/d21ca1f78ce43f0680fd5e6899f4ef1216099b3e))
* add build badge ([310bc9e](https://github.com/informatievlaanderen/streetname-registry/commit/310bc9eec2b32d47d21152aa8d385ea1a8af62b6))
* add Code to propose validators ([74b5706](https://github.com/informatievlaanderen/streetname-registry/commit/74b570639fc342e435f1720b7e8b9dac525db6f7))
* add consumer to connection strings ([c3caf10](https://github.com/informatievlaanderen/streetname-registry/commit/c3caf105b48caf0aadb71f27a988fc96def661da))
* add debug folders ([b3fdbce](https://github.com/informatievlaanderen/streetname-registry/commit/b3fdbce61f563f9f047b1c4132246cab369a554f))
* add deploy test ([0078ac8](https://github.com/informatievlaanderen/streetname-registry/commit/0078ac82a2a0bb3969a0994bf688b25155124a25))
* add docs ([91289b0](https://github.com/informatievlaanderen/streetname-registry/commit/91289b0a4737ed3aaebbcead50affd4f91a6c503))
* add etag to response header ([e1c135b](https://github.com/informatievlaanderen/streetname-registry/commit/e1c135b6038e161c4bffff4537f7efd01021adf1))
* add generator version GRAR-1540 ([b0ee494](https://github.com/informatievlaanderen/streetname-registry/commit/b0ee4942140665cf8f7e3d5dd9123e0ea96e5fb8))
* add LABEL to Dockerfile (for easier DataDog filtering) ([abc9b26](https://github.com/informatievlaanderen/streetname-registry/commit/abc9b2618b1a23ac866d3217a30ab5f26df4904a))
* add lambda's & nuget ([55c6169](https://github.com/informatievlaanderen/streetname-registry/commit/55c61697d3d08bb8b87cfe32748081ba42a01d88))
* add list index ([d71ffd5](https://github.com/informatievlaanderen/streetname-registry/commit/d71ffd5642db32a7e670a286a0ad20e4bdae9b01))
* add location to etag response ([3209e8e](https://github.com/informatievlaanderen/streetname-registry/commit/3209e8ec7c2531884901a398f5bbbde621a7f745))
* add make complete for incomplete streetnames in staging ([cda7a76](https://github.com/informatievlaanderen/streetname-registry/commit/cda7a76938b58018b4f290c0479d1c90693c80bc))
* add migration persistent local id's to backoffice ([2f5394f](https://github.com/informatievlaanderen/streetname-registry/commit/2f5394f5ffac923e2f99081ebf86e9f54d4f2b60))
* add missing files ([b2f3a48](https://github.com/informatievlaanderen/streetname-registry/commit/b2f3a48c62f18aa7c20c8c3c84dbf0738a7db56b))
* add missing mapping for street name status Rejected ([b6d5e58](https://github.com/informatievlaanderen/streetname-registry/commit/b6d5e5815eab625b81b5c3bcd9a8e8692c4d50ff))
* add municipality event tag on municipality events ([80eb619](https://github.com/informatievlaanderen/streetname-registry/commit/80eb6194cfcbc6ef02262da41366b6244340025a))
* add nis code filter ([97314f0](https://github.com/informatievlaanderen/streetname-registry/commit/97314f0010e489cf7188c04e75164e88ecee80a6))
* add order by in api's + add clustered index bosa ([29f401a](https://github.com/informatievlaanderen/streetname-registry/commit/29f401a10a8ef0be50a95b9cce9e49047285cb40))
* add paket install ([eb7d0ce](https://github.com/informatievlaanderen/streetname-registry/commit/eb7d0cec8672d6f21e5f45e0a50af157772c5666))
* add produce jsonld to totaal aantal ([a470bd7](https://github.com/informatievlaanderen/streetname-registry/commit/a470bd70a9855d7457fef0e6d0bc81337e86e7ff))
* add producer to CI/CD ([513835a](https://github.com/informatievlaanderen/streetname-registry/commit/513835aac1667d332825c431ecbc7abb4dc964c0))
* add property descriptions ([68569cb](https://github.com/informatievlaanderen/streetname-registry/commit/68569cb6d1e5b35cb9c6986207600fe11faff4e5))
* add rejected status and test with propose ([0c7c6ae](https://github.com/informatievlaanderen/streetname-registry/commit/0c7c6ae327074108122ccb93368cdf3cb7e68b50))
* add repo name ([2dc1f25](https://github.com/informatievlaanderen/streetname-registry/commit/2dc1f25b72df769a6772bb429299f62496dcee5c))
* add retirementdate to retire command ([ffc15ee](https://github.com/informatievlaanderen/streetname-registry/commit/ffc15ee32f2829e2ea25d0827b5d4164fa883e8b))
* add streetnamelist index ([6f4d034](https://github.com/informatievlaanderen/streetname-registry/commit/6f4d034a5717f59c512ef49984085a13035d22a9))
* add streetnamename index ([92c7faf](https://github.com/informatievlaanderen/streetname-registry/commit/92c7fafc4ad2698bfbf7d1caebc5eaf7664c048d))
* add streetnamesearch fields, migration ([9ce1064](https://github.com/informatievlaanderen/streetname-registry/commit/9ce1064bb100c3d55cd95ce74b90a34073a9321e))
* add syndication to api references ([d2c24de](https://github.com/informatievlaanderen/streetname-registry/commit/d2c24de75200b643bcb7b86eaa1c92921cf84488))
* add SyndicationItemCreatedAt GRAR-1442 ([284f5a2](https://github.com/informatievlaanderen/streetname-registry/commit/284f5a2755947f0b92f47f63508099782276f67e))
* add tags to new events ([2b342a6](https://github.com/informatievlaanderen/streetname-registry/commit/2b342a65122a29e7e959f317386ec72424eadc65))
* add Test to ECR ([05384ca](https://github.com/informatievlaanderen/streetname-registry/commit/05384ca6a2f7eaefd8fbd8c736aa1bc4be558c92))
* add type to problemdetails ([227a301](https://github.com/informatievlaanderen/streetname-registry/commit/227a301f99245d6209efde02943a5a48aebc1401))
* add unique constraint to persistentlocalid ([bf5d7f8](https://github.com/informatievlaanderen/streetname-registry/commit/bf5d7f85d73a4f6584af18f843e78538d767053c))
* adjust testje.yml ([b7e8574](https://github.com/informatievlaanderen/streetname-registry/commit/b7e85749e6af875a19619f5e0f327c120fc8d88f))
* auto-deploy to test ([df7dabe](https://github.com/informatievlaanderen/streetname-registry/commit/df7dabe9aa06230af37bb130bee055b6528ea507))
* bosa empty body does not crash anymore GR-855 ([c8aa3fd](https://github.com/informatievlaanderen/streetname-registry/commit/c8aa3fdef0a24cafb96a40e56a11c16874642515))
* bosa exact filter takes exact name into account ([0a06aa6](https://github.com/informatievlaanderen/streetname-registry/commit/0a06aa6dade3d85c005f706ba62035799ba9f321))
* bosa streetname version now offsets to belgian timezone ([7aad2cf](https://github.com/informatievlaanderen/streetname-registry/commit/7aad2cf1432b088d6dc16e878960689fb786f99e))
* build ([aea0de1](https://github.com/informatievlaanderen/streetname-registry/commit/aea0de11fe257bce51bf99b2e88044754e2cc4f5))
* build msil version for public api ([1e21df7](https://github.com/informatievlaanderen/streetname-registry/commit/1e21df71eaeb6f4dcca39aad47140155f35c3231))
* build test ([4227aa1](https://github.com/informatievlaanderen/streetname-registry/commit/4227aa1b6bedaddd105ddbb56ae2cf7f841d6644))
* bump api ([5dfd737](https://github.com/informatievlaanderen/streetname-registry/commit/5dfd7377b5137f9f1d7c56235d75a306b984622b))
* bump aws lambda to fix mapping problem ([a4a3563](https://github.com/informatievlaanderen/streetname-registry/commit/a4a356328471c5b9845ef61bbc34a226439d2d9f))
* bump grar-common and use event hash pipe & extension method ([9a75e99](https://github.com/informatievlaanderen/streetname-registry/commit/9a75e9990d4a749bc4c41a6f5beadedf486740d7))
* bump Kafka Simple ([00e49c2](https://github.com/informatievlaanderen/streetname-registry/commit/00e49c2693886e830be8aa9990a6b4bd0ce3505c))
* bump messagehandling and use queueurl instead of name ([4146838](https://github.com/informatievlaanderen/streetname-registry/commit/414683855c1e99ca5fdb5c4ee927aa7c88fcd58c))
* bump netcore 3.1.2 ([49b5880](https://github.com/informatievlaanderen/streetname-registry/commit/49b5880a76430f92439e746082f4e0f4d76f57b1))
* bump netcore dockerfiles ([e08f517](https://github.com/informatievlaanderen/streetname-registry/commit/e08f517685e81f055ac7c0ad4a82948766787bfe))
* bump packges and fix build issues after bump ([a9f8050](https://github.com/informatievlaanderen/streetname-registry/commit/a9f80504a45dcdb5aff928f9ff87a8784c0e404e))
* bump problemjson ([3af9a65](https://github.com/informatievlaanderen/streetname-registry/commit/3af9a65e7d565001efb509c7c7e56931cac849ce))
* bump problemjson again ([af9386b](https://github.com/informatievlaanderen/streetname-registry/commit/af9386bbd0039a27d7b7fecb4fa8e487d36a4c1e))
* bump ticketing / lambda packages ([b44eb77](https://github.com/informatievlaanderen/streetname-registry/commit/b44eb77c7e20399529ee08ed1d0a1863973c969b))
* bump version in backoffice to 2.0 ([77af2fe](https://github.com/informatievlaanderen/streetname-registry/commit/77af2fe3dea2f95eb5739f662dfd2b96fb254f0b))
* call cd test ([25f06b6](https://github.com/informatievlaanderen/streetname-registry/commit/25f06b63ea215d4009912fabd6eb7b2f7b8e80e1))
* call test.yml from release.yml ([89cf43f](https://github.com/informatievlaanderen/streetname-registry/commit/89cf43f994dd26de6d50606295fe946cdc368db3))
* can propose with retired duplicate name present GAWR-2843 ([47489d6](https://github.com/informatievlaanderen/streetname-registry/commit/47489d67de453154303c23c5f07b39c93b56eb20))
* change idempotency hash to be stable ([9cff84f](https://github.com/informatievlaanderen/streetname-registry/commit/9cff84ff2845aa0ee3315db711d9e176c008e86f))
* change MessageHandler & build scripts ([06720a6](https://github.com/informatievlaanderen/streetname-registry/commit/06720a6809927d23555361b6d2c859c34dd4ff40))
* change oslo context & type ([e6fefda](https://github.com/informatievlaanderen/streetname-registry/commit/e6fefdaa835596c32527713f4bd5754aeccb747b))
* change oslo context & type ([1193acc](https://github.com/informatievlaanderen/streetname-registry/commit/1193acc9fd77b3810189d6e0cd4b83e33c1d72f8))
* change tags language events GRAR-1898 ([ecadbe5](https://github.com/informatievlaanderen/streetname-registry/commit/ecadbe5c4966766062d529cb170eebe465f2341d))
* changes that wouldnt be added to commit ([f61f9d0](https://github.com/informatievlaanderen/streetname-registry/commit/f61f9d096868674230112bb8c48d979d2d840fcb))
* check removed before completeness GR-900 ([eb26fd4](https://github.com/informatievlaanderen/streetname-registry/commit/eb26fd489cb8800a610b45efe5f020006791960c))
* comment deployment to test ([402f0be](https://github.com/informatievlaanderen/streetname-registry/commit/402f0be916136de44d8aea03d12eb1d2ba4c81b6))
* comment lambda deployment ([ce2b75d](https://github.com/informatievlaanderen/streetname-registry/commit/ce2b75d21d3547542095a0134820bf50e7a65709))
* comment lambda deployment ([6da27e6](https://github.com/informatievlaanderen/streetname-registry/commit/6da27e61b66e4f7c40992165bcf21c36f3f5d035))
* comment lambda packaging ([1b6b324](https://github.com/informatievlaanderen/streetname-registry/commit/1b6b3249b32a1abdf43401126186ba3719ad49e9))
* configure baseurls for all problemdetails GRAR-1357 ([ee0043c](https://github.com/informatievlaanderen/streetname-registry/commit/ee0043c90c4ccda5761e54d65670cc482a5e6276))
* configure baseurls for all problemdetails GRAR-1358 GRAR-1357 ([6844438](https://github.com/informatievlaanderen/streetname-registry/commit/684443831e4996f2aa6486daff764c063935433e))
* configure services in lambda ([1efd342](https://github.com/informatievlaanderen/streetname-registry/commit/1efd3424fb1adb0b4db2c67e5e4583526a5e0dbc))
* consumer docker + assembly file ([f1ab955](https://github.com/informatievlaanderen/streetname-registry/commit/f1ab9552318270b8a078ba22c52e37c86ba99b16))
* copy correct repo ([69a609b](https://github.com/informatievlaanderen/streetname-registry/commit/69a609b7d864a7309f9a8d246a590610ffb7e05f))
* correct author, entry links atom feed + example GRAR-1443 GRAR-1447 ([0f040ee](https://github.com/informatievlaanderen/streetname-registry/commit/0f040eefa5065e2690255960e99d2f70ffa3a9d6))
* correct bosa exact search GR-857 ([ecded98](https://github.com/informatievlaanderen/streetname-registry/commit/ecded9869db8ae26fcb8d9d8e0215639c5307d2a))
* correct consumer non admin usage ([cad68d9](https://github.com/informatievlaanderen/streetname-registry/commit/cad68d9cc7af7b840ea7df3d24227692efb74d6c))
* correct datadog inits ([22fc3ec](https://github.com/informatievlaanderen/streetname-registry/commit/22fc3ec530219a043ad36c2cd1efd10373f92b71))
* correct docs with events ([1eefcf9](https://github.com/informatievlaanderen/streetname-registry/commit/1eefcf956b7a47a284e4aee88fd0c40343a74996))
* correct extract filename to Straatnaam.dbf ([bd920fa](https://github.com/informatievlaanderen/streetname-registry/commit/bd920fa724fe23a20bdd08c5dae8d1115ffa1d2c))
* correct handling removed status in extract ([01a4185](https://github.com/informatievlaanderen/streetname-registry/commit/01a4185f1402ee42542a2fdc1111f4759146a95d))
* correct homonymaddition for object in sync api GRAR-1626 ([d9d3e31](https://github.com/informatievlaanderen/streetname-registry/commit/d9d3e314b1c8d9d44f7c2ffe026054ed2a75ae05))
* correct idempotency in lambda handlers ([421e4bd](https://github.com/informatievlaanderen/streetname-registry/commit/421e4bd9496606249239a2043f811c9176d44ca8))
* correct merge statement in migration AddStatusList ([48342e9](https://github.com/informatievlaanderen/streetname-registry/commit/48342e9c24b3d21559b562d712d4eadcb766d49d))
* correct migration script GRAR-1442 ([70710cf](https://github.com/informatievlaanderen/streetname-registry/commit/70710cfaa9f932e0261878f29cc7e8944b60048e))
* correct migrations concerning existing indexes ([588a686](https://github.com/informatievlaanderen/streetname-registry/commit/588a686981938329b7b3a771d22f07066ff914a3))
* correct municipailty language for list streetnames GAWR-2970 ([46a5379](https://github.com/informatievlaanderen/streetname-registry/commit/46a53792c6f721e1484df58a86e9f15dc119b6d6))
* correct oslo id type for extract ([f735cd8](https://github.com/informatievlaanderen/streetname-registry/commit/f735cd87d25f2d0b5924b457016fbad0253e66e1))
* correct projections + tests ([ff3b298](https://github.com/informatievlaanderen/streetname-registry/commit/ff3b29818b80e80cc12e90875ffc5fdcf3e8c838))
* correct property description ([8c6bba3](https://github.com/informatievlaanderen/streetname-registry/commit/8c6bba31d2a1093439ee483dd6dbac4f651cc425))
* correct queue name ([ccaabd0](https://github.com/informatievlaanderen/streetname-registry/commit/ccaabd07bbf35cc797ad5d0c8d2dec90efdf0004))
* correct street name change order of validations ([f227404](https://github.com/informatievlaanderen/streetname-registry/commit/f227404fe3c2193b72afa417b82e91cf6a72983d))
* correct streetname names replace 204 response ([f0e67ee](https://github.com/informatievlaanderen/streetname-registry/commit/f0e67ee912409bc2e7ab6a5abbe5131c65f41e24))
* correct testje ([e024632](https://github.com/informatievlaanderen/streetname-registry/commit/e024632693c4b7456957763ea8d1a6d488306dfd))
* correct xml serialization ([e14f568](https://github.com/informatievlaanderen/streetname-registry/commit/e14f568ad8851f28377b8140879548957965f60d))
* correctly resume projections asnyc ([78b5f84](https://github.com/informatievlaanderen/streetname-registry/commit/78b5f84d1fa68408659cbad2771f64188d84d337))
* correctly setting primary language in sync projection ([825ba1a](https://github.com/informatievlaanderen/streetname-registry/commit/825ba1ad0d0b212cb34a49f42c302a8fd8c33fb7))
* count streetname now counts correctly when filtered ([313e952](https://github.com/informatievlaanderen/streetname-registry/commit/313e95205153880e14c62c7939df76615c6650eb))
* deploy lambda functions to test & stg environments ([86c103c](https://github.com/informatievlaanderen/streetname-registry/commit/86c103c28c44cebc0e43f6fa4fe9883751c4f18e))
* display municipality languages for bosa search ([755896a](https://github.com/informatievlaanderen/streetname-registry/commit/755896a50e2b1262e5bb6b4d45ae59bd4fd1c12b))
* display sync response example as correct xml GRAR-1599 ([6128480](https://github.com/informatievlaanderen/streetname-registry/commit/61284806d5c829ab1eeeccd4aa41d8005a014098))
* do not hardcode logging to console ([a214c59](https://github.com/informatievlaanderen/streetname-registry/commit/a214c59f14afcf8d29b15c782cf85ba37fb9fdd6))
* do not log to console write ([d67003b](https://github.com/informatievlaanderen/streetname-registry/commit/d67003bc269d29b5ca10b518afe845a3ab937eb5))
* docs on propose streetname ([8006132](https://github.com/informatievlaanderen/streetname-registry/commit/80061322dcf1dd77f0ee56fa232477e41bf7c2b0))
* don't run V2 of extract! ([0188666](https://github.com/informatievlaanderen/streetname-registry/commit/0188666f48b1882bc3264a332932718f61ebb74d))
* download & load images ([b1652fa](https://github.com/informatievlaanderen/streetname-registry/commit/b1652faf70e7a9e46f8f6a810388d60478d38e2b))
* duplicate items on publish ([aae6600](https://github.com/informatievlaanderen/streetname-registry/commit/aae6600d4e4f5cca05a6b46f4301095843ae1034))
* duplicate items on publish ([e781c55](https://github.com/informatievlaanderen/streetname-registry/commit/e781c55fabbb38963a998ef3b67121e23fa7558a))
* enums were not correctly serialized in syndication event GRAR-1490 ([107d1ac](https://github.com/informatievlaanderen/streetname-registry/commit/107d1ac9060be513aed2c8a2592368f61a0287d3))
* etag ([ee6287e](https://github.com/informatievlaanderen/streetname-registry/commit/ee6287e9a5092b150f61d030d237c039e4049afe))
* extract incomplete can happen after removed ([6f7b66d](https://github.com/informatievlaanderen/streetname-registry/commit/6f7b66d0b2cb6ba6f6a18b5bbdb3759f83056634))
* extract only exports completed items ([6baf2e9](https://github.com/informatievlaanderen/streetname-registry/commit/6baf2e9ccd695c6c387e09ed1706d74f38c96893))
* fake call ([5dba6c7](https://github.com/informatievlaanderen/streetname-registry/commit/5dba6c71edfaf2dacec516f8e1f93ecbd176ef5d))
* final test before calling cd test ([10fe5b5](https://github.com/informatievlaanderen/streetname-registry/commit/10fe5b5a0723d3f887e7017ab5541d85cb534adb))
* first implementation of retries ([e43146f](https://github.com/informatievlaanderen/streetname-registry/commit/e43146f7982de46cfdb9cf102ceff71f05b00ec8))
* fix build ([65c9f37](https://github.com/informatievlaanderen/streetname-registry/commit/65c9f37dd4d92d362568ae0a1b7d299d29c65498))
* fix container id in logging ([c40607b](https://github.com/informatievlaanderen/streetname-registry/commit/c40607b244175880ce880b261723c36b146a38be))
* fix contains search ([db2437c](https://github.com/informatievlaanderen/streetname-registry/commit/db2437c9efcf9aff5791fb0b28ce86da51586887))
* fix debug folders ([5b63af0](https://github.com/informatievlaanderen/streetname-registry/commit/5b63af0a49bb572aef67f0e51f593840855b2832))
* fix ENV ([6747d46](https://github.com/informatievlaanderen/streetname-registry/commit/6747d4678e3807707742566cd18dcc9af1a4f71c))
* fix env vars for tagging ([f8821f0](https://github.com/informatievlaanderen/streetname-registry/commit/f8821f02f8180bbc6af9b6df210217cdc0192790))
* fix image names ([81aa1fe](https://github.com/informatievlaanderen/streetname-registry/commit/81aa1fe39c2bc11f777293caba0551a575fce1ef))
* fix indentation ([47151ac](https://github.com/informatievlaanderen/streetname-registry/commit/47151ac9f9a44c7da8d3109cdd4a8143221a3acb))
* fix lambda destination in main.yml ([59abb7a](https://github.com/informatievlaanderen/streetname-registry/commit/59abb7a44f5228735a0afe9688f2f803e18b3d5f))
* fix logging for syndication ([6035e2d](https://github.com/informatievlaanderen/streetname-registry/commit/6035e2d4b35da66d7162fe29dccbf3316324d054))
* fix migrations extract ([8ca953b](https://github.com/informatievlaanderen/streetname-registry/commit/8ca953beb10d625ff9e2ce46d7ba4ba3db7c2281))
* fix niscode filter ([4f4550a](https://github.com/informatievlaanderen/streetname-registry/commit/4f4550a0f311ea406aef64aeb224028f74edd6a4))
* fix nuget ([ecbb6d7](https://github.com/informatievlaanderen/streetname-registry/commit/ecbb6d7f61585f61b984fa9c3b296b2506157734))
* fix nuget folder ([0db4964](https://github.com/informatievlaanderen/streetname-registry/commit/0db4964fc4d5f5979246ad27c7c7311103ea16cd))
* fix nuget-projector ([df7e276](https://github.com/informatievlaanderen/streetname-registry/commit/df7e2766ba7a43bcec84dcbff8915a7756e7e107))
* fix set version ([#670](https://github.com/informatievlaanderen/streetname-registry/issues/670)) ([f460e1e](https://github.com/informatievlaanderen/streetname-registry/commit/f460e1efc1f1cdc648425c65e63de1e5eb468bf0))
* fix starting Syndication projection ([46788bc](https://github.com/informatievlaanderen/streetname-registry/commit/46788bc18e8c1fa4c94de4e8cc58dc6a681cee25))
* fix swagger ([43c2f7e](https://github.com/informatievlaanderen/streetname-registry/commit/43c2f7eefe98c247bc001c8a8f9ba1729f995fad))
* fix typo ([631ac7c](https://github.com/informatievlaanderen/streetname-registry/commit/631ac7cfefca2b75e1d45df57d192202a3624b56))
* force build ([f2b6b2c](https://github.com/informatievlaanderen/streetname-registry/commit/f2b6b2c2be4a5a27c2faee1cb9f3213938eb4fcc))
* force version bump ([d6acf8a](https://github.com/informatievlaanderen/streetname-registry/commit/d6acf8a9df8a609d0c9f56a6f9f3b09ac9387220))
* gawr-2202 add api documentation ([1ac30e4](https://github.com/informatievlaanderen/streetname-registry/commit/1ac30e4ee381d13cb040f813a67b624415e975fb))
* gawr-2202 paket bump ([3a3add5](https://github.com/informatievlaanderen/streetname-registry/commit/3a3add50ca028da5a93b781ffb032835d2350b7f))
* gawr-2502 docs ([e287aa3](https://github.com/informatievlaanderen/streetname-registry/commit/e287aa3e767ef385a7148a5a77fa191fd4a5ac02))
* gawr-2502 docs ([9af3e50](https://github.com/informatievlaanderen/streetname-registry/commit/9af3e50e0385021bdbc07a9072d929b4ed0fbf5b))
* gawr-611 fix exception detail ([49b97ad](https://github.com/informatievlaanderen/streetname-registry/commit/49b97ad3b69df3dc4e3901034d272f9988752185))
* gawr-618 voorbeeld straatnaam id sorteren ([7b16cb3](https://github.com/informatievlaanderen/streetname-registry/commit/7b16cb3949c8494f46fd7be4b10923258944636e))
* gawr-626 change doc language ([688e93c](https://github.com/informatievlaanderen/streetname-registry/commit/688e93cff76d2d32d293e1f16db4ff15df8c306d))
* GAWR-666 bump problemjson header package ([201cf75](https://github.com/informatievlaanderen/streetname-registry/commit/201cf75da75fe61924ac8edb07652288c15fcac2))
* get api's working again ([52c9edf](https://github.com/informatievlaanderen/streetname-registry/commit/52c9edf926886b66d5d34946f2d0e514d8e4c7f7))
* get updated value from projections GRAR-1442 ([9e19a4d](https://github.com/informatievlaanderen/streetname-registry/commit/9e19a4dcd8a623655f8f8f7899968947b00fb62a))
* give the correct name of the event in syndication ([7f70d04](https://github.com/informatievlaanderen/streetname-registry/commit/7f70d04e2a1d00515d41529e11d23343ac197493))
* got migrator working ([ab0d739](https://github.com/informatievlaanderen/streetname-registry/commit/ab0d739724df0126c1279200ea5b63644d84c241))
* grawr-615 versionid offset +2 ([07ad035](https://github.com/informatievlaanderen/streetname-registry/commit/07ad03599bb7cc76a1f1091b580751057feb9661))
* hide municipality events ([78cb8ff](https://github.com/informatievlaanderen/streetname-registry/commit/78cb8ff474593f4c074d1fb51e9ccd1847e8eae8))
* homoniemToevoeging can be null ([6eb91c8](https://github.com/informatievlaanderen/streetname-registry/commit/6eb91c8c1fa61e229472fbb98b3446e11a5ce757))
* implement municipality streetname events ([dfc30c9](https://github.com/informatievlaanderen/streetname-registry/commit/dfc30c9176c54706adf02fcb44e66c3f06734ac4))
* import municipality ([5f0e7ad](https://github.com/informatievlaanderen/streetname-registry/commit/5f0e7ad87ce4d5daaf9dd3b96d226ebf13ebe262))
* include PersistentLocalId in ProposeStreetName command ([0c4fddc](https://github.com/informatievlaanderen/streetname-registry/commit/0c4fddcac92d00473093effec0406f05f9e83f93))
* increase bosa result size to 1001 ([ea102c3](https://github.com/informatievlaanderen/streetname-registry/commit/ea102c3f8c5e41dd2ca0687ff694cdf0c93b69a8))
* initial jira version ([3a58880](https://github.com/informatievlaanderen/streetname-registry/commit/3a58880ae2dbcd59fce8becbce407e2791e31342))
* install build pipeline before publishing nuget ([aad9502](https://github.com/informatievlaanderen/streetname-registry/commit/aad9502f22739f6dbd9b2df197078b897bbc7857))
* install npm packages ([954896d](https://github.com/informatievlaanderen/streetname-registry/commit/954896d81cd98696ccf2543c0739dcf3471b21a7))
* instance uri for error examples now show correctly ([6da02d0](https://github.com/informatievlaanderen/streetname-registry/commit/6da02d0f74a457e04901420b24c99ddece9e2957))
* JSON default value for nullable fields ([0e297d5](https://github.com/informatievlaanderen/streetname-registry/commit/0e297d5e678f75f135969cbb304635165ba0b3d4))
* lambda executable ([d162001](https://github.com/informatievlaanderen/streetname-registry/commit/d16200139aa003f92d109a1a6b1039b3ccb41d2c))
* lambda settings ([207cf0e](https://github.com/informatievlaanderen/streetname-registry/commit/207cf0e6e591990d4caa674e7f35ba6f065862a9))
* legacy syndication now subsribes to OsloIdAssigned ([42f0f49](https://github.com/informatievlaanderen/streetname-registry/commit/42f0f4996370bb5ad14289256fc9acd6130109a5))
* list now displays correct homonym addition in german & english ([59925af](https://github.com/informatievlaanderen/streetname-registry/commit/59925af89b1628ff5a5a66ee1c53f2cd85ed38d9))
* list now displays name of streetnames correctly ([d02b6d2](https://github.com/informatievlaanderen/streetname-registry/commit/d02b6d2c48e2841c62550755c478978ae4de49a4))
* logging ([cacf938](https://github.com/informatievlaanderen/streetname-registry/commit/cacf9388d5279e33805a6836164fe59650e2bf9b))
* logging ([655a5e3](https://github.com/informatievlaanderen/streetname-registry/commit/655a5e3078da70356e79a8e3cbb0ae68178736e9))
* logging ([73b7615](https://github.com/informatievlaanderen/streetname-registry/commit/73b76157dc67ae7da85419e7bae72f11948c9fff))
* logging ([93697bc](https://github.com/informatievlaanderen/streetname-registry/commit/93697bcb4a0641c2d17d1cd6f70873e1be573022))
* logging ([d9e7321](https://github.com/informatievlaanderen/streetname-registry/commit/d9e73215440833c0b24410a91ba0882756a3e696))
* logging ([e96a710](https://github.com/informatievlaanderen/streetname-registry/commit/e96a7108434d370bcb3ff9cf69139034a3872a21))
* make datadog tracing check more for nulls ([b202f8c](https://github.com/informatievlaanderen/streetname-registry/commit/b202f8cac989d58841fecc7020f3ab34aa9d1eec))
* make properties required ([f28f669](https://github.com/informatievlaanderen/streetname-registry/commit/f28f669848021d864361c93110e3ab3db6082d9e))
* make test.yml callable from another workflow ([ca8d253](https://github.com/informatievlaanderen/streetname-registry/commit/ca8d2534937be05f69cd9b39f4fd928829f90b01))
* migrations history table for syndication ([f78cd51](https://github.com/informatievlaanderen/streetname-registry/commit/f78cd519320261bed23d4096c98c0b9c6781c452))
* modify Consumer paket.template ([dbaf823](https://github.com/informatievlaanderen/streetname-registry/commit/dbaf8234a8ff959affa1480f0a42555be4985ee1))
* modify Consumer paket.template yet again ([62e8482](https://github.com/informatievlaanderen/streetname-registry/commit/62e84821551aa167152eb5d8af6032f17e466e97))
* modify deploy test ([e35ac52](https://github.com/informatievlaanderen/streetname-registry/commit/e35ac529c34624fc43649d7bf6e7fa77f3cbcb60))
* modify deploy test ([eaf0f5f](https://github.com/informatievlaanderen/streetname-registry/commit/eaf0f5fcf32e4e7a65b89aa35cb2e21b7e854f30))
* move cd test into testje ([b9ea49f](https://github.com/informatievlaanderen/streetname-registry/commit/b9ea49f65a5f9bb5930d26af67b11e57ba182022))
* move to 3.1.4 and gh-actions ([59f5c6c](https://github.com/informatievlaanderen/streetname-registry/commit/59f5c6c8c4841802ecb0f3ac7b250bf3a18e3d58))
* move to 3.1.5 ([db00db5](https://github.com/informatievlaanderen/streetname-registry/commit/db00db59b5ba330e57ffe54e6e86abedfa68ad44))
* move to 3.1.6 ([abfc092](https://github.com/informatievlaanderen/streetname-registry/commit/abfc092d0447a486160f1afba08a91ce7895c2bc))
* move to 3.1.8 ([d8dd4ac](https://github.com/informatievlaanderen/streetname-registry/commit/d8dd4ac94189b23627826c07dfaf90e40dd3a4df))
* move to 5.0.1 ([c5d4b92](https://github.com/informatievlaanderen/streetname-registry/commit/c5d4b92787a47d6805121e7e6568b83b1b1fef01))
* move to 5.0.2 ([d60e19b](https://github.com/informatievlaanderen/streetname-registry/commit/d60e19be61ffa7285edd7f5630fdd3650c38821b))
* move to 5.0.6 ([ca8c146](https://github.com/informatievlaanderen/streetname-registry/commit/ca8c146ac2d8ca6f2bd33c2b6ca23918635f0d9a))
* name WFS adressen & name WMS adressen ([85f843d](https://github.com/informatievlaanderen/streetname-registry/commit/85f843dddc02f999de7f812bc0ddfaaeb83e53d3))
* next url is nullable ([ac03c71](https://github.com/informatievlaanderen/streetname-registry/commit/ac03c718fcf7a5e22428526f1c9dbfeb2ff8cdee))
* no need to check since we used to do .Value ([72dd538](https://github.com/informatievlaanderen/streetname-registry/commit/72dd53835d5fa96d8a5cf93972896ec10c2e5807))
* now compiles importer after package update ([78067b0](https://github.com/informatievlaanderen/streetname-registry/commit/78067b0e90e552f0fcb87eeedabc2ccaa057c959))
* nuget & staging ([ce26776](https://github.com/informatievlaanderen/streetname-registry/commit/ce267761e6a76638aaca99845e609f93fd67a716))
* null organisation defaults to unknown ([9395ebb](https://github.com/informatievlaanderen/streetname-registry/commit/9395ebbc768247904b188db2f337c46738e4cbf4))
* optimise catchup mode for versions ([4583327](https://github.com/informatievlaanderen/streetname-registry/commit/458332775ed2f58c776391bd820cd2da8ea0ffcf))
* oslo id and niscode in sync werent correctly projected ([32d9ee8](https://github.com/informatievlaanderen/streetname-registry/commit/32d9ee87c578bddd36cba2713355f9cda8c685ca))
* properly report errors ([b1d02cf](https://github.com/informatievlaanderen/streetname-registry/commit/b1d02cfddfe69dea237022d194cc95f047517daa))
* properly serialise rfc 3339 dates ([abd5daf](https://github.com/informatievlaanderen/streetname-registry/commit/abd5daf841294bf0995a1ba3a9f6490e66a30ffa))
* propose streetname validate on existing persistent local id ([437cc7a](https://github.com/informatievlaanderen/streetname-registry/commit/437cc7a406cb04d428e3aa9db8066d898d42f8ee))
* propose validation messages in NL ([80c45e3](https://github.com/informatievlaanderen/streetname-registry/commit/80c45e3336456eea0e3e195d3fb895594100bc21))
* push to correct docker repo ([a2d4d11](https://github.com/informatievlaanderen/streetname-registry/commit/a2d4d1109a732894af36dd9edfadab2fdd48a977))
* rebuild key and uri for v2 insert events ([e23f63c](https://github.com/informatievlaanderen/streetname-registry/commit/e23f63ce2d1e96e3e2a9c7bcb97cc139971f2d49))
* redirect sonar to /dev/null ([07296f8](https://github.com/informatievlaanderen/streetname-registry/commit/07296f8e6ccea6f605ee9c65a3440b6afc3cf3dd))
* reference correct packages for documentation ([7d28cd6](https://github.com/informatievlaanderen/streetname-registry/commit/7d28cd64f53cfbe25762adc4da0ec06947af2aae))
* register problem details helper for projector GRAR-1814 ([1dac227](https://github.com/informatievlaanderen/streetname-registry/commit/1dac227c8373885aaec6988f53b66eea390bb221))
* registration of httpproxy (ticketing) ([b3f0ae1](https://github.com/informatievlaanderen/streetname-registry/commit/b3f0ae1f3029bbd0a940b2fb4d50354e9e1b5df9))
* remade wms/wfs migrations cause of identity ([15ad445](https://github.com/informatievlaanderen/streetname-registry/commit/15ad445c07263e004eae9c9d7dc916f8320d34ec))
* remove --no-logo ([25d8fa6](https://github.com/informatievlaanderen/streetname-registry/commit/25d8fa6954800fab6b35cb230ee26a25a3e9f237))
* remove .Complete ([c081c8a](https://github.com/informatievlaanderen/streetname-registry/commit/c081c8a9f9d1827e9d14acdeb04e264c3374d1b7))
* remove identity insert for wfs/wms v2 ([b0f41b4](https://github.com/informatievlaanderen/streetname-registry/commit/b0f41b483f370de7478a1deeefb1984a6f93028d))
* remove Modification from xml GRAR-1529 ([4b85dc7](https://github.com/informatievlaanderen/streetname-registry/commit/4b85dc768d91b086bc500c0ae2fd5edec8f79733))
* remove offset and add from to next uri GRAR-1418 ([b1669ad](https://github.com/informatievlaanderen/streetname-registry/commit/b1669ade3342a1fbed32ab1c0affa0278b429936))
* remove pushing nuget to github ([ed32b1e](https://github.com/informatievlaanderen/streetname-registry/commit/ed32b1e78cf1e83f39ca55491b402b875fd8f88f))
* remove ridingwolf, collaboration ended ([efe6fe3](https://github.com/informatievlaanderen/streetname-registry/commit/efe6fe337ef56c4d86ae2fd98eba61b561ddb333))
* remove set-env usage in gh-actions ([a7ef9ea](https://github.com/informatievlaanderen/streetname-registry/commit/a7ef9eabde5230de613d16fe619f7415da06c33c))
* remove streetname versions GRAR-1876 ([df2ea71](https://github.com/informatievlaanderen/streetname-registry/commit/df2ea71a701be229759d76b89902f8e12f4dccfb))
* remove sync alternate links ([5982eb7](https://github.com/informatievlaanderen/streetname-registry/commit/5982eb7eaf1eaa25c675c20eecbd8648b58656f7))
* remove unneeded streetnamename indexes ([5067563](https://github.com/informatievlaanderen/streetname-registry/commit/5067563fecc26eb85891a046c1571537f6699862))
* remove user secrets ([65c8977](https://github.com/informatievlaanderen/streetname-registry/commit/65c8977db4c452959ab05911da2effce8016c940))
* remove Value from StreetNameWasMigratedToMunicipality.Status ([82287e0](https://github.com/informatievlaanderen/streetname-registry/commit/82287e0d1b9a5415de539117e59470c386423d29))
* removed streetname doesn't crash remove status event in extract ([069270e](https://github.com/informatievlaanderen/streetname-registry/commit/069270e196e6bcbd7bb372dfaee3a6517d99786b))
* rename cache status endpoint in projector ([367fddb](https://github.com/informatievlaanderen/streetname-registry/commit/367fddb8ca95b063a201f833b579cd0b6eeea7c9))
* rename from CI2 to Build ([4dc4ae6](https://github.com/informatievlaanderen/streetname-registry/commit/4dc4ae632409cd9b01701a4cb21b02d5efccee17))
* rename oslo contracts ([f11b647](https://github.com/informatievlaanderen/streetname-registry/commit/f11b647af9c68b32eb93b25dad741ac914573d0a))
* rename oslo example classes ([9daa1ea](https://github.com/informatievlaanderen/streetname-registry/commit/9daa1eaf4247b737878b68ac3fc3e587dc08d42f))
* rename oslo query & response classes ([1bcdf4d](https://github.com/informatievlaanderen/streetname-registry/commit/1bcdf4d95049b00c722cd51c1bc1b7cde71bba3a))
* rename projection description ([0f518fb](https://github.com/informatievlaanderen/streetname-registry/commit/0f518fb22fcbee75d55fa1328f856d5d94fe5eb3))
* rename testje ([bad685d](https://github.com/informatievlaanderen/streetname-registry/commit/bad685df3ecbe63241b2de6249d35dcb7eee3de0))
* replace 409 by 400 on reject and retire streetname ([5225097](https://github.com/informatievlaanderen/streetname-registry/commit/52250972650c846a3fc3c896b2cea0295db40587))
* replace internal ticket url with public url ([debf772](https://github.com/informatievlaanderen/streetname-registry/commit/debf77257f28716ca22c0117565f9cdfdb8c95a8))
* replaced contextobject in responses with perma link ([e72c688](https://github.com/informatievlaanderen/streetname-registry/commit/e72c688a7b754ac31413ee8e73cefbf480113e46))
* report correct version number ([c509492](https://github.com/informatievlaanderen/streetname-registry/commit/c509492d5da20ac0e40006af95823789171dca76))
* required upgrade for datadog tracing to avoid connection pool problems ([432dbb4](https://github.com/informatievlaanderen/streetname-registry/commit/432dbb4dbfd71a4fb6e91edc69a9ea5991e865c6))
* resume projections on startup ([1e9190a](https://github.com/informatievlaanderen/streetname-registry/commit/1e9190a0676dfdb59d34d31ac288de9b381adb16))
* return empty response when request has invalid data GR-856 ([c18b134](https://github.com/informatievlaanderen/streetname-registry/commit/c18b134e7d0b24da3abc8220b1ae3cf6821331e2))
* revert make pr's trigger build ([b3f6c27](https://github.com/informatievlaanderen/streetname-registry/commit/b3f6c27d5ab1f786ed35ada1fc23e4a2aeb5067a))
* review ([daad2d7](https://github.com/informatievlaanderen/streetname-registry/commit/daad2d70889591114a5bee987921c71a6c6d56bb))
* run CI only on InformatiaVlaanderen repo ([f4cd78e](https://github.com/informatievlaanderen/streetname-registry/commit/f4cd78e208fad6ff930d66c4f24fdfeabaf12b5e))
* run projection using the feedprojector GRAR-1562 ([23a551a](https://github.com/informatievlaanderen/streetname-registry/commit/23a551a57ac4995d46e07d5c22744b0ddc82152c))
* run sonar end when release version != none ([35b3d9d](https://github.com/informatievlaanderen/streetname-registry/commit/35b3d9d79470b6cf56c15cdf8c2b1abeb0eba40e))
* run wfs/wms v1 in v2 for testing ([77cb91d](https://github.com/informatievlaanderen/streetname-registry/commit/77cb91d6d889213e095b875f76a9b12dfdacf65e))
* separate restore packages ([f3b80ce](https://github.com/informatievlaanderen/streetname-registry/commit/f3b80ce0be202acc47ee7ba75bc02f6056b56023))
* separate restore packages ([4dca151](https://github.com/informatievlaanderen/streetname-registry/commit/4dca1511b0ec42bfe5e4ae08a17e59014a219006))
* set build.yml as CI workflow ([beb0860](https://github.com/informatievlaanderen/streetname-registry/commit/beb0860a767e2bf505794301c41d881625be944a))
* set ifmatchheader on sqsrequest ([55fa162](https://github.com/informatievlaanderen/streetname-registry/commit/55fa162b86658579ccd58385417d1e525e990e5b))
* set kafka username/pw for producer ([20128a5](https://github.com/informatievlaanderen/streetname-registry/commit/20128a57616639b1d8782a3f1acacd1f54f005ec))
* set missing persistent local id on sqs request ([3b03f58](https://github.com/informatievlaanderen/streetname-registry/commit/3b03f58ecb74c423a1d1e36d2ff401e7cea9939e))
* set name for importer feedname ([f588e29](https://github.com/informatievlaanderen/streetname-registry/commit/f588e2952bb5d309f6975e22b7b3ae6b994a7139))
* set oslo context type to string ([3c5f66a](https://github.com/informatievlaanderen/streetname-registry/commit/3c5f66aeadbbe8787bce4d26b2a3548442968b1c))
* set sync feed dates to belgian timezone ([cb6e2bc](https://github.com/informatievlaanderen/streetname-registry/commit/cb6e2bce4082ed202862a5ddb1ece0eae9197c45))
* snapshot settings ([1df2107](https://github.com/informatievlaanderen/streetname-registry/commit/1df2107ca8e24b88fe68a5afdbc2c8293023d419))
* snapshotting ([6e8373f](https://github.com/informatievlaanderen/streetname-registry/commit/6e8373fbfc02dff1e60c9c22a87735c97141c821))
* sonar issues ([1eba90a](https://github.com/informatievlaanderen/streetname-registry/commit/1eba90a937212f012b086dd864a36a59ed057f2e))
* sort streetname list by olsoid [GR-717] ([f62740e](https://github.com/informatievlaanderen/streetname-registry/commit/f62740e4829282c8939d74942f489cca3c84ae5d))
* specify non nullable responses ([7330a61](https://github.com/informatievlaanderen/streetname-registry/commit/7330a615c37bbac637d4f85d4d8f0ba4f0598cb6))
* speed up cache status ([ef0e4db](https://github.com/informatievlaanderen/streetname-registry/commit/ef0e4db95d1cdbb6603abda35943836335610c40))
* status code docs ([e6b9e34](https://github.com/informatievlaanderen/streetname-registry/commit/e6b9e3402bb90327ce1060d4a3fa12bbbca54c5b))
* streetname sort bosa is now by PersistentLocalId ([4ae3dd7](https://github.com/informatievlaanderen/streetname-registry/commit/4ae3dd7167a219206bf93b0b9f1a96100990ede0))
* streetnameid in extract file is a string ([f845424](https://github.com/informatievlaanderen/streetname-registry/commit/f845424ed7ff69b9ca453b0d36c7e614ebf47ae8))
* style to retrigger build ([c0340c5](https://github.com/informatievlaanderen/streetname-registry/commit/c0340c53b53fc178dc554ef9886a59f1266b3745))
* style to trigger build ([b7f18db](https://github.com/informatievlaanderen/streetname-registry/commit/b7f18dbf9cd2185b8c7148e0c08b13b92ff08616))
* support kafka sasl authentication ([6f64b9d](https://github.com/informatievlaanderen/streetname-registry/commit/6f64b9ddd6242e2163c4599e5b124e01763b0d2e))
* support nullable Rfc3339SerializableDateTimeOffset in converter ([7b3c704](https://github.com/informatievlaanderen/streetname-registry/commit/7b3c704eb1e6fe4d0c31b53acc00511bca8e8e7b))
* swagger docs now show list response correctly ([79adcf9](https://github.com/informatievlaanderen/streetname-registry/commit/79adcf907ec3646260601bdb16ee773ac2f66c56))
* syndication distributedlock now runs async ([76a1985](https://github.com/informatievlaanderen/streetname-registry/commit/76a1985a1132ef5d79d857c121c966d33f06df0d))
* take local changes into account for versions projection ([9560ec6](https://github.com/informatievlaanderen/streetname-registry/commit/9560ec65c87793c126ee85fcb9cd49adb56d42cc))
* testje.yml ([70ae13b](https://github.com/informatievlaanderen/streetname-registry/commit/70ae13bf47df1ae1aa02ae5f6d6ce0c488f54f1d))
* ticketing registration ([dc7e16a](https://github.com/informatievlaanderen/streetname-registry/commit/dc7e16a64bf8813cee4677553900425aaa89433c))
* trigger build ([a1d8266](https://github.com/informatievlaanderen/streetname-registry/commit/a1d82667db5f56f5f50c1d5167f66bc120f9c1dd))
* trigger build ([47c9eb7](https://github.com/informatievlaanderen/streetname-registry/commit/47c9eb790f2ef6875051092db178c98c9ac160ce))
* trigger build :( ([e775c3e](https://github.com/informatievlaanderen/streetname-registry/commit/e775c3ea0df61c4c0f9ad3869f4d1ba6413e5e8d))
* trigger build by correcting ident ([77464b8](https://github.com/informatievlaanderen/streetname-registry/commit/77464b89c3959fb2676de000168590e23e29e46b))
* typo in lambda release ([600c1c5](https://github.com/informatievlaanderen/streetname-registry/commit/600c1c57341ddd43add8b4896144a827ce350534))
* update api ([240b5ad](https://github.com/informatievlaanderen/streetname-registry/commit/240b5adbeb14bb444d3cafecb2d904a824acb3b2))
* update api for etag fix ([2ee757f](https://github.com/informatievlaanderen/streetname-registry/commit/2ee757fa15c27b2c174eaf674ca6d4eafd25eb33))
* update api with use of problemdetailshelper GRAR-1814 ([d0e549f](https://github.com/informatievlaanderen/streetname-registry/commit/d0e549f6f707caba1f2e819c764360a8a44758ab))
* update asset to fix importer ([7ee93a7](https://github.com/informatievlaanderen/streetname-registry/commit/7ee93a7a2da6000733eb07f1fd0fe2f4f27c6294))
* update aws DistributedMutex package ([7966039](https://github.com/informatievlaanderen/streetname-registry/commit/7966039efa956f6058092b5565d29d4710c7e0ac))
* update basisregisters api dependency ([3b162eb](https://github.com/informatievlaanderen/streetname-registry/commit/3b162eb6369788505e11857182ce2b0d2e69f927))
* update dependencies ([e0047f0](https://github.com/informatievlaanderen/streetname-registry/commit/e0047f08c05023577e201705d5e16baeecf7b048))
* update dependencies ([90b69e7](https://github.com/informatievlaanderen/streetname-registry/commit/90b69e76594d7810e161cc21b0d72b7fb4ca99aa))
* update dependencies GRAR-752 ([9873989](https://github.com/informatievlaanderen/streetname-registry/commit/98739890264bdeeb349489d52068b8bccf8f584f))
* update deps ([7acf78e](https://github.com/informatievlaanderen/streetname-registry/commit/7acf78e550f8e895473096eda764cee9917b6d38))
* update dockerid detection ([637ed8d](https://github.com/informatievlaanderen/streetname-registry/commit/637ed8d909943a582d6a2441f42a7cff3a26fd3e))
* update docs backoffice GAWR-2349 ([188b667](https://github.com/informatievlaanderen/streetname-registry/commit/188b66771c95fc06efe562c3f9f7de8a53c1ed27))
* update docs projections ([7c2f5e2](https://github.com/informatievlaanderen/streetname-registry/commit/7c2f5e227fb0c07f800b11e50d50c7ec3de04a05))
* update eventdescription StreetNameWasMigratedToMunicipality ([021a9dc](https://github.com/informatievlaanderen/streetname-registry/commit/021a9dc615b37673011569aac55860b91ec1cf2b))
* update grar common ([2af230f](https://github.com/informatievlaanderen/streetname-registry/commit/2af230f778e09d83879945fc4b4c704fddc97780))
* update grar common to fix provenance ([c63f2a7](https://github.com/informatievlaanderen/streetname-registry/commit/c63f2a7a61600836d4616eeb09bf66502f90da02))
* update grar common to fix versie id type ([7d4a7b1](https://github.com/informatievlaanderen/streetname-registry/commit/7d4a7b1895040f6b5a806daabf130bc364610ca0))
* update grar dependencies GRAR-412 ([155a7db](https://github.com/informatievlaanderen/streetname-registry/commit/155a7dbb976e7c6fa2f0743c07c967267bbee0a1))
* update grar import package ([48a4d18](https://github.com/informatievlaanderen/streetname-registry/commit/48a4d18496449e71ad9331e432a9c4d3f8fb4026))
* update grar-common dependencies GRAR-2060 ([20ae6e6](https://github.com/informatievlaanderen/streetname-registry/commit/20ae6e6cf6824fde2c0d0b3a7f4ae764171ea126))
* update grar-common packages ([debc262](https://github.com/informatievlaanderen/streetname-registry/commit/debc262e2a748a74e055768b8633b05421459f75))
* update import packages ([cd03b79](https://github.com/informatievlaanderen/streetname-registry/commit/cd03b79168f22504b21f1ec698515c21d4ae101d))
* update json serialization dependencies ([a8ab6e7](https://github.com/informatievlaanderen/streetname-registry/commit/a8ab6e7634b03a3e27169e0ab2320d0f7f1ccfbd))
* update legacy projections migration event ([7beee58](https://github.com/informatievlaanderen/streetname-registry/commit/7beee58c8facbd09ddf15f77c4067a22205bd4df))
* update message handling ([054fa09](https://github.com/informatievlaanderen/streetname-registry/commit/054fa092986c363bcce432e87eb64b9c103460ba))
* update nuget package ([3d79968](https://github.com/informatievlaanderen/streetname-registry/commit/3d7996856567d7c34a2c6425616c724d43750bd6))
* update package ([fd99fb2](https://github.com/informatievlaanderen/streetname-registry/commit/fd99fb21e2091e04541428d36506ad7864f718e5))
* update packages ([0c32f64](https://github.com/informatievlaanderen/streetname-registry/commit/0c32f64774f9a8156b1ffd44badf899d9dcd6504))
* update packages for import batch timestamps ([ee62c56](https://github.com/informatievlaanderen/streetname-registry/commit/ee62c569761314c3bc1b532883b5d3b99f8ee501))
* update packages to fix null operator/reason GRAR-1535 ([1b43cfa](https://github.com/informatievlaanderen/streetname-registry/commit/1b43cfa854177829815a820ed277f3e7960612e9))
* update paket.template in backoffice after removing reference ([dd26a89](https://github.com/informatievlaanderen/streetname-registry/commit/dd26a89fbd39c501fd43f3718e25e722921eea61))
* update problemdetails for xml response GR-829 ([39280b7](https://github.com/informatievlaanderen/streetname-registry/commit/39280b7287fa8fc4922aec940066c00fcd382900))
* update projection description ([e1c7bd5](https://github.com/informatievlaanderen/streetname-registry/commit/e1c7bd591d405d975dff6a1228983a6ef2dcb9e3))
* update projection handling & update sync migrator ([92029bd](https://github.com/informatievlaanderen/streetname-registry/commit/92029bdb30a4e08befc20451a3a0f6cf2784a4e5))
* update projector dependency GRAR-1876 ([bd26a3d](https://github.com/informatievlaanderen/streetname-registry/commit/bd26a3dd808cfbd666154cc15ced38fb8828a59e))
* update redis lastchangedlist to log time of lasterror ([18f99dc](https://github.com/informatievlaanderen/streetname-registry/commit/18f99dcd5fc5f6f3349b495774be1cb78323430d))
* update references for event property descriptions ([6e9bf93](https://github.com/informatievlaanderen/streetname-registry/commit/6e9bf93d99750b01930474099d81657b13f25b0d))
* update testje.yml ([f2eb478](https://github.com/informatievlaanderen/streetname-registry/commit/f2eb4789f29d5f460c96319022602d5702c60419))
* update testje.yml ([21d2a9b](https://github.com/informatievlaanderen/streetname-registry/commit/21d2a9beba52e94fbd3ff9629d36e33f1c151e50))
* updated paket files ([c36df86](https://github.com/informatievlaanderen/streetname-registry/commit/c36df867fd076e339cddef32e0b87a7455118dad))
* upgarde common to fix sync author ([912d1f0](https://github.com/informatievlaanderen/streetname-registry/commit/912d1f0dc27d625d7ddec8f78f686ebf0d2e83a5))
* upgrade api for error headers ([2f24b69](https://github.com/informatievlaanderen/streetname-registry/commit/2f24b699167ade5c3abfda9ed93c23a70851819e))
* upgrade common packages ([8843cbf](https://github.com/informatievlaanderen/streetname-registry/commit/8843cbf282619f80a2cb1f14d75c714595e8abf2))
* upgrade grar common ([a336465](https://github.com/informatievlaanderen/streetname-registry/commit/a3364650935a10611bd761c3da60c234efb38218))
* upgrade message handling ([7ef8e29](https://github.com/informatievlaanderen/streetname-registry/commit/7ef8e29f0882816984672f6e66d1a64f6de90dfd))
* upgrade packages to fix json order ([cda78af](https://github.com/informatievlaanderen/streetname-registry/commit/cda78afe70e831c4a8f912f8a522c6d0745aa350))
* upgrade swagger GRAR-1599 ([70906f6](https://github.com/informatievlaanderen/streetname-registry/commit/70906f664c6da2d3defc51646d67487f82dcbd40))
* use async startup of projections to fix hanging migrations ([e1b8f7c](https://github.com/informatievlaanderen/streetname-registry/commit/e1b8f7ceaab045c9e081a0f21ed669399883aad2))
* use columnstore for legacy syndication ([8907d63](https://github.com/informatievlaanderen/streetname-registry/commit/8907d63020b3318ec53c942fe327aa08f41cf71c))
* use correct build user ([ea26b87](https://github.com/informatievlaanderen/streetname-registry/commit/ea26b878533aef8988c7bde82e3cfceab33f1b19))
* use fixed datadog tracing ([6b40209](https://github.com/informatievlaanderen/streetname-registry/commit/6b402098232b7080fedac7e7a69e24ad7e77a18f))
* use generic dbtraceconnection ([7913401](https://github.com/informatievlaanderen/streetname-registry/commit/7913401938cdba746c840367adba60ca4aacbb1e))
* use https for namespace ([92965c1](https://github.com/informatievlaanderen/streetname-registry/commit/92965c1daf373543690b7d254c01d5eeb40bc914))
* use new desiredstate columns for projections ([b59c39a](https://github.com/informatievlaanderen/streetname-registry/commit/b59c39a29a5883b5f38b34751a5db0991a7f0cc8))
* use new lastchangedlist migrations runner ([4d4e0e2](https://github.com/informatievlaanderen/streetname-registry/commit/4d4e0e2cad19dfe062d02f3d64d36cfc069e5553))
* use nullable language for old events ([ef961cb](https://github.com/informatievlaanderen/streetname-registry/commit/ef961cb846d3160edc73ac9c5309af616a1673af))
* use persistentlocalid as id for object in feed ([a48c289](https://github.com/informatievlaanderen/streetname-registry/commit/a48c289f4bb1efb390b93360bb985885deec0628))
* use problemjson middleware ([3f961f0](https://github.com/informatievlaanderen/streetname-registry/commit/3f961f06dcfb89dd8cb9b42b430bf08919c462ce))
* use typed embed value GRAR-1465 ([948f242](https://github.com/informatievlaanderen/streetname-registry/commit/948f242c6d95f8273b44cd47b05119f463d71993))
* versie id type change to string for sync resources ([4e70471](https://github.com/informatievlaanderen/streetname-registry/commit/4e70471ae17adc8e25944d6c72ff8f1fd005475a))
* xml date serialization sync projection ([3e2b28e](https://github.com/informatievlaanderen/streetname-registry/commit/3e2b28eafc07e46263c40e7b97babaf752efdadd))


### Code Refactoring

* upgrade to netcoreapp31 ([da4ea9e](https://github.com/informatievlaanderen/streetname-registry/commit/da4ea9e19f41e4729aa1eaca8d27f0c8674a4852))


### Features

* adapted sync with new municipality changes ([c05d427](https://github.com/informatievlaanderen/streetname-registry/commit/c05d42722285b55d9a21d4306e96b70f762c90c6))
* add backoffice api + propose endpoint GAWR-2064 ([6fc4c4b](https://github.com/informatievlaanderen/streetname-registry/commit/6fc4c4b971ca9b7a422e79f4e223a2fbaf414b49))
* add backoffice, update buildscript, gh pipeline, add first intern api endpoint ([5ed8a9f](https://github.com/informatievlaanderen/streetname-registry/commit/5ed8a9fbcb4a04a410c7b1dd68c1d5c60686012f))
* add cache status to projector api ([ecbc48d](https://github.com/informatievlaanderen/streetname-registry/commit/ecbc48d0c1bc24d4ef5972d3cdfbc692ee795650))
* add context + type to oslo responses GAWR-666 ([065f8a0](https://github.com/informatievlaanderen/streetname-registry/commit/065f8a0d08c7474b4445827503f65f6b9ae8e225))
* add error message for syndication projections ([4b19b50](https://github.com/informatievlaanderen/streetname-registry/commit/4b19b506c5af12cdaa4a846d6938a5261e3440f4))
* add errorcodes to validationexception ([6c25f45](https://github.com/informatievlaanderen/streetname-registry/commit/6c25f45941a702e137fdf6f4089aadb5d6ff4067))
* add event + save to db ([e48792d](https://github.com/informatievlaanderen/streetname-registry/commit/e48792d96ecc550113ecd8d33632040665860c5e))
* add event data to sync endpoint ([31bd514](https://github.com/informatievlaanderen/streetname-registry/commit/31bd5145cabf3563112c9d9fbdb55c63dc958ab1))
* add hash to events ([e1e252d](https://github.com/informatievlaanderen/streetname-registry/commit/e1e252d375a40c3e831055319966f054ef98dd28))
* add import status endpoint GRAR-1400 ([c26fa70](https://github.com/informatievlaanderen/streetname-registry/commit/c26fa700ee5a73e91e7922701fe4c0898997cf16))
* add Kafka commands ([a7006a4](https://github.com/informatievlaanderen/streetname-registry/commit/a7006a4a0d8afd3965824e7873235021a4ff198a))
* add metadata file with latest event id to street name extract GRAR-2060 ([6d8d62c](https://github.com/informatievlaanderen/streetname-registry/commit/6d8d62c019d7bb7b094cbe8903e31f0d335dabc4))
* add migration command ([b1cfba0](https://github.com/informatievlaanderen/streetname-registry/commit/b1cfba0e54166a3d6c02cb931c04591040b84b7a))
* add missing event handlers where nothing was expected [#29](https://github.com/informatievlaanderen/streetname-registry/issues/29) ([35e315a](https://github.com/informatievlaanderen/streetname-registry/commit/35e315a2569a7135d80583f8928ded721bb6ddd6))
* add missing projection tests ([85e8389](https://github.com/informatievlaanderen/streetname-registry/commit/85e83898412b4e090ff180bd13e913610771b3d8))
* add municipality commands/events GAWR-1161 ([cdf2fdb](https://github.com/informatievlaanderen/streetname-registry/commit/cdf2fdb12e9096fb202bcb772b9e6c30390184a4))
* add new command to mark legacy streetname migration ([a22cc0b](https://github.com/informatievlaanderen/streetname-registry/commit/a22cc0be127d9d50567c57c06f1b7bfa5505a4a6))
* add new municipalitystreamid ([8d0e25f](https://github.com/informatievlaanderen/streetname-registry/commit/8d0e25f3bb7d48e7f0cf7f49c5fc250ec80e20ce))
* add oslo to lastchangedlist projection + migrate data ([6d73fa6](https://github.com/informatievlaanderen/streetname-registry/commit/6d73fa60c5292381a0c9e9a048bc821894458040))
* add position to ETag GAWR-2358 ([a1f4994](https://github.com/informatievlaanderen/streetname-registry/commit/a1f4994efbf81f27c58b60d16cb59a9e67f65745))
* add Producer ([#558](https://github.com/informatievlaanderen/streetname-registry/issues/558)) ([06af914](https://github.com/informatievlaanderen/streetname-registry/commit/06af9143b4144f3a487ea242a7c5a976b6a0e1d2))
* add projection attributes GRAR-1876 ([2d30d48](https://github.com/informatievlaanderen/streetname-registry/commit/2d30d48eba607e5efe29ff44e44f92ff28129286))
* add projections for new events ([b85a339](https://github.com/informatievlaanderen/streetname-registry/commit/b85a3391bbf5e1303224ee7b8365b183fb45c084))
* add projector + cleanup projection libraries ([a861da2](https://github.com/informatievlaanderen/streetname-registry/commit/a861da20cac655ad0d92bb1de29c1749ca0cca20))
* add retry policy for streetname migrator ([18bfc2e](https://github.com/informatievlaanderen/streetname-registry/commit/18bfc2ef755e9daf8b1e2f8e332c5f544c1efb2e))
* add snapshotting ([66672d6](https://github.com/informatievlaanderen/streetname-registry/commit/66672d6fccf68e67112a481e4d5bb76533c5346f))
* add status filter on legacy list ([ad1563b](https://github.com/informatievlaanderen/streetname-registry/commit/ad1563bccbd4df234cde6e47fcef27052f32fab7))
* add status to legacy list ([20c741c](https://github.com/informatievlaanderen/streetname-registry/commit/20c741cd12742cc2fd02e12eb826ec902942d8d5))
* add statuscode 410 Gone for removed streetnames ([4e5f7f6](https://github.com/informatievlaanderen/streetname-registry/commit/4e5f7f6c51165284f63c8aaaa34f1745a8750359))
* add streetname migrator proj ([fcd0727](https://github.com/informatievlaanderen/streetname-registry/commit/fcd0727662245723b0cb279af2487710900e2afa))
* add sync tag on events ([89d8f3e](https://github.com/informatievlaanderen/streetname-registry/commit/89d8f3e09fef5af44efd7532c753a6f3dc1b502d))
* add syndication status to projector api ([5d681f5](https://github.com/informatievlaanderen/streetname-registry/commit/5d681f5799a3663cc7632bb1b1de010b0d2dc65d))
* add timestamp to sync provenance GRAR-1451 ([1b069bc](https://github.com/informatievlaanderen/streetname-registry/commit/1b069bc08ef02a822f9daecc0f10b36b244c627d))
* add totaal aantal endpoint ([cf348b5](https://github.com/informatievlaanderen/streetname-registry/commit/cf348b5419c1d19306d555ccbceb3f08a002e90d))
* add v2 projections to projector with toggle ([8e024dd](https://github.com/informatievlaanderen/streetname-registry/commit/8e024dd85daf59c21bb0d1e4e0bbc99c8dee86f3))
* add validation 4d, enable json error action filter ([d0bf6f2](https://github.com/informatievlaanderen/streetname-registry/commit/d0bf6f2be8b458640465afff079e213d1da3f428))
* add validation 6 ([8f1af92](https://github.com/informatievlaanderen/streetname-registry/commit/8f1af924a787b2dedfbb79ee82d9beb6e3ccafc9))
* add validation 8 gawr-2692 ([6712489](https://github.com/informatievlaanderen/streetname-registry/commit/6712489f438db63305916d463c979b7d7c191088))
* add validation gawr-2687 duplicate streetname ([25eff5b](https://github.com/informatievlaanderen/streetname-registry/commit/25eff5ba2c71d995663327553d766f3afc3e0535))
* add validation gawr-2688 4B ([4f20409](https://github.com/informatievlaanderen/streetname-registry/commit/4f204095c78736d4f763957772af0ddb59462b3e))
* add validation gawr-2688 4C ([5c8db3b](https://github.com/informatievlaanderen/streetname-registry/commit/5c8db3b23c475c38e4f086f0be698d243313cf71))
* add validation gawr-2691 municipality retired ([71929b7](https://github.com/informatievlaanderen/streetname-registry/commit/71929b7a3be1ee3ebf8a8bb2f84d67516c2a4021))
* add validator for propose streetname GAWR-1162 + GAWR-2686 ([1ccd100](https://github.com/informatievlaanderen/streetname-registry/commit/1ccd1008ea1f334f2d4fe7fd2a45bc8158fbb69c))
* add wait for user input to importer ([fd1d14e](https://github.com/informatievlaanderen/streetname-registry/commit/fd1d14ec03e7a813ab123067005a24916a98905c))
* AggregateIdIsNotFoundException error code and message ([900ef85](https://github.com/informatievlaanderen/streetname-registry/commit/900ef85359d52cd4e56cbc1c3e6b8fef8acab090))
* allow only one projector instance ([c668b77](https://github.com/informatievlaanderen/streetname-registry/commit/c668b7757c24032d5c96722bb08e277045e17a28))
* API list count valid id's in indexed view ([ef31c11](https://github.com/informatievlaanderen/streetname-registry/commit/ef31c114327aa82a8fab5c7adb25ae30da87af0f))
* approval validation 4, status not proposed ([51060a1](https://github.com/informatievlaanderen/streetname-registry/commit/51060a1be2555de72475ad922fd1efa1e5560ea7))
* approve streetname ([beb9ae4](https://github.com/informatievlaanderen/streetname-registry/commit/beb9ae4023f50ecedc1bba52532f5cec67af7fa8))
* approve streetname endpoint backoffice ([fc513c2](https://github.com/informatievlaanderen/streetname-registry/commit/fc513c27a293e3bd6b0646a005704b62098c318b))
* build migrator streetname ([68a94af](https://github.com/informatievlaanderen/streetname-registry/commit/68a94af4e8546c4cb24a5dafbca147bbeb3603e8))
* bump packages ([a1ec84c](https://github.com/informatievlaanderen/streetname-registry/commit/a1ec84c1981e0ce2143acfe9ae4d6adc2bdca312))
* bump projector & projection handling ([fe9736a](https://github.com/informatievlaanderen/streetname-registry/commit/fe9736a1fdb4808382e0245772fb2ca22b0257f3))
* bump to .net 2.2.6 ([d6eaf38](https://github.com/informatievlaanderen/streetname-registry/commit/d6eaf38f49d818094f234bf270f9c4cf40493cc4))
* change display municipality name of detail in Api.Legacy ([79d693f](https://github.com/informatievlaanderen/streetname-registry/commit/79d693f6cf81ca5d8b647ba4f566a8bc60a073f9))
* change routes for propose and approve ([884fd38](https://github.com/informatievlaanderen/streetname-registry/commit/884fd38a0bdcba9d20fb4bbba1da8f5a7f7969e0))
* configurable polly retry policy ([1f5a3ae](https://github.com/informatievlaanderen/streetname-registry/commit/1f5a3ae058b2733fa7fde6c3aafca67552edca52))
* consuming messages without commandhandling ([575c838](https://github.com/informatievlaanderen/streetname-registry/commit/575c83860ca3cc8d1589b74dedef5b2ddede9312))
* create Api.Oslo project ([33e978a](https://github.com/informatievlaanderen/streetname-registry/commit/33e978a9ebebe4faebff0cdca5a8444ee36bceef))
* create WFS projection helper GAWR-2241 ([5430cb6](https://github.com/informatievlaanderen/streetname-registry/commit/5430cb6be5f1d78b097648792da6389e1375c32c))
* create wms projection ([4a1cab4](https://github.com/informatievlaanderen/streetname-registry/commit/4a1cab409fbc348da32a28d886922e2877ab7e72))
* deploy docker to production ([354a707](https://github.com/informatievlaanderen/streetname-registry/commit/354a7072a205fe0fd3065c64c82755561e89557c))
* do not migrate incomplete streetnames ([bd16610](https://github.com/informatievlaanderen/streetname-registry/commit/bd166106e442832525ae8ab82e379ee117ab1339))
* do not take diacritics into account when filtering on municipality ([025a122](https://github.com/informatievlaanderen/streetname-registry/commit/025a12248669047edb70717eab6f8d5d090ac00e))
* don't handle aggregatenotfoundexception in lambda ([c2e3491](https://github.com/informatievlaanderen/streetname-registry/commit/c2e3491b31f604b591f57295c957e38aaffc5e2a))
* don't process message which can't be cast to sqsrequest ([eb030a9](https://github.com/informatievlaanderen/streetname-registry/commit/eb030a9fefdc0a84304ea7ed3a78fad5e47c21f5))
* extract datavlaanderen namespace to settings [#3](https://github.com/informatievlaanderen/streetname-registry/issues/3) ([e13a831](https://github.com/informatievlaanderen/streetname-registry/commit/e13a831b8ced9e8e6e2f20a796195b4350519615))
* GAWR-1179 handle command ([1759364](https://github.com/informatievlaanderen/streetname-registry/commit/17593646a3ad63711b09d3cad96e3926f9ba58a3))
* keep track of how many times lastchanged has errored ([c81eb82](https://github.com/informatievlaanderen/streetname-registry/commit/c81eb82be08fc8295130d0f49cf39a09d6e39c08))
* make other actions async ([f4ec62c](https://github.com/informatievlaanderen/streetname-registry/commit/f4ec62c18a7253d9b782f5dc438a185475c8a7d9))
* make propose streetname async ([e710e38](https://github.com/informatievlaanderen/streetname-registry/commit/e710e380c375a0c55322fe578c872d3e570fe011))
* mediator handlers + tests ([f7a8b10](https://github.com/informatievlaanderen/streetname-registry/commit/f7a8b10b8f70a569a417c56b4f20d2292ed8fbed))
* migrateStreetName command with event 2708 ([315d126](https://github.com/informatievlaanderen/streetname-registry/commit/315d12650182d9116ff6b780011972cd6b478e75))
* move to dotnet 6.0.3 ([7bf80f2](https://github.com/informatievlaanderen/streetname-registry/commit/7bf80f2d9296f2de96584de5ca1201eb5397d195))
* municipality status validation when approving streetname ([2a864af](https://github.com/informatievlaanderen/streetname-registry/commit/2a864afca6f7555d0e39bb070fe6a8e39d822013))
* passthrough SQS request metadata ([a07bed5](https://github.com/informatievlaanderen/streetname-registry/commit/a07bed55ccb84c77b771a129c5a101119386e34e))
* propose streetname ([10478ee](https://github.com/informatievlaanderen/streetname-registry/commit/10478eea838b3fc196576308e025f9a0da0d12ce))
* refactor exception properties to value objects ([117667e](https://github.com/informatievlaanderen/streetname-registry/commit/117667efa7ebf7ef43e1c1248145530720531066))
* refactor metadata for atom feed-metadata GRAR-1436 GRAR-1445 GRAR-1453 GRAR-1455 ([b24b12f](https://github.com/informatievlaanderen/streetname-registry/commit/b24b12fcec061b9c1852527bf02f9f2191780556))
* reject street name ([51be9ed](https://github.com/informatievlaanderen/streetname-registry/commit/51be9ed593723559299506b350ae50b99855dc15))
* rename oslo id to persistent local id ([cd9fbb9](https://github.com/informatievlaanderen/streetname-registry/commit/cd9fbb9100e6d1b269ba01ca32af17d63a816a48))
* return http status 202 instead of 204 for success ([761b037](https://github.com/informatievlaanderen/streetname-registry/commit/761b037853c78830a24cb6141924afce66d06a77))
* send mail when importer crashes ([2ceb53d](https://github.com/informatievlaanderen/streetname-registry/commit/2ceb53d6b5edf9b056598ae7ebffedf4859150cd))
* sqs refactor ([3d881b2](https://github.com/informatievlaanderen/streetname-registry/commit/3d881b24983281d133fca09325de681e6f23fcd5))
* streetname name was corrected ([f89554b](https://github.com/informatievlaanderen/streetname-registry/commit/f89554b75dada4c6e758e5d8763f9d749a465a76))
* update api to 17.0.0 ([e9bac79](https://github.com/informatievlaanderen/streetname-registry/commit/e9bac797f75ac0cc269cdae448ed3b438d485edd))
* update grar common for IHasHash ([ec77314](https://github.com/informatievlaanderen/streetname-registry/commit/ec7731483a8a1e37a6745bb2a51596974b76aaa3))
* update grar-common to 16.15.1 ([b8f6984](https://github.com/informatievlaanderen/streetname-registry/commit/b8f6984b6e1916db63020a0ef24a036c84d021fa))
* update packages to include count func ([8e7eef4](https://github.com/informatievlaanderen/streetname-registry/commit/8e7eef4e549127f85b630a152e8690b934096c2c))
* update projections for StreetNameWasApproved ([cd3c488](https://github.com/informatievlaanderen/streetname-registry/commit/cd3c488cc79e691bc23815cabcc2676f74016f72))
* update projector with gap detection and extended status api ([ac8d5ce](https://github.com/informatievlaanderen/streetname-registry/commit/ac8d5ce0af2eeb674df1c4cf07fb958c403cf362))
* upgrade api package ([8190372](https://github.com/informatievlaanderen/streetname-registry/commit/8190372b5adb49aa715907c0ae33b839d7525c81))
* upgrade Be.Vlaanderen.Basisregisters.Api ([f2dd36b](https://github.com/informatievlaanderen/streetname-registry/commit/f2dd36b6a6d551062b6061fae7aa0a79c73fa0de))
* upgrade importer to netcore3 ([78ab7c9](https://github.com/informatievlaanderen/streetname-registry/commit/78ab7c91a542f468991827aaf53ab35ac7dc7e25))
* upgrade netcoreapp31 and dependencies ([77171a8](https://github.com/informatievlaanderen/streetname-registry/commit/77171a8a88309b3117fa570492325068ad33e7cb))
* upgrade NTS & shaperon packages ([c60f8b5](https://github.com/informatievlaanderen/streetname-registry/commit/c60f8b5b6e0a84f83f5a6c3f5012435b0152f7a1))
* upgrade packages ([6d9ad96](https://github.com/informatievlaanderen/streetname-registry/commit/6d9ad96115681e33e8d60feb4448481b41cfe89e))
* upgrade packages for import ([cd25375](https://github.com/informatievlaanderen/streetname-registry/commit/cd253758b2fff2eda70e1acfa5f736e955ce390d))
* upgrade projection handling to include errmessage lastchangedlist ([b8850ae](https://github.com/informatievlaanderen/streetname-registry/commit/b8850ae392632395acbb53b3ca1d4d5c62599403))
* upgrade projectionhandling package ([af8beb4](https://github.com/informatievlaanderen/streetname-registry/commit/af8beb43554c718905f1976b812e712ed735cbad))
* upgrade projector and removed explicit start of projections ([e7fb789](https://github.com/informatievlaanderen/streetname-registry/commit/e7fb789cf05500bf79278cfaa8f88b3580fa8bdc))
* upgrade provenance package Plan -> Reason ([fdb618e](https://github.com/informatievlaanderen/streetname-registry/commit/fdb618e56ed0f218f89f7bd49116803167341226))
* use different service lifetimescope per message ([5cd58d8](https://github.com/informatievlaanderen/streetname-registry/commit/5cd58d87816360dc0bc39621f5abbc384d2cea49))
* use distributed lock for syndication ([330ca69](https://github.com/informatievlaanderen/streetname-registry/commit/330ca694313f01ef982ec62b7a85b74b6dd255fe))
* useSqs feature toggle ([3d60ba0](https://github.com/informatievlaanderen/streetname-registry/commit/3d60ba042570fc91fcb7231cdabeeade30c54fb1))
* validate ifmatchheadervalue in lambdas ([46c11be](https://github.com/informatievlaanderen/streetname-registry/commit/46c11be7f278557f920c58242ff83afc34dcdb05))
* validate streetname correction has atleast one language specified ([03e5a0a](https://github.com/informatievlaanderen/streetname-registry/commit/03e5a0ad1715e74eead4badfa02716039bc1a8b8))
* validate streetname is found and not removed ([e5f7711](https://github.com/informatievlaanderen/streetname-registry/commit/e5f7711316ce876ea6267152777b340425ec9d70))


### Performance Improvements

* add index wfs ([abc5004](https://github.com/informatievlaanderen/streetname-registry/commit/abc5004ae913bd73d141728fb640ae7282d303cd))
* increase performance by removing count from lists ([2212fd2](https://github.com/informatievlaanderen/streetname-registry/commit/2212fd212e63e415c6f982356fac56340137b8cf))


### BREAKING CHANGES

* move to dotnet 6.0.3
* Upgrade to .NET Core 3.1

## [3.10.28](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.27...v3.10.28) (2022-09-23)

## [3.10.27](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.26...v3.10.27) (2022-09-23)


### Bug Fixes

* remove pushing nuget to github ([ed32b1e](https://github.com/informatievlaanderen/streetname-registry/commit/ed32b1e78cf1e83f39ca55491b402b875fd8f88f))

## [3.10.26](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.25...v3.10.26) (2022-09-23)


### Bug Fixes

* comment lambda deployment ([ce2b75d](https://github.com/informatievlaanderen/streetname-registry/commit/ce2b75d21d3547542095a0134820bf50e7a65709))

## [3.10.25](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.24...v3.10.25) (2022-09-23)


### Bug Fixes

* install npm packages ([954896d](https://github.com/informatievlaanderen/streetname-registry/commit/954896d81cd98696ccf2543c0739dcf3471b21a7))

## [3.10.24](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.23...v3.10.24) (2022-09-22)


### Bug Fixes

* fix nuget ([ecbb6d7](https://github.com/informatievlaanderen/streetname-registry/commit/ecbb6d7f61585f61b984fa9c3b296b2506157734))
* lambda settings ([207cf0e](https://github.com/informatievlaanderen/streetname-registry/commit/207cf0e6e591990d4caa674e7f35ba6f065862a9))

## [3.10.23](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.22...v3.10.23) (2022-09-22)


### Bug Fixes

* lambda executable ([d162001](https://github.com/informatievlaanderen/streetname-registry/commit/d16200139aa003f92d109a1a6b1039b3ccb41d2c))
* replace internal ticket url with public url ([debf772](https://github.com/informatievlaanderen/streetname-registry/commit/debf77257f28716ca22c0117565f9cdfdb8c95a8))

## [3.10.22](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.21...v3.10.22) (2022-09-22)


### Bug Fixes

* bump messagehandling and use queueurl instead of name ([4146838](https://github.com/informatievlaanderen/streetname-registry/commit/414683855c1e99ca5fdb5c4ee927aa7c88fcd58c))
* set missing persistent local id on sqs request ([3b03f58](https://github.com/informatievlaanderen/streetname-registry/commit/3b03f58ecb74c423a1d1e36d2ff401e7cea9939e))

## [3.10.21](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.20...v3.10.21) (2022-09-22)


### Bug Fixes

* correct xml serialization ([e14f568](https://github.com/informatievlaanderen/streetname-registry/commit/e14f568ad8851f28377b8140879548957965f60d))
* fix nuget-projector ([df7e276](https://github.com/informatievlaanderen/streetname-registry/commit/df7e2766ba7a43bcec84dcbff8915a7756e7e107))
* registration of httpproxy (ticketing) ([b3f0ae1](https://github.com/informatievlaanderen/streetname-registry/commit/b3f0ae1f3029bbd0a940b2fb4d50354e9e1b5df9))

## [3.10.20](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.19...v3.10.20) (2022-09-22)


### Bug Fixes

* install build pipeline before publishing nuget ([aad9502](https://github.com/informatievlaanderen/streetname-registry/commit/aad9502f22739f6dbd9b2df197078b897bbc7857))

## [3.10.19](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.18...v3.10.19) (2022-09-22)


### Bug Fixes

* fix debug folders ([5b63af0](https://github.com/informatievlaanderen/streetname-registry/commit/5b63af0a49bb572aef67f0e51f593840855b2832))
* fix indentation ([47151ac](https://github.com/informatievlaanderen/streetname-registry/commit/47151ac9f9a44c7da8d3109cdd4a8143221a3acb))

## [3.10.18](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.17...v3.10.18) (2022-09-22)


### Bug Fixes

* correct queue name ([ccaabd0](https://github.com/informatievlaanderen/streetname-registry/commit/ccaabd07bbf35cc797ad5d0c8d2dec90efdf0004))
* move cd test into testje ([b9ea49f](https://github.com/informatievlaanderen/streetname-registry/commit/b9ea49f65a5f9bb5930d26af67b11e57ba182022))

## [3.10.17](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.16...v3.10.17) (2022-09-21)


### Bug Fixes

* fix env vars for tagging ([f8821f0](https://github.com/informatievlaanderen/streetname-registry/commit/f8821f02f8180bbc6af9b6df210217cdc0192790))

## [3.10.16](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.15...v3.10.16) (2022-09-21)


### Bug Fixes

* call cd test ([25f06b6](https://github.com/informatievlaanderen/streetname-registry/commit/25f06b63ea215d4009912fabd6eb7b2f7b8e80e1))

## [3.10.15](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.14...v3.10.15) (2022-09-21)


### Bug Fixes

* fix ENV ([6747d46](https://github.com/informatievlaanderen/streetname-registry/commit/6747d4678e3807707742566cd18dcc9af1a4f71c))

## [3.10.14](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.13...v3.10.14) (2022-09-21)


### Bug Fixes

* fix nuget folder ([0db4964](https://github.com/informatievlaanderen/streetname-registry/commit/0db4964fc4d5f5979246ad27c7c7311103ea16cd))

## [3.10.13](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.12...v3.10.13) (2022-09-21)


### Bug Fixes

* add debug folders ([b3fdbce](https://github.com/informatievlaanderen/streetname-registry/commit/b3fdbce61f563f9f047b1c4132246cab369a554f))

## [3.10.12](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.11...v3.10.12) (2022-09-21)


### Bug Fixes

* nuget & staging ([ce26776](https://github.com/informatievlaanderen/streetname-registry/commit/ce267761e6a76638aaca99845e609f93fd67a716))

## [3.10.11](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.10...v3.10.11) (2022-09-21)


### Bug Fixes

* comment lambda deployment ([6da27e6](https://github.com/informatievlaanderen/streetname-registry/commit/6da27e61b66e4f7c40992165bcf21c36f3f5d035))

## [3.10.10](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.9...v3.10.10) (2022-09-21)


### Bug Fixes

* add lambda's & nuget ([55c6169](https://github.com/informatievlaanderen/streetname-registry/commit/55c61697d3d08bb8b87cfe32748081ba42a01d88))
* fix image names ([81aa1fe](https://github.com/informatievlaanderen/streetname-registry/commit/81aa1fe39c2bc11f777293caba0551a575fce1ef))

## [3.10.9](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.8...v3.10.9) (2022-09-21)


### Bug Fixes

* final test before calling cd test ([10fe5b5](https://github.com/informatievlaanderen/streetname-registry/commit/10fe5b5a0723d3f887e7017ab5541d85cb534adb))

## [3.10.8](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.7...v3.10.8) (2022-09-21)


### Bug Fixes

* fix set version ([#670](https://github.com/informatievlaanderen/streetname-registry/issues/670)) ([f460e1e](https://github.com/informatievlaanderen/streetname-registry/commit/f460e1efc1f1cdc648425c65e63de1e5eb468bf0))

## [3.10.7](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.6...v3.10.7) (2022-09-21)


### Bug Fixes

* adjust testje.yml ([b7e8574](https://github.com/informatievlaanderen/streetname-registry/commit/b7e85749e6af875a19619f5e0f327c120fc8d88f))

## [3.10.6](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.5...v3.10.6) (2022-09-20)


### Bug Fixes

* download & load images ([b1652fa](https://github.com/informatievlaanderen/streetname-registry/commit/b1652faf70e7a9e46f8f6a810388d60478d38e2b))

## [3.10.5](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.4...v3.10.5) (2022-09-20)


### Bug Fixes

* correct testje ([e024632](https://github.com/informatievlaanderen/streetname-registry/commit/e024632693c4b7456957763ea8d1a6d488306dfd))
* rename testje ([bad685d](https://github.com/informatievlaanderen/streetname-registry/commit/bad685df3ecbe63241b2de6249d35dcb7eee3de0))
* testje.yml ([70ae13b](https://github.com/informatievlaanderen/streetname-registry/commit/70ae13bf47df1ae1aa02ae5f6d6ce0c488f54f1d))

## [3.10.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.3...v3.10.4) (2022-09-16)


### Bug Fixes

* call test.yml from release.yml ([89cf43f](https://github.com/informatievlaanderen/streetname-registry/commit/89cf43f994dd26de6d50606295fe946cdc368db3))
* comment deployment to test ([402f0be](https://github.com/informatievlaanderen/streetname-registry/commit/402f0be916136de44d8aea03d12eb1d2ba4c81b6))
* make test.yml callable from another workflow ([ca8d253](https://github.com/informatievlaanderen/streetname-registry/commit/ca8d2534937be05f69cd9b39f4fd928829f90b01))


### Performance Improvements

* add index wfs ([abc5004](https://github.com/informatievlaanderen/streetname-registry/commit/abc5004ae913bd73d141728fb640ae7282d303cd))

## [3.10.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.2...v3.10.3) (2022-09-14)


### Bug Fixes

* add deploy test ([0078ac8](https://github.com/informatievlaanderen/streetname-registry/commit/0078ac82a2a0bb3969a0994bf688b25155124a25))
* modify deploy test ([e35ac52](https://github.com/informatievlaanderen/streetname-registry/commit/e35ac529c34624fc43649d7bf6e7fa77f3cbcb60))
* modify deploy test ([eaf0f5f](https://github.com/informatievlaanderen/streetname-registry/commit/eaf0f5fcf32e4e7a65b89aa35cb2e21b7e854f30))
* update testje.yml ([f2eb478](https://github.com/informatievlaanderen/streetname-registry/commit/f2eb4789f29d5f460c96319022602d5702c60419))
* update testje.yml ([21d2a9b](https://github.com/informatievlaanderen/streetname-registry/commit/21d2a9beba52e94fbd3ff9629d36e33f1c151e50))

## [3.10.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.1...v3.10.2) (2022-09-13)


### Bug Fixes

* trigger build ([a1d8266](https://github.com/informatievlaanderen/streetname-registry/commit/a1d82667db5f56f5f50c1d5167f66bc120f9c1dd))

## [3.10.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.10.0...v3.10.1) (2022-09-13)


### Bug Fixes

* typo in lambda release ([600c1c5](https://github.com/informatievlaanderen/streetname-registry/commit/600c1c57341ddd43add8b4896144a827ce350534))

# [3.10.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.9.0...v3.10.0) (2022-09-13)


### Bug Fixes

* deploy lambda functions to test & stg environments ([86c103c](https://github.com/informatievlaanderen/streetname-registry/commit/86c103c28c44cebc0e43f6fa4fe9883751c4f18e))


### Features

* AggregateIdIsNotFoundException error code and message ([900ef85](https://github.com/informatievlaanderen/streetname-registry/commit/900ef85359d52cd4e56cbc1c3e6b8fef8acab090))
* configurable polly retry policy ([1f5a3ae](https://github.com/informatievlaanderen/streetname-registry/commit/1f5a3ae058b2733fa7fde6c3aafca67552edca52))

# [3.9.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.11...v3.9.0) (2022-09-09)


### Bug Fixes

* add location to etag response ([3209e8e](https://github.com/informatievlaanderen/streetname-registry/commit/3209e8ec7c2531884901a398f5bbbde621a7f745))
* bump ticketing / lambda packages ([b44eb77](https://github.com/informatievlaanderen/streetname-registry/commit/b44eb77c7e20399529ee08ed1d0a1863973c969b))
* correct idempotency in lambda handlers ([421e4bd](https://github.com/informatievlaanderen/streetname-registry/commit/421e4bd9496606249239a2043f811c9176d44ca8))
* first implementation of retries ([e43146f](https://github.com/informatievlaanderen/streetname-registry/commit/e43146f7982de46cfdb9cf102ceff71f05b00ec8))
* revert make pr's trigger build ([b3f6c27](https://github.com/informatievlaanderen/streetname-registry/commit/b3f6c27d5ab1f786ed35ada1fc23e4a2aeb5067a))
* separate restore packages ([f3b80ce](https://github.com/informatievlaanderen/streetname-registry/commit/f3b80ce0be202acc47ee7ba75bc02f6056b56023))
* separate restore packages ([4dca151](https://github.com/informatievlaanderen/streetname-registry/commit/4dca1511b0ec42bfe5e4ae08a17e59014a219006))
* set build.yml as CI workflow ([beb0860](https://github.com/informatievlaanderen/streetname-registry/commit/beb0860a767e2bf505794301c41d881625be944a))
* set ifmatchheader on sqsrequest ([55fa162](https://github.com/informatievlaanderen/streetname-registry/commit/55fa162b86658579ccd58385417d1e525e990e5b))
* ticketing registration ([dc7e16a](https://github.com/informatievlaanderen/streetname-registry/commit/dc7e16a64bf8813cee4677553900425aaa89433c))


### Features

* change routes for propose and approve ([884fd38](https://github.com/informatievlaanderen/streetname-registry/commit/884fd38a0bdcba9d20fb4bbba1da8f5a7f7969e0))
* don't handle aggregatenotfoundexception in lambda ([c2e3491](https://github.com/informatievlaanderen/streetname-registry/commit/c2e3491b31f604b591f57295c957e38aaffc5e2a))
* don't process message which can't be cast to sqsrequest ([eb030a9](https://github.com/informatievlaanderen/streetname-registry/commit/eb030a9fefdc0a84304ea7ed3a78fad5e47c21f5))
* make other actions async ([f4ec62c](https://github.com/informatievlaanderen/streetname-registry/commit/f4ec62c18a7253d9b782f5dc438a185475c8a7d9))
* make propose streetname async ([e710e38](https://github.com/informatievlaanderen/streetname-registry/commit/e710e380c375a0c55322fe578c872d3e570fe011))
* passthrough SQS request metadata ([a07bed5](https://github.com/informatievlaanderen/streetname-registry/commit/a07bed55ccb84c77b771a129c5a101119386e34e))
* use different service lifetimescope per message ([5cd58d8](https://github.com/informatievlaanderen/streetname-registry/commit/5cd58d87816360dc0bc39621f5abbc384d2cea49))
* useSqs feature toggle ([3d60ba0](https://github.com/informatievlaanderen/streetname-registry/commit/3d60ba042570fc91fcb7231cdabeeade30c54fb1))
* validate ifmatchheadervalue in lambdas ([46c11be](https://github.com/informatievlaanderen/streetname-registry/commit/46c11be7f278557f920c58242ff83afc34dcdb05))

## [3.8.11](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.10...v3.8.11) (2022-09-06)

## [3.8.10](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.9...v3.8.10) (2022-09-05)


### Bug Fixes

* fix lambda destination in main.yml ([59abb7a](https://github.com/informatievlaanderen/streetname-registry/commit/59abb7a44f5228735a0afe9688f2f803e18b3d5f))

## [3.8.9](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.8...v3.8.9) (2022-09-05)


### Bug Fixes

* add --no-restore & --no-build ([d21ca1f](https://github.com/informatievlaanderen/streetname-registry/commit/d21ca1f78ce43f0680fd5e6899f4ef1216099b3e))
* add paket install ([eb7d0ce](https://github.com/informatievlaanderen/streetname-registry/commit/eb7d0cec8672d6f21e5f45e0a50af157772c5666))
* add repo name ([2dc1f25](https://github.com/informatievlaanderen/streetname-registry/commit/2dc1f25b72df769a6772bb429299f62496dcee5c))
* fix typo ([631ac7c](https://github.com/informatievlaanderen/streetname-registry/commit/631ac7cfefca2b75e1d45df57d192202a3624b56))
* remove --no-logo ([25d8fa6](https://github.com/informatievlaanderen/streetname-registry/commit/25d8fa6954800fab6b35cb230ee26a25a3e9f237))
* rename from CI2 to Build ([4dc4ae6](https://github.com/informatievlaanderen/streetname-registry/commit/4dc4ae632409cd9b01701a4cb21b02d5efccee17))
* sonar issues ([1eba90a](https://github.com/informatievlaanderen/streetname-registry/commit/1eba90a937212f012b086dd864a36a59ed057f2e))

## [3.8.8](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.7...v3.8.8) (2022-09-02)


### Bug Fixes

* comment lambda packaging ([1b6b324](https://github.com/informatievlaanderen/streetname-registry/commit/1b6b3249b32a1abdf43401126186ba3719ad49e9))

## [3.8.7](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.6...v3.8.7) (2022-09-02)


### Bug Fixes

* change MessageHandler & build scripts ([06720a6](https://github.com/informatievlaanderen/streetname-registry/commit/06720a6809927d23555361b6d2c859c34dd4ff40))
* duplicate items on publish ([aae6600](https://github.com/informatievlaanderen/streetname-registry/commit/aae6600d4e4f5cca05a6b46f4301095843ae1034))
* duplicate items on publish ([e781c55](https://github.com/informatievlaanderen/streetname-registry/commit/e781c55fabbb38963a998ef3b67121e23fa7558a))

## [3.8.6](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.5...v3.8.6) (2022-09-02)

## [3.8.5](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.4...v3.8.5) (2022-08-24)


### Bug Fixes

* snapshotting ([6e8373f](https://github.com/informatievlaanderen/streetname-registry/commit/6e8373fbfc02dff1e60c9c22a87735c97141c821))

## [3.8.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.3...v3.8.4) (2022-08-23)


### Bug Fixes

* add missing mapping for street name status Rejected ([b6d5e58](https://github.com/informatievlaanderen/streetname-registry/commit/b6d5e5815eab625b81b5c3bcd9a8e8692c4d50ff))

## [3.8.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.2...v3.8.3) (2022-08-23)


### Bug Fixes

* correct street name change order of validations ([f227404](https://github.com/informatievlaanderen/streetname-registry/commit/f227404fe3c2193b72afa417b82e91cf6a72983d))
* propose streetname validate on existing persistent local id ([437cc7a](https://github.com/informatievlaanderen/streetname-registry/commit/437cc7a406cb04d428e3aa9db8066d898d42f8ee))

## [3.8.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.1...v3.8.2) (2022-08-23)


### Bug Fixes

* status code docs ([e6b9e34](https://github.com/informatievlaanderen/streetname-registry/commit/e6b9e3402bb90327ce1060d4a3fa12bbbca54c5b))

## [3.8.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.8.0...v3.8.1) (2022-08-19)

# [3.8.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.7.3...v3.8.0) (2022-08-19)


### Features

* validate streetname correction has atleast one language specified ([03e5a0a](https://github.com/informatievlaanderen/streetname-registry/commit/03e5a0ad1715e74eead4badfa02716039bc1a8b8))

## [3.7.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.7.2...v3.7.3) (2022-08-16)

## [3.7.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.7.1...v3.7.2) (2022-08-16)


### Bug Fixes

* replace 409 by 400 on reject and retire streetname ([5225097](https://github.com/informatievlaanderen/streetname-registry/commit/52250972650c846a3fc3c896b2cea0295db40587))

## [3.7.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.7.0...v3.7.1) (2022-08-12)


### Bug Fixes

* correct streetname names replace 204 response ([f0e67ee](https://github.com/informatievlaanderen/streetname-registry/commit/f0e67ee912409bc2e7ab6a5abbe5131c65f41e24))

# [3.7.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.6.0...v3.7.0) (2022-08-12)


### Features

* streetname name was corrected ([f89554b](https://github.com/informatievlaanderen/streetname-registry/commit/f89554b75dada4c6e758e5d8763f9d749a465a76))

# [3.6.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.5.0...v3.6.0) (2022-08-12)


### Features

* return http status 202 instead of 204 for success ([761b037](https://github.com/informatievlaanderen/streetname-registry/commit/761b037853c78830a24cb6141924afce66d06a77))

# [3.5.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.4.0...v3.5.0) (2022-08-12)


### Features

* refactor exception properties to value objects ([117667e](https://github.com/informatievlaanderen/streetname-registry/commit/117667efa7ebf7ef43e1c1248145530720531066))

# [3.4.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.3.1...v3.4.0) (2022-08-11)


### Features

* add missing projection tests ([85e8389](https://github.com/informatievlaanderen/streetname-registry/commit/85e83898412b4e090ff180bd13e913610771b3d8))

## [3.3.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.3.0...v3.3.1) (2022-08-10)


### Bug Fixes

* review ([daad2d7](https://github.com/informatievlaanderen/streetname-registry/commit/daad2d70889591114a5bee987921c71a6c6d56bb))

# [3.3.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.2.0...v3.3.0) (2022-08-09)


### Features

* sqs refactor ([3d881b2](https://github.com/informatievlaanderen/streetname-registry/commit/3d881b24983281d133fca09325de681e6f23fcd5))

# [3.2.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.1.4...v3.2.0) (2022-08-05)


### Features

* mediator handlers + tests ([f7a8b10](https://github.com/informatievlaanderen/streetname-registry/commit/f7a8b10b8f70a569a417c56b4f20d2292ed8fbed))
* reject street name ([51be9ed](https://github.com/informatievlaanderen/streetname-registry/commit/51be9ed593723559299506b350ae50b99855dc15))

## [3.1.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.1.3...v3.1.4) (2022-07-11)

## [3.1.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.1.2...v3.1.3) (2022-07-06)


### Bug Fixes

* snapshot settings ([1df2107](https://github.com/informatievlaanderen/streetname-registry/commit/1df2107ca8e24b88fe68a5afdbc2c8293023d419))

## [3.1.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.1.1...v3.1.2) (2022-06-30)


### Bug Fixes

* update projection description ([e1c7bd5](https://github.com/informatievlaanderen/streetname-registry/commit/e1c7bd591d405d975dff6a1228983a6ef2dcb9e3))

## [3.1.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.1.0...v3.1.1) (2022-06-30)


### Bug Fixes

* add LABEL to Dockerfile (for easier DataDog filtering) ([abc9b26](https://github.com/informatievlaanderen/streetname-registry/commit/abc9b2618b1a23ac866d3217a30ab5f26df4904a))
* rename projection description ([0f518fb](https://github.com/informatievlaanderen/streetname-registry/commit/0f518fb22fcbee75d55fa1328f856d5d94fe5eb3))

# [3.1.0](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.11...v3.1.0) (2022-06-29)


### Features

* add snapshotting ([66672d6](https://github.com/informatievlaanderen/streetname-registry/commit/66672d6fccf68e67112a481e4d5bb76533c5346f))

## [3.0.11](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.10...v3.0.11) (2022-06-29)


### Bug Fixes

* bump packges and fix build issues after bump ([a9f8050](https://github.com/informatievlaanderen/streetname-registry/commit/a9f80504a45dcdb5aff928f9ff87a8784c0e404e))

## [3.0.10](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.9...v3.0.10) (2022-06-02)

## [3.0.9](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.8...v3.0.9) (2022-05-17)


### Bug Fixes

* update eventdescription StreetNameWasMigratedToMunicipality ([021a9dc](https://github.com/informatievlaanderen/streetname-registry/commit/021a9dc615b37673011569aac55860b91ec1cf2b))

## [3.0.8](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.7...v3.0.8) (2022-05-16)


### Bug Fixes

* add tags to new events ([2b342a6](https://github.com/informatievlaanderen/streetname-registry/commit/2b342a65122a29e7e959f317386ec72424eadc65))

## [3.0.7](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.6...v3.0.7) (2022-05-13)


### Bug Fixes

* upgrade message handling ([7ef8e29](https://github.com/informatievlaanderen/streetname-registry/commit/7ef8e29f0882816984672f6e66d1a64f6de90dfd))

## [3.0.6](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.5...v3.0.6) (2022-05-05)


### Bug Fixes

* add make complete for incomplete streetnames in staging ([cda7a76](https://github.com/informatievlaanderen/streetname-registry/commit/cda7a76938b58018b4f290c0479d1c90693c80bc))

## [3.0.5](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.4...v3.0.5) (2022-04-29)


### Bug Fixes

* run sonar end when release version != none ([35b3d9d](https://github.com/informatievlaanderen/streetname-registry/commit/35b3d9d79470b6cf56c15cdf8c2b1abeb0eba40e))

## [3.0.4](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.3...v3.0.4) (2022-04-27)


### Bug Fixes

* redirect sonar to /dev/null ([07296f8](https://github.com/informatievlaanderen/streetname-registry/commit/07296f8e6ccea6f605ee9c65a3440b6afc3cf3dd))

## [3.0.3](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.2...v3.0.3) (2022-04-04)


### Bug Fixes

* correct municipailty language for list streetnames GAWR-2970 ([46a5379](https://github.com/informatievlaanderen/streetname-registry/commit/46a53792c6f721e1484df58a86e9f15dc119b6d6))

## [3.0.2](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.1...v3.0.2) (2022-04-04)


### Bug Fixes

* bump grar-common and use event hash pipe & extension method ([9a75e99](https://github.com/informatievlaanderen/streetname-registry/commit/9a75e9990d4a749bc4c41a6f5beadedf486740d7))
* set oslo context type to string ([3c5f66a](https://github.com/informatievlaanderen/streetname-registry/commit/3c5f66aeadbbe8787bce4d26b2a3548442968b1c))

## [3.0.1](https://github.com/informatievlaanderen/streetname-registry/compare/v3.0.0...v3.0.1) (2022-03-29)


### Bug Fixes

* set kafka username/pw for producer ([20128a5](https://github.com/informatievlaanderen/streetname-registry/commit/20128a57616639b1d8782a3f1acacd1f54f005ec))

# [3.0.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.62.2...v3.0.0) (2022-03-29)


### Bug Fixes

* propose validation messages in NL ([80c45e3](https://github.com/informatievlaanderen/streetname-registry/commit/80c45e3336456eea0e3e195d3fb895594100bc21))


### Features

* move to dotnet 6.0.3 ([7bf80f2](https://github.com/informatievlaanderen/streetname-registry/commit/7bf80f2d9296f2de96584de5ca1201eb5397d195))


### BREAKING CHANGES

* move to dotnet 6.0.3

## [2.62.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.62.1...v2.62.2) (2022-03-28)


### Bug Fixes

* add producer to CI/CD ([513835a](https://github.com/informatievlaanderen/streetname-registry/commit/513835aac1667d332825c431ecbc7abb4dc964c0))

## [2.62.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.62.0...v2.62.1) (2022-03-21)


### Bug Fixes

* implement municipality streetname events ([dfc30c9](https://github.com/informatievlaanderen/streetname-registry/commit/dfc30c9176c54706adf02fcb44e66c3f06734ac4))

# [2.62.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.61.0...v2.62.0) (2022-03-21)


### Features

* add Producer ([#558](https://github.com/informatievlaanderen/streetname-registry/issues/558)) ([06af914](https://github.com/informatievlaanderen/streetname-registry/commit/06af9143b4144f3a487ea242a7c5a976b6a0e1d2))

# [2.61.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.60.0...v2.61.0) (2022-03-16)


### Features

* municipality status validation when approving streetname ([2a864af](https://github.com/informatievlaanderen/streetname-registry/commit/2a864afca6f7555d0e39bb070fe6a8e39d822013))

# [2.60.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.59.1...v2.60.0) (2022-03-16)


### Features

* approval validation 4, status not proposed ([51060a1](https://github.com/informatievlaanderen/streetname-registry/commit/51060a1be2555de72475ad922fd1efa1e5560ea7))

## [2.59.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.59.0...v2.59.1) (2022-03-16)


### Bug Fixes

* add rejected status and test with propose ([0c7c6ae](https://github.com/informatievlaanderen/streetname-registry/commit/0c7c6ae327074108122ccb93368cdf3cb7e68b50))

# [2.59.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.58.3...v2.59.0) (2022-03-15)


### Features

* validate streetname is found and not removed ([e5f7711](https://github.com/informatievlaanderen/streetname-registry/commit/e5f7711316ce876ea6267152777b340425ec9d70))

## [2.58.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.58.2...v2.58.3) (2022-03-15)


### Bug Fixes

* add municipality event tag on municipality events ([80eb619](https://github.com/informatievlaanderen/streetname-registry/commit/80eb6194cfcbc6ef02262da41366b6244340025a))

## [2.58.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.58.1...v2.58.2) (2022-03-15)


### Bug Fixes

* add Code to propose validators ([74b5706](https://github.com/informatievlaanderen/streetname-registry/commit/74b570639fc342e435f1720b7e8b9dac525db6f7))
* correct docs with events ([1eefcf9](https://github.com/informatievlaanderen/streetname-registry/commit/1eefcf956b7a47a284e4aee88fd0c40343a74996))
* correct property description ([8c6bba3](https://github.com/informatievlaanderen/streetname-registry/commit/8c6bba31d2a1093439ee483dd6dbac4f651cc425))

## [2.58.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.58.0...v2.58.1) (2022-03-14)


### Bug Fixes

* hide municipality events ([78cb8ff](https://github.com/informatievlaanderen/streetname-registry/commit/78cb8ff474593f4c074d1fb51e9ccd1847e8eae8))

# [2.58.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.57.1...v2.58.0) (2022-03-14)


### Features

* update projections for StreetNameWasApproved ([cd3c488](https://github.com/informatievlaanderen/streetname-registry/commit/cd3c488cc79e691bc23815cabcc2676f74016f72))

## [2.57.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.57.0...v2.57.1) (2022-03-14)


### Bug Fixes

* add migration persistent local id's to backoffice ([2f5394f](https://github.com/informatievlaanderen/streetname-registry/commit/2f5394f5ffac923e2f99081ebf86e9f54d4f2b60))

# [2.57.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.56.1...v2.57.0) (2022-03-14)


### Bug Fixes

* add docs ([91289b0](https://github.com/informatievlaanderen/streetname-registry/commit/91289b0a4737ed3aaebbcead50affd4f91a6c503))
* add property descriptions ([68569cb](https://github.com/informatievlaanderen/streetname-registry/commit/68569cb6d1e5b35cb9c6986207600fe11faff4e5))
* update paket.template in backoffice after removing reference ([dd26a89](https://github.com/informatievlaanderen/streetname-registry/commit/dd26a89fbd39c501fd43f3718e25e722921eea61))


### Features

* approve streetname endpoint backoffice ([fc513c2](https://github.com/informatievlaanderen/streetname-registry/commit/fc513c27a293e3bd6b0646a005704b62098c318b))
* update grar-common to 16.15.1 ([b8f6984](https://github.com/informatievlaanderen/streetname-registry/commit/b8f6984b6e1916db63020a0ef24a036c84d021fa))

## [2.56.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.56.0...v2.56.1) (2022-03-10)


### Bug Fixes

* can propose with retired duplicate name present GAWR-2843 ([47489d6](https://github.com/informatievlaanderen/streetname-registry/commit/47489d67de453154303c23c5f07b39c93b56eb20))

# [2.56.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.55.7...v2.56.0) (2022-03-10)


### Features

* approve streetname ([beb9ae4](https://github.com/informatievlaanderen/streetname-registry/commit/beb9ae4023f50ecedc1bba52532f5cec67af7fa8))

## [2.55.7](https://github.com/informatievlaanderen/streetname-registry/compare/v2.55.6...v2.55.7) (2022-03-09)


### Bug Fixes

* use nullable language for old events ([ef961cb](https://github.com/informatievlaanderen/streetname-registry/commit/ef961cb846d3160edc73ac9c5309af616a1673af))

## [2.55.6](https://github.com/informatievlaanderen/streetname-registry/compare/v2.55.5...v2.55.6) (2022-03-09)


### Bug Fixes

* use persistentlocalid as id for object in feed ([a48c289](https://github.com/informatievlaanderen/streetname-registry/commit/a48c289f4bb1efb390b93360bb985885deec0628))

## [2.55.5](https://github.com/informatievlaanderen/streetname-registry/compare/v2.55.4...v2.55.5) (2022-03-07)


### Bug Fixes

* update api for etag fix ([2ee757f](https://github.com/informatievlaanderen/streetname-registry/commit/2ee757fa15c27b2c174eaf674ca6d4eafd25eb33))

## [2.55.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.55.3...v2.55.4) (2022-03-07)


### Bug Fixes

* rebuild key and uri for v2 insert events ([e23f63c](https://github.com/informatievlaanderen/streetname-registry/commit/e23f63ce2d1e96e3e2a9c7bcb97cc139971f2d49))

## [2.55.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.55.2...v2.55.3) (2022-03-07)


### Bug Fixes

* name WFS adressen & name WMS adressen ([85f843d](https://github.com/informatievlaanderen/streetname-registry/commit/85f843dddc02f999de7f812bc0ddfaaeb83e53d3))

## [2.55.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.55.1...v2.55.2) (2022-03-04)


### Bug Fixes

* remade wms/wfs migrations cause of identity ([15ad445](https://github.com/informatievlaanderen/streetname-registry/commit/15ad445c07263e004eae9c9d7dc916f8320d34ec))

## [2.55.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.55.0...v2.55.1) (2022-03-04)


### Bug Fixes

* remove identity insert for wfs/wms v2 ([b0f41b4](https://github.com/informatievlaanderen/streetname-registry/commit/b0f41b483f370de7478a1deeefb1984a6f93028d))
* run wfs/wms v1 in v2 for testing ([77cb91d](https://github.com/informatievlaanderen/streetname-registry/commit/77cb91d6d889213e095b875f76a9b12dfdacf65e))

# [2.55.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.54.0...v2.55.0) (2022-03-04)


### Bug Fixes

* add missing files ([b2f3a48](https://github.com/informatievlaanderen/streetname-registry/commit/b2f3a48c62f18aa7c20c8c3c84dbf0738a7db56b))


### Features

* add v2 projections to projector with toggle ([8e024dd](https://github.com/informatievlaanderen/streetname-registry/commit/8e024dd85daf59c21bb0d1e4e0bbc99c8dee86f3))

# [2.54.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.53.0...v2.54.0) (2022-03-03)


### Features

* add hash to events ([e1e252d](https://github.com/informatievlaanderen/streetname-registry/commit/e1e252d375a40c3e831055319966f054ef98dd28))

# [2.53.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.52.3...v2.53.0) (2022-03-03)


### Features

* add retry policy for streetname migrator ([18bfc2e](https://github.com/informatievlaanderen/streetname-registry/commit/18bfc2ef755e9daf8b1e2f8e332c5f544c1efb2e))

## [2.52.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.52.2...v2.52.3) (2022-03-02)


### Bug Fixes

* style to retrigger build ([c0340c5](https://github.com/informatievlaanderen/streetname-registry/commit/c0340c53b53fc178dc554ef9886a59f1266b3745))

## [2.52.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.52.1...v2.52.2) (2022-03-02)


### Bug Fixes

* add consumer to connection strings ([c3caf10](https://github.com/informatievlaanderen/streetname-registry/commit/c3caf105b48caf0aadb71f27a988fc96def661da))

## [2.52.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.52.0...v2.52.1) (2022-03-02)

# [2.52.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.51.0...v2.52.0) (2022-02-28)


### Features

* update grar common for IHasHash ([ec77314](https://github.com/informatievlaanderen/streetname-registry/commit/ec7731483a8a1e37a6745bb2a51596974b76aaa3))

# [2.51.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.50.0...v2.51.0) (2022-02-28)


### Features

* add errorcodes to validationexception ([6c25f45](https://github.com/informatievlaanderen/streetname-registry/commit/6c25f45941a702e137fdf6f4089aadb5d6ff4067))

# [2.50.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.49.0...v2.50.0) (2022-02-25)


### Bug Fixes

* remove user secrets ([65c8977](https://github.com/informatievlaanderen/streetname-registry/commit/65c8977db4c452959ab05911da2effce8016c940))


### Features

* update api to 17.0.0 ([e9bac79](https://github.com/informatievlaanderen/streetname-registry/commit/e9bac797f75ac0cc269cdae448ed3b438d485edd))

# [2.49.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.48.0...v2.49.0) (2022-02-25)


### Features

* create wms projection ([4a1cab4](https://github.com/informatievlaanderen/streetname-registry/commit/4a1cab409fbc348da32a28d886922e2877ab7e72))

# [2.48.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.47.1...v2.48.0) (2022-02-23)


### Features

* build migrator streetname ([68a94af](https://github.com/informatievlaanderen/streetname-registry/commit/68a94af4e8546c4cb24a5dafbca147bbeb3603e8))

## [2.47.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.47.0...v2.47.1) (2022-02-22)

# [2.47.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.46.3...v2.47.0) (2022-02-22)


### Bug Fixes

* changes that wouldnt be added to commit ([f61f9d0](https://github.com/informatievlaanderen/streetname-registry/commit/f61f9d096868674230112bb8c48d979d2d840fcb))
* remove .Complete ([c081c8a](https://github.com/informatievlaanderen/streetname-registry/commit/c081c8a9f9d1827e9d14acdeb04e264c3374d1b7))
* remove Value from StreetNameWasMigratedToMunicipality.Status ([82287e0](https://github.com/informatievlaanderen/streetname-registry/commit/82287e0d1b9a5415de539117e59470c386423d29))


### Features

* do not migrate incomplete streetnames ([bd16610](https://github.com/informatievlaanderen/streetname-registry/commit/bd166106e442832525ae8ab82e379ee117ab1339))

## [2.46.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.46.2...v2.46.3) (2022-02-18)


### Bug Fixes

* update legacy projections migration event ([7beee58](https://github.com/informatievlaanderen/streetname-registry/commit/7beee58c8facbd09ddf15f77c4067a22205bd4df))

## [2.46.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.46.1...v2.46.2) (2022-02-18)


### Bug Fixes

* support kafka sasl authentication ([6f64b9d](https://github.com/informatievlaanderen/streetname-registry/commit/6f64b9ddd6242e2163c4599e5b124e01763b0d2e))

## [2.46.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.46.0...v2.46.1) (2022-02-16)


### Bug Fixes

* bump Kafka Simple ([00e49c2](https://github.com/informatievlaanderen/streetname-registry/commit/00e49c2693886e830be8aa9990a6b4bd0ce3505c))

# [2.46.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.45.2...v2.46.0) (2022-02-16)


### Bug Fixes

* got migrator working ([ab0d739](https://github.com/informatievlaanderen/streetname-registry/commit/ab0d739724df0126c1279200ea5b63644d84c241))


### Features

* add new command to mark legacy streetname migration ([a22cc0b](https://github.com/informatievlaanderen/streetname-registry/commit/a22cc0be127d9d50567c57c06f1b7bfa5505a4a6))
* add new municipalitystreamid ([8d0e25f](https://github.com/informatievlaanderen/streetname-registry/commit/8d0e25f3bb7d48e7f0cf7f49c5fc250ec80e20ce))
* add streetname migrator proj ([fcd0727](https://github.com/informatievlaanderen/streetname-registry/commit/fcd0727662245723b0cb279af2487710900e2afa))

## [2.45.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.45.1...v2.45.2) (2022-02-15)


### Bug Fixes

* consumer docker + assembly file ([f1ab955](https://github.com/informatievlaanderen/streetname-registry/commit/f1ab9552318270b8a078ba22c52e37c86ba99b16))

## [2.45.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.45.0...v2.45.1) (2022-02-14)


### Bug Fixes

* correct consumer non admin usage ([cad68d9](https://github.com/informatievlaanderen/streetname-registry/commit/cad68d9cc7af7b840ea7df3d24227692efb74d6c))

# [2.45.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.44.0...v2.45.0) (2022-02-14)


### Features

* migrateStreetName command with event 2708 ([315d126](https://github.com/informatievlaanderen/streetname-registry/commit/315d12650182d9116ff6b780011972cd6b478e75))

# [2.44.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.43.1...v2.44.0) (2022-02-14)


### Features

* add migration command ([b1cfba0](https://github.com/informatievlaanderen/streetname-registry/commit/b1cfba0e54166a3d6c02cb931c04591040b84b7a))

## [2.43.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.43.0...v2.43.1) (2022-02-14)

# [2.43.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.42.0...v2.43.0) (2022-02-11)


### Features

* create WFS projection helper GAWR-2241 ([5430cb6](https://github.com/informatievlaanderen/streetname-registry/commit/5430cb6be5f1d78b097648792da6389e1375c32c))

# [2.42.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.41.0...v2.42.0) (2022-02-11)


### Features

* add validation 4d, enable json error action filter ([d0bf6f2](https://github.com/informatievlaanderen/streetname-registry/commit/d0bf6f2be8b458640465afff079e213d1da3f428))

# [2.41.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.40.0...v2.41.0) (2022-02-09)


### Features

* add validation gawr-2688 4B ([4f20409](https://github.com/informatievlaanderen/streetname-registry/commit/4f204095c78736d4f763957772af0ddb59462b3e))
* add validation gawr-2688 4C ([5c8db3b](https://github.com/informatievlaanderen/streetname-registry/commit/5c8db3b23c475c38e4f086f0be698d243313cf71))

# [2.40.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.39.0...v2.40.0) (2022-02-09)


### Features

* add validation 8 gawr-2692 ([6712489](https://github.com/informatievlaanderen/streetname-registry/commit/6712489f438db63305916d463c979b7d7c191088))

# [2.39.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.38.0...v2.39.0) (2022-02-08)


### Features

* add validation gawr-2691 municipality retired ([71929b7](https://github.com/informatievlaanderen/streetname-registry/commit/71929b7a3be1ee3ebf8a8bb2f84d67516c2a4021))

# [2.38.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.37.0...v2.38.0) (2022-02-08)


### Features

* add validation 6 ([8f1af92](https://github.com/informatievlaanderen/streetname-registry/commit/8f1af924a787b2dedfbb79ee82d9beb6e3ccafc9))

# [2.37.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.36.0...v2.37.0) (2022-02-08)


### Features

* add validation gawr-2687 duplicate streetname ([25eff5b](https://github.com/informatievlaanderen/streetname-registry/commit/25eff5ba2c71d995663327553d766f3afc3e0535))

# [2.36.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.35.2...v2.36.0) (2022-02-08)


### Features

* add validator for propose streetname GAWR-1162 + GAWR-2686 ([1ccd100](https://github.com/informatievlaanderen/streetname-registry/commit/1ccd1008ea1f334f2d4fe7fd2a45bc8158fbb69c))

## [2.35.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.35.1...v2.35.2) (2022-02-07)


### Bug Fixes

* add retirementdate to retire command ([ffc15ee](https://github.com/informatievlaanderen/streetname-registry/commit/ffc15ee32f2829e2ea25d0827b5d4164fa883e8b))

## [2.35.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.35.0...v2.35.1) (2022-02-04)

# [2.35.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.34.1...v2.35.0) (2022-02-03)


### Bug Fixes

* fix build ([65c9f37](https://github.com/informatievlaanderen/streetname-registry/commit/65c9f37dd4d92d362568ae0a1b7d299d29c65498))
* modify Consumer paket.template ([dbaf823](https://github.com/informatievlaanderen/streetname-registry/commit/dbaf8234a8ff959affa1480f0a42555be4985ee1))
* modify Consumer paket.template yet again ([62e8482](https://github.com/informatievlaanderen/streetname-registry/commit/62e84821551aa167152eb5d8af6032f17e466e97))


### Features

* propose streetname ([10478ee](https://github.com/informatievlaanderen/streetname-registry/commit/10478eea838b3fc196576308e025f9a0da0d12ce))

## [2.34.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.34.0...v2.34.1) (2022-02-02)


### Bug Fixes

* import municipality ([5f0e7ad](https://github.com/informatievlaanderen/streetname-registry/commit/5f0e7ad87ce4d5daaf9dd3b96d226ebf13ebe262))

# [2.34.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.33.1...v2.34.0) (2022-02-01)


### Features

* add Kafka commands ([a7006a4](https://github.com/informatievlaanderen/streetname-registry/commit/a7006a4a0d8afd3965824e7873235021a4ff198a))

## [2.33.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.33.0...v2.33.1) (2022-01-28)


### Bug Fixes

* update message handling ([054fa09](https://github.com/informatievlaanderen/streetname-registry/commit/054fa092986c363bcce432e87eb64b9c103460ba))

# [2.33.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.13...v2.33.0) (2022-01-28)


### Features

* consuming messages without commandhandling ([575c838](https://github.com/informatievlaanderen/streetname-registry/commit/575c83860ca3cc8d1589b74dedef5b2ddede9312))

## [2.32.13](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.12...v2.32.13) (2022-01-21)


### Bug Fixes

* correctly resume projections asnyc ([78b5f84](https://github.com/informatievlaanderen/streetname-registry/commit/78b5f84d1fa68408659cbad2771f64188d84d337))

## [2.32.12](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.11...v2.32.12) (2022-01-18)


### Bug Fixes

* build ([aea0de1](https://github.com/informatievlaanderen/streetname-registry/commit/aea0de11fe257bce51bf99b2e88044754e2cc4f5))
* change oslo context & type ([e6fefda](https://github.com/informatievlaanderen/streetname-registry/commit/e6fefdaa835596c32527713f4bd5754aeccb747b))

## [2.32.11](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.10...v2.32.11) (2021-12-21)


### Bug Fixes

* gawr-2502 docs ([e287aa3](https://github.com/informatievlaanderen/streetname-registry/commit/e287aa3e767ef385a7148a5a77fa191fd4a5ac02))

## [2.32.10](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.9...v2.32.10) (2021-12-21)


### Bug Fixes

* gawr-2502 docs ([9af3e50](https://github.com/informatievlaanderen/streetname-registry/commit/9af3e50e0385021bdbc07a9072d929b4ed0fbf5b))

## [2.32.9](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.8...v2.32.9) (2021-12-21)


### Bug Fixes

* replaced contextobject in responses with perma link ([e72c688](https://github.com/informatievlaanderen/streetname-registry/commit/e72c688a7b754ac31413ee8e73cefbf480113e46))

## [2.32.8](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.7...v2.32.8) (2021-12-20)


### Bug Fixes

* bump version in backoffice to 2.0 ([77af2fe](https://github.com/informatievlaanderen/streetname-registry/commit/77af2fe3dea2f95eb5739f662dfd2b96fb254f0b))

## [2.32.7](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.6...v2.32.7) (2021-12-17)


### Bug Fixes

* use async startup of projections to fix hanging migrations ([e1b8f7c](https://github.com/informatievlaanderen/streetname-registry/commit/e1b8f7ceaab045c9e081a0f21ed669399883aad2))

## [2.32.6](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.5...v2.32.6) (2021-12-10)


### Bug Fixes

* change oslo context & type ([1193acc](https://github.com/informatievlaanderen/streetname-registry/commit/1193acc9fd77b3810189d6e0cd4b83e33c1d72f8))

## [2.32.5](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.4...v2.32.5) (2021-12-02)


### Bug Fixes

* add produce jsonld to totaal aantal ([a470bd7](https://github.com/informatievlaanderen/streetname-registry/commit/a470bd70a9855d7457fef0e6d0bc81337e86e7ff))

## [2.32.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.3...v2.32.4) (2021-12-01)


### Bug Fixes

* bump problemjson ([3af9a65](https://github.com/informatievlaanderen/streetname-registry/commit/3af9a65e7d565001efb509c7c7e56931cac849ce))

## [2.32.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.2...v2.32.3) (2021-12-01)


### Bug Fixes

* trigger build by correcting ident ([77464b8](https://github.com/informatievlaanderen/streetname-registry/commit/77464b89c3959fb2676de000168590e23e29e46b))

## [2.32.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.1...v2.32.2) (2021-12-01)


### Bug Fixes

* bump problemjson again ([af9386b](https://github.com/informatievlaanderen/streetname-registry/commit/af9386bbd0039a27d7b7fecb4fa8e487d36a4c1e))

## [2.32.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.32.0...v2.32.1) (2021-11-30)


### Bug Fixes

* GAWR-666 bump problemjson header package ([201cf75](https://github.com/informatievlaanderen/streetname-registry/commit/201cf75da75fe61924ac8edb07652288c15fcac2))

# [2.32.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.31.4...v2.32.0) (2021-11-29)


### Features

* add oslo to lastchangedlist projection + migrate data ([6d73fa6](https://github.com/informatievlaanderen/streetname-registry/commit/6d73fa60c5292381a0c9e9a048bc821894458040))

## [2.31.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.31.3...v2.31.4) (2021-11-29)


### Bug Fixes

* use problemjson middleware ([3f961f0](https://github.com/informatievlaanderen/streetname-registry/commit/3f961f06dcfb89dd8cb9b42b430bf08919c462ce))

## [2.31.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.31.2...v2.31.3) (2021-11-24)


### Bug Fixes

* rename oslo example classes ([9daa1ea](https://github.com/informatievlaanderen/streetname-registry/commit/9daa1eaf4247b737878b68ac3fc3e587dc08d42f))

## [2.31.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.31.1...v2.31.2) (2021-11-24)


### Bug Fixes

* rename oslo contracts ([f11b647](https://github.com/informatievlaanderen/streetname-registry/commit/f11b647af9c68b32eb93b25dad741ac914573d0a))

## [2.31.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.31.0...v2.31.1) (2021-11-24)


### Bug Fixes

* rename oslo query & response classes ([1bcdf4d](https://github.com/informatievlaanderen/streetname-registry/commit/1bcdf4d95049b00c722cd51c1bc1b7cde71bba3a))

# [2.31.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.30.0...v2.31.0) (2021-11-24)


### Features

* add context + type to oslo responses GAWR-666 ([065f8a0](https://github.com/informatievlaanderen/streetname-registry/commit/065f8a0d08c7474b4445827503f65f6b9ae8e225))

# [2.30.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.29.1...v2.30.0) (2021-11-23)


### Features

* create Api.Oslo project ([33e978a](https://github.com/informatievlaanderen/streetname-registry/commit/33e978a9ebebe4faebff0cdca5a8444ee36bceef))

## [2.29.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.29.0...v2.29.1) (2021-11-22)


### Bug Fixes

* don't run V2 of extract! ([0188666](https://github.com/informatievlaanderen/streetname-registry/commit/0188666f48b1882bc3264a332932718f61ebb74d))

# [2.29.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.28.2...v2.29.0) (2021-11-22)


### Bug Fixes

* correct migrations concerning existing indexes ([588a686](https://github.com/informatievlaanderen/streetname-registry/commit/588a686981938329b7b3a771d22f07066ff914a3))


### Features

* add position to ETag GAWR-2358 ([a1f4994](https://github.com/informatievlaanderen/streetname-registry/commit/a1f4994efbf81f27c58b60d16cb59a9e67f65745))

## [2.28.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.28.1...v2.28.2) (2021-11-18)


### Bug Fixes

* update docs backoffice GAWR-2349 ([188b667](https://github.com/informatievlaanderen/streetname-registry/commit/188b66771c95fc06efe562c3f9f7de8a53c1ed27))

## [2.28.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.28.0...v2.28.1) (2021-11-16)


### Bug Fixes

* correct projections + tests ([ff3b298](https://github.com/informatievlaanderen/streetname-registry/commit/ff3b29818b80e80cc12e90875ffc5fdcf3e8c838))

# [2.28.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.27.1...v2.28.0) (2021-11-16)


### Features

* add projections for new events ([b85a339](https://github.com/informatievlaanderen/streetname-registry/commit/b85a3391bbf5e1303224ee7b8365b183fb45c084))

## [2.27.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.27.0...v2.27.1) (2021-11-09)


### Bug Fixes

* include PersistentLocalId in ProposeStreetName command ([0c4fddc](https://github.com/informatievlaanderen/streetname-registry/commit/0c4fddcac92d00473093effec0406f05f9e83f93))

# [2.27.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.26.4...v2.27.0) (2021-11-08)


### Features

* add municipality commands/events GAWR-1161 ([cdf2fdb](https://github.com/informatievlaanderen/streetname-registry/commit/cdf2fdb12e9096fb202bcb772b9e6c30390184a4))

## [2.26.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.26.3...v2.26.4) (2021-10-28)


### Bug Fixes

* fake call ([5dba6c7](https://github.com/informatievlaanderen/streetname-registry/commit/5dba6c71edfaf2dacec516f8e1f93ecbd176ef5d))

## [2.26.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.26.2...v2.26.3) (2021-10-27)


### Bug Fixes

* trigger build ([47c9eb7](https://github.com/informatievlaanderen/streetname-registry/commit/47c9eb790f2ef6875051092db178c98c9ac160ce))

## [2.26.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.26.1...v2.26.2) (2021-10-25)


### Bug Fixes

* gawr-2202 paket bump ([3a3add5](https://github.com/informatievlaanderen/streetname-registry/commit/3a3add50ca028da5a93b781ffb032835d2350b7f))

## [2.26.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.26.0...v2.26.1) (2021-10-21)


### Bug Fixes

* gawr-2202 add api documentation ([1ac30e4](https://github.com/informatievlaanderen/streetname-registry/commit/1ac30e4ee381d13cb040f813a67b624415e975fb))

# [2.26.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.25.0...v2.26.0) (2021-10-20)


### Features

* add event + save to db ([e48792d](https://github.com/informatievlaanderen/streetname-registry/commit/e48792d96ecc550113ecd8d33632040665860c5e))

# [2.25.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.24.3...v2.25.0) (2021-10-19)


### Features

* GAWR-1179 handle command ([1759364](https://github.com/informatievlaanderen/streetname-registry/commit/17593646a3ad63711b09d3cad96e3926f9ba58a3))

## [2.24.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.24.2...v2.24.3) (2021-10-18)


### Bug Fixes

* etag ([ee6287e](https://github.com/informatievlaanderen/streetname-registry/commit/ee6287e9a5092b150f61d030d237c039e4049afe))

## [2.24.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.24.1...v2.24.2) (2021-10-18)


### Bug Fixes

* add etag to response header ([e1c135b](https://github.com/informatievlaanderen/streetname-registry/commit/e1c135b6038e161c4bffff4537f7efd01021adf1))

## [2.24.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.24.0...v2.24.1) (2021-10-15)


### Bug Fixes

* make properties required ([f28f669](https://github.com/informatievlaanderen/streetname-registry/commit/f28f669848021d864361c93110e3ab3db6082d9e))

# [2.24.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.23.1...v2.24.0) (2021-10-15)


### Bug Fixes

* docs on propose streetname ([8006132](https://github.com/informatievlaanderen/streetname-registry/commit/80061322dcf1dd77f0ee56fa232477e41bf7c2b0))


### Features

* add backoffice, update buildscript, gh pipeline, add first intern api endpoint ([5ed8a9f](https://github.com/informatievlaanderen/streetname-registry/commit/5ed8a9fbcb4a04a410c7b1dd68c1d5c60686012f))

## [2.23.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.23.0...v2.23.1) (2021-10-14)


### Bug Fixes

* build test ([4227aa1](https://github.com/informatievlaanderen/streetname-registry/commit/4227aa1b6bedaddd105ddbb56ae2cf7f841d6644))

# [2.23.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.9...v2.23.0) (2021-10-13)


### Features

* add backoffice api + propose endpoint GAWR-2064 ([6fc4c4b](https://github.com/informatievlaanderen/streetname-registry/commit/6fc4c4b971ca9b7a422e79f4e223a2fbaf414b49))

## [2.22.9](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.8...v2.22.9) (2021-10-06)


### Bug Fixes

* gawr-626 change doc language ([688e93c](https://github.com/informatievlaanderen/streetname-registry/commit/688e93cff76d2d32d293e1f16db4ff15df8c306d))

## [2.22.8](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.7...v2.22.8) (2021-10-06)


### Bug Fixes

* add Test to ECR ([05384ca](https://github.com/informatievlaanderen/streetname-registry/commit/05384ca6a2f7eaefd8fbd8c736aa1bc4be558c92))

## [2.22.7](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.6...v2.22.7) (2021-10-05)


### Bug Fixes

* grawr-615 versionid offset +2 ([07ad035](https://github.com/informatievlaanderen/streetname-registry/commit/07ad03599bb7cc76a1f1091b580751057feb9661))
* updated paket files ([c36df86](https://github.com/informatievlaanderen/streetname-registry/commit/c36df867fd076e339cddef32e0b87a7455118dad))

## [2.22.6](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.5...v2.22.6) (2021-10-01)


### Bug Fixes

* update packages ([0c32f64](https://github.com/informatievlaanderen/streetname-registry/commit/0c32f64774f9a8156b1ffd44badf899d9dcd6504))

## [2.22.5](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.4...v2.22.5) (2021-09-27)


### Bug Fixes

* gawr-618 voorbeeld straatnaam id sorteren ([7b16cb3](https://github.com/informatievlaanderen/streetname-registry/commit/7b16cb3949c8494f46fd7be4b10923258944636e))

## [2.22.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.3...v2.22.4) (2021-09-22)


### Bug Fixes

* gawr-611 fix exception detail ([49b97ad](https://github.com/informatievlaanderen/streetname-registry/commit/49b97ad3b69df3dc4e3901034d272f9988752185))

## [2.22.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.2...v2.22.3) (2021-09-22)


### Bug Fixes

* style to trigger build ([b7f18db](https://github.com/informatievlaanderen/streetname-registry/commit/b7f18dbf9cd2185b8c7148e0c08b13b92ff08616))

## [2.22.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.1...v2.22.2) (2021-09-20)


### Bug Fixes

* update package ([fd99fb2](https://github.com/informatievlaanderen/streetname-registry/commit/fd99fb21e2091e04541428d36506ad7864f718e5))

## [2.22.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.22.0...v2.22.1) (2021-08-26)


### Bug Fixes

* update grar-common dependencies GRAR-2060 ([20ae6e6](https://github.com/informatievlaanderen/streetname-registry/commit/20ae6e6cf6824fde2c0d0b3a7f4ae764171ea126))

# [2.22.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.9...v2.22.0) (2021-08-25)


### Features

* add metadata file with latest event id to street name extract GRAR-2060 ([6d8d62c](https://github.com/informatievlaanderen/streetname-registry/commit/6d8d62c019d7bb7b094cbe8903e31f0d335dabc4))

## [2.21.9](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.8...v2.21.9) (2021-06-25)


### Bug Fixes

* update aws DistributedMutex package ([7966039](https://github.com/informatievlaanderen/streetname-registry/commit/7966039efa956f6058092b5565d29d4710c7e0ac))

## [2.21.8](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.7...v2.21.8) (2021-06-25)


### Bug Fixes

* add unique constraint to persistentlocalid ([bf5d7f8](https://github.com/informatievlaanderen/streetname-registry/commit/bf5d7f85d73a4f6584af18f843e78538d767053c))

## [2.21.7](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.6...v2.21.7) (2021-06-17)


### Bug Fixes

* update nuget package ([3d79968](https://github.com/informatievlaanderen/streetname-registry/commit/3d7996856567d7c34a2c6425616c724d43750bd6))

## [2.21.6](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.5...v2.21.6) (2021-06-11)


### Bug Fixes

* fix niscode filter ([4f4550a](https://github.com/informatievlaanderen/streetname-registry/commit/4f4550a0f311ea406aef64aeb224028f74edd6a4))

## [2.21.5](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.4...v2.21.5) (2021-06-09)


### Bug Fixes

* add streetnamesearch fields, migration ([9ce1064](https://github.com/informatievlaanderen/streetname-registry/commit/9ce1064bb100c3d55cd95ce74b90a34073a9321e))

## [2.21.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.3...v2.21.4) (2021-06-09)


### Bug Fixes

* add nis code filter ([97314f0](https://github.com/informatievlaanderen/streetname-registry/commit/97314f0010e489cf7188c04e75164e88ecee80a6))

## [2.21.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.2...v2.21.3) (2021-05-31)


### Bug Fixes

* bump api ([5dfd737](https://github.com/informatievlaanderen/streetname-registry/commit/5dfd7377b5137f9f1d7c56235d75a306b984622b))

## [2.21.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.1...v2.21.2) (2021-05-31)


### Bug Fixes

* update api ([240b5ad](https://github.com/informatievlaanderen/streetname-registry/commit/240b5adbeb14bb444d3cafecb2d904a824acb3b2))

## [2.21.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.21.0...v2.21.1) (2021-05-29)


### Bug Fixes

* move to 5.0.6 ([ca8c146](https://github.com/informatievlaanderen/streetname-registry/commit/ca8c146ac2d8ca6f2bd33c2b6ca23918635f0d9a))

# [2.21.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.20.0...v2.21.0) (2021-05-04)


### Features

* bump packages ([a1ec84c](https://github.com/informatievlaanderen/streetname-registry/commit/a1ec84c1981e0ce2143acfe9ae4d6adc2bdca312))

# [2.20.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.19.1...v2.20.0) (2021-04-28)


### Features

* add status filter on legacy list ([ad1563b](https://github.com/informatievlaanderen/streetname-registry/commit/ad1563bccbd4df234cde6e47fcef27052f32fab7))

## [2.19.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.19.0...v2.19.1) (2021-04-26)


### Bug Fixes

* rename cache status endpoint in projector ([367fddb](https://github.com/informatievlaanderen/streetname-registry/commit/367fddb8ca95b063a201f833b579cd0b6eeea7c9))

# [2.19.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.18.2...v2.19.0) (2021-03-31)


### Bug Fixes

* update docs projections ([7c2f5e2](https://github.com/informatievlaanderen/streetname-registry/commit/7c2f5e227fb0c07f800b11e50d50c7ec3de04a05))


### Features

* bump projector & projection handling ([fe9736a](https://github.com/informatievlaanderen/streetname-registry/commit/fe9736a1fdb4808382e0245772fb2ca22b0257f3))

## [2.18.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.18.1...v2.18.2) (2021-03-22)


### Bug Fixes

* remove ridingwolf, collaboration ended ([efe6fe3](https://github.com/informatievlaanderen/streetname-registry/commit/efe6fe337ef56c4d86ae2fd98eba61b561ddb333))

## [2.18.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.18.0...v2.18.1) (2021-03-17)


### Bug Fixes

* change tags language events GRAR-1898 ([ecadbe5](https://github.com/informatievlaanderen/streetname-registry/commit/ecadbe5c4966766062d529cb170eebe465f2341d))

# [2.18.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.17.4...v2.18.0) (2021-03-11)


### Bug Fixes

* update projector dependency GRAR-1876 ([bd26a3d](https://github.com/informatievlaanderen/streetname-registry/commit/bd26a3dd808cfbd666154cc15ced38fb8828a59e))


### Features

* add projection attributes GRAR-1876 ([2d30d48](https://github.com/informatievlaanderen/streetname-registry/commit/2d30d48eba607e5efe29ff44e44f92ff28129286))

## [2.17.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.17.3...v2.17.4) (2021-03-08)


### Bug Fixes

* remove streetname versions GRAR-1876 ([df2ea71](https://github.com/informatievlaanderen/streetname-registry/commit/df2ea71a701be229759d76b89902f8e12f4dccfb))

## [2.17.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.17.2...v2.17.3) (2021-02-15)


### Bug Fixes

* register problem details helper for projector GRAR-1814 ([1dac227](https://github.com/informatievlaanderen/streetname-registry/commit/1dac227c8373885aaec6988f53b66eea390bb221))

## [2.17.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.17.1...v2.17.2) (2021-02-11)


### Bug Fixes

* update api with use of problemdetailshelper GRAR-1814 ([d0e549f](https://github.com/informatievlaanderen/streetname-registry/commit/d0e549f6f707caba1f2e819c764360a8a44758ab))

## [2.17.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.17.0...v2.17.1) (2021-02-02)


### Bug Fixes

* move to 5.0.2 ([d60e19b](https://github.com/informatievlaanderen/streetname-registry/commit/d60e19be61ffa7285edd7f5630fdd3650c38821b))

# [2.17.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.16.2...v2.17.0) (2021-01-30)


### Features

* add sync tag on events ([89d8f3e](https://github.com/informatievlaanderen/streetname-registry/commit/89d8f3e09fef5af44efd7532c753a6f3dc1b502d))

## [2.16.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.16.1...v2.16.2) (2021-01-29)


### Bug Fixes

* remove sync alternate links ([5982eb7](https://github.com/informatievlaanderen/streetname-registry/commit/5982eb7eaf1eaa25c675c20eecbd8648b58656f7))

## [2.16.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.16.0...v2.16.1) (2021-01-19)


### Bug Fixes

* xml date serialization sync projection ([3e2b28e](https://github.com/informatievlaanderen/streetname-registry/commit/3e2b28eafc07e46263c40e7b97babaf752efdadd))

# [2.16.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.15.8...v2.16.0) (2021-01-12)


### Features

* add syndication status to projector api ([5d681f5](https://github.com/informatievlaanderen/streetname-registry/commit/5d681f5799a3663cc7632bb1b1de010b0d2dc65d))

## [2.15.8](https://github.com/informatievlaanderen/streetname-registry/compare/v2.15.7...v2.15.8) (2021-01-07)


### Bug Fixes

* speed up cache status ([ef0e4db](https://github.com/informatievlaanderen/streetname-registry/commit/ef0e4db95d1cdbb6603abda35943836335610c40))

## [2.15.7](https://github.com/informatievlaanderen/streetname-registry/compare/v2.15.6...v2.15.7) (2021-01-07)


### Bug Fixes

* update deps ([7acf78e](https://github.com/informatievlaanderen/streetname-registry/commit/7acf78e550f8e895473096eda764cee9917b6d38))

## [2.15.6](https://github.com/informatievlaanderen/streetname-registry/compare/v2.15.5...v2.15.6) (2020-12-28)


### Bug Fixes

* update basisregisters api dependency ([3b162eb](https://github.com/informatievlaanderen/streetname-registry/commit/3b162eb6369788505e11857182ce2b0d2e69f927))

## [2.15.5](https://github.com/informatievlaanderen/streetname-registry/compare/v2.15.4...v2.15.5) (2020-12-21)


### Bug Fixes

* move to 5.0.1 ([c5d4b92](https://github.com/informatievlaanderen/streetname-registry/commit/c5d4b92787a47d6805121e7e6568b83b1b1fef01))

## [2.15.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.15.3...v2.15.4) (2020-11-19)


### Bug Fixes

* remove set-env usage in gh-actions ([a7ef9ea](https://github.com/informatievlaanderen/streetname-registry/commit/a7ef9eabde5230de613d16fe619f7415da06c33c))
* update references for event property descriptions ([6e9bf93](https://github.com/informatievlaanderen/streetname-registry/commit/6e9bf93d99750b01930474099d81657b13f25b0d))

## [2.15.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.15.2...v2.15.3) (2020-11-13)


### Bug Fixes

* display sync response example as correct xml GRAR-1599 ([6128480](https://github.com/informatievlaanderen/streetname-registry/commit/61284806d5c829ab1eeeccd4aa41d8005a014098))
* upgrade swagger GRAR-1599 ([70906f6](https://github.com/informatievlaanderen/streetname-registry/commit/70906f664c6da2d3defc51646d67487f82dcbd40))

## [2.15.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.15.1...v2.15.2) (2020-11-06)


### Bug Fixes

* logging ([cacf938](https://github.com/informatievlaanderen/streetname-registry/commit/cacf9388d5279e33805a6836164fe59650e2bf9b))
* logging ([655a5e3](https://github.com/informatievlaanderen/streetname-registry/commit/655a5e3078da70356e79a8e3cbb0ae68178736e9))
* logging ([73b7615](https://github.com/informatievlaanderen/streetname-registry/commit/73b76157dc67ae7da85419e7bae72f11948c9fff))
* logging ([93697bc](https://github.com/informatievlaanderen/streetname-registry/commit/93697bcb4a0641c2d17d1cd6f70873e1be573022))
* logging ([d9e7321](https://github.com/informatievlaanderen/streetname-registry/commit/d9e73215440833c0b24410a91ba0882756a3e696))
* logging ([e96a710](https://github.com/informatievlaanderen/streetname-registry/commit/e96a7108434d370bcb3ff9cf69139034a3872a21))

## [2.15.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.15.0...v2.15.1) (2020-11-04)


### Bug Fixes

* correct homonymaddition for object in sync api GRAR-1626 ([d9d3e31](https://github.com/informatievlaanderen/streetname-registry/commit/d9d3e314b1c8d9d44f7c2ffe026054ed2a75ae05))

# [2.15.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.14.0...v2.15.0) (2020-10-27)


### Features

* add error message for syndication projections ([4b19b50](https://github.com/informatievlaanderen/streetname-registry/commit/4b19b506c5af12cdaa4a846d6938a5261e3440f4))

# [2.14.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.13.1...v2.14.0) (2020-10-27)


### Features

* update projector with gap detection and extended status api ([ac8d5ce](https://github.com/informatievlaanderen/streetname-registry/commit/ac8d5ce0af2eeb674df1c4cf07fb958c403cf362))

## [2.13.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.13.0...v2.13.1) (2020-10-14)


### Bug Fixes

* correct merge statement in migration AddStatusList ([48342e9](https://github.com/informatievlaanderen/streetname-registry/commit/48342e9c24b3d21559b562d712d4eadcb766d49d))

# [2.13.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.12.1...v2.13.0) (2020-10-13)


### Features

* add status to legacy list ([20c741c](https://github.com/informatievlaanderen/streetname-registry/commit/20c741cd12742cc2fd02e12eb826ec902942d8d5))

## [2.12.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.12.0...v2.12.1) (2020-10-05)


### Bug Fixes

* run projection using the feedprojector GRAR-1562 ([23a551a](https://github.com/informatievlaanderen/streetname-registry/commit/23a551a57ac4995d46e07d5c22744b0ddc82152c))

# [2.12.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.11.0...v2.12.0) (2020-10-01)


### Features

* add cache status to projector api ([ecbc48d](https://github.com/informatievlaanderen/streetname-registry/commit/ecbc48d0c1bc24d4ef5972d3cdfbc692ee795650))

# [2.11.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.10.7...v2.11.0) (2020-09-22)


### Features

* add import status endpoint GRAR-1400 ([c26fa70](https://github.com/informatievlaanderen/streetname-registry/commit/c26fa700ee5a73e91e7922701fe4c0898997cf16))

## [2.10.7](https://github.com/informatievlaanderen/streetname-registry/compare/v2.10.6...v2.10.7) (2020-09-22)


### Bug Fixes

* move to 3.1.8 ([d8dd4ac](https://github.com/informatievlaanderen/streetname-registry/commit/d8dd4ac94189b23627826c07dfaf90e40dd3a4df))

## [2.10.6](https://github.com/informatievlaanderen/streetname-registry/compare/v2.10.5...v2.10.6) (2020-09-11)


### Bug Fixes

* remove Modification from xml GRAR-1529 ([4b85dc7](https://github.com/informatievlaanderen/streetname-registry/commit/4b85dc768d91b086bc500c0ae2fd5edec8f79733))

## [2.10.5](https://github.com/informatievlaanderen/streetname-registry/compare/v2.10.4...v2.10.5) (2020-09-11)


### Bug Fixes

* update packages to fix null operator/reason GRAR-1535 ([1b43cfa](https://github.com/informatievlaanderen/streetname-registry/commit/1b43cfa854177829815a820ed277f3e7960612e9))

## [2.10.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.10.3...v2.10.4) (2020-09-10)


### Bug Fixes

* add generator version GRAR-1540 ([b0ee494](https://github.com/informatievlaanderen/streetname-registry/commit/b0ee4942140665cf8f7e3d5dd9123e0ea96e5fb8))

## [2.10.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.10.2...v2.10.3) (2020-09-03)


### Bug Fixes

* null organisation defaults to unknown ([9395ebb](https://github.com/informatievlaanderen/streetname-registry/commit/9395ebbc768247904b188db2f337c46738e4cbf4))

## [2.10.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.10.1...v2.10.2) (2020-09-02)


### Bug Fixes

* upgarde common to fix sync author ([912d1f0](https://github.com/informatievlaanderen/streetname-registry/commit/912d1f0dc27d625d7ddec8f78f686ebf0d2e83a5))

## [2.10.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.10.0...v2.10.1) (2020-07-19)


### Bug Fixes

* move to 3.1.6 ([abfc092](https://github.com/informatievlaanderen/streetname-registry/commit/abfc092d0447a486160f1afba08a91ce7895c2bc))

# [2.10.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.9.5...v2.10.0) (2020-07-14)


### Features

* add timestamp to sync provenance GRAR-1451 ([1b069bc](https://github.com/informatievlaanderen/streetname-registry/commit/1b069bc08ef02a822f9daecc0f10b36b244c627d))

## [2.9.5](https://github.com/informatievlaanderen/streetname-registry/compare/v2.9.4...v2.9.5) (2020-07-13)


### Bug Fixes

* update dependencies ([e0047f0](https://github.com/informatievlaanderen/streetname-registry/commit/e0047f08c05023577e201705d5e16baeecf7b048))
* use typed embed value GRAR-1465 ([948f242](https://github.com/informatievlaanderen/streetname-registry/commit/948f242c6d95f8273b44cd47b05119f463d71993))

## [2.9.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.9.3...v2.9.4) (2020-07-10)


### Bug Fixes

* correct author, entry links atom feed + example GRAR-1443 GRAR-1447 ([0f040ee](https://github.com/informatievlaanderen/streetname-registry/commit/0f040eefa5065e2690255960e99d2f70ffa3a9d6))

## [2.9.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.9.2...v2.9.3) (2020-07-10)


### Bug Fixes

* enums were not correctly serialized in syndication event GRAR-1490 ([107d1ac](https://github.com/informatievlaanderen/streetname-registry/commit/107d1ac9060be513aed2c8a2592368f61a0287d3))

## [2.9.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.9.1...v2.9.2) (2020-07-03)


### Bug Fixes

* correct migration script GRAR-1442 ([70710cf](https://github.com/informatievlaanderen/streetname-registry/commit/70710cfaa9f932e0261878f29cc7e8944b60048e))

## [2.9.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.9.0...v2.9.1) (2020-07-03)


### Bug Fixes

* add SyndicationItemCreatedAt GRAR-1442 ([284f5a2](https://github.com/informatievlaanderen/streetname-registry/commit/284f5a2755947f0b92f47f63508099782276f67e))
* get updated value from projections GRAR-1442 ([9e19a4d](https://github.com/informatievlaanderen/streetname-registry/commit/9e19a4dcd8a623655f8f8f7899968947b00fb62a))
* run CI only on InformatiaVlaanderen repo ([f4cd78e](https://github.com/informatievlaanderen/streetname-registry/commit/f4cd78e208fad6ff930d66c4f24fdfeabaf12b5e))
* update dependencies ([90b69e7](https://github.com/informatievlaanderen/streetname-registry/commit/90b69e76594d7810e161cc21b0d72b7fb4ca99aa))

# [2.9.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.15...v2.9.0) (2020-07-01)


### Features

* refactor metadata for atom feed-metadata GRAR-1436 GRAR-1445 GRAR-1453 GRAR-1455 ([b24b12f](https://github.com/informatievlaanderen/streetname-registry/commit/b24b12fcec061b9c1852527bf02f9f2191780556))

## [2.8.15](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.14...v2.8.15) (2020-06-30)


### Bug Fixes

* remove offset and add from to next uri GRAR-1418 ([b1669ad](https://github.com/informatievlaanderen/streetname-registry/commit/b1669ade3342a1fbed32ab1c0affa0278b429936))

## [2.8.14](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.13...v2.8.14) (2020-06-23)


### Bug Fixes

* configure baseurls for all problemdetails GRAR-1357 ([ee0043c](https://github.com/informatievlaanderen/streetname-registry/commit/ee0043c90c4ccda5761e54d65670cc482a5e6276))

## [2.8.13](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.12...v2.8.13) (2020-06-22)


### Bug Fixes

* configure baseurls for all problemdetails GRAR-1358 GRAR-1357 ([6844438](https://github.com/informatievlaanderen/streetname-registry/commit/684443831e4996f2aa6486daff764c063935433e))

## [2.8.12](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.11...v2.8.12) (2020-06-19)


### Bug Fixes

* move to 3.1.5 ([db00db5](https://github.com/informatievlaanderen/streetname-registry/commit/db00db59b5ba330e57ffe54e6e86abedfa68ad44))

## [2.8.11](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.10...v2.8.11) (2020-06-08)


### Bug Fixes

* build msil version for public api ([1e21df7](https://github.com/informatievlaanderen/streetname-registry/commit/1e21df71eaeb6f4dcca39aad47140155f35c3231))

## [2.8.10](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.9...v2.8.10) (2020-05-29)


### Bug Fixes

* update dependencies GRAR-752 ([9873989](https://github.com/informatievlaanderen/streetname-registry/commit/98739890264bdeeb349489d52068b8bccf8f584f))

## [2.8.9](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.8...v2.8.9) (2020-05-20)


### Bug Fixes

* add build badge ([310bc9e](https://github.com/informatievlaanderen/streetname-registry/commit/310bc9eec2b32d47d21152aa8d385ea1a8af62b6))

## [2.8.8](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.7...v2.8.8) (2020-05-19)


### Bug Fixes

* move to 3.1.4 and gh-actions ([59f5c6c](https://github.com/informatievlaanderen/streetname-registry/commit/59f5c6c8c4841802ecb0f3ac7b250bf3a18e3d58))

## [2.8.7](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.6...v2.8.7) (2020-04-28)


### Bug Fixes

* update grar dependencies GRAR-412 ([155a7db](https://github.com/informatievlaanderen/streetname-registry/commit/155a7db))

## [2.8.6](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.5...v2.8.6) (2020-04-14)


### Bug Fixes

* now compiles importer after package update ([78067b0](https://github.com/informatievlaanderen/streetname-registry/commit/78067b0))

## [2.8.5](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.4...v2.8.5) (2020-04-14)


### Bug Fixes

* update import packages ([cd03b79](https://github.com/informatievlaanderen/streetname-registry/commit/cd03b79))

## [2.8.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.3...v2.8.4) (2020-04-10)


### Bug Fixes

* upgrade common packages ([8843cbf](https://github.com/informatievlaanderen/streetname-registry/commit/8843cbf))

## [2.8.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.2...v2.8.3) (2020-04-10)


### Bug Fixes

* update grar-common packages ([debc262](https://github.com/informatievlaanderen/streetname-registry/commit/debc262))

## [2.8.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.1...v2.8.2) (2020-04-09)


### Bug Fixes

* update packages for import batch timestamps ([ee62c56](https://github.com/informatievlaanderen/streetname-registry/commit/ee62c56))

## [2.8.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.8.0...v2.8.1) (2020-04-06)


### Bug Fixes

* set name for importer feedname ([f588e29](https://github.com/informatievlaanderen/streetname-registry/commit/f588e29))

# [2.8.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.7.4...v2.8.0) (2020-04-03)


### Features

* upgrade projection handling to include errmessage lastchangedlist ([b8850ae](https://github.com/informatievlaanderen/streetname-registry/commit/b8850ae))

## [2.7.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.7.3...v2.7.4) (2020-03-27)


### Bug Fixes

* set sync feed dates to belgian timezone ([cb6e2bc](https://github.com/informatievlaanderen/streetname-registry/commit/cb6e2bc))

## [2.7.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.7.2...v2.7.3) (2020-03-23)


### Bug Fixes

* update grar common to fix versie id type ([7d4a7b1](https://github.com/informatievlaanderen/streetname-registry/commit/7d4a7b1))
* versie id type change to string for sync resources ([4e70471](https://github.com/informatievlaanderen/streetname-registry/commit/4e70471))

## [2.7.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.7.1...v2.7.2) (2020-03-20)


### Bug Fixes

* update grar import package ([48a4d18](https://github.com/informatievlaanderen/streetname-registry/commit/48a4d18))

## [2.7.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.7.0...v2.7.1) (2020-03-19)


### Bug Fixes

* use correct build user ([ea26b87](https://github.com/informatievlaanderen/streetname-registry/commit/ea26b87))

# [2.7.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.6.1...v2.7.0) (2020-03-19)


### Features

* send mail when importer crashes ([2ceb53d](https://github.com/informatievlaanderen/streetname-registry/commit/2ceb53d))

## [2.6.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.6.0...v2.6.1) (2020-03-17)


### Bug Fixes

* force build ([f2b6b2c](https://github.com/informatievlaanderen/streetname-registry/commit/f2b6b2c))

# [2.6.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.5.2...v2.6.0) (2020-03-17)


### Features

* upgrade importer to netcore3 ([78ab7c9](https://github.com/informatievlaanderen/streetname-registry/commit/78ab7c9))

## [2.5.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.5.1...v2.5.2) (2020-03-11)

## [2.5.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.5.0...v2.5.1) (2020-03-11)


### Bug Fixes

* count streetname now counts correctly when filtered ([313e952](https://github.com/informatievlaanderen/streetname-registry/commit/313e952))

# [2.5.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.16...v2.5.0) (2020-03-10)


### Features

* add totaal aantal endpoint ([cf348b5](https://github.com/informatievlaanderen/streetname-registry/commit/cf348b5))

## [2.4.16](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.15...v2.4.16) (2020-03-05)


### Bug Fixes

* update grar common to fix provenance ([c63f2a7](https://github.com/informatievlaanderen/streetname-registry/commit/c63f2a7))

## [2.4.15](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.14...v2.4.15) (2020-03-04)


### Bug Fixes

* bump netcore dockerfiles ([e08f517](https://github.com/informatievlaanderen/streetname-registry/commit/e08f517))

## [2.4.14](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.13...v2.4.14) (2020-03-03)


### Bug Fixes

* bump netcore 3.1.2 ([49b5880](https://github.com/informatievlaanderen/streetname-registry/commit/49b5880))
* update dockerid detection ([637ed8d](https://github.com/informatievlaanderen/streetname-registry/commit/637ed8d))

## [2.4.13](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.12...v2.4.13) (2020-02-27)


### Bug Fixes

* update json serialization dependencies ([a8ab6e7](https://github.com/informatievlaanderen/streetname-registry/commit/a8ab6e7))

## [2.4.12](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.11...v2.4.12) (2020-02-26)


### Bug Fixes

* increase bosa result size to 1001 ([ea102c3](https://github.com/informatievlaanderen/streetname-registry/commit/ea102c3))

## [2.4.11](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.10...v2.4.11) (2020-02-24)


### Bug Fixes

* update projection handling & update sync migrator ([92029bd](https://github.com/informatievlaanderen/streetname-registry/commit/92029bd))

## [2.4.10](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.9...v2.4.10) (2020-02-21)


### Performance Improvements

* increase performance by removing count from lists ([2212fd2](https://github.com/informatievlaanderen/streetname-registry/commit/2212fd2))

## [2.4.9](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.8...v2.4.9) (2020-02-20)


### Bug Fixes

* update grar common ([2af230f](https://github.com/informatievlaanderen/streetname-registry/commit/2af230f))

## [2.4.8](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.7...v2.4.8) (2020-02-19)


### Bug Fixes

* add order by in api's + add clustered index bosa ([29f401a](https://github.com/informatievlaanderen/streetname-registry/commit/29f401a))

## [2.4.7](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.6...v2.4.7) (2020-02-17)


### Bug Fixes

* upgrade packages to fix json order ([cda78af](https://github.com/informatievlaanderen/streetname-registry/commit/cda78af))

## [2.4.6](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.5...v2.4.6) (2020-02-14)


### Bug Fixes

* add list index ([d71ffd5](https://github.com/informatievlaanderen/streetname-registry/commit/d71ffd5))

## [2.4.5](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.4...v2.4.5) (2020-02-10)


### Bug Fixes

* JSON default value for nullable fields ([0e297d5](https://github.com/informatievlaanderen/streetname-registry/commit/0e297d5))

## [2.4.4](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.3...v2.4.4) (2020-02-04)


### Bug Fixes

* instance uri for error examples now show correctly ([6da02d0](https://github.com/informatievlaanderen/streetname-registry/commit/6da02d0))

## [2.4.3](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.2...v2.4.3) (2020-02-03)


### Bug Fixes

* add type to problemdetails ([227a301](https://github.com/informatievlaanderen/streetname-registry/commit/227a301))

## [2.4.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.1...v2.4.2) (2020-02-03)


### Bug Fixes

* homoniemToevoeging can be null ([6eb91c8](https://github.com/informatievlaanderen/streetname-registry/commit/6eb91c8))
* next url is nullable ([ac03c71](https://github.com/informatievlaanderen/streetname-registry/commit/ac03c71))

## [2.4.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.4.0...v2.4.1) (2020-02-03)


### Bug Fixes

* specify non nullable responses ([7330a61](https://github.com/informatievlaanderen/streetname-registry/commit/7330a61))

# [2.4.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.3.2...v2.4.0) (2020-02-01)


### Features

* upgrade netcoreapp31 and dependencies ([77171a8](https://github.com/informatievlaanderen/streetname-registry/commit/77171a8))

## [2.3.2](https://github.com/informatievlaanderen/streetname-registry/compare/v2.3.1...v2.3.2) (2020-01-24)


### Bug Fixes

* add syndication to api references ([d2c24de](https://github.com/informatievlaanderen/streetname-registry/commit/d2c24de))

## [2.3.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.3.0...v2.3.1) (2020-01-23)


### Bug Fixes

* syndication distributedlock now runs async ([76a1985](https://github.com/informatievlaanderen/streetname-registry/commit/76a1985))

# [2.3.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.2.0...v2.3.0) (2020-01-23)


### Features

* upgrade projectionhandling package ([af8beb4](https://github.com/informatievlaanderen/streetname-registry/commit/af8beb4))

# [2.2.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.1.1...v2.2.0) (2020-01-23)


### Features

* use distributed lock for syndication ([330ca69](https://github.com/informatievlaanderen/streetname-registry/commit/330ca69))

## [2.1.1](https://github.com/informatievlaanderen/streetname-registry/compare/v2.1.0...v2.1.1) (2020-01-16)


### Bug Fixes

* get api's working again ([52c9edf](https://github.com/informatievlaanderen/streetname-registry/commit/52c9edf))

# [2.1.0](https://github.com/informatievlaanderen/streetname-registry/compare/v2.0.0...v2.1.0) (2020-01-03)


### Features

* allow only one projector instance ([c668b77](https://github.com/informatievlaanderen/streetname-registry/commit/c668b77))

# [2.0.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.19.5...v2.0.0) (2019-12-24)


### Code Refactoring

* upgrade to netcoreapp31 ([da4ea9e](https://github.com/informatievlaanderen/streetname-registry/commit/da4ea9e))


### BREAKING CHANGES

* Upgrade to .NET Core 3.1

## [1.19.5](https://github.com/informatievlaanderen/streetname-registry/compare/v1.19.4...v1.19.5) (2019-11-28)

## [1.19.4](https://github.com/informatievlaanderen/streetname-registry/compare/v1.19.3...v1.19.4) (2019-11-27)


### Bug Fixes

* correct extract filename to Straatnaam.dbf ([bd920fa](https://github.com/informatievlaanderen/streetname-registry/commit/bd920fa))

## [1.19.3](https://github.com/informatievlaanderen/streetname-registry/compare/v1.19.2...v1.19.3) (2019-11-26)


### Bug Fixes

* extract incomplete can happen after removed ([6f7b66d](https://github.com/informatievlaanderen/streetname-registry/commit/6f7b66d))

## [1.19.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.19.1...v1.19.2) (2019-11-26)


### Bug Fixes

* correct handling removed status in extract ([01a4185](https://github.com/informatievlaanderen/streetname-registry/commit/01a4185))

## [1.19.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.19.0...v1.19.1) (2019-11-25)

# [1.19.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.18.0...v1.19.0) (2019-11-25)


### Features

* upgrade api package ([8190372](https://github.com/informatievlaanderen/streetname-registry/commit/8190372))

# [1.18.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.17.6...v1.18.0) (2019-11-25)


### Bug Fixes

* removed streetname doesn't crash remove status event in extract ([069270e](https://github.com/informatievlaanderen/streetname-registry/commit/069270e))


### Features

* API list count valid id's in indexed view ([ef31c11](https://github.com/informatievlaanderen/streetname-registry/commit/ef31c11))
* update packages to include count func ([8e7eef4](https://github.com/informatievlaanderen/streetname-registry/commit/8e7eef4))

## [1.17.6](https://github.com/informatievlaanderen/streetname-registry/compare/v1.17.5...v1.17.6) (2019-10-24)

## [1.17.5](https://github.com/informatievlaanderen/streetname-registry/compare/v1.17.4...v1.17.5) (2019-10-24)


### Bug Fixes

* no need to check since we used to do .Value ([72dd538](https://github.com/informatievlaanderen/streetname-registry/commit/72dd538))
* upgrade grar common ([a336465](https://github.com/informatievlaanderen/streetname-registry/commit/a336465))

## [1.17.4](https://github.com/informatievlaanderen/streetname-registry/compare/v1.17.3...v1.17.4) (2019-10-14)


### Bug Fixes

* push to correct docker repo ([a2d4d11](https://github.com/informatievlaanderen/streetname-registry/commit/a2d4d11))
* trigger build :( ([e775c3e](https://github.com/informatievlaanderen/streetname-registry/commit/e775c3e))

## [1.17.3](https://github.com/informatievlaanderen/streetname-registry/compare/v1.17.2...v1.17.3) (2019-09-30)


### Bug Fixes

* check removed before completeness GR-900 ([eb26fd4](https://github.com/informatievlaanderen/streetname-registry/commit/eb26fd4))

## [1.17.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.17.1...v1.17.2) (2019-09-26)


### Bug Fixes

* update asset to fix importer ([7ee93a7](https://github.com/informatievlaanderen/streetname-registry/commit/7ee93a7))

## [1.17.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.17.0...v1.17.1) (2019-09-26)


### Bug Fixes

* resume projections on startup ([1e9190a](https://github.com/informatievlaanderen/streetname-registry/commit/1e9190a))

# [1.17.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.16.0...v1.17.0) (2019-09-25)


### Features

* upgrade projector and removed explicit start of projections ([e7fb789](https://github.com/informatievlaanderen/streetname-registry/commit/e7fb789))

# [1.16.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.15.7...v1.16.0) (2019-09-19)


### Features

* upgrade NTS & shaperon packages ([c60f8b5](https://github.com/informatievlaanderen/streetname-registry/commit/c60f8b5))

## [1.15.7](https://github.com/informatievlaanderen/streetname-registry/compare/v1.15.6...v1.15.7) (2019-09-17)


### Bug Fixes

* upgrade api for error headers ([2f24b69](https://github.com/informatievlaanderen/streetname-registry/commit/2f24b69))

## [1.15.6](https://github.com/informatievlaanderen/streetname-registry/compare/v1.15.5...v1.15.6) (2019-09-17)


### Bug Fixes

* fix contains search ([db2437c](https://github.com/informatievlaanderen/streetname-registry/commit/db2437c))

## [1.15.5](https://github.com/informatievlaanderen/streetname-registry/compare/v1.15.4...v1.15.5) (2019-09-16)


### Bug Fixes

* use generic dbtraceconnection ([7913401](https://github.com/informatievlaanderen/streetname-registry/commit/7913401))

## [1.15.4](https://github.com/informatievlaanderen/streetname-registry/compare/v1.15.3...v1.15.4) (2019-09-16)


### Bug Fixes

* correct bosa exact search GR-857 ([ecded98](https://github.com/informatievlaanderen/streetname-registry/commit/ecded98))

## [1.15.3](https://github.com/informatievlaanderen/streetname-registry/compare/v1.15.2...v1.15.3) (2019-09-13)


### Bug Fixes

* remove unneeded streetnamename indexes ([5067563](https://github.com/informatievlaanderen/streetname-registry/commit/5067563))

## [1.15.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.15.1...v1.15.2) (2019-09-13)


### Bug Fixes

* add streetnamelist index ([6f4d034](https://github.com/informatievlaanderen/streetname-registry/commit/6f4d034))
* add streetnamename index ([92c7faf](https://github.com/informatievlaanderen/streetname-registry/commit/92c7faf))

## [1.15.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.15.0...v1.15.1) (2019-09-13)


### Bug Fixes

* update redis lastchangedlist to log time of lasterror ([18f99dc](https://github.com/informatievlaanderen/streetname-registry/commit/18f99dc))

# [1.15.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.10...v1.15.0) (2019-09-12)


### Features

* keep track of how many times lastchanged has errored ([c81eb82](https://github.com/informatievlaanderen/streetname-registry/commit/c81eb82))

## [1.14.10](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.9...v1.14.10) (2019-09-05)


### Bug Fixes

* initial jira version ([3a58880](https://github.com/informatievlaanderen/streetname-registry/commit/3a58880))

## [1.14.9](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.8...v1.14.9) (2019-09-04)


### Bug Fixes

* report correct version number ([c509492](https://github.com/informatievlaanderen/streetname-registry/commit/c509492))

## [1.14.8](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.7...v1.14.8) (2019-09-03)


### Bug Fixes

* update problemdetails for xml response GR-829 ([39280b7](https://github.com/informatievlaanderen/streetname-registry/commit/39280b7))

## [1.14.7](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.6...v1.14.7) (2019-09-02)


### Bug Fixes

* do not log to console write ([d67003b](https://github.com/informatievlaanderen/streetname-registry/commit/d67003b))

## [1.14.6](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.5...v1.14.6) (2019-09-02)


### Bug Fixes

* properly report errors ([b1d02cf](https://github.com/informatievlaanderen/streetname-registry/commit/b1d02cf))

## [1.14.5](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.4...v1.14.5) (2019-08-29)


### Bug Fixes

* use columnstore for legacy syndication ([8907d63](https://github.com/informatievlaanderen/streetname-registry/commit/8907d63))

## [1.14.4](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.3...v1.14.4) (2019-08-27)


### Bug Fixes

* make datadog tracing check more for nulls ([b202f8c](https://github.com/informatievlaanderen/streetname-registry/commit/b202f8c))

## [1.14.3](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.2...v1.14.3) (2019-08-27)


### Bug Fixes

* use new desiredstate columns for projections ([b59c39a](https://github.com/informatievlaanderen/streetname-registry/commit/b59c39a))

## [1.14.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.1...v1.14.2) (2019-08-26)


### Bug Fixes

* use fixed datadog tracing ([6b40209](https://github.com/informatievlaanderen/streetname-registry/commit/6b40209))

## [1.14.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.14.0...v1.14.1) (2019-08-26)


### Bug Fixes

* fix swagger ([43c2f7e](https://github.com/informatievlaanderen/streetname-registry/commit/43c2f7e))

# [1.14.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.13.0...v1.14.0) (2019-08-26)


### Features

* bump to .net 2.2.6 ([d6eaf38](https://github.com/informatievlaanderen/streetname-registry/commit/d6eaf38))

# [1.13.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.12.1...v1.13.0) (2019-08-22)


### Features

* extract datavlaanderen namespace to settings [#3](https://github.com/informatievlaanderen/streetname-registry/issues/3) ([e13a831](https://github.com/informatievlaanderen/streetname-registry/commit/e13a831))

## [1.12.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.12.0...v1.12.1) (2019-08-22)


### Bug Fixes

* bosa empty body does not crash anymore GR-855 ([c8aa3fd](https://github.com/informatievlaanderen/streetname-registry/commit/c8aa3fd))
* bosa exact filter takes exact name into account ([0a06aa6](https://github.com/informatievlaanderen/streetname-registry/commit/0a06aa6))
* return empty response when request has invalid data GR-856 ([c18b134](https://github.com/informatievlaanderen/streetname-registry/commit/c18b134))

# [1.12.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.11.0...v1.12.0) (2019-08-16)


### Features

* add wait for user input to importer ([fd1d14e](https://github.com/informatievlaanderen/streetname-registry/commit/fd1d14e))

# [1.11.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.10.7...v1.11.0) (2019-08-13)


### Features

* add missing event handlers where nothing was expected [#29](https://github.com/informatievlaanderen/streetname-registry/issues/29) ([35e315a](https://github.com/informatievlaanderen/streetname-registry/commit/35e315a))

## [1.10.7](https://github.com/informatievlaanderen/streetname-registry/compare/v1.10.6...v1.10.7) (2019-08-09)


### Bug Fixes

* fix container id in logging ([c40607b](https://github.com/informatievlaanderen/streetname-registry/commit/c40607b))

## [1.10.6](https://github.com/informatievlaanderen/streetname-registry/compare/v1.10.5...v1.10.6) (2019-08-06)


### Bug Fixes

* bosa streetname version now offsets to belgian timezone ([7aad2cf](https://github.com/informatievlaanderen/streetname-registry/commit/7aad2cf))
* display municipality languages for bosa search ([755896a](https://github.com/informatievlaanderen/streetname-registry/commit/755896a))

## [1.10.5](https://github.com/informatievlaanderen/streetname-registry/compare/v1.10.4...v1.10.5) (2019-08-05)


### Bug Fixes

* streetname sort bosa is now by PersistentLocalId ([4ae3dd7](https://github.com/informatievlaanderen/streetname-registry/commit/4ae3dd7))

## [1.10.4](https://github.com/informatievlaanderen/streetname-registry/compare/v1.10.3...v1.10.4) (2019-07-17)


### Bug Fixes

* do not hardcode logging to console ([a214c59](https://github.com/informatievlaanderen/streetname-registry/commit/a214c59))

## [1.10.3](https://github.com/informatievlaanderen/streetname-registry/compare/v1.10.2...v1.10.3) (2019-07-15)


### Bug Fixes

* correct datadog inits ([22fc3ec](https://github.com/informatievlaanderen/streetname-registry/commit/22fc3ec))

## [1.10.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.10.1...v1.10.2) (2019-07-10)


### Bug Fixes

* fix migrations extract ([8ca953b](https://github.com/informatievlaanderen/streetname-registry/commit/8ca953b))

## [1.10.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.10.0...v1.10.1) (2019-07-10)


### Bug Fixes

* give the correct name of the event in syndication ([7f70d04](https://github.com/informatievlaanderen/streetname-registry/commit/7f70d04))

# [1.10.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.9.0...v1.10.0) (2019-07-10)


### Features

* rename oslo id to persistent local id ([cd9fbb9](https://github.com/informatievlaanderen/streetname-registry/commit/cd9fbb9))

# [1.9.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.8.4...v1.9.0) (2019-07-05)


### Features

* upgrade Be.Vlaanderen.Basisregisters.Api ([f2dd36b](https://github.com/informatievlaanderen/streetname-registry/commit/f2dd36b))

## [1.8.4](https://github.com/informatievlaanderen/streetname-registry/compare/v1.8.3...v1.8.4) (2019-07-02)


### Bug Fixes

* list now displays correct homonym addition in german & english ([59925af](https://github.com/informatievlaanderen/streetname-registry/commit/59925af))

## [1.8.3](https://github.com/informatievlaanderen/streetname-registry/compare/v1.8.2...v1.8.3) (2019-06-28)


### Bug Fixes

* reference correct packages for documentation ([7d28cd6](https://github.com/informatievlaanderen/streetname-registry/commit/7d28cd6))

## [1.8.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.8.1...v1.8.2) (2019-06-27)


### Bug Fixes

* fix logging for syndication ([6035e2d](https://github.com/informatievlaanderen/streetname-registry/commit/6035e2d))

## [1.8.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.8.0...v1.8.1) (2019-06-27)

# [1.8.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.7.0...v1.8.0) (2019-06-20)


### Features

* upgrade packages for import ([cd25375](https://github.com/informatievlaanderen/streetname-registry/commit/cd25375))

# [1.7.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.6.2...v1.7.0) (2019-06-11)


### Features

* upgrade provenance package Plan -> Reason ([fdb618e](https://github.com/informatievlaanderen/streetname-registry/commit/fdb618e))

## [1.6.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.6.1...v1.6.2) (2019-06-06)


### Bug Fixes

* copy correct repo ([69a609b](https://github.com/informatievlaanderen/streetname-registry/commit/69a609b))

## [1.6.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.6.0...v1.6.1) (2019-06-06)


### Bug Fixes

* force version bump ([d6acf8a](https://github.com/informatievlaanderen/streetname-registry/commit/d6acf8a))

# [1.6.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.5.2...v1.6.0) (2019-06-06)


### Features

* deploy docker to production ([354a707](https://github.com/informatievlaanderen/streetname-registry/commit/354a707))

## [1.5.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.5.1...v1.5.2) (2019-06-06)


### Bug Fixes

* change idempotency hash to be stable ([9cff84f](https://github.com/informatievlaanderen/streetname-registry/commit/9cff84f))

## [1.5.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.5.0...v1.5.1) (2019-05-23)


### Bug Fixes

* correct oslo id type for extract ([f735cd8](https://github.com/informatievlaanderen/streetname-registry/commit/f735cd8))

# [1.5.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.4.2...v1.5.0) (2019-05-22)


### Features

* add event data to sync endpoint ([31bd514](https://github.com/informatievlaanderen/streetname-registry/commit/31bd514))

## [1.4.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.4.1...v1.4.2) (2019-05-21)

## [1.4.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.4.0...v1.4.1) (2019-05-20)

# [1.4.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.12...v1.4.0) (2019-04-30)


### Features

* add projector + cleanup projection libraries ([a861da2](https://github.com/informatievlaanderen/streetname-registry/commit/a861da2))
* upgrade packages ([6d9ad96](https://github.com/informatievlaanderen/streetname-registry/commit/6d9ad96))

## [1.3.12](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.11...v1.3.12) (2019-04-18)

## [1.3.11](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.10...v1.3.11) (2019-04-17)


### Bug Fixes

* [#8](https://github.com/informatievlaanderen/streetname-registry/issues/8) + Volgende is now not emitted if null ([fe6eb46](https://github.com/informatievlaanderen/streetname-registry/commit/fe6eb46))

## [1.3.10](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.9...v1.3.10) (2019-04-16)


### Bug Fixes

* sort streetname list by olsoid [GR-717] ([f62740e](https://github.com/informatievlaanderen/streetname-registry/commit/f62740e))

## [1.3.9](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.8...v1.3.9) (2019-03-06)

## [1.3.8](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.7...v1.3.8) (2019-02-28)


### Bug Fixes

* swagger docs now show list response correctly ([79adcf9](https://github.com/informatievlaanderen/streetname-registry/commit/79adcf9))

## [1.3.7](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.6...v1.3.7) (2019-02-26)

## [1.3.6](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.5...v1.3.6) (2019-02-25)

## [1.3.5](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.4...v1.3.5) (2019-02-25)


### Bug Fixes

* extract only exports completed items ([6baf2e9](https://github.com/informatievlaanderen/streetname-registry/commit/6baf2e9))
* use new lastchangedlist migrations runner ([4d4e0e2](https://github.com/informatievlaanderen/streetname-registry/commit/4d4e0e2))

## [1.3.4](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.3...v1.3.4) (2019-02-07)


### Bug Fixes

* support nullable Rfc3339SerializableDateTimeOffset in converter ([7b3c704](https://github.com/informatievlaanderen/streetname-registry/commit/7b3c704))

## [1.3.3](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.2...v1.3.3) (2019-02-06)


### Bug Fixes

* properly serialise rfc 3339 dates ([abd5daf](https://github.com/informatievlaanderen/streetname-registry/commit/abd5daf))

## [1.3.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.1...v1.3.2) (2019-02-06)


### Bug Fixes

* oslo id and niscode in sync werent correctly projected ([32d9ee8](https://github.com/informatievlaanderen/streetname-registry/commit/32d9ee8))

## [1.3.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.3.0...v1.3.1) (2019-02-04)

# [1.3.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.2.3...v1.3.0) (2019-01-25)


### Bug Fixes

* correctly setting primary language in sync projection ([825ba1a](https://github.com/informatievlaanderen/streetname-registry/commit/825ba1a))
* fix starting Syndication projection ([46788bc](https://github.com/informatievlaanderen/streetname-registry/commit/46788bc))
* list now displays name of streetnames correctly ([d02b6d2](https://github.com/informatievlaanderen/streetname-registry/commit/d02b6d2))


### Features

* adapted sync with new municipality changes ([c05d427](https://github.com/informatievlaanderen/streetname-registry/commit/c05d427))
* change display municipality name of detail in Api.Legacy ([79d693f](https://github.com/informatievlaanderen/streetname-registry/commit/79d693f))

## [1.2.3](https://github.com/informatievlaanderen/streetname-registry/compare/v1.2.2...v1.2.3) (2019-01-22)


### Bug Fixes

* use https for namespace ([92965c1](https://github.com/informatievlaanderen/streetname-registry/commit/92965c1))

## [1.2.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.2.1...v1.2.2) (2019-01-18)

## [1.2.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.2.0...v1.2.1) (2019-01-18)


### Bug Fixes

* migrations history table for syndication ([f78cd51](https://github.com/informatievlaanderen/streetname-registry/commit/f78cd51))

# [1.2.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.1.2...v1.2.0) (2019-01-17)


### Features

* do not take diacritics into account when filtering on municipality ([025a122](https://github.com/informatievlaanderen/streetname-registry/commit/025a122))

## [1.1.2](https://github.com/informatievlaanderen/streetname-registry/compare/v1.1.1...v1.1.2) (2019-01-16)


### Bug Fixes

* required upgrade for datadog tracing to avoid connection pool problems ([432dbb4](https://github.com/informatievlaanderen/streetname-registry/commit/432dbb4))

## [1.1.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.1.0...v1.1.1) (2019-01-16)


### Bug Fixes

* optimise catchup mode for versions ([4583327](https://github.com/informatievlaanderen/streetname-registry/commit/4583327))

# [1.1.0](https://github.com/informatievlaanderen/streetname-registry/compare/v1.0.1...v1.1.0) (2019-01-16)


### Bug Fixes

* legacy syndication now subsribes to OsloIdAssigned ([42f0f49](https://github.com/informatievlaanderen/streetname-registry/commit/42f0f49))
* take local changes into account for versions projection ([9560ec6](https://github.com/informatievlaanderen/streetname-registry/commit/9560ec6))


### Features

* add statuscode 410 Gone for removed streetnames ([4e5f7f6](https://github.com/informatievlaanderen/streetname-registry/commit/4e5f7f6))

## [1.0.1](https://github.com/informatievlaanderen/streetname-registry/compare/v1.0.0...v1.0.1) (2019-01-15)


### Bug Fixes

* streetnameid in extract file is a string ([f845424](https://github.com/informatievlaanderen/streetname-registry/commit/f845424))

# 1.0.0 (2019-01-14)


### Features

* open source with EUPL-1.2 license as 'agentschap Informatie Vlaanderen' ([bba50fd](https://github.com/informatievlaanderen/streetname-registry/commit/bba50fd))
