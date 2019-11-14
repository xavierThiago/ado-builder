# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.6] - 2019-11-14
### Added
- Readme temporary description.

## [0.0.5] - 2019-11-14
### Added
- Apache licensing.

## [0.0.4] - 2019-11-14
### Fixed
- Changelog comment to reflect correct current code coverage threshold;
- Normalized all files line break to LF.

## [0.0.3] - 2019-11-13
### Added
- Test project;
- VS Code launch and task files.

### Changed
- Makefile to not infer code coverage (at least for now);
- Makefile's "pack" rule inherits "cbt" once again; when using "cbt" rule, test execution will avoid building all project again;
- All project files now ensures latest language support.

### Fixed
- Making sure all files end in a newline;
- Some linting on all project files.

## [0.0.2] - 2019-11-13
### Added
- README in every project;
- Every project is now referenced on solution file;
- Makefile recipes.

### Changed
- Moved solution file to root;
- Every .nuspec to describe each project packaging information.

## [0.0.1] - 2019-11-13
### Added
- Initial solution structure;
- ADO connections from Postgresql and Oracle.
