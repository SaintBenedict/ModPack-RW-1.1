# De-generalize Work
RimWorld version 1.1 lumped SculptingSpeed, SmeltingSpeed, SmithingSpeed and TailorSpeed into one stat (GeneralLaborSpeed), unfortunately this change is a huge blow to trait mods (including my own Additional Traits mod) that used those stats to give the game just a little bit more depth. As such, this mod adds those stats back as dependents of GeneralLaborSpeed, this means that changes to GeneralLaborSpeed will be passed down to SculptingSpeed, SmeltingSpeed, SmithingSpeed and TailorSpeed, thus (hopefully) ensuring compatibility.

## Modders
If your mod includes traits recreated by De-generalize Work, the following section will explain how to integrate them or set up your mod so that compatibility maintained with the core game (ie the game doesn't vomit red errors when the user does something stupid). I've implemented some of these in my [Additional Traits mod](https://github.com/Alias44/Gewens-Additional-Traits), if it helps so see how these things work in practice, feel free to check it out.

### Load Before
At the very least you'll want to tell the game that De-generalize Work should be loaded before your mod, this won't require the user to have De-generalize Work, but if it is installed and activated the game will auto-sort it accordingly or otherwise indicate that mods are loaded in the wrong order (the game will still probably load even if the order is wrong, but it's good practice to load things before you try to use them). You can do this by including a loadAfter tag in the ```ModMetaData``` section of your ```About.xml``` file.
```XML
<loadAfter>
	<li>Alias.DegeneralizeWork</li>
</loadAfter>
```

### Mod Dependency
If your mod heavily relies on the stats that De-generalize Work adds back, you'll also want to mark it as a required dependency, so that the game notifies the user if De-generalize Work is not installed or not active, this is also done in the ```ModMetaData``` section of your ```About.xml``` file.
```XML
<modDependenciesByVersion>
    <v1.1>
        <li>
            <packageId>Alias.DegeneralizeWork</packageId>
            <displayName>De-generalize Work</displayName>
            <steamWorkshopUrl>steam://url/CommunityFilePage/2011655761</steamWorkshopUrl>
            <downloadUrl>https://github.com/Alias44/Degeneralize-Work/releases/latest</downloadUrl>
        </li>
    </v1.1>
</modDependenciesByVersion>
```

### Pure XPATH
If your mod only has a few things that depend on De-generalize Work, your mod can be configured to stand on its own and patch itself when De-generalize Work is also loaded. This works well when a trait already has features, but you'd like to add in or swap them out for the traits in De-generalize Work. For more robust examples/ explanations of XPATH in RimWorld, feel free to check out [Zhentar's guide](https://gist.github.com/Zhentar/4a1b71cea45b9337f70b30a21d868782) or the [RimWorld wiki page on XPATH](https://rimworldwiki.com/wiki/Modding_Tutorials/PatchOperations). In this example, the GeneralLaborSpeed stat in GAT_Vulcan trait from Additional Traits is replaced with SmithingSpeed if De-generalize Work is detected in the mods that the user has enabled.
```XML
<Operation Class="PatchOperationFindMod">
	<mods>
		<li>De-generalize Work</li>
	</mods>
	<match Class="PatchOperationReplace">
		<xpath>Defs/TraitDef[defName="GAT_Vulcan"]/degreeDatas/li/statOffsets/GeneralLaborSpeed</xpath>
		<value>
			<SmithingSpeed>1</SmithingSpeed>
		</value>
	</match>
</Operation>
```

### Conditional LoadFolders
In some cases loading in new traits through XPATH may not be the most efficient or modder friendly way to setup your mod, such as creating several traits, instances where having a file associated with a def is important, or you would otherwise like files to only be loaded when another mod is activated. To do this, you can create a folder in the main directory of your mod for files specific to De-generalize Work (the example will assume that it's called ```DegeneralizeWork_Specific```, but you can name it whatever, just be sure to change the code below if you are copy/pasting), this folder should subfolders similar to the core structure of your mod (ie a folder for Defs, Patches, etc. where applicable), if you don't have one already create a file called ```LoadFolders.xml``` in the main directory of your mod, the loadFolders file should look similar to the following (it may look different depending on your personal preferences or how closely you followed Brrainz's excellent [1.1 update tutorial](https://gist.github.com/pardeike/08ff826bf40ee60452f02d85e59f32ff)):
```XML
<?xml version="1.0" encoding="utf-8"?>
<loadFolders>
  <v1.1>
    <li>/</li> <!--This part tells the game that 1.1 related stuff can be found at the main directory of your mod-->
    <li IfModActive="Alias.DegeneralizeWork">DegeneralizeWork_Specific</li><!--This part tells the game that De-generalize Work related stuff can be found in the DegeneralizeWork_Specific folder-->
  </v1.1>
</loadFolders>
```

## Licensing
This mod is licensed under the GNU General Public License v2.0

## Thanks
* to all the wonderful people on the RimWorld Discord

## Links
[Steam link](https://steamcommunity.com/sharedfiles/filedetails/?id=2011655761)