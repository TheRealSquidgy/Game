using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;

/*
v1.5 - Code refactor! Change save files to XML filetype, create orchestrator-style switch to allow dynamic function ordering, 
v1.4 - Introduce new RPG mechanics. A levelling system, critical hits. End-of-round shop function created but not implemented.
v1.3 - Introduced save files - binary filetype.
v1.2 - Made combat repeatable
*/

namespace Gamev1._5
{
    class Program
    {
        static void Main(string[] args)
        {
            Soldier player = new Soldier();
            Sectoid enemy = new Sectoid();
            Inventory playerInventory = new Inventory();
            if (File.Exists(State.SaveLocation))
            {
                player = State.Load(player);
            }

            string nextFunction = "Start";
            while (nextFunction != "Exit")
            {
                switch(nextFunction)
                {
                    case "Start":
                        {
                            nextFunction = Start(player);
                            break;
                        }
                    case "Mission":
                        {
                            Entity winner;
                            Mission.PreCombat(player, enemy);
                            winner = Mission.Combat(player, enemy);
                            Mission.PostCombat(winner);
                            nextFunction = "OptionSelect";
                            break;
                        }
                    case "OptionSelect":
                        {
                            nextFunction = OptionSelect();
                            break;
                        }
                    case "Shop":
                        {
                            nextFunction = GoShopping();
                            break;
                        }
                    default:
                        {
                            Console.Write("This should never run. Something has gone wrong. Press enter to end the session.");
                            Console.ReadLine();
                            //Exit(player);
                            System.Environment.Exit(0);
                            break;
                        }
                }
            }
            //Exit(player);
            System.Environment.Exit(0);
        }

        //Game start menu. Player can start a new game or continue an existing one.
        static string Start(Soldier player)
        {
            Console.WriteLine("Welcome to Squidgy's Intergalactice Arena of Death! \n");
            Console.WriteLine("Press 1 - Start new game");
            Console.WriteLine("Press 2 - Continue");
            Console.WriteLine("Press 3 - Quit");
            if (File.Exists(State.SaveLocation))
            {
                Console.WriteLine("\t Name: " + player.Name);
                Console.WriteLine("\t Level: " + player.Level);
                Console.WriteLine("\t Career winnings: " + player.Moonbux + " Moonbux");
                Console.WriteLine("\t Missions survived: " + player.Survived);
            }
            else
            {
                Console.WriteLine("\t---NO SAVE DATA---");
            }
            string Choice = "";
            while (Choice == "")
            {
                Choice = Console.ReadLine();
                switch(Choice)
                {
                    case "1":
                        {
                            if (File.Exists(State.SaveLocation))
                            {
                                Console.WriteLine("Creating a new soldier will end the current soldier's career!");
                                Console.WriteLine("Are you sure you wish to do this? Y/N");
                                string overwrite_save = Console.ReadLine();
                                if (overwrite_save.ToUpper() == "Y")
                                {
                                    File.Delete(State.SaveLocation);
                                    CreateSoldier(player);
                                    State.Save(player);
                                    Console.WriteLine(player.Name + " saved to file. \n");
                                }
                                else
                                {
                                    Console.WriteLine("Creation cancelled.");
                                    Console.WriteLine("Press 1 to start a new game");
                                    Console.WriteLine("Press 2 to continue a soldier's career");
                                    Choice = "";
                                }
                            }
                            else
                            {
                                CreateSoldier(player);
                            }
                            break;
                        }
                    case "2":
                        {
                            if (File.Exists(State.SaveLocation))
                            {
                                return "Mission";
                                //break;
                            }
                            else
                            {
                                Console.WriteLine("No save data! Please select 1 to create a new soldier.");
                                Choice = "";
                                break;
                            }

                        }
                    case "3":
                        System.Environment.Exit(0);
                    default:
                        {
                            Console.WriteLine("Invalid input. Please enter 1 or 2.");
                            Choice = "";
                            break;
                        }
                }
            }
            return "Mission";
        }

        static string CreateSoldier(Soldier player)
        {
            //Currently handles naming of new soldiers
            //Will be more useful when we introduce classes and abilities
            string name_confirmed = "";
            while (name_confirmed == "")
            {
                Console.WriteLine("Please enter the name of your soldier: ");
                string soldier_name = Console.ReadLine();
                Console.WriteLine("Do you wish you call your soldier " + soldier_name + "? Y/N");
                name_confirmed = Console.ReadLine();
                if (name_confirmed == "y" || name_confirmed == "Y")
                {
                    player.Name = soldier_name;
                    Console.WriteLine("Your soldier is now called " + player.Name + "!");
                    Console.Write("Press any key to continue.");
                    Console.ReadLine();
                    Console.Clear();
                    return "PreCombat";
                }
                else
                {
                    name_confirmed = "";
                    Console.WriteLine("Name cancelled. Please re-enter your soldier's name.");
                }
            }
            return "PreCombat";
        }

