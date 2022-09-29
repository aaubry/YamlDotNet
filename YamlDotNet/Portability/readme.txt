The subdirectories of this directory contain platform-specific code
that aims to bridge the gap between the different targetted platforms.

Each directory should contain code that should only be included or excluded
on a set of platforms. The name of the directory should consist of a list
of target platforms, separated by a plus sign (+).
In that directory, every file inside the 'include' subdirectory
will be included in those platforms, while every file inside the 'exclude' subdirectory
will be included in all other platforms.

Most (all?) types added to this folder should be internal to avoid conflicting
with other libraries.
