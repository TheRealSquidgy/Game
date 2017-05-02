using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

//1.2 - Made combat repeatable.
//1.3 - Introduced save files.
//1.4 - Introduced new RPG mechanics. A levelling system, critical hits, end-of round shop.
namespace Gamev1._4
{
    class Program
    {
        static void Main(string[] args)
        {
            Soldier player = new Soldier();
            if (File.Exists(Save.SaveFile))
            {
                player = LoadFromFile(player);
            }
            Start(player);
            bool playTheGame = true;
            Entity Victor;
            while (playTheGame)
            {
                Sectoid enemy = new Sectoid();
                PreCombat(player, enemy);
                Victor = Combat(player, enemy);
                PostCombat(Victor, player);
                playTheGame = PlayAgain();
            }
            System.Environment.Exit(1);
        }

        static void Start(Soldier player)
        {
            Console.WriteLine("Welcome to Squidgy's Intergalactice Arena of Death! \n");
            Console.WriteLine("Press 1 to create a new soldier");
            Console.WriteLine("Press 2 to continue a soldier's career");
            if (File.Exists(Save.SaveFile))
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
                if (Choice == "1")
                {
                    if (File.Exists(Save.SaveFile))
                    {
                        Console.WriteLine("Creating a new soldier will end the current soldier's career!");
                        Console.WriteLine("Are you sure you wish to do this? Y/N");
                        string overwrite_save = Console.ReadLine();
                        if (overwrite_save == "y" || overwrite_save == "Y")
                        {
                            File.Delete(Save.SaveFile);
                            CreateSoldier(player);
                            SaveToFile(player);
                            Console.WriteLine(player.Name + " saved to file. \n");
                        }
                        else
                        {
                            Console.WriteLine("Creation cancelled.");
                            Console.WriteLine("Press 1 to create a new soldier");
                            Console.WriteLine("Press 2 to continue a soldier's career");
                            Choice = "";
                        }
                    }
                    else
                    {
                        CreateSoldier(player);
                    }

                }
                else if (Choice == "2")
                {
                    if (File.Exists(Save.SaveFile))
                    {
                        player = LoadFromFile(player);
                    }
                    else
                    {
                        Choice = "";
                        Console.WriteLine("No save data! Please select 1.");
                    }

                }
                else
                {
                    Choice = "";
                    Console.WriteLine("Invalid input. Please enter 1 or 2.");
                }
            }
        }

