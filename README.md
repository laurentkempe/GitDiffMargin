 Git Diff Margin [![Build status](https://ci.appveyor.com/api/projects/status/n2j1hcqpdel0xj0c/branch/master?svg=true)](https://ci.appveyor.com/project/laurentkempe/gitdiffmargin/branch/master) [![Gitter](https://img.shields.io/gitter/room/inferred/freebuilder.svg?style=flat-square)](https://gitter.im/GitDiffMargin/Lobby)


Git Diff Margin displays live Git changes of the currently edited file on Visual Studio margin and scroll bar.

* Supports Visual Studio 2012 through Visual Studio 2017
* Quickly view all current file changes on
    * Left margin
    * Scroll Bars in map and bar mode with and without source overview
        * blue rectangle for modifications
        * green rectangles for new lines
        * red triangles for deletions
        * all colors configurable through Visual Studio Fonts and Colors options
* Undo the change
* Copy the old code into the clipboard
* Copy a part of the old code by selecting it in the popup
* Show the diff in Visual Studio Diff window
* Navigate to previous/next change on the file using user defined keyboard shortcuts or the popup icons
* Open popup with user defined keyboard shortcuts, close with esc key 
* Support Visual Studio 2013 Dark, Light and Blue Theme
* Support zoom

![Screenshot](https://farm4.staticflickr.com/3893/15335334635_a88dc1f271.jpg)

Git Diff Margin version 3.2.2 is the latest release supporting Visual Studio 2010 it uses LibGit2Sharp v0.23.1 and is not being maintained. You can [download it here](https://github.com/laurentkempe/GitDiffMargin/releases/tag/v3.2.2).

## Installation

Grab it from inside of Visual Studio's Extension Manager, or via the [Extension Gallery link](https://marketplace.visualstudio.com/items?itemName=LaurentKempe.GitDiffMargin)

## Video

You might see a little video on the [following page](https://www.flickr.com/photos/laurentkempe/14879945429/).

## Feedback

* Please use the [**Issue Tracker**](https://github.com/laurentkempe/GitDiffMargin/issues) to report bugs and feature requests
* Write a [**review**](https://marketplace.visualstudio.com/items?itemName=LaurentKempe.GitDiffMargin#review-details)
* Tweet me [![Follow on Twitter](https://img.shields.io/twitter/url/http/realvizu.svg?style=social&label=@laurentkempe)](https://twitter.com/laurentkempe)

## Credits

Thanks to

* Sam Harwell [@sharwell](https://github.com/sharwell) for all the improvements
* Rick Sladkey [@ricksladkey](https://github.com/ricksladkey) for the fixes
* Ethan J. Brown [@Iristyle](https://github.com/Iristyle) for the chocolatey package
* [@heinzbeinz](https://github.com/heinzbeinz) for the support of Visual Studio 15 preview
* Jamie Cansdale [@jcansdale](https://github.com/jcansdale) for bugfix
* Charles Milette [sylveon](https://github.com/sylveon) for bugfix