        //Exit function removed for now. The redundant save call gets in the way more than it helps.
        //static void Exit(Soldier player)
        //{
        //    //Saves and exits the game. The save process at this stage is cautionary and optional as the last function to write to the save file should have saved on exit anyway.
        //    State.Save(player);
        //    System.Environment.Exit(0);
        //}

        //Win or lose, OptionSelect handles what happens after PostCombat. If the player won (savefile still exists), they can play again, visit the item shop or save and quit. If the player lost, they can return to title screen or quit.
        public static string OptionSelect()
        {
            if (File.Exists(State.SaveLocation))
            {
                string Choice = "";
                while (Choice == "")
                {
                    Console.WriteLine("What would you like to do next? \n");
                    Console.WriteLine("1. Play again.");
                    Console.WriteLine("2. Visit the Item Shop.");
                    Console.WriteLine("3. Save and exit.");
                    Choice = Console.ReadLine();
                    switch (Choice)
                    {
                        case "1":
                            {
                                return "Mission";
                                break;
                            }
                        case "2":
                            {
                                return "Shop";
                                break;
                            }
                        case "3":
                            {
                                return "Exit";
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Invalid input. Please enter 1, 2 or 3 from the above options.");
                                Choice = "";

                            }

                    }
                }
            }
            else
            {
                string Choice = "";
                while (Choice == "")
                {
                    Console.WriteLine("What would you like to do next?\n");
                    Console.WriteLine("1. Start a new game");
                    Console.WriteLine("2. Quit");
                    switch (Choice)
                    {
                        case "1":
                            {
                                return "Start";
                                break;
                            }
                        case "2":
                            {
                                return "Exit";
                                break;
                            }
                        default:
                            {
                                Console.WriteLine("Invalid input. Please enter 1 or 2.");
                                Choice = "";
                            }
                    }
                }
            }
        }
    }

    static class Mission
    {
        //Displays player and enemy stats before the match. May be useful in the future for handling loadouts, selecting team mates, etc
        public static void PreCombat(Soldier player, Sectoid enemy)
        {

            Console.WriteLine("You are {0}, a {1} with {2}HP, armed with a {3} that does {4}-{5} damage.", player.Name, player.Class, player.HPMax, player.WeaponEquipped.Name, player.WeaponEquipped.DmgMin, player.WeaponEquipped.DmgMax);
            Console.WriteLine("Your opponent is a {0} with {1}HP, armed with a {2} that does {3}-{4} damage. \n", enemy.Class, enemy.HPMax, enemy.WeaponEquipped.Name, enemy.WeaponEquipped.DmgMin, enemy.WeaponEquipped.DmgMax);
            Console.WriteLine("Let battle commence! \n");
            Console.ReadLine();
        }

        //Combat continues until there is only one survivor, detected and returned by CheckForSurvivors function. The survivor will be announced in PostCombat.
        public static Entity Combat(Soldier player, Sectoid enemy)
        {
            Entity survivor = null;
            while (survivor == null)
            {
                Console.WriteLine("{0} the {1} - {2}/{3}HP", player.Name, player.Class, player.HPCurrent, player.HPMax);
                Console.WriteLine("Enemy {0} - {1}/{2}HP\n", enemy.Name, enemy.HPCurrent, enemy.HPMax);
                Console.WriteLine("Press 1 to Attack");
                string Choice = "";
                while (Choice = "")
                {
                    Choice = Console.ReadLine();
                    switch(Choice)
                    {
                        case "1":
                            bool criticalHit;
                            criticalHit = Attack(player, enemy);
                            survivor = CheckForSurvivors(enemy, player, criticalHit);
                            criticalHit = Attack(enemy, player);
                            survivor = CheckForSurvivors(player, enemy, criticalHit);
                            break;
                        default:
                            //This screenclear might need adjustment.
                            Console.WriteLine("Invalid input. Press 1 to Attack.");
                            Console.ReadLine();
                            Console.Clear();
                            Choice = "";
                            break;

                    }
                }

            }
            return survivor;
        }

        public static bool Attack(Entity Attacker, Entity Defender)
        {
            bool attackLands = RollToHit(Attacker);
            bool attackCrits = RollToCrit(Attacker);
            if (attackLands)
            {
                int damage = CalculateDamage(Attacker, attackCrits);
                Defender.HPCurrent -= damage;
                if (attackCrits)
                {
                    Console.WriteLine("{0} lands a devastating critical hit on the {1}, doing {2} damage!", Attacker.Name, Defender.Class, damage);
                    return true;
                }
                else
                {
                    Console.WriteLine("{0} fires their {1} at the {2}, doing {3} damage!", Attacker.Name, Attacker.WeaponEquipped.Name, damage);
                    return false;
                }
            }
            else
            {
                Console.WriteLine("{0} fires at the {1}...and misses!", Attacker.Name, Defender.Class);
                return false;
            }
        }

