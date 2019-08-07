# Git Diff Margin

Git Diff Margin displays live Git changes of the currently edited file on Visual Studio margin and scroll bar.

*   Supports Visual Studio 2012 through Visual Studio 2019 Preview 1
*   Quickly view all current file changes on
    *   Left margin
    *   Scroll Bars in map and bar mode with and without source overview
        *   blue rectangle for modifications
        *   green rectangles for new lines
        *   red triangles for deletions
        *   all colors configurable through Visual Studio Fonts and Colors options
*   Undo the change
*   Copy the old code into the clipboard
*   Copy a part of the old code by selecting it in the popup
*   Show the diff in Visual Studio Diff window except for Visual Studio 2010 which still use configured Git external diff tool
*   Navigate to previous/next change on file using user defined keyboard shortcuts or the popup icons
*   Open popup with user defined keyboard shortcuts, close with esc key 
*   Support Visual Studio 2013 Dark, Light, and Blue Theme
*   Support zoom

![](/142621/1/gitdiffmargin-preview.png)

# Get the code

[https://github.com/laurentkempe/GitDiffMargin](https://github.com/laurentkempe/GitDiffMargin)

# Installation

You might also install it using the following [Chocolatey package](https://chocolatey.org/packages/GitDiffMargin).

# Report Issue

* To report a bug, please use the [**Issue Tracker**](https://github.com/laurentkempe/GitDiffMargin/issues/new?assignees=&labels=bug&template=bug_report.md&title=)
* To suggest an idea, please use the [**Issue Tracker**](https://github.com/laurentkempe/GitDiffMargin/issues/new?assignees=&labels=&template=feature_request.md&title=)

# Release Notes

## Version 3.10.0

### New features

* Diff against something other than HEAD (Advanced users)

### Fix

* Fix memory leak

## Version 3.9.4

### Fix

* Fix files with CRLF have whole file highlighted as modified

## Version 3.9.3

### New feature

* Chocolatey package

## Version 3.9.2

### Fix

* Fix show the diff showing empty content

## Version 3.9.1

### Security fix

* Update to LibGit2Sharp v0.26.0

## Version 3.9.0

### New feature

* Add support for Visual Studio 2019 Preview 1

## Version 3.8.2

### Fixes

* Fix MoeIDE conflict

## Version 3.8.1

### Fixes

* Keyboard shortcut to show diff at a change works only on 1st line of a change
* Avoid to focus textbox when opening popup
* Avoid crashing if GitDiffMarginCommandHandler is not in the property bag

## Version 3.8.0

### New feature

* Center when moving to next/previous change with keyboard shortcus

### Fix

* Address the "Gold Bar" notification saying Git Diff Margin was responsible for editor slowness

## Version 3.7.1

### Security fix

* Update to LibGit2Sharp v0.24.1 This is a security release fixing two issues. It updates libgit2's included zlib to 1.2.11, and includes a libgit2 fix for memory handling issues when reading crafted repository index files.

## Version 3.7.0

### New feature

*   Add keyboard shortcut to show diff popup

## Version 3.6.0

### New feature

*   Close popup using Escape key

## Version 3.5.3

### Fixes

*   Fix unhandled AccessViolationException crashes in Visual Studio updating LibGit2Sharp

## Version 3.5.2

### Fixes

*   Fix diff margin not shown when editing a project file  

## Version 3.5.1

### Fixes

*   Fix diff window complains that files have different encodings 

## Version 3.5.0

### New feature

*   Double-clicking on diff bar open `Show Difference`

## Version 3.4.0

### Fixes

*   Fix installation in Visual Studio 2013 and earlier

## Version 3.3.0

### New features

*   Add support for Visual Studio 2017 RC

### Fixes

*   Fix issue to upload on the marketplace

### Other

*   Drop support for Visual Studio 2010, download latest version supporting it [here](https://github.com/laurentkempe/GitDiffMargin/releases/tag/v3.2.2).

## Version 3.2.2

### Fixes

*   Update to LibGit2Sharp v0.23.1

## Version 3.2.1

### Fixes

*   Breaks Edit.GoTo command
*   Update to LibGit2Sharp v0.23 
*   Automate AppVeyor build to create vsix

## Version 3.2.0

### New features

*   Add support for Visual Studio 15 Preview and now Visual Studio 2017 RC

## Version 3.1.2

### Fixes

*   Update LibGit2Sharp and LibGit2Sharp.NativeBinaries
*   Visual Studio crashes #96
*   With 3.1.1 use of memory of VS2013 increased until crashing #93
*   Crash in git2-785d8c4.dll seen in version 3.1.1.0 #92

## Version 3.1.1

### Fixes

*   Fix SQL Server Object Explorer issue #81
*   Fix Enabling GitDiffMargin kills Git Source Control Provider #85
*   Fix Memory corruption in underlying libgit2 native DLL #87

## Version 3.1.0

### Improvements

*   Move diff bar to the right of the line numbers #71 

## Version 3.0.0

### New features

*   Support for Visual Studio 2010, 2012 and Visual Studio 2015
*   Show diff using Visual Studio Diff window except for Visual Studio 2010 which still use external diff tool
*   Possibility to define shortcuts for next/previous change navigation
*   Add options for highlighting untracked lines #29
*   Update icons

### Improvements

*   Improve external diff configuration handling in .gitconfig #32
*   Improve "removed" glyph and editor diff positioning
*   Improve support of Dark, Light and Blue theme
*   Make sure the text editor is focused after a rollback
*   Prevent ScrollDiffMargin from affecting the scroll bar behavior
*   Play nice with other source control providers

### Fixes

*   Fix Show Difference fails with DiffMerge for file names with spaces #38
*   Fix submodules issue #40

[Previous release notes](https://github.com/laurentkempe/GitDiffMargin/releases)

# Credits

Thanks to

*   Sam Harwell [@sharwell](https://github.com/sharwell) for all the improvements
*   Rick Sladkey [@ricksladkey](https://github.com/ricksladkey) for the fixes and features
*   [@Iristyle](https://github.com/Iristyle) for the chocolatey package
*   Yves Goergen [@](https://github.com/dg9ngf)[dg9ngf](https://github.com/dg9ngf)
*   [@heinzbeinz](https://github.com/heinzbeinz) for the support of Visual Studio 15 preview
*   Jamie Cansdale [@jcansdale](https://github.com/jcansdale) for bugfix
*   Charles Milette [sylveon](https://github.com/sylveon) for bugfix
*   Gary Ewan Park [@gep13](https://github.com/gep13) for the new chocolatey package
*   Duncan Smart [@duncansmart](https://github.com/duncansmart) for bug fix
