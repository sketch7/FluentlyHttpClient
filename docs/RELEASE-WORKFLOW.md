# Release workflow
In order to release follow the following procedure.

 - Create branch e.g. `feature/xyz`.. *onces changes are ready...*
 - Create a PR from `feature/xyz` to `master`

## Stable Versions
 - Create PR to version branch e.g. `3.x`
 - Update version within `package.json`
 - Update `CHANGELOG.md`
 - Once merged it will auto `publish` and `git tag`