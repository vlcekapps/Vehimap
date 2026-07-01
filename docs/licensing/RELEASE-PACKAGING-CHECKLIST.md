# Release packaging checklist for Vehimap

Use this checklist before publishing a binary build.

- [ ] Confirm that the project license is shown as `GPL-3.0-or-later` in `README.md` and repository metadata.
- [ ] Include `LICENSE` in every binary release artifact.
- [ ] Include `THIRD-PARTY-NOTICES.md` in every binary release artifact.
- [ ] Include license texts from `LICENSES/` or otherwise ensure they are available with the release artifact.
- [ ] Run `dotnet restore` in a clean checkout.
- [ ] Run `dotnet list dotnet/Vehimap.sln package --include-transitive` and update third-party notices for shipped transitive dependencies.
- [ ] Check publish output for native/runtime components and add notices for those actually distributed.
- [ ] Check non-code assets: icons, images, fonts, screenshots, sample data, localization files, and documentation.
- [ ] Keep original copyright notices in third-party files.
- [ ] For installer formats, verify that license/notice files are installed or displayed according to the platform packaging rules.