        static void CreateSoldier(Soldier player)
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
                }
                else
                {
                    name_confirmed = "";
                    Console.WriteLine("Name cancelled. Please re-enter your soldier's name.");
                }
            }
        }

        static void SaveToFile(Soldier player)
        {
            FileStream stream = new FileStream(Save.SaveFile, FileMode.Create);
            BinaryFormatter bformatter = new BinaryFormatter();
            bformatter.Serialize(stream, player);
            stream.Close();
        }

        static Soldier LoadFromFile(Soldier player)
        {
            FileStream stream = new FileStream(Save.SaveFile, FileMode.Open);
            BinaryFormatter bformatter = new BinaryFormatter();
            Soldier RetrievedSaveState = (Soldier)bformatter.Deserialize(stream);
            stream.Close();
            return RetrievedSaveState;

        }

        static void PreCombat(Soldier player, Entity enemy)
        {
            Console.WriteLine("You are " + player.Name + ", a " + player.Class + " with " + player.HPCurrent + "HP, armed with a " + player.WeaponEquipped.Name + " that does " + player.WeaponEquipped.DmgMin + "-" + player.WeaponEquipped.DmgMax + " damage.");
            Console.WriteLine("Your opponent is a " + enemy.Class + " with " + enemy.HPCurrent + "HP, armed with a " + enemy.WeaponEquipped.Name + " that does " + enemy.WeaponEquipped.DmgMin + "-" + enemy.WeaponEquipped.DmgMax + " damage. \n");
            Console.WriteLine("Let battle commence! \n");
        }

        static Entity Combat(Entity player, Entity enemy)
        {
            while (player.HPCurrent > 0 || enemy.HPCurrent > 0)
            {
                Console.WriteLine("Press 1 to Attack. \n");
                string command = Console.ReadLine();
                if (command == "1")
                {
                    Attack(player, enemy);
                    if (enemy.HPCurrent <= 0)
                    {
                        return player;
                    }
                    Attack(enemy, player);
                    if (player.HPCurrent <= 0)
                    {
                        return enemy;
                    }
                }
                else
                {
                    Console.WriteLine("Invalid input. Please press 1.");
                }
            }

            return player; //this return value should never trigger

        }

        static void Attack(Entity attacker, Entity defender)
        {
            Random random = new Random();
            int roll_to_hit = random.Next(1, 101);
            if (roll_to_hit < attacker.Aim)
            {
                int roll_for_crit = random.Next(1, 101);
                if (roll_for_crit < attacker.WeaponEquipped.CritChance)
                {
                    int critDamage = random.Next(attacker.WeaponEquipped.CritDmgMin, (attacker.WeaponEquipped.CritDmgMax + 1));
                    defender.HPCurrent -= critDamage;
                    if (defender.HPCurrent > 0)
                    {
                        Console.WriteLine(attacker.Name + " lands a devastating critical hit on the " + defender.Name + ", doing " + critDamage + " damage!");
                        Console.WriteLine(attacker.Class + " - " + attacker.HPCurrent + "HP remaining. \t" + defender.Class + " - " + defender.HPCurrent + "HP remaining. \n");
                    }
                    else
                    {
                        Console.WriteLine("BLAM! " + attacker.Name + " blasts a big messy hole straight through the " + defender.Class + ", killing them super dead!");
                    }
                }
                else
                {
                    int damage = random.Next(attacker.WeaponEquipped.DmgMin, (attacker.WeaponEquipped.DmgMax + 1));
                    defender.HPCurrent -= damage;
                    if (defender.HPCurrent > 0)
                    {
                        Console.WriteLine(attacker.Name + " fires their " + attacker.WeaponEquipped.Name + " at the " + defender.Class + ", doing " + damage.ToString() + " damage!");
                        Console.WriteLine(attacker.Class + " - " + attacker.HPCurrent + "HP remaining. \t" + defender.Class + " - " + defender.HPCurrent + "HP remaining. \n");
                    }
                    else
                    {
                        Console.WriteLine(attacker.Name + " fires their " + attacker.WeaponEquipped.Name + " at the " + defender.Class + ", doing " + damage.ToString() + " damage, killing them! \n");
                    }
                }
            }
            else
            {
                Console.WriteLine(attacker.Name + " fires their " + attacker.WeaponEquipped.Name + " at the " + defender.Class + "...and misses!");
                Console.WriteLine(attacker.Class + " - " + attacker.HPCurrent + "HP remaining. \t" + defender.Class + " - " + defender.HPCurrent + "HP remaining. \n");
            }
        }

        static void PostCombat(Entity Victor, Soldier player)
        {
            if (Victor == player)
            {
                Console.WriteLine("Congratulations, you win!");
                int experience_earned = GenerateExp();
                Console.WriteLine(player.Name + " earned " + experience_earned + " experience points!");
                player.Experience += experience_earned;
                while (player.Experience >= player.ExpReqToLevel)
                {
                    int ExperienceCarriedOver = player.Experience - player.ExpReqToLevel;
                    player.Experience = ExperienceCarriedOver;
                    player.Level++;
                    Console.WriteLine("You are now level " + player.Level + "!");
                    player.ExpReqToLevel = CalculateNewLevelExpReq(player);
                }
                int prize_money = GeneratePrizeMoney();
                player.Moonbux += prize_money;
                Console.WriteLine(player.Name + " wins " + prize_money + " Moonbux as prize money! \n");
                int nextLevel = player.Level + 1;
                Console.WriteLine("Progress to level " + nextLevel + ": " + player.Experience + "/" + player.ExpReqToLevel + " \t Moonbux: " + player.Moonbux);
                player.HPCurrent = player.HPMax;
                player.Survived += 1;
                SaveToFile(player);
                Console.WriteLine(player.Name + "'s progress has been saved.");
            }
            else
            {
                Console.WriteLine(player.Name + " is dead! Game over.");
                Console.WriteLine("!!!---SAVE DELETED---!!!");
                File.Delete(Save.SaveFile);
                Console.ReadLine();
                System.Environment.Exit(1);

            }

        }

        static int GeneratePrizeMoney()
        {
            Random randomPayout = new Random();
            int prize_money = randomPayout.Next(100, 201);
            return prize_money;
        }

        static int GenerateExp()
        {
            Random ExperienceGain = new Random();
            int Experience = ExperienceGain.Next(80, 121);
            return Experience;
        }

        static int CalculateNewLevelExpReq(Soldier player)
        {
            int LevelExpReq = 100 + (player.Level * 50);
            return LevelExpReq;
        }

        static bool PlayAgain()
        {
            string choice = "";
            while (choice == "")
            {
                Console.WriteLine("Would you like to play again? Y/N");
                choice = Console.ReadLine();
                if (choice == "Y" || choice == "y")
                {
                    return true;
                }
                else if (choice == "N" || choice == "n")
                {
                    return false;
                }
                else
                {
                    choice = "";
                    Console.WriteLine("Invalid input. Please answer with Y/N \n");
                }
            }

            return false;
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
            Description = "Standard issue for all soldiers.";
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
            Description = "An alien weapon. Small but powerful.";
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
            Description = "Close range weapon with high crit damage.";
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
            Description = "Long range weapon with  good crit chance.";
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

    [Serializable]
    class PlayerInventory
    {
        public List<Weapon> Weapons; 
    }

    class ShopInventory
    {
        public ShopInventory()
        {

        }

        public List<Weapon> Weapons;


    }

    static class Save
    {
        public static string SaveFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "save.bin");
    }
    
}