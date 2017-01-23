using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SimpleECS;

namespace TestApp {
    public abstract class MyEntityBase : Entity { }

    public class AgeComponent : Component {
        public ulong Age;

        public AgeComponent(ulong age = 0) {
            Age = age;
        }

        public override string ToString() => $"{Age:N0}";
    }

    public class BranchComponent : Component {
        public byte Branches;
        public double BranchBooster;

        public BranchComponent(byte branches = 0, double branchBooster = 1) {
            if (branchBooster < 1)
                throw new ArgumentOutOfRangeException(nameof(branchBooster), "Branch booster must be >= 1");
            Branches = branches;
            BranchBooster = branchBooster;
        }

        public override string ToString() => $"{Branches:N0}";
    }

    public class TreeEntity : MyEntityBase {
        public string Name { get; }
        public readonly AgeComponent AgeComponent;
        public readonly BranchComponent BranchComponent;

        public TreeEntity(string name, ulong age = 0, byte branches = 0, double branchBooster = 1) {
            Name = name;
            AgeComponent = new AgeComponent(age);
            BranchComponent = new BranchComponent(branches, branchBooster);
        }

        public override IEnumerable<Component> Components => new Component[] { AgeComponent, BranchComponent };

        public override string ToString() => $"Tree {Name} with {AgeComponent} rings and {BranchComponent} branches. (x{BranchComponent.BranchBooster:N0})";
    }

    public class AgingSystem : System<TriggerEnum, TriggerData>.ComponentSystem<AgeComponent> {
        public AgingSystem(Universe<TriggerEnum, TriggerData> universe) : base(universe) { }

        public override IEnumerable<TriggerEnum> Triggers { get { yield return TriggerEnum.Tick; } }

        public override void OnTrigger(TriggerEnum trigger, TriggerData data, AgeComponent component) {
            //Console.WriteLine("\t\tIncrementing age...");
            component.Age++;
        }
    }

    public class BranchingSystem : System<TriggerEnum, TriggerData>.ComponentSystem<BranchComponent> {
        private readonly double _branchProbability;

        public BranchingSystem(Universe<TriggerEnum, TriggerData> universe, double branchProbability = 2) : base(universe) {
            _branchProbability = branchProbability;
        }

        public override IEnumerable<TriggerEnum> Triggers { get { yield return TriggerEnum.Tick; } }

        public override void OnTrigger(TriggerEnum trigger, TriggerData data, BranchComponent component) {
            if (data.Rand.NextDouble() * (_branchProbability / component.BranchBooster) < 1.0) {
                //Console.WriteLine("\t\tBranching...");
                component.Branches++;
            }//else
                //Console.WriteLine("\t\tNot branching...");
        }
    }

    public class PrintSystem : System<TriggerEnum, TriggerData>.EntitySystem {
        public PrintSystem(Universe<TriggerEnum, TriggerData> universe) : base(universe) { }
        private static readonly IEnumerable<TriggerEnum> _triggers = new[] { TriggerEnum.PreTick, TriggerEnum.PostTick };
        public override IEnumerable<TriggerEnum> Triggers => _triggers;
        public override void PreTrigger(TriggerEnum trigger, TriggerData data) {
            switch (trigger) {
                case TriggerEnum.PreTick:
                    Console.WriteLine("Prestate: ");
                    break;
                case TriggerEnum.PostTick:
                    Console.WriteLine("Poststate: ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void OnTrigger(TriggerEnum trigger, TriggerData data, Entity entity) => Console.WriteLine($"\t{entity}");

        public override void PostTrigger(TriggerEnum trigger, TriggerData data) => Console.WriteLine();
    }

    public class LumberjackSystem : System<TriggerEnum, TriggerData>.EntitySystemByEntity<TreeEntity> {
        public LumberjackSystem(Universe<TriggerEnum, TriggerData> universe) : base(universe) {}
        public override IEnumerable<TriggerEnum> Triggers { get{yield return TriggerEnum.PreTick;} }
        public override void OnTrigger(TriggerEnum trigger, TriggerData data, TreeEntity tree) {
            if (tree.AgeComponent.Age > 300) {
                //Console.WriteLine($"\t\tChopping tree {tree.Name}...");
                Universe.RemoveEntity(tree);
            }//else Console.WriteLine($"\t\tNot chopping tree {tree.Name}.");
        }
    }

    public enum TriggerEnum {
        PreTick,
        Tick,
        PostTick
    }

    public struct TriggerData {
        public Random Rand;

        public TriggerData(Random rand) {
            Rand = rand;
        }
    }

    internal class Program {
        private static readonly Universe<TriggerEnum, TriggerData> Universe = new Universe<TriggerEnum, TriggerData>();

        private static void Main() {
            var rand = new Random();

            for (var i = 0; i < 100000; i++)
                Universe.AddEntity(new TreeEntity($"{i+1}", (ulong)rand.Next(500), (byte)rand.Next(5), rand.NextDouble() * 3 + 1));

            Universe.AddSystem(new AgingSystem(Universe));
            Universe.AddSystem(new BranchingSystem(Universe));
            //Universe.AddSystem(new PrintSystem(Universe));
            Universe.AddSystem(new LumberjackSystem(Universe));

            //Console.WriteLine("System primed and ready.");
            //Console.ReadLine();

            //var rounds = 0;
            var count = 1;
            var tdata = new TriggerData(rand);
            //var sw = new Stopwatch();
            while (count > 0) {
                //rounds++;
                //sw.Restart();
                Universe.Trigger(TriggerEnum.PreTick, tdata);
                Universe.Trigger(TriggerEnum.Tick, tdata);
                Universe.Trigger(TriggerEnum.PostTick, tdata);
                //sw.Stop();
                count = Universe.GetEntities().Count();
                //Console.WriteLine($"{count,6:N0} trees remain, updated took {sw.ElapsedMilliseconds,6:N0}ms.\t{new string('|', (int) Math.Log(sw.ElapsedMilliseconds, 2))}");
            }
            //Console.WriteLine($"Finished in {rounds:N0} rounds");
            //Console.ReadKey();
        }
    }
}
