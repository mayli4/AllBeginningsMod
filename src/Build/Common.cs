#if BUILDINGFROMCSPROJ_CLIENT || BUILDINGFROMCSPROJ_SERVER || BUILDINGFROMCSPROJ_MOD
using Microsoft.Xna.Framework;
using MonoMod.RuntimeDetour;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System;
using Terraria.ModLoader;
using Terraria;
using System.Buffers.Binary;
using System.Threading;
using System.Text;
using System.Runtime.InteropServices;
using MonoMod.Core.Utils;
using System.Collections;
using System.IO.Compression;
using Terraria.ModLoader.Core;
using MonoMod.Cil;
using System.Security.Cryptography;
using Mono.Cecil.Cil;

#if !BUILDINGFROMCSPROJ_MOD

[assembly: System.Reflection.AssemblyCompanyAttribute("TheKnightsBeningingOfTheTrelamiumCrusadersForTheEtheralCowGirlsInTheHorizon")]
#if DEBUG
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
#elif RELEASE
[assembly: System.Reflection.AssemblyConfigurationAttribute("Release")]
#endif

#endif

internal static class Launcher {
    const BindingFlags finstance = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    const BindingFlags fstatic = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
    public static bool IsClientEx { get; private set; }
    public static bool IsServer { get; private set; }
    public static bool IsMod { get; private set; }
    
    public static void ClientMain(string[] args, int clientIndex) {
        string file = args?.FirstOrDefault();
        Console.WriteLine(file);
        Console.WriteLine($"current dir: {Environment.CurrentDirectory}");
        if (!File.Exists(file)) {
            Console.WriteLine($"Missing tml path, does not exist or is not accessible");
            try
            {
                file = File.ReadAllLines("tmlpath.txt")[0];
                Console.WriteLine($"Using tml path from tmlpath.txt");
            }
            catch (FileNotFoundException)
            {

                Console.WriteLine("tmlpath.txt file not found");
                return;
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"Missing tml path in tmlpath.txt");
                return;
            }
        }
        Environment.CurrentDirectory = new FileInfo(file).Directory.FullName;

        IsMod = clientIndex == 0;
        IsServer = clientIndex == -1;
        IsClientEx = clientIndex > 0;
        DoRun(args);
    }

    public static void ServerMain(string[] args) {
        ClientMain(args, -1);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    static void DoRun(string[] args) {
        applyingDetoursTask = Task.Run(ApplyDetours).ContinueWith(t => Console.WriteLine($"Finished applying detours in {sw.Elapsed}"));

        string[] mainArgs = ["-console", .. args];//   args; 
        if (IsServer)
            mainArgs = ["-server", .. args];
        typeof(ModLoader).Assembly.EntryPoint.Invoke(null, [mainArgs]);
    }

    public static event Action<List<Hook>> RegisterDetours;

    private static List<Hook> detours = new(10);
    private static List<ILHook> ilhooks = new(10);
    private static Task applyingDetoursTask;
    private static Stopwatch sw;

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    static void ApplyDetours() {
        Assembly tmlAssembly = typeof(ModLoader).Assembly;
        sw = Stopwatch.StartNew();

        Type amt = tmlAssembly.GetType("Terraria.ModLoader.Core.AssemblyManager");
        Type tpt = tmlAssembly.GetType("Terraria.Program");
        Type tmt = typeof(Terraria.Main);
        Type mot = tmlAssembly.GetType("Terraria.ModLoader.Core.ModOrganizer");

        detours.Add(new Hook(amt.GetMethod("IsLoadable", fstatic),
            (Func<object, Type, bool> orig, object mod, Type type) => true, false));

        detours.Add(new Hook(amt.GetMethod("JITAssemblies", fstatic),
            (Action<IEnumerable<Assembly>, PreJITFilter> orig, IEnumerable<Assembly> assemblies, PreJITFilter filter) => { }, false));

        detours.Add(new Hook(tpt.GetMethod("ForceJITOnAssembly", fstatic)!,
            (Action<IEnumerable<Type>> orig, IEnumerable<Type> assemblies) => { }, applyByDefault: false));

        detours.Add(new Hook(tpt.GetMethod("ForceStaticInitializers", fstatic, [typeof(Assembly)])!,
            (Action<Assembly> orig, Assembly assemblies) => { }, applyByDefault: false));

        detours.Add(new Hook(tmt.GetMethod("LoadContent", finstance)!, static (Action<Main> orig, Main self) => {
            if (applyingDetoursTask?.IsCompleted is false) {
                Console.WriteLine("Waiting detours");
                applyingDetoursTask.ConfigureAwait(false).GetAwaiter().GetResult();
            }
            orig(self);
        }, false));

        detours.Add(new Hook(tmt.GetMethod("DrawSplash", finstance)!, static (Action<Main, GameTime> orig, Main self, GameTime gameTime) => {
            Console.WriteLine("Fast splash start");
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 900 && Terraria.Main.showSplash; i++)
            {
                orig(self, gameTime);
                Terraria.Main.Assets.TransferCompletedAssets();
            }
            sw.Stop();
            Console.WriteLine($"Fast DrawSplash time: {sw.Elapsed}");

        }, false));

        // to trigger recompilation
        detours.Add(new Hook(amt.GetMethod("GetLoadableTypes", fstatic, [amt.GetNestedType("ModLoadContext", fstatic | finstance), typeof(MetadataLoadContext)]),
            (Func<object, MetadataLoadContext, IDictionary<Assembly, Type[]>> orig, object mod, MetadataLoadContext mlc) => { return orig(mod, mlc); }, false));

        RegisterDetours?.Invoke(detours);

        List<Action> actions = new(64);

        for (int i = 0; i < detours.Count; i++)
            actions.Add(detours[i].Apply);
        for (int i = 0; i < ilhooks.Count; i++)
            actions.Add(ilhooks[i].Apply);

        Parallel.Invoke([.. actions]);
        sw.Stop();
    }
}
#endif