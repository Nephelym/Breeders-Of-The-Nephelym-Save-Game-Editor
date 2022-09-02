
using UnrealEngine.Gvas;
using UnrealEngine.Gvas.FProperties;

Console.WriteLine("Please enter the location of the save file.");
Console.WriteLine("Should be in AppData\\Local\\OBF\\Saved\\SaveGames\\");
var fileName = Console.ReadLine().Trim('\"');
if (fileName is null || !new FileInfo(fileName).Exists)
{
    Console.WriteLine("The file you have selected doesn't exist. Make sure you have entered the full path.");
    Console.WriteLine("The programm will exit not");
    Console.ReadKey();
    return;
}

var saveFile = SaveGameFile.LoadFrom(fileName);
var monsters = saveFile.Root!.GetField<FArrayProperty>("PlayerMonsters");

var races = monsters.Elements.OfType<FStructProperty>()
    .Select(x => x.GetField<FStructProperty>("Variant"))
    .Select(x => x.GetField<FArrayProperty>("Tags"))
    .Select(x => (x.Elements.OfType<FStrProperty>().First(x => x.Value?.StartsWith("Race") ?? false).Value))
    .OfType<string>()
    .Concat(new[] { "Race.Human.Breeder" })
    .Distinct()
    .OrderBy(x => x.Length)
    .ToList();

var existingTraitsArray = monsters.Elements
    .OfType<FStructProperty>()
    .Select(x => x.GetField<FStructProperty>("Traits"))
    .Select(x => x.GetField<FArrayProperty>("Tags"))
    .SelectMany(x => x.Elements)
    .OfType<FStrProperty>()
    .Concat(new[] {
new FStrProperty(){Value="Trait.Stat.Charmer.3"},
new FStrProperty(){Value="Trait.Stat.Debauched.3"},
new FStrProperty(){Value="Trait.Stat.Fertile.3"},
new FStrProperty(){Value="Trait.Stat.Hornball.3"},
new FStrProperty(){Value="Trait.Stat.Juicy.3"},
new FStrProperty(){Value="Trait.Stat.Meaty.3"},
new FStrProperty(){Value="Trait.Stat.Nurturing.3"},
new FStrProperty(){Value="Trait.Stat.Potent.3"},
new FStrProperty(){Value="Trait.Appearance.Slender.3"},
new FStrProperty(){Value="Trait.Appearance.Slick.3"},
new FStrProperty(){Value="Trait.Stat.Swift.3"},
new FStrProperty(){Value="Trait.Stat.Stoic.3"},
new FStrProperty(){Value="Trait.Stat.Valuable.3" },

    })
    .DistinctBy(x => x.Value)
    .OrderBy(x=>x.Value)
    .ToList();

