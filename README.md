# RestoreIdeUI
VSIX package to restore Visual Studio 2019 UI to preferred VS2017 style

[![Build status](https://ci.appveyor.com/api/projects/status/36b9d7v0nfsycgx9/branch/master?svg=true)](https://ci.appveyor.com/project/UbiquityDotNet/restoreideui/branch/master)

## Features
This extension enables users of VS2019 to effectively revert some of the controversial UI changes made to VS2019.
* Hide the search box (again)
* Use the start page (again)
   * Including news feed (again)
* Restore the new project dialog to it's previous form

## Justification
VS2019 introduced a number of controversial changes that triggered some rather heated discussions in the user feedback.
Unfortunately, the feedback was not enough to effect any significant changes (other than restoring the option to keep
the title bar layout from VS2017). THerefore, after a bunch of spelunking with the debugger and ILSpy, this package was
created to restore the rest of the most controversial changes. 

### Search Box
In all prior versions of VS since the QuickLaunch search box was introduced, users have always had the option to
disable/hide it from the menus etc... Not everyone finds it useful, and many view it as a distracting waste of space
on the menus or title bar. Unfortunately, in VS2019 this functionality was removed, such that users can't disable/hide
it. This package allows users to hide the search box again as was always possible in previous versions of the IDE.

### Start Page
VS2019 for reasons that surpass comprehension, chose to eliminate the start page that has been present in VS for many
years now (though it's design has changed and even has/had support for individuals to create their own pages with the
VS SDK) For VS 2019 it was replaced with a floating window instead of a dockable page, worse the window appears at
launch blocking the main IDE until you pick something. This is annoying enough that it became the first thing to figure
out how to disable in the pre-releases. Fortunately, the start with a blank project startup option still exists so that
UI could be hidden. The command to load the start page, still exists, and with some manual customization a user can place
the command to open the start page onto one of the menus. However, this still required manually opening the page. Additionally,
one of the major benefits of the start page was still disabled - the news feed.

#### Start page News Feed
The start page contains an RSS news feed to help keep developers informed about what's happening with all things related
to Visual Studio and Microsoft development in general. This is arguably one of the biggest values of the start page.
(Some of us complained the reduction in screen space of the news feed in previous releases was a bad thing, so eliminating
or disabling it completely is just flat out wrong in our view) Getting this to work again was the most complex and fragile
part of creating this package. It is hoped that the VS team will see the efforts that we are willing to take on to get
this functionality back that they adjust their planning and bring it back officially.

Start page Screenshot with newsfeed enabled (VS2019):
![Screenshot with newsfeed](https://github.com/UbiquityDotNET/RestoreIdeUI/blob/master/RestoreIdeUI/RestoreUI.png)

### New Project Dialog
The new project dialog received a LOT of negative feedback from the first appearance. Generally the feedback is along the
lines of "It ain't broke so don't fix it". In a [blog article](https://devblogs.microsoft.com/visualstudio/redesigning-the-new-project-dialog/)
the team attempts to explain the rational for the changes. Though an important key factor is missing from the "data' they
collected. (Part of the problem of relying entirely on data is that you have to interpret it and doing that without actually
talking openly with your users about what the data means often leads to lots of missteps and frustrations for all) The
missing information was that, yes people use the new project dialog a lot more than what was assumed (a problem of it's
own), but what was missing was that there wasn't a correspondingly high level of feedback on the new project dialog (e.g.
users were using it a lot but didn't have complaints enough about it to warrant any negative feedback) - so why change what
people are using more than you thought, and not complaining about it!? That's not good disruption. 

Fortunately, a happy accident/bug in the preview triggered a problem showing the new experience so VS would show an error,
and then... FALBACK to the original UI! Bingo, the original UI was still there. So, a bit of digging revealed the preview
setting to disable it was still active and could restore the new project and item dialog experiences

Yeah, productivity again.
