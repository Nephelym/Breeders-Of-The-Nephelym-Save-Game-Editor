#  Breeders of the Nephelym SaveGame Editor

Simple console app to modify some variables of the save game Breeders of the Nephelym.

### Supported Edits

All Edits also support editing the breeder itself, even though Traits don't show up in ui.

- Race 
	- Currently ownly Races with the same name length can be set
	- When changing the breeders race, the customizations are also for that race, like in spirit form
	- Other monsters can be set to the breeders race, but two breeders will never produce any offspring
	- Only discovered races can be selected, but others can manually be added by changing a bit of code, where the breeders race is always added
- Set Stat Ranks to S
	- The Ranks can't be selected, it will set all six to S
- Change Traits
	- Only discovered traits can be selected
	- Most of the positive ones are also added in rank 3
	- Level can currently not be changed
	- Having multiple traits of the same type doesn't seem to stack, but nothing prevents you from doing this

___

### Difficult to implement

- Appereance
	- A lot of properties, but all are already parsed (So more busy work than difficulty)
	- Would be better if a proper gui would be used instead of console
- Quests & Vagrants
	- These are currently only parsed into a "raw data array", as the gvas parser was difficult to iplement with those
	- Most likely these won't be supported for a longer time, as it requires changing the Gvas Parser, adding a new FType and more

____

### Gvas Parser

The UnrealEngine.Gvas parser was copied from https://github.com/SparkyTD/UnrealEngine.Gvas and modified to support this game.

The modifications are sometimes hacky / a workaround that's why I decided to not open a pull request. 

Making it work correctly without those is to much work for now, so the modification is part of this repository.

___

### Supported Game Versions

Currently only Version 0.756.4 is tested, but it's very likely to work on other versions aswell.

___

### Contributing

If you want to contribute you can do so by opening an issue, writing a discussion or opening a pull request.

Most likely I won't fix issues / not in a timely fassion, because this tool currently supports everything, that I wanted.