        public static bool RollToHit(Entity Attacker)
        {
            Random random = new Random();
            int rollToHit = random.Next(1, 101);
            if (rollToHit <= Attacker.Aim)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool RollToCrit(Entity Attacker)
        {
            Random random = new Random();
            int rollToCrit = random.Next(1, 101);
            if (rollToCrit <= Attacker.WeaponEquipped.CritChance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static int CalculateDamage(Entity Attacker, bool AttackCrits)
        {
            if (AttackCrits)
            {
                Random random1 = new Random();
                int CritDamage = random.Next(Attacker.WeaponEquipped.CritDmgMin, (Attacker.WeaponEquipped.CritDmgMax + 1));
                return CritDamage;
            }
            else
            {
                Random random2 = new Random();
                int Damage = random2.Next(Attacker.WeaponEquipped.DmgMin, (Attacker.WeaponEquipped.DmgMax + 1));
                return Damage;
            }
        }

        //Checks if damage to victim is fatal, and if so whether it was a critical attack. If so, return the victor.
        public static Entity CheckForSurvivors(Entity Victim, Entity Victor, bool CriticalHit)
        {
            if (Victim.HPCurrent <= 0)
            {
                if (CriticalHit)
                {
                    Console.WriteLine("BOOM headshot! The {0} splatters the {1}'s brains all over the arena, killing them instantly. The match is over!", Victor.Class, Victim.Class);
                }

                else
                {
                    Console.WriteLine("The {0} cuts down the {1} with a withering hail of gunfire. The match is over!", Victor.Class, Victim.Class);
                }
                return Victor;
            }
            else
            {
                return null;
            }
        }

        //Check if a soldier has won. If so, hand out post-combat rewards to the player. If not, delete the player character. Once the winner has been handled, return OptionSelect.
        public static string PostCombat(Entity Winner, Soldier player)
        {
            if (Winner.Class == "Soldier")
            {
                Console.WriteLine("Congratulations, you win!");
                int experienceEarned = GenerateExperience();
                Console.WriteLine("{0} earned {1} experience points!", player.Name, experienceEarned);
                player.Experience += experienceEarned;
                while (player.Experience >= player.ExpReqToLevel)
                {
                    int ExperienceCarriedOver = player.Experience - player.ExpReqToLevel;
                    player.Experience = ExperienceCarriedOver;
                    player.Level++;
                    Console.WriteLine("You are now level {0}!", player.Level);
                    player.ExpReqToLevel = CalculateNewLevelExpReq(player);
                }
                int prize_money = GeneratePrizeMoney();
                player.Moonbux += prize_money;
                Console.WriteLine("{0} wins {1} Moonbux as prize money! \n", player.Name, prize_money);
                int nextLevel = player.Level + 1;
                Console.WriteLine("Progress to level {0}: {1}/{2} \t Moonbux: {3}", nextLevel, player.Experience, player.ExpReqToLevel, player.Moonbux);
                player.HPCurrent = player.HPMax;
                player.Survived += 1;
                State.Save(player);
                Console.WriteLine("{0}'s progress has been saved.", player.Name);
            }
            else
            {
                Console.WriteLine(player.Name + " is dead! Game over.");
                Console.WriteLine("!!!---SAVE DELETED---!!!");
                File.Delete(State.SaveLocation);
            }
            return "OptionSelect";
        }

        public static int GenerateExperience()
        {
            Random ExperienceGain = new Random();
            int Experience = ExperienceGain(80, 121);
            return Experience;
        }

        public static int GeneratePrizeMoney()
        {
            Random randomPayout = new Random();
            int prizeMoney = randomPayout(100, 201);
            return prizeMoney;
        }

        static int CalculateNewLevelExpReq(Soldier player)
        {
            int LevelExpReq = 100 + (player.Level * 50);
            return LevelExpReq;
        }

    }

    static class Shop
    {
        public static Shop()
        {
            List<ShopItem> ShopInventory = new List<ShopItem>(); //Populates ShopInventory at runtime. Is there another way to handle this?           
            ShopInventory.Add(new ShopItem { WeaponData = new AssaultRifle(), ItemPrice = 0 });
            ShopInventory.Add(new ShopItem { WeaponData = new Shotgun(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new SniperRifle(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new LMG(), ItemPrice = 500 });
            ShopInventory.Add(new ShopItem { WeaponData = new RocketLauncher(), ItemPrice = 1000 });
            ShopInventory.Add(new ShopItem { WeaponData = new Minigun(), ItemPrice = 1000 });
        }


    }

    //This class deals data handling; Saving and loading game states.
    static class State
    {
        //The following variable requires System.IO library to make Path.Combine work
        public static string SaveLocation = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save.xml");

        //The Save and Load functions requires System.Xml.Serialization library to work
        public static void Save(Soldier player)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Soldier));
            using (TextWriter writer = new StreamWriter(SaveLocation))
            {
                serializer.Serialize(writer, player);
            }
        }

        public static Soldier Load(Soldier player)
        {
            //Instantiates Xmlserialiser with the object type to deserialize
            XmlSerializer deserializer = new XmlSerializer(typeof(Soldier));
            //Reads the file using FileStream
            FileStream myFileStream = new FileStream(SaveLocation, FileMode.Open);
            //Calls the Deserialize method and casts it to object type
            Soldier RetrievedSaveState = (Soldier)deserializer.Deserialize(myFileStream);
            return RetrievedSaveState;
        }
    }

    [Serializable]
    class Entity
    {
        public string Name;
        public string Class;
        public int HPCurrent;
        public int HPMax;
        public int Aim;
        public Weapon WeaponEquipped;
    }

    [Serializable]
    class Soldier : Entity
    {
        public Soldier()
        {
            Name = "";
            Class = "Soldier";
            HPCurrent = 80;
            HPMax = 80;
            Aim = 75;
            WeaponEquipped = new AssaultRifle();
        }

        public int Level = 1;
        public int Experience = 0;
        public int ExpReqToLevel = 100;
        public int Moonbux = 0;
        public int Survived = 0;

    }

    //player inventory. Currently only stores weapons. This needs to be expanded as items come online.
    [Serializable]
    class Inventory
    {
        public Inventory()
        {
            Weapons = new List<Weapon>();
        }

        public List<Weapon> Weapons;
    }

    class Sectoid : Entity
    {
        public Sectoid()
        {
            Name = "Sectoid";
            Class = "Sectoid";
            HPCurrent = 70;
            HPMax = 70;
            Aim = 60;
            WeaponEquipped = new PlasmaPistol();
        }
    }

    [Serializable]
    class Weapon
    {
        public string Name;
        public int CritChance;
        public int DmgMin;
        public int DmgMax;
        public int CritDmgMin;
        public int CritDmgMax;
        public string Description;
    }

    [Serializable]
    class AssaultRifle : Weapon
    {
        public AssaultRifle()
        {
            Name = "Assault Rifle";
            CritChance = 10;
            DmgMin = 20;
            DmgMax = 40;
            CritDmgMin = 50;
            CritDmgMax = 70;
            Description = "Standard issue firearm for all soldiers.";
        }
    }

    [Serializable]
    class PlasmaPistol : Weapon
    {
        public PlasmaPistol()
        {
            Name = "Plasma Pistol";
            CritChance = 0;
            DmgMin = 20;
            DmgMax = 40;
            CritDmgMin = 50;
            CritDmgMax = 70;
            Description = "An alien weapon. Small but powerful for a pistol.";
        }
    }

    [Serializable]
    class Shotgun : Weapon
    {
        public Shotgun()
        {
            Name = "Shotgun";
            CritChance = 20;
            DmgMin = 30;
            DmgMax = 50;
            CritDmgMin = 60;
            CritDmgMax = 80;
            Description = "Close range assault weapon with high crit damage.";
        }
    }

    [Serializable]
    class SniperRifle : Weapon
    {
        public SniperRifle()
        {
            Name = "Sniper Rifle";
            CritChance = 25;
            DmgMin = 30;
            DmgMax = 50;
            CritDmgMin = 60;
            CritDmgMax = 80;
            Description = "Long range bolt-action weapon with good crit chance.";
        }
    }

    [Serializable]
    class LMG : Weapon
    {
        public LMG()
        {
            Name = "LMG";
            CritChance = 10;
            DmgMin = 20;
            DmgMax = 60;
            CritDmgMin = 60;
            CritDmgMax = 80;
            Description = "The high recoil and firing rate gives this unpredictable weapon a wide damage range.";
        }
    }

    [Serializable]
    class RocketLauncher : Weapon
    {
        public RocketLauncher()
        {
            Name = "Rocket Launcher";
            CritChance = 0;
            DmgMin = 60;
            DmgMax = 60;
            CritDmgMin = 0;
            CritDmgMax = 0;
            Description = "Very powerful anti-tank weapon, its explosive payload ensures consistent high damage.";
        }
    }

    [Serializable]
    class Minigun : Weapon
    {
        public Minigun()
        {
            Name = "Minigun";
            CritChance = 10;
            DmgMin = 30;
            DmgMax = 70;
            CritDmgMin = 80;
            CritDmgMax = 100;
            Description = "A man-portable version of an aircraft weapon system. Unmatched firepower and fire rate!";
        }
    }

    class ShopItem
    {
        public Weapon WeaponData;
        public int ItemPrice;
    }
}
