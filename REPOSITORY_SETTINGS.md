# Repository Settings Documentation

This document describes the configuration settings for the ChartPro repository.

## Automatic Branch Deletion

**Status**: ✅ **ENABLED**

The ChartPro repository has automatic deletion of head branches enabled. This means that when a pull request is merged, the source branch (head branch) will be automatically deleted from the repository.

### Benefits

- **Cleaner Repository**: Prevents accumulation of stale/merged feature branches
- **Streamlined Maintenance**: Reduces manual cleanup workload
- **Better Organization**: Keeps the branch list focused on active development

### Configuration

The automatic branch deletion is configured in two ways:

1. **Repository Settings** (Web UI)
   - Navigate to: Settings > General > Pull Requests
   - Toggle: "Automatically delete head branches"
   - Status: ENABLED

2. **Settings as Code** (`.github/settings.yml`)
   - Configuration file: `.github/settings.yml`
   - Setting: `delete_branch_on_merge: true`
   - This file can be used with GitHub Apps like [Probot Settings](https://github.com/probot/settings) to automate repository configuration

### How It Works

When a pull request is merged:
1. The changes are merged into the target branch (e.g., `main` or `develop`)
2. The source branch (e.g., `feature/new-feature`) is automatically deleted
3. The branch is removed from the repository
4. Contributors can delete their local copies with: `git fetch --prune`

### Important Notes

- **Protected Branches**: This setting does not affect protected branches
- **Forked Repositories**: Branches in forked repositories are not affected
- **Manual Override**: You can still restore deleted branches if needed using the GitHub UI
- **Local Cleanup**: Contributors should regularly run `git fetch --prune` to clean up local references to deleted remote branches

### References

- [GitHub Documentation: Automatically deleting head branches](https://docs.github.com/en/repositories/configuring-branches-and-merges-in-your-repository/automatically-deleting-head-branches)
- [Probot Settings](https://github.com/probot/settings) - GitHub App for repository settings automation

### Related Settings

See `.github/settings.yml` for the complete repository configuration, including:
- Merge options (squash, merge commit, rebase)
- Repository features (issues, projects, wiki)
- Labels configuration

---

**Last Updated**: October 2024  
**Enabled By**: Repository Administrator  
**Issue Reference**: #[issue-number] - Enable auto-delete head branches after PR merge
