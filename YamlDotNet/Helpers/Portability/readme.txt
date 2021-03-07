The subdirectories of this directory contain platform-specific code
that aims to bridge the gap between the different targetted platforms.

Each directory sould contain code that should only be included
on a set of platforms. The name of the directory should consist of a list
of target platforms, separated by a plus sign (+).
Optionally, a sub-directory named 'others' may be added. Its contents will be
included in all other target platforms.

Most (all?) types added to this folder should be internal to avoid conflicting
with other libraries.
