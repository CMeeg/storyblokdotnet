---
name: github-create-pull-request
description: 'Create GitHub Pull Request for feature request from specification file using pull_request_template.md template.'
---

# Create GitHub Pull Request

Create GitHub Pull Request for the specification at `.github/pull_request_template.md` .

## Process

1. Analyze specification file template from `.github/pull_request_template.md` to extract requirements by `search` tool.
2. Check if there is an existing pull request of the current branch using the `get_pull_request` tool.
  - If there is an existing pull request for the current branch continue to step 4, and skip step 3.
3. Create pull request draft template by using `create_pull_request` tool for the current branch on to `${input:targetBranch}`.
4. Get changes for the pull request by using `get_pull_request_diff` tool to analyse information about what has changed.
5. Update the pull request body and title using the `update_pull_request` tool. Incorporate the information from the template obtained in the step 1 to update the body and title as needed with the information from step 4.
6. Respond with the URL for the pull request that has been created/updated.
7. If the pul request status is draft then ask the user if they want to switch from draft to ready for review.
  - If yes, use the `update_pull_request` tool to update state of the pull request.

## Requirements

- A single pull request is created or updated for the complete specification - if an existing pull request exists for the current branch it should be updated; otherwise a new pull request created.
- The pull request details follow the `.github/pull_request_template.md` specification.
- The information provided in the pull request is an accurate reflection of the changes made on this branch when diffed against the target branch.
