![dotnet-build](https://github.com/guitarrapc/githubaction-lab/workflows/dotnet-build/badge.svg?branch=master)

## githubactions-lab

GitHub Actions laboratory.

## Table of Contents

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->
<details>
<summary>Details</summary>

- [Not yet support](#not-yet-support)
- [Difference from other CI](#difference-from-other-ci)
  - [migration](#migration)
  - [job and workflow](#job-and-workflow)
  - [skip ci on commit message](#skip-ci-on-commit-message)
  - [path filter](#path-filter)
  - [job id or other meta values](#job-id-or-other-meta-values)
  - [cancel redundant builds](#cancel-redundant-builds)
  - [set environment variables for next step](#set-environment-variables-for-next-step)
  - [adding system path](#adding-system-path)
  - [set secrets for reposiory](#set-secrets-for-reposiory)
  - [approval](#approval)
- [Fundamentals](#fundamentals)
  - [Manual Trigger and input](#manual-trigger-and-input)
  - [retry failed workflow](#retry-failed-workflow)
  - [secrets](#secrets)
  - [meta github context](#meta-github-context)
  - [view webhook github context](#view-webhook-github-context)
  - [matrix and secret dereference](#matrix-and-secret-dereference)
  - [matrix and environment variables](#matrix-and-environment-variables)
  - [env refer env](#env-refer-env)
  - [set environment variables in script](#set-environment-variables-in-script)
  - [reuse yaml actions - composite](#reuse-yaml-actions---composite)
  - [reuse Node actions - node12](#reuse-node-actions---node12)
  - [runs only previous job is success](#runs-only-previous-job-is-success)
  - [runs only when previous step status is specific](#runs-only-when-previous-step-status-is-specific)
  - [timeout for job and step](#timeout-for-job-and-step)
  - [suppress redundant build](#suppress-redundant-build)
  - [if and context reference](#if-and-context-reference)
- [Branch and tag handling](#branch-and-tag-handling)
  - [run when branch push only but skip on tag push](#run-when-branch-push-only-but-skip-on-tag-push)
  - [skip when branch push but run on tag push only](#skip-when-branch-push-but-run-on-tag-push-only)
  - [build only specific tag pattern](#build-only-specific-tag-pattern)
  - [get pushed tag name](#get-pushed-tag-name)
  - [create release](#create-release)
  - [schedule job on non-default branch](#schedule-job-on-non-default-branch)
- [Commit handling](#commit-handling)
  - [trigger via commit message](#trigger-via-commit-message)
  - [commit file handling](#commit-file-handling)
- [Issue and Pull Request handling](#issue-and-pull-request-handling)
  - [skip ci on pull request title](#skip-ci-on-pull-request-title)
  - [skip pr from fork repo](#skip-pr-from-fork-repo)
  - [detect labels on pull request](#detect-labels-on-pull-request)
  - [skip job when Draft PR](#skip-job-when-draft-pr)
- [ADVANCED](#advanced)
  - [Dispatch other repo from workflow](#dispatch-other-repo-from-workflow)

</details>
<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## Not yet support

- [ ] YAML anchor support
  - [Support for YAML anchors \- GitHub Community Forum](https://github.community/t5/GitHub-Actions/Support-for-YAML-anchors/td-p/30336)

## Difference from other CI

### migration

> * GitHub Actions -> CircleCI: [Migrating from Github Actions \- CircleCI](https://circleci.com/docs/2.0/migrating-from-github/)
> * CircleCI -> GitHub Actions: [Migrating from CircleCI to GitHub Actions \- GitHub Help](https://help.github.com/en/actions/migrating-to-github-actions/migrating-from-circleci-to-github-actions)
> * Azure pipeline -> GitHub Actions: [Migrating from Azure Pipelines to GitHub Actions \- GitHub Help](https://help.github.com/en/actions/migrating-to-github-actions/migrating-from-azure-pipelines-to-github-actions)
> * Jenkins -> GitHub Actions: [Migrating from Jenkins to GitHub Actions \- GitHub Help](https://help.github.com/en/actions/migrating-to-github-actions/migrating-from-jenkins-to-github-actions)

### job and workflow

GitHub Actions cannnot reuse yaml and need to write same job for each workflow.
Better define step in script and call it from step, so that we can reuse same execution from other workflows or jobs.

* GitHub Actions define job inside workflow, and GitHub Actions cannot refer yaml from others.
* CircleCI define job and conbinate it in workflow. Reusing job is natual way in circleci.
* Azure Pipeline offer's template to refer stage, job and step from other yaml. This enable user to reuse yaml.
* Jenkins has pipeline and could refer other pipeline. However a lot case would be define job step in script and reuse script, not pipeline.

### skip ci on commit message

GitHub Actions support when HEAD commit contains key word like other ci.

* GitHub Actions can skip workflow via `[skip ci]`, `[ci skip]`, `[no ci]`, `[skip actions]` or `[actions skip]`. If PR last commit message contains `[skip ci]`, then merge commit also skip.
* CircleCI can skip job via `[skip ci]` or `[ci skip]`. If PR last commit message contains `[skip ci]`, then merge commit also skip.
* Azure Pipeline can skip job via `***NO_CI***`, `[skip ci]` or `[ci skip]`, or [others](https://github.com/Microsoft/azure-pipelines-agent/issues/858#issuecomment-475768046).
* Jenkins has plugin to support `[skip ci]` or any expression w/pipeline via [SCM Skip \| Jenkins plugin](https://plugins.jenkins.io/scmskip/).

### path filter

GitHub Actions can use `on.<event>.paths-ignore:` and `on.<event>.paths:` by default.

> [paths - Workflow syntax for GitHub Actions \- GitHub Help](https://help.github.com/en/actions/reference/workflow-syntax-for-github-actions#onpushpull_requestpaths)

* GitHub Actions can set path-filter.
* CircleCI can not set path-filter.
* Azure Pipeline can set path-filter.
* Jenkins ... I think I need filter change from changes?

### job id or other meta values

GitHub Actions has Context concept, you can access job specific info via `github`.
for example, `github.run_id` is A unique number for each run within a repository.
Also you can access default environment variables like `GITHUB_RUN_ID`.

* GitHub Actions [environment variable](https://help.github.com/en/actions/configuring-and-managing-workflows/using-environment-variables#default-environment-variables) `GITHUB_RUN_ID` or [context](https://help.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#github-context) `github.run_id`
* CircleCI [environment vairable](https://circleci.com/docs/2.0/env-vars/#built-in-environment-variables) `CIRCLE_BUILD_NUM` and `CIRCLE_WORKFLOW_ID`
* Azure Pipeline [environment variable](https://docs.microsoft.com/ja-jp/azure/devops/pipelines/process/run-number?view=azure-devops&tabs=yaml#tokens) `BuildID`.
* Jenkins [environment vairable](https://wiki.jenkins.io/display/JENKINS/Building+a+software+project) `BUILD_NUMBER`

### cancel redundant builds

GitHub Actions not support cancel redundant build as CircleCI do.
> [Solved: Github actions: Cancel redundant builds \(Not solve\.\.\. \- GitHub Community Forum](https://github.community/t5/GitHub-Actions/Github-actions-Cancel-redundant-builds-Not-solved/td-p/29549)

You can accomplish via actions. Workflow run cleanup action is the recommended.

> [Workflow run cleanup action · Actions · GitHub Marketplace](https://github.com/marketplace/actions/workflow-run-cleanup-action)

This one is bit too much.

> [technote\-space/auto\-cancel\-redundant\-job: GitHub Actions to automatically cancel redundant jobs\.](https://github.com/technote-space/auto-cancel-redundant-job)

Theses are minimum specs.

> [auto\-cancellation\-running\-action · Actions · GitHub Marketplace](https://github.com/marketplace/actions/auto-cancellation-running-action)
>
> [GH actions stale run canceller · Actions · GitHub Marketplace](https://github.com/marketplace/actions/gh-actions-stale-run-canceller)

* GitHub Actions need use some of actions
* CircleCI support cancel redundant build
* Azure Pipeline not support cancel redundant build
* Jenkins not support cancel redundant build, you need cancel it from parallel job.

### set environment variables for next step

GitHub Actions need to create or update Environment File, it's similar to CircleCI.

> https://help.github.com/en/actions/reference/workflow-commands-for-github-actions#setting-an-environment-variable

GitHub Actions use Environment Files to manage Environment variables, create or update via `echo "{name}={value}" >> $GITHUB_ENV` syntax, or `echo "{name}={value}" | tee -a $GITHUB_ENV`

> `::set-env` syntax is deprecated for [security reason](https://github.blog/changelog/2020-10-01-github-actions-deprecating-set-env-and-add-path-commands/).

```yaml
steps:
  - name: test
    run: |
      # new
      echo "INPUT_LOGLEVEL=${{ github.event.inputs.logLevel }}" >> $GITHUB_ENV
      echo "INPUT_TAGS=${{ github.event.inputs.tags }}" >> $GITHUB_ENV
      # deprecated
      # echo ::set-env name=INPUT_LOGLEVEL::${{ github.event.inputs.logLevel }}
      # echo ::set-env name=INPUT_TAGS::${{ github.event.inputs.tags }}
```

* CircleCI use redirect to `> BASH_ENV` will automatically load on next step
* Azure Pipeline use task.setvariable. `echo "##vso[task.setvariable variable=NAME]VALUE"`
* Jenkins use `Env.` in groovy declarative pipeline.

### adding system path

GitHub Actions need to create or update Environment File, it's similar to CircleCI.
* GitHub Actions use Environment Files to manage System Path, create or update via `echo "{path}" >> $GITHUB_PATH` syntax.
  * `::add-path` syntax is deprecated for [security reason](https://github.blog/changelog/2020-10-01-github-actions-deprecating-set-env-and-add-path-commands/).

### set secrets for reposiory

GitHub ACtions offer Secrets for each repository and Organization.
Secrets will be masked on the log.

* GitHub Actions use Secrets
* CircleCI offer Environment Variables and Context.
* Azure Pipeline has Environment Variables and Paramter.
* Jenkins has Credential Provider.

### approval

[TBD]

* GitHub Actions supports Approval.
* CircleCI supports Approval.
* Azure Pipelin supports Approval.
* Jenkins supports Approval.

## Fundamentals

### Manual Trigger and input

GitHub Actions offer `workflow_dispatch` event to execute workflow manually from WebUI.
Also you can use [action inputs](https://docs.github.com/en/actions/creating-actions/metadata-syntax-for-github-actions#inputs) to specify value trigger on manual trigger.

```yaml
name: manual trigger
on:
  workflow_dispatch:
    inputs:
      branch:
        description: "branch name to clone"
        required: true
        default: "master"
      logLevel:
        description: "Log level"
        required: true
        default: "warning"
      tags:
        description: "Test scenario tags"
        required: false
jobs:
  printInputs:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
        with:
          ref: ${{ github.event.inputs.branch }}
      - name: dump github context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(github) }}
      - name: dump inputs context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(github.event.inputs) }}
      - run: |
          echo "Log level: ${{ github.event.inputs.logLevel }}"
          echo "Tags: ${{ github.event.inputs.tags }}"
      # INPUT_ not automatcally generated
      - run: |
          echo ${INPUT_TEST_VAR}
          echo ${TEST_VAR}
      - run: export
      - run: |
          echo "INPUT_LOGLEVEL=${{ github.event.inputs.logLevel }}" >> $GITHUB_ENV
          echo "INPUT_TAG=${{ github.event.inputs.tags }}" >> $GITHUB_ENV
      - run: echo "/path/to/dir" >> $GITHUB_PATH
      - run: |
          echo "Log level: ${INPUT_LOGLEVEL}"
          echo "Tags: ${INPUT_TAGS}"
```

Even if you specify action inputs, input value will not store as ENV var `INPUT_{INPUTS_ID}` as usual.

### retry failed workflow

GitHub Actions support Re-run jobs.
You can re-run whole workflow again, but you cannot re-run specified job only.

### secrets

GitHub Actions supports "Indivisual Repository Secrets" and "Organization Secrets"

* You can set secrets for each repository with `Settings > Secrets`.
* You can set secrets for Organization and filter to selected repository with `Settings > Secrets`.

If same secrets key is exists, `Repository Secrets` > `Organization Secrets`.

When you want spread your secrets with indivisual account, you need set each repository secrets or use [google/secrets\-sync\-action](https://github.com/google/secrets-sync-action).

### meta github context

job id, name and others.

> [Context and expression syntax for GitHub Actions \- GitHub Help](https://help.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#github-context)

**TIPS**

You can not directly refer github context in script.
Let's check this behaviour with script `.github/scripts/github-context.sh`.

```sh
#!/bin/bash
echo "ref"
echo ${{ github.ref }}
```

call it from workflow.

```yaml
name: dump context on script
on: [push]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - run: bash -eu .github/scripts/github-context.sh
```

Then you will see `${{ github.ref }}: bad substitution` error.
This is because `${{ github }}` is not bash syntax, but github workflow's syntax.

```sh
.github/scripts/github-context.sh: line 3: ${{ github.ref }}: bad substitution
ref
```

### view webhook github context

dump context with `toJson()` is a easiest way to dump context.

```yaml
name: dump context push

on: push

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Dump environment
        run: export
      - name: Dump GitHub context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(github) }}
      - name: Dump job context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(job) }}
      - name: Dump steps context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(steps) }}
      - name: Dump runner context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(runner) }}
      - name: Dump strategy context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(strategy) }}
      - name: Dump matrix context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(matrix) }}
```

pull request dump.

```yaml
name: dump context pr

on: pull_request

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: Dump environment
        run: export
      - name: Dump GitHub context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(github) }}
      - name: Dump job context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(job) }}
      - name: Dump steps context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(steps) }}
      - name: Dump runner context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(runner) }}
      - name: Dump strategy context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(strategy) }}
      - name: Dump matrix context
        run: echo "$CONTEXT"
        env:
          CONTEXT: ${{ toJson(matrix) }}
```

### matrix and secret dereference

matrix cannot reference `secret` context, so pass secret key in matrix then dereference secret with `secrets[matrix.SECRET_KEY]`.

let's set secrets in settings.

![image](https://user-images.githubusercontent.com/3856350/79934065-99de6c00-848c-11ea-8995-bfe948e6c0fb.png)

```yaml
name: matrix secret

on: ["push"]

jobs:
  build:
    strategy:
      matrix:
        org: [apples, bananas, carrots] #Array of org mnemonics to use below
        include:
          # includes a new variable for each org (this is effectively a switch statement)
          - org: apples
            secret: APPLES
          - org: bananas
            secret: BANANAS
          - org: carrots
            secret: CARROTS
    runs-on: ubuntu-latest
    steps:
      - run: echo "org:${{ matrix.org }} secret:${{ secrets[matrix.secret] }}"
```

### matrix and environment variables

you can refer matrix in job's `env:` section before steps.
However you cannot use expression, you must evaluate in step.

```yaml
name: matrix envvar

on: ["push"]

jobs:
  build:
    strategy:
      matrix:
        org: [apples, bananas, carrots]
    runs-on: ubuntu-latest
    env:
      ORG: ${{ matrix.org }}
      # you can not use expression. do it on step.
      # output on step is -> ci-`date '+%Y%m%d-%H%M%S'`+${GITHUB_SHA:0:6}
      # GIT_TAG: "ci-`date '+%Y%m%d-%H%M%S'`+${GITHUB_SHA:0:6}"
    steps:
      - run: echo "${ORG}"
```

### env refer env

You cannot use `${{ env. }}` in `env:` section.
Following is invalid with error.

> The workflow is not valid. .github/workflows/env_refer_env.yaml (Line: 12, Col: 16): Unrecognized named-value: 'env'. Located at position 1 within expression: env.global_env

```yaml
name: env refer env

on: ["push"]

env:
  global_env: global

jobs:
  build:
    runs-on: ubuntu-latest
    env:
      job_env: ${{ env.global_env }}
    steps:
      - run: echo "${{ env.global_env }}"
      - run: echo "${{ env.job_env }}"
```

### set environment variables in script

[set environment variables for next step](#set-environment-variables-for-next-step) explains how to set environment variables for next step.
This syntax can be write in the script, let's see `.github/scripts/setenv.sh`.

```shell
#!/bin/bash
while [ $# -gt 0 ]; do
    case $1 in
        --ref) GITHUB_REF=$2; shift 2; ;;
        *) shift ;;
    esac
done

echo GIT_TAG_SCRIPT=${GITHUB_REF#refs/tags/} >> $GITHUB_ENV
```

Call this script from workflow.

```yaml
name: env with script

on: push

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - run: echo "GIT_TAG=${GITHUB_REF#refs/heads/}" >> $GITHUB_ENV
      - run: echo ${{ env.GIT_TAG }}
      - run: bash -eux .github/scripts/setenv.sh --ref "${GITHUB_REF#refs/heads/}"
      - run: echo ${{ env.GIT_TAG_SCRIPT }}
```

`echo ${{ env.GIT_TAG_SCRIPT }}` will output `chore/context_in_script` as expected.

### reuse yaml actions - composite

To reuse YAML only job, use Repository Actions with `composite`.

* step1. Place your yaml to `.github/actions/your_dir/action.yaml`
* step2. Write your composite actions yaml.

```yaml
# local_composite_actions.yaml
name: YOUR ACTION NAME
description: |
  Desctiption of your action
inputs:
  foo:
    description: thi is foo input
    default: FOO
    required: false
runs:
  using: "composite" # this is key point
  steps:
    - name: THIS IS STEP1
      shell: bash # this is key point
      run: echo ${{ inputs.foo }}
```

* step3. Use actions from your workflow.


```yaml
name: reuse local action
on: push
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: use local action
        uses: ./.github/actions/local_composite_actions
        with:
          foo: BAR
```

### reuse Node actions - node12

To reuse Node, action, just place action inside `./github/actions/your_dir/` with action.yaml and source codes.

* step1. Place your ation.yaml and src to `.github/actions/your_dir/`
* step2. Use actions from your workflow.

```yaml
name: reuse local action
on: push
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - name: use local action
        uses: ./.github/actions/local_node_action
```

### runs only previous job is success

to accomplish sequential job run, use `needs:` for which you want the job to depends on.

this enforce job to be run when only previous job is **success**.

```yaml
name: sequential jobs

on: ["push"]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - run: echo $COMMIT_MESSAGES
        env:
          COMMIT_MESSAGES: ${{ toJson(github.event.commits.*.message) }}

  publish:
    runs-on: ubuntu-latest
    needs: build
    steps:
      - run: run when only build success
```

### runs only when previous step status is specific

> [job-status-check-functions \- Context and expression syntax for GitHub Actions \- GitHub Help](https://help.github.com/en/actions/reference/context-and-expression-syntax-for-github-actions#job-status-check-functions)

use `if:` you want set step to be run on particular status.

```yaml
name: status step

on: ["push"]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - run: echo $COMMIT_MESSAGES
        env:
          COMMIT_MESSAGES: ${{ toJson(github.event.commits.*.message) }}
      - run: echo run when none of previous steps  have failed or been canceled
        if: success()
      - run: echo run even cancelled. it won't run only when critical failure prevents the task.
        if: always()
      - run: echo run when Workflow cancelled.
        if: cancelled()
      - run: echo run when any previous step of a job fails.
        if: failure()
```

### timeout for job and step

default timeout is 360min. You should set much more shorten timeout like 15min or 30min to prevent spending a lot build time.

```yaml
name: timeout

on: ["push"]

jobs:
  my-job:
    runs-on: ubuntu-latest
    timeout-minutes: 5
    steps:
      - run: echo done before timeout
        timeout-minutes: 1 # step個別
```

### suppress redundant build

This will trouble only when you are runnning private repo, if repo is public, you don't need mind build comsume time.

> Detail: When created `pull_request` then pushed, both `push` and `pull_request/synchronize` event emmit. This trigger duplicate build and waste build time.

**do not trigger push on pull_request**

In this example `push` will trigger only when `master`, default branch, this means push will not run when `pull_request` synchronize event was emmited.
Simple enough for almost usage.

```yaml
name: push and pull_request avoid redundant

on:
  # prevent push run on pull_request
  push:
    branches:
      - master
  pull_request:
    types:
      - synchronize
      - opened
      - reopened

jobs:
  my-job:
    runs-on: ubuntu-latest
    steps:
      - run: echo push and pull_request trigger
```

**redundant build cancel**

Cancel duplicate workflow.
Make sure cancel will set `Status API` as failure.

```yaml
name: cancel redundant build
# when pull_request, both push and pull_request (synchronize) will trigger.
# this action sample will prevent duplicate run, but run only 1 of them.
on: [push, pull_request]

jobs:
  cancel:
    runs-on: ubuntu-latest
    steps:
      # no check for master and tag
      - uses: rokroskar/workflow-run-cleanup-action@v0.2.2
        if: "!startsWith(github.ref, 'refs/tags/') && github.ref != 'refs/heads/master'"
        env:
          GITHUB_TOKEN: "${{ secrets.GITHUB_TOKEN }}"
```

**redundant build cancel except master and tag**

cancelling if `push is not tag` and `push is not branch "master"`.
It means push to branch will be cancelled if duplicated workflow run at once.

```yaml
name: cancel redundant build
on: push

jobs:
  cancel:
    runs-on: ubuntu-latest
    steps:
      - uses: rokroskar/workflow-run-cleanup-action@v0.2.2
        if: "!startsWith(github.ref, 'refs/tags/') && github.ref != 'refs/heads/master'"
        env:
          GITHUB_TOKEN: "${{ secrets.GITHUB_TOKEN }}"
```

### if and context reference

GitHub Actions allow `if` condition for `step`.
when you want refer any context, `env`, `github` and `matrix`, on `if` condition, you don't need add `${{}}` to context reference.

> NOTE: `matrix` cannot refer with `job.if`.

> [Solved: What is the correct if condition syntax for checki\.\.\. \- GitHub Community Forum](https://github.community/t5/GitHub-Actions/What-is-the-correct-if-condition-syntax-for-checking-matrix-os/td-p/31269)

```yaml
name: if and context reference
on: push

jobs:
  matrix_reference:
    strategy:
      matrix:
        sample: ["hoge", "fuga"]
    env:
      APP: hoge
    runs-on: ubuntu-latest
    steps:
      # env context reference
      - run: echo "this is env if for hoge"
        if: env.APP == matrix.sample
      - run: echo "this is env if for fuga"
        if: env.APP == matrix.sample
      # github context reference
      - run: echo "this is github if event push"
        if: github.event_name == push
      # matrix context reference
      - run: echo "this is matrix if for hoge"
        if: matrix.sample == 'hoge'
      - run: echo "this is matrix if for fuga"
        if: matrix.sample == 'fuga'
```

## Branch and tag handling

### run when branch push only but skip on tag push

If you want run job only when push to branch, and not for tag push.

```yaml
name: branch push only

on:
  push:
    branches:
      - "**"
    tags:
      - "!*" # not a tag push

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - run: echo not run on tag
```

### skip when branch push but run on tag push only

If you want run job only when push to tag, and not for branch push.

```yaml
name: tag push only

on:
  push:
    tags:
      - "**" # only tag

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - run: echo not run on branch push
```

### build only specific tag pattern

You can use pattern on `on.push.tags`, but you can't on `step.if`.
This pattern will match following.

* 0.0.1
* 1.0.0+preview
* 0.0.3-20200421-preview+abcd123408534

not for below.

* v0.0.1
* release

```yaml
name: tag push only pattern

on:
  push:
    tags:
      - "[0-9]+.[0-9]+.[0-9]+*" # only tag with pattern match

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - run: echo not run on branch push
```

### get pushed tag name

You need extract refs to get tag name.
Save it to `step context` and refer from other step or save it to env is much eacher.

```yaml
name: tag push only

on:
  push:
    tags:
      - "**" # only tag

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - run: echo ::set-output name=GIT_TAG::${GITHUB_REF#refs/tags/}
        id: CI_TAG
      - run: echo ${{ steps.CI_TAG.outputs.GIT_TAG }}
      - run: echo "GIT_TAG=${GITHUB_REF#refs/tags/}" >> $GITHUB_ENV
      - run: echo ${{ env.GIT_TAG }}
```

### create release

You can create release and upload assets through GitHub Actions.
Multiple assets upload is supported by running running `actions/upload-release-asset` for each asset.

```yaml
name: create release

on:
  push:
    tags:
      - "[0-9]+.[0-9]+.[0-9]+*"

jobs:
  create-release:
    runs-on: ubuntu-latest
    env:
      DOTNET_CLI_TELEMETRY_OPTOUT: 1
      DOTNET_SKIP_FIRST_TIME_EXPERIENCE: 1
      NUGET_XMLDOC_MODE: skip
    steps:
      # set release tag(*.*.*) to env.GIT_TAG
      - run: echo "GIT_TAG=${GITHUB_REF#refs/tags/}" >> $GITHUB_ENV

      - run: echo "hoge" > hoge.${GIT_TAG}.txt
      - run: echo "fuga" > fuga.${GIT_TAG}.txt
      - run: ls -l

      # Create Releases
      - uses: actions/create-release@v1
        id: create_release
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        with:
          tag_name: ${{ github.ref }}
          release_name: Ver.${{ github.ref }}

      # Upload to Releases(hoge)
      - uses: actions/upload-release-asset@v1
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        with:
          upload_url: ${{ steps.create_release.outputs.upload_url }}
          asset_path: hoge.${{ env.GIT_TAG }}.txt
          asset_name: hoge.${{ env.GIT_TAG }}.txt
          asset_content_type: application/octet-stream

      # Upload to Releases(fuga)
      - uses: actions/upload-release-asset@v1
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        with:
          upload_url: ${{ steps.create_release.outputs.upload_url }}
          asset_path: fuga.${{ env.GIT_TAG }}.txt
          asset_name: fuga.${{ env.GIT_TAG }}.txt
          asset_content_type: application/octet-stream
```

### schedule job on non-default branch

Schedule job will offer `Last commit on default branch`.

> ref: [Events that trigger workflows \- GitHub Help](https://help.github.com/en/actions/reference/events-that-trigger-workflows#scheduled-events-schedule)

schedule workflow should merge to default branch to apply workflow change.

Pass branch info when you want run checkout on non-default branch.
Don't forget pretend `refs/heads/` to your branch.

* good: refs/heads/some-branch
* bad: some-branch

```yaml
name: schedule job
on:
  schedule:
   - cron: "0 0 * * *"
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
        with:
          ref: refs/heads/some-branch
```

## Commit handling

### trigger via commit message

```yaml
name: trigger ci commit

on: ["push"]

jobs:
  build:
    if: "contains(toJSON(github.event.commits.*.message), '[build]')"
    runs-on: ubuntu-latest
    steps:
      - run: echo $COMMIT_MESSAGES
        env:
          COMMIT_MESSAGES: ${{ toJson(github.event.commits.*.message) }}
```

### commit file handling

you can handle commit file handle with github actions [trilom/file\-changes\-action](https://github.com/trilom/file-changes-action).

```yaml
name: pr path changed
on: [pull_request]

jobs:
  changes:
    runs-on: ubuntu-latest
    steps:
      - id: file_changes
        uses: trilom/file-changes-action@v1.2.4
        with:
          output: ","
          pushBefore: master
      - run: echo "${{ steps.file_changes.outputs.files }}"
      - if: contains(steps.file_changes.outputs.files, '.github/workflows/')
        run: echo changes contains .github/workflows/
```


## Issue and Pull Request handling

use [actions/github\-script](https://github.com/actions/github-script).

### skip ci on pull request title

original `pull_request` event will invoke when activity type is `opened`, `synchronize`, or `reopened`.

> [Events that trigger workflows \- GitHub Help](https://help.github.com/en/actions/reference/events-that-trigger-workflows#pull-request-event-pull_request)

```yaml
name: skip ci pr title

on: ["pull_request"]

jobs:
  build:
    if: "!contains(github.event.pull_request.title, '[skip ci]') || !contains(github.event.pull_request.title, '[ci skip]')"
    runs-on: ubuntu-latest
    steps:
      - run: echo $GITHUB_CONTEXT
        env:
          GITHUB_CONTEXT: ${{ toJson(github) }}
      - run: echo $TITLE
        env:
          TITLE: ${{ toJson(github.event.pull_request.title) }}
```

### skip pr from fork repo

default `pull_request` event trigger from even fork repository, however fork pr could not read `secrets` and may fail PR checks.
To control job to be skip from fork but run on self pr or push, use `if` conditions.

```yaml
name: skip pr from fork

on:
  push:
    branches:
      - "master"
  pull_request:
    types:
      - opened
      - synchronize

jobs:
  build:
    runs-on: ubuntu-latest
    # push & my repo will trigger
    # pull_request on my repo will trigger
    if: "(github.event == 'push' && github.repository_owner == 'guitarrapc') || startsWith(github.event.pull_request.head.label, 'guitarrapc:')"
    steps:
      - run: echo build
```

### detect labels on pull request

`pull_request` event contains tags and you can use it to filter step execution.
`${{ contains(github.event.pull_request.labels.*.name, 'hoge') }}` will return `true` if tag contains `hoge`.

```yaml
name: pr label get
on:
  pull_request:
    types:
      - labeled
      - opened
      - reopened
      - synchronize

jobs:
  changes:
    runs-on: ubuntu-latest
    env:
      IS_HOGE: "false"
    steps:
      - run: echo "${{ toJson(github.event.pull_request.labels.*.name) }}"
      - run: echo "IS_HOGE=${{ contains(github.event.pull_request.labels.*.name, 'hoge') }}" >> $GITHUB_ENV
      - run: echo "${IS_HOGE}"
      - run: echo "run!"
        if: env.IS_HOGE == 'true'
```

### skip job when Draft PR

You can skip job and steps if Pull Request is Draft.
Unfortunately GitHub Webhook v3 event not provide draft pr type, but `event.pull_request.draft` shows `true` when PR is draft.

```yaml
name: skip draft pr
on: pull_request

jobs:
  build:
    if: "!(github.event.pull_request.draft)"
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
```

## ADVANCED

Advanced tips.

### Dispatch other repo from workflow

You can dispatch this repository to other repository via calling GitHub `workflow_dispatch` event API.
You don't need use `repository_dispatch` event API anymore.


Target repo `testtest`'s workflow `test.yaml`.

```
name: test
on:
  workflow_dispatch:

jobs:
  test:
    runs-on: ubuntu-latest
    timeout-minutes: 10
    steps:
      - uses: actions/checkout@v2
```

This repo will dispatch event with following worlflow.

```yaml
name: dispatch changes
on:
  workflow_dispatch:

jobs:
  dispatch:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        repo: [testtest] #Array of target repos
        include:
          - repo: testtest
            ref: main
            workflow: test.yml
    steps:
      - name: dispatch ${{ matrix.repo }}
        run: |
          curl -f -X POST \
               -H "authorization: Bearer ${{ secrets.SYNCED_GITHUB_TOKEN_REPO }}" \
               -H "Accept: application/vnd.github.everest-preview+json" \
               -H "Content-Type: application/json" \
               -d '{"ref": "${{ matrix.ref }}"}' \
               https://api.github.com/repos/guitarrapc/${{ matrix.repo }}/actions/workflows/${{ matrix.workflow }}/dispatches
```

You can use [Workflow Dispatch Action](https://github.com/marketplace/actions/workflow-dispatch) insead, like this.

```yaml
name: dispatch changes actions
on:
  workflow_dispatch:

jobs:
  dispatch:
    runs-on: ubuntu-latest
    strategy:
      matrix:
        repo: [testtest] #Array of target repos
        include:
          - repo: testtest
            ref: main
            workflow: test # workflow name, not file name
    steps:
      - name: dispatch ${{ matrix.repo }}
        uses: benc-uk/workflow-dispatch@v1.1
        with:
          repo: ${{ matrix.repo }}
          ref: ${{ matrix.ref }}
          workflow: ${{ matrix.workflow }}
          token: ${{ secrets.SYNCED_GITHUB_TOKEN_REPO }}
```
