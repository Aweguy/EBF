# Introduction & Prerequisites
Hey there, if you're here then that likely means you wish to help develop the EBF mod. This page contains the guidelines for how to get started on that. 

In order to contribute, you will need the following:
- **Some knowledge of the C# programming language**, which is what Terraria and its mods are written in.
- **Terraria and tModLoader**, you will need these to test your changes. They can be found on Steam.
- **A git client**, either via the [Command line](https://git-scm.com/), [GitHub Desktop](https://github.com/apps/desktop) or [Fork](https://git-fork.com/).
- **A GitHub account**, otherwise you can't request your local changes to be merged in.
- **A programming environment**, such as [Visual Studio](https://visualstudio.microsoft.com/).

# Getting started
## Step 1, setting up:
Before you grab any files from here, you need to at least do a first time launch of Terraria. Once the game runs, close the game and then launch tModLoader. This is done to create the necessary folders on your computer, and you won't need to do it a second time.

Next you will need to create your own mod through tModLoader, so you can get a feel for creating Terraria mods. You can follow [this tutorial](https://forums.terraria.org/index.php?threads/modding-tutorial-1-basics.118751/) for that. 

After that, you must create a fork of the EBF mod. On this mod's front page on GitHub, to the right of the name, you'll find a "fork" button. Press this and create your repository.

Finally, you will use your git client to clone the repository from GitHub down to your local computer, make sure to place the files in the ModSources folder. If done right, then you can go into tModLoader and find your copy of the mod in the Develop page.

## Step 2, making changes:
First, you should ensure that your fork of the mod is up to date. Go to your forked repository on GitHub and check the "Sync" button. Ideally, your master branch should be up to date with the EBF mod's master branch.
Additionally, you must create an "Update" branch so that you can be in sync with any changes that have not yet been published to tModloader.

Once that's checked, go into your git client and create a new branch for whatever work you wish to do. From there, you can begin programming. Please do also take a moment to familiarize yourself with the EBF mod files. If your programming environment doesn't recognize the necessary libraries, try to go into tModLoader and build the mod.

When you work, try to choose a focus point and work solely on that. This could be adding a recipe, or creating the default settings of a new item, or to replace a bit of existing logic. Once that focus point is finished, commit it to your branch and give it a fitting name. Spreading out your commits will make it easier for the mod owner to accept or reject the individual changes.

After you're done making changes to whatever you dedicated your branch for, use your git client to push the branch to your repository. You can then visit your repository on GitHub and create a pull request. Make sure you thoroughly check which two branches you are comparing.

If you have any questions, please join our discord server: https://discord.gg/wRqU5nhJQf