int index = 0;
bool redraw = true;
var ordered = monsters.Elements.OfType<FStructProperty>().OrderBy(item => item.GetField<FStrProperty>("Name").Value).ToList();
while (true)
{
    if (redraw)
    {
        Console.Clear();
        var max = Console.WindowHeight - 1;
        var begin = Math.Max(0, index - 10);

        for (int i = begin; i < Math.Min(ordered.Count, max + begin); i++)
        {
            FStructProperty? item = ordered[i];

            var contains = index == i;
            if (i == index)
                Console.Write("> ");
            else
                Console.Write("  ");

            Console.WriteLine(" " + item.GetField<FStrProperty>("Name").Value);
        }
        Console.Write("A: Modify All Monsters, R: Modify Race, T: Modify Traits, S: Set all Ranks to S, Enter: Save, Esc: Exit");
    }
    var k = Console.ReadKey().Key;
    if (k == ConsoleKey.Escape)
    {
        break;
    }
    else if (k == ConsoleKey.A)
    {
        Console.Clear();
        Console.WriteLine("Modify All");
        Console.WriteLine("P = Give all good traits");
        Console.WriteLine("T = Select Traits for all");
        Console.WriteLine("S = Set all Ranks S");
        switch (Console.ReadKey().Key)
        {
            case ConsoleKey.P:
                ModifyTraits(ordered, existingTraitsArray.Where(x => MatchesAny((x, y) => x.Contains(y), x.Value,
           "Charmer.3",
           "Debauched.3",
           "Fertile.3",
           "Hornball.3",
           "Juicy.3",
           "Meaty.3",
           "Nurturing.3",
           "Potent.3",
           "Slender.3",
           "Slick.3",
           "Swift.3",
           "Stoic.3",
           "Valuable.3")).ToList(), monsters, true);
                break;
            case ConsoleKey.T:
                ModifyTraits(ordered, existingTraitsArray, monsters);
                break;
            case ConsoleKey.S:
                foreach (var item in ordered)
                {
                    var fields = item.GetField<FStructProperty>("Stats").Fields;
                    foreach (var rankProp in fields.Where(x => x.Item1.Contains("Rank")))
                    {
                        if (rankProp.Item2 is FByteProperty fb)
                        {
                            fb.Payload[0] = 6;
                        }
                    }
                }
                break;
            default:
                break;
        }
    }
    else if (k == ConsoleKey.S)
    {
        var item = ordered[index];

        var fields = item.GetField<FStructProperty>("Stats").Fields;
        foreach (var rankProp in fields.Where(x => x.Item1.Contains("Rank")))
        {
            if (rankProp.Item2 is FByteProperty fb)
            {
                fb.Payload[0] = 6;
            }
        }
    }
    else if (k == ConsoleKey.R)
    {
        ModifyRace(ordered[index], races);
    }
    else if (k == ConsoleKey.Enter)
    {

        saveFile.Save(fileName);
        Console.Clear();
        Console.WriteLine($"Saved in ${fileName}");
        Console.WriteLine("Press any key to continue");
        Console.ReadKey();
    }
    else if (k == ConsoleKey.T)
    {
        ModifyTraits(new[] { ordered[index] }, existingTraitsArray, monsters);
    }
    else if (k == ConsoleKey.DownArrow)
    {
        if (index == ordered.Count - 1)
            index = 0;
        else
            index = Math.Min(index + 1, ordered.Count - 1);
    }
    else if (k == ConsoleKey.UpArrow)
    {
        if (index == 0)
            index = ordered.Count - 1;
        else
            index = Math.Max(index - 1, 0);
    }
    else
    {
        redraw = false;
        continue;
    }
    redraw = true;
}

bool MatchesAny<T, T2>(Func<T, T2, bool> comparer, T obj, params T2[] filter)
{
    foreach (var item2 in filter)
    {
        if (comparer(obj, item2))
            return true;
    }
    return false;
}


long GetFieldLengthOfFArray(FArrayProperty items)
{
    var itemsStr = items.Elements.Where(x => x is FStrProperty).Cast<FStrProperty>().ToList();
    return itemsStr.Count * 5 + 4 + itemsStr.Sum(x => x.Value.Length);
}

void ModifyRace(FStructProperty monster, List<string> allRaces)
{
    var fArray = monster.GetField<FStructProperty>("Variant").GetField<FArrayProperty>("Tags");

    var race = fArray.Elements.OfType<FStrProperty>().First(x => x.Value?.StartsWith("Race") ?? false);
    allRaces = allRaces.Where(x => x.Length == race.Value.Length).ToList();
    int index = 0;
    while (true)
    {
        Console.Clear();
        var max = Console.WindowHeight - 1;
        var begin = Math.Max(0, index - 10);

        for (int i = begin; i < Math.Min(allRaces.Count, max + begin); i++)
        {
            var item = allRaces[i];

            var shouldAdd = race.Value == item;
            if (i == index)
                Console.Write("> ");
            else
                Console.Write("  ");

            if (shouldAdd)
                Console.Write("  [X]");
            else
                Console.Write("  [ ]");
            Console.WriteLine(" " + item);
        }
        var key = Console.ReadKey().Key;
        if (key == ConsoleKey.Escape)
            break;
        if (key == ConsoleKey.Enter)
        {
            var item = allRaces[index];
            race.Value = item;
        }
        else if (key == ConsoleKey.DownArrow)
        {
            if (index == allRaces.Count - 1)
                index = 0;
            else
                index = Math.Min(index + 1, allRaces.Count - 1);
        }
        else if (key == ConsoleKey.UpArrow)
        {
            if (index == 0)
                index = allRaces.Count - 1;
            else
                index = Math.Max(index - 1, 0);
        }

    }

    //This seems to not solve the length issue
    //Thats why we can only assign different races with the same name length
    monster.GetField<FStructProperty>("Variant").FieldLength = GetFieldLengthOfFArray(fArray);
}

