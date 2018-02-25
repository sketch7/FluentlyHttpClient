# Release workflow
In order to release follow the following procedure.

 - Create branch e.g. `feature/xyz`.. *onces changes are ready...*
 - Update version within `package.json`
 - Update `CHANGELOG.md`
 - Create a PR from `feature/xyz` to `master`
 - Once merged it will auto `publish` and `git tag`