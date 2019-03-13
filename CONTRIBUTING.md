[fork]: https://github.com/laurentkempe/GitDiffMargin/fork
[pr]: https://github.com/laurentkempe/GitDiffMargin/compare

# Contributing to Git Diff Margin

Hi there! We're delighted that you'd like to contribute to the **Git Diff Margin**. Your help is appreciated and essential for keeping Git Diff Margin great.

Contributions to this project are released to the public under the [project's open source license](LICENSE.md).

Please note that this project is released with a [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating in this project you agree to abide by its terms.

## Feature request
Do you have a feature that you think would be a great addition to Git Diff Margin, please verify first that we do not have something similar in our [issues labeled feature](https://github.com/laurentkempe/GitDiffMargin/issues?q=is%3Aissue+is%3Aopen+label%3Afeature).
Also take a peek at our [pull requests](https://github.com/laurentkempe/GitDiffMargin/pulls) to see what we're currently working on. 
To suggest a feature use the [feature request template](https://github.com/laurentkempe/GitDiffMargin/issues/new?assignees=&labels=&template=feature_request.md&title=).

## Pull request
1. [Fork][] and clone the Git Diff Margin repository.
2. Create a new branch: `git checkout -b my-branch-name`.
3. Make your change, add tests, and make sure the tests still pass.
4. Push to your fork and [submit a pull request][pr].
5. That's it! Your pull request will be reviewed, discussed if needed and finally merged when ready.

Here are a few things you can do that will increase the likelihood of your pull request being accepted:

- Follow the style/format of the existing code.
- Keep your change as focused as possible. If there are multiple changes you would like to make that are not dependent upon each other, consider submitting them as separate pull requests.
- Write tests for your changes.
- Write a [good commit message](http://tbaggery.com/2008/04/19/a-note-about-git-commit-messages.html).

## Bug Reporting

Here are a few helpful tips when reporting a bug:

- Verify that the bug resides in Git Diff Margin extension.
- If the bug is not related to the Git Diff Margin extension, visit the [Visual Studio support page](https://www.visualstudio.com/support/support-overview-vs) for help.
- To report a bug use the [bug report template](https://github.com/laurentkempe/GitDiffMargin/issues/new?assignees=&labels=bug&template=bug_report.md&title=).
- Screenshots are very helpful in diagnosing bugs and understanding the state of the extension when it's experiencing problems. Please include them whenever possible.
- A log file is helpful in diagnosing bug issues. 

To include log files in your issue:
* Close Visual Studio if it's open
* Open a Developer Command Prompt for VS2017
* Run devenv /log
* Reproduce your issue
* Close VS
* Locate the following file on your system and create a gist and link it in the issue report:
  - `%appdata%\Microsoft\VisualStudio\15.0\ActivityLog.xml`


## Resources

- [Contributing to Open Source on GitHub](https://guides.github.com/activities/contributing-to-open-source/)
- [Using Pull Requests](https://help.github.com/articles/using-pull-requests/)
- [GitHub Help](https://help.github.com)