void ModifyTraits(ICollection<FStructProperty> players, List<FStrProperty> allTraitArrays, FArrayProperty monsters, bool selectAll = false)
{
    var playerTraits = players.Select(x => x.GetField<FStructProperty>("Traits")).ToList();
    var traitArrays = playerTraits.Select(x => x.GetField<FArrayProperty>("Tags")).ToList();
    var oldLens = playerTraits.Select(x => x.FieldLength).ToList();

    int index = 0;

    if (selectAll)
    {
        foreach (var item in allTraitArrays)
            foreach (var traitsArr in traitArrays)
            {
                var contains = traitsArr.Elements.OfType<FStrProperty>().Any(x => x.Value == item.Value);
                if (!contains)
                {
                    ((FIntProperty)traitsArr.Elements[0]).Value++;
                    traitsArr.Elements.Add(item);
                }
            }
    }
    else
        while (true)
        {
            Console.Clear();
            var max = Console.WindowHeight - 1;
            var begin = Math.Max(0, index - 10);

            for (int i = begin; i < Math.Min(allTraitArrays.Count, max + begin); i++)
            {
                FStrProperty? item = allTraitArrays[i];

                var shouldAdd = !traitArrays.All(x => x.Elements.OfType<FStrProperty>().Any(x => x.Value == item.Value));
                if (i == index)
                    Console.Write("> ");
                else
                    Console.Write("  ");

                if (!shouldAdd)
                    Console.Write("  [X]");
                else
                    Console.Write("  [ ]");
                Console.WriteLine(" " + item.Value);
            }
            var key = Console.ReadKey().Key;
            if (key == ConsoleKey.Escape)
                break;
            if (key == ConsoleKey.Enter)
            {
                var item = allTraitArrays[index];
                var shouldAdd = !traitArrays.All(x => x.Elements.OfType<FStrProperty>().Any(x => x.Value == item.Value));
                foreach (var traitsArr in traitArrays)
                {
                    var contains = traitsArr.Elements.OfType<FStrProperty>().Any(x => x.Value == item.Value);

                    if (contains && !shouldAdd)
                    {
                        var first = traitsArr.Elements.OfType<FStrProperty>().FirstOrDefault(x => x.Value == item.Value);
                        if (first is null)
                            continue;
                        traitsArr.Elements.Remove(first);
                        ((FIntProperty)traitsArr.Elements[0]).Value--;
                    }
                    else if (!contains && shouldAdd)
                    {
                        ((FIntProperty)traitsArr.Elements[0]).Value++;
                        traitsArr.Elements.Add(item);
                    }
                }
            }
            else if (key == ConsoleKey.DownArrow)
            {
                if (index == allTraitArrays.Count - 1)
                    index = 0;
                else
                    index = Math.Min(index + 1, allTraitArrays.Count - 1);
            }
            else if (key == ConsoleKey.UpArrow)
            {
                if (index == 0)
                    index = allTraitArrays.Count - 1;
                else
                    index = Math.Max(index - 1, 0);
            }


        }
    for (int i = 0; i < traitArrays.Count; i++)
    {
        var item = traitArrays[i];
        var oldLen = oldLens[i];
        var newLen = GetFieldLengthOfFArray(item);
        var player = playerTraits[i];
        var diff = newLen - oldLen;
        monsters.FieldLength += diff;
        player.FieldLength = newLen;

    }
}